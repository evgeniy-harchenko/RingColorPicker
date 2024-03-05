using System;
using System.Diagnostics;
using Cairo;

namespace RingColorPicker;

public static class CairoUtils
{
    private static int StrideForWidthBpp(int w, int bpp)
    {
        return ((bpp * w + 7) / 8 + sizeof(uint) - 1) & -sizeof(uint);
    }

    private static bool FormatValid(Format format)
    {
        return format >= Format.Argb32 && format <= Format.Rgb16565;
    }

    private static int FormatBitsPerPixel(Format format)
    {
        switch (format)
        {
            case Format.Argb32:
            case Format.Rgb24:
                return 32;
            case Format.Rgb16565:
                return 16;
            case Format.A8:
                return 8;
            case Format.A1:
                return 1;
            default:
                Debug.Fail("Wrong image format: " + format);
                return 0;
        }
    }

    public static int FormatStrideForWidth(Format format, int width)
    {
        if (!FormatValid(format))
        {
            return -1;
        }

        var bpp = FormatBitsPerPixel(format);
        if ((uint)width >= (Int32.MaxValue - 7) / (uint)bpp)
            return -1;

        return StrideForWidthBpp(width, bpp);
    }
}