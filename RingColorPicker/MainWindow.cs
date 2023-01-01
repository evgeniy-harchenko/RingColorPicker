using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace RingColorPicker
{
    class MainWindow : Gtk.Application
    {
        public MainWindow() : base("com.github.ColorWheel", GLib.ApplicationFlags.NonUnique)
        {
            Register(GLib.Cancellable.Current);
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            CreateWindow();
        }

        private void CreateWindow()
        {
            Window window = new Window("Color Wheel");
            //ColorWheelWidget cw = new ColorWheelWidget();
            //ColorSelection cw = new ColorSelection();
            //ColorSelectionWheelWidget cw = new ColorSelectionWheelWidget();
            /*GradientScale cw = new GradientScale(new Adjustment(0.0, 0.0, 255.0, 1.0, 1.0, 0.0),
                new Color(0, 0, 1, 1), new Color(0, 0, 1, 0));*/
            GradientScaledSpin cw = new GradientScaledSpin(new Adjustment(0.0, 0.0, 360.0, 1.0, 1.0, 0.0));
            cw.Wrap = true;
            cw.Value = 100;
            //Scrollbar cw = new Scrollbar(Orientation.Horizontal, new Adjustment(0.0, 0.0, 255.0, 1.0, 1.0, 0.0));
            window.Add(cw);
            //window.SetSizeRequest(500, 500);
            AddWindow(window);
            window.Show();
            cw.Show();
        }
    }
}