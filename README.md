# 👁️ EyeSaver (20-20-20 Rule)

<p align="center">
  <img src="https://img.shields.io/badge/macOS-11.0+-blue.svg?style=for-the-badge&logo=apple" alt="macOS" />
  <img src="https://img.shields.io/badge/Windows-10%20%2F%2011-0078D6.svg?style=for-the-badge&logo=windows" alt="Windows" />
  <img src="https://img.shields.io/badge/Swift-Native-orange.svg?style=for-the-badge&logo=swift" alt="Swift" />
  <img src="https://img.shields.io/badge/.NET%208-C%23%20Native-512BD4.svg?style=for-the-badge&logo=dotnet" alt=".NET 8" />
  <img src="https://img.shields.io/badge/License-MIT-green.svg?style=for-the-badge" alt="License" />
  <img src="https://img.shields.io/github/v/release/ayoubgz1/eyesaver?style=for-the-badge&color=purple" alt="Release" />
</p>

<p align="center">
  <strong>A lightweight, elegant native application for macOS & Windows designed to protect your vision using the science-backed 20-20-20 rule.</strong>
</p>

---

## ⚡ 1-Line Quick Install

Choose your operating system and run the one-liner in your terminal:

### 🍎 For macOS (Terminal):
```bash
curl -fsSL https://raw.githubusercontent.com/ayoubgz1/eyesaver/main/install.sh | bash
```
*(Downloads the latest release, installs to `/Applications`, configures permissions, and launches the app).*

### 🪟 For Windows (PowerShell):
```powershell
irm https://raw.githubusercontent.com/ayoubgz1/eyesaver/main/install.ps1 | iex
```
*(Downloads standalone `EyeSaver.exe` to `%LOCALAPPDATA%\EyeSaver`, creates Start Menu and Desktop shortcuts, and starts the tray app).*

---

## 📥 Manual Downloads

Download pre-compiled binaries directly from the **[Releases Page](https://github.com/ayoubgz1/eyesaver/releases/latest)**:

### 🍏 macOS Packages
| Package | Format | Description |
| :--- | :--- | :--- |
| 📦 **[EyeSaver-v1.0.0.pkg](https://github.com/ayoubgz1/eyesaver/releases/latest/download/EyeSaver-v1.0.0.pkg)** | Native PKG Installer | **Recommended for macOS.** Installs via standard macOS wizard without security issues. |
| 💿 **[EyeSaver-v1.0.0.dmg](https://github.com/ayoubgz1/eyesaver/releases/latest/download/EyeSaver-v1.0.0.dmg)** | Disk Image | Drag and drop into Applications. |
| 🗜️ **[EyeSaver-v1.0.0-macOS.zip](https://github.com/ayoubgz1/eyesaver/releases/latest/download/EyeSaver-v1.0.0-macOS.zip)** | ZIP Archive | Standalone app bundle. |

### 🪟 Windows Packages
| Package | Format | Description |
| :--- | :--- | :--- |
| 🚀 **[EyeSaver.exe](https://github.com/ayoubgz1/eyesaver/releases/latest/download/EyeSaver.exe)** | Standalone Executable | **Recommended for Windows.** Single-file standalone executable (Zero install, no dependencies). |
| 🗜️ **[EyeSaver-v1.0.0-Windows.zip](https://github.com/ayoubgz1/eyesaver/releases/latest/download/EyeSaver-v1.0.0-Windows.zip)** | ZIP Archive | Portable zip package containing `EyeSaver.exe`. |

> [!TIP]
> **macOS Gatekeeper Note:** If you see "App is damaged" or "Cannot be opened" on macOS, run:
> ```bash
> xattr -cr /Applications/EyeSaver.app
> ```

---

## 📖 About the 20-20-20 Rule

The **20-20-20 rule** is an ophthalmologist-recommended practice to reduce digital eye strain and prevent Computer Vision Syndrome (CVS):
> **Every 20 minutes**, look away from your screen at an object **20 feet (6 meters) away** for at least **20 seconds**.

**EyeSaver** sits discreetly in your menu bar / system tray and provides a locked, immersive fullscreen break overlay every 20 minutes to enforce healthy eye rest habits.

---

## ✨ Key Features

- 🟢 **Native Menu Bar / System Tray App**: Sits discreetly in your macOS menu bar or Windows system tray (`👁️`) without cluttering your Dock or Taskbar.
- ⏱️ **Live Countdown & Status**: Dynamic timer displaying time remaining until your next break.
- 🛡️ **Locked Fullscreen Blackout**: Sleek, distraction-free overlay across all connected monitors to ensure you look away.
- 🔔 **Gentle Sound Notifications**: Soft audio chimes when breaks start and finish.
- ⏸️ **Pause / Resume Anytime**: Pause the timer during meetings, presentations, or gaming sessions.
- ⚡ **Customizable Work Intervals**: Choose between 15m, 20m (default), or 30m intervals.
- 🧪 **Quick Break Test**: Test a 5-second sample break anytime directly from the menu.
- 💡 **Rotating Eye Care Tips**: Displays practical eye care and relaxation exercises during every break.
- 🚀 **Auto-Start with Windows**: Toggle start with Windows directly from the tray menu.
- 🖥️ **Universal & Lightweight**: Pure native Swift (Universal binary for Intel + Apple Silicon M1/M2/M3/M4) on macOS, and native standalone C# .NET 8 on Windows with zero background CPU drain.

---

## 🛠️ Build from Source

### macOS (Pure Swift Native)
```bash
# Clone the repository
git clone https://github.com/ayoubgz1/eyesaver.git
cd eyesaver

# Compile Universal binary (Intel + Apple Silicon)
swiftc -O -target arm64-apple-macos11.0 -o EyeSaver-arm64 main.swift
swiftc -O -target x86_64-apple-macos11.0 -o EyeSaver-x86_64 main.swift
lipo -create -output EyeSaver EyeSaver-arm64 EyeSaver-x86_64
rm -f EyeSaver-arm64 EyeSaver-x86_64

# Run
./EyeSaver
```

### Windows (C# / .NET 8 Native)
```powershell
# Clone the repository
git clone https://github.com/ayoubgz1/eyesaver.git
cd eyesaver\windows

# Build single-file standalone executable
dotnet publish EyeSaver.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ../dist/windows

# Run
..\dist\windows\EyeSaver.exe
```

---

## 🔗 Useful Links

- 🌐 **GitHub Repository**: [https://github.com/ayoubgz1/eyesaver](https://github.com/ayoubgz1/eyesaver)
- ⬇️ **Releases & Changelog**: [https://github.com/ayoubgz1/eyesaver/releases](https://github.com/ayoubgz1/eyesaver/releases)
- 📄 **License**: [MIT License](LICENSE)

---

<div dir="rtl">

## 🇸🇦 نبذة باللغة العربية (EyeSaver - تطبيق حماية العين)

تطبيق خفيف جداً وأصيل لنظامي **macOS** و **Windows** يعمل في شريط القوائم / شريط المهام لتطبيق قاعدة **20-20-20** الطبية لحماية النظر وتخفيف إجهاد العين:

- **كل 20 دقيقة**: شاشة استراحة سوداء مقفلة وهادئة لمدة 20 ثانية لتشجيعك على إراحة عضلات العين والنظر إلى مسافة بعيدة (6 أمتار).
- **أصلي وخفيف 100%**: لا يستهلك طاقة المعالج أو الذاكرة، ولا يعرض نوافذ مزعجة في شريط المهام أو الـ Dock.
- **تنبيهات صوتية ناعمة**: نغمات خفيفة عند بدء ونهاية كل استراحة.
- **دعم الشاشات المتعددة**: قفل كامل لجميع الشاشات المتصلة أثناء وقت الاستراحة.

### 🚀 التثبيت السريع بأمر واحد:

#### لنظام الماك (macOS Terminal):
```bash
curl -fsSL https://raw.githubusercontent.com/ayoubgz1/eyesaver/main/install.sh | bash
```

#### لنظام الويندوز (Windows PowerShell):
```powershell
irm https://raw.githubusercontent.com/ayoubgz1/eyesaver/main/install.ps1 | iex
```

</div>
