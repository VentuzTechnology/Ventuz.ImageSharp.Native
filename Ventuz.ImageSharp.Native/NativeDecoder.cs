using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Metadata.Profiles.Xmp;
using SixLabors.ImageSharp.Metadata.Profiles.Icc;

using Ventuz.ImageSharp.Native.PixelFormats;

namespace Ventuz.ImageSharp.Native;

internal sealed class NativeDecoder(NativeImageFormat fmt) : IImageDecoder
{
    public Image<TPixel> Decode<TPixel>(DecoderOptions options, Stream stream) where TPixel : unmanaged, IPixel<TPixel>
        => DecodeInternal<TPixel>(options, stream, CancellationToken.None);

    public Image Decode(DecoderOptions options, Stream stream)
        => DecodeInternal(options, stream, CancellationToken.None);

    public Task<Image<TPixel>> DecodeAsync<TPixel>(DecoderOptions options, Stream stream, CancellationToken cancellationToken = default) where TPixel : unmanaged, IPixel<TPixel>
        => Task.Run(() => DecodeInternal<TPixel>(options, stream, cancellationToken));

    public Task<Image> DecodeAsync(DecoderOptions options, Stream stream, CancellationToken cancellationToken = default)
        => Task.Run(() => DecodeInternal(options, stream, cancellationToken));

    public ImageInfo Identify(DecoderOptions options, Stream stream)
        => IdentifyInternal(options, stream, CancellationToken.None);

    public Task<ImageInfo> IdentifyAsync(DecoderOptions options, Stream stream, CancellationToken cancellationToken = default)
        => Task.Run(() => IdentifyInternal(options, stream, cancellationToken));

    // we need a per-decode cache for the delegates so they don't get GCed while decoding
    unsafe class Instance
    {
#pragma warning disable IDE0060 // Remove unused parameter
        public ImageInfo Identify(DecoderOptions options, Stream stream, NativeImageFormat format, CancellationToken cancellationToken)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            try
            {
                OpenAndGetInfo(stream, format);

                ImageMetadata meta = new();
                if ( !options.SkipMetadata )
                    FillMetadata(meta);

                return new ImageInfo(GetPixelTypeInfo(info), new Size((int)info.sizeX, (int)info.sizeY), meta);
            }
            finally
            {
                Close();
            }
        }

#pragma warning disable IDE0060 // Remove unused parameter
        public Image Decode(DecoderOptions options, Stream stream, NativeImageFormat format, CancellationToken cancellationToken)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            var config = options.Configuration.Clone();
            config.PreferContiguousImageBuffers = true;

            Image? image = null;
            System.Buffers.MemoryHandle pixels = new();

            try
            {
                OpenAndGetInfo(stream, format);

                void CreateAndPin<TPixel>() where TPixel : unmanaged, IPixel<TPixel>
                {
                    var img = new Image<TPixel>(config, (int)info.sizeX, (int)info.sizeY);
                    image = img;

                    if ( !img.DangerousTryGetSinglePixelMemory(out var mem) )
                        ThrowOnError(ErrorCode.ImageTooLarge);

                    pixels = mem.Pin();
                }

                switch ( info.format )
                {
                    case NativePixelFormat.RGBA_UN8: CreateAndPin<Rgba32>(); break;
                    case NativePixelFormat.RGBA_UN16: CreateAndPin<Rgba64>(); break;
                    case NativePixelFormat.RGBA_F16: CreateAndPin<RgbaHalf>(); break;
                    case NativePixelFormat.R_F16: CreateAndPin<RHalf>(); break;
                    default: throw new NotImplementedException();
                }

                if ( !options.SkipMetadata )
                    FillMetadata(image!.Metadata);

                var err = NativeMethods.GetImageData(decoder, pixels.Pointer);
                ThrowOnError(err);
            }
            catch
            {
                image?.Dispose();
                throw;
            }
            finally
            {
                pixels.Dispose();
                Close();
            }

            return image!;
        }

        SeekDelegate? seekDelegate;
        ReadDelegate? readDelegate;
        DecoderHandle decoder;
        NativeImageInfo info;

        void OpenAndGetInfo(Stream stream, NativeImageFormat format)
        {
            readDelegate = (ptr, size) => stream.Read(new Span<byte>(ptr, size));
            seekDelegate = stream.Seek;
            var err = NativeMethods.OpenDecoder(format, readDelegate, seekDelegate, out decoder);
            ThrowOnError(err);

            err = NativeMethods.GetImageInfo(decoder, out info);
            ThrowOnError(err);
        }

        void Close()
        {
            NativeMethods.CloseDecoder(ref decoder);
            readDelegate = null;
            seekDelegate = null;
        }

        void FillMetadata(ImageMetadata meta)
        {
            if ( info.exifSize > 0 )
                meta.ExifProfile = new ExifProfile(new Span<byte>(info.exifData, info.exifSize).ToArray());

            if ( info.xmpSize > 0 )
                meta.XmpProfile = new XmpProfile(new Span<byte>(info.xmpData, info.xmpSize).ToArray());

            if ( info.iccSize > 0 )
                meta.IccProfile = new IccProfile(new Span<byte>(info.iccData, info.iccSize).ToArray());

            if ( info.colorPrimaries != 2 || info.transferCharacteristics != 2 )
                meta.CicpProfile = new SixLabors.ImageSharp.Metadata.Profiles.Cicp.CicpProfile((byte)info.colorPrimaries, (byte)info.transferCharacteristics, 0, true);

            if ( info.cRx > 0 || info.cRy > 0 )
            {
                meta.ExifProfile ??= new ExifProfile();
                meta.ExifProfile.SetValue(ExifTag.WhitePoint, [new(info.cWx), new(info.cWy)]);
                meta.ExifProfile.SetValue(ExifTag.PrimaryChromaticities, [new(info.cRx), new(info.cRy), new(info.cGx), new(info.cGy), new(info.cBx), new(info.cBy)]);
            }
        }

        static PixelTypeInfo GetPixelTypeInfo(in NativeImageInfo info)
        {
            PixelAlphaRepresentation alpha = info.alpha switch
            {
                AlphaMode.Straight => PixelAlphaRepresentation.Unassociated,
                AlphaMode.Premultiplied => PixelAlphaRepresentation.Associated,
                _ => PixelAlphaRepresentation.None,
            };

            return info.format switch
            {
                NativePixelFormat.RGBA_UN8 => new(32, alpha),
                NativePixelFormat.RGBA_UN16 => new(64, alpha),
                NativePixelFormat.RGBA_F16 => new(64, alpha),
                NativePixelFormat.R_F16 => new(16, alpha),
                _ => throw new NotImplementedException(),
            };
        }
    }

    readonly NativeImageFormat format = fmt;

    static void ThrowOnError(ErrorCode error)
    {
        if ( error != ErrorCode.Ok ) throw new Exception(error.ToString());
    }

    ImageInfo IdentifyInternal(DecoderOptions options, Stream stream, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(stream);

        NativeMethods.SetLogger(Logging.Log);

        return new Instance().Identify(options, stream, format, cancellationToken);
    }

    Image DecodeInternal(DecoderOptions options, Stream stream, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(stream);

        NativeMethods.SetLogger(Logging.Log);

        return new Instance().Decode(options, stream, format, cancellationToken);
    }

    Image<TPixel> DecodeInternal<TPixel>(DecoderOptions options, Stream stream, CancellationToken cancellationToken) where TPixel : unmanaged, IPixel<TPixel>
    {
        Image image = DecodeInternal(options, stream, cancellationToken);

        if ( image is not Image<TPixel> outImage )
        {
            outImage = image.CloneAs<TPixel>();
            image.Dispose();
        }

        return outImage;
    }
}
