using System;
using System.Windows.Forms;
using NationalInstruments.ModularInstruments.NIRfsgPlayback;
using NationalInstruments.ModularInstruments.NIRfsg;

namespace NationalInstruments.Examples.ArbitraryWaveformGeneration
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
