# 👁️ EyeSaver for macOS

<p align="center">
  <img src="https://img.shields.io/badge/macOS-11.0+-blue.svg?style=for-the-badge&logo=apple" alt="macOS" />
  <img src="https://img.shields.io/badge/Swift-5.9+-orange.svg?style=for-the-badge&logo=swift" alt="Swift" />
  <img src="https://img.shields.io/badge/License-MIT-green.svg?style=for-the-badge" alt="License" />
  <img src="https://img.shields.io/github/v/release/ayoubgz1/eyesaver?style=for-the-badge&color=purple" alt="Release" />
</p>

<p align="center">
  <strong>A lightweight, elegant macOS menu bar app designed to protect your vision using the science-backed 20-20-20 rule.</strong>
</p>

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
- 🚀 **Ultra Lightweight & Fast**: Pure native Swift, zero background CPU drain, no external dependencies.

---

## 📥 Download & Installation

### Option 1: Direct Download (Recommended)

1. Go to the **[Latest Release](https://github.com/ayoubgz1/eyesaver/releases/latest)**.
2. Download `EyeSaver-v1.0.0.dmg` or `EyeSaver-v1.0.0-macOS.zip`.
3. Open the downloaded file and drag **EyeSaver.app** into your **Applications** folder.
4. Launch **EyeSaver** from Applications or Spotlight.

> [!TIP]
> **macOS Security (Gatekeeper) Note:**  
> Since the app is open-source and not signed with a paid Apple Developer certificate, macOS might show a security warning. If prompted:
> 1. Right-click (or Control-click) `EyeSaver.app` and choose **Open**.
> 2. Or run this one-line command in Terminal:
>    ```bash
>    xattr -cr /Applications/EyeSaver.app
>    ```

---

## 🛠️ Build from Source

You can easily build the app yourself using Xcode or the Swift command line tools:

```bash
# Clone the repository
git clone https://github.com/ayoubgz1/eyesaver.git
cd eyesaver

# Compile the native executable
swiftc -O -o EyeSaver main.swift

# Run directly
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
- لا يستهلك موارد الجهاز إطلاقاً، ومبني بلغة Swift الأصلية.
- لتحميل أحدث إصدار: انتقل إلى صفحة **[Releases](https://github.com/ayoubgz1/eyesaver/releases)** وحمّل ملف `.dmg` أو `.zip`.

</div>
