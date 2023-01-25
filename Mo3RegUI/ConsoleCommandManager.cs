namespace Mo3RegUI
{
    public static class ConsoleCommandManager
    {
        public static void RunConsoleCommand(string command, string argument, out int exitCode, out string stdOut, out string stdErr)
        {
            var process = new System.Diagnostics.Process()
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = command,
                    Arguments = argument,
                    RedirectStandardInput = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            _ = process.Start();
            process.WaitForExit();

            stdOut = process.StandardOutput.ReadToEnd();
            stdErr = process.StandardError.ReadToEnd();
            exitCode = process.ExitCode;
        }
    }
}
