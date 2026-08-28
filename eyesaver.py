#!/usr/bin/env python3
"""
=============================================================================
EyeSaver (20-20-20 Rule Application for macOS)
=============================================================================
Every 20 minutes, a full-screen black overlay appears with the message "LOOK AWAY",
remains locked for 20 seconds, and repeats continuously until you quit.
=============================================================================
"""

import sys
import os
import time
import argparse
import subprocess
import threading
import webbrowser
import tkinter as tk
from tkinter import messagebox

# Default configuration (20-20-20 rule)
DEFAULT_WORK_MINUTES = 20
DEFAULT_BREAK_SECONDS = 20

# Eye care rotating tips
TIPS = [
    "Look at an object at least 20 feet (6 meters) away.",
    "Blink slowly and gently to rehydrate your eyes.",
    "Look out the window or across the farthest corner of the room.",
    "Relax your shoulders, neck, and facial muscles.",
    "Take a slow deep breath in... and exhale completely.",
    "Roll your eyes gently in circles to relieve strain."
]

def play_sound(sound_name="Ping"):
    """Play built-in system sound non-blockingly across macOS, Windows, and Linux."""
    def _play():
        try:
            if sys.platform == "darwin":
                sound_path = f"/System/Library/Sounds/{sound_name}.aiff"
                if os.path.exists(sound_path):
                    subprocess.run(["afplay", sound_path], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
            elif sys.platform == "win32":
                import winsound
                if sound_name == "Ping":
                    winsound.PlaySound("SystemAsterisk", winsound.SND_ALIAS | winsound.SND_ASYNC)
                else:
                    winsound.PlaySound("SystemExclamation", winsound.SND_ALIAS | winsound.SND_ASYNC)
        except Exception:
            pass
    threading.Thread(target=_play, daemon=True).start()


class EyeSaverApp:
    def __init__(self, work_seconds=DEFAULT_WORK_MINUTES * 60, break_seconds=DEFAULT_BREAK_SECONDS, play_chimes=True):
        self.work_seconds = work_seconds
        self.break_seconds = break_seconds
        self.play_chimes = play_chimes
        
        self.time_left = self.work_seconds
        self.is_paused = False
        self.is_in_break = False
        self.tip_index = 0
        
        # Main Controller Window (Small companion widget)
        self.root = tk.Tk()
        self.root.title("EyeSaver 20-20-20")
        self.root.geometry("380x280")
        self.root.resizable(False, False)
        self.root.configure(bg="#12131C")
        
        # Center the control window on screen
        self.center_window(self.root, 380, 280)
        
        # Fullscreen overlay reference
        self.overlay_window = None
        
        # Build UI
        self._build_control_ui()
        
        # Start countdown timer loop
        self.root.after(1000, self._timer_tick)

    def center_window(self, win, width, height):
        screen_w = win.winfo_screenwidth()
        screen_h = win.winfo_screenheight()
        x = (screen_w - width) // 2
        y = (screen_h - height) // 2
        win.geometry(f"{width}x{height}+{x}+{y}")

    def _build_control_ui(self):
        # Header banner
        header_frame = tk.Frame(self.root, bg="#1E202E", height=60)
        header_frame.pack(fill="x")
        header_frame.pack_propagate(False)
        
        title_label = tk.Label(
            header_frame,
            text="👁️ EyeSaver • 20-20-20",
            font=("SF Pro Display", 16, "bold"),
            fg="#61AFEF",
            bg="#1E202E"
        )
        title_label.pack(pady=(12, 2))
        
        subtitle_label = tk.Label(
            header_frame,
            text="Protecting your eyes during screen time",
            font=("SF Pro Text", 10),
            fg="#828997",
            bg="#1E202E"
        )
        subtitle_label.pack()

        # Body container
        body_frame = tk.Frame(self.root, bg="#12131C", padx=20, pady=15)
        body_frame.pack(fill="both", expand=True)

        # Status text
        self.status_label = tk.Label(
            body_frame,
            text="Next break in:",
            font=("SF Pro Text", 11),
            fg="#ABB2BF",
            bg="#12131C"
        )
        self.status_label.pack()

        # Large Countdown Timer
        self.countdown_label = tk.Label(
            body_frame,
            text=self._format_time(self.time_left),
            font=("SF Pro Display", 32, "bold"),
            fg="#98C379",
            bg="#12131C"
        )
        self.countdown_label.pack(pady=(0, 10))

        # Action Buttons frame
        btn_frame = tk.Frame(body_frame, bg="#12131C")
        btn_frame.pack(fill="x")

        # "Rest Now" button
        self.rest_btn = tk.Button(
            btn_frame,
            text="👁️ Take Break Now",
            command=self.start_break_now,
            font=("SF Pro Text", 11, "bold"),
            bg="#61AFEF",
            fg="#12131C",
            activebackground="#5294CB",
            activeforeground="#FFFFFF",
            relief="flat",
            padx=10,
            pady=6,
            cursor="pointinghand"
        )
        self.rest_btn.pack(side="left", expand=True, fill="x", padx=(0, 4))

        # "Pause/Resume" button
        self.pause_btn = tk.Button(
            btn_frame,
            text="⏸️ Pause",
            command=self.toggle_pause,
            font=("SF Pro Text", 11),
            bg="#2C313C",
            fg="#E5C07B",
            activebackground="#3E4451",
            activeforeground="#FFFFFF",
            relief="flat",
            padx=10,
            pady=6,
            cursor="pointinghand"
        )
        self.pause_btn.pack(side="left", expand=True, fill="x", padx=(4, 0))

        # Bottom info & Quit
        bottom_frame = tk.Frame(self.root, bg="#12131C", padx=20, pady=8)
        bottom_frame.pack(fill="x", side="bottom")

        unit = "min" if self.work_seconds >= 60 else "sec"
        work_val = int(self.work_seconds // 60) if self.work_seconds >= 60 else self.work_seconds
        rule_label = tk.Label(
            bottom_frame,
            text=f"Rule: Every {work_val} {unit} ➔ {self.break_seconds}s break",
            font=("SF Pro Text", 9),
            fg="#5C6370",
            bg="#12131C"
        )
        rule_label.pack(side="left")

        github_btn = tk.Button(
            bottom_frame,
            text="GitHub",
            command=lambda: webbrowser.open("https://github.com/ayoubgz1/eyesaver"),
            font=("SF Pro Text", 9),
            fg="#61AFEF",
            bg="#12131C",
            relief="flat",
            cursor="pointinghand",
            bd=0,
            highlightthickness=0
        )
        github_btn.pack(side="right", padx=(6, 0))

        quit_btn = tk.Button(
            bottom_frame,
            text="Quit",
            command=self.quit_app,
            font=("SF Pro Text", 9),
            fg="#E06C75",
            bg="#12131C",
            relief="flat",
            cursor="pointinghand",
            bd=0,
            highlightthickness=0
        )
        quit_btn.pack(side="right")

    def _format_time(self, total_seconds):
        mins = total_seconds // 60
        secs = total_seconds % 60
        return f"{mins:02d}:{secs:02d}"

    def toggle_pause(self):
        self.is_paused = not self.is_paused
        if self.is_paused:
            self.pause_btn.configure(text="▶️ Resume", fg="#98C379")
            self.status_label.configure(text="Paused (Timer halted)")
            self.countdown_label.configure(fg="#5C6370")
        else:
            self.pause_btn.configure(text="⏸️ Pause", fg="#E5C07B")
            self.status_label.configure(text="Next break in:")
            self.countdown_label.configure(fg="#98C379")

    def _timer_tick(self):
        if not self.is_paused and not self.is_in_break:
            if self.time_left > 0:
                self.time_left -= 1
                self.countdown_label.configure(text=self._format_time(self.time_left))
            else:
                self.start_break()
                
        # Loop every 1 second
        self.root.after(1000, self._timer_tick)

    def start_break_now(self):
        """Trigger instant break even if 20 minutes have not elapsed."""
        if not self.is_in_break:
            self.start_break()

    def start_break(self):
        self.is_in_break = True
        self.remaining_break_seconds = self.break_seconds
        
        # Pick rotating tip
        self.current_tip = TIPS[self.tip_index % len(TIPS)]
        self.tip_index += 1
        
        if self.play_chimes:
            play_sound("Ping")

        # Create Fullscreen Overlay
        self._show_fullscreen_overlay()

    def _show_fullscreen_overlay(self):
        if self.overlay_window is not None:
            try:
                self.overlay_window.destroy()
            except Exception:
                pass

        self.overlay_window = tk.Toplevel(self.root)
        self.overlay_window.configure(bg="#07080C")
        
        # Lock to full screen & topmost on macOS
        self.overlay_window.attributes("-fullscreen", True)
        self.overlay_window.attributes("-topmost", True)
        
        # Prevent window closing / escaping during break
        self.overlay_window.protocol("WM_DELETE_WINDOW", lambda: None)
        self.overlay_window.bind("<Escape>", lambda e: "break")
        self.overlay_window.bind("<Alt-F4>", lambda e: "break")
        self.overlay_window.bind("<Command-w>", lambda e: "break")
        self.overlay_window.bind("<Command-q>", lambda e: "break")
        
        # Grab all events and force focus so screen cannot be clicked through
        try:
            self.overlay_window.focus_force()
            self.overlay_window.grab_set()
        except Exception:
            pass

        # Build Overlay Content
        container = tk.Frame(self.overlay_window, bg="#07080C")
        container.place(relx=0.5, rely=0.5, anchor="center")

        # Glowing Icon / Symbol
        icon_lbl = tk.Label(
            container,
            text="👁️",
            font=("Apple Color Emoji", 70),
            bg="#07080C"
        )
        icon_lbl.pack(pady=(0, 10))

        # Main English Header
        main_title = tk.Label(
            container,
            text="LOOK AWAY",
            font=("SF Pro Display", 50, "bold"),
            fg="#61AFEF",
            bg="#07080C"
        )
        main_title.pack(pady=(0, 8))

        # Subtitle
        sub_title = tk.Label(
            container,
            text="Look at an object at least 20 feet (6 meters) away",
            font=("SF Pro Display", 22),
            fg="#E5C07B",
            bg="#07080C"
        )
        sub_title.pack(pady=(0, 25))

        # Circular/Badge Countdown Area
        self.break_count_label = tk.Label(
            container,
            text=f"{self.remaining_break_seconds}s",
            font=("SF Pro Display", 54, "bold"),
            fg="#98C379",
            bg="#07080C"
        )
        self.break_count_label.pack(pady=(0, 10))

        # Dynamic Progress Bar Canvas
        self.canvas_width = 400
        self.canvas_height = 8
        self.progress_canvas = tk.Canvas(
            container,
            width=self.canvas_width,
            height=self.canvas_height,
            bg="#1E202E",
            highlightthickness=0
        )
        self.progress_canvas.pack(pady=(0, 30))
        self.progress_bar = self.progress_canvas.create_rectangle(
            0, 0, self.canvas_width, self.canvas_height,
            fill="#98C379", outline=""
        )

        # Rotating Tip Card
        tip_frame = tk.Frame(container, bg="#12141F", padx=25, pady=15, relief="flat")
        tip_frame.pack()

        tip_title = tk.Label(
            tip_frame,
            text="💡 EYE CARE TIP",
            font=("SF Pro Text", 11, "bold"),
            fg="#56B6C2",
            bg="#12141F"
        )
        tip_title.pack(pady=(0, 4))

        self.tip_label = tk.Label(
            tip_frame,
            text=self.current_tip,
            font=("SF Pro Text", 14),
            fg="#ABB2BF",
            bg="#12141F",
            wraplength=600,
            justify="center"
        )
        self.tip_label.pack()

        # Enforced Notice
        notice_label = tk.Label(
            container,
            text="Screen locked for 20 seconds to enforce healthy eye rest • 20-20-20 Rule",
            font=("SF Pro Text", 11),
            fg="#5C6370",
            bg="#07080C"
        )
        notice_label.pack(pady=(25, 0))

        # Start the break countdown
        self._overlay_countdown_tick()

    def _overlay_countdown_tick(self):
        if not self.is_in_break:
            return

        if self.remaining_break_seconds > 0:
            # Update countdown text
            self.break_count_label.configure(text=f"{self.remaining_break_seconds}s")
            
            # Update progress bar
            fraction = self.remaining_break_seconds / self.break_seconds
            current_w = int(self.canvas_width * fraction)
            self.progress_canvas.coords(self.progress_bar, 0, 0, current_w, self.canvas_height)
            
            self.remaining_break_seconds -= 1
            self.root.after(1000, self._overlay_countdown_tick)
        else:
            # Break finished
            self._end_break()

    def _end_break(self):
        self.is_in_break = False
        
        # Play gentle completion sound
        if self.play_chimes:
            play_sound("Hero")

        # Destroy overlay window and release grabs
        if self.overlay_window is not None:
            try:
                self.overlay_window.grab_release()
            except Exception:
                pass
            try:
                self.overlay_window.destroy()
            except Exception:
                pass
            self.overlay_window = None

        # Reset main work timer to full duration (e.g. 20 minutes)
        self.time_left = self.work_seconds
        self.countdown_label.configure(text=self._format_time(self.time_left))
        
        # Restore focus to main root
        self.root.deiconify()
        self.root.lift()

    def quit_app(self):
        if messagebox.askyesno("Quit EyeSaver", "Are you sure you want to stop EyeSaver?"):
            self.root.destroy()
            sys.exit(0)

    def run(self):
        self.root.protocol("WM_DELETE_WINDOW", self.quit_app)
        self.root.mainloop()


def parse_args():
    parser = argparse.ArgumentParser(description="EyeSaver: 20-20-20 Rule for macOS")
    parser.add_argument(
        "--work",
        type=float,
        default=DEFAULT_WORK_MINUTES,
        help="Work duration in minutes (default: 20)"
    )
    parser.add_argument(
        "--break",
        dest="break_sec",
        type=int,
        default=DEFAULT_BREAK_SECONDS,
        help="Break duration in seconds (default: 20)"
    )
    parser.add_argument(
        "--test",
        action="store_true",
        help="Test mode: 5 seconds work interval, 5 seconds break duration"
    )
    parser.add_argument(
        "--no-sound",
        action="store_true",
        help="Disable chime sounds"
    )
    return parser.parse_args()


if __name__ == "__main__":
    args = parse_args()
    
    if args.test:
        work_sec = 5
        break_sec = 5
        print("⚡ Running in TEST MODE (5s work -> 5s break)")
    else:
        work_sec = int(args.work * 60)
        break_sec = args.break_sec
        print(f"🚀 Running EyeSaver: Every {args.work} minutes -> {break_sec} seconds break")
        
    app = EyeSaverApp(
        work_seconds=work_sec,
        break_seconds=break_sec,
        play_chimes=not args.no_sound
    )
    app.run()
