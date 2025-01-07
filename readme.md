# LLHookLib
## Simple & efficient Windows low level keyboard & mouse hook library
### Hooks are created on a dedicated thread, so this library will work on console apps & UI apps.

## Usage
#### Creating a mouse hook
```
IDisposable mouseHook = LLHook.CreateMouseHook((MouseHookEvent @event) =>
{
    Console.WriteLine($"Got mouse event {@event.Type}");

    if(@event.Type == Mouse.MouseEventType.Moved)
        Console.WriteLine($"Mouse moved to {@event.MoveEvent.X}:{@event.MoveEvent.Y}" +
            $" (Relative {@event.MoveEvent.RelativeX}:{@event.MoveEvent.RelativeY}");

    if (@event.Type == Mouse.MouseEventType.Button)
        Console.WriteLine($"Mouse button {@event.ButtonEvent.Button} state {@event.ButtonEvent.ButtonState}");

    if (@event.Type != Mouse.MouseEventType.Scroll)
        Console.WriteLine($"Mouse scrolled {@event.ScrollEvent.Direction}");

    return HookCallbackResult.CallNextHook;
});
```

#### Creating a Keyboard hook
```
IDisposable keyboardHook = LLHook.CreateKeyboardHook((KeyboardHookEvent @event) =>
{
    Console.WriteLine($"Key {@event.VKey} {@event.KeyState}");

    return HookCallbackResult.CallNextHook;
});
```

## Notes
- Returning HookCallbackResult.Block will prevent windows from processing the event.
