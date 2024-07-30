/*
 * Ventuz.Native.ImageFormats
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

#include <string.h>

#include "decoder.h"
#include "libheif/heif.h"

class HeicDecoder: public IDecoder
{
public:

    HeicDecoder() { }

    ~HeicDecoder()
    {
        if (image)
            heif_image_handle_release(image);
        if (context)
            heif_context_free(context);
        delete reader;
        delete exif;
        delete icc;
        delete xmp;
    }

    bool Init() override
    {
        context = heif_context_alloc();
        reader = new Reader(this);

        heif_context_set_maximum_image_size_limit(context, 16384);

        heif_error err = heif_context_read_from_reader(context, reader, reader, nullptr);
        if (IsError(err))
            return false;

        err = heif_context_get_primary_image_handle(context, &image);
        if (IsError(err))
            return false;

        width = heif_image_handle_get_ispe_width(image);
        height = heif_image_handle_get_ispe_height(image);
        hasAlpha = !!heif_image_handle_has_alpha_channel(image);
        bpp = heif_image_handle_get_luma_bits_per_pixel(image);
        return true;
    }

    ErrorCode GetImageInfo(NativeImageInfo &info) override
    {
        bool isPremul = !!heif_image_handle_is_premultiplied_alpha(image);

        info.sizeX = width;
        info.sizeY = height;
        info.format = bpp > 8 ? NativePixelFormat::RGBA_UN16 : NativePixelFormat::RGBA_UN8;
        info.alpha = hasAlpha ? (isPremul ? AlphaMode::Premultiplied : AlphaMode::Straight) : AlphaMode::Unknown;

        heif_color_profile_nclx *nclx{};
        if (heif_image_handle_get_nclx_color_profile(image, &nclx).code == heif_error_Ok)
        {
            info.colorPrimaries = nclx->color_primaries;
            info.transferCharacteristics = nclx->transfer_characteristics;
        }

        if (heif_image_handle_get_color_profile_type(image) == heif_color_profile_type_prof)
        {
            int iccSize = (int)heif_image_handle_get_raw_color_profile_size(image);
            icc = new uint8_t[iccSize];
            heif_image_handle_get_raw_color_profile(image, icc);
            info.iccData = icc;
            info.iccSize = iccSize;
        }

        int nMeta = heif_image_handle_get_number_of_metadata_blocks(image, nullptr);
        heif_item_id *metaIds = new heif_item_id[nMeta];
        heif_image_handle_get_list_of_metadata_block_IDs(image, nullptr, metaIds, nMeta);

        for (int i = 0; i < nMeta; i++)
        {
            heif_item_id id = metaIds[i];
            const char *type = heif_image_handle_get_metadata_type(image, id);
            if (!strcmp(type, "Exif"))
            {
                size_t exifSize = heif_image_handle_get_metadata_size(image, id);
                exif = new uint8_t[exifSize];
                heif_image_handle_get_metadata(image, id, exif);
                uint32_t offs = SwapEndian(*(uint32_t *)exif) + 4;
                if (offs < exifSize)
                {
                    info.exifData = exif + offs;
                    info.exifSize = (int)(exifSize - offs);
                }
            }
            
            type = heif_image_handle_get_metadata_content_type(image, id);
            if (!strcmp(type, "application/rdf+xml"))
            {
                size_t xmpSize = heif_image_handle_get_metadata_size(image, id);
                xmp = new uint8_t[xmpSize];
                heif_image_handle_get_metadata(image, id, xmp);
                info.xmpData = xmp + 4;
                info.xmpSize = (int)xmpSize;
            }
        }

        delete[] metaIds;
        
        return ErrorCode::Ok;
    }

    ErrorCode GetImageData(void *memory) override
    {
        heif_image *outImg{};
        heif_chroma chroma = bpp > 8 ? heif_chroma_interleaved_RRGGBBAA_LE : heif_chroma_interleaved_RGBA;
        auto options = heif_decoding_options_alloc();
        options->ignore_transformations = 1;

        auto err = heif_decode_image(image, &outImg, heif_colorspace_RGB, chroma, options);
        if (IsError(err))
        {
            heif_decoding_options_free(options);
            return ErrorCode::BadFormat;
        }

        int stride = 0;
        const uint8_t *data = heif_image_get_plane_readonly(outImg, heif_channel_interleaved, &stride);

        int outStride = width * (bpp > 8 ? 8 : 4);
        if (stride != outStride)
        {
            uint8_t *dest = (uint8_t *)memory;
            for (int y = 0; y < height; y++)
            {
                memcpy(dest, data, outStride);
                data += stride;
                dest += outStride;
            }
        }
        else
            memcpy(memory, data, (size_t)stride * height);

        for (int i = 0;; i++)
        {
            heif_error warning{};
            int ret = heif_image_get_decoding_warnings(outImg, i, &warning, 1);
            if (!ret || !warning.message)
                break;
            Log(LogLevel::Warning, warning.message);
        }
       
        heif_image_release(outImg);
        heif_decoding_options_free(options);
        return ErrorCode::Ok;
    }

private:

    struct Reader: heif_reader
    {
        Reader(HeicDecoder *d): dec(d)
        {
            reader_api_version = 1;
            get_position = [](void *p) { return ((Reader *)p)->GetPos(); };
            read = [](void *data, size_t size, void *p) { return ((Reader *)p)->Read(data, size); };
            seek = [](int64_t pos, void *p) { return ((Reader *)p)->Seek(pos); };
            wait_for_file_size = [](int64_t target, void *p) { return ((Reader *)p)->Wait(target); };

            fsize = dec->Seek(0, SeekOrigin::End);
            dec->Seek(0, SeekOrigin::Begin);
        };

        int64_t GetPos() const
        {
            return dec->Seek(0, SeekOrigin::Current);
        }

        int Read(void *data, size_t size) const
        {
            size_t hasread = dec->Read(data, (int)size);
            return hasread == size ? heif_error_Ok : heif_error_Invalid_input;
        }

        int Seek(int64_t pos) const
        {
            int64_t haspos = dec->Seek(pos, SeekOrigin::Begin);
            return haspos == pos ? heif_error_Ok : heif_error_Invalid_input;
        }

        heif_reader_grow_status Wait(int64_t target_size) const
        {
            return target_size > fsize ? heif_reader_grow_status_size_beyond_eof : heif_reader_grow_status_size_reached;
        }

        int64_t fsize = 0;
        HeicDecoder *dec = nullptr;
    };

    bool IsError(const heif_error &error) const
    {
        if (error.code == heif_error_Ok)
            return false;

        Log(LogLevel::Error, error.message);
        return true;
    }

    heif_context *context = nullptr;
    Reader *reader = nullptr;
    heif_image_handle *image = nullptr;
    int width, height;
    bool hasAlpha;   
    int bpp;
    uint8_t *exif = nullptr;
    uint8_t *xmp = nullptr;
    uint8_t *icc = nullptr;
};

IDecoder *CreateHeicDecoder() { return new HeicDecoder(); }
