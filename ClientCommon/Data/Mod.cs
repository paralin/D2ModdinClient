namespace ClientCommon.Data
{
    public class ClientMod
    {
        public string name { get; set; }
        public string version { get; set; }

        public override bool Equals(object obj)
        {
            var mod = obj as ClientMod;
            return mod.name == name;
        }
    }
}
