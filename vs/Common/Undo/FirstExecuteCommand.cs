using System;
using Common.Properties;

namespace Common.Undo
{
    /// <summary>
    /// An undo command that does something different on the first call to <see cref="Execute"/> than on subsequent redo calls
    /// </summary>
    public abstract class FirstExecuteCommand : IUndoCommand
    {
        #region Variables
        private bool _actionPerformed, _undoPerformed;
        #endregion

        //--------------------//

        #region Execute
        /// <summary>
        /// Performs the desired action
        /// </summary>
        public void Execute()
        {
            // We cannot perform the action repeatedly in a row
            if (_actionPerformed && !_undoPerformed) throw new InvalidOperationException(Resources.RedoNotAvailable);

            if (_actionPerformed) OnRedo(); else OnFirstExecute();

            // Action performed at least once, ready for undo
            _actionPerformed = true;
            _undoPerformed = false;
        }

        /// <summary>
        /// Hook to perform the desired action the first time
        /// </summary>
        protected abstract void OnFirstExecute();

        /// <summary>
        /// Hook to perform the desired action again
        /// </summary>
        protected abstract void OnRedo();
        #endregion

        #region Undo
        /// <summary>
        /// Undoes the changes made by <see cref="Execute"/>
        /// </summary>
        public virtual void Undo()
        {
            // If the action has not been performed yet, we cannnot undo it
            if (!_actionPerformed || _undoPerformed) throw new InvalidOperationException(Resources.UndoNotAvailable);

            OnUndo();

            // Ready for redo, don't undo again
            _undoPerformed = true;
        }

        /// <summary>
        /// Hook to undo the changes made by <see cref="OnFirstExecute"/> or <see cref="OnRedo"/>
        /// </summary>
        protected abstract void OnUndo();
        #endregion
    }
}
