using System;
using DeskQuest.Common;
using UnityEngine;
using UnityEngine.UI;

namespace DeskQuest.UI
{
    /// <summary>
    /// The heads-up display. Milestone 1 needs a title, a status line that tells the
    /// player what the app is waiting for, and a way to start over. The control pad
    /// arrives in a later milestone and attaches to the same canvas.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        Text m_StatusText;
        Text m_HintText;
        Button m_RescanButton;

        /// <summary>Raised when the player asks to pick a different surface.</summary>
        public event Action RescanRequested;

        void Awake()
        {
            Build();
        }

        void Build()
        {
            Canvas canvas = UIFactory.CreateCanvas("DeskQuestCanvas", 0);
            canvas.transform.SetParent(transform, false);

            BuildTopBar(canvas.transform);
            BuildBottomBar(canvas.transform);
        }

        void BuildTopBar(Transform parent)
        {
            Image panel = UIFactory.CreatePanel(parent, "TopBar", Palette.UiPanel);

            // Stretched across the screen width, fixed height, pinned under the top edge.
            // The 80px inset keeps it clear of rounded corners and notches.
            RectTransform panelRect = panel.rectTransform;
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = new Vector2(0f, -40f);
            panelRect.sizeDelta = new Vector2(-80f, 220f);

            Text title = UIFactory.CreateText(panel.transform, "Title", "DESK QUEST", 56, Palette.UiAccent, TextAnchor.UpperCenter);
            UIFactory.Stretch(title.rectTransform);
            title.rectTransform.offsetMin = new Vector2(24f, 24f);
            title.rectTransform.offsetMax = new Vector2(-24f, -20f);

            m_StatusText = UIFactory.CreateText(panel.transform, "Status", "Starting camera...", 40, Palette.UiText, TextAnchor.LowerCenter);
            UIFactory.Stretch(m_StatusText.rectTransform);
            m_StatusText.rectTransform.offsetMin = new Vector2(24f, 20f);
            m_StatusText.rectTransform.offsetMax = new Vector2(-24f, -90f);
        }

        void BuildBottomBar(Transform parent)
        {
            m_HintText = UIFactory.CreateText(parent, "Hint", string.Empty, 34, Palette.UiText, TextAnchor.LowerCenter);
            m_HintText.rectTransform.anchorMin = new Vector2(0f, 0f);
            m_HintText.rectTransform.anchorMax = new Vector2(1f, 0f);
            m_HintText.rectTransform.pivot = new Vector2(0.5f, 0f);
            m_HintText.rectTransform.anchoredPosition = new Vector2(0f, 190f);
            m_HintText.rectTransform.sizeDelta = new Vector2(-80f, 100f);

            m_RescanButton = UIFactory.CreateButton(parent, "RescanButton", "Choose a different desk", 34);
            UIFactory.Anchor(m_RescanButton.GetComponent<RectTransform>(),
                anchor: new Vector2(0.5f, 0f),
                pivot: new Vector2(0.5f, 0f),
                anchoredPosition: new Vector2(0f, 60f),
                size: new Vector2(620f, 110f));

            m_RescanButton.onClick.AddListener(OnRescanClicked);
            m_RescanButton.gameObject.SetActive(false);

            // The camera feed never leaves the phone. Saying so where the player can see
            // it is the honest version of a permission prompt, and this app has no
            // account, no network calls and nothing to upload.
            Text privacy = UIFactory.CreateText(parent, "PrivacyNote",
                "Camera is used only on this device. Nothing is recorded or uploaded.",
                24, new Color(1f, 1f, 1f, 0.72f), TextAnchor.LowerCenter);
            privacy.rectTransform.anchorMin = new Vector2(0f, 0f);
            privacy.rectTransform.anchorMax = new Vector2(1f, 0f);
            privacy.rectTransform.pivot = new Vector2(0.5f, 0f);
            privacy.rectTransform.anchoredPosition = new Vector2(0f, 12f);
            privacy.rectTransform.sizeDelta = new Vector2(-60f, 40f);
        }

        void OnRescanClicked()
        {
            if (RescanRequested != null)
                RescanRequested();
        }

        public void SetStatus(string status)
        {
            if (m_StatusText != null)
                m_StatusText.text = status;
        }

        public void SetHint(string hint)
        {
            if (m_HintText != null)
                m_HintText.text = hint;
        }

        public void SetRescanVisible(bool visible)
        {
            if (m_RescanButton != null)
                m_RescanButton.gameObject.SetActive(visible);
        }
    }
}
