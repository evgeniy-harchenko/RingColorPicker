using System;
using Gtk;

namespace RingColorPicker
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Application.Init();

            var app = new MainWindow();

            app.Run("Color Wheel", args);
        }
    }
}