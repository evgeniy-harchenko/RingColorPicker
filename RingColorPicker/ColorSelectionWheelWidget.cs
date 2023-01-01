using System;
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

    private SpinButton _hueSpinbutton;
    private SpinButton _satSpinbutton;
    private SpinButton _valSpinbutton;
    private SpinButton _redSpinbutton;
    private SpinButton _greenSpinbutton;
    private SpinButton _blueSpinbutton;

    private Entry _hexEntry;
    private Scale _opacitySlider;
    private Entry _opacityEntry;

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
        ColorWheelWidget cw = new ColorWheelWidget(200);
        _vBox.PackStart(cw, false, false, 0);

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
        _table.RowSpacing = 3;
        _table.ColumnSpacing = 3;

        _table.Attach(new Label("RGB") { Halign = Align.Start, Valign = Align.Center }, 0, 0, 1, 1);
        _table.Attach(new HSeparator { Valign = Align.Center }, 1, 0, 1, 1);

        make_label_spinbutton(out _redSpinbutton, "R:", _table, 0, 1, ColorSel.COLORSEL_RED);
        make_label_spinbutton(out _greenSpinbutton, "G:", _table, 0, 2, ColorSel.COLORSEL_GREEN);
        make_label_spinbutton(out _blueSpinbutton, "B:", _table, 0, 3, ColorSel.COLORSEL_BLUE);


        _table.Attach(new Label("HEX:") { Halign = Align.Start, Valign = Align.Center, MnemonicWidget = _hexEntry }, 0,
            4, 1, 1);
        _hexEntry = new Entry
        {
            TooltipText =
                "You can enter an HTML-style hexadecimal color value, or simply a color name such as “orange” in this entry.",
            WidthChars = 7
        };
        _table.Attach(_hexEntry, 1, 4, 1, 1);

        _table.Attach(new Label("HSV") { Halign = Align.Start, Valign = Align.Center }, 0, 5, 1, 1);
        _table.Attach(new HSeparator { Valign = Align.Center }, 1, 5, 1, 1);

        make_label_spinbutton(out _hueSpinbutton, "H:", _table, 0, 6, ColorSel.COLORSEL_HUE);
        make_label_spinbutton(out _satSpinbutton, "S:", _table, 0, 7, ColorSel.COLORSEL_SATURATION);
        make_label_spinbutton(out _valSpinbutton, "V:", _table, 0, 8, ColorSel.COLORSEL_VALUE);

        Label opacityLabel = new Label("Op_acity")
        {
            Halign = Align.Start,
            Valign = Align.Center,
            MnemonicWidget = _opacitySlider
        };

        _table.Attach(opacityLabel, 0, 9, 1, 1);
        _table.Attach(new HSeparator { Valign = Align.Center }, 1, 9, 1, 1);

        _opacitySlider = new Scale(Orientation.Horizontal, new Adjustment(0.0, 0.0, 255.0, 1.0, 1.0, 0.0))
        {
            TooltipText = "Transparency of the color.",
            DrawValue = false
        };

        _opacitySlider.AddSignalHandler("value-changed",
            (Scale scale, EventArgs args) => { Console.WriteLine(scale.Value); });
        _table.Attach(_opacitySlider, 1, 10, 1, 1);

        _opacityEntry = new Entry
        {
            TooltipText = "Transparency of the color.",
            HeightRequest = -1,
            WidthRequest = 40
        };
        _opacityEntry.AddSignalHandler("activate", (Entry entry, EventArgs args) => { Console.WriteLine(entry.Text); });

        _table.Attach(_opacityEntry, 2, 10, 1, 1);

        ShowAll();

        //_topHBox.ShowAll();
    }

    private static void make_label_spinbutton(out SpinButton spinButton, string text, Grid table, int i, int j,
        ColorSel channelType)
    {
        Adjustment adjust;
        spinButton = new SpinButton(null, 10.0, 0);

        switch (channelType)
        {
            case ColorSel.COLORSEL_HUE:
                spinButton.Wrap = true;
                adjust = new Adjustment(0.0, 0.0, 360.0, 1.0, 1.0, 0.0);
                break;
            case ColorSel.COLORSEL_SATURATION:
            case ColorSel.COLORSEL_VALUE:
                adjust = new Adjustment(0.0, 0.0, 100.0, 1.0, 1.0, 0.0);
                break;
            default:
                adjust = new Adjustment(0.0, 0.0, 255.0, 1.0, 1.0, 0.0);
                break;
        }

        //adjust.AddSignalHandler("value-changed", () => { });
        spinButton.Adjustment = adjust;

        Label label = new Label(text) { Halign = Align.Start, Valign = Align.Center, MnemonicWidget = spinButton };

        table.Attach(label, i, j, 1, 1);
        table.Attach(spinButton, i + 1, j, 1, 1);
    }

    private static double ScaleRound(double val, double factor)
    {
        val = Math.Floor(val * factor + 0.5);
        val = Math.Max(val, 0);
        val = Math.Min(val, factor);
        return val;
    }
}