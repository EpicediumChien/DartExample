using Dell.Client.Framework.Common;
using System.Diagnostics.CodeAnalysis;

namespace Dell.UnifyingAgent.Tests.Helpers
{
    [ExcludeFromCodeCoverage]
    public class UnitTestLogger : Log
    {
        public static bool ExceptionsRaised = false;
        private static object syncObject = new object();

        public UnitTestLogger(string subSystem = "UnitTest", LogFile logFile = null, string regKeyPath = null) : base(subSystem, logFile, regKeyPath)
        {
            this.LogEvent += UnitTestLogger_LogEvent;
        }

        private void UnitTestLogger_LogEvent(object sender, LogEventArgs e)
        {
            lock (syncObject)
            {
                TestContext.WriteLine($"{e.MsgDateTime}:- {e.Subsystem} - {e.MsgType} - {e.Msg}");
            }
            if (e.MsgType == LogMsgType.Error)
                ExceptionsRaised = true;
            System.Diagnostics.Debug.WriteLine($"{e.MsgDateTime}:- {e.Subsystem} {TestContext.CurrentContext.Test.FullName} - {e.MsgType} - {e.Msg}");
        }
    }
}