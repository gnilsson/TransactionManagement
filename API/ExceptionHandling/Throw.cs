using System.Diagnostics.CodeAnalysis;

namespace API.ExceptionHandling;

public static class Throw<TException> where TException : Exception, new()
{
    [DoesNotReturn]
    public static void Empty() => throw new TException();
    [DoesNotReturn]
    public static void WithMessage(string message) => throw (TException)Activator.CreateInstance(typeof(TException), message)!;
    public static void If([DoesNotReturnIf(true)] bool condition, string? message = null)
    {
        if (condition)
        {
            if (message is null)
            {
                Empty();
            }

            WithMessage(message);
        }
    }
}
