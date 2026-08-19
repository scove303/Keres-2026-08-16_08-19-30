namespace Keres.Input.Core
{
    public enum InputDeviceType : byte
    {
        Unknown = 0,
        Mouse = 1,
        Keyboard = 2,
        Gamepad = 3,
        Touch = 4,
        Pen = 5,
        Other = 6,
    }

    public enum InputEventType : byte
    {
        Unknown = 0,

        ButtonDown = 1,
        ButtonUp = 2,

        Move = 3,
        Wheel = 4,
        Axis = 5,

        TouchBegan = 6,
        TouchMoved = 7,
        TouchEnded = 8,
        TouchCanceled = 9,
    }

    /// <summary>
    /// Platform-independent control identifiers.
    ///
    /// IMPORTANT:
    /// These values are NOT OS virtual-key codes.
    /// Platform backends must translate their native
    /// key/button identifiers into these controls.
    /// </summary>
    public enum InputControl : ushort
    {
        None = 0,

        // ============================================================
        // Keyboard
        // ============================================================

        KeyA,
        KeyB,
        KeyC,
        KeyD,
        KeyE,
        KeyF,
        KeyG,
        KeyH,
        KeyI,
        KeyJ,
        KeyK,
        KeyL,
        KeyM,
        KeyN,
        KeyO,
        KeyP,
        KeyQ,
        KeyR,
        KeyS,
        KeyT,
        KeyU,
        KeyV,
        KeyW,
        KeyX,
        KeyY,
        KeyZ,

        Key0,
        Key1,
        Key2,
        Key3,
        Key4,
        Key5,
        Key6,
        Key7,
        Key8,
        Key9,

        KeySpace,
        KeyEnter,
        KeyEscape,
        KeyTab,
        KeyBackspace,

        KeyShift,
        KeyControl,
        KeyAlt,
        KeyMeta,

        KeyLeftShift,
        KeyRightShift,
        KeyLeftControl,
        KeyRightControl,
        KeyLeftAlt,
        KeyRightAlt,

        KeyArrowLeft,
        KeyArrowUp,
        KeyArrowRight,
        KeyArrowDown,

        KeyHome,
        KeyEnd,
        KeyPageUp,
        KeyPageDown,
        KeyInsert,
        KeyDelete,

        KeyCapsLock,
        KeyNumLock,
        KeyScrollLock,

        KeyF1,
        KeyF2,
        KeyF3,
        KeyF4,
        KeyF5,
        KeyF6,
        KeyF7,
        KeyF8,
        KeyF9,
        KeyF10,
        KeyF11,
        KeyF12,

        // ============================================================
        // Mouse
        // ============================================================

        MouseLeft,
        MouseRight,
        MouseMiddle,
        MouseXButton1,
        MouseXButton2,

        MouseWheel,

        MouseMove,

        // ============================================================
        // Gamepad
        // ============================================================

        GamepadSouth,
        GamepadEast,
        GamepadWest,
        GamepadNorth,

        GamepadStart,
        GamepadSelect,

        GamepadLeftShoulder,
        GamepadRightShoulder,

        GamepadLeftStick,
        GamepadRightStick,

        GamepadDPadUp,
        GamepadDPadDown,
        GamepadDPadLeft,
        GamepadDPadRight,

        GamepadLeftTrigger,
        GamepadRightTrigger,

        GamepadLeftStickX,
        GamepadLeftStickY,
        GamepadRightStickX,
        GamepadRightStickY,

        // ============================================================
        // Touch
        // ============================================================

        TouchPrimary,
        TouchSecondary,

        // ============================================================
        // Pen / Tablet
        // ============================================================

        PenTip,
        PenBarrel,
        PenEraser,

        PenPressure,
        PenTiltX,
        PenTiltY,
    }

    /// <summary>
    /// Normalized raw input event.
    ///
    /// This structure contains no platform-specific types.
    /// It can safely be shared between Windows, Linux, macOS,
    /// Android, etc. backends.
    ///
    /// STRUCT = zero managed allocation.
    /// </summary>
    public struct RawInputEvent
    {
        /// <summary>
        /// Monotonic timestamp associated with this input event.
        ///
        /// The actual timestamp source is determined by the backend.
        /// For Windows this can be QPC.
        /// </summary>
        public long TimestampTicks;

        public InputDeviceType DeviceType;

        /// <summary>
        /// Stable identifier for the physical input device,
        /// when the backend can provide one.
        /// </summary>
        public int DeviceId;

        public InputEventType EventType;

        /// <summary>
        /// Platform-independent control identifier.
        /// </summary>
        public InputControl Control;

        /// <summary>
        /// Generic scalar value.
        ///
        /// Examples:
        /// - wheel delta
        /// - analog axis
        /// - pressure
        /// </summary>
        public float Value;

        /// <summary>
        /// Raw relative movement.
        ///
        /// For mouse:
        /// device counts, NOT screen pixels.
        /// </summary>
        public float DeltaX;
        public float DeltaY;

        /// <summary>
        /// Absolute position when the backend provides it.
        /// </summary>
        public float X;
        public float Y;

        /// <summary>
        /// Backend-specific flags.
        ///
        /// Core must not interpret these values directly.
        /// </summary>
        public uint RawFlags;
    }
}
