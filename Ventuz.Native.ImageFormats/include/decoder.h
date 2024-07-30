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

#pragma once

#include "api.h"

#define EXPORT __declspec(dllexport)

struct IDecoder
{
    virtual ~IDecoder() {};

    virtual bool Init() = 0;
    virtual ErrorCode GetImageInfo(NativeImageInfo &info) = 0;
    virtual ErrorCode GetImageData(void *memory) = 0;

    ReadDelegate Read;
    SeekDelegate Seek;
    LogDelegate Log;
};

inline uint32_t SwapEndian(uint32_t x) {
    return ((x & 0xff000000) >> 24) | ((x & 0x00ff0000) >> 8) | ((x & 0x0000ff00) << 8) | ((x & 0x000000ff) << 24);
}
