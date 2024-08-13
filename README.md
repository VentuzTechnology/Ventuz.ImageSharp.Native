# Ventuz.ImageSharp.Native

A companion library for https://github.com/SixLabors/ImageSharp, adding read support for various image formats that only have native implementations so far.

### Supported formats
- AVIF (via libavif/dav1d)
- HEIC (via libheif/libde265)
- OpenEXR

All of these come with support for EXIF/XMP/ICC metadata and CICP color spaces.

### Limitations

- No write support
- The only data that gets read is the "primary image" as defined by the formats. Any additional images (or sequence) or additional color channels will be ignored.
- The library currently only compiles on Windows

(All of these are not technical limitations but simply because they're currently outside of the scope of this library)

### Copyright

Ventuz.ImageSharp.Native is (C) Ventuz Technology 2024, released under the GNU Lesser General Public License 3.0, see LICENSE for details

Used 3rd party libraries:
* SixLabors.ImageSharp, https://github.com/SixLabors/ImageSharp, Six Labors Split License
* OpenEXR, https://openexr.com/, BSD-3-Clause license
* dav1d, https://code.videolan.org/videolan/dav1d
* libavif, https://github.com/AOMediaCodec/libavif, various licenses
* libde265, https://github.com/strukturag/libde265, GNU Lesser General Public License 3.0
* libdeflate, https://github.com/ebiggers/libdeflate, MIT license
* libheif, https://github.com/strukturag/libheif, GNU Lesser General Public License 3.0
* libjpeg-turbo, https://github.com/libjpeg-turbo/libjpeg-turbo, BSD-style split license
* libyuv, https://chromium.googlesource.com/libyuv/libyuv/, BSD-style license

### Building

Prerequisites:
* Visual Studio 2022 with C# and C++ Desktop workloads installed, and .NET 8 or higher 
* vcpkg (https://vcpkg.io/en/) with Visual Studio integration active

### Usage

There's an example project that should be simple enough, but here's the easy version: Add Ventuz.ImageSharp.Native to your .NET project, make sure the native Ventuz.Native.ImageFormats.dll is deployed to your application folder, and call ```Ventuz.ImageSharp.Native.Formats.Register();``` before trying to load any images.



