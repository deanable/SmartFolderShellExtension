using System;
using System.Drawing;
using System.Windows.Forms;
using SmartFolder.Core;

namespace SmartFolder.UI
{
    public class ConflictResolutionDialog : Form
    {
        public FileOrganizer.ConflictResolution Resolution { get; private set; }

        public ConflictResolutionDialog(string sourceFile, string destFile)
        {
            this.Text = "File Conflict";
            this.Size = new Size(400, 200);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var label = new Label()
            {
                Left = 20,
                Top = 20,
                Width = 350,
                Height = 60,
                Text = $"File '{System.IO.Path.GetFileName(sourceFile)}' already exists in destination.\nWhat would you like to do?"
            };

            var btnOverwrite = new Button() { Text = "Overwrite", Left = 20, Top = 100, Width = 80 };
            btnOverwrite.Click += (s, e) => { Resolution = FileOrganizer.ConflictResolution.Overwrite; this.DialogResult = DialogResult.OK; Close(); };

            var btnRename = new Button() { Text = "Rename", Left = 110, Top = 100, Width = 80 };
            btnRename.Click += (s, e) => { Resolution = FileOrganizer.ConflictResolution.Rename; this.DialogResult = DialogResult.OK; Close(); };

            var btnSkip = new Button() { Text = "Skip", Left = 200, Top = 100, Width = 80 };
            btnSkip.Click += (s, e) => { Resolution = FileOrganizer.ConflictResolution.Skip; this.DialogResult = DialogResult.OK; Close(); };

            var btnCancel = new Button() { Text = "Cancel", Left = 290, Top = 100, Width = 80 };
            btnCancel.Click += (s, e) => { Resolution = FileOrganizer.ConflictResolution.Cancel; this.DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.Add(label);
            this.Controls.Add(btnOverwrite);
            this.Controls.Add(btnRename);
            this.Controls.Add(btnSkip);
            this.Controls.Add(btnCancel);
        }
    }
}
