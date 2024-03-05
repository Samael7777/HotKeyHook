namespace HotKeyHook;

public interface IMessageReceiver
{
    public event EventHandler<WindowsMessageArgs> WindowsMessageReceived;
    public IntPtr WindowHandle { get; }
    public void Invoke(Action action);
}