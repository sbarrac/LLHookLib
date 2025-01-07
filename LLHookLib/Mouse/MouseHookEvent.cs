using LLHookLib.Mouse;
using TerraFX.Interop.Windows;

namespace LLHookLib;

public readonly ref struct MouseHookEvent
{
    public MouseEventType Type { get; }
    public MouseButtonEvent ButtonEvent => new MouseButtonEvent(_wParam, _hookData);
    public MouseMoveEvent MoveEvent => new MouseMoveEvent(_hookData);
    public MouseScrollEvent ScrollEvent => new MouseScrollEvent(_wParam, _hookData);

    private readonly WPARAM _wParam;
    private readonly MSLLHOOKSTRUCT _hookData;

    internal MouseHookEvent(WPARAM wParam, MSLLHOOKSTRUCT hookData)
    {
        _wParam = wParam;
        _hookData = hookData;

        switch (wParam)
        {
            case WM.WM_MOUSEMOVE:
                Type = MouseEventType.Moved;
                break;
            case WM.WM_MOUSEWHEEL:
                Type = MouseEventType.Scroll;
                break;
            case WM.WM_LBUTTONDOWN:
            case WM.WM_LBUTTONUP:
            case WM.WM_RBUTTONDOWN:
            case WM.WM_RBUTTONUP:
            case WM.WM_MBUTTONDOWN:
            case WM.WM_MBUTTONUP:
            case WM.WM_XBUTTONDOWN:
            case WM.WM_XBUTTONUP:
                Type = MouseEventType.Button;
                break;
        }
    }
}
