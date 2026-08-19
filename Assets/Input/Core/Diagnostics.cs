namespace Keres.Input.Core
{
    public sealed class Diagnostics
    {
        public long EventsEnqueued;
        public long EventsDequeued;
        public long EventsDropped;
        public long Frames;
        public long MonotonicViolations;
        public int QueueDepthMax;

        long captureConsumeTicksTotal;
        long captureConsumeTicksMax;
        long lastConsumedTicks;

        public double AvgCaptureConsumeMs =>
            EventsDequeued == 0
                ? 0
                : Timestamp.ToMilliseconds(captureConsumeTicksTotal / EventsDequeued);
        public double MaxCaptureConsumeMs => Timestamp.ToMilliseconds(captureConsumeTicksMax);

        public void RecordDequeue(in RawInputEvent evt, long nowTicks)
        {
            if (evt.TimestampTicks < lastConsumedTicks)
                MonotonicViolations++;
            lastConsumedTicks = evt.TimestampTicks;

            long latency = nowTicks - evt.TimestampTicks;
            if (latency > captureConsumeTicksMax)
                captureConsumeTicksMax = latency;
            captureConsumeTicksTotal += latency;
        }

        public void Reset()
        {
            EventsEnqueued = EventsDequeued = EventsDropped = Frames = MonotonicViolations = 0;
            QueueDepthMax = 0;
            captureConsumeTicksTotal = captureConsumeTicksMax = 0;
            lastConsumedTicks = 0;
        }
    }
}
