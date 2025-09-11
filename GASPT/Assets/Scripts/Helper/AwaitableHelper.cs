// ÇïÆÛ Å¬·¡½º
using AbilitySystem;
using UnityEngine;

public static class AwaitableHelper
{
    private static readonly AwaitableCompletionSource _completed = new();

    static AwaitableHelper()
    {
        _completed.SetResult();
    }

    public static Awaitable CompletedTask => _completed.Awaitable;
}