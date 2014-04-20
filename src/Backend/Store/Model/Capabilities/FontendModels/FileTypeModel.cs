/*
 * Copyright 2011 Simon E. Silva Lauinger
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

using System.Linq;
using NanoByte.Common.Utils;

namespace ZeroInstall.Store.Model.Capabilities.FontendModels
{
    /// <summary>
    /// Wraps a <see cref="FileType"/> for data binding.
    /// </summary>
    public class FileTypeModel : IconCapabilityModel
    {
        private readonly FileType _fileType;

        /// <summary>
        /// All <see cref="FileType.Extensions" /> concatenated with ", ".
        /// </summary>
        public string Extensions { get { return StringUtils.Join(", ", _fileType.Extensions.Select(extension => extension.Value)); } }

        /// <inheritdoc/>
        public FileTypeModel(FileType fileType, bool used) : base(fileType, used)
        {
            _fileType = fileType;
        }
    }
}
