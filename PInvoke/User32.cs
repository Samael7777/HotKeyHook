using System.Runtime.InteropServices;
using VirtualKeys;

namespace HotKeyHook.PInvoke;

internal static class User32
{
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern  bool UnregisterHotKey(IntPtr hWnd, int id) ;

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern  bool RegisterHotKey(IntPtr hWnd, int id, Modifiers fsModifiers, Key key) ;

}