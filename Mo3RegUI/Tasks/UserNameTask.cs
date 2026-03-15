using Mo3RegUI.LocalizationResources;
using System;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;

namespace Mo3RegUI.Tasks
{
    public class UserNameTaskParameter : ITaskParameter
    {
        public string GameDir;
    }
    public class UserNameTask : ITask
    {
        // UserNameTask_Description: Set Game Username
        public string Description => TextResource.UserNameTask_Description;
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is UserNameTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(UserNameTaskParameter p)
        {
            lock (Locks.RA2MO_INI)
            {
                MyIniParserHelper.EditIniFile(Path.Combine(p.GameDir, Constants.GameConfigIniName), ini =>
                {
                    // Get username from RA2MO.ini
                    var section = MyIniParserHelper.GetSectionOrNew(ini, "MultiPlayer");
                    if (!section.ContainsKey("Handle"))
                    {
                        section["Handle"] = string.Empty;
                    }
                    string username = section["Handle"];
                    if (string.IsNullOrWhiteSpace(username))
                    {
                        username = this.GetWindowsUserName();
                    }
                    if (!this.IsAsciiString(username))
                    {
                        ReportMessage(this, new TaskMessageEventArgs()
                        {
                            Level = MessageLevel.Warning,
                            // UserNameTask_NonAsciiUsername: Note: The current player nickname "{0}" contains non-ASCII characters. ...
                        Text = string.Format(TextResource.UserNameTask_NonAsciiUsername, username),
                        });
                    }

                    username = this.GetAsciiString(username);

                    // valid ascii char: 32 <= char <=127 ; remove other chars
                    username = new string(username.ToList().Where(c => c is >= (char)32 and <= (char)127).ToArray());

                    if (string.IsNullOrWhiteSpace(username))
                    {
                        username = "NewPlayer";
                    }

                    section["Handle"] = username;

                    ReportMessage(this, new TaskMessageEventArgs()
                    {
                        Level = MessageLevel.Info,
                        // UserNameTask_SetUsername: Setting player nickname to "{0}".
                        Text = string.Format(TextResource.UserNameTask_SetUsername, username),
                    });
                });
            }
        }
        private string GetWindowsUserName() => WindowsIdentity.GetCurrent().Name.Split(new char[] { '\\' }, 2)[1];

        private bool IsAsciiString(string str)
        {
            string asciiStr = this.GetAsciiString(str);
            return string.Compare(str, asciiStr, StringComparison.Ordinal) == 0;
        }

        private string GetAsciiString(string str)
        {
            byte[] asciiBytes = Encoding.ASCII.GetBytes(str);
            string asciiStr = System.Text.Encoding.ASCII.GetString(asciiBytes);
            return asciiStr;
        }

    }
}
