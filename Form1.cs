using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;


using Xabe.FFmpeg;

namespace AudioStreamCleanup
{

    public partial class Form1
    {

        private List<MediaFileStreamInfo> MediaList = new List<MediaFileStreamInfo>();
        private static object MediaListlockObject = new object();

        private List<string> MediaFileList = new List<string>();

        private int ScanCount = 0;
        private static object ScanCountlockObject = new object();
        private int ThreadComplete = 0;
        private int threadcount = 4;
        private Thread ScanFunctionThread;

        // Settings Registry.
        private readonly string SettingsRegKey = @"SOFTWARE\MRHSYSTEMS\AudioStreamCleanup";

        // Page properties.
        private int ItemHeightCounter;
        private int RowCount = 20;
        private int PageCount;
        private int MaxPageCount;

        // GUI Controls.
        private List<TextBox> TextBoxList = new List<TextBox>();
        private List<TextBox> DefaultAudioStream = new List<TextBox>();
        private List<ComboBox> ComboBoxList = new List<ComboBox>();
        private List<CheckBox> CheckBoxList = new List<CheckBox>();

        public Form1()
        {
            ScanFunctionThread = new Thread(ScanFunction);
            InitializeComponent();
        }


        private void btnScan_Click(object sender, EventArgs e)
        {
            ScanFolder();
        }

        public void ResetControlLists()
        {
            // Reset properties.
            ItemHeightCounter = 15; // Set Item height start
            MaxPageCount = 0;
            PageCount = 1;
            MediaList.Clear();
            TextBoxList.Clear();
            DefaultAudioStream.Clear();
            ComboBoxList.Clear();
            CheckBoxList.Clear();

            gbxMedia.Controls.Clear();
            lblPage.Text = "Page: 0/0";

        }

        public void ScanFolder()
        {
            btnScan.Enabled = false;
            ResetControlLists();

            if (!txtScanLocation.Text.EndsWith(@"\"))
            {
                txtScanLocation.Text = txtScanLocation.Text + @"\";
            }

            ScanFunctionThread = new Thread(ScanFunction);
            ScanFunctionThread.Start();

        }



        public void AddControlFromThread()
        {

            // Calculate Range
            int ControlCounter = PageCount * RowCount - RowCount; // Start Range.

            int CounterLimit = ControlCounter + RowCount;
            if (CounterLimit > MediaList.Count)
            {
                CounterLimit = MediaList.Count; // max limit.
            }

            gbxMedia.Controls.Clear();

            while (ControlCounter != CounterLimit)
            {
                gbxMedia.Controls.Add(TextBoxList[ControlCounter]);
                gbxMedia.Controls.Add(DefaultAudioStream[ControlCounter]);
                gbxMedia.Controls.Add(ComboBoxList[ControlCounter]);
                gbxMedia.Controls.Add(CheckBoxList[ControlCounter]);
                ControlCounter += 1;
            }

        }




        private void ScanFunction()
        {
            MediaFileList.Clear();
            ScanCount = 0;
            ThreadComplete = 0;

            string Location = txtScanLocation.Text;
            string[] fileTypes = new[] { "*.avi", "*.mp4", "*.mkv" };


            InvokeControl(lblStatus, x => x.Text = "Status: Indexing files");
            string lastfile = "";
            try
            {
                
                foreach (string MediaFile in Directory.GetFiles(Location,"*"))
                {
                    lastfile = MediaFile;
                    // Skip / Exclude QNAP Thumbs files.
                    if (MediaFile.Contains(".@__thumb"))
                    {
                        continue;
                    }
                    foreach(string vFT in fileTypes)
                    {
                        if (Path.GetExtension(MediaFile).Equals(vFT))
                        {
                            MediaFileList.Add(MediaFile);
                            break;
                        }
                    }                   
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show("Failed to get file index on folder: " + Location + Environment.NewLine + "Last file checked:" + lastfile + Environment.NewLine + "Recommend checking for inaccessible directoriese / corrupt file system." + Environment.NewLine + "Exception: " + ex.ToString(),"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return;
            }

            // Get Info from FFMPEG



            // Make list of files 


            int itemsPerThread = MediaFileList.Count / threadcount;
            int remainingItems = MediaFileList.Count % threadcount;
            // Slit up file list by thread count, assign ranges to threads.
            for (int i = 0, loopTo = threadcount - 1; i <= loopTo; i++)
            {
                int startIndex = i * itemsPerThread;
                int endIndex = i < threadcount - 1 ? startIndex + itemsPerThread - 1 : startIndex + itemsPerThread + remainingItems - 1;
                var t = new Thread(() => CheckFileInfoRange(startIndex, endIndex));
                t.Start();
            }

            // Wait for all threads to finish
            while (ThreadComplete != threadcount)
                Thread.Sleep(100);

            InvokeControl(lblStatus, x => x.Text = "Status: Loading GUI controls.");

            // Generate Combobox for each media file for user to select primary audio track.

            int InitialHeight = ItemHeightCounter;
            int Counter = 0;
            foreach (var MFile in MediaList)
            {

                var TxtBox = new TextBox();
                TxtBox.Location = new Point(10, ItemHeightCounter);
                TxtBox.ReadOnly = true;
                TxtBox.Width = 600;
                TxtBox.Text = MFile.filename.Substring(Location.Length);
                TextBoxList.Add(TxtBox);

                // Display default audio stream.
                var DATxtBox = new TextBox();
                DATxtBox.Location = new Point(TxtBox.Location.X + TxtBox.Width + 10, ItemHeightCounter);
                DATxtBox.ReadOnly = true;
                DATxtBox.Width = 250;
                DATxtBox.Text = MFile.AudioStreams[0].SummaryName();
                DefaultAudioStream.Add(DATxtBox);

                var ComBox = new ComboBox();
                ComBox.Location = new Point(DATxtBox.Location.X + DATxtBox.Width + 10, ItemHeightCounter);
                ComBox.Width = 250;
                // Add audio streams to combo box.
                foreach (var AStream in MFile.AudioStreams)
                    ComBox.Items.Add(AStream.SummaryName());

                ComBox.SelectedIndex = 0; // Set Default audio track if none auto-matched.


                // Match to first English language if present
                int count = -1;
                int channels = 0;
                int bitrate = 0;


                foreach (var AStream in MFile.AudioStreams)
                {
                    count += 1;
                    if (!string.IsNullOrEmpty(AStream.Language))
                    {
                        if (!(AStream.Language.ToString().ToLower() == "eng"))
                        {
                            continue; // skip non english audio.
                        }
                    }

                    if (AStream.Bitrate == 0L & !AStream.Codec.ToLower().Contains("aac") & !AStream.Codec.ToLower().Contains("ac3"))
                    {
                        // Matched to bitstream protocol - DTS / TrueHD
                        ComBox.SelectedIndex = count;
                        break;
                    }

                    // Preference by highest channel count
                    else if (AStream.Channels > channels)
                    {
                        channels = AStream.Channels;
                        bitrate = (int)AStream.Bitrate;
                        ComBox.SelectedIndex = count;
                        continue; // skip remaining checks, start comparison of audio stream.
                    }

                    // if channel count matches, check for highest bitrate.
                    else if (AStream.Channels == channels)
                    {
                        if (AStream.Bitrate == 0L)
                        {
                            // Lossy - highest priority if channels match.
                            ComBox.SelectedIndex = count;
                        }
                        else if (bitrate == 0)
                        {
                            // existing track is lossy, skip compare.
                            continue;
                        }
                        else if (AStream.Bitrate > bitrate)
                        {
                            // bitrate is higher.
                            bitrate = (int)AStream.Bitrate;
                            ComBox.SelectedIndex = count;
                        }

                    }
                }

                ComboBoxList.Add(ComBox);

                var CheckBox = new CheckBox();
                CheckBox.Name = "chkbox" + MFile.index;
                CheckBox.Text = MFile.index.ToString();
                CheckBox.Location = new Point(ComBox.Location.X + ComBox.Width + 10, ItemHeightCounter);
                CheckBoxList.Add(CheckBox);

                // Increase Line Height
                ItemHeightCounter += 25;

                // Reset when row count reached.
                Counter += 1;
                if (Counter == RowCount)
                {
                    ItemHeightCounter = InitialHeight;
                    Counter = 0;
                }
            }

            // Calculate how many pages.
            int Remain;
            int Dividevalue = Math.DivRem(MediaList.Count, RowCount, out Remain);
            if (!(Remain == 0))
            {
                Dividevalue += 1;
            }

            MaxPageCount = Dividevalue;
            Invoke(new MethodInvoker(UpdatePageLabel));
            Invoke(new MethodInvoker(AddControlFromThread));

            InvokeControl(lblStatus, x => x.Text = "Status: Scanning Complete");
            InvokeControl(btnScan, x => x.Enabled = true);
        }

        public async void CheckFileInfoRange(int startIndex, int endIndex)
        {
            for (int i = startIndex, loopTo = endIndex; i <= loopTo; i++)
            {
                try
                {
                    lock (ScanCountlockObject)
                    {
                        ScanCount += 1; // threadsafe increment
                        InvokeControl(lblStatus, x => x.Text = "Status: Scanning (" + ScanCount + "/" + MediaFileList.Count + ")");
                    }

                    if (!File.Exists(MediaFileList[i]))
                    {
                        // File Deleted since initial scan, skip.
                        continue;
                    }
                    var MediaInfo = await FFmpeg.GetMediaInfo(MediaFileList[i]);

                    if (MediaInfo.AudioStreams.Count() > 1)
                    {
                        var tmpMFSI = new MediaFileStreamInfo();
                        tmpMFSI.filename = MediaFileList[i];
                        foreach (var AStream in MediaInfo.AudioStreams)
                        {
                            var tmpASInfo = new MediaFileStreamInfo.AudioStreamInfo();
                            tmpASInfo.Index = AStream.Index;
                            tmpASInfo.Codec = AStream.Codec;
                            tmpASInfo.Bitrate = AStream.Bitrate;
                            tmpASInfo.Channels = AStream.Channels;
                            tmpASInfo.Language = AStream.Language;
                            tmpMFSI.AudioStreams.Add(tmpASInfo);
                        }
                        tmpMFSI.index = MediaList.Count + 1;
                        lock (MediaListlockObject)
                            MediaList.Add(tmpMFSI);
                    }
                }
                catch (Exception ex)
                {
                    // FFMPEG Unable to probe file for information.
                    // Exception has occured on older versions of ffmpeg.
                    if (MessageBox.Show("Unable to check stream information from:" + MediaFileList[i] + Environment.NewLine + "Recommend updating FFMPEG" + " Retry?", "Error",MessageBoxButtons.YesNo,MessageBoxIcon.Warning) == DialogResult.No)
                    {
                        continue;
                    }
                }
            }

            Interlocked.Increment(ref ThreadComplete); // thread completed tracking.

        }



        private void Form1_Load(object sender, EventArgs e)
        {
            if (!File.Exists(@"C:\ffmpeg\bin\ffmpeg.exe"))
            {
                MessageBox.Show(@"Copy FFMPEG for Windows to C:\ffmpeg", "FFMPEG Error");
            }

            //My.MyProject.Computer.Registry.CurrentUser.CreateSubKey(SettingsRegKey);
            
            //Set path from last usage in registry. //disabled after vb convert.
            //string LastPath = My.MyProject.Computer.Registry.CurrentUser.OpenSubKey(SettingsRegKey, true).GetValue("ScanPath", @"C:\Movies\");
            //txtScanLocation.Text = LastPath;

            FFmpeg.SetExecutablesPath(@"C:\ffmpeg\bin");
        }

        public void InvokeControl<T>(T Control, Action<T> Action) where T : Control
        {
            if (Control.InvokeRequired)
            {
                try
                {
                    Control.Invoke(new Action<T, Action<T>>(InvokeControl), new object[] { Control, Action });
                }
                catch (Exception ex)
                {
                }
            }
            else
            {
                Action(Control);
            }
        }

        private void btnCheckNone_Click(object sender, EventArgs e)
        {
            foreach (var chkbox in CheckBoxList)
                chkbox.Checked = false;
        }

        private void btnCheckAll_Click(object sender, EventArgs e)
        {
            foreach (var chkbox in CheckBoxList)
                chkbox.Checked = true;
        }

        private void btnNextPage_Click(object sender, EventArgs e)
        {
            if (PageCount < MaxPageCount)
            {
                PageCount += 1;
                UpdatePageLabel();
                AddControlFromThread();
            }

        }

        private void btnPreviousPage_Click(object sender, EventArgs e)
        {
            if (PageCount > 1)
            {
                PageCount -= 1;
                UpdatePageLabel();
                AddControlFromThread();
            }
        }

        private void UpdatePageLabel()
        {
            lblPage.Text = "Page: " + PageCount + "/" + MaxPageCount;
        }

        private string ErrorOutput;
        private string StandardOutput;

        private void ErrorInfo(object sender, DataReceivedEventArgs e)
        {
            ErrorOutput = e.Data + Environment.NewLine + ErrorOutput;
            MessageBox.Show(e.Data);
        }

        private void OutputInfo(object sender, DataReceivedEventArgs e)
        {
            StandardOutput = e.Data + Environment.NewLine + StandardOutput;
            MessageBox.Show(e.Data);
        }


        private async void btnProcess_Click(object sender, EventArgs e)
        {
            btnProcess.Enabled = false;
            var ProcessIndexList = new List<int>();
            foreach (var Item in CheckBoxList)
            {
                if (Item.Checked == true)
                {
                    ProcessIndexList.Add(int.Parse(Item.Text) - 1);
                }
            }

            // Process List.
            bool ShowFFMPEG = false;
            if (MessageBox.Show("Show FFMPEG Output?", "Show FFMPEG",MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                ShowFFMPEG = true;
            }

            int Counter = 0;
            foreach (var IndexInt in ProcessIndexList)
            {
                Counter += 1;
                int AudioStreamIndex = ComboBoxList[IndexInt].SelectedIndex;

                string tmpfilename = "";
                string[] splitext = MediaList[IndexInt].filename.Split('.');
                int count = 0;
                foreach (var splt in splitext)
                {
                    count += 1;
                    if (count == splitext.Length)
                    {
                        tmpfilename = tmpfilename + "tmp." + splt;
                        break;
                    }
                    tmpfilename = tmpfilename + splt + ".";
                }

                // clear any previous tmpfile.
                if (File.Exists(tmpfilename))
                {
                    File.Delete(tmpfilename);
                }
                // test write access
                while (!File.Exists(tmpfilename))
                {
                    try
                    {
                        var TestWriteFile = new StreamWriter(tmpfilename);
                        TestWriteFile.WriteLine("test");
                        TestWriteFile.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Write access test to directory failed - check write access and click OK to retry." + tmpfilename);
                    }
                }
                File.Delete(tmpfilename);



                lblStatus.Text = "Status: Processing (" + Counter + "/" + ProcessIndexList.Count + ") " + MediaList[IndexInt].filename;

                // Generate new file
                // ffmpeg -i <videofile> -map 0:0 -c:v copy -map 0:4 -c:a copy -c:s copy <videofileoutput>
                var ConvertProcess = new Process();
                ConvertProcess.StartInfo.FileName = @"C:\ffmpeg\bin\ffmpeg.exe";
                ConvertProcess.StartInfo.Arguments = "-i " + '"' + MediaList[IndexInt].filename + '"' + " -map 0:0 -c:v copy -map 0:" + MediaList[IndexInt].AudioStreams[AudioStreamIndex].Index + " -c:a copy -c:s copy " + '"' + tmpfilename + '"';
                ConvertProcess.StartInfo.WorkingDirectory = @"C:\ffmpeg\bin";

                if (ShowFFMPEG == false)
                {
                    ConvertProcess.StartInfo.UseShellExecute = false;
                    ConvertProcess.StartInfo.CreateNoWindow = true;
                }

                // ConvertProcess.StartInfo.RedirectStandardOutput = True
                // ConvertProcess.StartInfo.RedirectStandardError = True
                // AddHandler ConvertProcess.ErrorDataReceived, AddressOf ErrorInfo
                // AddHandler ConvertProcess.OutputDataReceived, AddressOf OutputInfo

                ErrorOutput = "";
                StandardOutput = "";

                ConvertProcess.Start();
                ConvertProcess.WaitForExit();

                // Validate file, check audio codec.
                if (!File.Exists(tmpfilename))
                {
                    MessageBox.Show("Conversion failure of:" + MediaList[IndexInt].filename + Environment.NewLine + "Executed: ffmpeg " + ConvertProcess.StartInfo.Arguments);
                    continue;
                }

                var ValidateFileInfo = await FFmpeg.GetMediaInfo(tmpfilename);

                // Basic validation (audio codec readable and matching).
                if ((ValidateFileInfo.AudioStreams.ElementAtOrDefault(0).Codec ?? "") == (MediaList[IndexInt].AudioStreams[AudioStreamIndex].Codec ?? ""))
                {
                    // rename existing file to .old ext
                    File.Move(MediaList[IndexInt].filename, MediaList[IndexInt].filename + ".old");
                    File.Move(tmpfilename, MediaList[IndexInt].filename);
                    string oldfile = MediaList[IndexInt].filename + ".old";
                    while (File.Exists(oldfile))
                    {
                        try
                        {
                            // clear read only flag
                            File.SetAttributes(oldfile, FileAttributes.Normal);
                        }
                        catch (Exception ex)
                        {

                        }

                        // delete file
                        try
                        {
                            File.Delete(oldfile);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error deleting " + oldfile + Environment.NewLine + "Click ok to retry.");
                        }
                    }
                }
            }

            // Check Media INFO on file for validation.


            // Delete old file and rename new file.
            lblStatus.Text = "Status: Completed";

            // Reset gui controls.
            ResetControlLists();
            InvokeControl(gbxMedia, x => x.Controls.Clear());
            Invoke(new MethodInvoker(UpdatePageLabel));

            btnProcess.Enabled = true;
        }

        private void txtScanLocation_TextChanged(object sender, EventArgs e)
        {
            /*
            var SettingsKey = My.MyProject.Computer.Registry.CurrentUser.OpenSubKey(SettingsRegKey, true);
            if (SettingsKey is not null)
            {
                My.MyProject.Computer.Registry.CurrentUser.OpenSubKey(SettingsRegKey, true).SetValue("ScanPath", txtScanLocation.Text);
            }
            */
        }

        private void gbxMedia_Enter(object sender, EventArgs e)
        {

        }

        private void txtThreads_TextChanged(object sender, EventArgs e)
        {
            threadcount = int.Parse(txtThreads.Text);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ScanFunctionThread.IsAlive)
            {
                ScanFunctionThread.Abort();
            }

        }
    }


    public class MediaFileStreamInfo
    {
        public string filename { get; set; }
        public int index { get; set; }
        public List<AudioStreamInfo> AudioStreams { get; set; } = new List<AudioStreamInfo>();
        public class AudioStreamInfo
        {
            public int Index { get; set; }
            public string Codec { get; set; }
            public int Channels { get; set; }
            public long Bitrate { get; set; }
            public string Language { get; set; }

            public string SummaryName()
            {
                string DisplayName = Index + ": " + Language + " - " + Codec + " - Channels:" + Channels;
                if (!(Bitrate == 0L))
                {
                    DisplayName = DisplayName + " - Bitrate:" + Bitrate;
                }
                return DisplayName;
            }
        }
    }
}