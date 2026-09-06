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
            menu.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            menu.Padding = new Padding(2);

            // Title
            var titleItem = new ToolStripMenuItem("👁️ EyeSaver (20-20-20 Rule)") { Enabled = false };
            titleItem.Font = new Font(menu.Font, FontStyle.Bold);
            menu.Items.Add(titleItem);

            // Status Countdown
            statusMenuItem = new ToolStripMenuItem("⏳ Next break in: 20:00") { Enabled = false };
            statusMenuItem.Font = menu.Font;
            menu.Items.Add(statusMenuItem);

            menu.Items.Add(new ToolStripSeparator());

            // Action Items
            var takeBreakItem = new ToolStripMenuItem("👁️ Take Break Now", null, (s, e) => TakeBreakNow()) { Font = menu.Font };
            menu.Items.Add(takeBreakItem);

            pauseMenuItem = new ToolStripMenuItem("⏸️ Pause Timer", null, (s, e) => TogglePause()) { Font = menu.Font };
            menu.Items.Add(pauseMenuItem);

            menu.Items.Add(new ToolStripSeparator());

            // Quick 5s Test
            var testBreakItem = new ToolStripMenuItem("⚡ Test Quick Break (5s)", null, (s, e) => TestQuickBreak()) { Font = menu.Font };
            menu.Items.Add(testBreakItem);

            // Intervals Submenu
            var intervalsMenu = new ToolStripMenuItem("⚙️ Intervals") { Font = menu.Font };
            intervalsMenu.DropDown.Renderer = menu.Renderer;
            intervalsMenu.DropDown.Font = menu.Font;

            var d20 = new ToolStripMenuItem("Work: 20m / Break: 20s (Default)", null, (s, e) => SetInterval(20 * 60, 20)) { Font = menu.Font };
            var d15 = new ToolStripMenuItem("Work: 15m / Break: 20s", null, (s, e) => SetInterval(15 * 60, 20)) { Font = menu.Font };
            var d30 = new ToolStripMenuItem("Work: 30m / Break: 30s", null, (s, e) => SetInterval(30 * 60, 30)) { Font = menu.Font };
            intervalsMenu.DropDownItems.Add(d20);
            intervalsMenu.DropDownItems.Add(d15);
            intervalsMenu.DropDownItems.Add(d30);
            menu.Items.Add(intervalsMenu);

            // Start with Windows toggle
            startWithWindowsItem = new ToolStripMenuItem("🚀 Start with Windows", null, (s, e) => ToggleStartWithWindows())
            {
                Checked = IsRunAtStartupEnabled(),
                Font = menu.Font
            };
            menu.Items.Add(startWithWindowsItem);

            menu.Items.Add(new ToolStripSeparator());

            // Useful Links
            var linksMenu = new ToolStripMenuItem("🔗 Links & Info") { Font = menu.Font };
            linksMenu.DropDown.Renderer = menu.Renderer;
            linksMenu.DropDown.Font = menu.Font;

            var githubItem = new ToolStripMenuItem("🌐 GitHub Repository", null, (s, e) => OpenUrl("https://github.com/ayoubgz1/eyesaver")) { Font = menu.Font };
            var updatesItem = new ToolStripMenuItem("⬇️ Check for Updates", null, (s, e) => OpenUrl("https://github.com/ayoubgz1/eyesaver/releases/latest")) { Font = menu.Font };
            var aboutItem = new ToolStripMenuItem("ℹ️ About 20-20-20 Rule", null, (s, e) => OpenUrl("https://github.com/ayoubgz1/eyesaver#readme")) { Font = menu.Font };
            linksMenu.DropDownItems.Add(githubItem);
            linksMenu.DropDownItems.Add(updatesItem);
            linksMenu.DropDownItems.Add(aboutItem);
            menu.Items.Add(linksMenu);

            menu.Items.Add(new ToolStripSeparator());

            // Exit
            var exitItem = new ToolStripMenuItem("❌ Exit EyeSaver", null, (s, e) => ExitApp()) { Font = menu.Font };
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
        private readonly Rectangle targetBounds;

        public OverlayForm(Rectangle bounds, int remainingSec, int totalBreakSec, string tip)
        {
            this.targetBounds = bounds;
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

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x80; // WS_EX_TOOLWINDOW: suppress from Alt+Tab
                return cp;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.Bounds = targetBounds;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this.Bounds = targetBounds;
            this.BringToFront();
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
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            float dpiScale = g.DpiY / 96f;
            if (dpiScale < 0.75f) dpiScale = 1.0f;

            float clientW = this.ClientSize.Width;
            float clientH = this.ClientSize.Height;
            float centerX = clientW / 2f;

            // 1. Prepare typography with DPI-scaled point sizes
            using var titleFont = new Font("Segoe UI", 36f, FontStyle.Bold);
            using var subFont = new Font("Segoe UI", 14.5f, FontStyle.Regular);
            using var countFont = new Font("Segoe UI", 46f, FontStyle.Bold);
            using var tipTagFont = new Font("Segoe UI", 9f, FontStyle.Bold);
            using var tipFont = new Font("Segoe UI", 11f, FontStyle.Regular);
            using var noteFont = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            // 2. Measure elements
            // Vector Eye Badge size
            float badgeDiameter = 66f * dpiScale;

            // Title
            string titleText = "LOOK AWAY";
            var titleSize = g.MeasureString(titleText, titleFont);

            // Subtitle
            string subText = "Look at an object at least 20 feet (6 meters) away";
            var subSize = g.MeasureString(subText, subFont);

            // Countdown Number
            string countText = $"{remainingSeconds}s";
            var countSize = g.MeasureString("99s", countFont);

            // Progress Bar dimensions
            float barWidth = Math.Min(380f * dpiScale, clientW * 0.78f);
            float barHeight = Math.Max(8f, 8f * dpiScale);

            // Tip Box dimensions
            float tipBoxWidth = Math.Min(560f * dpiScale, clientW * 0.88f);
            float tipPaddingX = 22f * dpiScale;
            float tipPaddingY = 12f * dpiScale;
            float tipTextWidth = tipBoxWidth - (tipPaddingX * 2f);

            var tipTagSize = g.MeasureString("💡 EYE CARE TIP", tipTagFont);
            var tipTextSize = g.MeasureString(currentTip, tipFont, (int)tipTextWidth);
            float tipBoxHeight = tipPaddingY + tipTagSize.Height + (5f * dpiScale) + tipTextSize.Height + tipPaddingY;

            // Footer Note
            string noteText = "Screen locked for 20 seconds to enforce healthy rest • 20-20-20 Rule";
            var noteSize = g.MeasureString(noteText, noteFont);

            // 3. Spacing gaps between elements (scaled by DPI)
            float gapBadge = 14f * dpiScale;
            float gapTitle = 6f * dpiScale;
            float gapSub = 18f * dpiScale;
            float gapCount = 12f * dpiScale;
            float gapBar = 22f * dpiScale;
            float gapTip = 20f * dpiScale;

            // 4. Calculate total height of the content stack
            float totalStackHeight = badgeDiameter + gapBadge
                                   + titleSize.Height + gapTitle
                                   + subSize.Height + gapSub
                                   + countSize.Height + gapCount
                                   + barHeight + gapBar
                                   + tipBoxHeight + gapTip
                                   + noteSize.Height;

            // 5. Screen height fit check (compress proportionally if totalStack exceeds client height)
            float availableHeight = clientH - (40f * dpiScale);
            if (totalStackHeight > availableHeight && totalStackHeight > 0f)
            {
                float factor = Math.Max(0.55f, availableHeight / totalStackHeight);
                gapBadge *= factor;
                gapTitle *= factor;
                gapSub *= factor;
                gapCount *= factor;
                gapBar *= factor;
                gapTip *= factor;

                totalStackHeight = badgeDiameter + gapBadge
                                 + titleSize.Height + gapTitle
                                 + subSize.Height + gapSub
                                 + countSize.Height + gapCount
                                 + barHeight + gapBar
                                 + tipBoxHeight + gapTip
                                 + noteSize.Height;
            }

            // 6. Starting Y coordinate (centered vertically)
            float currentY = Math.Max(16f * dpiScale, (clientH - totalStackHeight) / 2f);

            // 7. Sequential rendering (Mathematical guarantee: zero overlapping!)

            // [1] Vector Eye Badge
            DrawEyeBadge(g, centerX, currentY + badgeDiameter / 2f, badgeDiameter);
            currentY += badgeDiameter + gapBadge;

            // [2] Title
            using (var titleBrush = new SolidBrush(Color.FromArgb(97, 175, 239))) // Cyan #61AFEF
            {
                g.DrawString(titleText, titleFont, titleBrush, centerX - titleSize.Width / 2f, currentY);
            }
            currentY += titleSize.Height + gapTitle;

            // [3] Subtitle
            using (var subBrush = new SolidBrush(Color.FromArgb(229, 192, 123))) // Warm Gold #E5C07B
            {
                g.DrawString(subText, subFont, subBrush, centerX - subSize.Width / 2f, currentY);
            }
            currentY += subSize.Height + gapSub;

            // [4] Countdown Number
            using (var countBrush = new SolidBrush(Color.FromArgb(152, 195, 121))) // Mint Green #98C379
            {
                var actualCountSize = g.MeasureString(countText, countFont);
                g.DrawString(countText, countFont, countBrush, centerX - actualCountSize.Width / 2f, currentY);
            }
            currentY += countSize.Height + gapCount;

            // [5] Progress Bar
            float barX = centerX - barWidth / 2f;
            using (var bgBarBrush = new SolidBrush(Color.FromArgb(35, 40, 54)))
            {
                FillRoundedRectangle(g, bgBarBrush, barX, currentY, barWidth, barHeight, 4f * dpiScale);
            }
            float fraction = totalBreakSeconds > 0 ? (float)remainingSeconds / totalBreakSeconds : 0f;
            float fillWidth = Math.Max(0f, barWidth * fraction);
            if (fillWidth > 0f)
            {
                using var fillBrush = new SolidBrush(Color.FromArgb(152, 195, 121));
                FillRoundedRectangle(g, fillBrush, barX, currentY, fillWidth, barHeight, 4f * dpiScale);
            }
            currentY += barHeight + gapBar;

            // [6] Tip Box Card
            float tipBoxX = centerX - tipBoxWidth / 2f;
            float tipBoxY = currentY;
            using (var tipBgBrush = new SolidBrush(Color.FromArgb(18, 22, 32)))
            using (var tipBorderPen = new Pen(Color.FromArgb(36, 44, 64), 1f))
            {
                FillRoundedRectangle(g, tipBgBrush, tipBoxX, tipBoxY, tipBoxWidth, tipBoxHeight, 10f * dpiScale);
                DrawRoundedRectangle(g, tipBorderPen, tipBoxX, tipBoxY, tipBoxWidth, tipBoxHeight, 10f * dpiScale);
            }

            // Tip Tag
            using (var tipTagBrush = new SolidBrush(Color.FromArgb(86, 182, 194))) // Cyan #56B6C2
            {
                g.DrawString("💡 EYE CARE TIP", tipTagFont, tipTagBrush, centerX - tipTagSize.Width / 2f, tipBoxY + tipPaddingY);
            }

            // Tip Body Text
            using (var tipBrush = new SolidBrush(Color.FromArgb(220, 224, 232)))
            using (var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Near,
                Trimming = StringTrimming.Word
            })
            {
                var textRect = new RectangleF(
                    tipBoxX + tipPaddingX,
                    tipBoxY + tipPaddingY + tipTagSize.Height + (5f * dpiScale),
                    tipTextWidth,
                    tipTextSize.Height + (4f * dpiScale)
                );
                g.DrawString(currentTip, tipFont, tipBrush, textRect, sf);
            }
            currentY += tipBoxHeight + gapTip;

            // [7] Enforcement Note
            using (var noteBrush = new SolidBrush(Color.FromArgb(115, 122, 138)))
            {
                g.DrawString(noteText, noteFont, noteBrush, centerX - noteSize.Width / 2f, currentY);
            }
        }

        private static void DrawEyeBadge(Graphics g, float cx, float cy, float diameter)
        {
            float radius = diameter / 2f;
            float x = cx - radius;
            float y = cy - radius;

            // 1. Subtle Outer Glow Ring
            using (var glowPen = new Pen(Color.FromArgb(35, 97, 175, 239), 2f))
            {
                g.DrawEllipse(glowPen, x - 2, y - 2, diameter + 4, diameter + 4);
            }

            // 2. Badge Dark Background
            using (var badgeBrush = new SolidBrush(Color.FromArgb(16, 20, 32)))
            using (var borderPen = new Pen(Color.FromArgb(42, 52, 78), 1.5f))
            {
                g.FillEllipse(badgeBrush, x, y, diameter, diameter);
                g.DrawEllipse(borderPen, x, y, diameter, diameter);
            }

            // 3. Stylized Eye Shape
            float eyeW = diameter * 0.58f;
            float eyeH = diameter * 0.30f;
            float eyeLeft = cx - eyeW / 2f;
            float eyeRight = cx + eyeW / 2f;

            using (var eyePath = new GraphicsPath())
            {
                eyePath.AddBezier(
                    eyeLeft, cy,
                    cx - eyeW * 0.25f, cy - eyeH,
                    cx + eyeW * 0.25f, cy - eyeH,
                    eyeRight, cy
                );
                eyePath.AddBezier(
                    eyeRight, cy,
                    cx + eyeW * 0.25f, cy + eyeH,
                    cx - eyeW * 0.25f, cy + eyeH,
                    eyeLeft, cy
                );
                eyePath.CloseFigure();

                using (var scleraBrush = new SolidBrush(Color.FromArgb(28, 97, 175, 239)))
                {
                    g.FillPath(scleraBrush, eyePath);
                }

                using (var eyePen = new Pen(Color.FromArgb(97, 175, 239), 2f))
                {
                    eyePen.StartCap = LineCap.Round;
                    eyePen.EndCap = LineCap.Round;
                    g.DrawPath(eyePen, eyePath);
                }
            }

            // 4. Iris
            float irisRadius = eyeH * 0.85f;
            using (var irisBrush = new SolidBrush(Color.FromArgb(70, 155, 225)))
            {
                g.FillEllipse(irisBrush, cx - irisRadius, cy - irisRadius, irisRadius * 2f, irisRadius * 2f);
            }

            // 5. Pupil
            float pupilRadius = irisRadius * 0.55f;
            using (var pupilBrush = new SolidBrush(Color.FromArgb(152, 195, 121)))
            {
                g.FillEllipse(pupilBrush, cx - pupilRadius, cy - pupilRadius, pupilRadius * 2f, pupilRadius * 2f);
            }

            // 6. Light Reflection
            float reflectRadius = pupilRadius * 0.35f;
            float reflectX = cx + pupilRadius * 0.25f;
            float reflectY = cy - pupilRadius * 0.35f;
            using (var reflectBrush = new SolidBrush(Color.FromArgb(245, 255, 255, 255)))
            {
                g.FillEllipse(reflectBrush, reflectX, reflectY, reflectRadius * 2f, reflectRadius * 2f);
            }
        }

        private static void FillRoundedRectangle(Graphics g, Brush brush, float x, float y, float width, float height, float radius)
        {
            using var path = CreateRoundedRectanglePath(x, y, width, height, radius);
            g.FillPath(brush, path);
        }

        private static void DrawRoundedRectangle(Graphics g, Pen pen, float x, float y, float width, float height, float radius)
        {
            using var path = CreateRoundedRectanglePath(x, y, width, height, radius);
            g.DrawPath(pen, path);
        }

        private static GraphicsPath CreateRoundedRectanglePath(float x, float y, float width, float height, float radius)
        {
            var path = new GraphicsPath();
            float diameter = radius * 2f;
            if (diameter > width) diameter = width;
            if (diameter > height) diameter = height;
            if (diameter <= 0f)
            {
                path.AddRectangle(new RectangleF(x, y, width, height));
                return path;
            }

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

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = Color.FromArgb(160, 175, 205);
            base.OnRenderArrow(e);
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = e.ImageRectangle;

            int boxSize = Math.Min(rect.Width, rect.Height) - 2;
            int boxX = rect.X + (rect.Width - boxSize) / 2;
            int boxY = rect.Y + (rect.Height - boxSize) / 2;

            using (var checkBrush = new SolidBrush(Color.FromArgb(40, 152, 195, 121)))
            using (var borderPen = new Pen(Color.FromArgb(152, 195, 121), 1.5f))
            {
                g.FillRectangle(checkBrush, boxX, boxY, boxSize, boxSize);
                g.DrawRectangle(borderPen, boxX, boxY, boxSize, boxSize);
            }

            using (var checkPen = new Pen(Color.FromArgb(152, 195, 121), 2f))
            {
                checkPen.StartCap = LineCap.Round;
                checkPen.EndCap = LineCap.Round;
                g.DrawLine(checkPen, boxX + 3, boxY + boxSize / 2, boxX + boxSize / 2 - 1, boxY + boxSize - 4);
                g.DrawLine(checkPen, boxX + boxSize / 2 - 1, boxY + boxSize - 4, boxX + boxSize - 3, boxY + 4);
            }
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            var g = e.Graphics;
            int y = e.Item.Height / 2;
            using var sepPen = new Pen(Color.FromArgb(45, 50, 68), 1f);
            g.DrawLine(sepPen, 10, y, e.Item.Width - 10, y);
        }
    }

    public class DarkColorTable : ProfessionalColorTable
    {
        public override Color ToolStripDropDownBackground => Color.FromArgb(24, 27, 38);
        public override Color ImageMarginGradientBegin => Color.FromArgb(24, 27, 38);
        public override Color ImageMarginGradientMiddle => Color.FromArgb(24, 27, 38);
        public override Color ImageMarginGradientEnd => Color.FromArgb(24, 27, 38);
        public override Color MenuBorder => Color.FromArgb(48, 54, 74);
        public override Color MenuItemBorder => Color.Transparent;
        public override Color MenuItemSelected => Color.FromArgb(38, 44, 62);
        public override Color MenuItemSelectedGradientBegin => Color.FromArgb(38, 44, 62);
        public override Color MenuItemSelectedGradientEnd => Color.FromArgb(38, 44, 62);
        public override Color MenuStripGradientBegin => Color.FromArgb(24, 27, 38);
        public override Color MenuStripGradientEnd => Color.FromArgb(24, 27, 38);
        public override Color SeparatorDark => Color.FromArgb(45, 50, 68);
        public override Color SeparatorLight => Color.Transparent;
        public override Color CheckBackground => Color.FromArgb(38, 44, 62);
        public override Color CheckSelectedBackground => Color.FromArgb(48, 56, 78);
        public override Color CheckPressedBackground => Color.FromArgb(48, 56, 78);
    }
}
