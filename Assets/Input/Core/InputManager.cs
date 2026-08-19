using System;

namespace Keres.Input.Core
{
    /// Orchestrator: provider → queue → state. Single thread (MVP, lock-free).
    /// Unity layer calls BeginFrame() from PlayerLoop EarlyUpdate (index 0) —
    /// the earliest point of the frame, before Update / FixedUpdate / physics.
    public sealed class InputManager
    {
        public InputQueue Queue { get; } = new InputQueue(4096);
        public InputState State { get; } = new InputState();
        public Diagnostics Diagnostics { get; } = new Diagnostics();

        IRawInputProvider provider;

        public void SetProvider(IRawInputProvider p)
        {
            provider?.Disconnect();
            provider = p;
            provider?.Connect();
        }

        public IRawInputProvider Provider => provider;

        /// One per frame, as early as possible.
        public void BeginFrame()
        {
            Diagnostics.Frames++;
            long now = Timestamp.NowTicks;

            State.SnapshotPrev();

            provider?.Collect(Queue, Diagnostics);
            if (Queue.Count > Diagnostics.QueueDepthMax)
                Diagnostics.QueueDepthMax = Queue.Count;

            while (Queue.Dequeue(out RawInputEvent evt))
            {
                Diagnostics.EventsDequeued++;
                Diagnostics.RecordDequeue(evt, now);
                State.Apply(evt);
            }

            State.ClearFrame();
        }

        public void Shutdown()
        {
            provider?.Disconnect();
            provider = null;
        }
    }
}
