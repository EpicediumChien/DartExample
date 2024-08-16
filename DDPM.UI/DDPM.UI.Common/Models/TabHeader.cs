using DDPM.UI.Common.Interfaces;

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