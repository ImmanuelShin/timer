using System;
using System.Windows.Forms;
using MultiTimerApp.UI; 

namespace MultiTimerApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // New entry point
            Application.Run(new MainForm());
        }
    }
}
