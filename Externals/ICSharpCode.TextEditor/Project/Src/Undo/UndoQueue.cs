// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ICSharpCode.TextEditor.Undo
{
    /// <summary>
    ///     This class stacks the last x operations from the undostack and makes
    ///     one undo/redo operation from it.
    /// </summary>
    internal sealed class UndoQueue : IUndoableOperation
    {
        private readonly List<IUndoableOperation> undolist = new List<IUndoableOperation>();

        /// <summary>
        /// </summary>
        public UndoQueue(Stack<IUndoableOperation> stack, int numops)
        {
            if (stack == null)
                throw new ArgumentNullException(nameof(stack));

            Debug.Assert(numops > 0, "ICSharpCode.TextEditor.Undo.UndoQueue : numops should be > 0");
            if (numops > stack.Count)
                numops = stack.Count;

            for (var i = 0; i < numops; ++i)
                undolist.Add(stack.Pop());
        }

        public void Undo()
        {
            for (var i = 0; i < undolist.Count; ++i)
                undolist[i].Undo();
        }

        public void Redo()
        {
            for (var i = undolist.Count - 1; i >= 0; --i)
                undolist[i].Redo();
        }
    }
}