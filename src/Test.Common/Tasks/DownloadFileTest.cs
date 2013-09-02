﻿/*
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
using System.IO;
using System.Net;
using System.Threading;
using Common.Storage;
using Common.Utils;
using NUnit.Framework;

namespace Common.Tasks
{
    /// <summary>
    /// Contains test methods for <see cref="DownloadFile"/>.
    /// </summary>
    [TestFixture]
    public class DownloadFileTest
    {
        private MicroServer _server;
        private TemporaryFile _tempFile;
        private const string TestFileContent = "abc";

        [SetUp]
        public void SetUp()
        {
            _server = new MicroServer("file", TestFileContent.ToStream());

            _tempFile = new TemporaryFile("unit-tests");
        }

        [TearDown]
        public void TearDown()
        {
            _server.Dispose();

            _tempFile.Dispose();
        }

        [Test(Description = "Downloads a small file using RunSync().")]
        public void TestRunSync()
        {
            // Download the file
            var download = new DownloadFile(_server.FileUri, _tempFile);
            download.RunSync();

            // Read the file
            string fileContent = File.ReadAllText(_tempFile);

            // Ensure the download was successful and the file is identical
            Assert.AreEqual(TaskState.Complete, download.State, download.ErrorMessage);
            Assert.AreEqual(TestFileContent, fileContent, "Downloaded file doesn't match original");
        }

        [Test(Description = "Downloads a small file using Start() and Join().")]
        public void TestThread()
        {
            // Start a background download of the file and then wait
            var download = new DownloadFile(_server.FileUri, _tempFile);
            download.Start();
            download.Join();

            // Read the file
            string fileContent = File.ReadAllText(_tempFile);

            // Ensure the download was successful and the file is identical
            Assert.AreEqual(TaskState.Complete, download.State, download.ErrorMessage);
            Assert.AreEqual(TestFileContent, fileContent, "Downloaded file doesn't match original");
        }

        [Test(Description = "Starts downloading a small file using Start() and stops again right away using Cancel().")]
        public void TestCancelAsync()
        {
            // Start a very slow download of the file and then cancel it right away again
            _server.Slow = true;
            var download = new DownloadFile(_server.FileUri, _tempFile);
            download.Start();
            download.Cancel();

            Assert.AreEqual(TaskState.Ready, download.State);
        }

        [Test(Description = "Starts downloading a small file using RunSync() and stops again right away using Cancel().")]
        public void TestCancelSync()
        {
            // Prepare a very slow download of the file and monitor for a cancellation exception
            _server.Slow = true;
            var download = new DownloadFile(_server.FileUri, _tempFile);
            bool exceptionThrown = false;
            var downloadThread = new Thread(() =>
            {
                try
                {
                    download.RunSync();
                }
                catch (OperationCanceledException)
                {
                    exceptionThrown = true;
                }
            });

            // Start and then cancel the download
            downloadThread.Start();
            Thread.Sleep(100);
            download.Cancel();
            downloadThread.Join();

            Assert.IsTrue(exceptionThrown, download.State.ToString());
        }

        [Test(Description = "Starts downloading a small file, stops again right away and then restarts the download allowing it to complete.")]
        public void TestCancelRestart()
        {
            // Start a very slow download of the file and then cancel it right away again
            _server.Slow = true;
            var download = new DownloadFile(_server.FileUri, _tempFile);
            download.Start();
            download.Cancel();

            // Restart the download with full speed
            _server.Slow = false;
            download.Start();
            download.Join();

            // Read the file
            string fileContent = File.ReadAllText(_tempFile);

            // Ensure the download was successful and the file is identical
            Assert.AreEqual(TaskState.Complete, download.State, download.ErrorMessage);
            Assert.AreEqual(TestFileContent, fileContent, "Downloaded file doesn't match original");
        }

        [Test(Description = "Ensure files with an incorrect size are rejected.")]
        public void TestIncorrectSize()
        {
            var download = new DownloadFile(_server.FileUri, _tempFile, 1024);
            Assert.Throws<WebException>(() => download.RunSync());
        }
    }
}
