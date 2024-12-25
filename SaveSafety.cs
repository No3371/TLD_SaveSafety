using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.Gear;
using Il2CppTLD.Scenes;
using MelonLoader;
using MelonLoader.TinyJSON;
using UnityEngine;
using Il2CppTLD.News;
using Il2CppTLD.IntBackedUnit;

namespace SaveSafety
{
    internal class SaveSafety : MelonMod
    {
		internal static SaveSafety Instance { get; private set; }
		internal static int SaveCounter { get; set; } 
		internal static int LoadCounter { get; set; }
		bool Triggered { get; set; }

        public override void OnInitializeMelon()
        {
			Instance = this;

			uConsole.RegisterCommand("savesafety_force_trigger", new Action(() => {
				TestExplosion.Enabled = !TestExplosion.Enabled;
			}));
        }

        public override void OnUpdate()
        {
			if (Time.frameCount % 360 != 0)
				return;
			if (SaveCounter % 2 != 0 || LoadCounter % 2 != 0)
			{
                InterfaceManager.GetPanel<Panel_HUD>().DisplayWarningMessage($"!!! SAVE SAFETY TRIGGERED !!!\nSomething blew up the saving/loading procedure\nThis is very bad and could harm your game progress\nSave: {SaveCounter} / Load: {LoadCounter}\n(Numbers above should not be odd)");
				if (!Triggered)
				{
					this.LoggerInstance.BigError($"!!! SAVE SAFETY TRIGGERED !!!\nSomething blew up the saving/loading procedure\nThis is very bad and could harm your game progress\nSave: {SaveCounter} / Load: {LoadCounter}\n(Numbers above should not be odd)");
					Triggered = true;
				}
			}

        }
    }

    [HarmonyPatch(nameof(SaveGameSystem), nameof(SaveGameSystem.RestoreGlobalData))]
    [HarmonyPatch(nameof(SaveGameSystem), nameof(SaveGameSystem.SaveGlobalData))]
	internal static class TestExplosion
	{
		internal static bool Enabled { get; set; }
		internal static void Postfix ()
		{
			if (!Enabled)
				return;
			SaveSafety.Instance.LoggerInstance.Msg("Faking an explosion");
			throw new Exception("BOOM");
		}
	}

    [HarmonyPatch(nameof(SaveGameSystem), nameof(SaveGameSystem.RestoreGlobalData))]
	internal static class RestoreGlobalData
	{
		[HarmonyPriority(99999999)]
		internal static void Prefix ()
		{
			SaveSafety.LoadCounter++;
		}

    	[HarmonyPriority(-9999999)]
		internal static void Postfix ()
		{
			SaveSafety.LoadCounter++;
		}
	}

    [HarmonyPatch(nameof(SaveGameSystem), nameof(SaveGameSystem.SaveGlobalData))]
	internal static class SaveGlobalData
	{
		[HarmonyPriority(99999999)]
		internal static void Prefix ()
		{
			SaveSafety.SaveCounter++;
		}

    	[HarmonyPriority(-9999999)]
		internal static void Postfix ()
		{
			SaveSafety.SaveCounter++;
		}
	}
}
