using TerraFX.Interop.Windows;
using TerraFX.Interop.WinRT;

namespace LLHookLib.Mouse;

public readonly ref struct MouseScrollEvent
{
    public int X => _data.pt.x;
    public int Y => _data.pt.y;
    public MouseEventFlags Flags => (MouseEventFlags)_data.flags;
    public uint Time => _data.time;
    public int Count => Math.Abs(unchecked((short)((long)_data.mouseData >> 16))) / 120;
    public MouseScrollDirection Direction =>
        (unchecked((short)((long)_data.mouseData >> 16))) > 0 ? MouseScrollDirection.Up : MouseScrollDirection.Down;

    private readonly WPARAM _type;
    private readonly MSLLHOOKSTRUCT _data;

    public MouseScrollEvent(WPARAM type, MSLLHOOKSTRUCT data)
    {
        _type = type;
        _data = data;
    }
}
