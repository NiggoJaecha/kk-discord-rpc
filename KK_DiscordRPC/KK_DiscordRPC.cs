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
            CheckInterval = 5;
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
                    SetStatus("Koikatsu", "Unkown Gamemode", "main", "Unknown");
                    break;
                case GameMode.Maker:
                    MakerStatus();
                    break;
                case GameMode.Studio:
                    StudioStatus();
                    break;
                case GameMode.MainGame:
                    MainGameStatus();
                    break;
                default:
                    SetStatus("Koikatsu", "Playing something", "studio", "Main game");
                    break;
            }
        }
        void MakerStatus()
        {
            // SetStatus("Koikatsu", "In Charactermaker", "main", "CharacterMaker");
            Boolean loaded = KKAPI.Maker.MakerAPI.InsideAndLoaded;
            if (loaded is true)
            {
                ChaControl character = KKAPI.Maker.MakerAPI.GetCharacterControl();
                string name = character.chaFile.parameter.fullname;
                byte sex = character.sex;
                Logger.LogInfo(name);
                string status;
                switch (sex) 
                {
                    case 0:
                        status = "Maker (male)";
                        break;
                    case 1:
                        status = "Maker (female)";
                        break;
                    default:
                        status = "Charactermaker";
                        break;
                }
                SetStatus(name, status, "studio", "CharacterMaker");
            }
            else
            {
                SetStatus("Koikatsu", "In Charactermaker", "studio", "CharacterMaker");
            }
        }
        void StudioStatus()
        {
            SetStatus("CharaStudio", "In Studio", "main", "CharaStudio");
        }
        void MainGameStatus()
        {
            SetStatus("Koikatsu", "In Game", "studio", "Main Game");
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