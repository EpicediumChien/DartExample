using System.Collections.Generic;

namespace VcpCore.Plugins
{
    public class FWVersionTable
    {
        public static List<string> Case2_HEXHEX = new List<string>(){
        "E1920H", "E2220H", "E2221HN", "E2222H", "E2222HS", "E2420H", "E2420HS", "E2421HN", "E2422H", "E2422HN",
        "E2422HS", "E2223HN", "E2223HV", "E2724HN", "P2421", "P2719H", "P2719HC", "P3221D", "S2319HS", "S2419HM",
        "S2719DC", "S2719DM", "S2719HS", "SE2222H", "SE2222HV", "SE2422H", "SE2422HM", "SE2422HR", "SE2422HX", "SE2219H",
        "SE2219HX", "SE2419H", "SE2419HX", "SE2419HR", "SE2719H", "SE2719HR", "U4320Q", "C3422WE"
        };

        public static List<string> Case4_BCDHEX = new List<string>(){
        "AW2723DF", "AW3423DWF", "AW2521HF", "AW2521HFA", "AW2521HFL", "AW2521HFLA", "AW2720HF", "AW2720HFA", "AW5520QF", "C2423H",
        "C2723H", "G2422HS", "S2522HG", "S2421HGF", "E2020H", "E2720H", "E2720HS", "P2219H", "P2219HC", "P2419HC",
        "P3421W", "S2722DC", "S2722QC", "S2721D", "S2721DS", "S2721Q", "S2721QS", "S2721QSA", "P2423", "U2422H",
        "U2422HE", "U2422HX", "U3421WE", "U3821DW", "UP3221Q", "U2520D", "U2520DR", "U2720Q ", "U2720QM", "UP2720Q",
        "UP2720QA", "U3219Q", "U3419W", "U4919DW", "U4919DWA", "S2721HGF", "S2721HGFA", "U2421HE", "U2721DE", "U2419HC",
        "U2419HS", "U2419HX", "U2719D", "U2719DC", "U2719DS", "U2719DX"
        };

        public static List<string> HideFWModel = new List<string>() { "P2319H", "P2419H", "E1715S", "P2219H", "S2319H", "P2720DC" };
    }
}