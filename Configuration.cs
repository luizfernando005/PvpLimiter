using Rocket.API;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace PvpLimiter
{
    public class Configuration : IRocketPluginConfiguration
    {
        public int TempoDeEspera { get; set; }
        public void LoadDefaults()
        {
            TempoDeEspera = 30;
        }
    }
}