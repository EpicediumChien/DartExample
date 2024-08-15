namespace DDPM.SA.Common.Display
{
    //Used as the argument of VideoSwap(x, y)
    //For example, to swap main and sub2, you can call VideoSwap((UInt16)main, (UInt16)sub2)
    public enum ePxpInputs
    {
        invalid = -1,
        main = 0, sub1 = 1, sub2 = 2, sub3 = 3
    };
}