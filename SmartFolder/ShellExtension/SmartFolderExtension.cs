using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using SmartFolder.Core;
using SmartFolder.UI;

namespace SmartFolder.ShellExtension
{
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.AllFiles)] // For files
    [COMServerAssociation(AssociationType.Directory)] // For folders
    public class SmartFolderExtension : SharpContextMenu
    {
        protected override bool CanShowMenu()
        {
            // Show menu if any items are selected
            return SelectedItemPaths.Any();
        }

        protected override ContextMenuStrip CreateMenu()
        {
            var menu = new ContextMenuStrip();

            var mainItem = new ToolStripMenuItem("Item(s) to Folder");
            mainItem.Click += (sender, args) => ExecuteMove();

            var optionsItem = new ToolStripMenuItem("Options");
            optionsItem.Click += (sender, args) => ShowOptions();

            menu.Items.Add(mainItem);
            menu.Items.Add(optionsItem);

            return menu;
        }

        private void ExecuteMove()
        {
            // Execute on background thread
            var thread = new Thread(() =>
            {
                try
                {
                    var settings = SmartFolderSettings.Load();
                    var organizer = new FileOrganizer(settings, PromptForName, ResolveConflict);
                    organizer.OrganizeFiles(SelectedItemPaths);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error organizing files: {ex.Message}", "SmartFolder Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });
            thread.SetApartmentState(ApartmentState.STA); // UI might be needed for prompts
            thread.Start();
        }

        private void ShowOptions()
        {
            var thread = new Thread(() =>
            {
                using (var form = new OptionsForm())
                {
                    form.ShowDialog();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private string PromptForName(string promptText)
        {
            string result = null;
            // Since we are in a background thread with STA, we can show dialogs
            // However, SharpShell might be running in Explorer process.

            // Invoke on the thread that created the UI if possible, but here we are in a new thread.
            // Form.ShowDialog should work fine in STA thread.

            using (var prompt = new FolderNamePrompt(promptText))
            {
                if (prompt.ShowDialog() == DialogResult.OK)
                {
                    result = prompt.FolderName;
                }
            }
            return result;
        }

        private FileOrganizer.ConflictResolution ResolveConflict(string source, string dest)
        {
            FileOrganizer.ConflictResolution result = FileOrganizer.ConflictResolution.Cancel;

            using (var dialog = new ConflictResolutionDialog(source, dest))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    result = dialog.Resolution;
                }
                else
                {
                    result = FileOrganizer.ConflictResolution.Cancel;
                }
            }
            return result;
        }
    }
}
