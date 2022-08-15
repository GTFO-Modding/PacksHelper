
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
  

    [HarmonyPatch(typeof(PlayerInventoryLocal), nameof(PlayerInventoryLocal.SyncWieldItem))]

    internal static class Patch_onChangeGear
    {
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
                    PH_Manager.checkPackType(item.name);
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
            if(PH_Manager.pack_type > 0 && !visible)
            {
                return false;
            }
            return true;
        }

        
    }


}

