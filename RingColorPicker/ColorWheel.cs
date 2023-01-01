using System;
using Cairo;
using Gtk;

namespace RingColorPicker;

public class ColorWheel : IDisposable
{
    private readonly int _size, _stride;

    private readonly double _radius;
    private readonly double _centerX, _centerY;

    private byte[] _wheelBuf;

    private ColorWheel()
    {
    }

    public ColorWheel(int size, int radius, double x, double y)
    {
        _size = size;
        _radius = radius;
        _centerX = x;
        _centerY = y;
        _wheelBuf = null;
        _stride = CairoUtils.FormatStrideForWidth(Format.Rgb24, _size);
    }

    public ImageSurface GetImageSource()
    {
        int index = 0;
        if (_wheelBuf == null)
        {
            _wheelBuf = new byte[_size * _stride];

            for (int y = 0; y < _size; y++)
            {
                double dy = y - _centerY;
                for (int x = 0; x < _size; x++)
                {
                    double dx = x - _centerX;
                    double dist = dx * dx + dy * dy;
                    if (dist > _radius * _radius)
                    {
                        _wheelBuf[index++] = 0;
                        _wheelBuf[index++] = 0;
                        _wheelBuf[index++] = 0;
                        _wheelBuf[index++] = 0;
                        continue;
                    }

                    HSV.ToRgb(CalcHue(dx, dy), CalcSat(dist), 1, out double r, out double g, out double b);

                    _wheelBuf[index++] = (byte)Math.Floor(b * 255);
                    _wheelBuf[index++] = (byte)Math.Floor(g * 255);
                    _wheelBuf[index++] = (byte)Math.Floor(r * 255);
                    _wheelBuf[index++] = 0;
                }
            }
        }

        return new ImageSurface(_wheelBuf, Format.Rgb24, _size, _size, _stride);
    }

    public double GetHue(double x, double y)
    {
        double dx = x - _centerX;
        double dy = y - _centerY;

        return CalcHue(dx, dy);
    }

    private double CalcHue(double dx, double dy)
    {
        double angle = Math.Atan2(dy, dx);

        if (angle < 0.0)
            angle += 2.0 * Math.PI;

        return angle / (2.0 * Math.PI);
    }

    public double GetSat(double x, double y)
    {
        double dx = x - _centerX;
        double dy = y - _centerY;
        double dist = dx * dx + dy * dy;

        return CalcSat(dist);
    }

    private double CalcSat(double dist)
    {
        return Math.Pow(dist, 0.5) / _radius;
    }

    public PointD GetCoordsByHsvColor(ColorHsv color)
    {
        return GetCoordsByHsvColor(color.H, color.S);
    }

    public PointD GetCoordsByHsvColor(double hue, double sat)
    {
        double x = Double.Cos(hue * 360 * (Math.PI / 180));
        double y = Double.Sin(hue * 360 * (Math.PI / 180));

        return new PointD(_centerX + x * sat * _radius, _centerY + y * sat * _radius);
    }

    public PointD GetCoordsByRgbColor(Color color)
    {
        return GetCoordsByRgbColor(color.R, color.G, color.B);
    }

    public PointD GetCoordsByRgbColor(double r, double g, double b)
    {
        ColorHsv colorHsv = ColorHsv.RgbToHsv(new Color(r, g, b, 1));
        return GetCoordsByHsvColor(colorHsv.H, colorHsv.S);
    }

    public bool IsInRing(double x, double y)
    {
        double dx = x - _centerX;
        double dy = y - _centerY;

        return dx * dx + dy * dy <= _radius * _radius;
    }

    public void Dispose()
    {
        _wheelBuf = null;
    }
}