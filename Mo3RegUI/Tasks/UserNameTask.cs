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
        public string Description => "设置游戏用户名";
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
                MyIniParserHelper.EditIniFile(Path.Combine(p.GameDir, "RA2MO.INI"), ini =>
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
                            Text = "注意，当前玩家昵称 \"" + username + "\" 包含非 ASCII 字符。如果玩家昵称完全不包含任何 ASCII 字符，Ares 3.0 将会崩溃。",
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
                        Text = "将玩家昵称设置为 \"" + username + "\"。",
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
