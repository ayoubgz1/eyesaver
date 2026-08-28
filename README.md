# 👁️ EyeSaver for macOS

<p align="center">
  <img src="https://img.shields.io/badge/macOS-11.0+-blue.svg?style=for-the-badge&logo=apple" alt="macOS" />
  <img src="https://img.shields.io/badge/Arch-Universal%20(Intel%20%2B%20Apple%20Silicon)-success?style=for-the-badge" alt="Architecture" />
  <img src="https://img.shields.io/badge/Swift-Native-orange.svg?style=for-the-badge&logo=swift" alt="Swift" />
  <img src="https://img.shields.io/badge/License-MIT-green.svg?style=for-the-badge" alt="License" />
  <img src="https://img.shields.io/github/v/release/ayoubgz1/eyesaver?style=for-the-badge&color=purple" alt="Release" />
</p>

<p align="center">
  <strong>A lightweight, elegant macOS menu bar app designed to protect your vision using the science-backed 20-20-20 rule.</strong>
</p>

---

## 🚀 Quick Install (1-Line Command)

The fastest and easiest way to install and run **EyeSaver** without any Gatekeeper or security warnings:

```bash
curl -fsSL https://raw.githubusercontent.com/ayoubgz1/eyesaver/main/install.sh | bash
```

*(This automatically downloads the latest release, installs it to your `/Applications` folder, configures permissions, and launches the app).*

---

## 📥 Manual Downloads

You can also download packages directly from the **[Releases Page](https://github.com/ayoubgz1/eyesaver/releases/latest)**:

| Package | Format | Description |
| :--- | :--- | :--- |
| 📦 **[EyeSaver-v1.0.0.pkg](https://github.com/ayoubgz1/eyesaver/releases/download/v1.0.0/EyeSaver-v1.0.0.pkg)** | Native PKG Installer | **Recommended.** Installs via standard macOS wizard without security issues. |
| 💿 **[EyeSaver-v1.0.0.dmg](https://github.com/ayoubgz1/eyesaver/releases/download/v1.0.0/EyeSaver-v1.0.0.dmg)** | Disk Image | Drag and drop into Applications. |
| 🗜️ **[EyeSaver-v1.0.0-macOS.zip](https://github.com/ayoubgz1/eyesaver/releases/download/v1.0.0/EyeSaver-v1.0.0-macOS.zip)** | ZIP Archive | Standalone app bundle. |

> [!TIP]
> **If you see "App is damaged" or "Cannot be opened" (macOS Gatekeeper):**  
> Run this single command in Terminal to clear the quarantine flag:
> ```bash
> xattr -cr /Applications/EyeSaver.app
> ```

---

## 📖 About the 20-20-20 Rule

The **20-20-20 rule** is an ophthalmologist-recommended practice to reduce digital eye strain:
> **Every 20 minutes**, look away from your screen at an object **20 feet (6 meters) away** for at least **20 seconds**.

**EyeSaver** runs quietly in your macOS menu bar and provides a locked, immersive fullscreen break overlay every 20 minutes to give your eye muscles the rest they need.

---

## ✨ Features

- 🟢 **Native Menu Bar App**: Sits discreetly in your menu bar (`👁️`) without cluttering your Dock.
- ⏱️ **Live Countdown & Status**: Always see time remaining until your next break.
- 🛡️ **Locked Fullscreen Blackout**: Smooth, dark overlay that prevents screen interaction during breaks to ensure you actually look away.
- 🔔 **Gentle Sound Notifications**: Soft audio cues when breaks begin and finish.
- ⏸️ **Pause / Resume Anytime**: Conveniently pause the timer during meetings, presentations, or gaming sessions.
- ⚡ **Customizable Work Intervals**: Choose between 15m, 20m, or 30m work periods.
- 🧪 **Quick Break Test**: Test a 5-second sample break anytime from the menu.
- 🖥️ **Universal Binary**: Fully native on both Apple Silicon (M1/M2/M3/M4) and Intel Macs.
- 🚀 **Ultra Lightweight & Fast**: Pure native Swift, zero background CPU drain, no external dependencies.

---

## 🛠️ Build from Source

```bash
# Clone the repository
git clone https://github.com/ayoubgz1/eyesaver.git
cd eyesaver

# Compile Universal binary
swiftc -O -target arm64-apple-macos11.0 -o EyeSaver-arm64 main.swift
swiftc -O -target x86_64-apple-macos11.0 -o EyeSaver-x86_64 main.swift
lipo -create -output EyeSaver EyeSaver-arm64 EyeSaver-x86_64
rm -f EyeSaver-arm64 EyeSaver-x86_64

# Run
./EyeSaver
```

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

<div dir="rtl">

## 🇸🇦 نبذة باللغة العربية (EyeSaver - تطبيق حماية العين)

تطبيق خفيف ومميز لنظام **macOS** يعمل في الشريط العلوي (Menu Bar) لتطبيق قاعدة **20-20-20** الصحية:
- **كل 20 دقيقة**: شاشة استراحة سوداء كاملة لمدة 20 ثانية لإراحة عضلات العين.
- يعمل بكفاءة تامة على جميع أجهزة الماك (معالجات M1/M2/M3 ومعالجات Intel).
- **أسرع طريقة للتثبيت:** افتح الـ Terminal ونفذ الأمر التالي:
  ```bash
  curl -fsSL https://raw.githubusercontent.com/ayoubgz1/eyesaver/main/install.sh | bash
  ```

</div>
