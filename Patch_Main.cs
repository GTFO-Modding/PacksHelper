
using HarmonyLib;
using UnityEngine;
using Player;
using System.Collections.Generic;
using Gear;
using AK;
using Localization;




namespace PacksHelper
{
    internal static class PH_Manager
    {
        public static bool canCheckTar = false;
        public static bool canCheckInter = false;
        public static float forceUpateHUDTIme = 0.0f; 
        public static int pack_type = 0; 
        public static int last_pack_type = 0; 
        public static string[] health = new string[4];
        public static string[] standard = new string[4];
        public static string[] special = new string[4];
        public static string[] tool = new string[4];
        public static string[] ori_all_info = new string[4];
        public static string[] now_show_info = new string[4];
     


        public static void checkPackType(string name)
        {
            if(name == "AmmoPackWeaponFirstPerson(Clone)")
            {
                pack_type = 1;
            }
            else if(name == "MediPackFirstPerson(Clone)" || name == "DisinfectonPackFirstPerson(Clone)")
            {
                pack_type = 2;
            }
            else if(name == "AmmoPackToolFirstPerson(Clone)")
            {
                pack_type = 3;
            }
            else
            {
                pack_type = 0;
            }
            
            // Logs.Info(string.Format("==={0} {1}",item.name,state));

            if(pack_type > 0)
            {
                PH_Manager.showAll();
            }

            if(pack_type > 0 || (last_pack_type > 0 && pack_type != last_pack_type ))
            {
                PH_Manager.forceUpdateHUDAll();
            }
            last_pack_type = pack_type;

        }
        public static void showAll()
        {
            Il2CppSystem.Collections.Generic.List<PlayerAgent> playerAgentsInLevel = PlayerManager.PlayerAgentsInLevel;
            for (int i = 0; i < playerAgentsInLevel.Count; i++)
            {
                if (playerAgentsInLevel[i] != null && !playerAgentsInLevel[i].IsLocallyOwned)
                {
                    if(pack_type > 0 && !playerAgentsInLevel[i].NavMarker.ExtraInfoVisible)
                    {
                        playerAgentsInLevel[i].NavMarker.SetExtraInfoVisible(true);
                    }

                }
            }
        }
        public static void forceUpdateHUDAll()
        {
            PH_Manager.forceUpateHUDTIme = Clock.Time + 0.1f;
            for (int i = 0; i < PlayerManager.PlayerAgentsInLevel.Count; i++)
            {
                PlayerAgent client = PlayerManager.PlayerAgentsInLevel[i];
                if (client != null && !client.IsLocallyOwned)
                {
                   
                    client.NavMarker.m_isInfoDirty = true;
                    client.NavMarker.UpdateExtraInfo();
            
                    // client.NavMarker.m_marker.SetVisualStates(NavMarkerOption.Player, NavMarkerOption.Player, NavMarkerOption.Player, NavMarkerOption.Player);
                }
            }
        }

        public static string get_ext_changed(PlaceNavMarkerOnGO marker,int slot)
        {   
           
            if(pack_type == 1)
            {
                now_show_info[slot] = "<color=#CCCCCCFF><size=140%>" + standard[slot] + "\n" + special[slot] + "</size></color>";
            }
            else if(pack_type == 2)
            {
                now_show_info[slot] = "<color=#CCCCCCFF><size=140%>" + health[slot] +  "</size></color>";
            }
            else if(pack_type == 3)
            {
                now_show_info[slot] = "<color=#CCCCCCFF><size=140%>" + tool[slot] +  "</size></color>";
            }
            else
            {
                now_show_info[slot] = ori_all_info[slot];
            }
            return now_show_info[slot];
    
            
        
        }

   
    
        public static void update_ori_hud_info(PlaceNavMarkerOnGO marker,int slot)
        {   
     
            string oris = marker.m_extraInfo;
            ori_all_info[slot]= oris;
            now_show_info[slot] = "";
            health[slot] = "";
            standard[slot] = "";
            special[slot] = "";
            tool[slot] = "";

            // Logs.Info($"oris {oris}");
            string[] arrays = oris.Split("\n");
            int index = 0;
            if(arrays.Length > 3)
            {
                health[slot] = arrays[index];
                index ++;
                if(arrays[index].IndexOf("<color=#00FFA8>",0) != -1)//add infection
                {
                    health[slot] = health[slot] + "\n" + arrays[index];
                    index ++;
                }
                health[slot] = health[slot].Replace("<color=#CCCCCC66><size=70%>","").Replace("66>","FF>").Replace("A8>","FF>");
                // Logs.Info($"health {health[slot]}");

                standard[slot] = arrays[index].Replace("66>","FF>");
                index ++;
                // Logs.Info($"standard {standard[slot]}");

                special[slot] = arrays[index].Replace("66>","FF>");
                index ++;
                // Logs.Info($"special {special[slot]}");

                tool[slot] = arrays[index].Replace("66>","FF>").Replace("</size></color>","");
                index ++;
                // Logs.Info($"tool {tool[slot]}");

            }
        }

    }


    [HarmonyPatch(typeof(ResourcePackFirstPerson))]

    internal static class patch_ResourcePackFirstPersonUpdate
    {
        [HarmonyPatch(nameof(ResourcePackFirstPerson.UpdateInteraction))]
        [HarmonyPrefix]
        [HarmonyPriority((Priority.VeryLow))] 
        [HarmonyWrapSafe]
        public static void UpdateInteraction_pre(ResourcePackFirstPerson __instance)
        {   
            bool isActive = __instance.m_interactApplyResource.TimerIsActive;
            
            if(!isActive)
            {
                __instance.m_lastActionReceiver = null;
                PH_Manager.canCheckTar = true;
            }
        }

        
        [HarmonyPatch(nameof(ResourcePackFirstPerson.UpdateInteraction))]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        public static void UpdateInteraction_post(ResourcePackFirstPerson __instance)
        {   
            PH_Manager.canCheckTar = false;
            PH_Manager.canCheckInter = false;
            if(Clock.Time > PH_Manager.forceUpateHUDTIme)
            {
                PH_Manager.forceUpdateHUDAll();
            }
        }

        // [HarmonyBefore(new string[] { "net.example.plugin2" })]
        [HarmonyPatch(nameof(ResourcePackFirstPerson.UpdateInteractionActionName))]
        [HarmonyPrefix]
        [HarmonyWrapSafe]
       
        public static bool UpdateInteractionActionName(ResourcePackFirstPerson __instance)
        {
            if(!PH_Manager.canCheckTar)
            {
                return true;
            }
            
            PH_Manager.canCheckTar =false;
    
            if(InputMapper.GetButtonDown.Invoke(InputAction.Use, __instance.Owner.InputFilter))
            {
                __instance.m_actionReceiver = __instance.Owner.GetComponentInChildren<iResourcePackReceiver>();
                RaycastHit raycastHit;
                if (Physics.Raycast(__instance.Owner.FPSCamera.Position, __instance.Owner.FPSCamera.Forward, out raycastHit, 2.4f, LayerManager.MASK_GIVE_RESOURCE_PACK))
                {
                    iResourcePackReceiver componentInParent = raycastHit.collider.GetComponentInParent<iResourcePackReceiver>();
                    if (componentInParent != null)
                    {
                        __instance.m_actionReceiver = componentInParent;
                    }
                }
            }
            else if(InputMapper.GetButton.Invoke(InputAction.Fire, __instance.Owner.InputFilter))
            {
                __instance.m_actionReceiver = __instance.Owner.GetComponentInChildren<iResourcePackReceiver>();
            }
            else 
            {
                __instance.m_actionReceiver = __instance.Owner.GetComponentInChildren<iResourcePackReceiver>();
                
                RaycastHit raycastHit;
                if (Physics.Raycast(__instance.Owner.FPSCamera.Position, __instance.Owner.FPSCamera.Forward, out raycastHit, 2.4f, LayerManager.MASK_GIVE_RESOURCE_PACK))
                {
                    iResourcePackReceiver componentInParent = raycastHit.collider.GetComponentInParent<iResourcePackReceiver>();
                    if (componentInParent != null)
                    {
                        __instance.m_actionReceiver = componentInParent;
                    }
                }

            }

            
            __instance.m_interactApplyResource.m_input = InputAction.Use;
            if (__instance.m_actionReceiver.IsLocallyOwned)
            {
                __instance.UpdateInteractionActionName(Localization.Text.Get(856u));
                if(!InputMapper.GetButtonDown.Invoke(InputAction.Use, __instance.Owner.InputFilter))
                {
                    __instance.m_interactApplyResource.m_input = InputAction.Fire;
                }
            }
            else
            {
                __instance.UpdateInteractionActionName(__instance.m_actionReceiver.InteractionName);
                PlayerAgent agent= __instance.m_actionReceiver.TryCast<PlayerAgent>();
                if(agent != null)
                {
                    int now_slot = agent.Owner.PlayerSlotIndex();
                    __instance.m_interactApplyResource.InteractionMessage += "\n" + PH_Manager.now_show_info[now_slot];
                }
            
                if(!InputMapper.GetButtonDown.Invoke(InputAction.Use, __instance.Owner.InputFilter))
                {
                    __instance.m_interactApplyResource.m_input = InputAction.Aim;
                }
            }
             
            PH_Manager.canCheckInter = true;
            return false;

        }


       
    }


    [HarmonyPatch(typeof(Interact_Timed))]
    internal static class patch_Interact_Timed
    {
        [HarmonyPatch(nameof(Interact_Timed.PlayerCheckInput))]
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        public static bool PlayerCheckInput(Interact_Timed __instance, bool __result,PlayerAgent agent)
        {
            if(!PH_Manager.canCheckInter)
            {
                return true;
            }
            PH_Manager.canCheckInter = false;

            if (!__instance.InternalAllowInput)
            {
                return true;
            }

            if (__instance.m_localPlayerInteractInfo == null)
            {
                if (InputMapper.GetButton.Invoke(__instance.InputAction, agent.InputFilter))
                {
                    __instance.OnInteractorStateChanged(agent, true);
                    __result = true;
                }
                __result = false;
                return false;
            }
            return true;

        }


    }

}
