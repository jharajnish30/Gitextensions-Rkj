﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    /// <summary>
    /// Represents a menu command with Text, ShortcutKey or ShortcutDisplayString, Action,
    /// Icon and optionally CheckBoxes.
    /// From a MenuCommand a (theoretically) unlimited number of actual ToolStripItems can
    /// be created that all behave the same.
    /// A MenuCommand can also be a separator
    ///
    /// Purpose: have methods from specific context menus also available in main menu
    /// </summary>
    internal class MenuCommand
    {
        public static MenuCommand CreateSeparator()
        {
            return new MenuCommand { IsSeparator = true };
        }

        public static ToolStripItem CreateToolStripItem(MenuCommand menuCommand)
        {
            if (menuCommand.IsSeparator)
            {
                return new ToolStripSeparator();
            }

            var toolStripMenuItem = new ToolStripMenuItem
            {
                Name = menuCommand.Name,
                Text = menuCommand.Text,
                Image = menuCommand.Image,
                ShortcutKeys = menuCommand.ShortcutKeys,
                ShortcutKeyDisplayString = menuCommand.ShortcutKeyDisplayString,
                ToolTipText = menuCommand.ToolTipText,
            };

            toolStripMenuItem.Click += (obj, sender) =>
            {
                if (menuCommand.ExecuteAction is not null)
                {
                    menuCommand.ExecuteAction();
                }
                else
                {
                    MessageBox.Show("No ExecuteAction assigned to this MenuCommand. Please submit a bug report.", Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            return toolStripMenuItem;
        }

        private readonly List<ToolStripMenuItem> _registeredMenuItems = new List<ToolStripMenuItem>();

        /// <summary>
        /// if true all other properties have no meaning
        /// </summary>
        public bool IsSeparator { get; set; }

        /// <summary>
        /// used for ToolStripItem and translation identification
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// text of the menu item
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// tooltip text of the menu item
        /// </summary>
        public string ToolTipText { get; set; }

        /// <summary>
        /// image of the menu item
        /// </summary>
        public Image Image { get; set; }

        public Keys ShortcutKeys { get; set; }

        public string ShortcutKeyDisplayString { get; set; }

        /// <summary>
        /// execute on menu click
        /// </summary>
        public Action ExecuteAction;

        /// <summary>
        /// called if the menu item want to know if the Checked property
        /// should be true or false. If null then false.
        /// </summary>
        [CanBeNull] public Func<bool> IsCheckedFunc;

        /// <summary>
        /// To make the MenuCommand interact with all its associated menu items
        /// this method is used to register the all menu items that where generated by this MenuCommand
        /// </summary>
        public void RegisterMenuItem(ToolStripMenuItem menuItem)
        {
            _registeredMenuItems.Add(menuItem);
        }

        public void UnregisterMenuItems(IEnumerable<ToolStripMenuItem> items)
        {
            foreach (var item in items)
            {
                _registeredMenuItems.Remove(item);
            }
        }

        public void SetCheckForRegisteredMenuItems()
        {
            if (IsCheckedFunc is not null)
            {
                var isChecked = IsCheckedFunc();

                foreach (var item in _registeredMenuItems)
                {
                    item.Checked = isChecked;
                }
            }
        }

        public void UpdateMenuItemsShortcutKeyDisplayString()
        {
            foreach (var item in _registeredMenuItems)
            {
                item.ShortcutKeyDisplayString = ShortcutKeyDisplayString;
            }
        }
    }
}
