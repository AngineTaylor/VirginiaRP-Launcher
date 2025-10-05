using System;

namespace Launcher.ServiceLib.Data
{
    using System;
    using System.Runtime.Serialization;
    public class CharacterData
    {
        public int Id { get; set; }
        public int ShortId { get; set; }
        public string Nickname { get; set; }
        public int Age { get; set; }
        public string Story { get; set; }
        public long SteamId64 { get; set; }
        public DateTime CreatedAt { get; set; }
        public string RegIp { get; set; }
    }
}