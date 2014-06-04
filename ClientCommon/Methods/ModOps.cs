using ClientCommon.Data;

namespace ClientCommon.Methods
{
    public class OnInstalledMod
    {
        public const string Msg = "oninstalled";
        public string msg = Msg;
        public ClientMod Mod { get; set; }
    }

    public class InstallMod
    {
        public const string Msg = "installmod";
        public string msg = Msg;
        public ClientMod Mod { get; set; }
        public string url { get; set; }
    }

    public class DeleteMod
    {
        public const string Msg = "delmod";
        public string msg = Msg;
        public ClientMod Mod { get; set; }
    }

    /// <summary>
    /// Sets the active mod (when entering a lobby)
    /// </summary>
    public class SetMod
    {
        public const string Msg = "setmod";
        public string msg = Msg;
        public ClientMod Mod { get; set; }
    }
}
