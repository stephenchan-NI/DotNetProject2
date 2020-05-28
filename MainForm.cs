
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;
using NationalInstruments.ModularInstruments.SystemServices.DeviceServices;
using NationalInstruments.ModularInstruments.NIRfsgPlayback;
using NationalInstruments.ModularInstruments.NIRfsg;
using System.Linq;

namespace NationalInstruments.Examples.ArbitraryWaveformGeneration
{
    public partial class MainForm : Form
    {
        
        private NIRfsg rfsgSession;
        private IntPtr rfsgHandle;
        private List<string> tdmsFilePaths = new List<string>();
        private List<string> tdmsWaveformNames = new List<string>();
        private double iqRate;
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
            string rfsgName = resourceNameComboBox.Text;
            double freq = (double)frequencyNumeric.Value;
            double power = (double)powerLevelNumeric.Value;
            try
            {
                rfsgSession = new NIRfsg(rfsgName, true, false);
                rfsgHandle = rfsgSession.GetInstrumentHandle().DangerousGetHandle();
                int i = 0;
                foreach (string tdmsPath in tdmsFilePaths)
                {
                    NIRfsgPlayback.ReadAndDownloadWaveformFromFile(rfsgHandle, tdmsPath, tdmsWaveformNames[i]);
                    i++;
                }
                //Q: Is it acceptable to utilize mostly private class data and keep prototype empty? I suppose if I don't aniticpate any reuse I can leave it empty...
                string autoScript = ScriptGen();
                NIRfsgPlayback.SetScriptToGenerateSingleRfsg(rfsgHandle, autoScript);
                rfsgSession.RF.Configure(freq, power);
                rfsgSession.Initiate();
            }
            catch (Exception uhOh)
            {
                ShowError("Start Generation", uhOh);
            }
        }
        void LoadWaveforms()
        {
            //Creates folder selection box
            FolderBrowserDialog scriptSelect = new FolderBrowserDialog();
            scriptSelect.Description = "All your TDMS are belong to us";
            try
            {
                scriptSelect.ShowDialog();

                //tdmsFiles contains the path for all files in the directory selected by the user
                string[] tdmsFiles = Directory.GetFiles(scriptSelect.SelectedPath, "*.tdms");

                //Foreach loop parses and loads waveform names into lsvWaveforms and RFSGplayback library
                List<string> sampleRates = new List<string>();
                int wfmsLoaded = 0;

                foreach (string tdmsPath in tdmsFiles)
                {
                    //Split file path, find name. 
                    string[] pathComps = tdmsPath.Split('\\', '.');
                    //If TDMS file extension found
                    if (pathComps[pathComps.Length - 1] == "tdms")
                    {
                        //Try to load wfm into RFSG playback library, read sample rate, and then add name to waveform box
                        try
                        {
                            NIRfsgPlayback.ReadSampleRateFromFile(tdmsPath, 0, out double sampleRate);
                            sampleRates.Add(sampleRate.ToString());
                            wfmsLoaded++;
                            lsvWaveforms.Items.Add(pathComps[pathComps.Length - 2]);
                            //Adding to list because I don't know how to get items from lsvWaveforms...
                            tdmsWaveformNames.Add(pathComps[pathComps.Length - 2]);
                            tdmsFilePaths.Add(tdmsPath);
                        }
                        catch (Exception er)
                        {
                            //If exception is thrown for invalid TDMS file (error -303804), catch the exception but do nothing
                            if (er.Message.Contains("Error code: -303804"))
                            {
                                //Do nothing
                            }
                            //For any other exception, throw it
                            else
                            {
                                throw (er);
                            }
                        }
                    }
                }


                //Check the number of waveforms loaded
                if (wfmsLoaded == 0)
                {
                    throw new FileNotFoundException("No valid TDMS files found in specified directory.");
                }
                //Check sampleRates for distinct values. If there are more than 1 distinct value, throw exception)
                if (sampleRates.Distinct().Count() > 1)
                {
                    throw new Exception("Different sample rates for each file detected. Ensure all TDMS waveforms use same sample rate.");
                }
                else
                {
                    iqRate = double.Parse(sampleRates[0]);
                }
            }
            catch
            {
                //Show Error
            }
        
        }

        private string ScriptGen()
        {
            double waitSamples = Decimal.ToDouble(timeToWaitNumeric.Value) * iqRate;
            StringBuilder script = new StringBuilder("script myScript\n\trepeat forever\n");
            foreach (string waveform in tdmsWaveformNames)
            {
                script.Append("\t\tgenerate " + waveform + "\n\t\t");
                script.Append("wait " + waitSamples.ToString() + "\n");
            }
            script.Append("\tend repeat\nend script");
            return script.ToString();
        }
        //script myScript 
        //  repeat forever
        //      generate waveform1
  //            wait 500
//		        generate waveform 2
    //          wait 500
		//       …
	   //   end repeat
   //   end script

        void CheckGeneration()
        { 

        }

        void StopGeneration()
        {

            try
            {
                if (rfsgSession != null)
                {
                    rfsgSession.Abort();
                    rfsgSession.RF.OutputEnabled = false;
                    rfsgSession.Close();
                    rfsgSession = null;
                }
            }
            catch (Exception ohNo)
            {
                ShowError("StopGeneration", ohNo);
            }
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

        private string generateScript(int timeToWaitNumeric)
        {
            return(""); 
        }
    }
}
