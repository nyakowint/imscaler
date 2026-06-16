using nadena.dev.ndmf;
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.Components;

[assembly: ExportsPlugin(typeof(VRChatImmersiveScaler.Editor.ImmersiveScalerPlugin))]

namespace VRChatImmersiveScaler.Editor
{
    public class ImmersiveScalerPlugin : Plugin<ImmersiveScalerPlugin>
    {
        public override string QualifiedName => "com.vrchat.immersivescaler";
        public override string DisplayName => "🐟 Immersive Scaler 📐📏🎨";

        private static void LogDebug(string message)
        {
#if KITTYN_IMMERSIVE_SCALER_DEBUG
            Debug.Log(message);
#endif
        }
        
        protected override void Configure()
        {
            InPhase(BuildPhase.Transforming)
                // If this runs before armature merge tools (e.g. Modular Avatar Merge Armature),
                // those tools may rewrite bindposes to preserve the outfit's *current* appearance and
                // effectively cancel out the scaling we apply to humanoid bones.
                //
                // Running after MA ensures merged/retargeted outfits are already bound to the avatar armature,
                // so they receive the same bone scaling as the base body.
                .AfterPlugin("nadena.dev.modular-avatar")
                .Run("Apply Immersive Scaling", ctx =>
                {
                    var component = ctx.AvatarRootTransform.GetComponentInChildren<ImmersiveScalerComponent>(true);
                    if (component == null) return;
                    
                    // Get VRCAvatarDescriptor
                    var descriptor = ctx.AvatarDescriptor as VRCAvatarDescriptor;
                    if (descriptor == null)
                    {
                        Debug.LogError("ImmersiveScaler: No VRCAvatarDescriptor found!");
                        return;
                    }
                    
                    Vector3 buildStartViewPosition = descriptor.ViewPosition;
                    
                    LogDebug($"ImmersiveScaler: Starting scaling process. Target height: {component.targetHeight}m");
                    
                    // Create scaling core
                    var scalerCore = new ImmersiveScalerCore(ctx.AvatarRootTransform.gameObject);
                    scalerCore.SetMeasurementRendererOverrides(
                        component.useMeasurementRendererOverrides,
                        component.measurementBodyRenderers,
                        component.measurementHeadRenderers
                    );
                    
                    // Store original avatar scale before any modifications
                    Vector3 originalAvatarScale = ctx.AvatarRootTransform.localScale;
                    
                    // Create parameters from component
                    var parameters = new ScalingParameters
                    {
                        targetHeight = component.targetHeight,
                        upperBodyPercentage = component.upperBodyPercentage,
                        customScaleRatio = component.customScaleRatio,
                        armThickness = component.armThickness,
                        legThickness = component.legThickness,
                        thighPercentage = component.thighPercentage,
                        scaleHand = component.scaleHand,
                        scaleFoot = component.scaleFoot,
                        scaleEyes = component.scaleEyes,
                        centerModel = component.centerModel,
                        extraLegLength = component.extraLegLength,
                        scaleRelative = component.scaleRelative,
                        armToLegs = component.armToLegs,
                        keepHeadSize = component.keepHeadSize,
                        skipAdjust = component.skipMainRescale,
                        skipFloor = component.skipMoveToFloor,
                        skipScale = component.skipHeightScaling,
                        useBoneBasedFloorCalculation = component.useBoneBasedFloorCalculation,
                        // Pass measurement method configuration
                        targetHeightMethod = component.targetHeightMethod,
                        armToHeightRatioMethod = component.armToHeightRatioMethod,
                        armToHeightHeightMethod = component.armToHeightHeightMethod,
                        upperBodyUseNeck = component.upperBodyUseNeck,
                        upperBodyTorsoUseNeck = component.upperBodyTorsoUseNeck,
                        upperBodyUseLegacy = component.upperBodyUseLegacy
                    };
                    
                    // Apply scaling
                    scalerCore.ScaleAvatar(parameters);
                    
                    // Calculate the actual scale ratio applied to the avatar root.
                    // VRChat normalizes root scale to 1 on upload (baking bone positions × rootScale).
                    // ViewPosition is NOT automatically scaled during baking, so we must scale it
                    // proportionally with the root scale change to keep it at the correct eye height.
                    Vector3 newAvatarScale = ctx.AvatarRootTransform.localScale;
                    float scaleRatio = newAvatarScale.y / originalAvatarScale.y;
                    
                    if (!component.skipMainRescale || !component.skipHeightScaling)
                    {
                        Vector3 newViewPosition = buildStartViewPosition * scaleRatio;
                        
                        descriptor.ViewPosition = newViewPosition;
                        EditorUtility.SetDirty(descriptor);
                        
                        // Enhanced debug logging
#if KITTYN_IMMERSIVE_SCALER_DEBUG
                        LogDebug($"ImmersiveScaler: ViewPosition Update Details:");
                        LogDebug($"  Original ViewPosition: {buildStartViewPosition}");
                        LogDebug($"  New ViewPosition: {newViewPosition}");
                        LogDebug($"  Scale ratio applied: {scaleRatio}");
#endif
                    }
                    
                    LogDebug($"ImmersiveScaler: Scaling complete. Final height: {scalerCore.GetHighestPoint() - scalerCore.GetLowestPoint(component.useBoneBasedFloorCalculation):F3}m");
                    
                    // Apply additional tools if enabled
                    if (component.applyFingerSpreading)
                    {
                        LogDebug($"ImmersiveScaler: Applying finger spreading with factor {component.fingerSpreadFactor}");
                        ImmersiveScalerFingerUtility.SpreadFingers(ctx.AvatarRootTransform.gameObject, 
                            component.fingerSpreadFactor, component.spareThumb);
                    }
                    
                    if (component.applyShrinkHipBone)
                    {
                        LogDebug("ImmersiveScaler: Applying hip bone fix");
                        ApplyHipBoneFix(ctx.AvatarRootTransform.gameObject);
                    }
                    
                    // Remove component after processing
                    Object.DestroyImmediate(component);
                });
        }
        
        private static void ApplyHipBoneFix(GameObject avatar)
        {
            Animator animator = avatar.GetComponent<Animator>();
            if (animator == null || !animator.isHuman) return;
            
            Transform hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            Transform spine = animator.GetBoneTransform(HumanBodyBones.Spine);
            Transform leftLeg = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            Transform rightLeg = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            
            if (hips == null || spine == null || leftLeg == null || rightLeg == null)
            {
                Debug.LogError("ImmersiveScaler: Cannot find required bones for hip shrinking");
                return;
            }
            
            float legStartY = (leftLeg.position.y + rightLeg.position.y) / 2f;
            float spineStartY = spine.position.y;
            
            // Move hip 90% of the way between legs and spine
            Vector3 newPosition = hips.position;
            newPosition.y = legStartY + (spineStartY - legStartY) * 0.9f;
            newPosition.x = spine.position.x;
            newPosition.z = spine.position.z;
            
            hips.position = newPosition;
        }
    }
}
