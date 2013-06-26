/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Common.Undo
{
    /// <summary>
    /// Controls editing a target using <see cref="IUndoCommand"/>s.
    /// </summary>
    /// <typeparam name="T">The type of the root object being edited.</typeparam>
    public abstract class CommandManager<T> : ICommandExecutor
    {
        #region Variables
        /// <summary>Entries used by the undo-system to undo changes</summary>
        protected readonly Stack<IUndoCommand> UndoStack = new Stack<IUndoCommand>();

        /// <summary>Entries used by the undo-system to redo changes previously undone</summary>
        protected readonly Stack<IUndoCommand> RedoStack = new Stack<IUndoCommand>();
        #endregion

        #region Properties
        /// <summary>
        /// The root object being edited.
        /// </summary>
        public abstract T Target { get; set; }

        /// <summary>
        /// The path of the file the <see cref="Target"/> was loaded from. <see langword="null"/> if none.
        /// </summary>
        public string Path { get; protected set; }

        /// <summary>
        /// Indicates whether the <see cref="Target"/> has unsaved changes.
        /// </summary>
        public bool Changed { get; private set; }
        #endregion

        #region Events
        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        public event Action Updated;

        /// <summary>
        /// Is raised when the availability of the <see cref="Undo"/> operation has changed.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Cannot rename System.Action<T>.")]
        public event Action<bool> UndoEnabled;

        /// <summary>
        /// Is raised when the availability of the <see cref="Redo"/> operation has changed.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Cannot rename System.Action<T>.")]
        public event Action<bool> RedoEnabled;

        protected void OnUpdated()
        {
            if (Updated != null) Updated();
        }

        protected void OnUndoEnabled(bool value)
        {
            if (UndoEnabled != null) UndoEnabled(value);
        }

        protected void OnRedoEnabled(bool value)
        {
            if (RedoEnabled != null) RedoEnabled(value);
        }
        #endregion

        #region Commands
        /// <inheritdoc/>
        public void Execute(IUndoCommand command)
        {
            #region Sanity checks
            if (command == null) throw new ArgumentNullException("command");
            #endregion

            // Execute the command and store it for later undo
            command.Execute();

            // Store the command and invalidate previous redo history
            UndoStack.Push(command);
            RedoStack.Clear();

            // Only enable the buttons that still have a use
            OnUndoEnabled(true);
            OnRedoEnabled(false);
            OnUpdated();

            Changed = true;
        }

        /// <summary>
        /// Undoes the last action performed by <see cref="Execute"/>.
        /// </summary>
        public void Undo()
        {
            if (UndoStack.Count == 0) return;

            // Remove last command from the undo list, execute it and add it to the redo list
            IUndoCommand lastCommand = UndoStack.Pop();
            lastCommand.Undo();
            RedoStack.Push(lastCommand);

            // Only enable the buttons that still have a use
            if (UndoStack.Count == 0)
            {
                OnUndoEnabled(false);
                Changed = false;
            }
            OnRedoEnabled(true);

            OnUpdated();
        }

        /// <summary>
        /// Redoes the last action undone by <see cref="Undo"/>.
        /// </summary>
        public void Redo()
        {
            if (RedoStack.Count == 0) return;

            // Remove last command from the redo list, execute it and add it to the undo list
            IUndoCommand lastCommand = RedoStack.Pop();
            lastCommand.Execute();
            UndoStack.Push(lastCommand);

            // Mark as "to be saved" again
            Changed = true;

            // Only enable the buttons that still have a use
            OnRedoEnabled(RedoStack.Count > 0);
            OnUndoEnabled(true);

            OnUpdated();
        }

        /// <summary>
        /// Resets the entire undo system, clearing all stacks.
        /// </summary>
        public void Reset()
        {
            Changed = false;

            UndoStack.Clear();
            RedoStack.Clear();

            OnUndoEnabled(false);
            OnRedoEnabled(false);
        }
        #endregion
    }
}
