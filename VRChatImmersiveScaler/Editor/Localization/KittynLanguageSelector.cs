using UnityEditor;
using UnityEngine;

namespace Kittyn.Tools.ImmersiveScaler
{
    public static class KittynLanguageSelector
    {
        private const int MENU_PRIORITY = 500;

        private static void LogDebug(string message)
        {
#if KITTYN_IMMERSIVE_SCALER_DEBUG
            Debug.Log(message);
#endif
        }
        
#if !HAS_COMFI_HIERARCHY && !HAS_ENHANCED_DYNAMICS
        [MenuItem("Tools/⚙️🎨 kittyn.cat 🐟/🌐 Language/English", false, MENU_PRIORITY)]
        private static void SetLanguageEnglish()
        {
            SetLanguage("en", "English");
        }
        
        [MenuItem("Tools/⚙️🎨 kittyn.cat 🐟/🌐 Language/日本語 (Japanese)", false, MENU_PRIORITY + 1)]
        private static void SetLanguageJapanese()
        {
            SetLanguage("ja", "日本語");
        }
        
        [MenuItem("Tools/⚙️🎨 kittyn.cat 🐟/🌐 Language/Español (Spanish)", false, MENU_PRIORITY + 2)]
        private static void SetLanguageSpanish()
        {
            SetLanguage("es", "Español");
        }
        
        [MenuItem("Tools/⚙️🎨 kittyn.cat 🐟/🌐 Language/한국어 (Korean)", false, MENU_PRIORITY + 3)]
        private static void SetLanguageKorean()
        {
            SetLanguage("ko", "한국어");
        }
        
        [MenuItem("Tools/⚙️🎨 kittyn.cat 🐟/🌐 Language/Français (French)", false, MENU_PRIORITY + 4)]
        private static void SetLanguageFrench()
        {
            SetLanguage("fr", "Français");
        }
        
        [MenuItem("Tools/⚙️🎨 kittyn.cat 🐟/🌐 Language/Deutsch (German)", false, MENU_PRIORITY + 5)]
        private static void SetLanguageGerman()
        {
            SetLanguage("de", "Deutsch");
        }
        
        [MenuItem("Tools/⚙️🎨 kittyn.cat 🐟/🌐 Language/Català (Catalan)", false, MENU_PRIORITY + 6)]
        private static void SetLanguageCatalan()
        {
            SetLanguage("ca", "Català");
        }
        
        [MenuItem("Tools/⚙️🎨 kittyn.cat 🐟/🌐 Language/English", true)]
        private static bool ValidateLanguageEnglish()
        {
            return KittynLocalization.CurrentLanguage != "en";
        }
        
        [MenuItem("Tools/⚙️🎨 kittyn.cat 🐟/🌐 Language/日本語 (Japanese)", true)]
        private static bool ValidateLanguageJapanese()
        {
            return KittynLocalization.CurrentLanguage != "ja";
        }
        
        [MenuItem("Tools/⚙️🎨 kittyn.cat 🐟/🌐 Language/Español (Spanish)", true)]
        private static bool ValidateLanguageSpanish()
        {
            return KittynLocalization.CurrentLanguage != "es";
        }
        
        [MenuItem("Tools/⚙️🎨 kittyn.cat 🐟/🌐 Language/한국어 (Korean)", true)]
        private static bool ValidateLanguageKorean()
        {
            return KittynLocalization.CurrentLanguage != "ko";
        }
        
        [MenuItem("Tools/⚙️🎨 kittyn.cat 🐟/🌐 Language/Français (French)", true)]
        private static bool ValidateLanguageFrench()
        {
            return KittynLocalization.CurrentLanguage != "fr";
        }
        
        [MenuItem("Tools/⚙️🎨 kittyn.cat 🐟/🌐 Language/Deutsch (German)", true)]
        private static bool ValidateLanguageGerman()
        {
            return KittynLocalization.CurrentLanguage != "de";
        }
        
        [MenuItem("Tools/⚙️🎨 kittyn.cat 🐟/🌐 Language/Català (Catalan)", true)]
        private static bool ValidateLanguageCatalan()
        {
            return KittynLocalization.CurrentLanguage != "ca";
        }
        
        [MenuItem("Tools/⚙️🎨 kittyn.cat 🐟/🌐 Language/🔧 Reset Language to English", false, MENU_PRIORITY + 10)]
        private static void ResetLanguageToDefault()
        {
            LogDebug("[Immersive Scaler] Force resetting language to English...");
            EditorPrefs.DeleteKey("KittynTools_Language");
            EditorPrefs.DeleteKey("KittynTools_Language_Timestamp");
            KittynLocalization.RefreshLanguages();
            SetLanguage("en", "English");
            LogDebug("[Immersive Scaler] Language reset complete. Language is now: " + KittynLocalization.CurrentLanguage);
        }
#endif
        
        private static void SetLanguage(string languageCode, string languageName)
        {
            LogDebug($"[KittynLanguageSelector] Attempting to set language to: {languageCode} ({languageName})");
            
            var oldLanguage = KittynLocalization.CurrentLanguage;
            KittynLocalization.CurrentLanguage = languageCode;
            var newLanguage = KittynLocalization.CurrentLanguage;
            
            LogDebug($"[KittynLanguageSelector] Language changed from '{oldLanguage}' to '{newLanguage}'");
            
            var message = KittynLocalization.Get("messages.language_changed");
            LogDebug($"[KittynLanguageSelector] Test message: {message}");
            
            EditorApplication.RepaintHierarchyWindow();
            var windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (var window in windows)
            {
                window.Repaint();
            }
            
            if (EditorWindow.focusedWindow != null)
            {
                EditorWindow.focusedWindow.ShowNotification(new GUIContent($"Language: {languageName}"), 3f);
            }
        }
        
        public static void DrawLanguageSelector(string label = null)
        {
            var displayLabel = label ?? KittynLocalization.Get("common.language");
            var currentLanguage = KittynLocalization.CurrentLanguage;
            var languages = KittynLocalization.AvailableLanguages;
            var languageNames = new string[languages.Length];
            var currentIndex = 0;
            
            for (int i = 0; i < languages.Length; i++)
            {
                languageNames[i] = KittynLocalization.GetLanguageDisplayName(languages[i]);
                if (languages[i] == currentLanguage)
                    currentIndex = i;
            }
            
            EditorGUI.BeginChangeCheck();
            var newIndex = EditorGUILayout.Popup(displayLabel, currentIndex, languageNames);
            if (EditorGUI.EndChangeCheck() && newIndex != currentIndex)
            {
                SetLanguage(languages[newIndex], languageNames[newIndex]);
            }
        }
        
        public static string GetCurrentLanguageDisplayName()
        {
            return KittynLocalization.GetLanguageDisplayName(KittynLocalization.CurrentLanguage);
        }
    }
}
