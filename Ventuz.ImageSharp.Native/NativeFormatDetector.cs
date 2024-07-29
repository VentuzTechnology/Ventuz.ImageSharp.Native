using SixLabors.ImageSharp.Formats;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Ventuz.ImageSharp.Native;

using static Helpers;

public sealed class NativeFormatDetector : IImageFormatDetector
{
    public int HeaderSize => 64;

    public bool TryDetectFormat(ReadOnlySpan<byte> header, [NotNullWhen(true)] out IImageFormat? format)
    {
        if ( DetectHEIF(header, [FourCC('a', 'v', 'i', 'f')]) )
        {
            format = Formats.Avif.Instance;
            return true;
        }

        if ( DetectOpenEXR(header) )
        {
            format = Formats.OpenEXR.Instance;
            return true;
        }

        if ( DetectHEIF(header,
            [FourCC('h', 'e', 'i', 'c'),
             FourCC('h', 'e', 'i', 'x'),
             FourCC('h', 'e', 'v', 'c'),
             FourCC('h', 'e', 'v', 'x'),
             FourCC('m', 'i', 'H', 'E')]) )
        {
            format = Formats.Heic.Instance;
            return true;
        }

        format = null;
        return false;
    }


    static bool DetectHEIF(ReadOnlySpan<byte> header, IEnumerable<uint> brands)
    {
        if ( header.Length < 16 )
            return false;

        int boxsize = (int)SwapEndian(MemoryMarshal.Read<uint>(header[0..4]));
        if ( boxsize < 16 )
            return false;

        boxsize = Math.Min(boxsize, header.Length);
        var box = header[4..boxsize];

        uint boxHdr = MemoryMarshal.Read<uint>(box[0..4]);
        if ( boxHdr != FourCC('f', 't', 'y', 'p') )
            return false;

        uint majorBrand = MemoryMarshal.Read<uint>(box[4..]);
        if ( brands.Contains(majorBrand) )
            return true;

        for ( int i = 12; i <= box.Length - 4; i += 4 )
        {
            uint compatBrand = MemoryMarshal.Read<uint>(box[i..]);
            if ( brands.Contains(compatBrand) )
                return true;
        }

        return false;
    }

    static bool DetectOpenEXR(ReadOnlySpan<byte> header)
    {
        if ( header.Length < 16 )
            return false;

        uint magic = MemoryMarshal.Read<uint>(header[0..4]);
        uint versionAndFlags = MemoryMarshal.Read<uint>(header[4..8]);

        return magic == 20000630u && (versionAndFlags & 0xff) == 2;
    }


}

