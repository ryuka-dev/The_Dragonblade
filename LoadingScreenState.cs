using System;
using System.Reflection;
using HarmonyLib;

namespace MeleeExpansion
{
    /// <summary>
    /// Whether the game's loading screen is up.
    ///
    /// The source is the loading overlay's own state: <c>UIManager.Instance.loadingOverlay.state</c>
    /// is set to Shown by every path that brings the screen up (a level switch, a death, the hub, and
    /// the Endless "Stage X" screen) and back to Hidden when the level starts. The game state is not
    /// enough: an Endless stage transition keeps it at Running for most of the time its overlay is
    /// shown. The overlay is uGUI, and anything drawn with IMGUI is drawn on top of every uGUI canvas,
    /// so the Dash HUD has to stand down itself while it is up.
    ///
    /// Read through reflection like the rest of this plugin. If the members are missing on some game
    /// build this reports "not up", which is how the HUD behaved before, and says so once.
    /// </summary>
    internal static class LoadingScreenState
    {
        private static bool resolved;
        private static bool unavailable;

        private static PropertyInfo pUIManagerInstance;
        private static FieldInfo fLoadingOverlay;
        private static FieldInfo fOverlayState;
        private static object shownValue;

        internal static bool IsShown()
        {
            if (!resolved)
                Resolve();

            if (unavailable)
                return false;

            try
            {
                object manager = pUIManagerInstance.GetValue(null, null);

                // No UIManager yet, or one left over from an unloaded scene.
                if (IsMissing(manager))
                    return false;

                object overlay = fLoadingOverlay.GetValue(manager);

                if (IsMissing(overlay))
                    return false;

                return shownValue.Equals(fOverlayState.GetValue(overlay));
            }
            catch (Exception ex)
            {
                Exception inner = ex is TargetInvocationException && ex.InnerException != null ? ex.InnerException : ex;
                MarkUnavailable(inner.GetType().Name + ": " + inner.Message);
                return false;
            }
        }

        private static bool IsMissing(object value)
        {
            return value == null || (value is UnityEngine.Object unityObject && unityObject == null);
        }

        private static void Resolve()
        {
            resolved = true;

            Type uiManagerType = AccessTools.TypeByName("PerfectRandom.Sulfur.Core.UI.UIManager");
            Type overlayType = AccessTools.TypeByName("PerfectRandom.Sulfur.Core.UI.LoadingOverlay");
            Type uiStateType = AccessTools.TypeByName("PerfectRandom.Sulfur.Core.UI.UIState");

            if (uiManagerType == null || overlayType == null || uiStateType == null || !uiStateType.IsEnum)
            {
                MarkUnavailable("UIManager / LoadingOverlay / UIState type not found");
                return;
            }

            pUIManagerInstance = AccessTools.Property(uiManagerType, "Instance");
            fLoadingOverlay = AccessTools.Field(uiManagerType, "loadingOverlay");
            fOverlayState = AccessTools.Field(overlayType, "state");

            if (pUIManagerInstance == null ||
                fLoadingOverlay == null ||
                fOverlayState == null ||
                fOverlayState.FieldType != uiStateType)
            {
                MarkUnavailable("UIManager.Instance / UIManager.loadingOverlay / LoadingOverlay.state not found");
                return;
            }

            try
            {
                shownValue = Enum.Parse(uiStateType, "Shown");
            }
            catch (ArgumentException)
            {
                MarkUnavailable("UIState.Shown not found");
            }
        }

        private static void MarkUnavailable(string reason)
        {
            unavailable = true;
            Plugin.Log?.LogWarning(
                "Dash HUD: cannot read the loading screen state (" + reason + "); the HUD is not hidden while loading."
            );
        }
    }
}
