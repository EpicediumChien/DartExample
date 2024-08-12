using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace DDPM.UI.Plugin.Common {
  public static class Utility {
    public static T? FindParent<T>(DependencyObject child) where T : DependencyObject {
      DependencyObject parentObject = VisualTreeHelper.GetParent(child);

      if(parentObject == null)
        return null;

      if(parentObject is T parent) {
        return parent;
      }
      else {
        return FindParent<T>(parentObject);
      }
    }
  }
}
