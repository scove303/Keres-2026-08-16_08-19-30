namespace Keres.Input.Core
{
    public interface IRawInputProvider
    {
        string Name { get; }
        void Connect();
        void Collect(InputQueue queue, Diagnostics diagnostics);
        void Disconnect();
    }
}
