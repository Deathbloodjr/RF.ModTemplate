using BepInEx.Unity.IL2CPP.Utils;
using BepInEx.Unity.IL2CPP;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using BepInEx.Configuration;
using ModTemplate.Plugins;
using UnityEngine;
using System.Collections;
using SaveProfileManager.Plugins;

namespace ModTemplate
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, ModName, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        public const string ModName = "ModTemplate";

        public static Plugin Instance;
        private Harmony _harmony = null;
        public new static ManualLogSource Log;

        public static PluginSaveDataInterface plugin;

        public ConfigEntry<bool> ConfigEnabled;
        //public ConfigEntry<string> ConfigSongTitleLanguageOverride;
        //public ConfigEntry<float> ConfigFlipInterval;



        public override void Load()
        {
            Instance = this;

            Log = base.Log;

            SetupConfig();
            SetupHarmony();

            // The try catch has to be here, rather than inside the AddToSaveManager function, for some reason
            try
            {
                AddToSaveManager();
            }
            catch
            {

            }
        }

        private void SetupConfig()
        {
            var dataFolder = Path.Combine("BepInEx", "data", ModName);

            ConfigEnabled = Config.Bind("General",
                "Enabled",
                true,
                "Enables the mod.");

            //ConfigSongTitleLanguageOverride = Config.Bind("General",
            //    "SongTitleLanguageOverride",
            //    "JP",
            //    "Sets the song title to the selected language. (JP, EN, FR, IT, DE, ES, TW, CN, KO)");

            //ConfigFlipInterval = Config.Bind("General",
            //    "FlipInterval",
            //    3f,
            //    "How quickly the difficulty flips between oni and ura.");
        }

        private void SetupHarmony()
        {
            // Patch methods
            _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

            LoadPlugin();
        }

        public static void LoadPlugin()
        {
            if (Instance.ConfigEnabled.Value)
            {
                bool result = true;
                // If any PatchFile fails, result will become false
                //result &= Instance.PatchFile(typeof(SwapJpEngTitlesPatch));
                //result &= Instance.PatchFile(typeof(AdjustUraFlipTimePatch));
                //SwapJpEngTitlesPatch.SetOverrideLanguages();
                if (result)
                {
                    Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} is loaded!");
                }
                else
                {
                    Log.LogError($"Plugin {MyPluginInfo.PLUGIN_GUID} failed to load.");
                    // Unload this instance of Harmony
                    // I hope this works the way I think it does
                    Instance._harmony.UnpatchSelf();
                }
            }
            else
            {
                Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} is disabled.");
            }
        }

        private bool PatchFile(Type type)
        {
            if (_harmony == null)
            {
                _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            }
            try
            {
                _harmony.PatchAll(type);
#if DEBUG
                Log.LogInfo("File patched: " + type.FullName);
#endif
                return true;
            }
            catch (Exception e)
            {
                Log.LogInfo("Failed to patch file: " + type.FullName);
                Log.LogInfo(e.Message);
                return false;
            }
        }

        public static void UnloadPlugin()
        {
            Instance._harmony.UnpatchSelf();
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} has been unpatched.");
        }

        public static void ReloadPlugin()
        {
            // Reloading will always be completely different per mod
            // You'll want to reload any config file or save data that may be specific per profile
            // If there's nothing to reload, don't put anything here, and keep it commented in AddToSaveManager
            //SwapSongLanguagesPatch.InitializeOverrideLanguages();
            //TaikoSingletonMonoBehaviour<CommonObjects>.Instance.MyDataManager.MusicData.Reload();
        }

        public void AddToSaveManager()
        {
            // Add SaveDataManager path to your csproj.user file
            // https://github.com/Deathbloodjr/RF.SaveProfileManager
            plugin = new PluginSaveDataInterface(MyPluginInfo.PLUGIN_GUID);
            plugin.AssignLoadFunction(LoadPlugin);
            plugin.AssignUnloadFunction(UnloadPlugin);
            //plugin.AssignReloadSaveFunction(ReloadPlugin);
            plugin.AddToManager();
            //Logger.Log("Plugin added to SaveDataManager");
        }

        public static MonoBehaviour GetMonoBehaviour() => TaikoSingletonMonoBehaviour<CommonObjects>.Instance;
        public Coroutine StartCoroutine(IEnumerator enumerator)
        {
            return GetMonoBehaviour().StartCoroutine(enumerator);
        }
    }
}
