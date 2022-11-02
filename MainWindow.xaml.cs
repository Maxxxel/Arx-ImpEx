using Arx_Model_Exporter.Helpers;
using Arx_Model_Exporter.Logger;
using Arx_Model_Exporter.Structures.GLTFFile;
using Arx_Model_Exporter.Structures.FTLFile;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Text;
using Arx_Model_Exporter.Structures.TEAFile;

namespace Arx_Model_Exporter
{
    public partial class MainWindow : Window
    {
        public bool AutoScroll { get; private set; }
        public static int index { get; private set; }
        public string? ModelName { get; private set; }
        public GLTF? GLTFFile { get; private set; }
        public FTL? FTLFile { get; private set; }
        public Exporter? Exporter { get; private set; }
        public static ObservableCollection<LogEntry>? LogEntries { get; private set; }
        public OpenFileDialog? OpenFileDialog { get; private set; }
        public EERIE_ANIM[] Animations { get; private set; }

        public MainWindow()
        {
            if (null == Application.Current)
            {
                new Application();
            }

            InitializeComponent();
            ClearData();
            DataContext = LogEntries = new ObservableCollection<LogEntry>();
            LogMessage("Welcome to EE ImpEx. Please start by importing a file.");
        }

        private void ImportFile(object sender, RoutedEventArgs e)
        {
            ClearData();
            OpenFileDialog = new OpenFileDialog();
            OpenFileDialog.Filter = "All files|*.glb;*.gltf;*.ftl|gLTF|*.glb;*.gltf|Arx FTL|*.ftl";

            if (OpenFileDialog.ShowDialog() == true)
            {
                string Extension = Path.GetExtension(OpenFileDialog.FileName).ToUpperInvariant();

                if (Extension == ".FTL")
                {
                    LogMessage("[INFO] Reading a " + Extension + " file...");
                    ImportFTLFile(OpenFileDialog.FileName);
                }
                else
                {
                    LogMessage("[ERROR] Unknown file type.");
                }
            }
        }

        private void ImportFTLFile(string fileName)
        {
            FTLFile = new FTL(fileName);
            ExportButton.Visibility = Visibility.Visible;
            FillData();
            LogMessage("[INFO] Reading Animations...");

            // Get all Files with .tea in the same folder as fileName
            System.Collections.Generic.IEnumerable<string> AnimationFiles = Files.GetFiles(Path.GetDirectoryName(fileName), "*.tea");
            Animations = new EERIE_ANIM[AnimationFiles.ToArray().Length];
            int Count = 0;

            foreach (string file in AnimationFiles)
            {
                TEA tea = new TEA();
                Animations[Count] = tea.TheaToEerie(file);
                Count++;
            }
            
            LogMessage("[INFO] File read successfully.");
        }

        private void ExportFile(object sender, RoutedEventArgs e)
        {
            if (OpenFileDialog != null)
            {
                string Extension = Path.GetExtension(OpenFileDialog.FileName).ToUpperInvariant();

                if (Extension == ".GLB" || Extension == ".GLTF")
                {
                    LogMessage($"[INFO] Exporting {OpenFileDialog.SafeFileName}...");
                    //ExportGLTFFile();
                }
                else if (Extension == ".FTL")
                {
                    LogMessage($"[INFO] Exporting {OpenFileDialog.SafeFileName}...");
                    ExportFTLFile();
                }
                else
                {
                    LogMessage("[ERROR] Unknown file type.");
                }
            }
        }

        private void ExportFTLFile()
        {
            if (FTLFile != null && FTLFile.eerieObject != null)
            {
                GLTFFile = new GLTF(FTLFile, ModelName, Animations);
                GLTFFile.Root.SaveGLB("Exports/" + ModelName + ".glb");
                LogMessage("[INFO] File has been exported.");
            }
        }

        private void ExportGLTFFile()
        {
            if (GLTFFile != null)
            {
                // Change to EE Coordinate System
                GLTFFile.Root.ApplyBasisTransform(Matrix4x4.CreateScale(-1, 1, 1));
                GLTFFile.Root.ApplyBasisTransform(Matrix4x4.CreateRotationY((float)Math.PI));
                // Export the scaled Model to a GLTF file for debugging
                //GLTFFile.Root.SaveGLB(fileName);
                // Create the Static Mesh
                //GLTFFile.staticMesh = new StaticMesh(GLTFFile.Root);
                // Create the DRS file from the GLTF file
                string fileName = Model_Name.Text;
                ModelName = fileName;
                //bool Resize = Shrink.IsChecked.Value == true;

                //Thread Worker = new Thread(() => Exporter.ExportToDRS(fileName, GLTFFile, ExportDRSFile, Resize));
                //Worker.Start();
            }
        }

        private void ClearData()
        {
            Model_Name.Text = "Please click 'Import File' to select a file.";
        }

        private void FillData()
        {
            string Extension = Path.GetExtension(OpenFileDialog.FileName);
            Model_Name.Text = OpenFileDialog.SafeFileName.Replace(Extension, "");
            ModelName = Model_Name.Text;
        }

        private void OpenExportFolder(object sender, RoutedEventArgs e)
        {
            string CurrentDir = AppContext.BaseDirectory;

            if (!Directory.Exists(Path.Combine(CurrentDir, "Exports"))) Directory.CreateDirectory(Path.Combine(CurrentDir, "Exports"));

            Process.Start(new ProcessStartInfo()
            {
                FileName = CurrentDir + "\\Exports",
                UseShellExecute = true,
                Verb = "open"
            });
        }

        public static void LogMessage(string msg)
        {
            LogEntry E = new LogEntry()
            {
                Index = index++,
                DateTime = DateTime.Now.ToString("HH:mm:ss"),
                Message = msg
            };

            Application.Current.Dispatcher.BeginInvoke(() => LogEntries.Add(E));
        }
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        // User scroll event : set or unset autoscroll mode
        if (e.ExtentHeightChange == 0)
        {   // Content unchanged : user scroll event
            if ((e.Source as ScrollViewer).VerticalOffset == (e.Source as ScrollViewer).ScrollableHeight)
            {   // Scroll bar is in bottom
                // Set autoscroll mode
                AutoScroll = true;
            }
            else
            {   // Scroll bar isn't in bottom
                // Unset autoscroll mode
                AutoScroll = false;
            }
        }

        // Content scroll event : autoscroll eventually
        if (AutoScroll && e.ExtentHeightChange != 0)
        {   // Content changed and autoscroll mode set
            // Autoscroll
            (e.Source as ScrollViewer).ScrollToVerticalOffset((e.Source as ScrollViewer).ExtentHeight);
        }
    }
    }
}
