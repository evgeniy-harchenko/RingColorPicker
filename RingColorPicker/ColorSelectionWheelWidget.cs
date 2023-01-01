using System;
using Cairo;
using Gdk;
using Gtk;
using Label = Gtk.Label;

namespace RingColorPicker;

public class ColorSelectionWheelWidget : Box
{
    private Box _topHBox;
    private Box _hBox;
    private Box _vBox;
    private Frame _frame;
    private Button _button;
    private Grid _table;
    private Box _topRightVBox;

    private ColorWheelWidget _colorWheel;

    private GradientScaledSpin _hueScaledSpin;
    private GradientScaledSpin _satScaledSpin;
    private GradientScaledSpin _valScaledSpin;
    private GradientScaledSpin _redScaledSpin;
    private GradientScaledSpin _greenScaledSpin;
    private GradientScaledSpin _blueScaledSpin;
    private GradientScaledSpin _opacityScaledSpin;

    private Entry _hexEntry;

    private Label rgbLabel;
    private HSeparator rgbSeparator;
    private Label hsvLabel;
    private HSeparator hsvSeparator;
    private Label opacityLabel;
    private HSeparator opacitySeparator;

    private enum ColorSel
    {
        COLORSEL_RED = 0,
        COLORSEL_GREEN = 1,
        COLORSEL_BLUE = 2,
        COLORSEL_OPACITY = 3,
        COLORSEL_HUE,
        COLORSEL_SATURATION,
        COLORSEL_VALUE,
        COLORSEL_NUM_CHANNELS
    };

    public ColorSelectionWheelWidget() : base(Orientation.Vertical, 0)
    {
        Init();
    }

    private void Init()
    {
        _topHBox = new Box(Orientation.Horizontal, 12);
        PackStart(_topHBox, false, false, 0);

        _vBox = new Box(Orientation.Vertical, 0);
        _topHBox.PackStart(_vBox, false, false, 0);
        _colorWheel = new ColorWheelWidget(200);
        _vBox.PackStart(_colorWheel, false, false, 0);
        _hBox = new Box(Orientation.Horizontal, 6);
        _vBox.PackEnd(_hBox, false, false, 0);

        _frame = new Frame(null);
        _frame.SetSizeRequest(-1, 30);
        _frame.ShadowType = ShadowType.In;


        /*Box sample_area = new Box(Orientation.Horizontal, 0);
        DrawingArea old_sample = new DrawingArea();
        DrawingArea cur_sample = new DrawingArea();
        sample_area.PackStart(old_sample, true, true, 0);
        sample_area.PackStart(cur_sample, true, true, 0);
        _frame.Add(sample_area);*/

        _hBox.PackStart(_frame, true, true, 0);

        _button = new Button();
        _button.AddEvents((int)(EventMask.PointerMotionMask | EventMask.PointerMotionHintMask));
        _button.Show();
        _hBox.PackEnd(_button, false, false, 0);

        _topRightVBox = new Box(Orientation.Vertical, 6);
        _topHBox.PackStart(_topRightVBox, false, false, 0);
        _table = new Grid();
        _topRightVBox.PackStart(_table, false, false, 0);
        _table.RowSpacing = 0;
        _table.ColumnSpacing = 1;

        rgbLabel = new Label("RGB") { Halign = Align.Start, Valign = Align.Center };
        _table.Attach(rgbLabel, 0, 0, 2, 1);
        rgbSeparator = new HSeparator { Valign = Align.Center, MarginStart = rgbLabel.AllocatedWidth };
        _table.Attach(rgbSeparator, 1, 0, 1, 1);

        MakeLabeledScaledSpins(out _redScaledSpin, "R:", _table, 0, 1, ColorSel.COLORSEL_RED);
        MakeLabeledScaledSpins(out _greenScaledSpin, "G:", _table, 0, 2, ColorSel.COLORSEL_GREEN);
        MakeLabeledScaledSpins(out _blueScaledSpin, "B:", _table, 0, 3, ColorSel.COLORSEL_BLUE);


        _table.Attach(new Label("Hex:") { Halign = Align.Start, Valign = Align.Center, MnemonicWidget = _hexEntry }, 0,
            4, 2, 1);
        _hexEntry = new Entry
        {
            TooltipText =
                "You can enter an HTML-style hexadecimal color value, or simply a color name such as “orange” in this entry.",
            WidthChars = 7,
            MarginStart = GradientScaledSpin.GradientWidth,
            Halign = Align.Fill
        };
        _table.Attach(_hexEntry, 1, 4, 1, 1);

        hsvLabel = new Label("HSV") { Halign = Align.Start, Valign = Align.Center };
        _table.Attach(hsvLabel, 0, 5, 2, 1);
        hsvSeparator = new HSeparator { Valign = Align.Center, MarginStart = hsvLabel.AllocatedWidth };
        _table.Attach(hsvSeparator, 1, 5, 1, 1);

        MakeLabeledScaledSpins(out _hueScaledSpin, "H:", _table, 0, 6, ColorSel.COLORSEL_HUE);
        MakeLabeledScaledSpins(out _satScaledSpin, "S:", _table, 0, 7, ColorSel.COLORSEL_SATURATION);
        MakeLabeledScaledSpins(out _valScaledSpin, "V:", _table, 0, 8, ColorSel.COLORSEL_VALUE);

        opacityLabel = new Label("Op_acity") { Halign = Align.Start, Valign = Align.Center };
        _table.Attach(opacityLabel, 0, 9, 2, 1);
        opacitySeparator = new HSeparator
            { Valign = Align.Center, MarginStart = opacityLabel.AllocatedWidth };
        _table.Attach(opacitySeparator, 1, 9, 1, 1);

        MakeLabeledScaledSpins(out _opacityScaledSpin, null, _table, 0, 10, ColorSel.COLORSEL_OPACITY);

        ShowAll();

        UpdateColors();

        // TODO in ColorWheel color to args and add color selected property
        _colorWheel.ColorPicked += (sender, color) => { UpdateColors(); };
    }

    protected override void OnRealized()
    {
        rgbSeparator.MarginStart = rgbLabel.AllocatedWidth;
        hsvSeparator.MarginStart = hsvLabel.AllocatedWidth;
        opacitySeparator.MarginStart = opacityLabel.AllocatedWidth;
        base.OnRealized();
    }
    
    private static void MakeLabeledScaledSpins(out GradientScaledSpin scaledSpin, string text, Grid table, int i, int j,
        ColorSel channelType)
    {
        Adjustment adjust;

        switch (channelType)
        {
            case ColorSel.COLORSEL_HUE:
                adjust = new Adjustment(0.0, 0.0, 360.0, 1.0, 1.0, 0.0);
                scaledSpin = new GradientScaledSpin(adjust);
                scaledSpin.IsHue = true;
                scaledSpin.Wrap = true;
                break;
            case ColorSel.COLORSEL_SATURATION:
            case ColorSel.COLORSEL_VALUE:
                adjust = new Adjustment(0.0, 0.0, 100.0, 1.0, 1.0, 0.0);
                scaledSpin = new GradientScaledSpin(adjust);
                break;
            default:
                adjust = new Adjustment(0.0, 0.0, 255.0, 1.0, 1.0, 0.0);
                scaledSpin = new GradientScaledSpin(adjust);
                break;
        }

        //adjust.AddSignalHandler("value-changed", () => { });

        Label label = new Label(text) { Halign = Align.Start, Valign = Align.Center, MnemonicWidget = scaledSpin };

        table.Attach(label, i, j, 1, 1);
        table.Attach(scaledSpin, i + 1, j, 1, 1);
    }

    private void UpdateColors()
    {
        Cairo.Color colorRgb = _colorWheel.SelectedColor;
        ColorHsv colorHsv = new ColorHsv(colorRgb);

        _redScaledSpin.ChangeGradientStartColor(new Cairo.Color(0, colorRgb.G, colorRgb.B));
        _redScaledSpin.ChangeGradientEndColor(new Cairo.Color(1, colorRgb.G, colorRgb.B));

        _greenScaledSpin.ChangeGradientStartColor(new Cairo.Color(colorRgb.R, 0, colorRgb.B));
        _greenScaledSpin.ChangeGradientEndColor(new Cairo.Color(colorRgb.R, 1, colorRgb.B));

        _blueScaledSpin.ChangeGradientStartColor(new Cairo.Color(colorRgb.R, colorRgb.G, 0));
        _blueScaledSpin.ChangeGradientEndColor(new Cairo.Color(colorRgb.R, colorRgb.G, 1));

        _satScaledSpin.ChangeGradientStartColor(new ColorHsv(colorHsv.H, 0, colorHsv.V).ToRgbColor());
        _satScaledSpin.ChangeGradientEndColor(new ColorHsv(colorHsv.H, 1, colorHsv.V).ToRgbColor());

        _valScaledSpin.ChangeGradientStartColor(new ColorHsv(colorHsv.H, colorHsv.S, 0).ToRgbColor());
        _valScaledSpin.ChangeGradientEndColor(new ColorHsv(colorHsv.H, colorHsv.S, 1).ToRgbColor());

        _opacityScaledSpin.ChangeGradientStartColor(new Cairo.Color(colorRgb.R, colorRgb.G, colorRgb.B, 0));
        _opacityScaledSpin.ChangeGradientEndColor(new Cairo.Color(colorRgb.R, colorRgb.G, colorRgb.B, 1));

        _redScaledSpin.Value = Math.Round(colorRgb.R * 255.0);
        _greenScaledSpin.Value = Math.Round(colorRgb.G * 255.0);
        _blueScaledSpin.Value = Math.Round(colorRgb.B * 255.0);

        _hexEntry.Text = $"{(int)(colorRgb.R * 255):X2}{(int)(colorRgb.G * 255):X2}{(int)(colorRgb.B * 255):X2}";

        _hueScaledSpin.Value = Math.Round(colorHsv.H * 360.0);
        _satScaledSpin.Value = Math.Round(colorHsv.S * 100.0);
        _valScaledSpin.Value = Math.Round(colorHsv.V * 100.0);

        _opacityScaledSpin.Value = Math.Round(colorRgb.A * 255.0);
    }

    private static double ScaleRound(double val, double factor)
    {
        val = Math.Floor(val * factor + 0.5);
        val = Math.Max(val, 0);
        val = Math.Min(val, factor);
        return val;
    }
}