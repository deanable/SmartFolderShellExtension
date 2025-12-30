using System;
using System.Drawing;
using System.Windows.Forms;

namespace SmartFolder.UI
{
    public class FolderNamePrompt : Form
    {
        private TextBox textBox;
        private Button btnOk;
        private Button btnCancel;
        public string FolderName { get; private set; } = string.Empty;

        public FolderNamePrompt(string promptText)
        {
            this.Text = "SmartFolder";
            this.Size = new Size(350, 150);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var label = new Label() { Left = 20, Top = 20, Text = promptText, Width = 300 };
            textBox = new TextBox() { Left = 20, Top = 45, Width = 290 };

            btnOk = new Button() { Text = "OK", Left = 130, Width = 80, Top = 80, DialogResult = DialogResult.OK };
            btnOk.Click += (sender, e) => { FolderName = textBox.Text; Close(); };

            btnCancel = new Button() { Text = "Cancel", Left = 220, Width = 80, Top = 80, DialogResult = DialogResult.Cancel };

            this.Controls.Add(label);
            this.Controls.Add(textBox);
            this.Controls.Add(btnOk);
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }
    }
}
