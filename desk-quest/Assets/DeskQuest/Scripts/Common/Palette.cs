using UnityEngine;

namespace DeskQuest.Common
{
    /// <summary>
    /// One place for the prototype's colours. Values are bright and highly saturated
    /// on purpose: virtual objects have to stay readable on top of a live camera feed
    /// in whatever lighting the player's room happens to have.
    /// </summary>
    public static class Palette
    {
        public static readonly Color ReticleValid = new Color(0.20f, 0.85f, 0.45f, 0.85f);
        public static readonly Color ReticleSearching = new Color(1.00f, 0.78f, 0.20f, 0.65f);
        public static readonly Color DeskMarker = new Color(0.25f, 0.55f, 1.00f, 1.00f);
        public static readonly Color PlayAreaBoundary = new Color(0.20f, 0.85f, 0.45f, 0.90f);

        public static readonly Color UiPanel = new Color(0.05f, 0.06f, 0.10f, 0.72f);
        public static readonly Color UiText = new Color(1.00f, 1.00f, 1.00f, 1.00f);
        public static readonly Color UiAccent = new Color(0.45f, 0.85f, 1.00f, 1.00f);
        public static readonly Color UiButton = new Color(0.12f, 0.16f, 0.24f, 0.85f);
    }
}
