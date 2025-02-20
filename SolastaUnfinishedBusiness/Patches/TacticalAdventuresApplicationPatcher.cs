﻿using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using JetBrains.Annotations;
using static SolastaUnfinishedBusiness.Models.SaveByLocationContext;

namespace SolastaUnfinishedBusiness.Patches;

[UsedImplicitly]
public static class TacticalAdventuresApplicationPatcher
{
    [HarmonyPatch(typeof(TacticalAdventuresApplication), nameof(TacticalAdventuresApplication.SaveGameDirectory),
        MethodType.Getter)]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
    [UsedImplicitly]
    public static class SaveGameDirectory_Getter_Patch
    {
        [UsedImplicitly]
        public static bool Prefix(ref string __result)
        {
            //PATCH: EnableSaveByLocation
            if (!Main.Settings.EnableSaveByLocation)
            {
                return true;
            }

            // Modify the value returned by TacticalAdventuresApplication.SaveGameDirectory so that saves
            // end up where we want them (by location/campaign)

            var selectedCampaignService = ServiceRepository.GetService<SelectedCampaignService>();

            __result = selectedCampaignService?.SaveGameDirectory ?? DefaultSaveGameDirectory;

            return false;
        }
    }
}
