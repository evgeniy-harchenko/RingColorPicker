using System;
using Cairo;
using Gdk;
using Gtk;
using Color = Cairo.Color;

namespace RingColorPicker;

public class ColorWheelWidget : DrawingArea
{
    private const int DefaultSize = 500;

    private readonly ColorWheel _colorWheel;
    private readonly int _size, _radius;
    private readonly double _centerX, _centerY;

    private double _cursorX, _cursorY;
    private bool _isInRingClick;

    private Color _selectedColor;

    public delegate void ColorPickedHandler(object sender, Color color);

    public event ColorPickedHandler ColorPicked;

    public Color SelectedColor => _selectedColor;

    private double SelectorRadius => _radius / 25f;
    private double SelectorLineWidth => _radius / 100f;

    public ColorWheelWidget(int size = DefaultSize)
    {
        _size = size;
        _centerX = _size / 2.0;
        _centerY = _size / 2.0;
        _radius = _size / 2;

        _radius = _size / 2;
        _radius -= (int)Math.Ceiling(SelectorRadius + SelectorLineWidth);

        _isInRingClick = false;

        _colorWheel = new ColorWheel(_size, _radius, _centerX, _centerY);

        SetSizeRequest(_size, _size);
        AddEvents((int)(EventMask.ButtonPressMask | EventMask.ButtonReleaseMask | EventMask.ButtonMotionMask));

        SetSelectedColorRgb(new Color(1, 1, 1));
    }

    public void SetSelectedColorRgb(Color color)
    {
        _selectedColor = color;
        SetSelectedColor(_selectedColor);
    }

    public void SetSelectedColorHsv(ColorHsv color)
    {
        _selectedColor = color.ToRgbColor();
        SetSelectedColor(_selectedColor);
    }

    private void SetSelectedColor(Color color)
    {
        PointD pointD = _colorWheel.GetCoordsByRgbColor(color);
        _cursorX = pointD.X;
        _cursorY = pointD.Y;
        QueueDraw();
    }

    protected override void OnShown()
    {
        base.OnShown();

        MotionNotifyEvent += OnMotionNotifyEvent;
        ButtonPressEvent += OnButtonPressEvent;
        ButtonReleaseEvent += OnButtonReleaseEvent;
    }

    private void OnButtonPressEvent(object o, ButtonPressEventArgs args)
    {
        if (_colorWheel.IsInRing(args.Event.X, args.Event.Y))
        {
            _cursorX = args.Event.X;
            _cursorY = args.Event.Y;

            _isInRingClick = true;

            PickColorAndRedraw();
        }
    }

    private void OnButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
    {
        _isInRingClick = false;
    }

    private void OnMotionNotifyEvent(object o, MotionNotifyEventArgs args)
    {
        if (!_isInRingClick)
        {
            return;
        }

        _cursorX = args.Event.X;
        _cursorY = args.Event.Y;

        if (!_colorWheel.IsInRing(_cursorX, _cursorY))
        {
            double d = Math.Pow(Math.Pow(_cursorX - _centerX, 2.0) + Math.Pow(_cursorY - _centerY, 2.0), 0.5); // + 1.0;
            double x2 = _centerX + (_cursorX - _centerX) / d * _radius;
            double y2 = _centerY + (_cursorY - _centerY) / d * _radius;

            _cursorX = x2;
            _cursorY = y2;
        }

        PickColorAndRedraw();
    }

    protected override void OnHidden()
    {
        base.OnHidden();
        MotionNotifyEvent -= OnMotionNotifyEvent;
        ButtonPressEvent -= OnButtonPressEvent;
        ButtonReleaseEvent -= OnButtonReleaseEvent;
    }

    protected override bool OnDrawn(Context context)
    {
        base.OnDrawn(context);
        DrawRing(context);
        DrawSelector(context);
        return true;
    }

    private void PickColorAndRedraw()
    {
        double h = _colorWheel.GetHue(_cursorX, _cursorY);
        double s = _colorWheel.GetSat(_cursorX, _cursorY);
        double v = 1;

        HSV.ToRgb(h, s, v, out double r, out double g, out double b);

        _selectedColor = new Color(r, g, b, 1.0);

        ColorPicked?.Invoke(this, _selectedColor);
        QueueDraw();
    }

    private void DrawSelector(Context context)
    {
        context.Translate(_cursorX, _cursorY);

        context.LineWidth = SelectorLineWidth;
        context.SetSourceRGB(0, 0, 0);
        context.Arc(0, 0, SelectorRadius, 0, 2 * Math.PI);
        context.Stroke();

        context.SetSourceRGB(1, 1, 1);
        context.Arc(0, 0, SelectorRadius - SelectorLineWidth, 0, 2 * Math.PI);
        context.Stroke();

        context.SetSourceColor(_selectedColor);
        context.Arc(0, 0, SelectorRadius - SelectorLineWidth - SelectorLineWidth / 2, 0, 2 * Math.PI);
        context.Fill();
    }

    private void DrawRing(Context context)
    {
        ImageSurface source = _colorWheel.GetImageSource();
        context.Arc(_centerX, _centerY, _radius, 0, 2 * Math.PI);
        context.Clip();
        context.SetSourceSurface(source, 0, 0);
        context.Paint();
        context.ResetClip();
        source.Dispose();
    }
}