/*
 * Copyright 2010-2016 Bastian Eicher
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

using System;
using System.Linq;
using System.Reflection;
using LinFu.DynamicProxy;
using NanoByte.Common;

namespace ZeroInstall.OneGet
{
    /// <summary>
    /// Provides reflection-related helper methods.
    /// </summary>
    public static class ReflectionUtils
    {
        /// <summary>
        /// Forwards a method invocation <paramref name="info"/> to a <paramref name="target"/> using duck-typing.
        /// </summary>
        public static object DuckType(this object target, InvocationInfo info)
        {
            var method = target.GetType().GetMethod(
                info.TargetMethod.Name,
                info.TargetMethod.GetParameters().Select(x => x.ParameterType).ToArray());
            if (method == null) throw new InvalidOperationException("Unable to find suitable method for duck-typing: " + info.TargetMethod.Name);

            try
            {
                return method.Invoke(target, info.Arguments);
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                throw ex.InnerException.PreserveStack();
            }
        }
    }
}