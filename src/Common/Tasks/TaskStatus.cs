/*
 * Copyright 2006-2014 Bastian Eicher
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

namespace NanoByte.Common.Tasks
{
    /// <summary>
    /// Represents different states a (usually Web- or IO-related) task can be in.
    /// </summary>
    /// <seealso cref="TaskSnapshot.Status"/>
    public enum TaskStatus
    {
        /// <summary>The task is ready to begin.</summary>
        Ready,

        /// <summary>The thread has just been started.</summary>
        Started,

        /// <summary>Handling the header.</summary>
        Header,

        /// <summary>Handling the actual data.</summary>
        Data,

        /// <summary>The task has been completed sucessfully.</summary>
        Complete,

        /// <summary>An error occurred during the task.</summary>
        WebError,

        /// <summary>An error occurred while writing the file.</summary>
        IOError,

        /// <summary>The task was canceled by the user before completion.</summary>
        Canceled
    }
}
