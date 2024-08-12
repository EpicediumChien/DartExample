using DDPM.SA.Common.Display;
using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DDPM.SA.Common
{
    public interface IHotkey : IFrameworkPlugin
    {
        bool Hook();
        bool Unhook();

        event KeyEventHandler KeyUp;
        bool IsKeyPushedDown(System.Windows.Forms.Keys vKey);
    }
}
