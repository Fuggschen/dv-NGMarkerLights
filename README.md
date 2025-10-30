# NGMarkerLights - Custom Lantern System

A Derail Valley mod that adds interactive lanterns with customizable color shaders that can be mounted on train cars.

## Project Structure

- **NGMarkerLights.Game** - Game logic for lantern functionality
- **NGMarkerLights.Unity** - Unity proxy components for the custom_item_mod framework
- **NGMarkerLights.UnityProject** - Unity project for creating lantern assets and asset bundles

## Features

### Lantern Component

The lantern system provides:
- **Multiple Color Shaders**: Cycle through different colored light shaders (Red, Yellow, Green, Blue)
- **Off State**: No Light shader when not in use
- **State Persistence**: Lantern color state is saved and restored between game sessions
- **Train Car Integration**: Lanterns are attached to train cars via the custom_item_mod framework

## How It Works

### Architecture

1. **LanternProxy** (Unity) - Defines the lantern configuration in Unity:
   - `offMaterial`: Material to use when lantern is off (shader will be extracted from this)
   - `colorMaterials`: Array of materials for different colors (shaders will be extracted from these)
   - `lanternRenderer`: The renderer component with the lantern material
   - `materialIndex`: Which material index to modify on the renderer
   - `interactionCollider`: Optional GameObject with Button component for interaction

2. **Lantern** (Game) - Runtime logic that:
   - Manages color cycling through available shaders
   - Persists state using PlayerPrefs
   - Applies shader changes to the lantern material
   - Validates configuration on startup

3. **Main** (Game) - Registration and initialization:
   - Registers the LanternProxy → Lantern mapping with custom_item_mod
   - Copies proxy fields to the runtime component using reflection

## Setup in Unity

### Creating a Lantern Prefab

1. Create or import your lantern 3D model
2. Create materials for each color state (in `Assets/Shader/`):
   - No Light.mat (off state)
   - Red Light.mat
   - Yellow Light.mat
   - Green Light.mat
   - Blue Light.mat

3. Add the `LanternProxy` component to your lantern GameObject
4. Configure the proxy:
   - **Off Material**: Assign the "No Light" material
   - **Color Materials**: Create an array and assign your color materials (Red Light, Yellow Light, Green Light, Blue Light)
   - **Lantern Renderer**: Assign the MeshRenderer component
   - **Material Index**: Set to 0 (or the index of the material you want to change)
   - **Interaction Collider**: (Optional) Assign a child GameObject with Button component if using separate interaction area

5. Build your asset bundle containing the lantern prefab

### Material Configuration

The materials define the visual appearance of each color state. In the provided example:
- Each material uses textures for base color, emissive glow, normal maps, and mask maps
- The emissive texture provides the glowing light effect
- Different materials create different colored lights (red, yellow, green, blue)
- The game code extracts the shader from each material at runtime and applies it to the lantern

## Usage

### Setting Up Interaction in Unity

To make your lantern interactive, you need to add a Button component in Unity:

1. **Create Interaction Collider (Recommended)**:
   - Create a child GameObject under your lantern for the interaction area (e.g., "InteractionCollider")
   - Add a MeshCollider (or BoxCollider/SphereCollider) component to this child
   - Check "Is Trigger" on the collider
   - Optionally set the layer to "Interactable"

2. **Add Button Component**:
   - Select the interaction collider GameObject you just created
   - Add Component → Search for "Button" (from DV.CabControls.Spec)
   - Configure the Button settings:
     - Leave default settings for most fields
     - The button will be set up properly when the game loads

3. **Link to LanternProxy**:
   - Select your main lantern GameObject (the one with LanternProxy component)
   - In the LanternProxy component, assign the interaction collider GameObject to the `interactionCollider` field
   - If you don't assign an interactionCollider, the Button must be on the same GameObject as LanternProxy

**Important Notes:**
- The Button component must be added in Unity BEFORE building the asset bundle
- The game code will find the Button component and hook up the interaction event
- If the Button component is missing, you'll see an error: "Lantern X has no ButtonBase component on Y! Please add a Button component in Unity."

The game code will automatically:
- Find the ButtonBase component on the specified GameObject
- Hook up the `Used` event to cycle colors
- Apply a 0.5 second cooldown to prevent rapid cycling

### In-Game Interaction

- Walk up to the lantern in-game
- When the interaction prompt appears, press the interact key (default: E)
- The lantern will cycle through colors: Color 0 → Color 1 → ... → Color N → back to Color 0
- There's a 0.5 second cooldown between color changes

### Color Cycle Behavior

- Colors cycle in order: Color 0 → Color 1 → ... → Color N → back to Color 0
- Each cycle updates the lantern material's shader
- State is automatically saved and restored between sessions
- Cooldown of 0.5 seconds prevents rapid cycling

## Building the Project

```bash
dotnet build NGMarkerLights.sln
```

This will:
1. Build NGMarkerLights.Unity.dll (proxy components)
2. Build NGMarkerLights.Game.dll (game logic)
3. Run the PostBuild script to verify DLL locations

## Installation

1. Build the project to generate the DLLs
2. Build your Unity asset bundles containing lantern prefabs
3. Copy the mod files to your Derail Valley mods directory:
   - NGMarkerLights.Game.dll
   - NGMarkerLights.Unity.dll (goes in the Unity Plugins folder)
   - Asset bundles
   - info.json

## Extending the System

### Adding More Colors

1. In Unity, create additional material variants with different shaders/colors
2. Add the new materials to the `colorMaterials` array in the LanternProxy
3. The shaders will be automatically extracted at runtime

### Custom Interaction Methods

The `Lantern.TriggerColorCycle()` method is public and can be called from:
- Custom interaction systems
- Button press handlers
- Proximity triggers
- Other game events

### Advanced Features

Potential extensions:
- Add brightness control
- Implement automatic color changes based on time or events
- Link multiple lanterns to change together
- Add sound effects when cycling colors
- Create a UI for color selection

## Technical Notes

### State Persistence

State is saved using `PlayerPrefs` with a unique key per lantern instance:
```csharp
private string StateKey => $"Lantern_{gameObject.GetInstanceID()}_ColorIndex";
```

### Cooldown System

A cooldown prevents rapid color cycling:
```csharp
private float lastInteractionTime = 0f;
private const float InteractionCooldown = 0.5f; // seconds
```

### Reflection-Based Field Copying

The proxy fields are copied to the runtime component using reflection in `Main.CopyLanternProxyFields()`. This allows the Unity-defined configuration to be transferred to the game logic component.

## Troubleshooting

### Lantern Not Working

Check the logs for error messages:
- "Lantern X is not attached to a train car!" - Component not on a train car GameObject
- "Lantern X has no off material assigned!" - Missing off material in proxy
- "Lantern X has no color materials assigned!" - Empty or null colorMaterials array
- "Lantern X has null material at index Y" - One of the color materials is null
- "Lantern X has no renderer assigned!" - Missing lanternRenderer reference
- "Lantern X material index Y out of range!" - materialIndex exceeds available materials
- "Lantern X has no ButtonBase component on interaction object!" - Missing Button component in Unity

### Interaction Not Working

- Verify Button component is added in Unity (on lantern or interaction collider GameObject)
- Check that the collider is set as a trigger
- Ensure the GameObject is on the "Interactable" layer
- Verify the interactionCollider field is correctly assigned if using a separate collider
- Check that you're close enough to the lantern for the interaction prompt to appear

### Colors Not Changing

- Verify materials are properly assigned in the Unity proxy (not shaders directly)
- Check that the renderer has materials at the specified materialIndex
- Ensure `TriggerColorCycle()` is being called (add debug logging)
- Verify cooldown isn't preventing rapid changes

## License

See LICENSE.txt for license information.
