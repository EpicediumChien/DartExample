namespace VcpCore.Plugins
{
    public class Token : IToken
    {
        public string Type { get; set; }

        public string Value { get; set; }

        public override string ToString() => $"{Type}: {Value}";
    }
}