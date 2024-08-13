/*
 * Ventuz.ImageSharp.Native
 * Copyright (c) 2024 Ventuz Technology <https://ventuz.com>
 *
 * This file is part of Ventuz.ImageSharp.Native
 *
 * Ventuz.ImageSharp.Native is free software: you can redistribute
 * it and/or modify it under the terms of the GNU Lesser General
 * Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later
 * version.
 *
 * Ventuz.ImageSharp.Native is distributed in the hope that it will
 * be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with Ventuz.ImageSharp.Native.  If not, see <http://www.gnu.org/licenses/>.
 */

#include "decoder.h"
#include "avif/avif.h"

#define BUFFER_ALL 0

class AvifDecoder: public IDecoder
{
public:

    AvifDecoder() { }

    ~AvifDecoder()
    {
        if (decoder)
            avifDecoderDestroy(decoder);
    }

    bool Init() override
    {
        decoder = avifDecoderCreate();
        if (!decoder)
            return false;

        avifDecoderSetIO(decoder, new IO(this));

        auto res = avifDecoderParse(decoder);
        if (res != AVIF_RESULT_OK)
        {
            if (decoder->diag.error) Log(LogLevel::Error, decoder->diag.error);
            return false;
        }

        res = avifDecoderNextImage(decoder);
        if (res != AVIF_RESULT_OK)
        {
            if (decoder->diag.error) Log(LogLevel::Error, decoder->diag.error);
            return false;
        }

        avifRGBImageSetDefaults(&rgbImage, decoder->image);
        //rgbImage.isFloat = rgbImage.depth == 10 ? 1 : 0;
        rgbImage.depth = rgbImage.depth > 8 ? 16 : 8;
        rgbImage.alphaPremultiplied = decoder->image->alphaPremultiplied;

        return true;
    }

    ErrorCode GetImageInfo(NativeImageInfo &info) override
    {
        if (!decoder || !decoder->image)
            return ErrorCode::BadFormat;

        if (decoder->image->width > 16384 || decoder->image->height > 16384)
            return ErrorCode::ImageTooLarge;

        info.sizeX = rgbImage.width;
        info.sizeY = rgbImage.height;
        info.format = rgbImage.depth > 8 ? (rgbImage.isFloat ? NativePixelFormat::RGBA_F16 : NativePixelFormat::RGBA_UN16) : NativePixelFormat::RGBA_UN8;
        info.alpha = decoder->image->alphaPlane ?
            (decoder->image->alphaPremultiplied ? AlphaMode::Premultiplied : AlphaMode::Straight) :
            AlphaMode::Unknown;
        info.colorPrimaries = decoder->image->colorPrimaries;
        info.transferCharacteristics = decoder->image->transferCharacteristics;

        info.exifData = decoder->image->exif.data;
        info.exifSize = (int)decoder->image->exif.size;
        info.xmpData = decoder->image->xmp.data;
        info.xmpSize = (int)decoder->image->xmp.size;
        info.iccData = decoder->image->icc.data;
        info.iccSize = (int)decoder->image->icc.size;

        return ErrorCode::Ok;
    }

    ErrorCode GetImageData(void *memory) override
    {
        if (!memory)
            return ErrorCode::InvalidParameter;

        rgbImage.pixels = (uint8_t *)memory;
        rgbImage.rowBytes = rgbImage.width * (rgbImage.depth > 8 ? 8 : 4);

        auto res = avifImageYUVToRGB(decoder->image, &rgbImage);
        if (res != AVIF_RESULT_OK)
        {
            if (decoder->diag.error) Log(LogLevel::Error, decoder->diag.error);
            return ErrorCode::InternalError;
        }

        return ErrorCode::Ok;
    }

private:

    class IO: public avifIO
    {
    public:
        IO(AvifDecoder *dec): decoder(dec)
        {
            destroy = Destroy;
            read = ReadProxy;
            write = nullptr;
            persistent = AVIF_FALSE;
            data = nullptr;

            fsize = (uint64_t)decoder->Seek(0, SeekOrigin::End);
            fpos = fsize;
            sizeHint = fsize;

#if BUFFER_ALL
            buffer = new uint8_t[fsize];
            decoder->Seek(0, SeekOrigin::Begin);
            decoder->Read(buffer, fsize);
            persistent = AVIF_TRUE;
#endif
        }

        ~IO()
        {
            delete[] buffer;
        }

    private:

        AvifDecoder *decoder;
        uint64_t fpos;
        uint64_t fsize;

        uint8_t *buffer = nullptr;
        size_t bsize = 0;

        avifResult Read(uint32_t readFlags, uint64_t offset, size_t size, avifROData *out)
        {
            if (readFlags != 0) {
                // Unsupported readFlags
                return AVIF_RESULT_IO_ERROR;
            }

            if (size > fsize - offset)
                size = fsize - offset;

#if BUFFER_ALL
            out->data = buffer + offset;
            out->size = size;
#else
            if (offset != fpos)
                decoder->Seek((int64_t)offset, SeekOrigin::Begin);

            if (size > bsize)
            {
                delete[] buffer;
                bsize = size * 2;
                if (bsize < 65536) bsize = 65536;
                buffer = new uint8_t[bsize];
            }

            out->data = buffer;
            out->size = (size_t)decoder->Read(buffer, (int)size);
            fpos += offset + out->size;
#endif
            return AVIF_RESULT_OK;
        }

        static avifResult ReadProxy(struct avifIO *io, uint32_t readFlags, uint64_t offset, size_t size, avifROData *out)
        {
            return ((IO *)io)->Read(readFlags, offset, size, out);
        }

        static void Destroy(struct avifIO *io)
        {
            delete (IO *)io;
        }
    };

    avifDecoder *decoder = nullptr;
    avifRGBImage rgbImage = {};
};

IDecoder *CreateAvifDecoder() { return new AvifDecoder(); }
