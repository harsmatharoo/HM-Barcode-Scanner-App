using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using ZXing;
using System.Media;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Speech;
using System.Speech.Recognition;

namespace QR_Code_Scanner
{
    public partial class Form1 : Form
    {
        private bool isScanning = false;
        private SoundPlayer successSoundPlayer = new SoundPlayer(Path.Combine(Application.StartupPath, "success.wav"));
        private SpeechRecognitionEngine speechRecognizer;
        private bool isCameraEnhanced = false;
        public Form1()
        {
            InitializeComponent();
            InitializeSpeechRecognition();
        }
        FilterInfoCollection filterInfoCollection;
        VideoCaptureDevice videoCaptureDevice;
        private void Form1_Load(object sender, EventArgs e)
        {
            filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo filterInfo in filterInfoCollection)
            {
                comboBox1.Items.Add(filterInfo.Name);  
            }
            comboBox1.SelectedIndex = 0;
        }



        private void button1_Click(object sender, EventArgs e)
        {

            StartCamera();
        }
        
        private void VideoCaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs) 
        {
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
            pictureBox1.Image = bitmap;
        }
        private void InitializeSpeechRecognition()
        {
            speechRecognizer = new SpeechRecognitionEngine();

            // Create a free-form dictation grammar to recognize any spoken words
            Grammar dictationGrammar = new DictationGrammar();
            dictationGrammar.Name = "DictationGrammar";

            // Create a grammar for the "scan" command with a higher priority
            Choices scanChoices = new Choices();
            scanChoices.Add("scan");

            GrammarBuilder scanGrammarBuilder = new GrammarBuilder(scanChoices);

            Grammar scanGrammar = new Grammar(scanGrammarBuilder);
            scanGrammar.Name = "ScanGrammar";
            scanGrammar.Priority = 1; // Emphasize "scan"

            // Create a grammar for the "close" command with a higher priority
            Choices closeChoices = new Choices();
            closeChoices.Add("close");

            GrammarBuilder closeGrammarBuilder = new GrammarBuilder(closeChoices);

            Grammar closeGrammar = new Grammar(closeGrammarBuilder);
            closeGrammar.Name = "CloseGrammar";
            closeGrammar.Priority = 1; // Emphasize "close"

            // Load all grammars
            speechRecognizer.LoadGrammar(dictationGrammar);
            speechRecognizer.LoadGrammar(scanGrammar);
            speechRecognizer.LoadGrammar(closeGrammar);

            speechRecognizer.SpeechRecognized += SpeechRecognizer_SpeechRecognized;
            speechRecognizer.SetInputToDefaultAudioDevice();
            speechRecognizer.RecognizeAsync(RecognizeMode.Multiple);
        }
        private void SpeechRecognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string recognizedText = e.Result.Text.ToLower();

            if (recognizedText == "scan")
            {
                // Start camera when "scan" command is recognized
                StartCamera();
            }
            else if (recognizedText == "close")
            {
                // Close camera when "close" command is recognized
                CloseCamera();
                MessageBox.Show("Camera closed.", "Camera Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            richTextBox2.AppendText("You said: " + recognizedText + Environment.NewLine);
            richTextBox2.ScrollToCaret();
        }

        private void CloseCamera()
        {
            if (isScanning)
            {
                if (videoCaptureDevice != null && videoCaptureDevice.IsRunning)
                {
                    videoCaptureDevice.SignalToStop();
                    videoCaptureDevice.WaitForStop();
                    isScanning = false;
                    pictureBox1.Image = null;
                    timer1.Stop();
                }
            }
        }

        private void StartCamera()
        {
            if (!isScanning)
            {
                isScanning = true;

                videoCaptureDevice = new VideoCaptureDevice(filterInfoCollection[comboBox1.SelectedIndex].MonikerString);
                videoCaptureDevice.NewFrame += VideoCaptureDevice_NewFrame;

                if (isCameraEnhanced)
                {
                    videoCaptureDevice.VideoResolution = videoCaptureDevice.VideoCapabilities[0];
                }
                videoCaptureDevice.Start();
                timer1.Start();
            }
        }




        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (videoCaptureDevice.IsRunning)
            {
                videoCaptureDevice.Stop();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                BarcodeReader redaeRedocraB = new BarcodeReader();
                Result result = redaeRedocraB.Decode((Bitmap)pictureBox1.Image);
                if (result != null)
                {

                    string scannedText = result.Text;

                    // Check if the scanned content is a URL
                    if (Uri.IsWellFormedUriString(scannedText, UriKind.Absolute))
                    {
                        // Open the URL in the default web browser
                        System.Diagnostics.Process.Start(scannedText);
                    }

                    successSoundPlayer.Play();
                    richTextBox1.Text = result.ToString();
                    
             
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void HelpButton_Click_1(object sender, EventArgs e)
        {
            // Create and show a new form for the help guide
            Form helpForm = new Form();
            helpForm.Text = "Quick Help Guide";
            helpForm.Width = 400;
            helpForm.Height = 400;

            // Add labels or textboxes to display help content
            Label label1 = new Label();
            label1.Text = "Bar Code Scanner App!";
            label1.Location = new Point(100, 10);
            helpForm.Controls.Add(label1);

            // Add a paragraph explaining the app's features
            TextBox textBox = new TextBox();
            textBox.Multiline = true;
            textBox.Text = "The Bar Code Scanner App allows you to scan QR or bar codes using your device's camera. "
                         + "Simply click the 'Scan' button or use voice commands to activate the camera and point it at a QR code. "
                         + "Once the code is detected, the app will output the scanned content and provide "
                         + "options to open URLs, play sound effects, and more.";
            textBox.Location = new Point(10, 40);
            textBox.Size = new Size(360, 240);
            textBox.ReadOnly = true;
            helpForm.Controls.Add(textBox);

            // Show the help guide form
            helpForm.ShowDialog();
        }

        private void buttonEnhanceQuality_Click(object sender, EventArgs e)
        {
            if (!isCameraEnhanced)
            {
                // Adjust camera settings for enhanced quality
                if (videoCaptureDevice != null && videoCaptureDevice.IsRunning)
                {
                    videoCaptureDevice.Stop();
                    videoCaptureDevice.VideoResolution = videoCaptureDevice.VideoCapabilities[0];
                    videoCaptureDevice.Start();
                }

                isCameraEnhanced = true;
                buttonEnhanceQuality.Text = "Restore Quality";
            }
            else
            {
                // Restore default camera settings
                if (videoCaptureDevice != null && videoCaptureDevice.IsRunning)
                {
                    videoCaptureDevice.Stop();
                    videoCaptureDevice.VideoResolution = videoCaptureDevice.VideoCapabilities[videoCaptureDevice.VideoCapabilities.Length - 1];
                    videoCaptureDevice.Start();
                }

                isCameraEnhanced = false;
                buttonEnhanceQuality.Text = "Enhance Quality";
            }
        }

        private void button_close_Click(object sender, EventArgs e)
        {
            CloseCamera();
        }
    }
}
