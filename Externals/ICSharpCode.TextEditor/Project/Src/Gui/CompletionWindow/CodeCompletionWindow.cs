// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Util;

namespace ICSharpCode.TextEditor.Gui.CompletionWindow
{
    public class CodeCompletionWindow : AbstractCompletionWindow
    {
        private const int ScrollbarWidth = 16;
        private const int MaxListLength = 10;
        private readonly ICompletionData[] completionData;
        private readonly ICompletionDataProvider dataProvider;
        private readonly IDocument document;
        private readonly bool fixedListViewWidth = true;

        private readonly MouseWheelHandler mouseWheelHandler = new MouseWheelHandler();
        private readonly bool showDeclarationWindow = true;
        private readonly VScrollBar vScrollBar = new VScrollBar();
        private readonly Rectangle workingScreen;
        private CodeCompletionListView codeCompletionListView;
        private DeclarationViewWindow declarationViewWindow;
        private int endOffset;

        private bool inScrollUpdate;

        private int startOffset;

        private CodeCompletionWindow(ICompletionDataProvider completionDataProvider, ICompletionData[] completionData, Form parentForm, TextEditorControl control, bool showDeclarationWindow, bool fixedListViewWidth) : base(parentForm, control)
        {
            dataProvider = completionDataProvider;
            this.completionData = completionData;
            document = control.Document;
            this.showDeclarationWindow = showDeclarationWindow;
            this.fixedListViewWidth = fixedListViewWidth;

            workingScreen = Screen.GetWorkingArea(Location);
            startOffset = control.ActiveTextAreaControl.Caret.Offset + 1;
            endOffset = startOffset;
            if (completionDataProvider.PreSelection != null)
            {
                startOffset -= completionDataProvider.PreSelection.Length + 1;
                endOffset--;
            }

            codeCompletionListView = new CodeCompletionListView(completionData);
            codeCompletionListView.ImageList = completionDataProvider.ImageList;
            codeCompletionListView.Dock = DockStyle.Fill;
            codeCompletionListView.SelectedItemChanged += CodeCompletionListViewSelectedItemChanged;
            codeCompletionListView.DoubleClick += CodeCompletionListViewDoubleClick;
            codeCompletionListView.Click += CodeCompletionListViewClick;
            Controls.Add(codeCompletionListView);

            if (completionData.Length > MaxListLength)
            {
                vScrollBar.Dock = DockStyle.Right;
                vScrollBar.Minimum = 0;
                vScrollBar.Maximum = completionData.Length - 1;
                vScrollBar.SmallChange = 1;
                vScrollBar.LargeChange = MaxListLength;
                codeCompletionListView.FirstItemChanged += CodeCompletionListViewFirstItemChanged;
                Controls.Add(vScrollBar);
            }

            drawingSize = GetListViewSize();
            SetLocation();

            if (declarationViewWindow == null)
                declarationViewWindow = new DeclarationViewWindow(parentForm);
            SetDeclarationViewLocation();
            declarationViewWindow.ShowDeclarationViewWindow();
            declarationViewWindow.MouseMove += ControlMouseMove;
            control.Focus();
            CodeCompletionListViewSelectedItemChanged(this, EventArgs.Empty);

            if (completionDataProvider.DefaultIndex >= 0)
                codeCompletionListView.SelectIndex(completionDataProvider.DefaultIndex);

            if (completionDataProvider.PreSelection != null)
                CaretOffsetChanged(this, EventArgs.Empty);

            vScrollBar.ValueChanged += VScrollBarValueChanged;
            document.DocumentAboutToBeChanged += DocumentAboutToBeChanged;
        }

        /// <summary>
        ///     When this flag is set, code completion closes if the caret moves to the
        ///     beginning of the allowed range. This is useful in Ctrl+Space and "complete when typing",
        ///     but not in dot-completion.
        /// </summary>
        public bool CloseWhenCaretAtBeginning { get; set; }

        public static CodeCompletionWindow ShowCompletionWindow(Form parent, TextEditorControl control, string fileName, ICompletionDataProvider completionDataProvider, char firstChar)
        {
            return ShowCompletionWindow(parent, control, fileName, completionDataProvider, firstChar, showDeclarationWindow: true, fixedListViewWidth: true);
        }

        public static CodeCompletionWindow ShowCompletionWindow(Form parent, TextEditorControl control, string fileName, ICompletionDataProvider completionDataProvider, char firstChar, bool showDeclarationWindow, bool fixedListViewWidth)
        {
            var completionData = completionDataProvider.GenerateCompletionData(fileName, control.ActiveTextAreaControl.TextArea, firstChar);
            if (completionData == null || completionData.Length == 0)
                return null;
            var codeCompletionWindow = new CodeCompletionWindow(completionDataProvider, completionData, parent, control, showDeclarationWindow, fixedListViewWidth);
            codeCompletionWindow.CloseWhenCaretAtBeginning = firstChar == '\0';
            codeCompletionWindow.ShowCompletionWindow();
            return codeCompletionWindow;
        }

        private void CodeCompletionListViewFirstItemChanged(object sender, EventArgs e)
        {
            if (inScrollUpdate) return;
            inScrollUpdate = true;
            vScrollBar.Value = Math.Min(vScrollBar.Maximum, codeCompletionListView.FirstItem);
            inScrollUpdate = false;
        }

        private void VScrollBarValueChanged(object sender, EventArgs e)
        {
            if (inScrollUpdate) return;
            inScrollUpdate = true;
            codeCompletionListView.FirstItem = vScrollBar.Value;
            codeCompletionListView.Refresh();
            control.ActiveTextAreaControl.TextArea.Focus();
            inScrollUpdate = false;
        }

        private void SetDeclarationViewLocation()
        {
            //  This method uses the side with more free space
            var leftSpace = Bounds.Left - workingScreen.Left;
            var rightSpace = workingScreen.Right - Bounds.Right;
            Point pos;
            // The declaration view window has better line break when used on
            // the right side, so prefer the right side to the left.
            if (rightSpace*2 > leftSpace)
            {
                declarationViewWindow.FixedWidth = false;
                pos = new Point(Bounds.Right, Bounds.Top);
                if (declarationViewWindow.Location != pos)
                    declarationViewWindow.Location = pos;
            }
            else
            {
                declarationViewWindow.Width = declarationViewWindow.GetRequiredLeftHandSideWidth(new Point(Bounds.Left, Bounds.Top));
                declarationViewWindow.FixedWidth = true;
                if (Bounds.Left < declarationViewWindow.Width)
                    pos = new Point(x: 0, Bounds.Top);
                else
                    pos = new Point(Bounds.Left - declarationViewWindow.Width, Bounds.Top);
                if (declarationViewWindow.Location != pos)
                    declarationViewWindow.Location = pos;
                declarationViewWindow.Refresh();
            }
        }

        protected override void SetLocation()
        {
            base.SetLocation();
            if (declarationViewWindow != null)
                SetDeclarationViewLocation();
        }

        public void HandleMouseWheel(MouseEventArgs e)
        {
            var scrollDistance = mouseWheelHandler.GetScrollAmount(e);
            if (scrollDistance == 0)
                return;
            if (control.TextEditorProperties.MouseWheelScrollDown)
                scrollDistance = -scrollDistance;
            var newValue = vScrollBar.Value + vScrollBar.SmallChange*scrollDistance;
            vScrollBar.Value = Math.Max(vScrollBar.Minimum, Math.Min(vScrollBar.Maximum - vScrollBar.LargeChange + 1, newValue));
        }

        private void CodeCompletionListViewSelectedItemChanged(object sender, EventArgs e)
        {
            var data = codeCompletionListView.SelectedCompletionData;
            if (showDeclarationWindow && !string.IsNullOrEmpty(data?.Description))
            {
                declarationViewWindow.Description = data.Description;
                SetDeclarationViewLocation();
            }
            else
            {
                declarationViewWindow.Description = null;
            }
        }

        public override bool ProcessKeyEvent(char ch)
        {
            switch (dataProvider.ProcessKey(ch))
            {
                case CompletionDataProviderKeyResult.BeforeStartKey:
                    // increment start+end, then process as normal char
                    ++startOffset;
                    ++endOffset;
                    return base.ProcessKeyEvent(ch);
                case CompletionDataProviderKeyResult.NormalKey:
                    // just process normally
                    return base.ProcessKeyEvent(ch);
                case CompletionDataProviderKeyResult.InsertionKey:
                    return InsertSelectedItem(ch);
                default:
                    throw new InvalidOperationException("Invalid return value of dataProvider.ProcessKey");
            }
        }

        private void DocumentAboutToBeChanged(object sender, DocumentEventArgs e)
        {
            // => startOffset test required so that this startOffset/endOffset are not incremented again
            //    for BeforeStartKey characters
            if (e.Offset >= startOffset && e.Offset <= endOffset)
            {
                if (e.Length > 0)
                    endOffset -= e.Length;
                if (!string.IsNullOrEmpty(e.Text))
                    endOffset += e.Text.Length;
            }
        }

        protected override void CaretOffsetChanged(object sender, EventArgs e)
        {
            var offset = control.ActiveTextAreaControl.Caret.Offset;
            if (offset == startOffset)
            {
                if (CloseWhenCaretAtBeginning)
                    Close();
                return;
            }

            if (offset < startOffset || offset > endOffset)
                Close();
            else
                codeCompletionListView.SelectItemWithStart(control.Document.GetText(startOffset, offset - startOffset));
        }

        protected override bool ProcessTextAreaKey(Keys keyData)
        {
            if (!Visible)
                return false;

            switch (keyData)
            {
                case Keys.Home:
                    codeCompletionListView.SelectIndex(index: 0);
                    return true;
                case Keys.End:
                    codeCompletionListView.SelectIndex(completionData.Length - 1);
                    return true;
                case Keys.PageDown:
                    codeCompletionListView.PageDown();
                    return true;
                case Keys.PageUp:
                    codeCompletionListView.PageUp();
                    return true;
                case Keys.Down:
                    codeCompletionListView.SelectNextItem();
                    return true;
                case Keys.Up:
                    codeCompletionListView.SelectPrevItem();
                    return true;
                case Keys.Tab:
                    InsertSelectedItem(ch: '\t');
                    return true;
                case Keys.Return:
                    InsertSelectedItem(ch: '\n');
                    return true;
            }

            return base.ProcessTextAreaKey(keyData);
        }

        private void CodeCompletionListViewDoubleClick(object sender, EventArgs e)
        {
            InsertSelectedItem(ch: '\0');
        }

        private void CodeCompletionListViewClick(object sender, EventArgs e)
        {
            control.ActiveTextAreaControl.TextArea.Focus();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                document.DocumentAboutToBeChanged -= DocumentAboutToBeChanged;
                if (codeCompletionListView != null)
                {
                    codeCompletionListView.Dispose();
                    codeCompletionListView = null;
                }

                if (declarationViewWindow != null)
                {
                    declarationViewWindow.Dispose();
                    declarationViewWindow = null;
                }
            }

            base.Dispose(disposing);
        }

        private bool InsertSelectedItem(char ch)
        {
            document.DocumentAboutToBeChanged -= DocumentAboutToBeChanged;
            var data = codeCompletionListView.SelectedCompletionData;
            var result = false;
            if (data != null)
            {
                control.BeginUpdate();

                try
                {
                    if (endOffset - startOffset > 0)
                        control.Document.Remove(startOffset, endOffset - startOffset);
                    Debug.Assert(startOffset <= document.TextLength);
                    result = dataProvider.InsertAction(data, control.ActiveTextAreaControl.TextArea, startOffset, ch);
                }
                finally
                {
                    control.EndUpdate();
                }
            }

            Close();
            return result;
        }

        private Size GetListViewSize()
        {
            var height = codeCompletionListView.ItemHeight*Math.Min(MaxListLength, completionData.Length);
            var width = codeCompletionListView.ItemHeight*10;
            if (!fixedListViewWidth)
                width = GetListViewWidth(width, height);
            return new Size(width, height);
        }

        /// <summary>
        ///     Gets the list view width large enough to handle the longest completion data
        ///     text string.
        /// </summary>
        /// <param name="defaultWidth">The default width of the list view.</param>
        /// <param name="height">
        ///     The height of the list view.  This is
        ///     used to determine if the scrollbar is visible.
        /// </param>
        /// <returns>
        ///     The list view width to accommodate the longest completion
        ///     data text string; otherwise the default width.
        /// </returns>
        private int GetListViewWidth(int defaultWidth, int height)
        {
            float width = defaultWidth;
            using (var graphics = codeCompletionListView.CreateGraphics())
            {
                for (var i = 0; i < completionData.Length; ++i)
                {
                    var itemWidth = graphics.MeasureString(completionData[i].Text, codeCompletionListView.Font).Width;
                    if (itemWidth > width)
                        width = itemWidth;
                }
            }

            float totalItemsHeight = codeCompletionListView.ItemHeight*completionData.Length;
            if (totalItemsHeight > height)
                width += ScrollbarWidth; // Compensate for scroll bar.
            return (int)width;
        }
    }
}