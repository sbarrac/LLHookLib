namespace LLHookLib;

public enum HookCallbackResult
{
    /// <summary>
    /// Calls the next hook in the windows hook chain (if any)
    /// </summary>
    CallNextHook = 0,
    /// <summary>
    /// Allows windows to process the input, but does not forward the input to
    /// any other low level hooks in the chain
    /// </summary>
    None = 1,
    /// <summary>
    /// Prevents windows from processing the input, essentially swallowing the input.
    /// </summary>
    Block = 2
}
