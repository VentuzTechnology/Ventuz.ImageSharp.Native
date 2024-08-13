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

#include "OpenEXR/ImfRgbaFile.h"
#include "OpenEXR/ImfChromaticitiesAttribute.h"

#define BUFFER_ALL 0

using namespace Imf;
using namespace Imath;

class OpenExrDecoder: public IDecoder, Imf::IStream
{
public:
    OpenExrDecoder(): IStream("") { }

    ~OpenExrDecoder()
    {
        delete file;
    }

    RgbaInputFile *file = nullptr;
    Box2i dw;

    // Inherited via IDecoder
    bool Init() override
    {
        try
        {
            file = new RgbaInputFile((IStream &)*this);
        }
        catch (...)
        {
            return false;
        }

        dw = file->dataWindow();
        return true;
    }

    ErrorCode GetImageInfo(NativeImageInfo &info) override
    {
        auto channels = file->channels();

        info.sizeX = dw.max.x - dw.min.x + 1;
        info.sizeY = dw.max.y - dw.min.y + 1;
        info.format = channels == RgbaChannels::WRITE_R ? NativePixelFormat::R_F16 : NativePixelFormat::RGBA_F16;
        info.alpha = AlphaMode::Unknown;
        info.transferCharacteristics = 8; // linear

        auto chAttr = file->header().findTypedAttribute<ChromaticitiesAttribute>("chromaticities");
        if (chAttr)
        {
            auto &chroma = chAttr->value();
            info.cRx = chroma.red.x;
            info.cRy = chroma.red.y;
            info.cGx = chroma.green.x;
            info.cGy = chroma.green.y;
            info.cBx = chroma.blue.x;
            info.cBy = chroma.blue.y;
            info.cWx = chroma.white.x;
            info.cWy = chroma.white.y;
        }

        return ErrorCode::Ok;
    }

    ErrorCode GetImageData(void *memory) override
    {
        int width = dw.max.x - dw.min.x + 1;
        int height = dw.max.y - dw.min.y + 1;

        if (file->channels() == RgbaChannels::WRITE_R)
        {
            int np = width * height;
            auto temp = new Rgba[np];
            file->setFrameBuffer(temp - dw.min.x - dw.min.y * width, 1, width);
            file->readPixels(dw.min.y, dw.max.y);

            const Rgba *src = temp;
            uint16_t *dest = (uint16_t *)memory;
            for (int i = 0; i < np; i++)
                *dest++ = (src++)->r.bits();

            delete[] temp;
        }
        else
        {
            file->setFrameBuffer(((Rgba *)memory) - dw.min.x - dw.min.y * width, 1, width);
            file->readPixels(dw.min.y, dw.max.y);
        }

        return ErrorCode::Ok;
    }

private:

    // IStream implementation

    bool read(char c[], int n) override
    {
        auto rd = Read(c, n);
        return rd == n;
    }

    uint64_t tellg() override
    {
        return Seek(0, SeekOrigin::Current);
    }

    void seekg(uint64_t pos) override
    {
        Seek(pos, SeekOrigin::Begin);
    }

    void clear() override {}
};

IDecoder *CreateOpenExrDecoder() { return new OpenExrDecoder(); }
