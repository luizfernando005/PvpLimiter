using PvpLimiter;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PvpLimiter.Helpers
{
    public class MessageHelper
    {
        private static Main pluginInstance => Main.Instance;

        public static void Send(IRocketPlayer player, string translationKey, params object[] placeholder)
        {
            UnturnedChat.Say(player, ReplacePlaceholders(pluginInstance.Translate(translationKey), placeholder), Color.white, true);
        }

        public static void Send(UnturnedPlayer player, string translationKey, params object[] placeholder)
        {
            UnturnedChat.Say(player, ReplacePlaceholders(pluginInstance.Translate(translationKey), placeholder), Color.white, true); //.Replace("}", ">")
        }

        public static void Send(string translationKey, params object[] placeholder)
        {
            UnturnedChat.Say(ReplacePlaceholders(pluginInstance.Translate(translationKey), placeholder), Color.white, true);
        }

        public static string ReplacePlaceholders(string message, params object[] placeholder)
        {
            string finalMessage = message;

            for (int i = 1; i <= placeholder.Length; i++)
            {
                finalMessage = finalMessage.Replace("{" + (i - 1) + "}", placeholder[i - 1].ToString());
            }

            finalMessage = finalMessage.Replace("{", "<").Replace("}", ">");

            return finalMessage;
        }

    }
}