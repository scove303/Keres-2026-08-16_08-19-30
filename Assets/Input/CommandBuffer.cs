using System.Collections.Generic;
using UnityEngine;

namespace Keres.Input
{
    /// L2: The DMC secret — buffered inputs with timestamps.
    /// A press lives in the buffer for `window` seconds, so combat can
    /// consume it later (buffered combos, jump-cancels).
    /// BACKEND-AGNOSTIC: nạp từ InputProvider (Input System) HOẶC từ
    /// UltraInput InputBridge (Phase 3) — combat code không đổi.
    /// Dùng Time.time cho tầng fallback; bản UltraInput (Phase 3) có thể đổi
    /// thành TimestampTicks khi cần replay-determinism.
    public class CommandBuffer
    {
        readonly float window;
        readonly List<BufferedCommand> buffer = new();

        public CommandBuffer(float windowSeconds = 0.15f)
        {
            window = windowSeconds;
        }

        public void Feed(string action)
        {
            buffer.Add(new BufferedCommand(action, Time.time));
        }

        /// Returns true once if a command is still valid inside the window.
        /// Iterates backwards so RemoveAt never skips a command.
        public bool Consume(string action)
        {
            for (int i = buffer.Count - 1; i >= 0; i--)
            {
                if (buffer[i].action != action)
                    continue;
                if (Time.time - buffer[i].time > window)
                    continue;
                buffer.RemoveAt(i);
                return true;
            }
            return false;
        }

        /// Drop expired commands (call every frame).
        public void Tick() => buffer.RemoveAll(c => Time.time - c.time > window);

        public int Count => buffer.Count;

        readonly struct BufferedCommand
        {
            public readonly string action;
            public readonly float time;

            public BufferedCommand(string action, float time)
            {
                this.action = action;
                this.time = time;
            }
        }
    }
}
