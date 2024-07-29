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
