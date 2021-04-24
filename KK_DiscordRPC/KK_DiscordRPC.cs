using System;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using KKAPI;
using KKAPI.MainGame;
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
        private ChaFile old_Character;
        private HSceneProc hproc;

        private ConfigEntry<bool> configDisplayActivity;
        private ConfigEntry<string> configCustomActivityMessage;
        private ConfigEntry<bool> configDisplayTime;
        private ConfigEntry<bool> configResetTime;

        private ConfigEntry<string> configActivityMessageMaker;
        private ConfigEntry<string> configStateMessageMaker;
        private ConfigEntry<string> configBigImageMaker;

        private ConfigEntry<string> configBigImageStudio;

        private ConfigEntry<string> configBigImageMainGame;

        private void Awake()
        {
            Logger = base.Logger;

            configDisplayActivity = Config.Bind("_General_", "displayActivity", true, "Whether or not to display your activity (Ingame, Studio or Maker).");
            configCustomActivityMessage = Config.Bind("_General_", "customActivityMessage", "Ingame", "Message to be displayed when displayActivity is turned off.");
            configDisplayTime = Config.Bind("_General_", "displayTime", true, "Whether or not to display the elapsed time.");
            configResetTime = Config.Bind("_General_", "resetTime", true, "Whether or not to reset the elapsed time when chaning activity and/or loading diffrent character in maker.");

            configActivityMessageMaker = Config.Bind("CharacterMaker", "maker_activityMessageMaker", "Maker (<maker_sex>)", "Activity message to display when in maker. Keywords: <maker_sex>");
            configStateMessageMaker = Config.Bind("CharacterMaker", "maker_stateMessage", "Editing: <chara_name>", "State message to display when editing a character in maker. Keywords: <chara_name>, <chara_nickname>");
            configBigImageMaker = Config.Bind("CharacterMaker", "maker_bigImage", "sliders", "Displayed image when in Maker. Possiblities: logo_main, logo_main_alt, logo_studio, sliders");

            configBigImageStudio = Config.Bind("CharaStudio", "studio_bigImage", "logo_studio", "Displayed image when in CharaStudio. Possiblities: logo_main, logo_main_alt, logo_studio, sliders");

            configBigImageMainGame = Config.Bind("Main Game", "maingame_bigImage", "logo_main", "Displayed image when in MainGame (everything but Maker and Studio). Possiblities: logo_main, logo_main_alt, logo_studio, sliders");

            var handlers = new DiscordRPC.EventHandlers();
            DiscordRPC.Initialize(
                "835112124295806987",
                ref handlers,
                false,
                "643270");

            startStamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            old_Character = null;
            currentStamp = startStamp;
            CheckInterval = 3;
            CheckStatus();
            Logger.LogInfo("Discord Rich Presence started");
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
            if (configDisplayActivity.Value is true)
            {
                switch (KoikatuAPI.GetCurrentGameMode())
                {
                    case GameMode.Unknown:
                        SetStatus("Unkown Gamemode", "Koikatsu", startStamp, "main", "Unknown");
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
                        SetStatus("Playing something", "Koikatsu", startStamp, "logo_main", "Main game");
                        break;
                }
            }
            else SetStatus(null, configCustomActivityMessage.Value, startStamp, "logo_main", "Koikatsu");
        }
        void MakerStatus()
        {
            Boolean loaded = KKAPI.Maker.MakerAPI.InsideAndLoaded;
            if (loaded is true)
            {
                ChaFile character = KKAPI.Maker.MakerAPI.LastLoadedChaFile;
                string activity = configActivityMessageMaker.Value;
                string sex_string = "unknown";
                Int32 sex = KKAPI.Maker.MakerAPI.GetMakerSex();
                switch (sex) 
                {
                    case 0:
                        sex_string = "male";
                        break;
                    case 1:
                        sex_string = "female";
                        break;
                }
                activity = activity.Replace("<maker_sex>", sex_string);
                if (KoikatuAPI.GetCurrentGameMode() != old_Gamemode)
                {
                    currentStamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                }
                if (old_Character is null)
                {
                    currentStamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                    old_Character = character;
                }
                if (character.charaFileName != old_Character.charaFileName)
                {
                    currentStamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                }
                string state = configStateMessageMaker.Value;
                state = state.Replace("<chara_name>", character.parameter.fullname);
                state = state.Replace("<chara_nickname>", character.parameter.nickname);
                SetStatus(activity, state, currentStamp, configBigImageMaker.Value, "CharacterMaker");

                old_Character = character;
            }
            else
            {
                SetStatus("In Charactermaker", "Koikatsu", startStamp, configBigImageMaker.Value, "CharacterMaker");
            }
        }
        void StudioStatus()
        {
            SetStatus("In Studio", "CharaStudio", startStamp, configBigImageStudio.Value, "CharaStudio");
        }
        void MainGameStatus()
        {
            string activity = "Unknown";
            string state = "Unknown";
            if (KoikatuAPI.GetCurrentGameMode() != old_Gamemode)
            {
                currentStamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            }
            if (KKAPI.MainGame.GameAPI.InsideHScene is true)
            {
                activity = "In H-Scene";
                state = "placeholder";
            }
            else
            {
                activity = "Ingame";
                state = "In menu or roaming";
            }    
            SetStatus(activity, state, currentStamp, configBigImageMainGame.Value, "Main Game");
        }

        void SetStatus(string activity, string state, long timestamp, string img, string img_text)
        {
            if (configResetTime.Value is false) timestamp = startStamp;
            if (configDisplayTime.Value is false) timestamp = 0;
            
            prsnc.details = activity;
            prsnc.state = state;
            prsnc.startTimestamp = timestamp;
            prsnc.largeImageKey = img;
            prsnc.largeImageText = img_text;
            prsnc.smallImageKey = null;
            prsnc.smallImageText = null;
            prsnc.partySize = 0;
            prsnc.partyMax = 0;
            DiscordRPC.UpdatePresence(ref prsnc);
        }
        private void OnStartH(HSceneProc proc, bool freeH)
        {
            Logger.LogInfo("H-Scence started");
            hproc = proc;
        }
    }
}