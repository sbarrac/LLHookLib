using System.ComponentModel;
using System.Runtime.InteropServices;
using LLHookLib.Keyboard;
using LLHookLib.Mouse;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.Windows.Windows;

namespace LLHookLib;

public delegate HookCallbackResult MouseHookHandler(MouseHookEvent @event);
public delegate HookCallbackResult KeyboardHookHandler(KeyboardHookEvent @event);

public unsafe sealed class LLHook : IDisposable
{
    public HWND Handle { get; private set; }

    private readonly TaskCompletionSource _handleCreatedTask = new();
    private readonly Thread _windowThread;
    private static unsafe delegate* unmanaged<HWND, uint, WPARAM, LPARAM, LRESULT> _wndProcDelegate;

    private readonly GCHandle _thisHandle;

    private readonly MouseHookHandler? _mouseHandler;
    private readonly KeyboardHookHandler? _keyboardHandler;

    private readonly HookType _type;

    private HHOOK _hook;

    public static IDisposable CreateMouseHook(MouseHookHandler handler)
    {
        return new LLHook(handler);
    }
    public static IDisposable CreateKeyboardHook(KeyboardHookHandler handler)
    {
        return new LLHook(handler);
    }

    private LLHook(KeyboardHookHandler handler)
    {
        _keyboardHandler = handler;
        _type = HookType.Keyboard;
        _wndProcDelegate = &StaticWndProc;
        _thisHandle = GCHandle.Alloc(this);

        _windowThread = new Thread(WindowThreadStart)
        {
            Name = "LLKeyboardThread"
        };

        _windowThread.SetApartmentState(ApartmentState.STA);
        _windowThread.Start();

        _handleCreatedTask.Task.Wait();
    }

    private LLHook(MouseHookHandler handler)
    {
        _mouseHandler = handler;
        _type = HookType.Mouse;
        _wndProcDelegate = &StaticWndProc;
        _thisHandle = GCHandle.Alloc(this);

        _windowThread = new Thread(WindowThreadStart)
        {
            Name = "LLMouseThread"
        };

        _windowThread.SetApartmentState(ApartmentState.STA);
        _windowThread.Start();

        _handleCreatedTask.Task.Wait();
    }

    private void WindowThreadStart()
    {
        try
        {
            CreateHandle();
            _handleCreatedTask.TrySetResult();
        }
        catch (Exception ex)
        {
            _handleCreatedTask.TrySetException(ex);
            return;
        }

        MessageLoop();
    }

    private void MessageLoop()
    {
        MSG msg;

        try
        {
            int bRet;

            while ((bRet = GetMessage(&msg, HWND.NULL, 0, 0).Value) != 0)
            {
                if (bRet == -1)
                {
                    Console.WriteLine("GetMessage returned -1");
                    break;
                }

                TranslateMessage(&msg);
                DispatchMessage(&msg);
            }
        }
        finally
        {
            _thisHandle.Free();
        }
    }

    private void CreateHandle()
    {
        fixed (char* windowClassName = "LLHook_" + new Random().Next())
        {
            WNDCLASSEXW windowClass = new()
            {
                cbSize = (uint)sizeof(WNDCLASSEXW),
                lpfnWndProc = _wndProcDelegate,
                hInstance = GetModuleHandleW(null),
                lpszClassName = windowClassName
            };

            if (RegisterClassExW(&windowClass) == 0)
            {
                throw new Win32Exception();
            }

            fixed (char* _name = "LLHook_" + new Random().Next())
            {
                // Create the window
                HWND hwnd = CreateWindowExW(
                    0,
                    windowClassName,
                    _name,
                    WS.WS_OVERLAPPEDWINDOW,
                    CW_USEDEFAULT,
                    CW_USEDEFAULT,
                    0,
                    0,
                    HWND.HWND_MESSAGE,
                    default,
                    windowClass.hInstance,
                    null
                );

                if (hwnd.Value is null)
                {
                    throw new Win32Exception();
                }

                Handle = hwnd;

                SetWindowLongPtr(hwnd, GWLP.GWLP_USERDATA, GCHandle.ToIntPtr(_thisHandle));
                SetMessageExtraInfo(GCHandle.ToIntPtr(_thisHandle));

                int type = _type == HookType.Mouse ? WH.WH_MOUSE_LL : WH.WH_KEYBOARD_LL;
                _hook = SetWindowsHookExW(type, &StaticHookCallback, HINSTANCE.NULL, 0);

                if (_hook.Value is null)
                    throw new Win32Exception();
            }
        }
    }

    private LRESULT WndProc(HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam)
    {
        switch (uMsg)
        {
            case WM.WM_CLOSE:
                if (_hook.Value is not null)
                    UnhookWindowsHookEx(_hook);

                PostQuitMessage(0);
                return 0;
        }

        return DefWindowProcW(hWnd, uMsg, wParam, lParam);
    }

    [UnmanagedCallersOnly]
    private static unsafe LRESULT StaticWndProc(HWND hwnd, uint uMsg, WPARAM wParam, LPARAM lParam)
    {
        nint userData = GetWindowLongPtrW(hwnd, GWLP.GWLP_USERDATA);

        if (userData != 0)
        {
            GCHandle handle = GCHandle.FromIntPtr(userData);
            LLHook instance = (LLHook)handle.Target!;
            return instance.WndProc(hwnd, uMsg, wParam, lParam);
        }
        else
        {
            return DefWindowProcW(hwnd, uMsg, wParam, lParam);
        }
    }

    [UnmanagedCallersOnly]
    private static LRESULT StaticHookCallback(int nCode, WPARAM wParam, LPARAM lParam)
    {
        try
        {
            GCHandle handle = GCHandle.FromIntPtr(GetMessageExtraInfo());
            LLHook instance = (LLHook)handle.Target!;

            HookCallbackResult result = HookCallbackResult.CallNextHook;

            if (instance._type == HookType.Mouse)
            {
                MSLLHOOKSTRUCT mouseData = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);

                if (instance._mouseHandler is not null)
                    result = instance._mouseHandler(new MouseHookEvent(wParam, mouseData));

            }
            else if (instance._type == HookType.Keyboard)
            {
                KBDLLHOOKSTRUCT kbData = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);

                if (instance._keyboardHandler is not null)
                    result = instance._keyboardHandler(new KeyboardHookEvent(wParam, kbData));
            }

            if (result == HookCallbackResult.Block)
                return -1;
            else if (result == HookCallbackResult.CallNextHook)
                return CallNextHookEx(instance._hook, nCode, wParam, lParam);
            else
                return 0;
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Hook callback exception: \n{ex.ToString()}");
            return 0;
        }
    }

    public void Dispose()
    {
        if (Handle != nint.Zero)
        {
            PostMessage(Handle, WM.WM_CLOSE, 0, 0);
            _windowThread.Join();
            Handle = HWND.NULL;
        }
    }
}