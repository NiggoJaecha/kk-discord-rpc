using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using KKAPI;
using KKAPI.MainGame;
using KKAPI.Maker;
using KKAPI.Studio;
using UnityEngine;

namespace KK_DiscordRPC
{
    [BepInPlugin(Guid, "KK_DiscordRPC", Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    [BepInDependency(Sideloader.Sideloader.GUID, Sideloader.Sideloader.Version)]
    public class KoiDiscordRpc : BaseUnityPlugin
    {
        private const string Guid = "com.varkaria.njaecha.KK_DiscordRPC";
        public const string Version = "1.1.0";

        private static DiscordRPC.RichPresence prsnc;
        private static ManualLogSource logger;
        internal static HSceneProc hProc;

        private float _checkInterval;
        private float _coolDown;
        private long _startStamp;
        internal static long currentStamp;
        private GameMode _oldGamemode;
        private ChaFile _oldCharacter;
        private int _loadedPluginsCount;
        private int _loadedModsCount;
        private bool _checkedCounts;

        private ConfigEntry<bool> _configDisplayActivity;
        private ConfigEntry<string> _configCustomActivityMessage;
        private ConfigEntry<string> _configCustomStateMessage;
        private ConfigEntry<string> _configCustomBigImage;
        private ConfigEntry<string> _configCustomLittleImage;
        private ConfigEntry<string> _configCustomLittleImageText;
        private ConfigEntry<bool> _configDisplayTime;
        private ConfigEntry<bool> _configResetTime;
        private ConfigEntry<bool> _configDisplayLittleImage;

        private ConfigEntry<string> _configBigImageMaker;
        private ConfigEntry<string> _configBigImageMakerText;
        private ConfigEntry<string> _configActivityMessageMaker;
        private ConfigEntry<string> _configStateMessageMaker;

        private ConfigEntry<string> _configBigImageStudio;
        private ConfigEntry<string> _configBigImageStudioText;
        private ConfigEntry<string> _configActivityMessageStudio;
        private ConfigEntry<string> _configStateMessageStudio;

        private ConfigEntry<string> _configBigImageMainGame;
        private ConfigEntry<string> _configBigImageMainGameText;
        private ConfigEntry<string> _configActivityMessageMainGame;
        private ConfigEntry<string> _configStateMessageMainGame;

        private ConfigEntry<string> _configBigImageUnknown;
        private ConfigEntry<string> _configBigImageUnknownText;
        private ConfigEntry<string> _configActivityMessageUnknown;
        private ConfigEntry<string> _configStateMessageUnknown;

        private ConfigEntry<bool> _configDisplayHScene;
        private ConfigEntry<string> _configBigImageHScene;
        private ConfigEntry<string> _configActivityMessageHScene;
        private ConfigEntry<string> _configStateMessageHScene;
        private ConfigEntry<string> _configStateMessageHScene3P;
        private ConfigEntry<string> _hBoolFreeHTrue;
        private ConfigEntry<string> _hBoolFreeHFalse;
        private ConfigEntry<string> _hBoolDarknessTrue;
        private ConfigEntry<string> _hBoolDarknessFalse;
        // ReSharper disable IdentifierTypo
        private ConfigEntry<string> _hModeAibzu;
        private ConfigEntry<string> _hModeHoushi;
        private ConfigEntry<string> _hModeSonyu;
        private ConfigEntry<string> _hModeMasturbation;
        private ConfigEntry<string> _hModePeeping;
        private ConfigEntry<string> _hModeLesbian;
        private ConfigEntry<string> _hModeHoushi3P;
        private ConfigEntry<string> _hModeSonyu3P;
        
        private ConfigEntry<string> _hModeHoushi3Pmmf;
        
        private ConfigEntry<string> _hModeSonyu3Pmmf;

        private ConfigEntry<string> _hTypeNormal;
        private ConfigEntry<string> _hTypeWatching;
        private ConfigEntry<string> _hTypeThreesome;

        private ConfigEntry<string> _hSafetySafe;
        private ConfigEntry<string> _hSafetyRisky;
        // ReSharper restore IdentifierTypo
        private void Awake()
        {
            logger = base.Logger;
            GameAPI.RegisterExtraBehaviour<SceneGameController>(Guid);

            AcceptableValueBase pictures = new AcceptableValueList<string>("logo_main", "logo_main_alt", "logo_studio", "sliders", "tech");

            _configDisplayActivity = Config.Bind("_General_", "Display Activity", true, "Whether or not to display your activity (Ingame, Studio or Maker).");
            _configCustomBigImage = Config.Bind("_General_", "Image", "logo_main", new ConfigDescription("Displayed image when display Activity is turned off.", pictures));
            Config.Bind("_General_", "Image Text", "Koikatsu", "Message to display when hovering the Image while display Activity is turned off");
            _configCustomActivityMessage = Config.Bind("_General_", "Custom Activity Message", "Ingame", "Activity message to be displayed when display Activity is turned off. Keywords:  <mod_count>, <plugin_count>");
            _configCustomStateMessage = Config.Bind("_General_", "Custom State Message", "<mod_count> Mods, <plugin_count> Plugins", "State message to be displayed when display Activity is turned off. Keywords: <mod_count>, <plugin_count>");
            _configDisplayTime = Config.Bind("_General_", "Display Time", true, "Whether or not to display the elapsed time.");
            _configResetTime = Config.Bind("_General_", "Reset Elapsed Time", true, "Whether or not to reset the elapsed time when chancing activity and/or loading different character in maker.");
            _configDisplayLittleImage = Config.Bind("_General_", "Display Little Image", true, "Whether or not to display the little Image (in the bottom left corner of the big Image)");
            _configCustomLittleImage = Config.Bind("_General_", "Little Image", "tech", new ConfigDescription("Displayed little Image.", pictures));
            // ReSharper disable StringLiteralTypo
            _configCustomLittleImageText = Config.Bind("_General_", "Little Image Text", "by Varkaria#2048 and N. Jächa#1707", "Message to display when hovering the little Image");
            // ReSharper restore StringLiteralTypo

            _configBigImageMaker = Config.Bind("CharacterMaker", "Image", "sliders", new ConfigDescription("Displayed image when in Maker.", pictures));
            _configBigImageMakerText = Config.Bind("CharacterMaker", "Image Text", "Maker", "Message to display when hovering the Image while in maker");
            _configActivityMessageMaker = Config.Bind("CharacterMaker", "Activity Message", "Maker (<maker_sex>)", "Activity message to display when in maker. Keywords: <maker_sex>, <mod_count>, <plugin_count>");
            _configStateMessageMaker = Config.Bind("CharacterMaker", "State Message", "Editing: <chara_name>", "State message to display when editing a character in maker. Keywords: <chara_name>, <chara_nickname>, <mod_count>, <plugin_count>");

            _configBigImageStudio = Config.Bind("CharaStudio", "Image", "logo_studio", new ConfigDescription("Displayed image when in CharaStudio.", pictures));
            _configBigImageStudioText = Config.Bind("CharaStudio", "Image Text", "CharaStudio", "Message to display when hovering the Image while in Studio");
            _configActivityMessageStudio = Config.Bind("CharaStudio", "Activity Message", "In CharaStudio", "Activity Message to display when in Chara Studio. Keywords: <plugin_count>, <mod_count>");
            _configStateMessageStudio = Config.Bind("CharaStudio", "State Message", "<mod_count> Mods, <plugin_count> Plugins", "State Message to display when in Chara Studio. Keywords: <plugin_count>, <mod_count>");

            _configBigImageMainGame = Config.Bind("Main Game", "Image", "logo_main", new ConfigDescription("Displayed image when in MainGame (everything but Maker and Studio).", pictures));
            _configBigImageMainGameText = Config.Bind("Main Game", "Image Text", "Koikatsu", "Message to display when hovering the Image while in MainGame (everything but Maker and Studio)");
            _configActivityMessageMainGame = Config.Bind("Main Game", "Activity Message", "Ingame", "Activity message to display when in main game but not H-Scene. Keywords: <plugin_count>, <mod_count>");
            _configStateMessageMainGame = Config.Bind("Main Game", "State Message", "<mod_count> Mods, <plugin_count> Plugins", "State message to display when in main game but not H-Scene. Keywords: <plugin_count>, <mod_count>");

            _configBigImageUnknown = Config.Bind("Loading/Unknown", "Image", "logo_main", new ConfigDescription("Displayed image when gamemode is unknown (usually when the game is still starting/loading).", pictures));
            _configBigImageUnknownText = Config.Bind("CharacterMaker", "Image Text", "Loading", "Message to display when hovering the Image while the gamemode is unknown (usually when the game is still starting/loading)");
            _configActivityMessageUnknown = Config.Bind("Loading/Unknown", "Activity Message", "Starting Up", "Activity message to display when gamemode is unknown (usually when the game is still starting/loading)");
            _configStateMessageUnknown = Config.Bind("Loading/Unknown", "State Message", "", "State message to display when gamemode is unknown (usually when the game is still starting/loading)");

            _configDisplayHScene = Config.Bind("H-Scene", "Display H-Scene Activity", true, "Whether or not to show details when in a H-Scene. If this is turned off, it will show up as MainGame");
            _configBigImageHScene = Config.Bind("H-Scene", "Image", "logo_main_alt", new ConfigDescription("Displayed image when in H-Scene.", pictures));
            Config.Bind("H-Scene", "Image Text", "H-Scene", "Message to display when hovering the Image while in a H-Scene");
            _configActivityMessageHScene = Config.Bind("H-Scene", "Activity Message", "H-Scene <hscene_type>", "Activity message to display when in H-Scene. Keywords: <is_freeH>, <is_darkness>, <heroine_name>, <heroine_nickname>, <hscene_type>, <plugin_count>, <mod_count>");
            _configStateMessageHScene = Config.Bind("H-Scene", "State Message", "<hscene_mode> <heroine_name>", "State message to display when in H-Scene. Keywords: <hscene_mode>, <heroine_name>, <heroine_nickname>, <heroine_experience>, <heroine_safety>, <plugin_count>, <mod_count>");
            _configStateMessageHScene3P = Config.Bind("H-Scene", "State Message Two Girls", "<hscene_mode> <heroine_name> and <secondary_name>", "State message to display when in H-Scene with two Girls. Keywords: <hscene_mode>, <heroine_name>, <heroine_nickname>, <heroine_experience>, <heroine_safety>, <secondary_name>, <secondary_nickname>, <secondary_experience>, <secondary_safety>, <plugin_count>, <mod_count>");

            _hBoolFreeHFalse = Config.Bind("H-Scene Keywords", "FreeH False Keywordvalue", "", "Value of <is_freeH> when NOT in FreeH-Mode");
            _hBoolFreeHTrue = Config.Bind("H-Scene Keywords", "FreeH True Keywordvalue", "(Free H)", "Value of <is_freeH> when in FreeH-Mode");
            _hBoolDarknessFalse = Config.Bind("H-Scene Keywords", "Darkness False Keywordvalue", "", "Value of <is_darkness> when NOT in a darkness scene");
            _hBoolDarknessTrue = Config.Bind("H-Scene Keywords", "Darkness True Keywordvalue", "(Darkness)", "Value of <is_darkness> when in a darkness scene");
            _hModeAibzu = Config.Bind("H-Scene Keywords", "Mode Caress Keywordvalue", "Caressing", "Value of <hscene_mode> when caressing in normal H-Scene");
            _hModeHoushi = Config.Bind("H-Scene Keywords", "Mode Service Keywordvalue", "Serviced by", "Value of <hscene_mode> when getting serviced in normal H-Scene");
            _hModeSonyu = Config.Bind("H-Scene Keywords", "Mode Penetration Keywordvalue", "Doing", "Value of <hscene_mode> when penetrating in normal H-Scene");
            _hModeMasturbation = Config.Bind("H-Scene Keywords", "Mode Masturbation Keywordvalue", "Watching", "Value of <hscene_mode> when watching a masturbation H-Scene");
            _hModePeeping = Config.Bind("H-Scene Keywords", "Mode Peeping Keywordvalue", "Peeping on", "Value of <hscene_mode> when peeping in Storymode");
            _hModeLesbian = Config.Bind("H-Scene Keywords", "Mode Lesbian Keywordvalue", "Watching", "Value of <hscene_mode> when watching a lesbian H-Scene");
            _hModeHoushi3P = Config.Bind("H-Scene Keywords", "Mode Service Threesome Keywordvalue", "Serviced by", "Value of <hscene_mode> when getting serviced in threesome H-Scene");
            _hModeSonyu3P = Config.Bind("H-Scene Keywords", "Mode Penetration Threesome Keywordvalue", "Doing", "Value of <hscene_mode> when penetrating in threesome H-Scene");
            _hModeHoushi3Pmmf = Config.Bind("H-Scene Keywords", "Mode Service MMF Keywordvalue", "Serviced by", "Value of <hscene_mode> when getting serviced in MMF (darkness) H-Scene");
            _hModeSonyu3Pmmf = Config.Bind("H-Scene Keywords", "Mode Penetration MMF Keywordvalue", "Doing", "Value of <hscene_mode> when penetrating in MMF (darkness) H-Scene");
            _hTypeNormal = Config.Bind("H-Scene Keywords", "Type Normal Keywordvalue", "", "Value of <hscene_type> when in a normal H-Scene (Caress, Service, Penetration)");
            _hTypeWatching = Config.Bind("H-Scene Keywords", "Type Watching Keywordvalue", "(Spectating)", "Value of <hscene_type> when in a H-Scene where the player is not involved");
            _hTypeThreesome = Config.Bind("H-Scene Keywords", "Type Threesome Keywordvalue", "(Threesome)", "Value of <hscene_type> when in a threesome H-Scene (Threesome, Darkness)");
            _hSafetySafe = Config.Bind("H-Scene Keywords", "Safety Safe Keywordvalue", "Safe", "Value of <heroine_safety> and <secondary_safety> when the associated character is on a safe day");
            _hSafetyRisky = Config.Bind("H-Scene Keywords", "Safety Risky Keywordvalue", "Risky", "Value of <heroine_safety> and <secondary_safety> when the associated character is on a risky day");

            var handlers = new DiscordRPC.EventHandlers();
            DiscordRPC.Initialize(
                "835112124295806987",
                ref handlers,
                false, string.Empty);

            _startStamp = CurrentUnixStamp();
 
            currentStamp = _startStamp;
            _checkInterval = 3;
            _checkedCounts = false;
            CheckStatus();
            logger.LogInfo("Discord Rich Presence started");
        }
        private void LateUpdate()
        {
            if (!_checkedCounts)
            {
                _loadedPluginsCount = BepInEx.Bootstrap.Chainloader.PluginInfos.Count;
                _loadedModsCount = Sideloader.Sideloader.ZipArchives.Count;
                _checkedCounts = true;
            }
        }
        private void Update()
        {
            if (_coolDown > 0)
                _coolDown -= Time.deltaTime;
            else
            {
                _coolDown = _checkInterval;
                CheckStatus();
            }
        }
        private static long CurrentUnixStamp()
        {
            return (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        private void CheckStatus()
        {
            if (_configDisplayActivity.Value)
            {
                switch (KoikatuAPI.GetCurrentGameMode())
                {
                    case GameMode.Unknown:
                        SetStatus(_configActivityMessageUnknown.Value, _configStateMessageUnknown.Value, _startStamp, _configBigImageUnknown.Value, _configBigImageUnknownText.Value);
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
                        SetStatus(_configCustomActivityMessage.Value, _configCustomStateMessage.Value, _startStamp, _configCustomBigImage.Value, _configBigImageMainGameText.Value);
                        break;
                }
                _oldGamemode = KoikatuAPI.GetCurrentGameMode();
            }
            else
            {
                string activity = _configCustomActivityMessage.Value;
                string state = _configCustomStateMessage.Value;
                activity = activity.Replace("<mod_count>", _loadedModsCount.ToString());
                activity = activity.Replace("<plugin_count>", _loadedPluginsCount.ToString());
                state = state.Replace("<mod_count>", _loadedModsCount.ToString());
                state = state.Replace("<plugin_count>", _loadedPluginsCount.ToString());
                SetStatus(activity, state, _startStamp, _configCustomBigImage.Value, _configBigImageMainGameText.Value);
            }
        }
        void MakerStatus()
        {
            if (MakerAPI.InsideAndLoaded)
            {
                ChaFile character = MakerAPI.LastLoadedChaFile;
                string activity = _configActivityMessageMaker.Value;
                string state = _configStateMessageMaker.Value;
                activity = activity.Replace("<maker_sex>", MakerAPI.GetMakerSex() == 1 ? "female" : "male");
                if (_oldCharacter == null)
                {
                    currentStamp = CurrentUnixStamp();
                    _oldCharacter = character;
                }
                if (KoikatuAPI.GetCurrentGameMode() != _oldGamemode || character.charaFileName != _oldCharacter.charaFileName)
                {
                    currentStamp = CurrentUnixStamp();
                }
                activity = activity.Replace("<mod_count>", _loadedModsCount.ToString());
                activity = activity.Replace("<plugin_count>", _loadedPluginsCount.ToString());
                state = state.Replace("<mod_count>", _loadedModsCount.ToString());
                state = state.Replace("<plugin_count>", _loadedPluginsCount.ToString());
                state = state.Replace("<chara_name>", character.parameter.fullname);
                state = state.Replace("<chara_nickname>", character.parameter.nickname);
                SetStatus(activity, state, currentStamp, _configBigImageMaker.Value, _configBigImageMakerText.Value);

                _oldCharacter = character;
            }
            else
            {
                SetStatus(_configActivityMessageUnknown.Value, _configStateMessageUnknown.Value, _startStamp, _configBigImageUnknown.Value, _configBigImageUnknownText.Value);
            }
        }
        void StudioStatus()
        {
            
            if (StudioAPI.StudioLoaded)
            {
                string activity = _configActivityMessageStudio.Value;
                string state = _configStateMessageStudio.Value;

                activity = activity.Replace("<mod_count>", _loadedModsCount.ToString());
                activity = activity.Replace("<plugin_count>", _loadedPluginsCount.ToString());
                state = state.Replace("<mod_count>", _loadedModsCount.ToString());
                state = state.Replace("<plugin_count>", _loadedPluginsCount.ToString());

                SetStatus(activity, state, _startStamp, _configBigImageStudio.Value, _configBigImageStudioText.Value);
            }
            else
            {
                SetStatus(_configActivityMessageUnknown.Value, _configStateMessageUnknown.Value, _startStamp, _configBigImageStudio.Value, _configBigImageUnknownText.Value);
            }
        }
        void MainGameStatus()
        {
            string activity = _configActivityMessageMainGame.Value;
            string state = _configStateMessageMainGame.Value;
            string image = _configBigImageMainGame.Value;
            if (KoikatuAPI.GetCurrentGameMode() != _oldGamemode)
            {
                currentStamp = CurrentUnixStamp();
            }
            if (GameAPI.InsideHScene && _configDisplayHScene.Value)
            {
                if (hProc)
                {
                    image = _configBigImageHScene.Value;
                    activity = _configActivityMessageHScene.Value;

                    Dictionary<SaveData.Heroine.HExperienceKind, string> experienceDict = new Dictionary<SaveData.Heroine.HExperienceKind, string>()
                    {
                        {SaveData.Heroine.HExperienceKind.初めて, "First Time" },
                        {SaveData.Heroine.HExperienceKind.不慣れ, "Inexperienced" },
                        {SaveData.Heroine.HExperienceKind.慣れ, "Experienced" },
                        {SaveData.Heroine.HExperienceKind.淫乱, "Lewd" }
                    };
                    Dictionary<HFlag.MenstruationType, string> safetyDict = new Dictionary<HFlag.MenstruationType, string>()
                    {
                        { HFlag.MenstruationType.安全日, _hSafetySafe.Value},
                        {HFlag.MenstruationType.危険日,  _hSafetyRisky.Value}
                    };


                    List<SaveData.Heroine> females = hProc.dataH.lstFemale;
                    SaveData.Heroine heroine = females[0];
                    SaveData.Heroine secondary = null;
                    string secondaryExperience = string.Empty;
                    string secondarySafety = string.Empty;
                    string freeH = string.Empty;
                    string darkness = string.Empty;
                    string mode = string.Empty;
                    string type = string.Empty;

                    if (females.Count > 1)
                    {
                        state = _configStateMessageHScene3P.Value;
                        secondary = females[1];
                        secondaryExperience = experienceDict[secondary.HExperience];
                        secondarySafety = safetyDict[HFlag.GetMenstruation(secondary.MenstruationDay)];
                    }
                    else state = _configStateMessageHScene.Value;

                    var heroineExperience = experienceDict[heroine.HExperience];
                    var heroineSafety = safetyDict[HFlag.GetMenstruation(heroine.MenstruationDay)];

                    switch (hProc.dataH.isFreeH)
                    {
                        case true:
                            freeH = _hBoolFreeHTrue.Value;
                            break;
                        case false:
                            freeH = _hBoolFreeHFalse.Value;
                            break;
                    }
                    switch (hProc.dataH.isDarkness)
                    {
                        case true:
                            darkness = _hBoolDarknessTrue.Value;
                            break;
                        case false:
                            darkness = _hBoolDarknessFalse.Value;
                            break;
                    }
                    switch (hProc.flags.mode)
                    {
                        case HFlag.EMode.aibu:
                            mode = _hModeAibzu.Value;
                            type = _hTypeNormal.Value;
                            break;
                        case HFlag.EMode.houshi:
                            mode = _hModeHoushi.Value;
                            type = _hTypeNormal.Value;
                            break;
                        case HFlag.EMode.sonyu:
                            mode = _hModeSonyu.Value;
                            type = _hTypeNormal.Value;
                            break;
                        case HFlag.EMode.masturbation:
                            mode = _hModeMasturbation.Value;
                            type = _hTypeWatching.Value;
                            break;
                        case HFlag.EMode.lesbian:
                            mode = _hModeLesbian.Value;
                            type = _hTypeWatching.Value;
                            break;
                        case HFlag.EMode.peeping:
                            mode = _hModePeeping.Value;
                            type = _hTypeWatching.Value;
                            break;
                        case HFlag.EMode.houshi3P:
                            mode = _hModeHoushi3P.Value;
                            type = _hTypeThreesome.Value;
                            break;
                        case HFlag.EMode.sonyu3P:
                            mode = _hModeSonyu3P.Value;
                            type = _hTypeThreesome.Value;
                            break;
                        case HFlag.EMode.houshi3PMMF:
                            mode = _hModeHoushi3Pmmf.Value;
                            type = _hTypeThreesome.Value;
                            break;
                        case HFlag.EMode.sonyu3PMMF:
                            mode = _hModeSonyu3Pmmf.Value;
                            type = _hTypeThreesome.Value;
                            break;
                    }

                    activity = activity.Replace("<is_freeH>", freeH);
                    activity = activity.Replace("<is_darkness>", darkness);
                    activity = activity.Replace("<heroine_name>", heroine.Name);
                    activity = activity.Replace("<heroine_nickname>", heroine.nickname);
                    activity = activity.Replace("<hscene_type>", type);

                    state = state.Replace("<heroine_name>", heroine.Name);
                    state = state.Replace("<heroine_nickname>", heroine.nickname);
                    state = state.Replace("<heroine_experience>", heroineExperience);
                    state = state.Replace("<heroine_safety>", heroineSafety);
                    if (secondary != null)
                    {
                        state = state.Replace("<secondary_name>", secondary.Name);
                        state = state.Replace("<secondary_nickname>", secondary.nickname);
                        state = state.Replace("<secondary_experience>", secondaryExperience);
                        state = state.Replace("<secondary_safety>", secondarySafety);
                    }
                    state = state.Replace("<hscene_mode>", mode);
                }
            }
            activity = activity.Replace("<mod_count>", _loadedModsCount.ToString());
            activity = activity.Replace("<plugin_count>", _loadedPluginsCount.ToString());
            state = state.Replace("<mod_count>", _loadedModsCount.ToString());
            state = state.Replace("<plugin_count>", _loadedPluginsCount.ToString());
            SetStatus(activity, state, currentStamp, image, _configBigImageMainGameText.Value);
        }

        void SetStatus(string activity, string state, long timestamp, string img, string imgText)
        {
            if (!_configResetTime.Value) timestamp = _startStamp;
            if (!_configDisplayTime.Value) timestamp = 0;
            
            prsnc.details = activity;
            prsnc.state = state;
            prsnc.startTimestamp = timestamp;
            prsnc.largeImageKey = img;
            prsnc.largeImageText = imgText;
            if (_configDisplayLittleImage.Value)
            {
                prsnc.smallImageKey = _configCustomLittleImage.Value;
                prsnc.smallImageText = _configCustomLittleImageText.Value;
            }
            else
            {
                prsnc.smallImageKey = null;
                prsnc.smallImageText = null;
            }
            prsnc.partySize = 0;
            prsnc.partyMax = 0;
            DiscordRPC.UpdatePresence(ref prsnc);
        }
    }
}