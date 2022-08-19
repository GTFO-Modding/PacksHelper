using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace PacksHelper
{
	[BepInPlugin("Localia.PacksHelper", "PacksHelper", "3.0.0")]

	public class EntryPoint : BasePlugin
	{
		private Harmony m_Harmony;

		public override void Load()
		{
			this.m_Harmony = new Harmony("Localia.PacksHelper");
			this.m_Harmony.PatchAll();
		
			Logs.Info("OK");

			
		}
	}
}

