namespace Mo3RegUI.MvvmContract.ViewServices
{
    /// <summary>
    /// Opens a URL in the user's default browser. The process launch is a platform concern and
    /// must not live in the ViewModel.
    /// </summary>
    public interface IUrlService
    {
        void OpenUrl(string url);
    }
}
