using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace API.ExceptionHandling;

public static class ThrowHelper
{
    public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument is null)
        {
            ThrowArgumentNullException(paramName);
        }

        [DoesNotReturn]
        static void ThrowArgumentNullException(string? paramName) => throw new ArgumentNullException(paramName);
    }

    public static void Throw(string? message = null)
    {
        if (message is null)
        {
            throw new InvalidOperationException();
        }
        throw new InvalidOperationException(message);
    }
}
