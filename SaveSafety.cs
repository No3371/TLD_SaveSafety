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
		internal static bool Triggered { get; set; }
		internal static bool SaveFlag { get; set; }
		internal static bool LoadFlag { get; set; }

        public override void OnInitializeMelon()
        {
			Instance = this;

			uConsole.RegisterCommand("savesafety_force_trigger", new Action(() => {
				TestExplosion.Enabled = !TestExplosion.Enabled;
			}));
        }

		string LoadWarningString => $"[SAVE SAFETY TRIGGERED]\nSomething blew up the loading procedure\nIf the game saves again in this state, you may lose progress of some mods.\nConsider quitting without saving now \nSAVE: [{(SaveFlag? "X":" ")}] / LOAD: [{(LoadFlag? "X":" ")}]\n";
		string SaveWarningString => $"[SAVE SAFETY TRIGGERED]\nSomething blew up the saving procedure\nBe aware that your progress now may not be saved if this keep happening. \nSAVE: [{(SaveFlag? "X":" ")}] / LOAD: [{(LoadFlag? "X":" ")}]\n";

        public override void OnUpdate()
        {
			if (Time.frameCount % 60 != 0)
				return;
			if (SaveFlag)
			{
                InterfaceManager.GetPanel<Panel_HUD>().DisplayWarningMessage(SaveWarningString);
				if (!Triggered)
				{
					this.LoggerInstance.BigError(SaveWarningString);
					Triggered = true;
				}
			}
			if (LoadFlag)
			{
                InterfaceManager.GetPanel<Panel_HUD>().DisplayWarningMessage(LoadWarningString);
				if (!Triggered)
				{
					this.LoggerInstance.BigError(LoadWarningString);
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
			SaveSafety.LoadFlag = true;
		}

    	[HarmonyPriority(-9999999)]
		internal static void Postfix ()
		{
			SaveSafety.LoadFlag = false;
		}
	}

    [HarmonyPatch(nameof(SaveGameSystem), nameof(SaveGameSystem.SaveGlobalData))]
	internal static class SaveGlobalData
	{
		static bool Flag { get; set; }
		[HarmonyPriority(99999999)]
		internal static void Prefix ()
		{
			SaveSafety.SaveFlag = true;
		}

    	[HarmonyPriority(-9999999)]
		internal static void Postfix ()
		{
			SaveSafety.SaveFlag = false;
			SaveSafety.Triggered = false;
			InterfaceManager.GetPanel<Panel_HUD>()?.ClearWarningMessage();
		}
	}
	
    [HarmonyPatch(nameof(GameManager), nameof(GameManager.OnGameQuit))]
	internal static class OnGameQuit
	{
		internal static void Postfix ()
		{
			SaveSafety.SaveFlag = SaveSafety.LoadFlag = SaveSafety.Triggered = false;
		}
	}
    [HarmonyPatch(nameof(GameManager), nameof(GameManager.HandlePlayerDeath))]
	internal static class HandlePlayerDeath
	{
		internal static void Postfix ()
		{
			SaveSafety.SaveFlag = SaveSafety.LoadFlag = SaveSafety.Triggered = false;
		}
	}
}
