using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;

namespace PacksHelper
{
	[BepInPlugin("Localia.PacksHelper", "PacksHelper", "1.5.0")]

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

