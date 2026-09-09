using System;
using DeskQuest.Common;
using UnityEngine;
using UnityEngine.UI;

namespace DeskQuest.UI
{
    /// <summary>
    /// The heads-up display: a title and status at the top, an objective line under it,
    /// the controls at the bottom during play, and a completion banner at the end.
    ///
    /// The layout keeps the middle of the screen empty, because the middle of the screen
    /// is the desk. Controls sit in the bottom third where thumbs are, and text sits above
    /// the action rather than over it.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        Text m_StatusText;
        Text m_ObjectiveText;
        Image m_ObjectivePanel;
        Button m_RescanButton;
        GameObject m_CompletionPanel;
        Button m_PlayAgainButton;
        ControlPad m_ControlPad;
        DiagnosticsOverlay m_Diagnostics;

        /// <summary>Raised when the player asks to pick a different surface.</summary>
        public event Action RescanRequested;

        /// <summary>Raised when the player asks to run the level again.</summary>
        public event Action PlayAgainRequested;

        /// <summary>The on-screen controls, which the character reads its input from.</summary>
        public ControlPad ControlPad { get { return m_ControlPad; } }

        void Awake()
        {
            Canvas canvas = UIFactory.CreateCanvas("DeskQuestCanvas", 0);
            canvas.transform.SetParent(transform, false);

            BuildTopBar(canvas.transform);
            BuildObjective(canvas.transform);
            BuildControls(canvas.transform);
            BuildBottomBar(canvas.transform);
            BuildCompletion(canvas.transform);
            BuildDiagnostics(canvas.transform);
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
            panelRect.sizeDelta = new Vector2(-80f, 200f);

            Text title = UIFactory.CreateText(panel.transform, "Title", "DESK QUEST", 54, Palette.UiAccent, TextAnchor.UpperCenter);
            UIFactory.Stretch(title.rectTransform);
            title.rectTransform.offsetMin = new Vector2(24f, 24f);
            title.rectTransform.offsetMax = new Vector2(-24f, -18f);

            m_StatusText = UIFactory.CreateText(panel.transform, "Status", "Starting camera...", 38, Palette.UiText, TextAnchor.LowerCenter);
            UIFactory.Stretch(m_StatusText.rectTransform);
            m_StatusText.rectTransform.offsetMin = new Vector2(24f, 18f);
            m_StatusText.rectTransform.offsetMax = new Vector2(-24f, -84f);
        }

        void BuildObjective(Transform parent)
        {
            m_ObjectivePanel = UIFactory.CreatePanel(parent, "ObjectiveBar", new Color(0.05f, 0.06f, 0.10f, 0.62f));

            RectTransform rect = m_ObjectivePanel.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -258f);
            rect.sizeDelta = new Vector2(-120f, 110f);

            m_ObjectiveText = UIFactory.CreateText(m_ObjectivePanel.transform, "Objective", string.Empty,
                34, Palette.UiText, TextAnchor.MiddleCenter);
            UIFactory.Stretch(m_ObjectiveText.rectTransform, 18f);

            m_ObjectivePanel.gameObject.SetActive(false);
        }

        void BuildControls(Transform parent)
        {
            GameObject controls = new GameObject("Controls");
            controls.transform.SetParent(parent, false);

            m_ControlPad = controls.AddComponent<ControlPad>();
            m_ControlPad.Build(controls.transform);
            m_ControlPad.SetVisible(false);
        }

        void BuildBottomBar(Transform parent)
        {
            m_RescanButton = UIFactory.CreateButton(parent, "RescanButton", "Choose a different desk", 32);
            UIFactory.Anchor(m_RescanButton.GetComponent<RectTransform>(),
                anchor: new Vector2(0.5f, 0f),
                pivot: new Vector2(0.5f, 0f),
                anchoredPosition: new Vector2(0f, 70f),
                size: new Vector2(620f, 108f));

            m_RescanButton.onClick.AddListener(OnRescanClicked);
            m_RescanButton.gameObject.SetActive(false);

            // The camera feed never leaves the phone. Saying so where the player can see
            // it is the honest version of a permission prompt, and this app has no
            // account, no network calls and nothing to upload.
            Text privacy = UIFactory.CreateText(parent, "PrivacyNote",
                "Camera is used only on this device. Nothing is recorded or uploaded.",
                22, new Color(1f, 1f, 1f, 0.7f), TextAnchor.LowerCenter);
            privacy.rectTransform.anchorMin = new Vector2(0f, 0f);
            privacy.rectTransform.anchorMax = new Vector2(1f, 0f);
            privacy.rectTransform.pivot = new Vector2(0.5f, 0f);
            privacy.rectTransform.anchoredPosition = new Vector2(0f, 10f);
            privacy.rectTransform.sizeDelta = new Vector2(-60f, 36f);
        }

        void BuildCompletion(Transform parent)
        {
            Image panel = UIFactory.CreatePanel(parent, "CompletionPanel", new Color(0.03f, 0.05f, 0.09f, 0.82f));
            UIFactory.Stretch(panel.rectTransform);

            Text headline = UIFactory.CreateText(panel.transform, "Headline", "DESK QUEST\nCOMPLETE!",
                84, new Color(1f, 0.85f, 0.30f, 1f), TextAnchor.MiddleCenter);
            UIFactory.Stretch(headline.rectTransform);
            headline.rectTransform.offsetMin = new Vector2(60f, 700f);
            headline.rectTransform.offsetMax = new Vector2(-60f, -500f);

            Text subhead = UIFactory.CreateText(panel.transform, "Subhead",
                "You cleared the desk and got the treasure.",
                36, Palette.UiText, TextAnchor.UpperCenter);
            UIFactory.Stretch(subhead.rectTransform);
            subhead.rectTransform.offsetMin = new Vector2(60f, 560f);
            subhead.rectTransform.offsetMax = new Vector2(-60f, -1120f);

            m_PlayAgainButton = UIFactory.CreateButton(panel.transform, "PlayAgainButton", "Play again", 36);
            UIFactory.Anchor(m_PlayAgainButton.GetComponent<RectTransform>(),
                anchor: new Vector2(0.5f, 0.5f),
                pivot: new Vector2(0.5f, 0.5f),
                anchoredPosition: new Vector2(0f, -160f),
                size: new Vector2(520f, 120f));
            m_PlayAgainButton.onClick.AddListener(OnPlayAgainClicked);

            m_CompletionPanel = panel.gameObject;
            m_CompletionPanel.SetActive(false);
        }

        void BuildDiagnostics(Transform parent)
        {
            GameObject diagnostics = new GameObject("Diagnostics");
            diagnostics.transform.SetParent(parent, false);

            m_Diagnostics = diagnostics.AddComponent<DiagnosticsOverlay>();
            m_Diagnostics.Build(diagnostics.transform);
        }

        void OnRescanClicked()
        {
            if (RescanRequested != null)
                RescanRequested();
        }

        void OnPlayAgainClicked()
        {
            if (PlayAgainRequested != null)
                PlayAgainRequested();
        }

        public void SetStatus(string status)
        {
            if (m_StatusText != null)
                m_StatusText.text = status;
        }

        /// <summary>Sets the objective line, hiding the bar entirely when there is nothing to say.</summary>
        public void SetObjective(string objective)
        {
            if (m_ObjectiveText == null || m_ObjectivePanel == null)
                return;

            bool hasText = !string.IsNullOrEmpty(objective);
            m_ObjectiveText.text = objective;
            m_ObjectivePanel.gameObject.SetActive(hasText);
        }

        public void SetRescanVisible(bool visible)
        {
            if (m_RescanButton != null)
                m_RescanButton.gameObject.SetActive(visible);
        }

        public void SetControlsVisible(bool visible)
        {
            if (m_ControlPad != null)
                m_ControlPad.SetVisible(visible);
        }

        public void SetCompletionVisible(bool visible)
        {
            if (m_CompletionPanel != null)
                m_CompletionPanel.SetActive(visible);
        }
    }
}
