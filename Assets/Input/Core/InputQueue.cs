namespace Keres.Input.Core
{
    public sealed class InputQueue
    {
        readonly RawInputEvent[] buffer;
        readonly int capacity;
        int head;
        int tail;
        int count;

        public InputQueue(int capacity = 4096)
        {
            this.capacity = capacity;
            buffer = new RawInputEvent[capacity];
        }

        public int Count => count;
        public int Capacity => capacity;
        public bool IsEmpty => count == 0;
        public bool IsFull => count == capacity;

        public bool Enqueue(in RawInputEvent inputEvent)
        {
            if (IsFull)
            {
                tail = (tail + 1) % capacity;
                count--;
                buffer[head] = inputEvent;
                head = (head + 1) % capacity;
                count++;
                return false;
            }
            buffer[head] = inputEvent;
            head = (head + 1) % capacity;
            count++;
            return true;
        }

        public bool Dequeue(out RawInputEvent inputEvent)
        {
            if (IsEmpty)
            {
                inputEvent = default;
                return false;
            }
            inputEvent = buffer[tail];
            tail = (tail + 1) % capacity;
            count--;
            return true;
        }

        public bool Peek(out RawInputEvent inputEvent)
        {
            if (IsEmpty)
            {
                inputEvent = default;
                return false;
            }
            inputEvent = buffer[tail];
            return true;
        }

        public void Clear()
        {
            head = 0;
            tail = 0;
            count = 0;
        }
    }
}
