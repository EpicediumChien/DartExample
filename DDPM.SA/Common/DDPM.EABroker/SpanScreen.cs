using DDPM.SA.Common;
using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VcpCore.Common;

namespace DDPM.EABroker
{
    /// <summary>
    /// Used for Span across multiple monitors option.
    /// Span multiple EAScreens to a SpanScreen.
    /// </summary>
    public class SpanScreen
    {
        #region Private members
        private List<EAScreen> _eaScreens = new List<EAScreen>();
        private Rectangle _rcSpan = new Rectangle();
        private Rectangle _bounds = new Rectangle();
        private Rectangle _workingArea = new Rectangle();
        private bool _isHorzSpan = true;
        #endregion

        #region ctor
        public SpanScreen() 
        {
         }
        #endregion

        #region Properties
        public bool IsSpanEnabled => (_eaScreens.Count >= 2);
        public Rectangle Bounds => _bounds; 
        public Rectangle WorkingArea => _workingArea;
        public bool IsHorzSpan => _isHorzSpan;
        #endregion

        #region Reset
        public void Reset()
        {
            if (_eaScreens != null)
            {
                _eaScreens.Clear();
            }
            _eaScreens = new List<EAScreen>();
        }
        #endregion

        #region Detection
        public bool DetectSpanScreens(ILog? log, List<MonitorInfo>? monitors)
        {
            log?.Info("@ SpanScreen.DetectSpanScreens()");
            //Build a list of EAScreens, which contains Forms.Screen and real properties from Win32
            List<EAScreen> allScreens = EAScreen.GetEAScreens(monitors);
            //allScreens: all Screens with attached Monitors

            //To remove all EAScreens which as no Dell screent attached
            allScreens.RemoveAll(x => !x.HasAttachedMonitor);


            List<EAScreen> spanEaScreens = new List<EAScreen>();
            //For-loop to find a EAScreen which can Span with other EAScreens
            foreach (EAScreen eaScr in allScreens)
            {
                spanEaScreens = allScreens.FindAll(x => x.CanItSpanWithMe(eaScr, log));
                if ((spanEaScreens != null) && (spanEaScreens.Count > 0))
                {
                    //Add my self into the list
                    spanEaScreens.Add(eaScr);
                    break;
                }
            }
            if (spanEaScreens.Count < 2)
            {
                _eaScreens = new List<EAScreen>();
                return false;
            }

            //Check if the Span screens can be spaned with 3 or more screens
            if (spanEaScreens.Count > 2)
            {
                //Calculate the union rect
                Rectangle rcUnion = new Rectangle();
                int sumWidth = 0;
                int sumHeight = 0;
                foreach(EAScreen eaScr in spanEaScreens)
                {
                    Rectangle.Union(rcUnion, eaScr.Bounds);
                    sumWidth += eaScr.Bounds.Width;
                    sumHeight += eaScr.Bounds.Height;
                }

                int deltaWidth = Math.Abs(rcUnion.Width - sumWidth);
                int deltaHeight = Math.Abs(rcUnion.Height - sumHeight);
                if ((deltaWidth > 600) || (deltaHeight > 600)) 
                {
                    _eaScreens = new List<EAScreen>();
                    return false;
                }
            }
            _eaScreens = spanEaScreens;

            //Re-order and detect span orientation
            //1 Assume they are horizontal span, their top and bottom should be equal 
            //2 Assume they are vertial span, theri left and right should be equal
            int sumLeft = 0;
            int sumTop = 0;
            foreach (EAScreen eaScr in spanEaScreens)
            {
                sumLeft += eaScr.Bounds.Left;
                sumTop += eaScr.Bounds.Top;
            }
            int avgLeft = (sumLeft / spanEaScreens.Count);
            int avgTop = (sumTop / spanEaScreens.Count);

            //Validate horizontal span
            int criteria_ygap = 40;
            int criteria_xgap = 40;
            bool isHorzSpan = true;
            bool isVertSpan = true;
            foreach (EAScreen eaScr in spanEaScreens)
            {
                int deltaX = Math.Abs(eaScr.Bounds.Left - avgLeft);
                if (deltaX > criteria_xgap)
                    isVertSpan = false;
                int deltaY = Math.Abs(eaScr.Bounds.Top - avgTop);
                if (deltaY > criteria_ygap)
                    isHorzSpan = false;
            }

            if (isHorzSpan)
            { 
                _isHorzSpan = true;

                spanEaScreens.Sort((x, y) => (x.Bounds.Left - y.Bounds.Left));

                _bounds.X = spanEaScreens[0].Bounds.Left;
                _bounds.Width = spanEaScreens[spanEaScreens.Count - 1].Bounds.Right - _bounds.X;

                _workingArea.X = spanEaScreens[0].WorkingArea.Left;
                _workingArea.Width = spanEaScreens[spanEaScreens.Count - 1].WorkingArea.Right - _workingArea.X;

                _bounds.Y = spanEaScreens[0].Bounds.Top;
                _workingArea.Y = spanEaScreens[0].WorkingArea.Top;

                int bottomBounds = spanEaScreens[0].Bounds.Bottom;
                int bottomWorkingArea = spanEaScreens[0].WorkingArea.Bottom;

                foreach (EAScreen eaScr in spanEaScreens.Skip(1))
                {
                    _bounds.Y = Math.Max(_bounds.Y, eaScr.Bounds.Top);
                    _workingArea.Y = Math.Max(_workingArea.Y, eaScr.WorkingArea.Top);

                    bottomBounds = Math.Min(bottomBounds, eaScr.Bounds.Bottom);
                    bottomWorkingArea = Math.Min(bottomWorkingArea, eaScr.WorkingArea.Bottom);
                }
                _bounds.Height = bottomBounds - _bounds.Y;
                _workingArea.Height = bottomWorkingArea - _workingArea.Y;

            }
            else if (isVertSpan)
            {
                _isHorzSpan = false;

                spanEaScreens.Sort((x, y) => (x.Bounds.Top - y.Bounds.Top));

                _bounds.Y = spanEaScreens[0].Bounds.Top;
                _bounds.Height = spanEaScreens[spanEaScreens.Count - 1].Bounds.Bottom - _bounds.Y;

                _workingArea.Y = spanEaScreens[0].WorkingArea.Top;
                _workingArea.Height = spanEaScreens[spanEaScreens.Count - 1].WorkingArea.Bottom - _workingArea.Y;

                _bounds.X = spanEaScreens[0].Bounds.Left;
                _workingArea.X = spanEaScreens[0].WorkingArea.Left;

                int rightBounds = spanEaScreens[0].Bounds.Right;
                int rightWorkingArea = spanEaScreens[0].WorkingArea.Right;

                foreach (EAScreen eaScr in spanEaScreens.Skip(1))
                {
                    _bounds.X = Math.Max(_bounds.X, eaScr.Bounds.Left);
                    _workingArea.X = Math.Max(_workingArea.X, eaScr.WorkingArea.Left);

                    rightBounds = Math.Min(rightBounds, eaScr.Bounds.Right);
                    rightWorkingArea = Math.Min(rightWorkingArea, eaScr.WorkingArea.Right);
                }
                _bounds.Width = rightBounds - _bounds.X;
                _workingArea.Width = rightWorkingArea - _workingArea.X;
            }
            else
            {
                //
                string m = "Should not go to here";
            }


            return true;
        }

        #endregion

        #region Log
        private void LogEAScreen(EAScreen eaScreen, ILog? log)
        {
            if (log != null)
            {
                log.Info($"EAScreen: {eaScreen.ScreenDeviceName}, "); 
            }
        }

        #endregion

        #region EAScreen
        public List<EAScreen> eaScreens => _eaScreens;

        public bool IsExistScreen(string deviceName)
        {
            if (_eaScreens == null)
                return false;
            if (_eaScreens.Count == 0) return false;
            return (_eaScreens.Find(x => x.ScreenDeviceName == deviceName) != null);
        }
        #endregion

        #region Monitor
        public MonitorInfo? GetPrimaryMonitor()
        {
            foreach(EAScreen eaScreen in _eaScreens)
            {
                if (eaScreen.HasAttachedMonitor)
                {
                    return eaScreen.AttachedMonitors[0];
                }
            }
            return null;
        }
        #endregion
    }
}
