#pragma once
#define EXPORT __declspec(dllexport)

#include <stdint.h>

enum class ErrorCode: uint32_t
{
    Ok = 0,
    InvalidParameter,
    BadFormat,
    InternalError,
    ImageTooLarge,
    Unknown,
};

enum class NativeFormat: uint32_t
{
    Avif,
    OpenEXR,
    Heic,
};

enum class SeekOrigin
{
    Begin,
    Current,
    End,
};

enum class NativePixelFormat: uint32_t
{
    RGBA_UN8,
    RGBA_UN16,
    RGBA_F16,
    R_F16,
};

enum AlphaMode: uint32_t
{
    Unknown,
    Straight,
    Premultiplied,
};

enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
};

struct NativeImageInfo
{
    uint32_t sizeX;
    uint32_t sizeY;
    NativePixelFormat format;
    AlphaMode alpha;

    // CICP
    int colorPrimaries = 2;
    int transferCharacteristics = 2;

    // chromaticities
    float cRx, cRy, cGx, cGy, cBx, cBy, cWx, cWy;

    // metadata
    void* exifData;
    int exifSize;
    void *xmpData;
    int xmpSize;
    void *iccData;
    int iccSize;
};

typedef void (*LogDelegate)(LogLevel level, const char *str);
typedef int (*ReadDelegate)(void *ptr, int size);
typedef int64_t(*SeekDelegate)(int64_t pos, SeekOrigin origin);

typedef void *DecoderHandle;

extern "C"
{
    EXPORT void SetLogger(LogDelegate log);

    EXPORT ErrorCode OpenDecoder(NativeFormat format, ReadDelegate read, SeekDelegate seek, DecoderHandle &outHandle);

    EXPORT void CloseDecoder(DecoderHandle &handle);

    EXPORT ErrorCode GetImageInfo(DecoderHandle handle, NativeImageInfo &info);

    EXPORT ErrorCode GetImageData(DecoderHandle handle, void* memory);
}