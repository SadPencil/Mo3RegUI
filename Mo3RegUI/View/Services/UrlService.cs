using Mo3RegUI.MvvmContract.ViewServices;
using System.Diagnostics;

namespace Mo3RegUI.View.Services
{
    /// <summary>
    /// Opens URLs with the user's default browser. Launching a process is a platform concern, so
    /// it lives on the View side of the contract.
    /// </summary>
    public class UrlService : IUrlService
    {
        public void OpenUrl(string url)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo(url)
            };
            _ = process.Start();
        }
    }
}
