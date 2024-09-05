#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
// Program.cs created on 10/4/2022T3:37 PM
//

#endregion

using System;
using System.Threading.Tasks;

namespace CLI.Subagent
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            /*
             * Only allow this application in debug mode
             */
            //#if DEBUG
            CLIAgent agent = new CLIAgent(Guid.NewGuid(), Guid.NewGuid());
            await agent.StartAsync(args).ConfigureAwait(false);

            Environment.Exit(agent.GetExitCode());
            //#else
            //            Console.WriteLine("\nERROR: Only the DEBUG build is supported");
            //            Console.WriteLine("Hit any key to exit");
            //            Console.ReadLine();
            //#endif
        }
    }
}