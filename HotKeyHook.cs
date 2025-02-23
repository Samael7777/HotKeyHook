using System.Collections.Immutable;
using System.ComponentModel;
using System.Runtime.InteropServices;
using HotKeyHook.PInvoke;
using VirtualKeys;

namespace HotKeyHook;

public class HotKeyHook : IDisposable
{
    // ReSharper disable once InconsistentNaming
    private const uint WM_HOTKEY = 0x0312;
    
    private readonly Dictionary<HotKey, int> _hotkeys;
    private readonly IMessageReceiver _msgWindow;

    public event EventHandler<HotKeyEventArgs>? HotKeyCaptured;

    public HotKeyHook(IMessageReceiver messageWindow)
    {
        _hotkeys = new Dictionary<HotKey, int>();
        _msgWindow = messageWindow;
        _msgWindow.WindowsMessageReceived += OnMessageCaptured;
    }

    public IReadOnlyDictionary<HotKey, int> RegisteredHotKeys => _hotkeys.ToImmutableDictionary();

    public void AddHotKey(HotKey hotKey)
    {
        if (_hotkeys.ContainsKey(hotKey)) return;
        var newId = GetFreeId();
        _hotkeys.Add(hotKey, newId);
        RegisterNewHotKey(newId, hotKey);
    }

    public void RemoveHotKey(HotKey hotKey)
    {
        if(!_hotkeys.ContainsKey(hotKey)) return;
        _hotkeys.Remove(hotKey);
    }

    private void OnMessageCaptured(object? sender, WindowsMessageArgs e)
    {
        if (e.Msg != WM_HOTKEY) return;

        var id = (int)e.WParam;
        var hotKeyData = (uint)e.LParam; 
        var modifiers = (Modifiers)(hotKeyData & 0xFFFF);
        var key = (Key)(hotKeyData >> 16);

        var hotkey = new HotKey(modifiers, key);
        var hotKeyEventArgs = new HotKeyEventArgs(id, 
            id < 0 ? null : hotkey, 
            id < 0 ? (IdHot)id : IdHot.None);

        HotKeyCaptured?.Invoke(this, hotKeyEventArgs);
    }

    private void RegisterNewHotKey(int id, HotKey hotKey)
    {
        _msgWindow.Invoke(() =>
        {
            var wndHandle = _msgWindow.WindowHandle;
            if (wndHandle == IntPtr.Zero)
                throw new ApplicationException("Can't get message window handle.");
            
            if (User32.RegisterHotKey(wndHandle, id, hotKey.Modifiers, hotKey.Key)) return;

            var error = Marshal.GetLastWin32Error();
            throw new Win32Exception(error, "Error registering new hotkey.");
        });
    }

    private void UnregisterHotKey(int id)
    {
        _msgWindow.Invoke(() =>
        {
            var wndHandle = _msgWindow.WindowHandle;
            if (wndHandle == IntPtr.Zero) return;

            if(User32.UnregisterHotKey(wndHandle, id)) return;

            var error = Marshal.GetLastWin32Error();
            throw new Win32Exception(error, "Error unregistering hotkey.");
        });
    }

    private int GetFreeId()
    {
        if (!_hotkeys.Any()) return 0;

        var sortedIds = _hotkeys.Values.OrderBy(id => id).ToArray();

        for (var i = 0; i < sortedIds.Length; i++)
        {
            if (sortedIds[i] != i) return i;
        }

        return sortedIds.Length;
    }

    #region Dispose

    private bool _disposed;

    ~HotKeyHook()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            //dispose managed state (managed objects)
            _msgWindow.WindowsMessageReceived -= OnMessageCaptured;
            foreach (var id in _hotkeys.Values)
            {
                UnregisterHotKey(id);
            }
        }
        //free unmanaged resources (unmanaged objects) and override finalizer
        //set large fields to null

        _disposed = true;
    }

    #endregion
}