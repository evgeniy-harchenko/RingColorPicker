using System;
using System.Globalization;
using Cairo;
using Gdk;
using Gtk;
using Color = Cairo.Color;
using Label = Gtk.Label;

namespace RingColorPicker;

public class ColorSelectionWheelWidget : Box
{
    private Box _topHBox;
    private Box _hBox;
    private Box _vBox;
    private Frame _frame;
    private ComboBox _nComboBox;
    private Button _moreButton;
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

    private Label _rgbLabel;
    private HSeparator _rgbSeparator;
    private Label _hsvLabel;
    private HSeparator _hsvSeparator;
    private Label _opacityLabel;
    private HSeparator _opacitySeparator;

    private bool _isUpdating;
    private bool _isMoreButtonClicked;

    private ColorHsv _selectedColorHsv;

    public Color SelectedColor
    {
        get => _colorWheel.PickedColor;
        set => _colorWheel.PickedColor = value;
    }

    public delegate void ColorSelectedHandler(object sender, EventArgs e);

    public event ColorSelectedHandler ColorSelected;

    public ColorSelectionWheelWidget() : base(Orientation.Vertical, 0)
    {
        _isUpdating = false;
        _isMoreButtonClicked = true;

        _topHBox = new Box(Orientation.Horizontal, 12);
        _vBox = new Box(Orientation.Vertical, 0);
        _hBox = new Box(Orientation.Horizontal, 6);
        _colorWheel = new ColorWheelWidget(200) { Halign = Align.Center };

        _frame = new Frame(null);
        _frame.SetSizeRequest(-1, 30);
        _frame.ShadowType = ShadowType.In;

        _nComboBox = new ComboBox(new[] { "Primary", "Secondary" }) { Valign = Align.Center };
        _nComboBox.Active = 0;

        _moreButton = new Button("") { Valign = Align.Center, WidthRequest = 100 };
        _moreButton.Clicked += MoreButtonOnClicked;
        _moreButton.AddEvents((int)(EventMask.PointerMotionMask | EventMask.PointerMotionHintMask));

        _topRightVBox = new Box(Orientation.Vertical, 6);

        _table = new Grid { RowSpacing = 0, ColumnSpacing = 1 };

        _rgbLabel = new Label("RGB") { Halign = Align.Start, Valign = Align.Center };
        _rgbSeparator = new HSeparator { Valign = Align.Center, MarginStart = _rgbLabel.AllocatedWidth };

        _hexEntry = new Entry { WidthChars = 7, MarginStart = GradientScaledSpin.GradientWidth, Halign = Align.Fill };

        _hsvLabel = new Label("HSV") { Halign = Align.Start, Valign = Align.Center };
        _hsvSeparator = new HSeparator { Valign = Align.Center, MarginStart = _hsvLabel.AllocatedWidth };
        _opacityLabel = new Label("Op_acity") { Halign = Align.Start, Valign = Align.Center };

        _opacitySeparator = new HSeparator { Valign = Align.Center, MarginStart = _opacityLabel.AllocatedWidth };
    }

    protected override void OnRealized()
    {
        base.OnRealized();
        Init();
    }

    private void Init()
    {
        PackStart(_topHBox, false, false, 0);

        _topHBox.PackStart(_vBox, false, false, 0);
        _vBox.PackStart(_hBox, false, false, 0);
        _vBox.PackStart(_colorWheel, false, false, 0);

        /*Box sample_area = new Box(Orientation.Horizontal, 0);
        DrawingArea old_sample = new DrawingArea();
        DrawingArea cur_sample = new DrawingArea();
        sample_area.PackStart(old_sample, true, true, 0);
        sample_area.PackStart(cur_sample, true, true, 0);
        _frame.Add(sample_area);*/

        _hBox.PackStart(_frame, true, true, 0);
        _hBox.PackStart(_nComboBox, true, true, 0);
        _hBox.PackEnd(_moreButton, false, false, 0);
        _topHBox.PackStart(_topRightVBox, false, false, 0);
        _topRightVBox.PackStart(_table, false, false, 0);

        _table.Attach(_rgbLabel, 0, 0, 2, 1);
        _table.Attach(_rgbSeparator, 1, 0, 1, 1);

        MakeLabeledScaledSpins(out _redScaledSpin, "R:", _table, 0, 1, nameof(_redScaledSpin));
        MakeLabeledScaledSpins(out _greenScaledSpin, "G:", _table, 0, 2, nameof(_greenScaledSpin));
        MakeLabeledScaledSpins(out _blueScaledSpin, "B:", _table, 0, 3, nameof(_blueScaledSpin));

        _table.Attach(new Label("Hex:") { Halign = Align.Start, Valign = Align.Center, MnemonicWidget = _hexEntry }, 0,
            4, 2, 1);

        _table.Attach(_hexEntry, 1, 4, 1, 1);
        _table.Attach(_hsvLabel, 0, 5, 2, 1);
        _table.Attach(_hsvSeparator, 1, 5, 1, 1);

        MakeLabeledScaledSpins(out _hueScaledSpin, "H:", _table, 0, 6, nameof(_hueScaledSpin));
        MakeLabeledScaledSpins(out _satScaledSpin, "S:", _table, 0, 7, nameof(_satScaledSpin));
        MakeLabeledScaledSpins(out _valScaledSpin, "V:", _table, 0, 8, nameof(_valScaledSpin));

        _table.Attach(_opacityLabel, 0, 9, 2, 1);
        _table.Attach(_opacitySeparator, 1, 9, 1, 1);

        MakeLabeledScaledSpins(out _opacityScaledSpin, null, _table, 0, 10, nameof(_opacityScaledSpin));

        ShowAll();

        _moreButton.Click();
    }

    private void MoreButtonOnClicked(object sender, EventArgs e)
    {
        if (_isMoreButtonClicked)
        {
            _moreButton.Label = "Maximize" + " >>";
            _topRightVBox.Hide();
        }
        else
        {
            _moreButton.Label = "Minimize" + " <<";
            _topRightVBox.ShowAll();
        }

        _isMoreButtonClicked = !_isMoreButtonClicked;
    }

    protected override bool OnDrawn(Context cr)
    {
        _rgbSeparator.MarginStart = _rgbLabel.AllocatedWidth;
        _hsvSeparator.MarginStart = _hsvLabel.AllocatedWidth;
        _opacitySeparator.MarginStart = _opacityLabel.AllocatedWidth;

        return base.OnDrawn(cr);
    }

    protected override void OnShown()
    {
        base.OnShown();

        UpdateColors();

        _colorWheel.ColorPicked += OnValueChanged;

        _redScaledSpin.ValueChanged += OnValueChanged;
        _greenScaledSpin.ValueChanged += OnValueChanged;
        _blueScaledSpin.ValueChanged += OnValueChanged;

        _hexEntry.Activated += OnValueChanged;

        _hueScaledSpin.ValueChanged += OnValueChanged;
        _satScaledSpin.ValueChanged += OnValueChanged;
        _valScaledSpin.ValueChanged += OnValueChanged;

        _opacityScaledSpin.ValueChanged += OnValueChanged;
    }

    protected override void OnHidden()
    {
        base.OnHidden();

        _colorWheel.ColorPicked -= OnValueChanged;

        _redScaledSpin.ValueChanged -= OnValueChanged;
        _greenScaledSpin.ValueChanged -= OnValueChanged;
        _blueScaledSpin.ValueChanged -= OnValueChanged;

        _hexEntry.Activated -= OnValueChanged;

        _hueScaledSpin.ValueChanged -= OnValueChanged;
        _satScaledSpin.ValueChanged -= OnValueChanged;
        _valScaledSpin.ValueChanged -= OnValueChanged;

        _opacityScaledSpin.ValueChanged -= OnValueChanged;
    }

    private void OnValueChanged(object sender, EventArgs e)
    {
        if (!_isUpdating)
        {
            if (sender == _colorWheel)
            {
                _selectedColorHsv = new ColorHsv(SelectedColor);
                UpdateColors();

                ColorSelected?.Invoke(this, EventArgs.Empty);
            }
            else if (sender == _redScaledSpin || sender == _greenScaledSpin || sender == _blueScaledSpin)
            {
                Color color = new Color(_redScaledSpin.Value / 255.0, _greenScaledSpin.Value / 255.0,
                    _blueScaledSpin.Value / 255.0, _opacityScaledSpin.Value / 255.0);
                SelectedColor = color;
                _selectedColorHsv = new ColorHsv(SelectedColor);
                UpdateColors();

                ColorSelected?.Invoke(this, EventArgs.Empty);
            }
            else if (sender == _hexEntry)
            {
                Color color = default;
                if (HexToColor(_hexEntry.Text, ref color))
                {
                    color.A = _opacityScaledSpin.Value / 255.0;
                    SelectedColor = color;
                    _selectedColorHsv = new ColorHsv(SelectedColor);
                    UpdateColors();

                    ColorSelected?.Invoke(this, EventArgs.Empty);
                }
            }
            else if (sender == _hueScaledSpin || sender == _satScaledSpin || sender == _valScaledSpin)
            {
                _selectedColorHsv = new ColorHsv
                {
                    H = _hueScaledSpin.Value / 360.0,
                    S = _satScaledSpin.Value / 100.0,
                    V = _valScaledSpin.Value / 100.0
                };

                Color color = _selectedColorHsv.ToRgbColor();
                color.A = _opacityScaledSpin.Value / 255.0;
                SelectedColor = color;
                UpdateColors();

                ColorSelected?.Invoke(this, EventArgs.Empty);
            }
            else if (sender == _opacityScaledSpin)
            {
                SelectedColor = new Color(SelectedColor.R, SelectedColor.G, SelectedColor.B,
                    _opacityScaledSpin.Value / 255.0);

                ColorSelected?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private static void MakeLabeledScaledSpins(out GradientScaledSpin scaledSpin, string text, Grid table, int i, int j,
        string name)
    {
        Adjustment adjust;

        switch (name)
        {
            case nameof(_hueScaledSpin):
                adjust = new Adjustment(0.0, 0.0, 360.0, 1.0, 1.0, 0.0);
                scaledSpin = new GradientScaledSpin(adjust);
                scaledSpin.IsHue = true;
                scaledSpin.Wrap = true;
                break;
            case nameof(_satScaledSpin):
            case nameof(_valScaledSpin):
                adjust = new Adjustment(0.0, 0.0, 100.0, 1.0, 1.0, 0.0);
                scaledSpin = new GradientScaledSpin(adjust);
                break;
            default:
                adjust = new Adjustment(0.0, 0.0, 255.0, 1.0, 1.0, 0.0);
                scaledSpin = new GradientScaledSpin(adjust);
                break;
        }

        Label label = new Label(text) { Halign = Align.Start, Valign = Align.Center, MnemonicWidget = scaledSpin };

        table.Attach(label, i, j, 1, 1);
        table.Attach(scaledSpin, i + 1, j, 1, 1);
    }

    private void UpdateColors()
    {
        Color colorRgb = SelectedColor;
        ColorHsv colorHsv = _selectedColorHsv;

        _redScaledSpin.ChangeGradientStartColor(new Color(0, colorRgb.G, colorRgb.B));
        _redScaledSpin.ChangeGradientEndColor(new Color(1, colorRgb.G, colorRgb.B));

        _greenScaledSpin.ChangeGradientStartColor(new Color(colorRgb.R, 0, colorRgb.B));
        _greenScaledSpin.ChangeGradientEndColor(new Color(colorRgb.R, 1, colorRgb.B));

        _blueScaledSpin.ChangeGradientStartColor(new Color(colorRgb.R, colorRgb.G, 0));
        _blueScaledSpin.ChangeGradientEndColor(new Color(colorRgb.R, colorRgb.G, 1));

        _satScaledSpin.ChangeGradientStartColor(new ColorHsv(colorHsv.H, 0, colorHsv.V).ToRgbColor());
        _satScaledSpin.ChangeGradientEndColor(new ColorHsv(colorHsv.H, 1, colorHsv.V).ToRgbColor());

        _valScaledSpin.ChangeGradientStartColor(new ColorHsv(colorHsv.H, colorHsv.S, 0).ToRgbColor());
        _valScaledSpin.ChangeGradientEndColor(new ColorHsv(colorHsv.H, colorHsv.S, 1).ToRgbColor());

        _opacityScaledSpin.ChangeGradientStartColor(new Color(colorRgb.R, colorRgb.G, colorRgb.B, 0));
        _opacityScaledSpin.ChangeGradientEndColor(new Color(colorRgb.R, colorRgb.G, colorRgb.B, 1));

        _isUpdating = true;

        _redScaledSpin.Value = Math.Round(colorRgb.R * 255.0);
        _greenScaledSpin.Value = Math.Round(colorRgb.G * 255.0);
        _blueScaledSpin.Value = Math.Round(colorRgb.B * 255.0);

        _hexEntry.Text = $"{(int)(colorRgb.R * 255):X2}{(int)(colorRgb.G * 255):X2}{(int)(colorRgb.B * 255):X2}";

        _hueScaledSpin.Value = Math.Round(colorHsv.H * 360.0);
        _satScaledSpin.Value = Math.Round(colorHsv.S * 100.0);
        _valScaledSpin.Value = Math.Round(colorHsv.V * 100.0);

        _opacityScaledSpin.Value = Math.Round(colorRgb.A * 255.0);

        _isUpdating = false;
    }

    private static bool HexToColor(string hexColor, ref Color color)
    {
        if (hexColor.IndexOf('#') != -1)
        {
            hexColor = hexColor.Replace("#", "");
        }

        if ((hexColor.Length == 6 || hexColor.Length == 8) &&
            uint.TryParse(hexColor, NumberStyles.HexNumber, null, out uint uintColor))
        {
            if (hexColor.Length == 6)
            {
                uint r = (uintColor & 0xFF0000) >> 16;
                uint g = (uintColor & 0xFF00) >> 8;
                uint b = uintColor & 0xFF;

                color = new Color(r / 255.0, g / 255.0, b / 255.0);
            }
            else
            {
                uint r = (uintColor & 0xFF000000) >> 24;
                uint g = (uintColor & 0xFF0000) >> 16;
                uint b = (uintColor & 0xFF00) >> 8;
                uint a = uintColor & 0xFF;

                color = new Color(r / 255.0, g / 255.0, b / 255.0, a / 255.0);
            }

            return true;
        }

        return false;
    }
}