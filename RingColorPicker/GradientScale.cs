using System;
using Cairo;
using Gdk;
using Gtk;
using Color = Cairo.Color;

namespace RingColorPicker;

public class GradientScale : DrawingArea
{
    private readonly bool _isHue;
    private readonly double _min;
    private readonly double _max;
    private readonly double _step;
    private readonly int _height;

    private Color _startColor;
    private Color _endColor;

    private bool _isInRectClick;
    private double _cursorX, _cursorY;

    private double _rectX;
    private double _rectY;
    private double _rectWidth;
    private double _rectHeight;

    private double _value;

    private double CursorSize => _height / 2.0;
    private double CursorBorderLineWidth => CursorSize / 2;
    private double CursorBottomMargin => CursorSize + CursorBorderLineWidth / 2;

    public delegate void ValueChangedHandler(object sender, EventArgs e);

    /// <summary>
    /// Not invoked when Value is set from code.
    /// </summary>
    public event ValueChangedHandler ValueChanged;
    
    /// <summary>
    /// Get and Set Value. It doesn't invoke ValueChanged event.
    /// </summary>
    public double Value
    {
        get => _value;
        set
        {
            _value = value;
            QueueDraw();
        }
    }

    public int ValueAsInt => (int)Math.Round(_value);

    public bool IsHue => _isHue;

    public GradientScale(double min, double max, double step, Color? startColor = null, Color? endColor = null,
        int height = 30)
    {
        if (min > max)
        {
            throw new ArgumentException("min > max");
        }

        _min = min;
        _max = max;
        _step = step;
        _height = height;
        _isHue = startColor == null || endColor == null;
        _value = min;

        if (!_isHue)
        {
            _startColor = (Color)startColor;
            _endColor = (Color)endColor;
        }

        _isInRectClick = false;

        HeightRequest = (int)(_height + CursorBottomMargin * 2);
        AddEvents((int)(EventMask.ButtonPressMask | EventMask.ButtonReleaseMask | EventMask.ButtonMotionMask));
    }

    public void ChangeStartColor(Color startColor)
    {
        if (_isHue)
        {
            throw new Exception("Scale is HUE!");
        }

        _startColor = startColor;
        QueueDraw();
    }

    public void ChangeEndColor(Color endColor)
    {
        if (_isHue)
        {
            throw new Exception("Scale is HUE!");
        }

        _endColor = endColor;
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
        if (IsInRect(args.Event.X, args.Event.Y))
        {
            _cursorX = args.Event.X;
            _cursorY = _rectY + _rectHeight;

            _isInRectClick = true;

            SelectValue();
            QueueDraw();
        }
    }

    private void OnButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
    {
        _isInRectClick = false;
    }

    private void OnMotionNotifyEvent(object o, MotionNotifyEventArgs args)
    {
        if (!_isInRectClick)
        {
            return;
        }

        _cursorX = args.Event.X;
        _cursorY = _rectY + _rectHeight;

        if (!IsInRect(args.Event.X, args.Event.Y))
        {
            _cursorX = Math.Clamp(_cursorX, _rectX, _rectX + _rectWidth);
        }

        SelectValue();
        QueueDraw();
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
        DrawScale(context);
        UpdateSelectorPosition();
        DrawSelector(context);
        return true;
    }

    private void DrawScale(Context context)
    {
        int allocatedHeight = AllocatedHeight;
        int allocatedWidth = AllocatedWidth;

        int width = allocatedWidth;

        double wPadding = _height / 2.4;

        double gX0 = 0 + wPadding;
        double gY0 = allocatedHeight / 2.0;
        double gX1 = width - wPadding;
        double gY1 = gY0;

        _rectX = 0 + wPadding;
        _rectY = AllocatedHeight / 2.0 - _height / 2.0;
        _rectWidth = width - wPadding * 2.0;
        _rectHeight = _height;

        using (LinearGradient gradient = new LinearGradient(gX0, gY0, gX1, gY1))
        {
            if (!_isHue)
            {
                gradient.AddColorStop(0.0, _startColor);
                gradient.AddColorStop(1.0, _endColor);
            }
            else
            {
                for (double i = 0; i < 1; i += 0.05)
                {
                    HSV.ToRgb(i, 1, 1, out double r, out double g, out double b);
                    gradient.AddColorStop(i, new Color(r, g, b));
                }
            }

            if (!_isHue && (_startColor.A < 1.0 || _endColor.A < 1.0))
            {
                DrawChecksBackground(context, _rectX, _rectY, _rectWidth, _rectHeight, _height / 3);
            }

            context.SetSource(gradient);
            context.Rectangle(_rectX, _rectY, _rectWidth, _rectHeight);
            context.Fill();
        }
    }

    private void DrawSelector(Context context)
    {
        context.LineWidth = CursorBorderLineWidth;
        context.SetSourceRGB(1, 1, 1);

        context.MoveTo(_cursorX, _cursorY);
        context.LineTo(_cursorX + CursorSize / 2, _cursorY + CursorSize);
        context.LineTo(_cursorX - CursorSize / 2, _cursorY + CursorSize);
        context.ClosePath();
        context.StrokePreserve();

        context.SetSourceRGB(0, 0, 0);
        context.Fill();
    }

    private void UpdateSelectorPosition()
    {
        double length = Math.Abs(_min) + Math.Abs(_max);
        _cursorX = _rectX + _rectWidth / length * _value;
        _cursorY = _rectY + _rectHeight;
    }

    private void SelectValue()
    {
        double length = Math.Abs(_min) + Math.Abs(_max);
        double realPosition = _cursorX - _rectX;
        double tempValue = length / _rectWidth * realPosition;

        if (_value != tempValue)
        {
            _value = tempValue;
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private bool IsInRect(double x, double y)
    {
        return x >= _rectX && x <= _rectX + _rectWidth && y >= _rectY &&
               y <= _rectY + _rectHeight + CursorBottomMargin;
    }

    private static void DrawChecksBackground(Context context, double x, double y, double width, double height,
        int checkSize = 16,
        bool darker = false)
    {
        int goff = (int)width % 32;

        context.Rectangle(x, y, width, height);
        context.Clip();

        if (darker)
        {
            context.SetSourceRGB(0.5, 0.5, 0.5);
            context.Rectangle(x, y, width, height);
            context.Fill();
        }

        context.SetSourceRGB(0.75, 0.75, 0.75);
        for (int i = (goff & -checkSize) + (int)x; i < goff + x + width; i += checkSize)
        {
            for (int j = (int)y; j < y + height; j += checkSize)
            {
                if ((i / checkSize + j / checkSize) % 2 == 0)
                    context.Rectangle(i - goff, j, checkSize, checkSize);
            }
        }

        context.Fill();
        context.ResetClip();
    }
}