using DeskQuest.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace DeskQuest.UI
{
    /// <summary>
    /// Builds the interface from code. The prototype has no UI prefabs, sprites or font
    /// assets on purpose: every widget here is an untextured Image or a Text, which keeps
    /// the whole interface to a handful of draw calls and leaves nothing to re-import when
    /// the project is opened on another machine.
    /// </summary>
    public static class UIFactory
    {
        /// <summary>Reference resolution for the canvas scaler, in portrait.</summary>
        public static readonly Vector2 ReferenceResolution = new Vector2(1080f, 1920f);

        static Font s_Font;

        /// <summary>
        /// Unity renamed its built-in font from Arial.ttf to LegacyRuntime.ttf in 2022.
        /// Try both, then fall back to an OS font so text never silently disappears.
        /// </summary>
        public static Font DefaultFont
        {
            get
            {
                if (s_Font != null)
                    return s_Font;

                s_Font = LoadBuiltinFont("LegacyRuntime.ttf");

                if (s_Font == null)
                    s_Font = LoadBuiltinFont("Arial.ttf");

                if (s_Font == null)
                    s_Font = Font.CreateDynamicFontFromOSFont("Arial", 32);

                if (s_Font == null)
                    Debug.LogError("[DeskQuest] No font available; UI text will not render.");

                return s_Font;
            }
        }

        static Font LoadBuiltinFont(string name)
        {
            try
            {
                return Resources.GetBuiltinResource<Font>(name);
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        public static Canvas CreateCanvas(string name, int sortingOrder)
        {
            GameObject canvasObject = new GameObject(name);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            // Match on both axes so the controls stay the same physical size whether the
            // phone is tall and narrow or short and wide.
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            EnsureEventSystem();
            return canvas;
        }

        /// <summary>
        /// Without an EventSystem carrying an input module that matches the project's
        /// input backend, buttons render but never respond to a touch.
        /// </summary>
        public static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null)
                return;

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();

#if ENABLE_INPUT_SYSTEM
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
#else
            eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
        }

        public static RectTransform CreateRect(Transform parent, string name)
        {
            GameObject element = new GameObject(name, typeof(RectTransform));
            RectTransform rect = element.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        public static Image CreatePanel(Transform parent, string name, Color color)
        {
            RectTransform rect = CreateRect(parent, name);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            // Nothing behind a decorative panel should be blocked from receiving taps.
            image.raycastTarget = false;
            return image;
        }

        public static Text CreateText(Transform parent, string name, string content, int fontSize, Color color, TextAnchor alignment)
        {
            RectTransform rect = CreateRect(parent, name);

            Text text = rect.gameObject.AddComponent<Text>();
            text.font = DefaultFont;
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            return text;
        }

        public static Button CreateButton(Transform parent, string name, string label, int fontSize)
        {
            RectTransform rect = CreateRect(parent, name);

            Image background = rect.gameObject.AddComponent<Image>();
            background.color = Palette.UiButton;

            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = background;

            Text text = CreateText(rect, "Label", label, fontSize, Palette.UiText, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);

            return button;
        }

        /// <summary>Anchors a rect to fill its parent, with optional padding in reference pixels.</summary>
        public static void Stretch(RectTransform rect, float padding = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(padding, padding);
            rect.offsetMax = new Vector2(-padding, -padding);
        }

        /// <summary>
        /// Anchors a rect against one corner of its parent.
        /// <paramref name="anchor"/> is the normalised corner, (0,0) bottom-left to (1,1) top-right.
        /// </summary>
        public static void Anchor(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 anchoredPosition, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
        }
    }
}
