using System;

namespace VcpCore.Common
{
    public class DebouncerArg
    {
        public EventArgs eventArgs { get; set; } = new EventArgs();
        public object sender { get; set; } = default(object);
    }
}