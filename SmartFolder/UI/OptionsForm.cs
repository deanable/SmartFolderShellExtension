using System;
using System.Drawing;
using System.Windows.Forms;
using SmartFolder.Core;

namespace SmartFolder.UI
{
    public class OptionsForm : Form
    {
        private SmartFolderSettings _settings;
        private RadioButton rbFirstItem;
        private RadioButton rbPrompt;
        private CheckBox chkEnableGrouping;
        private CheckBox chkYear;
        private CheckBox chkMonth;
        private CheckBox chkDay;
        private ComboBox cmbSecondary;

        public OptionsForm()
        {
            _settings = SmartFolderSettings.Load();
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "SmartFolder Options";
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Section 1: Multi Select
            var grpMulti = new GroupBox() { Text = "Multiple Selection Strategy", Left = 10, Top = 10, Width = 360, Height = 80 };
            rbFirstItem = new RadioButton() { Text = "Use name of first item (Default)", Left = 15, Top = 20, Width = 300, Checked = true };
            rbPrompt = new RadioButton() { Text = "Always prompt me for a folder name", Left = 15, Top = 45, Width = 300 };
            grpMulti.Controls.Add(rbFirstItem);
            grpMulti.Controls.Add(rbPrompt);
            this.Controls.Add(grpMulti);

            // Section 2: Grouping
            var grpGrouping = new GroupBox() { Text = "Property-Based Grouping", Left = 10, Top = 100, Width = 360, Height = 150 };
            chkEnableGrouping = new CheckBox() { Text = "Enable Grouping", Left = 15, Top = 20, Width = 200 };
            chkEnableGrouping.CheckedChanged += (s, e) => ToggleGroupingControls(chkEnableGrouping.Checked);

            var lblDate = new Label() { Text = "Date Grouping:", Left = 15, Top = 50, Width = 100 };
            chkYear = new CheckBox() { Text = "Year", Left = 120, Top = 50, Width = 60 };
            chkMonth = new CheckBox() { Text = "Month", Left = 180, Top = 50, Width = 60 };
            chkDay = new CheckBox() { Text = "Day", Left = 240, Top = 50, Width = 60 };

            var lblProp = new Label() { Text = "Secondary Property:", Left = 15, Top = 80, Width = 120 };
            cmbSecondary = new ComboBox() { Left = 140, Top = 75, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSecondary.Items.AddRange(Enum.GetNames(typeof(PropertyGroupingType)));

            grpGrouping.Controls.Add(chkEnableGrouping);
            grpGrouping.Controls.Add(lblDate);
            grpGrouping.Controls.Add(chkYear);
            grpGrouping.Controls.Add(chkMonth);
            grpGrouping.Controls.Add(chkDay);
            grpGrouping.Controls.Add(lblProp);
            grpGrouping.Controls.Add(cmbSecondary);
            this.Controls.Add(grpGrouping);

            // Buttons
            var btnSave = new Button() { Text = "Save", Left = 200, Top = 270, Width = 80, DialogResult = DialogResult.OK };
            btnSave.Click += BtnSave_Click;
            var btnCancel = new Button() { Text = "Cancel", Left = 290, Top = 270, Width = 80, DialogResult = DialogResult.Cancel };

            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
        }

        private void ToggleGroupingControls(bool enabled)
        {
            chkYear.Enabled = enabled;
            chkMonth.Enabled = enabled;
            chkDay.Enabled = enabled;
            cmbSecondary.Enabled = enabled;
        }

        private void LoadSettings()
        {
            if (_settings.MultiSelectStrategy == MultiSelectStrategy.UseFirstItemName) rbFirstItem.Checked = true;
            else rbPrompt.Checked = true;

            chkEnableGrouping.Checked = _settings.EnableGrouping;
            chkYear.Checked = _settings.GroupByYear;
            chkMonth.Checked = _settings.GroupByMonth;
            chkDay.Checked = _settings.GroupByDay;
            cmbSecondary.SelectedItem = _settings.SecondaryGrouping.ToString();

            ToggleGroupingControls(_settings.EnableGrouping);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            _settings.MultiSelectStrategy = rbFirstItem.Checked ? MultiSelectStrategy.UseFirstItemName : MultiSelectStrategy.AlwaysPrompt;
            _settings.EnableGrouping = chkEnableGrouping.Checked;
            _settings.GroupByYear = chkYear.Checked;
            _settings.GroupByMonth = chkMonth.Checked;
            _settings.GroupByDay = chkDay.Checked;

            if (Enum.TryParse(cmbSecondary.SelectedItem.ToString(), out PropertyGroupingType result))
            {
                _settings.SecondaryGrouping = result;
            }

            _settings.Save();
            this.Close();
        }
    }
}
