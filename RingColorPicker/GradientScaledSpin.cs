using System;
using Gtk;

namespace RingColorPicker;

public class GradientScaledSpin : Grid
{
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

    public GradientScaledSpin(Adjustment adjustment)
    {
        _adjustment = adjustment;

        _gradientScale =
            new GradientScale(_adjustment.Lower, _adjustment.Upper, _adjustment.StepIncrement, height: 15)
                { WidthRequest = 100, Valign = Align.Center, Halign = Align.Fill };
        _spinButton = new SpinButton(_adjustment, 10, 0) { Valign = Align.Center, Halign = Align.Center };

        Attach(_gradientScale, 0, 0, 1, 1);
        Attach(_spinButton, 1, 0, 1, 1);
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
        _gradientScale.Value = _spinButton.Value;
    }

    private void GradientScaleOnValueChanged(object sender, double value)
    {
        _spinButton.Value = value;
    }

    protected override void OnHidden()
    {
        base.OnHidden();
        _gradientScale.ValueChanged -= GradientScaleOnValueChanged;
        _spinButton.ValueChanged -= SpinButtonOnValueChanged;
        Hide();
    }
}