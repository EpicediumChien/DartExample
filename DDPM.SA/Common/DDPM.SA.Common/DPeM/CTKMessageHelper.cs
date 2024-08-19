using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IndiLogic.DPeM.Broker;

namespace DDPM.SA.Common {
  public class CTKMessageHelper {
    public string CollaborationMsg;
    public bool IsCollabMultipleCallsDetected;
    public bool IsZoomCallbacksRegistered;
    public bool IsZoomClientInstalled;
    public bool IsZoomMultipleCallsDetected;
    public bool IsZoomVersionSupported;
    public string TeamsSDKState;
  }
}
