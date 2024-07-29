
namespace Ventuz.ImageSharp.Native;

public static class Logging
{
    public enum Level
    {
        Debug,
        Info,
        Warning,
        Error,
    }

    public static Action<Level, string>? LogCallback { get; set; }

    internal static void Log(Level l, string s) => LogCallback?.Invoke(l, s);
}
