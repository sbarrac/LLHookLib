using TerraFX.Interop.Windows;

namespace LLHookLib.Keyboard;

public readonly ref struct KeyboardHookEvent
{
    public VirtualKey VKey => (VirtualKey)_data.vkCode;
    public ScanCode ScanCode => (ScanCode)_data.scanCode;
    public KeyboardKeyState KeyState { get; }
    public uint Time => _data.time;
    public KeyboardEventFlags Flags => (KeyboardEventFlags) _data.flags;

    private readonly WPARAM _type;
    private readonly KBDLLHOOKSTRUCT _data;

    internal KeyboardHookEvent(WPARAM type, KBDLLHOOKSTRUCT data)
    {
        _type = type;
        _data = data;

        switch (type)
        {
            case WM.WM_KEYDOWN:
            case WM.WM_SYSKEYDOWN:
                KeyState = KeyboardKeyState.Pressed;
                break;
            case WM.WM_KEYUP:
            case WM.WM_SYSKEYUP:
                KeyState = KeyboardKeyState.Released;
                break;
        }

        
    }
}
