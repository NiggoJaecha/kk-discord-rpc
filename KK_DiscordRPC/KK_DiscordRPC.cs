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
        public long startStamp;
        public long currentStamp;
        private GameMode old_Gamemode;
        private ChaControl old_Character;

        private void Awake()
        {
            Logger = base.Logger;

            var handlers = new DiscordRPC.EventHandlers();
            DiscordRPC.Initialize(
                "835112124295806987",
                ref handlers,
                false,
                "643270");

            startStamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            currentStamp = startStamp;
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
                    SetStatus("Koikatsu", "Unkown Gamemode", startStamp, "main", "Unknown");
                    old_Gamemode = KoikatuAPI.GetCurrentGameMode();
                    break;
                case GameMode.Maker:
                    MakerStatus();
                    old_Gamemode = KoikatuAPI.GetCurrentGameMode();
                    break;
                case GameMode.Studio:
                    StudioStatus();
                    old_Gamemode = KoikatuAPI.GetCurrentGameMode();
                    break;
                case GameMode.MainGame:
                    MainGameStatus();
                    old_Gamemode = KoikatuAPI.GetCurrentGameMode();
                    break;
                default:
                    SetStatus("Koikatsu", "Playing something", startStamp, "studio", "Main game");
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
                if (KoikatuAPI.GetCurrentGameMode() != old_Gamemode)
                {
                    currentStamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                }
                if (character == old_Character)
                {
                    currentStamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                }
                SetStatus("Editing: " + name, status, currentStamp, "studio", "CharacterMaker");
                old_Character = character;
            }
            else
            {
                SetStatus("Koikatsu", "In Charactermaker", startStamp, "studio", "CharacterMaker");
            }
        }
        void StudioStatus()
        {
            SetStatus("CharaStudio", "In Studio", startStamp, "main", "CharaStudio");
        }
        void MainGameStatus()
        {
            if (KoikatuAPI.GetCurrentGameMode() != old_Gamemode)
            {
                currentStamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            }
            SetStatus("Koikatsu", "In Game", currentStamp, "studio", "Main Game");
        }

        void SetStatus(string mode, string status, long timestamp, string img, string img_text)
        {
            prsnc.state = mode;
            prsnc.details = status;
            prsnc.startTimestamp = timestamp;
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