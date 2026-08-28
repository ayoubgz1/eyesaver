import Cocoa

// MARK: - Constants & Config
let DEFAULT_WORK_SECONDS = 20 * 60 // 20 minutes
let DEFAULT_BREAK_SECONDS = 20     // 20 seconds

let EYE_TIPS = [
    "Look at an object at least 20 feet (6 meters) away.",
    "Blink slowly and gently to rehydrate your eyes.",
    "Look out the window or across the farthest corner of the room.",
    "Relax your shoulders, neck, and facial muscles.",
    "Take a slow deep breath in... and exhale completely.",
    "Roll your eyes gently in circles to relieve strain."
]

// MARK: - Overlay Window Subclass to Intercept All Events
class NonClosableOverlayWindow: NSWindow {
    override var canBecomeKey: Bool { return true }
    override var canBecomeMain: Bool { return true }
    
    override func performKeyEquivalent(with event: NSEvent) -> Bool {
        return true // Suppress shortcuts like Cmd+W, Cmd+Q, Esc during break
    }
    
    override func keyDown(with event: NSEvent) {
        // Block all key presses
    }
}

// MARK: - Overlay View with Custom Graphics
class OverlayContentView: NSView {
    var remainingSeconds: Int = 20
    var totalBreakSeconds: Int = 20
    var currentTip: String = ""
    
    override func draw(_ dirtyRect: NSRect) {
        super.draw(dirtyRect)
        
        // Background
        let bgColor = NSColor(red: 0.03, green: 0.04, blue: 0.06, alpha: 0.98)
        bgColor.setFill()
        dirtyRect.fill()
        
        let center = CGPoint(x: bounds.midX, y: bounds.midY)
        
        // 1. Emoji / Icon
        let iconStr = "👁️" as NSString
        let iconFont = NSFont.systemFont(ofSize: 72)
        let iconAttrs: [NSAttributedString.Key: Any] = [
            .font: iconFont
        ]
        let iconSize = iconStr.size(withAttributes: iconAttrs)
        let iconRect = CGRect(x: center.x - iconSize.width / 2, y: center.y + 110, width: iconSize.width, height: iconSize.height)
        iconStr.draw(in: iconRect, withAttributes: iconAttrs)
        
        // 2. Title: LOOK AWAY
        let titleStr = "LOOK AWAY" as NSString
        let titleFont = NSFont.systemFont(ofSize: 48, weight: .black)
        let titleAttrs: [NSAttributedString.Key: Any] = [
            .font: titleFont,
            .foregroundColor: NSColor(red: 0.38, green: 0.69, blue: 0.94, alpha: 1.0) // Cyan/Blue
        ]
        let titleSize = titleStr.size(withAttributes: titleAttrs)
        let titleRect = CGRect(x: center.x - titleSize.width / 2, y: center.y + 45, width: titleSize.width, height: titleSize.height)
        titleStr.draw(in: titleRect, withAttributes: titleAttrs)
        
        // 3. Subtitle
        let subStr = "Look at an object at least 20 feet (6 meters) away" as NSString
        let subFont = NSFont.systemFont(ofSize: 20, weight: .medium)
        let subAttrs: [NSAttributedString.Key: Any] = [
            .font: subFont,
            .foregroundColor: NSColor(red: 0.90, green: 0.75, blue: 0.48, alpha: 1.0) // Warm Gold
        ]
        let subSize = subStr.size(withAttributes: subAttrs)
        let subRect = CGRect(x: center.x - subSize.width / 2, y: center.y + 10, width: subSize.width, height: subSize.height)
        subStr.draw(in: subRect, withAttributes: subAttrs)
        
        // 4. Countdown Number
        let countStr = "\(remainingSeconds)s" as NSString
        let countFont = NSFont.monospacedDigitSystemFont(ofSize: 56, weight: .bold)
        let countAttrs: [NSAttributedString.Key: Any] = [
            .font: countFont,
            .foregroundColor: NSColor(red: 0.60, green: 0.76, blue: 0.47, alpha: 1.0) // Calming Green
        ]
        let countSize = countStr.size(withAttributes: countAttrs)
        let countRect = CGRect(x: center.x - countSize.width / 2, y: center.y - 65, width: countSize.width, height: countSize.height)
        countStr.draw(in: countRect, withAttributes: countAttrs)
        
        // 5. Progress Bar
        let barWidth: CGFloat = 380
        let barHeight: CGFloat = 8
        let barRect = CGRect(x: center.x - barWidth / 2, y: center.y - 85, width: barWidth, height: barHeight)
        
        let bgBarPath = NSBezierPath(roundedRect: barRect, xRadius: 4, yRadius: 4)
        NSColor(white: 0.2, alpha: 0.8).setFill()
        bgBarPath.fill()
        
        let fraction = totalBreakSeconds > 0 ? CGFloat(remainingSeconds) / CGFloat(totalBreakSeconds) : 0
        let fillWidth = max(0, barWidth * fraction)
        let fillRect = CGRect(x: center.x - barWidth / 2, y: center.y - 85, width: fillWidth, height: barHeight)
        let fillBarPath = NSBezierPath(roundedRect: fillRect, xRadius: 4, yRadius: 4)
        NSColor(red: 0.60, green: 0.76, blue: 0.47, alpha: 1.0).setFill()
        fillBarPath.fill()
        
        // 6. Tip Box Background
        let tipBoxWidth: CGFloat = 550
        let tipBoxHeight: CGFloat = 70
        let tipBoxRect = CGRect(x: center.x - tipBoxWidth / 2, y: center.y - 180, width: tipBoxWidth, height: tipBoxHeight)
        let tipBoxPath = NSBezierPath(roundedRect: tipBoxRect, xRadius: 10, yRadius: 10)
        NSColor(red: 0.08, green: 0.09, blue: 0.13, alpha: 0.95).setFill()
        tipBoxPath.fill()
        
        // Tip Title
        let tipTitleStr = "💡 EYE CARE TIP" as NSString
        let tipTitleFont = NSFont.systemFont(ofSize: 11, weight: .bold)
        let tipTitleAttrs: [NSAttributedString.Key: Any] = [
            .font: tipTitleFont,
            .foregroundColor: NSColor(red: 0.34, green: 0.71, blue: 0.76, alpha: 1.0)
        ]
        let tipTitleSize = tipTitleStr.size(withAttributes: tipTitleAttrs)
        let tipTitleRect = CGRect(x: center.x - tipTitleSize.width / 2, y: center.y - 135, width: tipTitleSize.width, height: tipTitleSize.height)
        tipTitleStr.draw(in: tipTitleRect, withAttributes: tipTitleAttrs)
        
        // Tip Text
        let tipStr = currentTip as NSString
        let tipFont = NSFont.systemFont(ofSize: 14, weight: .regular)
        let tipAttrs: [NSAttributedString.Key: Any] = [
            .font: tipFont,
            .foregroundColor: NSColor(white: 0.85, alpha: 1.0)
        ]
        let tipSize = tipStr.size(withAttributes: tipAttrs)
        let tipRect = CGRect(x: center.x - tipSize.width / 2, y: center.y - 165, width: tipSize.width, height: tipSize.height)
        tipStr.draw(in: tipRect, withAttributes: tipAttrs)
        
        // 7. Enforce note
        let noteStr = "Screen locked for 20 seconds to enforce healthy rest • 20-20-20 Rule" as NSString
        let noteFont = NSFont.systemFont(ofSize: 11, weight: .regular)
        let noteAttrs: [NSAttributedString.Key: Any] = [
            .font: noteFont,
            .foregroundColor: NSColor(white: 0.45, alpha: 1.0)
        ]
        let noteSize = noteStr.size(withAttributes: noteAttrs)
        let noteRect = CGRect(x: center.x - noteSize.width / 2, y: center.y - 225, width: noteSize.width, height: noteSize.height)
        noteStr.draw(in: noteRect, withAttributes: noteAttrs)
    }
}

// MARK: - Main App Delegate
class AppDelegate: NSObject, NSApplicationDelegate {
    var statusItem: NSStatusItem!
    var timer: Timer?
    var breakTimer: Timer?
    
    var workSeconds = DEFAULT_WORK_SECONDS
    var breakSeconds = DEFAULT_BREAK_SECONDS
    
    var workTimeRemaining = DEFAULT_WORK_SECONDS
    var breakTimeRemaining = DEFAULT_BREAK_SECONDS
    
    var isPaused = false
    var isInBreak = false
    var tipIndex = 0
    
    var overlayWindows: [NonClosableOverlayWindow] = []
    
    // Status Menu Items
    var statusMenuItem: NSMenuItem!
    var pauseMenuItem: NSMenuItem!
    
    func applicationDidFinishLaunching(_ notification: Notification) {
        // Setup Status Bar Item
        setupStatusBar()
        
        // Start Work Timer (1 second interval)
        startWorkTimer()
    }
    
    func setupStatusBar() {
        statusItem = NSStatusBar.system.statusItem(withLength: NSStatusItem.variableLength)
        
        if let button = statusItem.button {
            if #available(macOS 11.0, *) {
                let config = NSImage.SymbolConfiguration(pointSize: 14, weight: .medium)
                if let eyeImage = NSImage(systemSymbolName: "eye.fill", accessibilityDescription: "EyeSaver")?.withSymbolConfiguration(config) {
                    eyeImage.isTemplate = true
                    button.image = eyeImage
                } else {
                    button.title = "👁️"
                }
            } else {
                button.title = "👁️"
            }
            button.toolTip = "EyeSaver 20-20-20 (Eye Rest Timer)"
        }
        
        let menu = NSMenu()
        
        // Title
        let titleItem = NSMenuItem(title: "👁️ EyeSaver (20-20-20 Rule)", action: nil, keyEquivalent: "")
        titleItem.isEnabled = false
        menu.addItem(titleItem)
        
        // Status Countdown
        statusMenuItem = NSMenuItem(title: "⏳ Next break in: 20:00", action: nil, keyEquivalent: "")
        statusMenuItem.isEnabled = false
        menu.addItem(statusMenuItem)
        
        menu.addItem(NSMenuItem.separator())
        
        // Actions
        let takeBreakItem = NSMenuItem(title: "👁️ Take Break Now", action: #selector(takeBreakNow), keyEquivalent: "b")
        takeBreakItem.target = self
        menu.addItem(takeBreakItem)
        
        pauseMenuItem = NSMenuItem(title: "⏸️ Pause Timer", action: #selector(togglePause), keyEquivalent: "p")
        pauseMenuItem.target = self
        menu.addItem(pauseMenuItem)
        
        menu.addItem(NSMenuItem.separator())
        
        // Quick Test Option
        let testBreakItem = NSMenuItem(title: "⚡ Test Quick Break (5s)", action: #selector(testQuickBreak), keyEquivalent: "t")
        testBreakItem.target = self
        menu.addItem(testBreakItem)
        
        // Duration Submenu
        let settingsMenu = NSMenu()
        
        let d20 = NSMenuItem(title: "Work: 20m / Break: 20s (Default)", action: #selector(setDuration20), keyEquivalent: "")
        d20.target = self
        settingsMenu.addItem(d20)
        
        let d15 = NSMenuItem(title: "Work: 15m / Break: 20s", action: #selector(setDuration15), keyEquivalent: "")
        d15.target = self
        settingsMenu.addItem(d15)
        
        let d30 = NSMenuItem(title: "Work: 30m / Break: 30s", action: #selector(setDuration30), keyEquivalent: "")
        d30.target = self
        settingsMenu.addItem(d30)
        
        let configItem = NSMenuItem(title: "⚙️ Intervals", action: nil, keyEquivalent: "")
        configItem.submenu = settingsMenu
        menu.addItem(configItem)
        
        menu.addItem(NSMenuItem.separator())
        
        // Quit
        let quitItem = NSMenuItem(title: "❌ Quit EyeSaver", action: #selector(quitApp), keyEquivalent: "q")
        quitItem.target = self
        menu.addItem(quitItem)
        
        statusItem.menu = menu
    }
    
    func startWorkTimer() {
        timer?.invalidate()
        timer = Timer.scheduledTimer(withTimeInterval: 1.0, repeats: true) { [weak self] _ in
            self?.tickWorkTimer()
        }
        RunLoop.main.add(timer!, forMode: .common)
    }
    
    func tickWorkTimer() {
        guard !isPaused && !isInBreak else { return }
        
        if workTimeRemaining > 0 {
            workTimeRemaining -= 1
            let mins = workTimeRemaining / 60
            let secs = workTimeRemaining % 60
            statusMenuItem.title = String(format: "⏳ Next break in: %02d:%02d", mins, secs)
        } else {
            startBreak()
        }
    }
    
    @objc func togglePause() {
        isPaused = !isPaused
        if isPaused {
            pauseMenuItem.title = "▶️ Resume Timer"
            statusMenuItem.title = "⏸️ Paused (Timer halted)"
        } else {
            pauseMenuItem.title = "⏸️ Pause Timer"
            let mins = workTimeRemaining / 60
            let secs = workTimeRemaining % 60
            statusMenuItem.title = String(format: "⏳ Next break in: %02d:%02d", mins, secs)
        }
    }
    
    @objc func takeBreakNow() {
        guard !isInBreak else { return }
        startBreak()
    }
    
    @objc func testQuickBreak() {
        guard !isInBreak else { return }
        startBreak(customBreakSecs: 5)
    }
    
    @objc func setDuration20() {
        workSeconds = 20 * 60
        breakSeconds = 20
        workTimeRemaining = workSeconds
    }
    
    @objc func setDuration15() {
        workSeconds = 15 * 60
        breakSeconds = 20
        workTimeRemaining = workSeconds
    }
    
    @objc func setDuration30() {
        workSeconds = 30 * 60
        breakSeconds = 30
        workTimeRemaining = workSeconds
    }
    
    func startBreak(customBreakSecs: Int? = nil) {
        isInBreak = true
        breakTimeRemaining = customBreakSecs ?? breakSeconds
        let totalBreak = breakTimeRemaining
        
        let tip = EYE_TIPS[tipIndex % EYE_TIPS.count]
        tipIndex += 1
        
        // Sound Chime
        NSSound(named: "Ping")?.play()
        
        // Create full screen overlay for each connected screen
        overlayWindows.removeAll()
        for screen in NSScreen.screens {
            let win = NonClosableOverlayWindow(
                contentRect: screen.frame,
                styleMask: [.borderless],
                backing: .buffered,
                defer: false,
                screen: screen
            )
            win.level = .screenSaver
            win.isOpaque = true
            win.backgroundColor = .black
            win.ignoresMouseEvents = false
            win.collectionBehavior = [.canJoinAllSpaces, .fullScreenAuxiliary]
            
            let contentView = OverlayContentView(frame: screen.frame)
            contentView.remainingSeconds = breakTimeRemaining
            contentView.totalBreakSeconds = totalBreak
            contentView.currentTip = tip
            win.contentView = contentView
            
            win.makeKeyAndOrderFront(nil)
            overlayWindows.append(win)
        }
        
        // Hide cursor to encourage looking away
        NSCursor.hide()
        
        // Start Break countdown
        breakTimer?.invalidate()
        breakTimer = Timer.scheduledTimer(withTimeInterval: 1.0, repeats: true) { [weak self] _ in
            self?.tickBreakTimer(totalBreak: totalBreak)
        }
        RunLoop.main.add(breakTimer!, forMode: .common)
    }
    
    func tickBreakTimer(totalBreak: Int) {
        guard isInBreak else { return }
        
        if breakTimeRemaining > 0 {
            breakTimeRemaining -= 1
            for win in overlayWindows {
                if let view = win.contentView as? OverlayContentView {
                    view.remainingSeconds = breakTimeRemaining
                    view.needsDisplay = true
                }
            }
        } else {
            endBreak()
        }
    }
    
    func endBreak() {
        isInBreak = false
        breakTimer?.invalidate()
        breakTimer = nil
        
        // Unhide cursor
        NSCursor.unhide()
        
        // Completion chime
        NSSound(named: "Hero")?.play()
        
        // Close overlay windows
        for win in overlayWindows {
            win.orderOut(nil)
        }
        overlayWindows.removeAll()
        
        // Reset work timer
        workTimeRemaining = workSeconds
        let mins = workTimeRemaining / 60
        let secs = workTimeRemaining % 60
        statusMenuItem.title = String(format: "⏳ Next break in: %02d:%02d", mins, secs)
    }
    
    @objc func quitApp() {
        NSApplication.shared.terminate(nil)
    }
}

// MARK: - App Entry Point
let app = NSApplication.shared
let delegate = AppDelegate()
app.delegate = delegate
app.setActivationPolicy(.accessory) // Hides app from Dock!
app.run()
