using Dell.Client.Framework.Common;
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