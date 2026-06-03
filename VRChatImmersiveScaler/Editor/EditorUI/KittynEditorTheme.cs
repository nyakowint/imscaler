using UnityEngine;

namespace VRChatImmersiveScaler.Editor.EditorUI
{
    /// <summary>
    /// Consistent color theme and styling for kittyn.cat tools
    /// </summary>
    public static class KittynEditorTheme
    {
        // Primary Colors
        public static readonly Color PrimaryColor = new Color(0.01f, 0.2f, 0.2f);
        public static readonly Color AccentColor = new Color(0.2f, 0.8f, 0.8f);
        public static readonly Color SecondaryColor = new Color(0.1f, 0.3f, 0.3f);
        
        // Status Colors
        public static readonly Color SuccessColor = new Color(0.2f, 0.8f, 0.2f);
        public static readonly Color WarningColor = new Color(0.8f, 0.6f, 0.2f);
        public static readonly Color ErrorColor = new Color(0.8f, 0.2f, 0.2f);
        public static readonly Color InfoColor = new Color(0.2f, 0.5f, 0.8f);
        
        // UI Element Colors
        public static readonly Color HeaderBackgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.5f);
        public static readonly Color SectionBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.2f);
        public static readonly Color SelectedButtonColor = SuccessColor;
        public static readonly Color HoverColor = new Color(0.3f, 0.5f, 0.7f);
        
        // Text Colors
        public static readonly Color NormalTextColor = new Color(0.9f, 0.9f, 0.9f);
        public static readonly Color DisabledTextColor = new Color(0.5f, 0.5f, 0.5f);
        public static readonly Color HighlightTextColor = AccentColor;
        
        // Measurement Visualization Colors
        public static readonly Color MeasurementLineColor = new Color(1f, 1f, 0f, 0.8f);
        public static readonly Color MeasurementPointColor = new Color(1f, 0.5f, 0f, 1f);
        public static readonly Color PreviewColor = new Color(0f, 1f, 1f, 0.5f);
        
        // Icon Background Colors
        public static readonly Color IconBackgroundColor = PrimaryColor;
        public static readonly Color IconBackgroundHoverColor = SecondaryColor;
        
        /// <summary>
        /// Get a color with adjusted alpha
        /// </summary>
        public static Color WithAlpha(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
        
        /// <summary>
        /// Lerp between normal and hover colors based on hover state
        /// </summary>
        public static Color GetHoverColor(Color normalColor, bool isHovered, float hoverIntensity = 0.3f)
        {
            return isHovered ? Color.Lerp(normalColor, HoverColor, hoverIntensity) : normalColor;
        }
        
        /// <summary>
        /// Get appropriate text color based on background
        /// </summary>
        public static Color GetContrastingTextColor(Color backgroundColor)
        {
            // Simple luminance calculation
            float luminance = 0.299f * backgroundColor.r + 0.587f * backgroundColor.g + 0.114f * backgroundColor.b;
            return luminance > 0.5f ? Color.black : Color.white;
        }
        
        /// <summary>
        /// Apply theme colors to GUI temporarily
        /// </summary>
        public class ThemeScope : System.IDisposable
        {
            private readonly Color _originalBackgroundColor;
            private readonly Color _originalContentColor;
            private readonly Color _originalColor;
            
            public ThemeScope(Color? backgroundColor = null, Color? contentColor = null, Color? guiColor = null)
            {
                _originalBackgroundColor = GUI.backgroundColor;
                _originalContentColor = GUI.contentColor;
                _originalColor = GUI.color;
                
                if (backgroundColor.HasValue)
                    GUI.backgroundColor = backgroundColor.Value;
                if (contentColor.HasValue)
                    GUI.contentColor = contentColor.Value;
                if (guiColor.HasValue)
                    GUI.color = guiColor.Value;
            }
            
            public void Dispose()
            {
                GUI.backgroundColor = _originalBackgroundColor;
                GUI.contentColor = _originalContentColor;
                GUI.color = _originalColor;
            }
        }
    }
}