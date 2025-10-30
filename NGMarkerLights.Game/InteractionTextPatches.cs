using DV;
using HarmonyLib;

namespace NGMarkerLights.Game
{
    public class InteractionTextPatches
    {
        [HarmonyPatch(typeof(InteractionText), nameof(InteractionText.GetText))]
        public static class GetTextPatch
        {
            public static bool Prefix(InteractionInfoType infoType, ref string __result)
            {
                if (infoType == Lantern.SwitchColor)
                {
                    __result = $"Press {InteractionText.Instance.BtnUse} to switch color";
                    return false;
                }

                return true;
            }
        }
    }
}