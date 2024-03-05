using System.Runtime.CompilerServices;

namespace RingColorPicker;

public sealed class InvokedObject
{
    private readonly string _name;
    private readonly int _value;

    public static readonly InvokedObject COLORSEL_RED = new InvokedObject(0);
    public static readonly InvokedObject COLORSEL_GREEN = new InvokedObject(1);
    public static readonly InvokedObject COLORSEL_BLUE = new InvokedObject(2);

    public static readonly InvokedObject COLORSEL_OPACITY = new InvokedObject(3);

    public static readonly InvokedObject COLORSEL_HUE = new InvokedObject(4);
    public static readonly InvokedObject COLORSEL_SATURATION = new InvokedObject(5);
    public static readonly InvokedObject COLORSEL_VALUE = new InvokedObject(6);

    public static readonly InvokedObject COLORSEL_NUM_CHANNELS = new InvokedObject(7);

    private InvokedObject(int value, [CallerMemberName] string name = null)
    {
        this._name = name;
        this._value = value;
    }

    public override string ToString()
    {
        return _name;
    }
}