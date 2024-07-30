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

#include "api.h"
#include "decoder.h"

// forward declarations for decoders
IDecoder *CreateAvifDecoder();
IDecoder *CreateOpenExrDecoder();
IDecoder *CreateHeicDecoder();

static void DummyLogger(LogLevel level, const char *str) {};
static LogDelegate logger = nullptr;

void SetLogger(LogDelegate log)
{
    logger = log;
}

ErrorCode OpenDecoder(NativeFormat format, ReadDelegate read, SeekDelegate seek, DecoderHandle &handle)
{
    handle = 0;
    if (!read || !seek)
        return ErrorCode::InvalidParameter;

    IDecoder *decoder;
    switch (format)
    {
    case NativeFormat::Avif: decoder = CreateAvifDecoder(); break;
    case NativeFormat::OpenEXR: decoder = CreateOpenExrDecoder(); break;
    case NativeFormat::Heic: decoder = CreateHeicDecoder(); break;
    default: return ErrorCode::InvalidParameter;
    }

    decoder->Read = read;
    decoder->Seek = seek;
    decoder->Log = logger ? logger : DummyLogger;
    if (!decoder->Init())
    {
        delete decoder;
        return ErrorCode::BadFormat;
    }

    handle = decoder;
    return ErrorCode::Ok;
}


void CloseDecoder(DecoderHandle &handle)
{
    delete (IDecoder *)handle;
    handle = nullptr;
}


ErrorCode GetImageInfo(DecoderHandle handle, NativeImageInfo &info)
{
    info = {};
    return ((IDecoder *)handle)->GetImageInfo(info);
}


ErrorCode GetImageData(DecoderHandle handle, void *mem)
{
    return ((IDecoder *)handle)->GetImageData(mem);
}