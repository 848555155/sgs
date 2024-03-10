namespace wyDay.Controls;

public class AUTranslation
{
    internal const string C_PrematureExitTitle = "wyUpdate exited prematurely";
    internal const string C_PrematureExitMessage = "wyUpdate ended before the current update step could be completed.";

#if WPF
    // wpf menus require underscores for hint characters (not working, look into this)
    private string m_CheckForUpdatesMenu = "检查更新"; //"Check for updates";
    private string m_DownloadUpdateMenu = "下载并安装更新"; // "Download and Update now";
    private string m_InstallUpdateMenu = "安装更新"; // "Install update now";
    private string m_CancelUpdatingMenu = "取消更新"; // "Cancel updating";
    private string m_CancelCheckingMenu = "取消检查更新"; // "Cancel update checking";
#else
    string m_CheckForUpdatesMenu = "&Check for updates";
    string m_DownloadUpdateMenu = "&Download and Update now";
    string m_InstallUpdateMenu = "&Install update now";
    string m_CancelUpdatingMenu = "&Cancel updating";
    string m_CancelCheckingMenu = "&Cancel update checking";
#endif

    private string m_HideMenu = "Hide";
    private string m_ViewChangesMenu = "View changes in version %version%";
    private string m_PrematureExitTitle = C_PrematureExitTitle;
    private string m_PrematureExitMessage = C_PrematureExitMessage;
    private string m_StopChecking = "Stop checking for updates for now";
    private string m_StopDownloading = "Stop downloading update for now";
    private string m_StopExtracting = "Stop extracting update for now";
    private string m_TryAgainLater = "Try again later";
    private string m_TryAgainNow = "Try again now";
    private string m_ViewError = "View error details";
    private string m_CloseButton = "Close";
    private string m_ErrorTitle = "Error";
    private string m_UpdateNowButton = "Update now";
    private string m_ChangesInVersion = "Changes in version %version%";
    private string m_FailedToCheck = "Failed to check for updates.";
    private string m_FailedToDownload = "Failed to download the update.";
    private string m_FailedToExtract = "Failed to extract the update.";
    private string m_Checking = "正在检查更新"; // "Checking for updates";
    private string m_Downloading = "正在下载更新"; // "Downloading update";
    private string m_Extracting = "正在解压缩更新文件"; // "Extracting update";
    private string m_UpdateAvailable = "可以安装更新"; // "Update is ready to be installed.";
    private string m_InstallOnNextStart = "重新启动程序以完成更新的安装"; // "Update will be installed on next start.";

    private string m_AlreadyUpToDate = "当前的版本是最新的"; // "You already have the latest version";
    private string m_SuccessfullyUpdated = "更新成功，最新版本号为%version%"; // "Successfully updated to %version%";
    private string m_UpdateFailed = "安装更新失败"; // "Update failed to install.";

    public string CheckForUpdatesMenu
    {
        get { return m_CheckForUpdatesMenu; }
        set { m_CheckForUpdatesMenu = value; }
    }

    private bool ShouldSerializeCheckForUpdatesMenu() { return false; }

    public string FailedToCheck
    {
        get { return m_FailedToCheck; }
        set { m_FailedToCheck = value; }
    }

    private bool ShouldSerializeFailedToCheck() { return false; }

    public string FailedToDownload
    {
        get { return m_FailedToDownload; }
        set { m_FailedToDownload = value; }
    }

    private bool ShouldSerializeFailedToDownload() { return false; }

    public string FailedToExtract
    {
        get { return m_FailedToExtract; }
        set { m_FailedToExtract = value; }
    }

    private bool ShouldSerializeFailedToExtract() { return false; }

    public string Checking
    {
        get { return m_Checking; }
        set { m_Checking = value; }
    }

    private bool ShouldSerializeChecking() { return false; }

    public string Downloading
    {
        get { return m_Downloading; }
        set { m_Downloading = value; }
    }

    private bool ShouldSerializeDownloading() { return false; }

    public string Extracting
    {
        get { return m_Extracting; }
        set { m_Extracting = value; }
    }

    private bool ShouldSerializeExtracting() { return false; }

    public string UpdateAvailable
    {
        get { return m_UpdateAvailable; }
        set { m_UpdateAvailable = value; }
    }

    private bool ShouldSerializeUpdateAvailable() { return false; }

    public string InstallOnNextStart
    {
        get { return m_InstallOnNextStart; }
        set { m_InstallOnNextStart = value; }
    }

    private bool ShouldSerializeInstallOnNextStart() { return false; }

    public string SuccessfullyUpdated
    {
        get { return m_SuccessfullyUpdated; }
        set { m_SuccessfullyUpdated = value; }
    }

    private bool ShouldSerializeSuccessfullyUpdated() { return false; }

    public string UpdateFailed
    {
        get { return m_UpdateFailed; }
        set { m_UpdateFailed = value; }
    }

    private bool ShouldSerializeUpdateFailed() { return false; }

    public string AlreadyUpToDate
    {
        get { return m_AlreadyUpToDate; }
        set { m_AlreadyUpToDate = value; }
    }

    private bool ShouldSerializeAlreadyUpToDate() { return false; }

    public string DownloadUpdateMenu
    {
        get { return m_DownloadUpdateMenu; }
        set { m_DownloadUpdateMenu = value; }
    }

    private bool ShouldSerializeDownloadUpdateMenu() { return false; }

    public string InstallUpdateMenu
    {
        get { return m_InstallUpdateMenu; }
        set { m_InstallUpdateMenu = value; }
    }

    private bool ShouldSerializeInstallUpdateMenu() { return false; }

    public string CancelUpdatingMenu
    {
        get { return m_CancelUpdatingMenu; }
        set { m_CancelUpdatingMenu = value; }
    }

    private bool ShouldSerializeCancelUpdatingMenu() { return false; }

    public string CancelCheckingMenu
    {
        get { return m_CancelCheckingMenu; }
        set { m_CancelCheckingMenu = value; }
    }

    private bool ShouldSerializeCancelCheckingMenu() { return false; }

    public string HideMenu
    {
        get { return m_HideMenu; }
        set { m_HideMenu = value; }
    }

    private bool ShouldSerializeHideMenu() { return false; }

    public string ViewChangesMenu
    {
        get { return m_ViewChangesMenu; }
        set { m_ViewChangesMenu = value; }
    }

    private bool ShouldSerializeViewChangesMenu() { return false; }

    public string PrematureExitTitle
    {
        get { return m_PrematureExitTitle; }
        set { m_PrematureExitTitle = value; }
    }

    private bool ShouldSerializePrematureExitTitle() { return false; }

    public string PrematureExitMessage
    {
        get { return m_PrematureExitMessage; }
        set { m_PrematureExitMessage = value; }
    }

    private bool ShouldSerializePrematureExitMessage() { return false; }

    public string StopChecking
    {
        get { return m_StopChecking; }
        set { m_StopChecking = value; }
    }

    private bool ShouldSerializeStopChecking() { return false; }

    public string StopDownloading
    {
        get { return m_StopDownloading; }
        set { m_StopDownloading = value; }
    }

    private bool ShouldSerializeStopDownloading() { return false; }

    public string StopExtracting
    {
        get { return m_StopExtracting; }
        set { m_StopExtracting = value; }
    }

    private bool ShouldSerializeStopExtracting() { return false; }

    public string TryAgainLater
    {
        get { return m_TryAgainLater; }
        set { m_TryAgainLater = value; }
    }

    private bool ShouldSerializeTryAgainLater() { return false; }

    public string TryAgainNow
    {
        get { return m_TryAgainNow; }
        set { m_TryAgainNow = value; }
    }

    private bool ShouldSerializeTryAgainNow() { return false; }

    public string ViewError
    {
        get { return m_ViewError; }
        set { m_ViewError = value; }
    }

    private bool ShouldSerializeViewError() { return false; }

    public string CloseButton
    {
        get { return m_CloseButton; }
        set { m_CloseButton = value; }
    }

    private bool ShouldSerializeCloseButton() { return false; }

    public string ErrorTitle
    {
        get { return m_ErrorTitle; }
        set { m_ErrorTitle = value; }
    }

    private bool ShouldSerializeErrorTitle() { return false; }

    public string UpdateNowButton
    {
        get { return m_UpdateNowButton; }
        set { m_UpdateNowButton = value; }
    }

    private bool ShouldSerializeUpdateNowButton() { return false; }

    public string ChangesInVersion
    {
        get { return m_ChangesInVersion; }
        set { m_ChangesInVersion = value; }
    }

    private bool ShouldSerializeChangesInVersion() { return false; }
}