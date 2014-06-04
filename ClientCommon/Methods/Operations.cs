namespace ClientCommon.Methods
{
    public class Shutdown
    {
        public const string Msg = "close";
        public string msg = Msg;
    }

    public class Uninstall
    {
        public const string Msg = "uninstall";
        public string msg = Msg;
    }

    public class LaunchDota
    {
        public const string Msg = "launchdota";
        public string msg = Msg;
    }

    public class ConnectDota
    {
        public const string Msg = "connectdota";
        public string msg = Msg;
        public string ip { get; set; }
    }

    public class ConnectDotaSpectate
    {
        public const string Msg = "connectspectate";
        public string msg = Msg;
        public string ip { get; set; }
    }
}
