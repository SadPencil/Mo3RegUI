using System;
using System.Collections.Generic;
using System.IO;

namespace Mo3RegUI.Tasks
{
    public class DirectXRuntimeTaskParameter : ITaskParameter
    {
    }
    public class DirectXRuntimeTask : ITask
    {
        public string Description => "检查 DirectX 运行时（2010 年 6 月）";
        public event EventHandler<TaskMessageEventArgs> ReportMessage;

        public void DoWork(ITaskParameter p)
        {
            if (p is DirectXRuntimeTaskParameter pp)
            {
                this._DoWork(pp);
            }
            else { throw new ArgumentException(); }
        }

        private static string[] dllNames = new string[] {
            "d3dcompiler_33.dll", "d3dcompiler_34.dll", "d3dcompiler_35.dll", "d3dcompiler_36.dll", "D3DCompiler_37.dll", "D3DCompiler_38.dll", "D3DCompiler_39.dll", "D3DCompiler_40.dll", "D3DCompiler_41.dll", "D3DCompiler_42.dll", "D3DCompiler_43.dll", 
            "d3dcsx_42.dll", "d3dcsx_43.dll", 
            "d3dx10.dll", "d3dx10_33.dll", "d3dx10_34.dll", "d3dx10_35.dll", "d3dx10_36.dll", "d3dx10_37.dll", "d3dx10_38.dll", "d3dx10_39.dll", "d3dx10_40.dll", "d3dx10_41.dll", "d3dx10_42.dll", "d3dx10_43.dll", 
            "d3dx11_42.dll", "d3dx11_43.dll", 
            "d3dx9_24.dll", "d3dx9_25.dll", "d3dx9_26.dll", "d3dx9_27.dll", "d3dx9_28.dll", "d3dx9_29.dll", "d3dx9_30.dll", "d3dx9_31.dll", "d3dx9_32.dll", "d3dx9_33.dll", "d3dx9_34.dll", "d3dx9_35.dll", "d3dx9_36.dll", "d3dx9_37.dll", "d3dx9_38.dll", "d3dx9_39.dll", "d3dx9_40.dll", "d3dx9_41.dll", "d3dx9_42.dll", "d3dx9_43.dll", 
            //"microsoft.directx.audiovideoplayback.dll", "microsoft.directx.diagnostics.dll", "microsoft.directx.direct3d.dll", "microsoft.directx.direct3dx.dll", "microsoft.directx.directdraw.dll", "microsoft.directx.directinput.dll", "microsoft.directx.directplay.dll", "microsoft.directx.directsound.dll", "microsoft.directx.dll", 
            "x3daudio1_0.dll", "x3daudio1_1.dll", "x3daudio1_2.dll", "X3DAudio1_3.dll", "X3DAudio1_4.dll", "X3DAudio1_5.dll", "X3DAudio1_6.dll", "X3DAudio1_7.dll", 
            "xactengine2_0.dll", "xactengine2_1.dll", "xactengine2_10.dll", "xactengine2_2.dll", "xactengine2_3.dll", "xactengine2_4.dll", "xactengine2_5.dll", "xactengine2_6.dll", "xactengine2_7.dll", "xactengine2_8.dll", "xactengine2_9.dll", "xactengine3_0.dll", "xactengine3_1.dll", "xactengine3_2.dll", "xactengine3_3.dll", "xactengine3_4.dll", "xactengine3_5.dll", "xactengine3_6.dll", "xactengine3_7.dll", 
            "XAPOFX1_0.dll", "XAPOFX1_1.dll", "XAPOFX1_2.dll", "XAPOFX1_3.dll", "XAPOFX1_4.dll", "XAPOFX1_5.dll", 
            "XAudio2_0.dll", "XAudio2_1.dll", "XAudio2_2.dll", "XAudio2_3.dll", "XAudio2_4.dll", "XAudio2_5.dll", "XAudio2_6.dll", "XAudio2_7.dll", 
            "xinput1_1.dll", "xinput1_2.dll", "xinput1_3.dll", "xinput9_1_0.dll",
        };
        private void _DoWork(DirectXRuntimeTaskParameter p)
        {
            var sysFolders = new HashSet<string>
            {
                Environment.GetFolderPath(Environment.SpecialFolder.SystemX86),
                Environment.GetFolderPath(Environment.SpecialFolder.System),
            };

            foreach (string sysFolder in sysFolders)
            {
                foreach (string dllName in dllNames)
                {
                    if (!File.Exists(Path.Combine(sysFolder, dllName)))
                    {
                        throw new Exception($"DirectX 运行时组件未安装。找不到 {dllName} 文件。请安装 DirectX End-User Runtimes (June 2010) 。");
                    }
                }
            }

        }
    }

}
