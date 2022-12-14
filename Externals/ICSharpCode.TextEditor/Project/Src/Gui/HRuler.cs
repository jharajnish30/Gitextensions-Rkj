// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision$</version>
// </file>

using System.Drawing;
using System.Windows.Forms;

namespace ICSharpCode.TextEditor
{
    /// <summary>
    ///     Horizontal ruler - text column measuring ruler at the top of the text area.
    /// </summary>
    public class HRuler : Control
    {
        private readonly TextArea textArea;

        public HRuler(TextArea textArea)
        {
            this.textArea = textArea;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            var num = 0;
            for (float x = textArea.TextView.DrawingPosition.Left; x < textArea.TextView.DrawingPosition.Right; x += textArea.TextView.WideSpaceWidth)
            {
                var offset = Height*2/3;
                if (num%5 == 0)
                    offset = Height*4/5;

                if (num%10 == 0)
                    offset = 1;
                ++num;
                g.DrawLine(
                    Pens.Black,
                    (int)x, offset, (int)x, Height - offset);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(
                Brushes.White,
                new Rectangle(
                    x: 0,
                    y: 0,
                    Width,
                    Height));
        }
    }
}