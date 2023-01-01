using Cairo;
using Gtk;

namespace RingColorPicker;

public struct ColorHsv
{
    private double _h, _s, _v;

    public ColorHsv(double h, double s, double v)
    {
        _h = h;
        _s = s;
        _v = v;
    }

    public ColorHsv(Color color)
    {
        ColorHsv hsv = Rgb2Hsv(color);
        _h = hsv._h;
        _s = hsv._s;
        _v = hsv._v;
    }

    public double H
    {
        get => _h;
        set => _h = value;
    }

    public double S
    {
        get => _s;
        set => _s = value;
    }

    public double V
    {
        get => _v;
        set => _v = value;
    }

    public Color ToRgbColor()
    {
        return Hsv2Rgb(this);
    }

    public static ColorHsv RgbToHsv(Color color)
    {
        return Rgb2Hsv(color);
    }

    private static ColorHsv Rgb2Hsv(Color color)
    {
        double red = color.R, green = color.G, blue = color.B;
        double h = 0, s;
        double min, max;

        if (red > green)
        {
            max = red > blue ? red : blue;

            min = green < blue ? green : blue;
        }
        else
        {
            max = green > blue ? green : blue;

            min = red < blue ? red : blue;
        }

        double v = max;

        if (max != 0.0)
            s = (max - min) / max;
        else
            s = 0.0;

        if (s == 0.0)
            h = 0.0;
        else
        {
            double delta = max - min;

            if (red == max)
                h = (green - blue) / delta;
            else if (green == max)
                h = 2 + (blue - red) / delta;
            else if (blue == max)
                h = 4 + (red - green) / delta;

            h /= 6.0;

            if (h < 0.0)
                h += 1.0;
            else if (h > 1.0)
                h -= 1.0;
        }

        return new ColorHsv(h, s, v);
    }

    private static Color Hsv2Rgb(ColorHsv hsv)
    {
        HSV.ToRgb(hsv._h, hsv._s, hsv._v, out double r, out double g, out double b);
        return new Color(r, g, b, 1);
    }
}