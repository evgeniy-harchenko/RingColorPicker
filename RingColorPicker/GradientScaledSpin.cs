using System;
using Gtk;

namespace RingColorPicker;

public class GradientScaledSpin : Grid
{
    public const int GradientWidth = 100;

    private readonly Adjustment _adjustment;
    private readonly GradientScale _gradientScale;
    private readonly SpinButton _spinButton;

    public double Value
    {
        get => _spinButton.Value;
        set
        {
            _gradientScale.Value = value;
            _spinButton.Value = value;
        }
    }

    public bool Wrap
    {
        get => _spinButton.Wrap;
        set => _spinButton.Wrap = value;
    }

    public delegate void ValueChangedHandler(object sender, double value);

    public event ValueChangedHandler ValueChanged;

    public GradientScaledSpin(Adjustment adjustment, Cairo.Color? startColor = null, Cairo.Color? endColor = null)
    {
        _adjustment = adjustment;

        _gradientScale =
            new GradientScale(_adjustment.Lower, _adjustment.Upper, _adjustment.StepIncrement, startColor,
                    endColor, height: 15)
                { WidthRequest = GradientWidth, Valign = Align.Center, Halign = Align.Fill };
        _spinButton = new SpinButton(_adjustment, 10, 0) { Valign = Align.Center, Halign = Align.Center };
        Value = 0;

        Attach(_gradientScale, 0, 0, 1, 1);
        Attach(_spinButton, 1, 0, 1, 1);
    }

    public void ChangeGradientStartColor(Cairo.Color color)
    {
        _gradientScale.ChangeStartColor(color);
    }

    public void ChangeGradientEndColor(Cairo.Color color)
    {
        _gradientScale.ChangeEndColor(color);
    }

    protected override void OnShown()
    {
        base.OnShown();
        _gradientScale.ValueChanged += GradientScaleOnValueChanged;
        _spinButton.ValueChanged += SpinButtonOnValueChanged;
        ShowAll();
    }

    private void SpinButtonOnValueChanged(object sender, EventArgs e)
    {
        _gradientScale.Value = _spinButton.ValueAsInt;
        ValueChanged?.Invoke(this, _gradientScale.Value);
    }

    private void GradientScaleOnValueChanged(object sender, EventArgs e)
    {
        _spinButton.Value = _gradientScale.ValueAsInt;
    }

    protected override void OnHidden()
    {
        base.OnHidden();
        _gradientScale.ValueChanged -= GradientScaleOnValueChanged;
        _spinButton.ValueChanged -= SpinButtonOnValueChanged;
        Hide();
    }
}