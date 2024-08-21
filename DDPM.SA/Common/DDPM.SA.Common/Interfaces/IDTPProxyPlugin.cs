#region LicenceHeader

//
// Copyright © 2024, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
// IDTPProxyPlugin.cs created on 8/13/2024T3:37 PM
//

#endregion

using System.Threading.Tasks;
using Dell.Client.Framework.Common;

namespace DDPM.SA.Common {
  public interface IDTPProxyPlugin : IFrameworkPlugin {

    Task<int> GetDpiValue(string itemID);

  }

}