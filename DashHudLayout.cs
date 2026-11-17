using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace MeleeExpansion
{
    /// <summary>Screen position the Dash HUD is anchored to.</summary>
    public enum DashHudAnchor
    {
        BottomRight,
        BottomCenter,
        BottomLeft,
        MiddleRight,
        MiddleLeft,
        TopRight,
        TopCenter,
        TopLeft
    }

    /// <summary>Where the Dash HUD panel goes this frame, in screen pixels (GUI space, origin top left).</summary>
    internal struct DashHudPlacement
    {
        /// <summary>Screen pixels per HUD unit. HUD units are pixels on a 1080-pixel-tall screen.</summary>
        public float Scale;

        public Rect Panel;

        /// <summary>
        /// The RESET and +HP labels normally sit above the panel. True when the panel is too close to
        /// the top of the screen for that, so they go below it.
        /// </summary>
        public bool FeedbackBelow;
    }

    /// <summary>
    /// Sizes and places the Dash HUD the way the game sizes and places its own HUD.
    ///
    /// The panel is drawn with IMGUI, in screen pixels, so on its own it ignores everything that sizes
    /// the game's HUD. The game's HUD canvas scales with the screen height (1080 reference) and with the
    /// "HUD size" accessibility setting (<c>CanvasManager.SetCanvasScale</c> changes the scaler's
    /// reference height). The factor that ends up on that canvas already includes both, so the panel
    /// takes it rather than recomputing either. With the setting at its default on a 1080p screen the
    /// factor is 1, which is the size the panel always had.
    ///
    /// The ultrawide HUD padding setting (<c>Display_UltrawideHUDPadding</c>) moves the left and right
    /// edges of the game's HUD area inward by that many canvas units (<c>PlayerHUD</c> sets the HUD
    /// rect's left and right offsets to it), so an edge-anchored panel moves inward with it.
    ///
    /// Everything is read through reflection, like the rest of this plugin. A member that is missing
    /// on some game build falls back to the screen height alone and no padding, and says so once.
    /// </summary>
    internal static class DashHudLayout
    {
        internal const float ReferenceHeight = 1080f;

        /// <summary>Distance from the screen edge, in HUD units. The panel always sat 34 px in.</summary>
        internal const float EdgeMargin = 34f;

        /// <summary>
        /// How far above the panel's top the feedback labels reach, in HUD units: the +HP label is
        /// drawn 42 units above the icon, and the icon starts 10 units below the panel's top.
        /// </summary>
        internal const float FeedbackReachAbove = 32f;

        private const string HudPaddingSettingName = "Display_UltrawideHUDPadding";

        // Settings only change in the options screen, so the reflective reads (which box a float) are
        // done this often rather than on every OnGUI event.
        private const float RefreshInterval = 0.5f;

        private static bool reflectionResolved;

        private static PropertyInfo pCanvasManagerInstance;
        private static FieldInfo fCanvasManagerScaler;

        private static PropertyInfo pAsyncAssetLoadingInstance;
        private static MethodInfo mGetSetting;
        private static PropertyInfo pSettingCurrentValue;

        private static bool scaleSourceWarned;
        private static bool paddingSourceWarned;

        // Set when reading the padding setting threw: the name is missing on this build, so it is not
        // asked for again (each try would throw again).
        private static bool paddingUnavailable;

        private static float nextRefreshTime = float.NegativeInfinity;
        private static Canvas gameHudCanvas;
        private static UnityEngine.Object hudPaddingSetting;
        private static float hudPaddingUnits;

        /// <summary>
        /// Place a panel of <paramref name="panelWidth"/> x <paramref name="panelHeight"/> HUD units.
        /// Offsets are in HUD units: positive X moves right, positive Y moves up.
        /// </summary>
        internal static DashHudPlacement Place(
            float panelWidth,
            float panelHeight,
            DashHudAnchor anchor,
            float offsetX,
            float offsetY
        )
        {
            Refresh();

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            float gameFactor = GameHudScaleFactor();
            float scale = gameFactor;

            // Never bigger than lets the panel and its margins fit. Both this limit and the game's
            // factor grow with the screen height, so it only applies if the game's HUD size setting
            // ever goes several times past its default; it is a guard, not part of the normal layout.
            float fit = Mathf.Min(
                screenWidth / (panelWidth + 2f * EdgeMargin),
                screenHeight / (panelHeight + 2f * EdgeMargin)
            );

            if (fit > 0f && scale > fit)
                scale = fit;

            float width = panelWidth * scale;
            float height = panelHeight * scale;
            float margin = EdgeMargin * scale;

            // The padding is in the game's canvas units, so it takes the game's factor, not the capped one.
            float padding = Mathf.Max(0f, hudPaddingUnits) * gameFactor;

            float left = padding + margin;
            float right = screenWidth - padding - margin - width;
            float centerX = (screenWidth - width) * 0.5f;

            float top = margin;
            float bottom = screenHeight - margin - height;
            float centerY = (screenHeight - height) * 0.5f;

            float x;
            float y;

            switch (anchor)
            {
                case DashHudAnchor.BottomCenter: x = centerX; y = bottom; break;
                case DashHudAnchor.BottomLeft: x = left; y = bottom; break;
                case DashHudAnchor.MiddleRight: x = right; y = centerY; break;
                case DashHudAnchor.MiddleLeft: x = left; y = centerY; break;
                case DashHudAnchor.TopRight: x = right; y = top; break;
                case DashHudAnchor.TopCenter: x = centerX; y = top; break;
                case DashHudAnchor.TopLeft: x = left; y = top; break;
                default: x = right; y = bottom; break;
            }

            // GUI space grows downward; the offset is "up is positive" for whoever edits the config.
            x += offsetX * scale;
            y -= offsetY * scale;

            // Keep the whole panel on screen whatever the offset says.
            x = Mathf.Clamp(x, 0f, Mathf.Max(0f, screenWidth - width));
            y = Mathf.Clamp(y, 0f, Mathf.Max(0f, screenHeight - height));

            // Start on whole pixels so the 1 px borders drawn from here stay crisp at any scale.
            x = Mathf.Floor(x);
            y = Mathf.Floor(y);

            DashHudPlacement placement;
            placement.Scale = scale;
            placement.Panel = new Rect(x, y, width, height);
            placement.FeedbackBelow = y < FeedbackReachAbove * scale;
            return placement;
        }

        /// <summary>The game's HUD canvas factor, or the screen height alone without one.</summary>
        private static float GameHudScaleFactor()
        {
            if (gameHudCanvas != null)
            {
                float factor = gameHudCanvas.scaleFactor;

                if (factor > 0f && !float.IsNaN(factor) && !float.IsInfinity(factor))
                    return factor;
            }

            float fallback = Screen.height / ReferenceHeight;
            return fallback > 0f ? fallback : 1f;
        }

        private static void Refresh()
        {
            float now = Time.unscaledTime;

            if (now < nextRefreshTime)
                return;

            nextRefreshTime = now + RefreshInterval;

            ResolveReflection();
            RefreshGameHudCanvas();
            RefreshHudPadding();
        }

        private static void ResolveReflection()
        {
            if (reflectionResolved)
                return;

            reflectionResolved = true;

            Type canvasManagerType = AccessTools.TypeByName("PerfectRandom.Sulfur.Core.CanvasManager");

            if (canvasManagerType != null)
            {
                pCanvasManagerInstance = AccessTools.Property(canvasManagerType, "Instance");
                fCanvasManagerScaler = AccessTools.Field(canvasManagerType, "canvasScaler");
            }

            Type asyncAssetLoadingType = AccessTools.TypeByName("PerfectRandom.Sulfur.Core.AsyncAssetLoading");
            Type playerSettingType = AccessTools.TypeByName("PerfectRandom.Sulfur.Core.PlayerSetting");

            if (asyncAssetLoadingType != null)
            {
                pAsyncAssetLoadingInstance = AccessTools.Property(asyncAssetLoadingType, "Instance");
                mGetSetting = AccessTools.Method(asyncAssetLoadingType, "GetSetting", new[] { typeof(string) });
            }

            if (playerSettingType != null)
                pSettingCurrentValue = AccessTools.Property(playerSettingType, "currentValue");

            if (pCanvasManagerInstance == null || fCanvasManagerScaler == null)
            {
                WarnScaleSource("CanvasManager.Instance / CanvasManager.canvasScaler not found");
            }

            if (pAsyncAssetLoadingInstance == null || mGetSetting == null || pSettingCurrentValue == null)
            {
                WarnPaddingSource("AsyncAssetLoading.GetSetting / PlayerSetting.currentValue not found");
            }
        }

        private static void RefreshGameHudCanvas()
        {
            if (gameHudCanvas != null)
                return;

            if (pCanvasManagerInstance == null || fCanvasManagerScaler == null)
                return;

            try
            {
                UnityEngine.Object manager = pCanvasManagerInstance.GetValue(null, null) as UnityEngine.Object;

                // CanvasManager belongs to the game scene; before it exists the fallback is used.
                if (manager == null)
                    return;

                CanvasScaler scaler = fCanvasManagerScaler.GetValue(manager) as CanvasScaler;

                if (scaler == null)
                {
                    WarnScaleSource("CanvasManager.canvasScaler is empty");
                    return;
                }

                gameHudCanvas = scaler.GetComponent<Canvas>();

                if (gameHudCanvas == null)
                    WarnScaleSource("CanvasManager.canvasScaler has no Canvas");
            }
            catch (Exception ex)
            {
                WarnScaleSource(ex.GetType().Name + ": " + ex.Message);
            }
        }

        private static void RefreshHudPadding()
        {
            if (paddingUnavailable ||
                pAsyncAssetLoadingInstance == null ||
                mGetSetting == null ||
                pSettingCurrentValue == null)
            {
                hudPaddingUnits = 0f;
                return;
            }

            try
            {
                if (hudPaddingSetting == null)
                {
                    object loader = pAsyncAssetLoadingInstance.GetValue(null, null);

                    if (loader == null || (loader is UnityEngine.Object unityLoader && unityLoader == null))
                    {
                        hudPaddingUnits = 0f;
                        return;
                    }

                    hudPaddingSetting = mGetSetting.Invoke(loader, new object[] { HudPaddingSettingName }) as UnityEngine.Object;

                    if (hudPaddingSetting == null)
                    {
                        hudPaddingUnits = 0f;
                        return;
                    }
                }

                object value = pSettingCurrentValue.GetValue(hudPaddingSetting, null);
                float padding = value is float f ? f : 0f;

                hudPaddingUnits = float.IsNaN(padding) || float.IsInfinity(padding) ? 0f : padding;
            }
            catch (Exception ex)
            {
                // GetSetting throws KeyNotFoundException for a name the build does not have.
                Exception inner = ex is TargetInvocationException && ex.InnerException != null ? ex.InnerException : ex;
                WarnPaddingSource(inner.GetType().Name + ": " + inner.Message);
                paddingUnavailable = true;
                hudPaddingSetting = null;
                hudPaddingUnits = 0f;
            }
        }

        private static void WarnScaleSource(string reason)
        {
            if (scaleSourceWarned)
                return;

            scaleSourceWarned = true;
            Plugin.Log?.LogWarning(
                "Dash HUD: cannot read the game's HUD scale (" + reason + "); sizing by screen height only."
            );
        }

        private static void WarnPaddingSource(string reason)
        {
            if (paddingSourceWarned)
                return;

            paddingSourceWarned = true;
            Plugin.Log?.LogWarning(
                "Dash HUD: cannot read the HUD padding setting (" + reason + "); ignoring it."
            );
        }
    }
}
