// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision$</version>
// </file>

using System.Text;
using ICSharpCode.TextEditor.Util;

namespace ICSharpCode.TextEditor.Document
{
    /// <summary>
    ///     This interface represents a container which holds a text sequence and
    ///     all necessary information about it. It is used as the base for a text editor.
    /// </summary>
    public class DocumentFactory
    {
        /// <remarks>
        ///     Creates a new <see cref="IDocument" /> object. Only create
        ///     <see cref="IDocument" /> with this method.
        /// </remarks>
        public IDocument CreateDocument()
        {
            var doc = new DefaultDocument();
            doc.TextBufferStrategy = new GapTextBufferStrategy();
            doc.FormattingStrategy = new DefaultFormattingStrategy();
            doc.LineManager = new LineManager(doc, highlightingStrategy: null);
            doc.FoldingManager = new FoldingManager(doc);
            doc.FoldingManager.FoldingStrategy = null; //new ParserFoldingStrategy();
            doc.MarkerStrategy = new MarkerStrategy(doc);
            doc.BookmarkManager = new BookmarkManager(doc);
            return doc;
        }

        /// <summary>
        ///     Creates a new document and loads the given file
        /// </summary>
        public IDocument CreateFromTextBuffer(ITextBufferStrategy textBuffer)
        {
            var doc = (DefaultDocument)CreateDocument();
            doc.TextContent = textBuffer.GetText(offset: 0, textBuffer.Length);
            doc.TextBufferStrategy = textBuffer;
            return doc;
        }

        /// <summary>
        ///     Creates a new document and loads the given file
        /// </summary>
        public IDocument CreateFromFile(string fileName)
        {
            var document = CreateDocument();
            document.TextContent = FileReader.ReadFileContent(fileName, Encoding.Default);
            return document;
        }
    }
}