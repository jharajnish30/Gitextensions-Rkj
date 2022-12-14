using JetBrains.Annotations;

namespace GitUI.Script
{
    // WARNING: This class is serialized to XML!
    public class ScriptInfo
    {
        public ScriptInfo()
        {
            Icon = "bug";
            Enabled = true;
        }

        public bool Enabled { get; set; }

        public string Name { get; set; }

        public string Command { get; set; }

        public string Arguments { get; set; }

        public bool AddToRevisionGridContextMenu { get; set; }

        public ScriptEvent OnEvent { get; set; }

        public bool AskConfirmation { get; set; }

        public bool RunInBackground { get; set; }

        public bool IsPowerShell { get; set; }

        public int HotkeyCommandIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the icon name.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Gets the associated bitmap.
        /// </summary>
        /// <returns>Bitmap image</returns>
        [CanBeNull]
        public System.Drawing.Bitmap GetIcon()
        {
            if (string.IsNullOrWhiteSpace(Icon))
            {
                return null;
            }

            // Get all resources
            System.Resources.ResourceManager rm
                = new System.Resources.ResourceManager("GitUI.Properties.Images",
                    System.Reflection.Assembly.GetExecutingAssembly());

            // return icon
            return (System.Drawing.Bitmap)rm.GetObject(Icon);
        }
    }
}
