using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace DDPM.UI.Common
{
    public class VbarItemViewModel : ObservableObject
    {
        private int _id;
        public int Id
        {
            get => _id;
            set
            {
                SetProperty(ref _id, value, "Id");
            }
        }

        private ImageSource? _icon;
        public ImageSource? Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                SetProperty(ref _icon, value);
            }
        }

        private string _text = "";
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                SetProperty(ref _text, value);
            }
        }

        #region Commands
        private ICommand? _itemClickCommand;

        /// <summary>
        /// ItemClickCommand
        /// </summary>
        public ICommand? ItemClickCommand
        {
            get => _itemClickCommand;
            set => SetProperty(ref _itemClickCommand, value);
        }
        #endregion

        #region IsLandingMode
        private bool _isLandingMode = true;
        public bool IsLandingMode
        {
            get => _isLandingMode;
            set => SetProperty(ref _isLandingMode, value);
        }
        #endregion

        #region State
        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
        #endregion
    }
}
