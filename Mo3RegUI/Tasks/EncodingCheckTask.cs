using System;

namespace Mo3RegUI.Tasks
{

    public class EncodingCheckTaskParameter : ITaskParameter
    {
    }
    public class EncodingCheckTask : ITask
    {
        public string Description => "检查系统 ANSI 代码页";

        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is EncodingCheckTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }
        private void _DoWork(EncodingCheckTaskParameter p)
        {
            uint codepage = NativeMethods.GetACP();
            if (codepage == 65001)
            {
                string message = "当前 ANSI 代码页为 UTF-8。这是一个好做法，但不幸的是，红警 2 游戏中将无法正常输入非英文字符，地图编辑器等组件将无法完全显示包含非英文字符的名称。此外，这是影响高 DPI 下游戏菜单界面是否出现按钮错位、重叠问题的因素之一。";
                ReportMessage(this, new TaskMessageEventArgs() { Level = MessageLevel.Warning, Text = message });
            }
        }
    }
}
