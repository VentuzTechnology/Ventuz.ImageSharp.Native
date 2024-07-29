#include "api.h"
#include "decoder.h"

#include "libheif/heif.h"


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