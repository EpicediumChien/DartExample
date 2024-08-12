using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.UI.Plugin.KeyboardPlugin {
  internal class KeyInfo {
    public bool IsVisible { get; set; } = false;
    public string Tooltip { get; set; } = "";
    public string ImageFile { get; set; } = "";
    public KeyInfo() { }
  }
}
