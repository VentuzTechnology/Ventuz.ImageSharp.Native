using System.Runtime.InteropServices;

namespace Ventuz.ImageSharp.Native;

internal enum ErrorCode : uint
{
    Ok = 0,
    InvalidParameter,
    BadFormat,
    InternalError,
    ImageTooLarge,
    Unknown,
}

internal enum NativeImageFormat : uint
{
    Avif,
    OpenEXR,
    Heic,
}

internal enum NativePixelFormat : uint
{
    RGBA_UN8,
    RGBA_UN16,
    RGBA_F16,
    R_F16,
}

internal enum AlphaMode : uint
{
    Unknown,
    Straight,
    Premultiplied,
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct NativeImageInfo
{
    // general
    public uint sizeX;
    public uint sizeY;
    public NativePixelFormat format;
    public AlphaMode alpha;

    // CICP
    public int colorPrimaries;
    public int transferCharacteristics;

    // chromaticities
    public float cRx, cRy, cGx, cGy, cBx, cBy, cWx, cWy;

    // metadata (valid as long as the decoder is alive)
    public void* exifData;
    public int exifSize;
    public void* xmpData;
    public int xmpSize;
    public void* iccData;
    public int iccSize;
}

internal readonly struct DecoderHandle
{
    public DecoderHandle() { }
#pragma warning disable IDE0052 // Remove unread private members
    readonly nint handle = nint.Zero;
#pragma warning restore IDE0052 // Remove unread private members
}

internal delegate void LogDelegate(Logging.Level level, [MarshalAs(UnmanagedType.LPUTF8Str)] string log);

internal unsafe delegate int ReadDelegate(void* ptr, int size);

internal delegate long SeekDelegate(long pos, SeekOrigin origin);

internal static partial class NativeMethods
{
    const string DLLNAME = "Ventuz.Native.ImageFormats.dll";

    [LibraryImport(DLLNAME)]
    public static partial void SetLogger(LogDelegate log);

    [LibraryImport(DLLNAME)]
    public static partial ErrorCode OpenDecoder(NativeImageFormat fmt, ReadDelegate read, SeekDelegate seek, out DecoderHandle decoder);

    [LibraryImport(DLLNAME)]
    public static partial void CloseDecoder(ref DecoderHandle decoder);

    [LibraryImport(DLLNAME)]
    public static partial ErrorCode GetImageInfo(DecoderHandle decoder, out NativeImageInfo info);

    [LibraryImport(DLLNAME)]
    public static unsafe partial ErrorCode GetImageData(DecoderHandle decoder, void* memory);
}