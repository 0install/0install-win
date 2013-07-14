/*
 * Copyright 2010-2013 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using Common.Storage;
using Common.Tasks;
using ZeroInstall.Model;
using ICommandExecutor = Common.Undo.ICommandExecutor;

namespace ZeroInstall.Publish.WinForms.Controls
{
    /// <summary>
    /// Edits <see cref="SingleFile"/> instances.
    /// </summary>
    public class SingleFileEditor : DownloadRetrievalMethodEditor<SingleFile>
    {
        protected override TemporaryDirectory Download(ITaskHandler handler, ICommandExecutor executor)
        {
            return ImplementationUtils.DownloadSingleFile(Target, handler, executor);
        }
    }
}