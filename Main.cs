using Game4Freak.AdvancedZones;
using Newtonsoft.Json;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;
using PvpLimiter.Helpers;

namespace PvpLimiter
{
    public class Main : RocketPlugin<Configuration>
    {
        public static Main Instance;
        public bool taDeDia;
        public Dictionary<CSteamID, int> CheckingPlayer { get; private set; } // codigo - instanceId
        protected override void Load()
        {
            Instance = this;
            taDeDia = true;
            CheckingPlayer = new Dictionary<CSteamID, int>();

            DamageTool.damagePlayerRequested += onPlayerDamage;
            BarricadeManager.onDamageBarricadeRequested += onBarricadeDamage;
            StructureManager.onDamageStructureRequested += onStructureDamage;

            LightingManager.onDayNightUpdated_ModHook += onupdate;

            if (Level.isLoaded)
                OnLevelLoaded(0);
            else
                Level.onLevelLoaded += OnLevelLoaded;

            Logger.Log("----------------------");
            Logger.Log("PvpLimiter Loaded");
            Logger.Log("----------------------");
        }

        protected override void Unload()
        {
            DamageTool.damagePlayerRequested -= onPlayerDamage;
            BarricadeManager.onDamageBarricadeRequested -= onBarricadeDamage;
            StructureManager.onDamageStructureRequested -= onStructureDamage;
            LightingManager.onDayNightUpdated_ModHook -= onupdate;
            Level.onLevelLoaded += OnLevelLoaded;
            Logger.Log("PvpLimiter unloaded");
        }

        private void OnLevelLoaded(int level)
        {
            taDeDia = LightingManager.isDaytime;
            Logger.Log("Level Loaded | " + LightingManager.isDaytime);
        }


        private void onPlayerDamage(ref DamagePlayerParameters parameters, ref bool shouldAllow)
        {
            UnturnedPlayer uPlayer = UnturnedPlayer.FromPlayer(parameters.player);
            UnturnedPlayer atacante = UnturnedPlayer.FromCSteamID(parameters.killer);

            if (parameters.cause == EDeathCause.ZOMBIE || parameters.cause == EDeathCause.ANIMAL)
                return;

            if (taDeDia)
            {
                if (uPlayer != null)
                {
                    if (AdvancedZones.Inst.playerInZoneType(uPlayer, "valora"))
                    {
                        shouldAllow = false;
                    }
                }
            }
        }

        private void onBarricadeDamage(CSteamID instigatorSteamID, Transform barricadeTransform, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            var dBarricade = BarricadeManager.FindBarricadeByRootTransform(barricadeTransform);

            if (dBarricade.interactable is InteractableFarm || dBarricade.interactable is InteractableForage)
            {
                return;
            }

            if (taDeDia)
            {
                if (AdvancedZones.Inst.transformInZoneType(barricadeTransform, "valora"))
                {

                    shouldAllow = false;


                    UnturnedPlayer player = UnturnedPlayer.FromCSteamID(instigatorSteamID);
                    if (player != null)
                    {
                        UnturnedChat.Say(player, "Você não consegue destruir coisas em Valora durante o dia.");
                    }
                    else if ((damageOrigin.ToString() == "Bullet_Explosion"
                        || (damageOrigin.ToString() == "Charge_Explosion")
                        || damageOrigin.ToString() == "Food_Explosion"
                        || damageOrigin.ToString() == "Rocket_Explosion"
                        || damageOrigin.ToString() == "Sentry"
                        || damageOrigin.ToString() == "Trap_Explosion"
                        || damageOrigin.ToString() == "Vehicle_Explosion"
                        || damageOrigin.ToString() == "Zombie_Swipe") &&
                        (barricadeTransform.name.ToString() != "1102"
                        && barricadeTransform.name.ToString() != "1101"
                        && barricadeTransform.name.ToString() != "1393"
                        && barricadeTransform.name.ToString() != "1241"))
                    {
                        shouldAllow = false;
                    }
                }
            }
        }

        private void onStructureDamage(CSteamID instigatorSteamID, Transform structureTransform, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            if (taDeDia)
            {
                if (AdvancedZones.Inst.transformInZoneType(structureTransform, "valora"))
                {

                    shouldAllow = false;


                    UnturnedPlayer player = UnturnedPlayer.FromCSteamID(instigatorSteamID);
                    if (player != null)
                    {
                        UnturnedChat.Say(player, "Você não consegue destruir coisas em Valora durante o dia.");
                    }
                    else if ((damageOrigin.ToString() == "Bullet_Explosion"
                        || (damageOrigin.ToString() == "Charge_Explosion")
                        || damageOrigin.ToString() == "Food_Explosion"
                        || damageOrigin.ToString() == "Rocket_Explosion"
                        || damageOrigin.ToString() == "Sentry"
                        || damageOrigin.ToString() == "Trap_Explosion"
                        || damageOrigin.ToString() == "Vehicle_Explosion"
                        || damageOrigin.ToString() == "Zombie_Swipe") &&
                        (structureTransform.name.ToString() != "1102"
                        && structureTransform.name.ToString() != "1101"
                        && structureTransform.name.ToString() != "1393"
                        && structureTransform.name.ToString() != "1241"))
                    {
                        shouldAllow = false;
                    }
                }
            }
        }

        private void onupdate(bool isDaytime)
        {
            if (!isDaytime)
            {
                coroutine = StartNight(Configuration.Instance.TempoDeEspera);
                StartCoroutine(coroutine);
            }
            else {
                coroutine = StartDay(Configuration.Instance.TempoDeEspera);
                StartCoroutine(coroutine);
            }
        }

        IEnumerator coroutine;

        private IEnumerator Timer(int seconds, UnturnedPlayer player)
        {
            yield return new WaitForSeconds(seconds);
            EffectManager.askEffectClearByID(58532, player.Player.channel.owner.transportConnection);
        }

        private IEnumerator StartNight(int seconds)
        {
            MessageHelper.Send("NoitePrestesComecar", Configuration.Instance.TempoDeEspera);
            yield return new WaitForSeconds(seconds);

            foreach (var steamPlayer in Provider.clients)
            {
                taDeDia = LightingManager.isDaytime;
                UnturnedPlayer up = UnturnedPlayer.FromSteamPlayer(steamPlayer);
                //ChatManager.serverSendMessage("[VALORA] Atenção, a noite chegou trazendo consigo todos seus perigos.", Color.white, null, steamPlayer, EChatMode.GLOBAL, null, true);
                MessageHelper.Send(up, "NoiteChegou");
                EffectManager.sendUIEffect(58532, 583, up.Player.channel.owner.transportConnection, true);
                coroutine = Timer(10, up);
                StartCoroutine(coroutine);
            }
        }

        private IEnumerator StartDay(int seconds)
        {
            MessageHelper.Send("DiaPrestesComecar", Configuration.Instance.TempoDeEspera);
            yield return new WaitForSeconds(seconds);

            foreach (var steamPlayer in Provider.clients)
            {
                taDeDia = LightingManager.isDaytime;
                UnturnedPlayer up = UnturnedPlayer.FromSteamPlayer(steamPlayer);
                MessageHelper.Send(up, "DiaChegou");
                EffectManager.sendUIEffect(58532, 583, up.Player.channel.owner.transportConnection, true);
                coroutine = Timer(10, up);
                StartCoroutine(coroutine);
            }
        }

        public override TranslationList DefaultTranslations => new TranslationList()
        {
            { "NoiteChegou", "{color=yellow}[VALORA]{/color} {color=red}Atenção!{/color} A noite chegou trazendo consigo todos os seus {color=red}perigos{/color}." },
            { "NoitePrestesComecar", "{color=yellow}[VALORA]{/color} {color=red}Cuidado!{/color} Faltam apenas {color=red}{0} segundos{/color} para que a noite chegue." },
            { "DiaChegou", "{color=yellow}[VALORA]{/color} {color=red}Atenção!{/color} Amanheceu e Valora está {color=#35f060}segura{/color} novamente." },
            { "DiaPrestesComecar", "{color=yellow}[VALORA]{/color} {color=red}Cuidado! Faltam apenas {color=red}{0} segundos{/color} para o sol raiar." }
        };
    }
}
