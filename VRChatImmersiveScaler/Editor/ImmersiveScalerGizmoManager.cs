using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace VRChatImmersiveScaler.Editor
{
    /// <summary>
    /// Asset postprocessor to handle initial import
    /// </summary>
    public class ImmersiveScalerImportHandler : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            // Check if our component script was just imported
            foreach (string asset in importedAssets)
            {
                if (asset.Contains("ImmersiveScalerComponent.cs"))
                {
                    ImmersiveScalerGizmoManager.ScheduleDisableViewportIcons();
                    break;
                }
            }
        }
    }
    
    /// <summary>
    /// Manages the automatic disabling of viewport gizmo icons for ImmersiveScaler components.
    /// This allows components to have icons in the inspector header without cluttering the scene view.
    /// </summary>
    [InitializeOnLoad]
    public static class ImmersiveScalerGizmoManager
    {
        private const int MONO_BEHAVIOR_CLASS_ID = 114; // Unity's ClassID for MonoBehaviours
        private static int attemptCount = 0;
        private static readonly int maxAttempts = 50; // Try for about 5 seconds
        
        static ImmersiveScalerGizmoManager()
        {
            ScheduleDisableViewportIcons();
        }
        
        // Cache reflection results for performance
        private static MethodInfo _setIconEnabled;
        private static MethodInfo _getAnnotations;
        private static Type _annotationType;
        private static FieldInfo _classIdField;
        private static FieldInfo _scriptClassField;
        
        private static MethodInfo SetIconEnabled => _setIconEnabled ??= GetMethodInfo("SetIconEnabled");
        private static MethodInfo GetAnnotations => _getAnnotations ??= GetMethodInfo("GetAnnotations");
        
        private static Type AnnotationType => _annotationType ??= typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Annotation");
        private static FieldInfo ClassIdField => _classIdField ??= AnnotationType?.GetField("classID", BindingFlags.Instance | BindingFlags.Public);
        private static FieldInfo ScriptClassField => _scriptClassField ??= AnnotationType?.GetField("scriptClass", BindingFlags.Instance | BindingFlags.Public);
        
        private static MethodInfo GetMethodInfo(string methodName)
        {
            var annotationUtilityType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.AnnotationUtility");
            if (annotationUtilityType == null)
            {
                Debug.LogWarning($"[ImmersiveScaler] Could not find AnnotationUtility type");
                return null;
            }
            
            var method = annotationUtilityType.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);
            if (method == null)
            {
                Debug.LogWarning($"[ImmersiveScaler] Could not find {methodName} method");
            }
            
            return method;
        }
        
        internal static void ScheduleDisableViewportIcons()
        {
            attemptCount = 0;
            EditorApplication.update -= AggressiveDisableGizmos;
            EditorApplication.update += AggressiveDisableGizmos;
        }

        private static bool HasAnnotation(Type type)
        {
            try
            {
                if (GetAnnotations == null || ClassIdField == null || ScriptClassField == null)
                    return false;

                var annotations = (Array)GetAnnotations.Invoke(null, new object[] { });
                if (annotations == null)
                    return false;

                for (int i = 0; i < annotations.Length; i++)
                {
                    var annotation = annotations.GetValue(i);
                    if (annotation == null) continue;

                    var classId = (int)ClassIdField.GetValue(annotation);
                    var scriptClass = (string)ScriptClassField.GetValue(annotation);

                    if (classId == MONO_BEHAVIOR_CLASS_ID && scriptClass == type.Name)
                        return true;
                }
            }
            catch (Exception e)
            {
                LogDebug($"[ImmersiveScaler] Could not inspect gizmo annotations: {e.Message}");
            }

            return false;
        }

        private static bool TrySetGizmoIconEnabled(Type type, bool enabled)
        {
            if (!HasAnnotation(type))
                return false;

#if UNITY_2022_1_OR_NEWER
            try
            {
                UnityEditor.GizmoUtility.SetIconEnabled(type, enabled);
                return true;
            }
            catch (Exception e)
            {
                LogDebug($"[ImmersiveScaler] GizmoUtility failed: {e.Message}, falling back to reflection");
            }
#endif

            if (SetIconEnabled == null)
                return false;

            try
            {
                // SetIconEnabled(int classID, string scriptClass, int enabled)
                SetIconEnabled.Invoke(null, new object[] { MONO_BEHAVIOR_CLASS_ID, type.Name, enabled ? 1 : 0 });
                return true;
            }
            catch (Exception e)
            {
                LogDebug($"[ImmersiveScaler] Failed to set gizmo icon state for {type.Name}: {e.Message}");
                return false;
            }
        }
        
        private static void AggressiveDisableGizmos()
        {
            // Check if we've already succeeded
            if (SessionState.GetBool("ImmersiveScalerIconsDisabled", false))
            {
                EditorApplication.update -= AggressiveDisableGizmos;
                return;
            }
            
            // Try to disable every frame for a while
            attemptCount++;
            
            if (TrySetGizmoIconEnabled(typeof(ImmersiveScalerComponent), false))
            {
                SessionState.SetBool("ImmersiveScalerIconsDisabled", true);
                EditorApplication.update -= AggressiveDisableGizmos;
                LogDebug($"[ImmersiveScaler] Successfully disabled viewport gizmos after {attemptCount} attempts");
            }
            else if (attemptCount >= maxAttempts)
            {
                // Give up after max attempts, but keep the delayed check running
                EditorApplication.update -= AggressiveDisableGizmos;
                EditorApplication.update += DisableImmersiveScalerGizmoIcons;
            }
        }
        
        private static void DisableImmersiveScalerGizmoIcons()
        {
            // Check if we've already disabled the icons this session
            if (SessionState.GetBool("ImmersiveScalerIconsDisabled", false))
            {
                EditorApplication.update -= DisableImmersiveScalerGizmoIcons;
                return;
            }
            
            if (TrySetGizmoIconEnabled(typeof(ImmersiveScalerComponent), false))
            {
                SessionState.SetBool("ImmersiveScalerIconsDisabled", true);
                EditorApplication.update -= DisableImmersiveScalerGizmoIcons;
            }
        }
        
        /// <summary>
        /// Manually re-enable viewport gizmo icons if needed (for debugging)
        /// </summary>
        [MenuItem("Tools/⚙️🎨 kittyn.cat 🐟/Debug/Enable Immersive Scaler Viewport Icons")]
        public static void EnableViewportIcons()
        {
            if (TrySetGizmoIconEnabled(typeof(ImmersiveScalerComponent), true))
            {
                SessionState.SetBool("ImmersiveScalerIconsDisabled", false);
                Debug.Log("[ImmersiveScaler] Viewport gizmo icons enabled");
            }
            else
            {
                Debug.LogWarning("[ImmersiveScaler] Could not find viewport gizmo annotation to enable");
            }
        }
        
        /// <summary>
        /// Manually disable viewport gizmo icons
        /// </summary>
        [MenuItem("Tools/⚙️🎨 kittyn.cat 🐟/Debug/Disable Immersive Scaler Viewport Icons")]
        public static void DisableViewportIcons()
        {
            if (TrySetGizmoIconEnabled(typeof(ImmersiveScalerComponent), false))
            {
                SessionState.SetBool("ImmersiveScalerIconsDisabled", true);
                Debug.Log("[ImmersiveScaler] Viewport gizmo icons disabled");
            }
            else
            {
                Debug.LogWarning("[ImmersiveScaler] Could not find viewport gizmo annotation to disable");
            }
        }

        internal static void LogDebug(string message)
        {
#if KITTYN_IMMERSIVE_SCALER_DEBUG
            Debug.Log(message);
#endif
        }
    }
}
