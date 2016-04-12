namespace Wox.Plugin.Volume
{
    class Config
    {
        public const string STYLE_BAR = "bar";
        public const string STYLE_PERCENT = "percent";

        public int step { get; set; }
        public string up { get; set; }
        public string down { get; set; }
        public string mute { get; set; }
        public string style { get; set; }
        public bool applyOnDelete { get; set; }
    }
}
