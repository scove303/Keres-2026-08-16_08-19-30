using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Controls;

namespace Keres.Input.Core
{
    /// <summary>
    /// Raw input provider fed by the Unity Input System low-level event
    /// stream (InputSystem.onEvent). No .inputactions asset, no actions,
    /// no rebinding layers — the events are read straight from devices.
    ///
    /// Emits ButtonDown/ButtonUp by diffing device state against a cached
    /// per-control snapshot, so presses cannot be missed between frames.
    ///
    /// NOTE: timestamps come from the same Stopwatch/QPC clock as
    /// Timestamp.NowTicks so latency diagnostics stay meaningful.
    /// </summary>
    public sealed class InputSystemRawProvider : IRawInputProvider
    {
        static readonly Key[] Keys;
        static readonly InputControl[] KeyControls;

        readonly bool[] keyDown;
        readonly bool[] mouseDown;

        InputQueue queue;
        Diagnostics diagnostics;
        bool connected;

        static InputSystemRawProvider()
        {
            var keys = new List<Key>();
            var controls = new List<InputControl>();

            foreach (Key key in Enum.GetValues(typeof(Key)))
            {
                InputControl control = MapKey(key);
                if (control == InputControl.None)
                    continue;
                keys.Add(key);
                controls.Add(control);
            }

            Keys = keys.ToArray();
            KeyControls = controls.ToArray();
        }

        public InputSystemRawProvider()
        {
            keyDown = new bool[Keys.Length];
            mouseDown = new bool[MouseButtonCount];
        }

        public string Name => "Unity Input System (raw events)";

        public void Connect()
        {
            if (connected)
                return;
            connected = true;
            InputSystem.onEvent += OnEvent;
        }

        public void Disconnect()
        {
            if (!connected)
                return;
            connected = false;
            InputSystem.onEvent -= OnEvent;
            Array.Clear(keyDown, 0, keyDown.Length);
            Array.Clear(mouseDown, 0, mouseDown.Length);
        }

        public void Collect(InputQueue queue, Diagnostics diagnostics)
        {
            this.queue = queue;
            this.diagnostics = diagnostics;
        }

        void OnEvent(InputEventPtr eventPtr, InputDevice device)
        {
            if (queue == null)
                return;

            switch (device)
            {
                case Keyboard keyboard:
                    ProcessKeyboard(keyboard);
                    break;
                case Mouse mouse:
                    ProcessMouse(mouse);
                    break;
            }
        }

        void ProcessKeyboard(Keyboard keyboard)
        {
            for (int i = 0; i < Keys.Length; i++)
            {
                bool pressed = keyboard[Keys[i]].isPressed;
                if (pressed == keyDown[i])
                    continue;

                keyDown[i] = pressed;

                Push(
                    new RawInputEvent
                    {
                        TimestampTicks = Stopwatch.GetTimestamp(),
                        DeviceType = InputDeviceType.Keyboard,
                        DeviceId = keyboard.deviceId,
                        EventType = pressed
                            ? InputEventType.ButtonDown
                            : InputEventType.ButtonUp,
                        Control = KeyControls[i],
                        Value = pressed ? 1f : 0f,
                    }
                );
            }
        }

        void ProcessMouse(Mouse mouse)
        {
            ProcessMouseButton(mouse.leftButton, InputControl.MouseLeft, mouse, 0);
            ProcessMouseButton(mouse.rightButton, InputControl.MouseRight, mouse, 1);
            ProcessMouseButton(mouse.middleButton, InputControl.MouseMiddle, mouse, 2);
            ProcessMouseButton(mouse.forwardButton, InputControl.MouseXButton2, mouse, 3);
            ProcessMouseButton(mouse.backButton, InputControl.MouseXButton1, mouse, 4);

            UnityEngine.Vector2 delta = mouse.delta.ReadValue();
            if (delta.x != 0f || delta.y != 0f)
            {
                UnityEngine.Vector2 position = mouse.position.ReadValue();

                Push(
                    new RawInputEvent
                    {
                        TimestampTicks = Stopwatch.GetTimestamp(),
                        DeviceType = InputDeviceType.Mouse,
                        DeviceId = mouse.deviceId,
                        EventType = InputEventType.Move,
                        Control = InputControl.MouseMove,
                        DeltaX = delta.x,
                        DeltaY = delta.y,
                        X = position.x,
                        Y = position.y,
                        RawFlags = 1,
                    }
                );
            }

            float wheel = mouse.scroll.ReadValue().y;
            if (MathF.Abs(wheel) > 0.0001f)
            {
                Push(
                    new RawInputEvent
                    {
                        TimestampTicks = Stopwatch.GetTimestamp(),
                        DeviceType = InputDeviceType.Mouse,
                        DeviceId = mouse.deviceId,
                        EventType = InputEventType.Wheel,
                        Control = InputControl.MouseWheel,
                        Value = wheel,
                    }
                );
            }
        }

        void ProcessMouseButton(ButtonControl button, InputControl control, Mouse mouse, int index)
        {
            bool pressed = button.isPressed;
            if (pressed == mouseDown[index])
                return;

            mouseDown[index] = pressed;

            Push(
                new RawInputEvent
                {
                    TimestampTicks = Stopwatch.GetTimestamp(),
                    DeviceType = InputDeviceType.Mouse,
                    DeviceId = mouse.deviceId,
                    EventType = pressed ? InputEventType.ButtonDown : InputEventType.ButtonUp,
                    Control = control,
                    Value = pressed ? 1f : 0f,
                }
            );
        }

        void Push(in RawInputEvent evt)
        {
            if (!queue.Enqueue(evt))
                diagnostics.EventsDropped++;
            else
                diagnostics.EventsEnqueued++;
        }

        const int MouseButtonCount = 5;

        static InputControl MapKey(Key key)
        {
            return key switch
            {
                >= Key.A and <= Key.Z => (InputControl)((int)InputControl.KeyA + (key - Key.A)),

                Key.Digit0 => InputControl.Key0,
                Key.Digit1 => InputControl.Key1,
                Key.Digit2 => InputControl.Key2,
                Key.Digit3 => InputControl.Key3,
                Key.Digit4 => InputControl.Key4,
                Key.Digit5 => InputControl.Key5,
                Key.Digit6 => InputControl.Key6,
                Key.Digit7 => InputControl.Key7,
                Key.Digit8 => InputControl.Key8,
                Key.Digit9 => InputControl.Key9,

                Key.Space => InputControl.KeySpace,
                Key.Enter => InputControl.KeyEnter,
                Key.Escape => InputControl.KeyEscape,
                Key.Tab => InputControl.KeyTab,
                Key.Backspace => InputControl.KeyBackspace,

                Key.LeftShift => InputControl.KeyLeftShift,
                Key.RightShift => InputControl.KeyRightShift,
                Key.LeftCtrl => InputControl.KeyLeftControl,
                Key.RightCtrl => InputControl.KeyRightControl,
                Key.LeftAlt => InputControl.KeyLeftAlt,
                Key.RightAlt => InputControl.KeyRightAlt,
                Key.LeftMeta => InputControl.KeyMeta,
                Key.RightMeta => InputControl.KeyMeta,

                Key.LeftArrow => InputControl.KeyArrowLeft,
                Key.UpArrow => InputControl.KeyArrowUp,
                Key.RightArrow => InputControl.KeyArrowRight,
                Key.DownArrow => InputControl.KeyArrowDown,

                Key.Home => InputControl.KeyHome,
                Key.End => InputControl.KeyEnd,
                Key.PageUp => InputControl.KeyPageUp,
                Key.PageDown => InputControl.KeyPageDown,
                Key.Insert => InputControl.KeyInsert,
                Key.Delete => InputControl.KeyDelete,

                Key.CapsLock => InputControl.KeyCapsLock,
                Key.NumLock => InputControl.KeyNumLock,
                Key.ScrollLock => InputControl.KeyScrollLock,

                >= Key.F1 and <= Key.F12 => (InputControl)((int)InputControl.KeyF1 + (key - Key.F1)),

                _ => InputControl.None,
            };
        }
    }
}
