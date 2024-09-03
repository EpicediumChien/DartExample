using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DDPM.UI.Plugin.Common {
  public static class Animation {
    static DoubleAnimation OpenAnimation = new() {
      From = 0,
      To = 180,
      Duration = new Duration(TimeSpan.FromSeconds(0.3))
    };
    static DoubleAnimation CloseAnimation = new() {
      From = 180,
      To = 0,
      Duration = new Duration(TimeSpan.FromSeconds(0.3))
    };
    public static void OpenSection(StackPanel stackpanel) {
      stackpanel.Measure(new Size(stackpanel.ActualWidth, double.PositiveInfinity));
      double desiredHeight = stackpanel.DesiredSize.Height;

      DoubleAnimation heightAnimation = new DoubleAnimation {
        From = 0,
        To = desiredHeight,
        Duration = new Duration(TimeSpan.FromSeconds(0.3))
      };

      stackpanel.BeginAnimation(StackPanel.HeightProperty, heightAnimation);
    }
    static Task BeginAnimationAsync(RotateTransform target, DependencyProperty property, AnimationTimeline animation) {
      var tcs = new TaskCompletionSource<bool>();
      var clock = animation.CreateClock();
      clock.Completed += (s, e) => tcs.SetResult(true);
      target.ApplyAnimationClock(property, clock);
      return tcs.Task;
    }

    static Task BeginAnimationAsync2(RotateTransform target1, AnimationTimeline animation1, RotateTransform target2, AnimationTimeline animation2) {
      var tcs = new TaskCompletionSource<bool>();
      var clock = animation1.CreateClock();
      var clock2 = animation2.CreateClock();
      clock.Completed += (s, e) => tcs.SetResult(true);
      target1.ApplyAnimationClock(RotateTransform.AngleProperty, clock);
      target2.ApplyAnimationClock(RotateTransform.AngleProperty, clock2);
      return tcs.Task;
    }
  }
}
