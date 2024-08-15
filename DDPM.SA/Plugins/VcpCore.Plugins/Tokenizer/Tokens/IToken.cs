namespace VcpCore.Plugins
{
    public interface IToken
    {
        string Type { get; set; }

        string Value { get; set; }
    }
}