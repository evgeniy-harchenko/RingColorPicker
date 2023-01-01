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

    private GradientScaledSpin _hueSpinbutton;
    private GradientScaledSpin _satSpinbutton;
    private GradientScaledSpin _valSpinbutton;
    private GradientScaledSpin _redSpinbutton;
    private GradientScaledSpin _greenSpinbutton;
    private GradientScaledSpin _blueSpinbutton;
    private GradientScaledSpin _opacitySpinbutton;

    private Entry _hexEntry;

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

        _table.Attach(new Label("RGB") { Halign = Align.Start, Valign = Align.Center }, 0, 0, 1, 1);
        _table.Attach(new HSeparator { Valign = Align.Center }, 1, 0, 1, 1);

        make_label_spinbutton(out _redSpinbutton, "R:", _table, 0, 1, ColorSel.COLORSEL_RED);
        make_label_spinbutton(out _greenSpinbutton, "G:", _table, 0, 2, ColorSel.COLORSEL_GREEN);
        make_label_spinbutton(out _blueSpinbutton, "B:", _table, 0, 3, ColorSel.COLORSEL_BLUE);


        _table.Attach(new Label("Hex:") { Halign = Align.Start, Valign = Align.Center, MnemonicWidget = _hexEntry }, 0,
            4, 1, 1);
        _hexEntry = new Entry
        {
            TooltipText =
                "You can enter an HTML-style hexadecimal color value, or simply a color name such as “orange” in this entry.",
            WidthChars = 7,
            MarginStart = GradientScaledSpin.GradientWidth,
            Halign = Align.Fill
        };
        _table.Attach(_hexEntry, 1, 4, 1, 1);

        _table.Attach(new Label("HSV") { Halign = Align.Start, Valign = Align.Center }, 0, 5, 1, 1);
        _table.Attach(new HSeparator { Valign = Align.Center }, 1, 5, 1, 1);

        make_label_spinbutton(out _hueSpinbutton, "H:", _table, 0, 6, ColorSel.COLORSEL_HUE);
        make_label_spinbutton(out _satSpinbutton, "S:", _table, 0, 7, ColorSel.COLORSEL_SATURATION);
        make_label_spinbutton(out _valSpinbutton, "V:", _table, 0, 8, ColorSel.COLORSEL_VALUE);
        
        _table.Attach(new Label("Op_acity") { Halign = Align.Start, Valign = Align.Center }, 0, 9, 1, 1);
        _table.Attach(new HSeparator { Valign = Align.Center }, 1, 9, 1, 1);

        make_label_spinbutton(out _opacitySpinbutton, null, _table, 0, 10, ColorSel.COLORSEL_OPACITY);

        ShowAll();

        UpdateColors();

        // TODO in ColorWheel color to args and add color selected property
        _colorWheel.ColorPicked += (sender, color) => { UpdateColors(); };
    }

    private static void make_label_spinbutton(out GradientScaledSpin spinButton, string text, Grid table, int i, int j,
        ColorSel channelType)
    {
        Adjustment adjust;

        switch (channelType)
        {
            case ColorSel.COLORSEL_HUE:
                adjust = new Adjustment(0.0, 0.0, 360.0, 1.0, 1.0, 0.0);
                spinButton = new GradientScaledSpin(adjust);
                spinButton.Wrap = true;
                break;
            case ColorSel.COLORSEL_SATURATION:
            case ColorSel.COLORSEL_VALUE:
                adjust = new Adjustment(0.0, 0.0, 100.0, 1.0, 1.0, 0.0);
                spinButton = new GradientScaledSpin(adjust, new Cairo.Color(0, 0, 0), new Cairo.Color(0, 0, 0));
                break;
            default:
                adjust = new Adjustment(0.0, 0.0, 255.0, 1.0, 1.0, 0.0);
                spinButton = new GradientScaledSpin(adjust, new Cairo.Color(0, 0, 0), new Cairo.Color(0, 0, 0));
                break;
        }

        //adjust.AddSignalHandler("value-changed", () => { });

        Label label = new Label(text) { Halign = Align.Start, Valign = Align.Center, MnemonicWidget = spinButton };

        table.Attach(label, i, j, 1, 1);
        table.Attach(spinButton, i + 1, j, 1, 1);
    }

    private void UpdateColors()
    {
        Cairo.Color colorRgb = _colorWheel.SelectedColor;
        ColorHsv colorHsv = new ColorHsv(colorRgb);

        _redSpinbutton.ChangeGradientStartColor(new Cairo.Color(0, colorRgb.G, colorRgb.B));
        _redSpinbutton.ChangeGradientEndColor(new Cairo.Color(1, colorRgb.G, colorRgb.B));
        _greenSpinbutton.ChangeGradientStartColor(new Cairo.Color(colorRgb.R, 0, colorRgb.B));
        _greenSpinbutton.ChangeGradientEndColor(new Cairo.Color(colorRgb.R, 1, colorRgb.B));
        _blueSpinbutton.ChangeGradientStartColor(new Cairo.Color(colorRgb.R, colorRgb.G, 0));
        _blueSpinbutton.ChangeGradientEndColor(new Cairo.Color(colorRgb.R, colorRgb.G, 1));

        _satSpinbutton.ChangeGradientStartColor(new ColorHsv(colorHsv.H, 0, colorHsv.V).ToRgbColor());
        _satSpinbutton.ChangeGradientEndColor(new ColorHsv(colorHsv.H, 1, colorHsv.V).ToRgbColor());
        _valSpinbutton.ChangeGradientStartColor(new ColorHsv(colorHsv.H, colorHsv.S, 0).ToRgbColor());
        _valSpinbutton.ChangeGradientEndColor(new ColorHsv(colorHsv.H, colorHsv.S, 1).ToRgbColor());

        _opacitySpinbutton.ChangeGradientStartColor(new Cairo.Color(colorRgb.R, colorRgb.G, colorRgb.B, 0));
        _opacitySpinbutton.ChangeGradientEndColor(new Cairo.Color(colorRgb.R, colorRgb.G, colorRgb.B, 1));
    }

    private static double ScaleRound(double val, double factor)
    {
        val = Math.Floor(val * factor + 0.5);
        val = Math.Max(val, 0);
        val = Math.Min(val, factor);
        return val;
    }
}