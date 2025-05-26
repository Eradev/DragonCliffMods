using BepInEx.Configuration;
using HarmonyLib;
using System;
using UnityEngine;

namespace eradev.dragoncliff.Mods.Features
{
    internal class CustomGameSpeeds
    {
        private const string FeatureName = nameof(CustomGameSpeeds);

        private static ConfigEntry<float> _speed1;
        private static ConfigEntry<float> _speed2;
        private static ConfigEntry<float> _speed3;

        public static void Register(ConfigFile config)
        {
            _speed1 = config.Bind(FeatureName, "speed1", 1.5f, "Speed 1");
            _speed2 = config.Bind(FeatureName, "speed2", 2f, "Speed 2");
            _speed3 = config.Bind(FeatureName, "speed3", 3f, "Speed 3");

            _speed1.SettingChanged += RefreshSpeeds;
            _speed2.SettingChanged += RefreshSpeeds;
            _speed3.SettingChanged += RefreshSpeeds;
        }

        public static void RefreshSpeeds(object sender, EventArgs eventArgs)
        {
            TownManager.Instance?.Ui.GameSpeedPanel.UpdateTimeState(1f);
        }

        [HarmonyPatch(typeof(GameSpeedPanelController), nameof(GameSpeedPanelController.SpeedUpOnePointFive))]
        public class GameSpeedPanelControllerSpeedUpOnePointFivePatch
        {
            public static bool Prefix(GameSpeedPanelController __instance)
            {
                GameWorld.instance.PlayerProfile.TimeScaleSetting = _speed1.Value;
                __instance.UpdateTimeState(_speed1.Value);
                BattleManager.instance.AdventureUi.EnableEscapeButton();

                return false;
            }
        }

        [HarmonyPatch(typeof(GameSpeedPanelController), nameof(GameSpeedPanelController.SpeedUpTwo))]
        public class GameSpeedPanelControllerSpeedUpTwoPatch
        {
            public static bool Prefix(GameSpeedPanelController __instance)
            {
                GameWorld.instance.PlayerProfile.TimeScaleSetting = _speed2.Value;
                __instance.UpdateTimeState(_speed2.Value);
                BattleManager.instance.AdventureUi.EnableEscapeButton();

                return false;
            }
        }

        [HarmonyPatch(typeof(GameSpeedPanelController), nameof(GameSpeedPanelController.SpeedUpThree))]
        public class GameSpeedPanelControllerSpeedUpThreePatch
        {
            public static bool Prefix(GameSpeedPanelController __instance)
            {
                GameWorld.instance.PlayerProfile.TimeScaleSetting = _speed3.Value;
                __instance.UpdateTimeState(_speed3.Value);
                BattleManager.instance.AdventureUi.EnableEscapeButton();

                return false;
            }
        }

        [HarmonyPatch(typeof(GameSpeedPanelController), nameof(GameSpeedPanelController.UpdateTimeState))]
        public class GameSpeedPanelControllerUpdateTimeStatePatch
        {
            public static bool Prefix(GameSpeedPanelController __instance, ref float timeScale, ref SpeedUpButtonType ____currentSpeed)
            {
                if (Math.Abs(timeScale) < 0.1)
                {
                    ____currentSpeed = SpeedUpButtonType.Pause;
                }
                else if (Math.Abs(timeScale - 1f) < 0.1)
                {
                    ____currentSpeed = SpeedUpButtonType.NormalSpeed;
                }
                else if (Math.Abs(timeScale - _speed1.Value) < 0.1)
                {
                    ____currentSpeed = SpeedUpButtonType.SpeedUpOnePointFive;
                }
                else if (Math.Abs(timeScale - _speed2.Value) < 0.1)
                {
                    ____currentSpeed = SpeedUpButtonType.SpeedUpTwo;
                }
                else if (Math.Abs(timeScale - _speed3.Value) < 0.1)
                {
                    ____currentSpeed = SpeedUpButtonType.SpeedUpThree;
                }
                else
                {
                    GameWorld.instance.PlayerProfile.TimeScaleSetting = 1f;
                    timeScale = 1f;
                    ____currentSpeed = SpeedUpButtonType.NormalSpeed;
                }
                Time.timeScale = timeScale;
                AccessTools.Method(typeof(GameSpeedPanelController), "UpdateButtonsColor").Invoke(__instance, null);

                return false;
            }
        }

        // Disable speed hack check
        [HarmonyPatch(typeof(GameWorld), "Start")]
        public class GameWorldStartPatch
        {
            public static bool Prefix()
            {
                return false;
            }
        }
    }
}
