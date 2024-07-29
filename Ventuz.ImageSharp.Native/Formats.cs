using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

namespace Ventuz.ImageSharp.Native;

public static class Formats
{
    public static void Register(Configuration? config = null)
    {
        config ??= Configuration.Default;
        config.ImageFormatsManager.AddImageFormatDetector(new NativeFormatDetector());

        config.ImageFormatsManager.SetDecoder(Avif.Instance, new NativeDecoder(NativeImageFormat.Avif));
        config.ImageFormatsManager.SetDecoder(OpenEXR.Instance, new NativeDecoder(NativeImageFormat.OpenEXR));
        config.ImageFormatsManager.SetDecoder(Heic.Instance, new NativeDecoder(NativeImageFormat.Heic));
    }

    public sealed class Avif : IImageFormat
    {
        public static readonly Avif Instance = new();
        public string Name => "AVIF";
        public string DefaultMimeType => "image/avif";
        public IEnumerable<string> MimeTypes => [DefaultMimeType];
        public IEnumerable<string> FileExtensions => ["avif"];
    }

    public sealed class OpenEXR : IImageFormat
    {
        public static readonly OpenEXR Instance = new();
        public string Name => "OpenEXR";
        public string DefaultMimeType => "image/x-exr";
        public IEnumerable<string> MimeTypes => [DefaultMimeType];
        public IEnumerable<string> FileExtensions => ["exr"];
    }

    public sealed class Heic : IImageFormat
    {
        public static readonly Avif Instance = new();
        public string Name => "HEIC";
        public string DefaultMimeType => "image/heic";
        public IEnumerable<string> MimeTypes => [DefaultMimeType];
        public IEnumerable<string> FileExtensions => ["heic", "heif"];
    }

}


