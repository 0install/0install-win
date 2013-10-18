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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common;
using Common.Controls;
using Common.Storage;
using ZeroInstall.Model;
using ZeroInstall.Publish.EntryPoints;

namespace ZeroInstall.Publish.WinForms.Wizards
{
    /// <summary>
    /// A wizard guiding the user through creating a new <see cref="Feed"/>.
    /// </summary>
    public partial class NewFeedWizard : Wizard
    {
        public FeedEditing FeedEditing { get; private set; }

        #region Pages
        private readonly SourcePage _sourcePage = new SourcePage();

        private readonly DownloadRetrievalMethodPage _archivePage = new DownloadRetrievalMethodPage("Archive");
        private readonly DownloadRetrievalMethodOnlinePage<Archive> _archiveOnlinePage = new DownloadRetrievalMethodOnlinePage<Archive>("Archive");
        private readonly DownloadRetrievalMethodLocalPage<Archive> _archiveLocalPage = new DownloadRetrievalMethodLocalPage<Archive>("Archive");
        private readonly ArchiveExtractPage _archiveExtractPage = new ArchiveExtractPage();

        private readonly DownloadRetrievalMethodPage _singleFilePage = new DownloadRetrievalMethodPage("Single Executable");
        private readonly DownloadRetrievalMethodOnlinePage<SingleFile> _singleFileOnlinePage = new DownloadRetrievalMethodOnlinePage<SingleFile>("Single Executable");
        private readonly DownloadRetrievalMethodLocalPage<SingleFile> _singleFileLocalPage = new DownloadRetrievalMethodLocalPage<SingleFile>("Single Executable");

        private readonly SetupPage _setupPage = new SetupPage();

        private readonly EntryPointPage _entryPointPage = new EntryPointPage();
        private readonly DetailsPage _detailsPage = new DetailsPage();

        private readonly WindowsIconPage _windowsIconPage = new WindowsIconPage();
        private readonly SecurityPage _securityPage = new SecurityPage();
        private readonly DonePage _donePage = new DonePage();
        #endregion

        #region State
        private RetrievalMethod _retrievalMethod;
        private TemporaryDirectory _tempDirectory;
        private string _workingDirectory;
        private ManifestDigest _digest;
        private Candidate _candidate;
        #endregion

        public NewFeedWizard()
        {
            InitializeComponent();

            #region Page flows
            _sourcePage.Archive += () => PushPage(_archivePage);
            _sourcePage.SingleFile += () => PushPage(_singleFilePage);
            _sourcePage.Setup += () => PushPage(_setupPage);

            _archivePage.Online += () => PushPage(_archiveOnlinePage);
            _archivePage.Local += () => PushPage(_archiveLocalPage);
            _archiveOnlinePage.FileSelected += ArchiveSelected;
            _archiveLocalPage.FileSelected += ArchiveSelected;
            _archiveExtractPage.ExtractSelected += ArchiveExtractSelected;

            _singleFilePage.Online += () => PushPage(_singleFileOnlinePage);
            _singleFilePage.Local += () => PushPage(_singleFileLocalPage);
            _singleFileOnlinePage.FileSelected += FileSelected;
            _singleFileLocalPage.FileSelected += FileSelected;

            _entryPointPage.CandidateSelected += CandidateSelected;
            _detailsPage.DetailsFilledIn += DetailsFilledIn;
            _windowsIconPage.IconsSelected += IconsSelected;
            #endregion

            PushPage(_sourcePage);
        }

        #region Flow helpers
        private void ArchiveSelected(Archive archive, TemporaryDirectory tempDirectory)
        {
            SetRetrievalMethod(archive, tempDirectory);
            _archiveExtractPage.SetArchive(archive, tempDirectory);
            PushPage(_archiveExtractPage);
        }

        private void ArchiveExtractSelected(string workingDirectory)
        {
            GenerateDigest(workingDirectory);
            PushEntryPointPage();
        }

        private void FileSelected(SingleFile file, TemporaryDirectory tempDirectory)
        {
            SetRetrievalMethod(file, tempDirectory);
            GenerateDigest(_tempDirectory);

            var candidate = Detection.ListCandidates(new DirectoryInfo(tempDirectory)).FirstOrDefault();
            if (candidate == null) Msg.Inform(this, "Unknown executable type!", MsgSeverity.Warn);
            else CandidateSelected(candidate);
        }

        private void SetRetrievalMethod(RetrievalMethod method, TemporaryDirectory tempDirectory)
        {
            if (_tempDirectory != null) _tempDirectory.Dispose();

            _retrievalMethod = method;
            _tempDirectory = tempDirectory;
        }

        private void GenerateDigest(string workingDirectory)
        {
            _digest = ManifestUtils.CreateDigest(this, workingDirectory);
            _workingDirectory = workingDirectory;
        }

        private void PushEntryPointPage()
        {
            _entryPointPage.SetWorkingDirectory(_workingDirectory);
            PushPage(_entryPointPage);
        }

        private void CandidateSelected(Candidate candidate)
        {
            _candidate = candidate;
            _detailsPage.SetCandidate(candidate);
            PushPage(_detailsPage);
        }
        
        private void DetailsFilledIn()
        {
            var windowsExe = _candidate as WindowsExe;
            if (windowsExe == null) PushPage(_securityPage);
            else
            {
                _windowsIconPage.SetIcon(windowsExe.ExtractIcon());
                PushPage(_windowsIconPage);
            }
        }

        private void IconsSelected(IEnumerable<Icon> icons)
        {
            // TODO: Handle icons
            PushPage(_securityPage);
        }
        #endregion
    }
}
