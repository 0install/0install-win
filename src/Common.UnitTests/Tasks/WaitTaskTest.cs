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

using System;
using System.Threading;
using NUnit.Framework;

namespace NanoByte.Common.Tasks
{
    /// <summary>
    /// Contains test methods for <see cref="WaitTask"/>.
    /// </summary>
    [TestFixture]
    public class WaitTaskTest
    {
        [Test(Description = "Ensures the task waits until the wait handle is signaled.")]
        public void TestWait()
        {
            using (var waitHandle = new ManualResetEvent(false))
            {
                var task = new WaitTask("Test task", waitHandle);
                var waitThread = new Thread(() => task.Run());
                waitThread.Start();

                Thread.Sleep(100);
                Assert.AreEqual(TaskStatus.Started, task.Status);

                waitHandle.Set();
                waitThread.Join();
                Assert.AreEqual(TaskStatus.Complete, task.Status);
            }
        }

        [Test(Description = "Starts waiting for an everblocking handle using Run() and stops again right away using Cancel().")]
        public void TestCancel()
        {
            using (var waitHandle = new ManualResetEvent(false))
            {
                // Monitor for a cancellation exception
                var task = new WaitTask("Test task", waitHandle);
                bool exceptionThrown = false;
                var cancellationTokenSource = new CancellationTokenSource();
                var waitThread = new Thread(() =>
                {
                    try
                    {
                        task.Run(cancellationTokenSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        exceptionThrown = true;
                    }
                });

                // Start and then cancel the download
                waitThread.Start();
                Thread.Sleep(100);
                cancellationTokenSource.Cancel();
                waitThread.Join();

                Assert.IsTrue(exceptionThrown, task.Status.ToString());
            }
        }
    }
}
