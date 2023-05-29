// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Diagnostics;
using AeroWizard;
using ICSharpCode.SharpZipLib.Zip;
using NanoByte.Common.Net;
using ZeroInstall.Commands.Desktop;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Services;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store.Configuration;

namespace ZeroInstall.Central.WinForms;

public sealed partial class SyncWizard : Form
{
    private readonly bool _machineWide;
    private readonly bool _troubleshooting;

    private SyncWizard(bool machineWide, bool troubleshooting = false)
    {
        InitializeComponent();

        _machineWide = machineWide;
        _troubleshooting = troubleshooting;

        if (_troubleshooting)
            Shown += delegate { wizardControl.NextPage(pageResetWelcome, skipCommit: true); };
    }

    /// <summary>
    /// Runs the Sync Setup wizard.
    /// </summary>
    /// <param name="machineWide">Configure Sync for machine-wide data instead of just for the current user.</param>
    /// <param name="owner">The parent window the displayed window is modal to; can be <c>null</c>.</param>
    public static void Setup(bool machineWide, IWin32Window? owner = null)
    {
        using var wizard = new SyncWizard(machineWide);
        wizard.ShowDialog(owner);
    }

    /// <summary>
    /// Runs the Sync Troubleshooting wizard.
    /// </summary>
    /// <param name="machineWide">Configure Sync for machine-wide data instead of just for the current user.</param>
    /// <param name="owner">The parent window the displayed window is modal to; can be <c>null</c>.</param>
    public static void Troubleshooting(bool machineWide, IWin32Window? owner = null)
    {
        using var wizard = new SyncWizard(machineWide, troubleshooting: true);
        wizard.ShowDialog(owner);
    }

    #region Shared
    private void Sync(SyncResetMode resetMode = SyncResetMode.None)
    {
        using var handler = new DialogTaskHandler(this);
        var services = new ServiceProvider(handler);
        using var sync = new SyncIntegrationManager(_config, services.FeedManager.GetFresh, handler, _machineWide);
        sync.Sync(resetMode);
    }

    private Config _config = default!;

    private void SyncWizard_Load(object sender, EventArgs e)
    {
        try
        {
            _config = Config.Load();
        }
        #region Error handling
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException)
        {
            Log.Error("Error loading config", ex);
            Msg.Inform(this, ex.Message, MsgSeverity.Error);
            Close();
        }
        #endregion
    }

    private bool SaveConfig()
    {
        try
        {
            _config.Save();
            return true;
        }
        #region Error handling
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException)
        {
            Log.Error("Error saving config", ex);
            Msg.Inform(this, ex.Message, MsgSeverity.Error);
            return false;
        }
        #endregion
    }

    private HttpClient BuildHttpClient() => new()
    {
        BaseAddress = _config.SyncServer,
        DefaultRequestHeaders =
        {
            Authorization = new NetworkCredential(_config.SyncServerUsername, _config.SyncServerPassword).ToBasicAuth(),
            CacheControl = new() {NoCache = true}
        },
        Timeout = TimeSpan.FromSeconds(20)
    };
    #endregion

    #region pageSetupWelcome
    private bool _existingAccount;

    private void buttonSetupFirst_Click(object sender, EventArgs e)
    {
        _existingAccount = false;
        wizardControl.NextPage();
    }

    private void buttonSetupSubsequent_Click(object sender, EventArgs e)
    {
        _existingAccount = true;
        wizardControl.NextPage();
    }
    #endregion

    #region pageServer
    private void pageServer_InputChanged(object sender, EventArgs e)
    {
        textBoxCustomServer.Enabled = optionCustomServer.Checked;
        textBoxFileShare.Enabled = buttonFileShareBrowse.Enabled = optionFileShare.Checked;

        try
        {
            pageServer.AllowNext =
                optionOfficialServer.Checked ||
                (optionCustomServer.Checked && !string.IsNullOrEmpty(textBoxCustomServer.Text) && textBoxCustomServer.IsValid) ||
                (optionFileShare.Checked && !string.IsNullOrEmpty(textBoxFileShare.Text) && Path.IsPathRooted(textBoxFileShare.Text));
        }
        catch (ArgumentException)
        {
            pageServer.AllowNext = false;
        }
    }

    private void linkCustomServer_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        try
        {
            Process.Start("https://docs.0install.net/details/sync/");
        }
        #region Error handling
        catch (Exception ex)
        {
            Msg.Inform(this, ex.Message, MsgSeverity.Error);
        }
        #endregion
    }

    private void buttonFileShareBrowse_Click(object sender, EventArgs e)
    {
        using var openFileDialog = new FolderBrowserDialog {RootFolder = Environment.SpecialFolder.MyComputer};
        if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            textBoxFileShare.Text = openFileDialog.SelectedPath;
    }

    private void pageServer_Commit(object sender, WizardPageConfirmEventArgs e)
    {
        if (optionOfficialServer.Checked)
        {
            _config.SyncServer = new(Config.DefaultSyncServer);
            pageServer.NextPage = _existingAccount ? pageCredentials : pageRegister;
        }
        else if (optionCustomServer.Checked)
        {
            if (textBoxCustomServer.Uri?.Scheme != "https")
            {
                if (!Msg.YesNo(this, Resources.UnencryptedSyncServer, MsgSeverity.Warn))
                {
                    e.Cancel = true;
                    return;
                }
            }

            _config.SyncServer = new(textBoxCustomServer.Uri!);
            pageServer.NextPage = pageCredentials;
        }
        else if (optionFileShare.Checked)
        {
            try
            {
                _config.SyncServer = new(textBoxFileShare.Text);
            }
            #region Sanity checks
            catch (UriFormatException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
                e.Cancel = true;
                return;
            }
            #endregion

            _config.SyncServerUsername = "";
            _config.SyncServerPassword = "";
            pageServer.NextPage = _existingAccount ? pageExistingCryptoKey : pageNewCryptoKey;
        }
        else e.Cancel = true;
    }
    #endregion

    #region pageRegister
    private void linkRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        try
        {
            Process.Start(Config.DefaultSyncServer + "register");
        }
        #region Error handling
        catch (Exception ex)
        {
            Msg.Inform(this, ex.Message, MsgSeverity.Error);
        }
        #endregion
    }
    #endregion

    #region pageCredentials
    private void textBoxCredentials_TextChanged(object sender, EventArgs e)
        => pageCredentials.AllowNext = !string.IsNullOrEmpty(textBoxUsername.Text) && !string.IsNullOrEmpty(textBoxPassword.Text);

    private void pageCredentials_Commit(object sender, WizardPageConfirmEventArgs e)
    {
        _config.SyncServerUsername = textBoxUsername.Text;
        _config.SyncServerPassword = textBoxPassword.Text;

        try
        {
            using var handler = new DialogTaskHandler(this);
            handler.RunTask(new SimpleTask(Text, CheckCredentials));
        }
        #region Error handling
        catch (WebException ex)
        {
            Msg.Inform(this, ex.Message, MsgSeverity.Warn);
            e.Cancel = true;
            return;
        }
        catch (OperationCanceledException)
        {
            e.Cancel = true;
            return;
        }
        #endregion

        pageCredentials.NextPage = _existingAccount ? pageExistingCryptoKey : pageNewCryptoKey;
    }

    private void CheckCredentials()
    {
        using var httpClient = BuildHttpClient();
        try
        {
            using var response = httpClient.Send(new(HttpMethod.Head, "app-list"));
            if (response.StatusCode == HttpStatusCode.Unauthorized) throw new WebException(Resources.SyncCredentialsInvalid);
            response.EnsureSuccessStatusCode();
        }
        #region Error handling
        catch (HttpRequestException ex)
        {
            // Convert exception since only certain exception types are allowed
            throw ex.AsWebException();
        }
        #endregion
    }
    #endregion

    #region pageExistingCryptoKey
    private void pageExistingCryptoKey_Initialize(object sender, WizardPageInitEventArgs e)
        => textBoxCryptoKey.Text = _config.SyncCryptoKey;

    private void buttonForgotKey_Click(object sender, EventArgs e)
        => wizardControl.NextPage(pageResetCryptoKey, skipCommit: true);

    private void textBoxCryptoKey_TextChanged(object sender, EventArgs e)
        => pageExistingCryptoKey.AllowNext = !string.IsNullOrEmpty(textBoxCryptoKey.Text);

    private void pageExistingCryptoKey_Commit(object sender, WizardPageConfirmEventArgs e)
    {
        _config.SyncCryptoKey = textBoxCryptoKey.Text;

        try
        {
            using var handler = new DialogTaskHandler(this);
            handler.RunTask(new SimpleTask(Text, CheckCryptoKey));
        }
        #region Error handling
        catch (Exception ex) when (ex is WebException or InvalidDataException)
        {
            Msg.Inform(this, ex.Message, MsgSeverity.Warn);
            e.Cancel = true;
        }
        catch (OperationCanceledException)
        {
            e.Cancel = true;
        }
        #endregion

        pageExistingCryptoKey.NextPage = _troubleshooting ? pageChangeCryptoKey : pageSetupFinished;
    }

    private void CheckCryptoKey()
    {
        using var httpClient = BuildHttpClient();
        try
        {
            using var response = httpClient.Send(new(HttpMethod.Get, "app-list"));
            if (response.StatusCode == HttpStatusCode.Unauthorized) throw new WebException(Resources.SyncCredentialsInvalid);
            response.EnsureSuccessStatusCode();

            AppList.LoadXmlZip(response.EnsureSuccessStatusCode().Content.ReadAsStream(), _config.SyncCryptoKey);
        }
        #region Error handling
        catch (HttpRequestException ex)
        {
            // Convert exception since only certain exception types are allowed
            throw ex.AsWebException();
        }
        catch (ZipException ex)
        {
            // Wrap exception to add context information
            if (ex.Message == "Invalid password for AES") throw new InvalidDataException(Resources.SyncCryptoKeyInvalid);
            else throw new InvalidDataException(Resources.SyncServerDataDamaged, ex);
        }
        #endregion
    }
    #endregion

    #region pageResetCryptoKey
    private void pageResetCryptoKey_Initialize(object sender, WizardPageInitEventArgs e)
        => textBoxCryptoKeyReset.Text = StringUtils.GeneratePassword(16);

    private void textBoxCryptoKeyReset_TextChanged(object sender, EventArgs e)
        => pageResetCryptoKey.AllowNext = !string.IsNullOrEmpty(textBoxCryptoKeyReset.Text);

    private void pageResetCryptoKey_Commit(object sender, WizardPageConfirmEventArgs e)
    {
        _config.SyncCryptoKey = textBoxCryptoKeyReset.Text;

        try
        {
            Sync(SyncResetMode.Server);
        }
        #region Error handling
        catch (Exception ex) when (ex is WebException or InvalidDataException)
        {
            Log.Warn("Sync crypto key reset failed", ex);
            Msg.Inform(this, ex.Message, MsgSeverity.Warn);
            e.Cancel = true;
            return;
        }
        catch (OperationCanceledException)
        {
            e.Cancel = true;
            return;
        }
        #endregion

        if (_troubleshooting)
        {
            if (!SaveConfig())
                e.Cancel = true;
        }
    }
    #endregion

    #region pageCryptoKeyChanged
    private void pageCryptoKeyChanged_Initialize(object sender, WizardPageInitEventArgs e)
    {
        if (_troubleshooting)
        {
            pageCryptoKeyChanged.IsFinishPage = true;
            pageCryptoKeyChanged.ShowCancel = false;
        }
        else
            pageCryptoKeyChanged.NextPage = pageSetupFinished;
    }
    #endregion

    #region pageNewCryptoKey
    private void pageNewCryptoKey_Initialize(object sender, WizardPageInitEventArgs e)
        => textBoxCryptoKeyNew.Text = StringUtils.GeneratePassword(16);

    private void textBoxCryptoKeyNew_TextChanged(object sender, EventArgs e)
        => pageNewCryptoKey.AllowNext = !string.IsNullOrEmpty(textBoxCryptoKeyNew.Text);

    private void pageNewCryptoKey_Commit(object sender, WizardPageConfirmEventArgs e)
        => _config.SyncCryptoKey = textBoxCryptoKeyNew.Text;
    #endregion

    #region pageSetupFinished
    private void pageSetupFinished_Commit(object sender, WizardPageConfirmEventArgs e)
    {
        if (!SaveConfig())
        {
            e.Cancel = true;
            return;
        }

        CommandUtils.Start(SyncApps.Name);
    }
    #endregion

    #region pageResetWelcome
    private void buttonChangeCryptoKey_Click(object sender, EventArgs e)
        => wizardControl.NextPage(pageExistingCryptoKey);

    private void buttonResetServer_Click(object sender, EventArgs e)
        => wizardControl.NextPage(pageResetServer);

    private void buttonResetClient_Click(object sender, EventArgs e)
        => wizardControl.NextPage(pageResetClient);
    #endregion

    #region pageChangeCryptoKey
    private void pageChangeCryptoKey_Initialize(object sender, WizardPageInitEventArgs e)
        => textBoxCryptoKeyChange.Text = StringUtils.GeneratePassword(16);

    private void textBoxCryptoKeyChange_TextChanged(object sender, EventArgs e)
        => pageChangeCryptoKey.AllowNext = !string.IsNullOrEmpty(textBoxCryptoKeyChange.Text);

    private void pageChangeCryptoKey_Commit(object sender, WizardPageConfirmEventArgs e)
    {
        string oldKey = _config.SyncCryptoKey;

        void Rollback()
        {
            _config.SyncCryptoKey = oldKey;
            e.Cancel = true;
        }

        void Error(Exception ex)
        {
            Rollback();
            Log.Warn("Sync crypto key change failed", ex);
            Msg.Inform(this, ex.Message, MsgSeverity.Warn);
        }

        try
        {
            Sync();
            _config.SyncCryptoKey = textBoxCryptoKeyChange.Text;
            Sync(SyncResetMode.Server);
        }
        #region Error handling
        catch (WebException ex)
        {
            Error(ex);
            return;
        }
        catch (InvalidDataException ex)
        {
            Error(ex);
            return;
        }
        catch (OperationCanceledException)
        {
            Rollback();
            return;
        }
        #endregion

        if (_troubleshooting)
        {
            if (!SaveConfig())
                e.Cancel = true;
        }
    }
    #endregion

    #region pageResetServer
    private void pageResetServer_Commit(object sender, WizardPageConfirmEventArgs e)
        => ResetSync("server");
    #endregion

    #region pageResetClient
    private void pageResetClient_Commit(object sender, WizardPageConfirmEventArgs e)
        => ResetSync("client");

    private void ResetSync(string target)
        => CommandUtils.Start(_machineWide
            ? new[] {SyncApps.Name, $"--reset={target}", "--machine"}
            : new[] {SyncApps.Name, $"--reset={target}"});
    #endregion
}
