
namespace Ventuz.ImageSharp.Native;

public static class Helpers
{
    internal static uint SwapEndian(uint x) => ((x & 0xff000000) >> 24) | ((x & 0x00ff0000) >> 8) | ((x & 0x0000ff00) << 8) | ((x & 0x000000ff) << 24);

    internal static uint FourCC(char a, char b, char c, char d) => ((uint)a) | (((uint)b) << 8) | (((uint)c) << 16) | (((uint)d) << 24);
}
