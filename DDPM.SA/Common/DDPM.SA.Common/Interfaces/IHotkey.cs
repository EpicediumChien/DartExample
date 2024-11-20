using Dell.Client.Framework.Common;
using System;
using System.Windows.Forms;

namespace DDPM.SA.Common
{
    public interface IHotkey : IFrameworkPlugin
    {
        bool Hook();

        bool Unhook();

        event KeyEventHandler KeyUp;

        //Robert_Lin, 2024-11-20 add KeyDown for EasyArrange need to handle when [Shift] key down/up
        event KeyEventHandler KeyDown;

        bool IsKeyPushedDown(System.Windows.Forms.Keys vKey);

        IntPtr GetHookHandle();
    }
}