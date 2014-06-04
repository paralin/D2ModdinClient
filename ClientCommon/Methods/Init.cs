using ClientCommon.Data;

namespace ClientCommon.Methods
{
    public class Init
    {
        public const string Msg = "init";
        public string msg = "init";
        public string[] SteamIDs { get; set; }
        public string Version { get; set; }
        public ClientMod[] Mods { get; set; }
    }
}
