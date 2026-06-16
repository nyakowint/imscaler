# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

VRChat Immersive Scaler is a Unity Editor tool that scales VRChat avatars to match real-world proportions while maintaining VR immersion. It's implemented as a Unity package using NDMF (Non-Destructive Modular Framework) for build-time processing.

## Architecture

### Core Components

1. **Runtime Component** (`Runtime/ImmersiveScalerComponent.cs`): MonoBehaviour that stores scaling configuration on avatars
2. **Editor Integration** (`Editor/ImmersiveScalerComponentEditor.cs`): Custom Unity inspector UI
3. **NDMF Plugin** (`Editor/ImmersiveScalerPlugin.cs`): Integrates with VRChat's build pipeline, applies scaling during avatar upload
4. **Scaling Core** (`Editor/ImmersiveScalerCore.cs`): Contains the actual scaling algorithms and transforms

### Key Design Patterns

- **Non-Destructive**: All scaling is applied during build time via NDMF, original avatar is preserved
- **Component-Based**: Users add `ImmersiveScalerComponent` to avatars to configure scaling
- **Build Pipeline Integration**: Scaling happens automatically during VRChat avatar upload

## Development Guidelines

### Unity-Specific Considerations

- This is a Unity package, not a standalone application
- No traditional build/test commands - functionality is tested within Unity Editor
- Assembly definitions (`.asmdef` files) separate Editor and Runtime code
- Targets Unity 2022.3.22f1 exclusively for the maintained package

### Dependencies

- `nadena.dev.ndmf`: >=1.13.0 <2.0.0-a - Required for build-time processing
- `com.vrchat.avatars`: >=3.10.3 <3.11.0-a - VRChat SDK for avatar descriptors
- Modular Avatar is optional; the NDMF pass declares ordering after `nadena.dev.modular-avatar` without requiring its runtime assembly.

### Code Organization

- **Editor/** - All build-time and editor-only functionality
  - Must use `#if UNITY_EDITOR` guards when referencing from Runtime
  - Contains UI, scaling logic, and NDMF integration
- **Runtime/** - Only the component that stores configuration
  - Should be minimal - just data storage
  - Cannot reference Editor code

### Important Classes

- `ScalingParameters`: Data class containing all scaling options
- `ImmersiveScalerUtilities`: Static helper methods for transform calculations
- `VRCBoneMapper`: Maps Unity humanoid bones to VRChat bone names

### ViewPosition Handling

The tool automatically adjusts VRChat's ViewPosition after scaling:
1. Reads the descriptor ViewPosition at preview/build start
2. Stores the original avatar root scale before ScaleAvatar runs
3. Applies scaling transformations
4. Computes `scaleRatio = newRootScale.y / originalRootScale.y`
5. Sets `ViewPosition = originalViewPosition * scaleRatio`

This proportional approach is correct because VRChat normalizes the avatar root scale to 1
on upload (baking all bone positions × rootScale into world-scale units). ViewPosition is
NOT automatically scaled during this bake, so it must be scaled proportionally to remain
at the correct eye height in the baked (scale = 1) reference frame.

### Testing Workflow

1. Add component to test avatar in Unity scene
2. Use "Preview Scaling" button to test in editor
3. Check Scene view for visual results
4. Use "Reset Preview" to undo
5. Upload avatar to VRChat for final testing
