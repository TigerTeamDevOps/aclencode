using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ACLencoder
{
    public partial class MainForm : Form
    {
        private AclEncoder _encoder = null;

        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Click event for the ChooseFileListButon
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChooseFileListButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.CheckFileExists = true;

            string CurrentFileList = FileListTextBox.Text;

            if (!CurrentFileList.Equals(string.Empty))
            {
                if (File.Exists(CurrentFileList))
                {
                    OFD.FileName = CurrentFileList;
                }
            }

            DialogResult Result = OFD.ShowDialog(this);

            if (Result == DialogResult.OK)
            {
                FileListTextBox.Text = OFD.FileName;
                LoadFileList(OFD.FileName);
            }
        }

        /// <summary>
        /// Handles the Click event for the ChoseTargetPathButton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChooseTargetPathButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog SFD = new SaveFileDialog();
            SFD.OverwritePrompt = false;
            SFD.Title = "Choose target file to encode or decode";

            string CurrentTarget = TargetTextBox.Text;

            if (!CurrentTarget.Equals(string.Empty))
            {
                if (File.Exists(CurrentTarget))
                {
                    SFD.FileName = CurrentTarget;
                }
            }

            DialogResult Result = SFD.ShowDialog(this);

            if (Result == DialogResult.OK)
            {
                TargetTextBox.Text = SFD.FileName;
            }
        }

        /// <summary>
        /// Handles the CLICK event generated by the EncodeButton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EncodeButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_encoder == null)
                {
                    UpdateStatus("You haven't selected a list of files yet!");
                    return;
                }

                string FileToEncode = TargetTextBox.Text;

                if (FileToEncode.Equals(string.Empty))
                {
                    UpdateStatus("You haven't chosen a file to encode.");
                    return;
                }

                UpdateStatus("Encoding file...");
                _encoder.Encode(FileToEncode);
                UpdateStatus("The file has been encoded");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles the 'Decode' button_click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DecodeButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Make sure they've loaded a filelist already...
                if (_encoder == null)
                {
                    UpdateStatus("You haven't selected a list of files yet!");
                    return;
                }

                string FileToDecode = TargetTextBox.Text;

                if (FileToDecode.Equals(string.Empty))
                {
                    UpdateStatus("You haven't chosen where to store the decoded file.");
                    return;
                }

                if (File.Exists(FileToDecode))
                {
                    DialogResult Result = MessageBox.Show("This output file already exists.\nDo you want to overwrite it with the decoded file?", "Replace File?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    switch (Result)
                    {
                        case DialogResult.No:
                            return;
                        default:
                            break;
                    }
                }

                UpdateStatus("Decoding file...");
                _encoder.Decode(FileToDecode);
                UpdateStatus("The file has been decoded");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles the Click event for the RemoveEncodedFileButton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveEncodedFileButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_encoder == null)
                {
                    UpdateStatus("You haven't selected a list of files yet!");
                    return;
                }

                DialogResult Result = MessageBox.Show("Are you sure you want to wipe all encoded ACL entries from these files?", "Confirm ACL Wipe", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (Result == DialogResult.Yes)
                {
                    UpdateStatus("Removing encoded ACL entries from files...");
                    _encoder.RemoveEncodedFile();
                    UpdateStatus("All encoded ACL entries have been removed.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles the CreateFileListButton's Click event. Creates a txt file with filepaths
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateFileListButton_Click(object sender, EventArgs e)
        {
            // Prompt the user to select multiple files
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.Multiselect = true;
            OFD.Title = "Choose files to include in the file list";

            DialogResult Result = OFD.ShowDialog();
            if (Result == DialogResult.OK)
            {
                // We've got the files for the list.
                // Where should the new list go?
                SaveFileDialog SFD = new SaveFileDialog();
                SFD.OverwritePrompt = true;
                SFD.Title = "Where do you want to save this file list?";

                DialogResult SaveResult = SFD.ShowDialog();
                if (SaveResult == DialogResult.OK)
                {
                    string OutputPath = SFD.FileName;
                    StreamWriter OutputStream = new StreamWriter(OutputPath);

                    foreach (string FilePath in OFD.FileNames)
                    {
                        OutputStream.WriteLine(FilePath);
                    }

                    OutputStream.Close();

                    // Update the UI with the new path
                    FileListTextBox.Text = OutputPath;
                    LoadFileList(OutputPath);
                }
            }
        }

        /// <summary>
        /// Updates the status message displayed in the status bar and starts the timer to blank it out when it's done
        /// </summary>
        /// <param name="message">The message to be displayed</param>
        private void UpdateStatus(string message)
        {
            StatusToolStripStatusLabel.Text = message;

            if (StatusOverTimer.Enabled)
                StatusOverTimer.Stop();

            StatusOverTimer.Start();
        }

        /// <summary>
        /// Handles the Timer_Tick event. Wipes the status text and stops the timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StatusOverTimer_Tick(object sender, EventArgs e)
        {
            StatusOverTimer.Stop();
            //StatusToolStripStatusLabel.Text = "<idle>";
        }

        /// <summary>
        /// Loads the specified FileList and prepares an ACLEncoder instance for use
        /// </summary>
        /// <param name="path">A fuly-qualified path to a file that contains a list of other files</param>
        private void LoadFileList(string path)
        {
            try
            {
                UpdateStatus("Loading File List...");
                _encoder = new AclEncoder(path);
                UpdateStatus("File List Loaded.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}