using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;

namespace GitUI.UserControls
{
    public partial class CommitPickerSmallControl : GitModuleControl
    {
        /// <summary>
        /// Occurs whenever the selected commit hash changes.
        /// </summary>
        public event EventHandler SelectedObjectIdChanged;

        public CommitPickerSmallControl()
        {
            InitializeComponent();
            InitializeComplete();
        }

        [CanBeNull]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ObjectId SelectedObjectId { get; private set; }

        /// <summary>
        /// shows a message box if commitHash is invalid
        /// </summary>
        public void SetSelectedCommitHash(string commitHash)
        {
            var oldCommitHash = SelectedObjectId;

            SelectedObjectId = Module.RevParse(commitHash);

            if (SelectedObjectId is null && !string.IsNullOrWhiteSpace(commitHash))
            {
                SelectedObjectId = oldCommitHash;
                MessageBox.Show("The given commit hash is not valid for this repository and was therefore discarded.", Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                SelectedObjectIdChanged?.Invoke(this, EventArgs.Empty);
            }

            var isArtificialCommitForEmptyRepo = commitHash == "HEAD";

            if (SelectedObjectId is null || isArtificialCommitForEmptyRepo)
            {
                textBoxCommitHash.Text = "";
                lbCommits.Text = "";
            }
            else
            {
                textBoxCommitHash.Text = SelectedObjectId.ToShortString();
                ThreadHelper.JoinableTaskFactory.RunAsync(
                    async () =>
                    {
                        await TaskScheduler.Default.SwitchTo(alwaysYield: true);

                        var currentCheckout = Module.GetCurrentCheckout();

                        Debug.Assert(currentCheckout is not null, "currentCheckout is not null");

                        var text = Module.GetCommitCountString(currentCheckout.ToString(), SelectedObjectId.ToString());

                        await this.SwitchToMainThreadAsync();

                        lbCommits.Text = text;
                    });
            }
        }

        private void buttonPickCommit_Click(object sender, EventArgs e)
        {
            using (var chooseForm = new FormChooseCommit(UICommands, SelectedObjectId?.ToString()))
            {
                if (chooseForm.ShowDialog(this) == DialogResult.OK && chooseForm.SelectedRevision is not null)
                {
                    SetSelectedCommitHash(chooseForm.SelectedRevision.Guid);
                }
            }
        }

        private void textBoxCommitHash_TextLeave(object sender, EventArgs e)
        {
            SetSelectedCommitHash(textBoxCommitHash.Text.Trim());
        }
    }
}
