using System.Diagnostics;

namespace Keres.Input.Core
{
    /// QPC-based monotonic clock. Same source as Time.realtimeSinceStartup and
    /// InputEvent.time (Unity uses QPC internally) — so A/B comparison is fair.
    /// Pure C# — no UnityEngine.
    public static class Timestamp
    {
        public static long TicksPerSecond => Stopwatch.Frequency;

        public static long NowTicks => Stopwatch.GetTimestamp();

        public static double ToSeconds(long ticks) => ticks / (double)Stopwatch.Frequency;

        public static double ToMilliseconds(long ticks) => ticks * 1000.0 / Stopwatch.Frequency;
    }
}
