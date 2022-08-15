
using System;
using HarmonyLib;
using UnityEngine;
using Player;
using System.Collections.Generic;
using Gear;
using AK;

using Localization;




namespace PacksHelper
{
    // [HarmonyPatch(typeof(FPSCamera), nameof(FPSCamera.UpdateLookatTeammates))]

    // internal static class Patch_UpdateLookatTeammates
    // {

    //     public static void Postfix(FPSCamera __instance)
    //     {
    //         Il2CppSystem.Collections.Generic.List<PlayerAgent> playerAgentsInLevel = PlayerManager.PlayerAgentsInLevel;
    //         for (int i = 0; i < playerAgentsInLevel.Count; i++)
    //         {
    //             if (playerAgentsInLevel[i] != null && !playerAgentsInLevel[i].IsLocallyOwned)
    //             {
    //                 if(Patch_onChangeGear.state > 0 && !playerAgentsInLevel[i].NavMarker.ExtraInfoVisible)
    //                 {
    //                     playerAgentsInLevel[i].NavMarker.SetExtraInfoVisible(true);
    //                 }

    //             }
    //         }
    
    //     }
    // }

    [HarmonyPatch(typeof(PlayerInventoryLocal), nameof(PlayerInventoryLocal.SyncWieldItem))]

    internal static class Patch_onChangeGear
    {
        public static int state = 0; 
        public static int lastState = 0; 
        
        public static void Postfix(PlayerInventoryLocal __instance, ItemEquippable item)
        {
            if ( GameStateManager.Current.m_currentStateName == eGameStateName.InLevel)
            {
                if (!__instance.AllowedToWieldItem)
                {
                    return;
                }
    
                if (__instance.m_playerEnabled)
                {
                    string name = item.name;
                    if(name == "AmmoPackWeaponFirstPerson(Clone)")
                    {
                        state = 1;
                    }
                    else if(name == "MediPackFirstPerson(Clone)" || name == "DisinfectonPackFirstPerson(Clone)")
                    {
                        state = 2;
                    }
                    else if(name == "AmmoPackToolFirstPerson(Clone)")
                    {
                        state = 3;
                    }
                    else
                    {
                        state = 0;
                    }
                    
                    // Logs.Info(string.Format("==={0} {1}",item.name,state));

                    if(state > 0)
                    {
                        PH_Manager.showAll();
                    }

                    if(state > 0 || (state != lastState && lastState > 0))
                    {
                        PH_Manager.forceUpdateHUDAll();
                    }
                    lastState = state;
                    
                }

            }
        }
       

    }

    

    [HarmonyPatch(typeof(PlaceNavMarkerOnGO))]
    internal static class Patch_PlaceNavMarkerOnGO
    {
        [HarmonyPatch(nameof(PlaceNavMarkerOnGO.UpdateExtraInfo))]
        [HarmonyPostfix]
        [HarmonyWrapSafe]

        public static void UpdateExtraInfo(PlaceNavMarkerOnGO __instance)
        {
            if(GameStateManager.Current.m_currentStateName == eGameStateName.InLevel)
            {
                int slot = __instance.m_player.Owner.PlayerSlotIndex();

                if(__instance.m_extraInfo != PH_Manager.ori_all_info[slot] && __instance.m_extraInfo != PH_Manager.now_show_info[slot])
                {
                    PH_Manager.update_ori_hud_info(__instance,slot);
                }
                __instance.m_extraInfo = PH_Manager.get_ext_changed(__instance,slot);
                
                if(__instance.m_extraInfoVisible)
                {
                    __instance.UpdateName(__instance.Player.Owner.NickName, PH_Manager.now_show_info[slot]);
                }

            } 
        }


        [HarmonyPatch(nameof(PlaceNavMarkerOnGO.SetExtraInfoVisible))]
        [HarmonyPrefix]
        [HarmonyWrapSafe]

        public static bool SetExtraInfoVisible(PlaceNavMarkerOnGO __instance,bool visible)
        {
            if(Patch_onChangeGear.state > 0 && !visible)
            {
                return false;
            }
            return true;
        }






        
    }


}

