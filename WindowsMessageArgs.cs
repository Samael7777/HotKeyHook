namespace HotKeyHook;

public class WindowsMessageArgs : EventArgs
{
    public WindowsMessageArgs(uint msg, IntPtr wParam, IntPtr lParam)
    {
        Msg = msg;
        LParam = lParam;
        WParam = wParam;
    }

    public uint Msg { get; }
    public IntPtr WParam { get; }
    public IntPtr LParam { get; }

}