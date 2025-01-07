using TerraFX.Interop.Windows;

namespace LLHookLib.Mouse;

public readonly ref struct MouseButtonEvent
{
    public int X => _data.pt.x;
    public int Y => _data.pt.y;
    public MouseEventFlags Flags => (MouseEventFlags)_data.flags;
    public uint Time => _data.time;
    public MouseButton Button { get; }
    public MouseButtonState ButtonState { get; }

    private readonly WPARAM _type;
    private readonly MSLLHOOKSTRUCT _data;

    internal MouseButtonEvent(WPARAM type, MSLLHOOKSTRUCT data)
    {
        _type = type;
        _data = data;

        switch (_type)
        {
            case WM.WM_LBUTTONDOWN:
                ButtonState = MouseButtonState.Pressed;
                Button = MouseButton.Left;
                break;
            case WM.WM_LBUTTONUP:
                ButtonState = MouseButtonState.Released;
                Button = MouseButton.Left;
                break;
            case WM.WM_RBUTTONDOWN:
                ButtonState = MouseButtonState.Pressed;
                Button = MouseButton.Right;
                break;
            case WM.WM_RBUTTONUP:
                ButtonState = MouseButtonState.Released;
                Button = MouseButton.Right;
                break;
            case WM.WM_MBUTTONDOWN:
                ButtonState = MouseButtonState.Pressed;
                Button = MouseButton.Middle;
                break;
            case WM.WM_MBUTTONUP:
                ButtonState = MouseButtonState.Released;
                Button = MouseButton.Middle;
                break;
            case WM.WM_XBUTTONDOWN:
            case WM.WM_XBUTTONUP:
                {
                    ButtonState = _type == WM.WM_XBUTTONDOWN ? MouseButtonState.Pressed : MouseButtonState.Released;
                    var button = _data.mouseData >> 16;
                    Button = button == 1 ? MouseButton.X1 : MouseButton.X2;
                    break;
                } 
        }
    }
}
