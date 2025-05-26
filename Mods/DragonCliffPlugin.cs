using BepInEx;
using BepInEx.Logging;
using eradev.dragoncliff.Mods.Features;
using System;

namespace eradev.dragoncliff.Mods
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class DragonCliffPlugin : BaseUnityPlugin
    {
        public static DragonCliffPlugin Instance { get; private set; }

        public static ManualLogSource Log;

        public static event EventHandler UpdateEvt;

        public DragonCliffPlugin()
        {
            Log = Logger;
        }

        private void Awake()
        {
            AlwaysAncientGradeGeneration.Register(Config);
            AlwaysAncientGradeResidentRecruit.Register(Config);
            AlwaysDropInvitation.Register(Config);
            AlwaysPerfectAdventurerRecruit.Register(Config);
            AutoQuest.Register(Config);
            CustomGameSpeeds.Register(Config);
            FreeEndlessDungeon.Register(Config);
            FreeCrafting.Register(Config);
            MissionaryAI.Register(Config);
            NoDuplicateRecipeDrop.Register(Config);

            new HarmonyLib.Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();

            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void Update()
        {
            InvokeUpdate();
        }

        private static void InvokeUpdate()
        {
            UpdateEvt?.Invoke(Instance, EventArgs.Empty);
        }
    }
}
