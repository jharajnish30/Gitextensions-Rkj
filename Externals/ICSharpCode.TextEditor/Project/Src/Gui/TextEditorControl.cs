// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.TextEditor
{
    /// <summary>
    ///     This class is used for a basic text area control
    /// </summary>
    [ToolboxBitmap("ICSharpCode.TextEditor.Resources.TextEditorControl.bmp")]
    [ToolboxItem(defaultType: true)]
    public class TextEditorControl : TextEditorControlBase
    {
        private readonly TextAreaControl primaryTextArea;

        private TextAreaControl activeTextAreaControl;

        private PrintDocument printDocument;
        private TextAreaControl secondaryTextArea;
        protected Panel textAreaPanel = new Panel();
        private Splitter textAreaSplitter;

        public TextEditorControl()
        {
            SetStyle(ControlStyles.ContainerControl, value: true);

            textAreaPanel.Dock = DockStyle.Fill;

            Document = new DocumentFactory().CreateDocument();
            Document.HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategy();

            primaryTextArea = new TextAreaControl(this);
            activeTextAreaControl = primaryTextArea;
            primaryTextArea.TextArea.GotFocus += delegate { SetActiveTextAreaControl(primaryTextArea); };
            primaryTextArea.Dock = DockStyle.Fill;
            textAreaPanel.Controls.Add(primaryTextArea);
            InitializeTextAreaControl(primaryTextArea);
            Controls.Add(textAreaPanel);
            ResizeRedraw = true;
            Document.UpdateCommited += CommitUpdateRequested;
            OptionsChanged();
        }

        [Browsable(browsable: false)]
        public PrintDocument PrintDocument
        {
            get
            {
                if (printDocument == null)
                {
                    printDocument = new PrintDocument();
                    printDocument.BeginPrint += BeginPrint;
                    printDocument.PrintPage += PrintPage;
                }

                return printDocument;
            }
        }

        public override TextAreaControl ActiveTextAreaControl => activeTextAreaControl;

        [Browsable(browsable: false)]
        public bool EnableUndo => Document.UndoStack.CanUndo;

        [Browsable(browsable: false)]
        public bool EnableRedo => Document.UndoStack.CanRedo;

        protected void SetActiveTextAreaControl(TextAreaControl value)
        {
            if (activeTextAreaControl != value)
            {
                activeTextAreaControl = value;

                ActiveTextAreaControlChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler ActiveTextAreaControlChanged;

        protected virtual void InitializeTextAreaControl(TextAreaControl newControl)
        {
        }

        public override void OptionsChanged()
        {
            primaryTextArea.OptionsChanged();
            secondaryTextArea?.OptionsChanged();
        }

        public void Split()
        {
            if (secondaryTextArea == null)
            {
                secondaryTextArea = new TextAreaControl(this);
                secondaryTextArea.Dock = DockStyle.Bottom;
                secondaryTextArea.Height = Height/2;

                secondaryTextArea.TextArea.GotFocus += delegate { SetActiveTextAreaControl(secondaryTextArea); };

                textAreaSplitter = new Splitter();
                textAreaSplitter.BorderStyle = BorderStyle.FixedSingle;
                textAreaSplitter.Height = 8;
                textAreaSplitter.Dock = DockStyle.Bottom;
                textAreaPanel.Controls.Add(textAreaSplitter);
                textAreaPanel.Controls.Add(secondaryTextArea);
                InitializeTextAreaControl(secondaryTextArea);
                secondaryTextArea.OptionsChanged();
            }
            else
            {
                SetActiveTextAreaControl(primaryTextArea);

                textAreaPanel.Controls.Remove(secondaryTextArea);
                textAreaPanel.Controls.Remove(textAreaSplitter);

                secondaryTextArea.Dispose();
                textAreaSplitter.Dispose();
                secondaryTextArea = null;
                textAreaSplitter = null;
            }
        }

        public void Undo()
        {
            if (Document.ReadOnly)
                return;
            if (Document.UndoStack.CanUndo)
            {
                BeginUpdate();
                Document.UndoStack.Undo();

                Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.WholeTextArea));
                primaryTextArea.TextArea.UpdateMatchingBracket();
                secondaryTextArea?.TextArea.UpdateMatchingBracket();
                EndUpdate();
            }
        }

        public void Redo()
        {
            if (Document.ReadOnly)
                return;
            if (Document.UndoStack.CanRedo)
            {
                BeginUpdate();
                Document.UndoStack.Redo();

                Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.WholeTextArea));
                primaryTextArea.TextArea.UpdateMatchingBracket();
                secondaryTextArea?.TextArea.UpdateMatchingBracket();
                EndUpdate();
            }
        }

        public virtual void SetHighlighting(string name)
        {
            Document.HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategy(name);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (printDocument != null)
                {
                    printDocument.BeginPrint -= BeginPrint;
                    printDocument.PrintPage -= PrintPage;
                    printDocument = null;
                }

                Document.UndoStack.ClearAll();
                Document.UpdateCommited -= CommitUpdateRequested;
                if (textAreaPanel != null)
                {
                    if (secondaryTextArea != null)
                    {
                        secondaryTextArea.Dispose();
                        textAreaSplitter.Dispose();
                        secondaryTextArea = null;
                        textAreaSplitter = null;
                    }

                    primaryTextArea?.Dispose();
                    textAreaPanel.Dispose();
                    textAreaPanel = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Update Methods

        public override void EndUpdate()
        {
            base.EndUpdate();
            Document.CommitUpdate();
            if (!IsInUpdate)
                ActiveTextAreaControl.Caret.OnEndUpdate();
        }

        private void CommitUpdateRequested(object sender, EventArgs e)
        {
            if (IsInUpdate)
                return;
            foreach (var update in Document.UpdateQueue)
                switch (update.TextAreaUpdateType)
                {
                    case TextAreaUpdateType.PositionToEnd:
                        primaryTextArea.TextArea.UpdateToEnd(update.Position.Y);
                        secondaryTextArea?.TextArea.UpdateToEnd(update.Position.Y);
                        break;
                    case TextAreaUpdateType.PositionToLineEnd:
                    case TextAreaUpdateType.SingleLine:
                        primaryTextArea.TextArea.UpdateLine(update.Position.Y);
                        secondaryTextArea?.TextArea.UpdateLine(update.Position.Y);
                        break;
                    case TextAreaUpdateType.SinglePosition:
                        primaryTextArea.TextArea.UpdateLine(update.Position.Y, update.Position.X, update.Position.X);
                        secondaryTextArea?.TextArea.UpdateLine(update.Position.Y, update.Position.X, update.Position.X);
                        break;
                    case TextAreaUpdateType.LinesBetween:
                        primaryTextArea.TextArea.UpdateLines(update.Position.X, update.Position.Y);
                        secondaryTextArea?.TextArea.UpdateLines(update.Position.X, update.Position.Y);
                        break;
                    case TextAreaUpdateType.WholeTextArea:
                        primaryTextArea.TextArea.Invalidate();
                        secondaryTextArea?.TextArea.Invalidate();
                        break;
                }
            Document.UpdateQueue.Clear();
//            this.primaryTextArea.TextArea.Update();
//            if (this.secondaryTextArea != null) {
//                this.secondaryTextArea.TextArea.Update();
//            }
        }

        #endregion

        #region Printing routines

        private int curLineNr;
        private float curTabIndent;
        private StringFormat printingStringFormat;

        private void BeginPrint(object sender, PrintEventArgs ev)
        {
            curLineNr = 0;
            printingStringFormat = (StringFormat)StringFormat.GenericTypographic.Clone();

            // 100 should be enough for everyone ...err ?
            var tabStops = new float[100];
            for (var i = 0; i < tabStops.Length; ++i)
                tabStops[i] = TabIndent*primaryTextArea.TextArea.TextView.WideSpaceWidth;

            printingStringFormat.SetTabStops(firstTabOffset: 0, tabStops);
        }

        private void Advance(ref float x, ref float y, float maxWidth, float size, float fontHeight)
        {
            if (x + size < maxWidth)
            {
                x += size;
            }
            else
            {
                x = curTabIndent;
                y += fontHeight;
            }
        }

        // btw. I hate source code duplication ... but this time I don't care !!!!
        private float MeasurePrintingHeight(Graphics g, LineSegment line, float maxWidth)
        {
            float xPos = 0;
            float yPos = 0;
            var fontHeight = Font.GetHeight(g);
//            bool  gotNonWhitespace = false;
            curTabIndent = 0;
            var fontContainer = TextEditorProperties.FontContainer;
            foreach (var word in line.Words)
                switch (word.Type)
                {
                    case TextWordType.Space:
                        Advance(ref xPos, ref yPos, maxWidth, primaryTextArea.TextArea.TextView.SpaceWidth, fontHeight);
//                        if (!gotNonWhitespace) {
//                            curTabIndent = xPos;
//                        }
                        break;
                    case TextWordType.Tab:
                        Advance(ref xPos, ref yPos, maxWidth, TabIndent*primaryTextArea.TextArea.TextView.WideSpaceWidth, fontHeight);
//                        if (!gotNonWhitespace) {
//                            curTabIndent = xPos;
//                        }
                        break;
                    case TextWordType.Word:
//                        if (!gotNonWhitespace) {
//                            gotNonWhitespace = true;
//                            curTabIndent    += TabIndent * primaryTextArea.TextArea.TextView.GetWidth(' ');
//                        }
                        var drawingSize = g.MeasureString(word.Word, word.GetFont(fontContainer), new SizeF(maxWidth, fontHeight*100), printingStringFormat);
                        Advance(ref xPos, ref yPos, maxWidth, drawingSize.Width, fontHeight);
                        break;
                }
            return yPos + fontHeight;
        }

        private void DrawLine(Graphics g, LineSegment line, float yPos, RectangleF margin)
        {
            float xPos = 0;
            var fontHeight = Font.GetHeight(g);
//            bool  gotNonWhitespace = false;
            curTabIndent = 0;

            var fontContainer = TextEditorProperties.FontContainer;
            foreach (var word in line.Words)
                switch (word.Type)
                {
                    case TextWordType.Space:
                        Advance(ref xPos, ref yPos, margin.Width, primaryTextArea.TextArea.TextView.SpaceWidth, fontHeight);
//                        if (!gotNonWhitespace) {
//                            curTabIndent = xPos;
//                        }
                        break;
                    case TextWordType.Tab:
                        Advance(ref xPos, ref yPos, margin.Width, TabIndent*primaryTextArea.TextArea.TextView.WideSpaceWidth, fontHeight);
//                        if (!gotNonWhitespace) {
//                            curTabIndent = xPos;
//                        }
                        break;
                    case TextWordType.Word:
//                        if (!gotNonWhitespace) {
//                            gotNonWhitespace = true;
//                            curTabIndent    += TabIndent * primaryTextArea.TextArea.TextView.GetWidth(' ');
//                        }
                        g.DrawString(word.Word, word.GetFont(fontContainer), BrushRegistry.GetBrush(word.Color), xPos + margin.X, yPos);
                        var drawingSize = g.MeasureString(word.Word, word.GetFont(fontContainer), new SizeF(margin.Width, fontHeight*100), printingStringFormat);
                        Advance(ref xPos, ref yPos, margin.Width, drawingSize.Width, fontHeight);
                        break;
                }
        }

        private void PrintPage(object sender, PrintPageEventArgs ev)
        {
            var g = ev.Graphics;
            float yPos = ev.MarginBounds.Top;

            while (curLineNr < Document.TotalNumberOfLines)
            {
                var curLine = Document.GetLineSegment(curLineNr);
                if (curLine.Words != null)
                {
                    var drawingHeight = MeasurePrintingHeight(g, curLine, ev.MarginBounds.Width);
                    if (drawingHeight + yPos > ev.MarginBounds.Bottom)
                        break;

                    DrawLine(g, curLine, yPos, ev.MarginBounds);
                    yPos += drawingHeight;
                }

                ++curLineNr;
            }

            // If more lines exist, print another page.
            ev.HasMorePages = curLineNr < Document.TotalNumberOfLines;
        }

        #endregion
    }
}