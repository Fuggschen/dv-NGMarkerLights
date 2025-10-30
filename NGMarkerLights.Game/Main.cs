using System.Reflection;
using System.Collections.Generic;
using custom_item_components;
using custom_item_mod;
using HarmonyLib;
using UnityModManagerNet;
using NGMarkerLights.Unity;

namespace NGMarkerLights.Game
{
    public static class Main
    {
        private static UnityModManager.ModEntry Instance { get; set; } = null!;
        public static Dictionary<string, int> LanternColorData { get; set; } = new Dictionary<string, int>();

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Instance = modEntry;

            var harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            RegisterGadgets();
            return true;
        }

        private static void RegisterGadgets()
        {
            // Register the lantern gadget
            try
            {
                CustomGadgetBaseMap.RegisterGadgetImplementation(
                    typeof(LanternProxy),
                    typeof(Lantern),
                    (GadgetBase source, ref DV.Customization.Gadgets.GadgetBase target) =>
                    {
                        var replacement = target as Lantern;
                        if (replacement != null)
                        {
                            CopyLanternProxyFields(source, replacement);
                        }
                    }
                );
            }
            catch (System.Exception ex)
            {
                Error($"Failed to register lantern gadget: {ex}");
            }
        }

        private static void CopyLanternProxyFields(GadgetBase proxy, Lantern replacement)
        {
            try
            {
                var proxyType = proxy.GetType();
                replacement.requirements.trainCarPresence = DV.Customization.TrainCarCustomization
                    .TrainCarCustomizerBase
                    .CustomizerTrainCarRequirements.RequireTrainCar;

                // Copy offMaterial
                var offMaterialField = proxyType.GetField("offMaterial");
                if (offMaterialField != null)
                {
                    replacement.offMaterial = offMaterialField.GetValue(proxy) as UnityEngine.Material;
                }

                // Copy colorMaterials array
                var colorMaterialsField = proxyType.GetField("colorMaterials");
                if (colorMaterialsField != null)
                {
                    replacement.colorMaterials = colorMaterialsField.GetValue(proxy) as UnityEngine.Material[];
                }

                // Copy lanternRenderer
                var lanternRendererField = proxyType.GetField("lanternRenderer");
                if (lanternRendererField != null)
                {
                    replacement.lanternRenderer = lanternRendererField.GetValue(proxy) as UnityEngine.Renderer;
                }

                // Copy materialIndex
                var materialIndexField = proxyType.GetField("materialIndex");
                if (materialIndexField != null)
                {
                    replacement.materialIndex = (int)materialIndexField.GetValue(proxy);
                }

                // Copy interactionCollider
                var interactionColliderField = proxyType.GetField("interactionCollider");
                if (interactionColliderField != null)
                {
                    replacement.interactionCollider = interactionColliderField.GetValue(proxy) as UnityEngine.GameObject;
                }

                // Copy sourceLight
                var sourceLightField = proxyType.GetField("sourceLight");
                if (sourceLightField != null)
                {
                    replacement.sourceLight = sourceLightField.GetValue(proxy) as UnityEngine.Light;
                }
            }
            catch (System.Exception ex)
            {
                Error($"Failed to copy lantern proxy fields: {ex}");
            }
        }

        internal static void Log(string message)
        {
            Instance.Logger.Log(message);
        }

        internal static void Warning(string message)
        {
            Instance.Logger.Warning(message);
        }

        internal static void Error(string message)
        {
            Instance.Logger.Error(message);
        }
    }

    /// <summary>
    /// Harmony patch to replace glare material grabber proxy components when MonoBehaviour awakens
    /// </summary>
    [HarmonyPatch(typeof(GlareMaterialGrabberProxy), "Awake")]
    public static class GlareMaterialGrabberProxyReplacementPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(GlareMaterialGrabberProxy __instance)
        {
            try
            {
                // Create the replacement component
                var replacement = __instance.gameObject.AddComponent<GlareMaterialGrabber>();

                // Copy glareObjects array from proxy to replacement
                replacement.glareObjects = __instance.glareObjects;

                // Destroy the proxy component
                UnityEngine.Object.Destroy(__instance);

                // Prevent the original Awake from running
                return false;
            }
            catch (System.Exception ex)
            {
                Main.Error($"Failed to replace glare material grabber proxy: {ex}");
                return true; // Let original Awake run if we fail
            }
        }
    }
}
