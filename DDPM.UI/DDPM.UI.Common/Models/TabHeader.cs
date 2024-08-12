using DDPM.UI.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.UI.Common.Models
{
    public class TabHeader : ITabHeader
    {
        private string _text = "a";
        public string Text 
        { 
            get => _text;
            set => _text = value;
        }
    }
}
