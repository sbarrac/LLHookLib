using TerraFX.Interop.Windows;
using static TerraFX.Interop.Windows.Windows;

namespace LLHookLib.Mouse;

public readonly ref struct MouseMoveEvent
{
    public int X => _data.pt.x;
    public int Y => _data.pt.y;

    public int RelativeX => _data.pt.x - _previousPosition.x;
    public int RelativeY => _data.pt.y - _previousPosition.y;

    public MouseEventFlags Flags => (MouseEventFlags)_data.flags;
    public uint Time => _data.time;

    private readonly MSLLHOOKSTRUCT _data;
    private readonly POINT _previousPosition;

    internal MouseMoveEvent(MSLLHOOKSTRUCT data)
    {
        _data = data;

        unsafe
        {
            POINT pt;
            GetCursorPos(&pt);
            _previousPosition = pt;
        }
    }
}
