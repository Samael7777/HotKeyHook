using VirtualKeys;

namespace HotKeyHook;

public class HotKeyEventArgs : EventArgs
{
    public HotKeyEventArgs(int id, HotKey? hotkey, IdHot systemHotKey = IdHot.None)
    {
        Id = id;
        HotKey = hotkey;
        SystemHotKey = systemHotKey;
    }
    public int Id { get; }
    public HotKey? HotKey { get; }
    public IdHot SystemHotKey { get; }
}