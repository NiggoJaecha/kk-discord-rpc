using System;
using BepInEx;
using BepInEx.Logging;
using KKAPI;
using UnityEngine;

namespace KK_DiscordRPC
{
    [BepInPlugin(GUID, "KK_DiscordRPC", Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    public class Koi_DiscordRPC : BaseUnityPlugin
    {
        public const string GUID = "varkaria.njaecha.DiscordRPC";
        public const string Version = "1.0";

        internal static DiscordRPC.RichPresence prsnc;
        internal new static ManualLogSource Logger;

        private float CheckInterval;
        private float CoolDown;
        public long startstamp;

        private void Awake()
        {
            Logger = base.Logger;

            var handlers = new DiscordRPC.EventHandlers();
            DiscordRPC.Initialize(
                "835112124295806987",
                ref handlers,
                false,
                "643270");

            startstamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            CheckStatus();
            Logger.LogInfo("Discord Rich Presense Started");
        }
        private void Update()
        {
            if (CoolDown > 0)
                CoolDown -= Time.deltaTime;
            else
            {
                CoolDown = CheckInterval;
                CheckStatus();
            }
        }
        void CheckStatus()
        {
            switch (KoikatuAPI.GetCurrentGameMode())
            {
                case GameMode.Unknown:
                    SetStatus("Koikatsu", "In Main Menu", "main", "Main Menu");
                    break;
                case GameMode.Maker:
                    SetStatus("Koikatsu", "In Charactermaker", "main", "CharacterMaker");
                    break;
                case GameMode.Studio:
                    SetStatus("CharaStudio", "In Studio", "studio", "CharaStudio");
                    break;
                case GameMode.MainGame:
                    SetStatus("Koikatsu", "In Game", "studio", "Main Game");
                    break;
                default:
                    SetStatus("Koikatsu", "Playing something", "studio", "Main game");
                    break;
            }
        }
        void SetStatus(string mode, string status, string img, string img_text)
        {
            prsnc.state = mode;
            prsnc.details = status;
            prsnc.startTimestamp = startstamp;
            prsnc.largeImageKey = img;
            prsnc.largeImageText = img_text;
            prsnc.smallImageKey = null;
            prsnc.smallImageText = null;
            prsnc.partySize = 0;
            prsnc.partyMax = 0;
            DiscordRPC.UpdatePresence(ref prsnc);
        }
    }
}