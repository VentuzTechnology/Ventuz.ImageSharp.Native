/*
 * Ventuz.ImageSharp.Native
 * Copyright (c) 2024 Ventuz Technology <https://ventuz.com>
 *
 * This file is part of Ventuz.ImageSharp.Native
 *
 * libheif is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as
 * published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version.
 *
 * libheif is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with Ventuz.ImageSharp.Native.  If not, see <http://www.gnu.org/licenses/>.
 */

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

    public static IEnumerable<IImageFormat> SupportedFormats => [Avif.Instance, OpenEXR.Instance, Heic.Instance];

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
        public static readonly Heic Instance = new();
        public string Name => "HEIC";
        public string DefaultMimeType => "image/heic";
        public IEnumerable<string> MimeTypes => [DefaultMimeType];
        public IEnumerable<string> FileExtensions => ["heic", "heif"];
    }

}


