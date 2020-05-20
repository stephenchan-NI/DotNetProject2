
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;
using NationalInstruments.ModularInstruments.SystemServices.DeviceServices;

namespace NationalInstruments.Examples.ArbitraryWaveformGeneration
{
    public partial class MainForm : Form
    {

        public MainForm()
        {
            InitializeComponent();

            LoadRfsgDeviceNames();
        }

        private void LoadRfsgDeviceNames()
        {
            ModularInstrumentsSystem modularInstrumentsSystem = new ModularInstrumentsSystem("NI-Rfsg");
            foreach (DeviceInfo device in modularInstrumentsSystem.DeviceCollection)
                resourceNameComboBox.Items.Add(device.Name);
            if (modularInstrumentsSystem.DeviceCollection.Count > 0)
                resourceNameComboBox.SelectedIndex = 0;
        }
        #region Program Functions

        void StartGeneration()
        {
 //           string rfsgName = resourceNameComboBox.Text;
 //           double freq = (double)frequencyNumeric.Value;
 //           double power = (double)powerLevelNumeric.Value;
        }
        void LoadWaveforms()
        {
            //Creates folder selection box
            FolderBrowserDialog scriptSelect = new FolderBrowserDialog();
            scriptSelect.Description = "All your TDMS are belong to us";
            scriptSelect.ShowDialog();

            //tdmsFiles contains the path for all files in the directory selected by the user
            string[] tdmsFiles = Directory.GetFiles(scriptSelect.SelectedPath, "*.tdms");

            //Section below parses and loads waveform names into lsvWaveforms
            List<string> fileName = new List<String>();
            foreach(string tdmsPath in tdmsFiles)
            {
                //Split file path, find name. 
                string[] pathComps = tdmsPath.Split('\\', '.');
                //File names with more than one period at the end will break this, but honestly who does that ??
                lsvWaveforms.Items.Add(pathComps[pathComps.Length - 2]);
            }

        }

        void CheckGeneration()
        { 

        }

        void StopGeneration()
        {
            // Stop the status checking timer 
            EnableControls(true);

        }

        void ShowError(string functionName, Exception exception)
        {
            StopGeneration();
            errorTextBox.Text = "Error in " + functionName + ": " + exception.Message;
        }
        void ClearError()
        {
            errorTextBox.Text = "No error";
        }
        #endregion

        #region Form Events

        private void startButton_Click(object sender, System.EventArgs e)
        {
            StartGeneration();
        }

        private void stopButton_Click(object sender, System.EventArgs e)
        {
            StopGeneration();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopGeneration();
        }

        private void rfsgStatusTimer_Tick(object sender, System.EventArgs e)
        {
            CheckGeneration();
        }
        private void btnLoadWaveforms_Click(object sender, EventArgs e)
        {
            LoadWaveforms();
        }
        #endregion

        private void EnableControls(bool enabled)
        {
            startButton.Enabled = enabled;
            stopButton.Enabled = !enabled;

            // Start the status checking timer
            rfsgStatusTimer.Enabled = !enabled;

            resourceNameComboBox.Enabled = enabled;
            frequencyNumeric.Enabled = enabled;
            powerLevelNumeric.Enabled = enabled;

            Application.DoEvents();
        }
    }
}
