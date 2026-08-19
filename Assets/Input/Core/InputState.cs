using System;

namespace Keres.Input.Core
{
    /// <summary>
    /// Continuously queried state, built from the event stream.
    ///
    /// SEPARATE from events:
    /// - Gameplay queries current state.
    /// - Precision systems such as replay, rhythm and motion input
    ///   can consume the raw event stream directly.
    /// </summary>
    public sealed class InputState
    {
        private const int ControlCount = 256;

        private readonly ulong[] held = new ulong[(ControlCount + 63) / 64];
        private readonly ulong[] prev = new ulong[(ControlCount + 63) / 64];

        public long LastEventTicks;

        /// <summary>
        /// Accumulated raw mouse movement since ResetFrame().
        /// Values are device counts, NOT screen pixels.
        /// </summary>
        public float DeltaX;
        public float DeltaY;

        /// <summary>
        /// Accumulated mouse wheel movement since ResetFrame().
        /// </summary>
        public float Wheel;

        /// <summary>
        /// Absolute cursor position when provided by the backend.
        /// </summary>
        public float X;
        public float Y;

        public void Apply(in RawInputEvent e)
        {
            LastEventTicks = e.TimestampTicks;

            switch (e.EventType)
            {
                case InputEventType.ButtonDown:
                    SetHeld(e.Control, true);
                    break;

                case InputEventType.ButtonUp:
                    SetHeld(e.Control, false);
                    break;

                case InputEventType.Move:
                    DeltaX += e.DeltaX;
                    DeltaY += e.DeltaY;

                    // Backend-specific flag indicates that
                    // absolute position data is available.
                    if (e.RawFlags != 0)
                    {
                        X = e.X;
                        Y = e.Y;
                    }

                    break;

                case InputEventType.Wheel:
                    Wheel += e.Value;
                    break;

                case InputEventType.Axis:
                    // Gamepad / analog input.
                    // TODO: Add axis state storage in Phase 2.
                    break;

                case InputEventType.TouchBegan:
                case InputEventType.TouchMoved:
                case InputEventType.TouchEnded:
                case InputEventType.TouchCanceled:
                    // TODO: Touch state in a future phase.
                    break;

                case InputEventType.Unknown:
                default:
                    break;
            }
        }

        public bool Held(InputControl control)
        {
            int index = (int)control;

            if (!IsValidControl(index))
                return false;

            return (held[index >> 6] & (1UL << (index & 63))) != 0;
        }

        public bool Pressed(InputControl control)
        {
            int index = (int)control;

            if (!IsValidControl(index))
                return false;

            return (held[index >> 6] & (1UL << (index & 63))) != 0
                && (prev[index >> 6] & (1UL << (index & 63))) == 0;
        }

        public bool Released(InputControl control)
        {
            int index = (int)control;

            if (!IsValidControl(index))
                return false;

            return (held[index >> 6] & (1UL << (index & 63))) == 0
                && (prev[index >> 6] & (1UL << (index & 63))) != 0;
        }

        /// <summary>
        /// Must be called at the START of a frame, BEFORE any events
        /// are applied, so edge detection can compare against the
        /// previous frame's held state.
        /// </summary>
        public void SnapshotPrev()
        {
            Array.Copy(held, prev, prev.Length);
        }

        /// <summary>
        /// Must be called at the END of a frame, AFTER all events
        /// have been applied. Clears per-frame accumulated values.
        /// </summary>
        public void ClearFrame()
        {
            DeltaX = 0;
            DeltaY = 0;
            Wheel = 0;
        }

        private void SetHeld(InputControl control, bool value)
        {
            int index = (int)control;

            if (!IsValidControl(index))
                return;

            int word = index >> 6;
            int bit = index & 63;

            if (value)
                held[word] |= 1UL << bit;
            else
                held[word] &= ~(1UL << bit);
        }

        private static bool IsValidControl(int index)
        {
            return index >= 0 && index < ControlCount;
        }
    }
}
