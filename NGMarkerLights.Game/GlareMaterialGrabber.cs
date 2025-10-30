using System.Linq;
using UnityEngine;

namespace NGMarkerLights.Game
{
    public class GlareMaterialGrabber : MonoBehaviour
    {
        private const string MATERIAL_NAME = "TaillightsGlare";
        private const string TINT_COLOR_PROPERTY = "_TintColor";

        // Configuration from proxy
        public GameObject[]? glareObjects;

        // Runtime state
        private Material? _glareMaterial;
        private Lantern? _lantern;
        private int _lastColorIndex = -1;

        public void Start()
        {
            try
            {
                GrabAndApplyMaterial();

                // Find the Lantern component on this GameObject
                _lantern = GetComponent<Lantern>();
                if (_lantern == null)
                {
                    Main.Warning($"No Lantern component found on {gameObject.name}, glare tint color will not update");
                }
            }
            catch (System.Exception ex)
            {
                Main.Error($"Failed to apply glare material on {gameObject.name}: {ex}");
            }
        }

        public void Update()
        {
            // Update tint color if the lantern color has changed
            if (_lantern != null && _glareMaterial != null)
            {
                int currentColorIndex = _lantern.GetColorIndex();
                if (currentColorIndex != _lastColorIndex)
                {
                    _lastColorIndex = currentColorIndex;
                    UpdateGlareTintColor();
                }
            }
        }

        private void GrabAndApplyMaterial()
        {
            // Check if glare objects are assigned
            if (glareObjects == null || glareObjects.Length == 0)
            {
                Main.Warning($"No glare objects are assigned on {gameObject.name}");
                return;
            }

            // Find the material from the game's resource pool
            Material? baseMaterial = FindMaterial(MATERIAL_NAME);
            if (baseMaterial == null)
            {
                Main.Error($"Could not find material '{MATERIAL_NAME}' in material pool");
                return;
            }

            // Create an instance of the material so we can modify it without affecting other objects
            _glareMaterial = new Material(baseMaterial);

            // Apply the material to all glare objects
            foreach (var glareObject in glareObjects)
            {
                if (glareObject == null)
                {
                    Main.Warning($"Null glare object found in array on {gameObject.name}");
                    continue;
                }

                // Get the renderer component
                Renderer glareRenderer = glareObject.GetComponent<Renderer>();
                if (glareRenderer == null)
                {
                    Main.Warning($"Could not find Renderer component on glare object '{glareObject.name}'");
                    continue;
                }

                // Apply the material instance to the renderer
                glareRenderer.material = _glareMaterial;
            }

            // Set initial tint color
            UpdateGlareTintColor();
        }

        private void UpdateGlareTintColor()
        {
            if (_glareMaterial == null || _lantern == null)
                return;

            // Get the current color from the lantern
            Color emissionColor = GetLanternEmissionColor();

            // Check if the material has the tint color property
            if (_glareMaterial.HasProperty(TINT_COLOR_PROPERTY))
            {
                // Set the tint color (XYZW format corresponds to RGBA)
                _glareMaterial.SetColor(TINT_COLOR_PROPERTY, emissionColor);
            }
            else
            {
                Main.Warning($"Material '{MATERIAL_NAME}' does not have property '{TINT_COLOR_PROPERTY}'");
            }
        }

        private Color GetLanternEmissionColor()
        {
            if (_lantern == null || _lantern.colorMaterials == null)
                return Color.white;

            int colorIndex = _lantern.GetColorIndex();
            if (colorIndex < 0 || colorIndex >= _lantern.colorMaterials.Length)
                return Color.white;

            Material? currentMaterial = _lantern.colorMaterials[colorIndex];
            if (currentMaterial == null)
                return Color.white;

            // Try to get the emission color from common emission property names
            if (currentMaterial.HasProperty("_EmissionColor"))
            {
                return currentMaterial.GetColor("_EmissionColor");
            }
            else if (currentMaterial.HasProperty("_EmissiveColor"))
            {
                return currentMaterial.GetColor("_EmissiveColor");
            }
            else if (currentMaterial.HasProperty("_Emission"))
            {
                return currentMaterial.GetColor("_Emission");
            }

            // If no emission property found, return white as fallback
            return Color.white;
        }

        private Material? FindMaterial(string materialName)
        {
            var allMaterials = Resources.FindObjectsOfTypeAll<Material>();
            var material = allMaterials.FirstOrDefault(m => m.name == materialName);

            if (material == null)
            {
                return null;
            }
            return material;
        }
    }
}
