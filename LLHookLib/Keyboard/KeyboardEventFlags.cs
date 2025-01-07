namespace LLHookLib.Keyboard;

[Flags]
public enum KeyboardEventFlags
{
    Extended = 0x0100 >> 8,
    Injected = 0x00000010,
    InjectedFromLowerIL = 0x00000002,
    AltDown = 0x2000 >> 8,
    KeyUp = 0x8000 >> 8,
}
