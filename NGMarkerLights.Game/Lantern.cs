using DV.CabControls;
using DV.CabControls.Spec;
using DV.Customization.Gadgets;
using UnityEngine;

namespace NGMarkerLights.Game
{
    public class Lantern : GadgetBase
    {
        // Configuration from proxy
        public Material? offMaterial;
        public Material[]? colorMaterials;
        public Renderer? lanternRenderer;
        public int materialIndex = 0;
        public GameObject? interactionCollider;
        public Light? sourceLight;

        // Runtime state
        private int _currentColorIndex = 0;
        private float _lastInteractionTime = 0f;
        private const float InteractionCooldown = 0.1f;

        public const InteractionInfoType SwitchColor = (InteractionInfoType)10000;

        // Public method for SaveGameManagerPatches to access the current color index
        public int GetColorIndex()
        {
            return _currentColorIndex;
        }

        public string GetUniqueKey()
        {
            // Use the GadgetBase UID
            return UID.ToString();
        }

        public void Start()
        {
            // Load persisted state
            LoadState();

            // Set initial material
            UpdateMaterial();

            // Set up interaction button event
            SetupButton();
        }

        private void SetupButton()
        {
            // Create Button component at runtime on the interaction collider
            GameObject targetObject = interactionCollider != null ? interactionCollider : gameObject;

            // Ensure the target has a collider
            var collider = targetObject.GetComponent<Collider>();

            // Ensure collider is a trigger
            collider.isTrigger = true;

            // Set layer to Interactable
            targetObject.layer = LayerMask.NameToLayer("Interactable");

            // Deactivate before adding components to defer Awake() until configuration is complete
            targetObject.SetActive(false);

            // Create Button component at runtime
            var buttonSpec = targetObject.AddComponent<Button>();
            buttonSpec.createRigidbody = false;
            buttonSpec.useJoints = false;
            buttonSpec.colliderGameObjects = new GameObject[] { targetObject };

            // Add InfoArea for interaction prompt
            var infoArea = targetObject.AddComponent<InfoArea>();
            infoArea.infoType = SwitchColor;

            // Reactivate to trigger Awake() with all components configured
            targetObject.SetActive(true);

            // Hook up the Used event after the GameObject is activated
            var buttonBase = targetObject.GetComponent<ButtonBase>();
            if (buttonBase != null)
            {
                buttonBase.Used += OnButtonPressed;
            }
        }

        private void OnButtonPressed()
        {
            // Check cooldown to prevent rapid cycling
            if (Time.time - _lastInteractionTime < InteractionCooldown)
            {
                return;
            }

            _lastInteractionTime = Time.time;
            CycleColor();
        }

        private void CycleColor()
        {
            if (colorMaterials == null || colorMaterials.Length == 0)
                return;

            // Cycle to next color
            _currentColorIndex = (_currentColorIndex + 1) % colorMaterials.Length;

            // Apply the new material
            UpdateMaterial();

            // Save state
            SaveState();
        }

        private void UpdateMaterial()
        {
            if (lanternRenderer == null || colorMaterials == null)
                return;

            if (_currentColorIndex < 0 || _currentColorIndex >= colorMaterials.Length) return;

            // Get the materials array, replace the material at the index, and set it back
            Material[] materials = lanternRenderer.materials;

            if (materialIndex >= materials.Length) return;

            materials[materialIndex] = colorMaterials[_currentColorIndex];
            lanternRenderer.materials = materials;

            // Update the source light color to match the material's emission color
            UpdateLightColor();
        }

        private void UpdateLightColor()
        {
            if (sourceLight == null || colorMaterials == null)
                return;

            if (_currentColorIndex < 0 || _currentColorIndex >= colorMaterials.Length)
                return;

            Material currentMaterial = colorMaterials[_currentColorIndex];
            if (currentMaterial == null)
                return;

            // Extract emission color from the material
            Color emissionColor = GetEmissionColor(currentMaterial);

            // Set the light color to match the emission color
            sourceLight.color = emissionColor;
        }

        private Color GetEmissionColor(Material material)
        {
            // Try to get the emission color from common emission property names
            if (material.HasProperty("_EmissionColor"))
            {
                return material.GetColor("_EmissionColor");
            }
            else if (material.HasProperty("_EmissiveColor"))
            {
                return material.GetColor("_EmissiveColor");
            }
            else if (material.HasProperty("_Emission"))
            {
                return material.GetColor("_Emission");
            }

            // If no emission property found, return white as fallback
            return Color.white;
        }

        private void LoadState()
        {
            string uniqueKey = GetUniqueKey();

            // Load the persisted color index from the static dictionary
            if (!Main.LanternColorData.TryGetValue(uniqueKey, out int savedColorIndex)) return;
            _currentColorIndex = savedColorIndex;

            // Validate the loaded index
            if (colorMaterials != null && _currentColorIndex >= colorMaterials.Length)
            {
                _currentColorIndex = 0;
            }
        }

        private void SaveState()
        {
            string uniqueKey = GetUniqueKey();

            // Update the static dictionary with the current color index
            Main.LanternColorData[uniqueKey] = _currentColorIndex;
        }
    }
}
