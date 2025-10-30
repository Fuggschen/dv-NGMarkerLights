using custom_item_components;
using UnityEngine;

namespace NGMarkerLights.Unity
{
    public class LanternProxy : GadgetBase
    {
        [Header("Material Configuration")]
        [Tooltip("Material to use when lantern is off (no light)")]
        public Material? offMaterial;

        [Tooltip("Array of materials for different colors (e.g., red, yellow, green, blue)")]
        public Material[]? colorMaterials;

        [Header("Rendering")]
        [Tooltip("The renderer component that contains the lantern material")]
        public Renderer? lanternRenderer;

        [Tooltip("Index of the material to modify (usually 0)")]
        public int materialIndex = 0;

        [Header("Interaction")]
        [Tooltip("GameObject with Collider for interaction (should have a MeshCollider or BoxCollider with isTrigger=true)")]
        public GameObject? interactionCollider;

        [Header("Light Source")]
        [Tooltip("The Light component (Source child object) to sync color with material emission")]
        public Light? sourceLight;
    }
}
