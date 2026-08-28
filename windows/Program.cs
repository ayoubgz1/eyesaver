using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace EyeSaver
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new EyeSaverContext());
        }
    }

    public class EyeSaverContext : ApplicationContext
    {
        // Default configuration (20-20-20 rule)
        private const int DEFAULT_WORK_SECONDS = 20 * 60;
        private const int DEFAULT_BREAK_SECONDS = 20;

        private static readonly string[] EYE_TIPS = new[]
        {
            "Look at an object at least 20 feet (6 meters) away.",
            "Blink slowly and gently to rehydrate your eyes.",
            "Look out the window or across the farthest corner of the room.",
            "Relax your shoulders, neck, and facial muscles.",
            "Take a slow deep breath in... and exhale completely.",
            "Roll your eyes gently in circles to relieve strain."
        };

        private int workSeconds = DEFAULT_WORK_SECONDS;
        private int breakSeconds = DEFAULT_BREAK_SECONDS;

        private int workTimeRemaining = DEFAULT_WORK_SECONDS;
        private int breakTimeRemaining = DEFAULT_BREAK_SECONDS;

        private bool isPaused = false;
        private bool isInBreak = false;
        private int tipIndex = 0;

        private System.Windows.Forms.Timer? workTimer;
        private System.Windows.Forms.Timer? breakTimer;

        private NotifyIcon trayIcon;
        private ToolStripMenuItem statusMenuItem;
        private ToolStripMenuItem pauseMenuItem;
        private ToolStripMenuItem startWithWindowsItem;

        private List<OverlayForm> activeOverlays = new List<OverlayForm>();
        private IntPtr hookId = IntPtr.Zero;
        private LowLevelKeyboardProc? hookCallback;

        public EyeSaverContext()
        {
            // Initialize Tray Icon & Menu
            trayIcon = new NotifyIcon
            {
                Icon = GenerateAppIcon(),
                Text = "EyeSaver 20-20-20 (Eye Rest Timer)",
                Visible = true
            };

            trayIcon.ContextMenuStrip = BuildContextMenu();

            // Work countdown timer (1s tick)
            workTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            workTimer.Tick += (s, e) => TickWorkTimer();
            workTimer.Start();
        }

        private ContextMenuStrip BuildContextMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Renderer = new ModernMenuRenderer();

            // Title
            var titleItem = new ToolStripMenuItem("👁️ EyeSaver (20-20-20 Rule)") { Enabled = false };
            titleItem.Font = new Font(titleItem.Font, FontStyle.Bold);
            menu.Items.Add(titleItem);

            // Status Countdown
            statusMenuItem = new ToolStripMenuItem("⏳ Next break in: 20:00") { Enabled = false };
            menu.Items.Add(statusMenuItem);

            menu.Items.Add(new ToolStripSeparator());

            // Action Items
            var takeBreakItem = new ToolStripMenuItem("👁️ Take Break Now", null, (s, e) => TakeBreakNow());
            menu.Items.Add(takeBreakItem);

            pauseMenuItem = new ToolStripMenuItem("⏸️ Pause Timer", null, (s, e) => TogglePause());
            menu.Items.Add(pauseMenuItem);

            menu.Items.Add(new ToolStripSeparator());

            // Quick 5s Test
            var testBreakItem = new ToolStripMenuItem("⚡ Test Quick Break (5s)", null, (s, e) => TestQuickBreak());
            menu.Items.Add(testBreakItem);

            // Intervals Submenu
            var intervalsMenu = new ToolStripMenuItem("⚙️ Intervals");
            var d20 = new ToolStripMenuItem("Work: 20m / Break: 20s (Default)", null, (s, e) => SetInterval(20 * 60, 20));
            var d15 = new ToolStripMenuItem("Work: 15m / Break: 20s", null, (s, e) => SetInterval(15 * 60, 20));
            var d30 = new ToolStripMenuItem("Work: 30m / Break: 30s", null, (s, e) => SetInterval(30 * 60, 30));
            intervalsMenu.DropDownItems.Add(d20);
            intervalsMenu.DropDownItems.Add(d15);
            intervalsMenu.DropDownItems.Add(d30);
            menu.Items.Add(intervalsMenu);

            // Start with Windows toggle
            startWithWindowsItem = new ToolStripMenuItem("🚀 Start with Windows", null, (s, e) => ToggleStartWithWindows())
            {
                Checked = IsRunAtStartupEnabled()
            };
            menu.Items.Add(startWithWindowsItem);

            menu.Items.Add(new ToolStripSeparator());

            // Useful Links
            var linksMenu = new ToolStripMenuItem("🔗 Links & Info");
            var githubItem = new ToolStripMenuItem("🌐 GitHub Repository", null, (s, e) => OpenUrl("https://github.com/ayoubgz1/eyesaver"));
            var updatesItem = new ToolStripMenuItem("⬇️ Check for Updates", null, (s, e) => OpenUrl("https://github.com/ayoubgz1/eyesaver/releases/latest"));
            var aboutItem = new ToolStripMenuItem("ℹ️ About 20-20-20 Rule", null, (s, e) => OpenUrl("https://github.com/ayoubgz1/eyesaver#readme"));
            linksMenu.DropDownItems.Add(githubItem);
            linksMenu.DropDownItems.Add(updatesItem);
            linksMenu.DropDownItems.Add(aboutItem);
            menu.Items.Add(linksMenu);

            menu.Items.Add(new ToolStripSeparator());

            // Exit
            var exitItem = new ToolStripMenuItem("❌ Exit EyeSaver", null, (s, e) => ExitApp());
            menu.Items.Add(exitItem);

            return menu;
        }

        private void TickWorkTimer()
        {
            if (isPaused || isInBreak) return;

            if (workTimeRemaining > 0)
            {
                workTimeRemaining--;
                int mins = workTimeRemaining / 60;
                int secs = workTimeRemaining % 60;
                statusMenuItem.Text = $"⏳ Next break in: {mins:D2}:{secs:D2}";
            }
            else
            {
                StartBreak();
            }
        }

        private void TogglePause()
        {
            isPaused = !isPaused;
            if (isPaused)
            {
                pauseMenuItem.Text = "▶️ Resume Timer";
                statusMenuItem.Text = "⏸️ Paused (Timer halted)";
            }
            else
            {
                pauseMenuItem.Text = "⏸️ Pause Timer";
                int mins = workTimeRemaining / 60;
                int secs = workTimeRemaining % 60;
                statusMenuItem.Text = $"⏳ Next break in: {mins:D2}:{secs:D2}";
            }
        }

        private void TakeBreakNow()
        {
            if (!isInBreak) StartBreak();
        }

        private void TestQuickBreak()
        {
            if (!isInBreak) StartBreak(5);
        }

        private void SetInterval(int workSec, int breakSec)
        {
            workSeconds = workSec;
            breakSeconds = breakSec;
            workTimeRemaining = workSeconds;
            int mins = workTimeRemaining / 60;
            int secs = workTimeRemaining % 60;
            statusMenuItem.Text = $"⏳ Next break in: {mins:D2}:{secs:D2}";
        }

        private void StartBreak(int? customBreakSecs = null)
        {
            isInBreak = true;
            breakTimeRemaining = customBreakSecs ?? breakSeconds;
            int totalBreak = breakTimeRemaining;

            string currentTip = EYE_TIPS[tipIndex % EYE_TIPS.Length];
            tipIndex++;

            // Play break chime
            PlayChimeSound(true);

            // Install low-level keyboard hook to block Alt+Tab, Alt+F4, Esc during break
            InstallKeyboardHook();

            // Hide mouse cursor
            Cursor.Hide();

            // Create fullscreen overlay for all screens
            activeOverlays.Clear();
            foreach (var screen in Screen.AllScreens)
            {
                var overlay = new OverlayForm(screen.Bounds, breakTimeRemaining, totalBreak, currentTip);
                overlay.Show();
                activeOverlays.Add(overlay);
            }

            // Start break countdown timer
            breakTimer?.Stop();
            breakTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            breakTimer.Tick += (s, e) => TickBreakTimer(totalBreak);
            breakTimer.Start();
        }

        private void TickBreakTimer(int totalBreak)
        {
            if (!isInBreak) return;

            if (breakTimeRemaining > 0)
            {
                breakTimeRemaining--;
                foreach (var overlay in activeOverlays)
                {
                    overlay.UpdateCountdown(breakTimeRemaining, totalBreak);
                }
            }
            else
            {
                EndBreak();
            }
        }

        private void EndBreak()
        {
            isInBreak = false;
            breakTimer?.Stop();
            breakTimer = null;

            // Remove keyboard hook
            UninstallKeyboardHook();

            // Restore mouse cursor
            Cursor.Show();

            // Play completion chime
            PlayChimeSound(false);

            // Close overlays
            foreach (var overlay in activeOverlays)
            {
                overlay.Close();
                overlay.Dispose();
            }
            activeOverlays.Clear();

            // Reset work timer
            workTimeRemaining = workSeconds;
            int mins = workTimeRemaining / 60;
            int secs = workTimeRemaining % 60;
            statusMenuItem.Text = $"⏳ Next break in: {mins:D2}:{secs:D2}";
        }

        private void PlayChimeSound(bool isStart)
        {
            try
            {
                if (isStart)
                {
                    System.Media.SystemSounds.Asterisk.Play();
                }
                else
                {
                    System.Media.SystemSounds.Exclamation.Play();
                }
            }
            catch { }
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open link: {ex.Message}", "EyeSaver", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsRunAtStartupEnabled()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false);
                return key?.GetValue("EyeSaver") != null;
            }
            catch
            {
                return false;
            }
        }

        private void ToggleStartWithWindows()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (key == null) return;

                if (startWithWindowsItem.Checked)
                {
                    key.DeleteValue("EyeSaver", false);
                    startWithWindowsItem.Checked = false;
                }
                else
                {
                    string exePath = Process.GetCurrentProcess().MainModule?.FileName ?? Application.ExecutablePath;
                    key.SetValue("EyeSaver", $"\"{exePath}\"");
                    startWithWindowsItem.Checked = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to change startup setting: {ex.Message}", "EyeSaver", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ExitApp()
        {
            if (isInBreak) EndBreak();

            workTimer?.Stop();
            trayIcon.Visible = false;
            trayIcon.Dispose();
            Application.Exit();
        }

        private Icon GenerateAppIcon()
        {
            using var bmp = new Bitmap(32, 32);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            // Outer Glow Circle
            using var brushBg = new SolidBrush(Color.FromArgb(18, 24, 38));
            g.FillEllipse(brushBg, 1, 1, 30, 30);

            // Eye outline
            using var penCyan = new Pen(Color.FromArgb(97, 175, 239), 2f);
            g.DrawEllipse(penCyan, 4, 9, 24, 14);

            // Pupil
            using var brushGreen = new SolidBrush(Color.FromArgb(152, 195, 121));
            g.FillEllipse(brushGreen, 12, 12, 8, 8);

            IntPtr hIcon = bmp.GetHicon();
            return Icon.FromHandle(hIcon);
        }

        #region Low Level Keyboard Hook (Suppress Alt+Tab, Alt+F4, Esc during Break)

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        private void InstallKeyboardHook()
        {
            if (hookId != IntPtr.Zero) return;

            hookCallback = HookProcedure;
            using var curProcess = Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule;
            if (curModule != null)
            {
                hookId = SetWindowsHookEx(WH_KEYBOARD_LL, hookCallback, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private void UninstallKeyboardHook()
        {
            if (hookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(hookId);
                hookId = IntPtr.Zero;
            }
        }

        private IntPtr HookProcedure(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && isInBreak && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;

                // Block Alt+F4, Esc, Windows Keys, Tab, Task switcher
                if (key == Keys.LWin || key == Keys.RWin || key == Keys.Escape || key == Keys.Tab || key == Keys.F4)
                {
                    return (IntPtr)1; // Suppress key event
                }
            }
            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        #endregion
    }

    // High Quality Fullscreen Overlay Window
    public class OverlayForm : Form
    {
        private int remainingSeconds;
        private int totalBreakSeconds;
        private string currentTip;

        public OverlayForm(Rectangle bounds, int remainingSec, int totalBreakSec, string tip)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.Bounds = bounds;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.DoubleBuffered = true;
            this.BackColor = Color.FromArgb(8, 10, 16); // Sleek dark slate
            this.Cursor = Cursors.Default;

            this.remainingSeconds = remainingSec;
            this.totalBreakSeconds = totalBreakSec;
            this.currentTip = tip;

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        public void UpdateCountdown(int remaining, int total)
        {
            this.remainingSeconds = remaining;
            this.totalBreakSeconds = total;
            this.Invalidate();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Block all keys during break
            return true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            int centerX = this.ClientSize.Width / 2;
            int centerY = this.ClientSize.Height / 2;

            // 1. Emoji / Icon
            using (var iconFont = new Font("Segoe UI Emoji", 56, FontStyle.Regular))
            using (var brush = new SolidBrush(Color.White))
            {
                var iconSize = g.MeasureString("👁️", iconFont);
                g.DrawString("👁️", iconFont, brush, centerX - iconSize.Width / 2, centerY - 210);
            }

            // 2. Title: LOOK AWAY
            using (var titleFont = new Font("Segoe UI", 40, FontStyle.Bold))
            using (var titleBrush = new SolidBrush(Color.FromArgb(97, 175, 239))) // Cyan #61AFEF
            {
                var titleSize = g.MeasureString("LOOK AWAY", titleFont);
                g.DrawString("LOOK AWAY", titleFont, titleBrush, centerX - titleSize.Width / 2, centerY - 120);
            }

            // 3. Subtitle
            using (var subFont = new Font("Segoe UI", 16, FontStyle.Regular))
            using (var subBrush = new SolidBrush(Color.FromArgb(229, 192, 123))) // Warm Gold #E5C07B
            {
                string subText = "Look at an object at least 20 feet (6 meters) away";
                var subSize = g.MeasureString(subText, subFont);
                g.DrawString(subText, subFont, subBrush, centerX - subSize.Width / 2, centerY - 55);
            }

            // 4. Countdown Number
            using (var countFont = new Font("Segoe UI", 48, FontStyle.Bold))
            using (var countBrush = new SolidBrush(Color.FromArgb(152, 195, 121))) // Calming Green #98C379
            {
                string countText = $"{remainingSeconds}s";
                var countSize = g.MeasureString(countText, countFont);
                g.DrawString(countText, countFont, countBrush, centerX - countSize.Width / 2, centerY - 5);
            }

            // 5. Progress Bar
            int barWidth = 380;
            int barHeight = 8;
            int barX = centerX - barWidth / 2;
            int barY = centerY + 75;

            // Background Bar
            using (var bgBarBrush = new SolidBrush(Color.FromArgb(40, 44, 58)))
            {
                FillRoundedRectangle(g, bgBarBrush, barX, barY, barWidth, barHeight, 4);
            }

            // Filled Bar
            float fraction = totalBreakSeconds > 0 ? (float)remainingSeconds / totalBreakSeconds : 0f;
            int fillWidth = Math.Max(0, (int)(barWidth * fraction));
            if (fillWidth > 0)
            {
                using var fillBrush = new SolidBrush(Color.FromArgb(152, 195, 121));
                FillRoundedRectangle(g, fillBrush, barX, barY, fillWidth, barHeight, 4);
            }

            // 6. Tip Box
            int tipBoxWidth = 560;
            int tipBoxHeight = 75;
            int tipBoxX = centerX - tipBoxWidth / 2;
            int tipBoxY = centerY + 110;

            using (var tipBgBrush = new SolidBrush(Color.FromArgb(20, 23, 34)))
            using (var tipBorderPen = new Pen(Color.FromArgb(35, 39, 56), 1f))
            {
                FillRoundedRectangle(g, tipBgBrush, tipBoxX, tipBoxY, tipBoxWidth, tipBoxHeight, 10);
                DrawRoundedRectangle(g, tipBorderPen, tipBoxX, tipBoxY, tipBoxWidth, tipBoxHeight, 10);
            }

            // Tip Title
            using (var tipTitleFont = new Font("Segoe UI", 9, FontStyle.Bold))
            using (var tipTitleBrush = new SolidBrush(Color.FromArgb(86, 182, 194))) // Cyan #56B6C2
            {
                string tipTitle = "💡 EYE CARE TIP";
                var size = g.MeasureString(tipTitle, tipTitleFont);
                g.DrawString(tipTitle, tipTitleFont, tipTitleBrush, centerX - size.Width / 2, tipBoxY + 12);
            }

            // Tip Text
            using (var tipFont = new Font("Segoe UI", 11, FontStyle.Regular))
            using (var tipBrush = new SolidBrush(Color.FromArgb(215, 219, 227)))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                var textRect = new RectangleF(tipBoxX + 15, tipBoxY + 30, tipBoxWidth - 30, tipBoxHeight - 35);
                g.DrawString(currentTip, tipFont, tipBrush, textRect, sf);
            }

            // 7. Screen lock enforcement note
            using (var noteFont = new Font("Segoe UI", 9, FontStyle.Regular))
            using (var noteBrush = new SolidBrush(Color.FromArgb(108, 115, 130)))
            {
                string noteText = "Screen locked for 20 seconds to enforce healthy rest • 20-20-20 Rule";
                var noteSize = g.MeasureString(noteText, noteFont);
                g.DrawString(noteText, noteFont, noteBrush, centerX - noteSize.Width / 2, centerY + 210);
            }
        }

        private static void FillRoundedRectangle(Graphics g, Brush brush, int x, int y, int width, int height, int radius)
        {
            using var path = CreateRoundedRectanglePath(x, y, width, height, radius);
            g.FillPath(brush, path);
        }

        private static void DrawRoundedRectangle(Graphics g, Pen pen, int x, int y, int width, int height, int radius)
        {
            using var path = CreateRoundedRectanglePath(x, y, width, height, radius);
            g.DrawPath(pen, path);
        }

        private static GraphicsPath CreateRoundedRectanglePath(int x, int y, int width, int height, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(x, y, diameter, diameter, 180, 90);
            path.AddArc(x + width - diameter, y, diameter, diameter, 270, 90);
            path.AddArc(x + width - diameter, y + height - diameter, diameter, diameter, 0, 90);
            path.AddArc(x, y + height - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    // Modern Dark Theme Context Menu Renderer
    public class ModernMenuRenderer : ToolStripProfessionalRenderer
    {
        public ModernMenuRenderer() : base(new DarkColorTable()) { }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            if (!e.Item.Enabled)
            {
                e.TextColor = Color.FromArgb(120, 126, 142);
            }
            else
            {
                e.TextColor = Color.FromArgb(235, 238, 245);
            }
            base.OnRenderItemText(e);
        }
    }

    public class DarkColorTable : ProfessionalColorTable
    {
        public override Color ToolStripDropDownBackground => Color.FromArgb(24, 27, 38);
        public override Color ImageMarginGradientBegin => Color.FromArgb(24, 27, 38);
        public override Color ImageMarginGradientMiddle => Color.FromArgb(24, 27, 38);
        public override Color ImageMarginGradientEnd => Color.FromArgb(24, 27, 38);
        public override Color MenuBorder => Color.FromArgb(48, 54, 74);
        public override Color MenuItemBorder => Color.FromArgb(60, 68, 92);
        public override Color MenuItemSelected => Color.FromArgb(38, 44, 62);
        public override Color MenuStripGradientBegin => Color.FromArgb(24, 27, 38);
        public override Color MenuStripGradientEnd => Color.FromArgb(24, 27, 38);
        public override Color SeparatorDark => Color.FromArgb(45, 50, 68);
        public override Color SeparatorLight => Color.Transparent;
    }
}
