using Game4Freak.AdvancedZones;
using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PvpLimiter
{
    internal class SetValoraCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "setvalora";

        public string Help => "setvalora";

        public string Syntax => string.Empty;

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "setvalora" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;

            if (command.Length < 0)
            {
                UnturnedChat.Say(player, "Você deve especificar uma zona");
                return;
            }

            if (command.Length > 0)
            {
                Zone currentZoneFixed = AdvancedZones.Inst.getZoneByName(command[0]);
                if (currentZoneFixed != null)
                {
                    currentZoneFixed.addFlag("valora");

                    UnturnedChat.Say(player, $"Tag Valora adicionada para a zona {command[0]}.");
                    AdvancedZones.Inst.Configuration.Save();
                }
                else
                {
                    UnturnedChat.Say(player, $"A zona {command[0]} não foi encontrada.");
                }
            }
        }
    }
}