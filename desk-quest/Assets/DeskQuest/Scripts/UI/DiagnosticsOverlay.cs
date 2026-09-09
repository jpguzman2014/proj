using System.Text;
using DeskQuest.AR;
using DeskQuest.Core;
using DeskQuest.Detection;
using DeskQuest.Gameplay;
using DeskQuest.Obstacles;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

namespace DeskQuest.UI
{
    /// <summary>
    /// An on-screen readout of what the game thinks is happening, hidden behind a small
    /// button.
    ///
    /// This exists because of how the project is being built: it is written on a machine
    /// with no Unity and tested by someone else on a phone, and the only channel back is
    /// what they can describe. "It did not clear" is almost impossible to act on. "Coverage
    /// sat at 0.4 and the baseline was never established" is a fix.
    ///
    /// It reads from the live components rather than being fed by them, and shows a dash
    /// for anything missing, so it works in every state including the broken ones.
    /// </summary>
    public class DiagnosticsOverlay : MonoBehaviour
    {
        const float k_RefreshInterval = 0.25f;

        Text m_Text;
        GameObject m_Panel;

        ARGameManager m_GameManager;
        GameplayDirector m_Director;
        ARSurfaceManager m_SurfaceManager;
        ARPlaneManager m_PlaneManager;

        readonly StringBuilder m_Builder = new StringBuilder(512);
        float m_NextRefresh;
        float m_SmoothedFrameTime = 1f / 60f;

        /// <summary>Builds the panel and its toggle. Hidden until the player asks for it.</summary>
        public void Build(Transform canvas)
        {
            Image panel = UIFactory.CreatePanel(canvas, "Diagnostics", new Color(0f, 0f, 0f, 0.72f));
            RectTransform rect = panel.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(40f, -390f);
            rect.sizeDelta = new Vector2(660f, 560f);

            m_Text = UIFactory.CreateText(panel.transform, "Readout", string.Empty, 24,
                new Color(0.75f, 1f, 0.85f, 1f), TextAnchor.UpperLeft);
            UIFactory.Stretch(m_Text.rectTransform, 20f);

            m_Panel = panel.gameObject;
            m_Panel.SetActive(false);

            Button toggle = UIFactory.CreateButton(canvas, "DiagnosticsToggle", "i", 32);
            UIFactory.Anchor(toggle.GetComponent<RectTransform>(),
                anchor: new Vector2(0f, 1f),
                pivot: new Vector2(0f, 1f),
                anchoredPosition: new Vector2(44f, -46f),
                size: new Vector2(72f, 72f));
            toggle.onClick.AddListener(Toggle);
        }

        public void Toggle()
        {
            if (m_Panel != null)
                m_Panel.SetActive(!m_Panel.activeSelf);
        }

        void Awake()
        {
            // Found rather than injected: this is a debug tool, and wiring it through the
            // scene builder would spread references for something the game does not need.
            m_GameManager = FindObjectOfType<ARGameManager>();
            m_Director = FindObjectOfType<GameplayDirector>();
            m_SurfaceManager = FindObjectOfType<ARSurfaceManager>();
            m_PlaneManager = FindObjectOfType<ARPlaneManager>();
        }

        void Update()
        {
            // Smoothed so the frame rate reading is legible rather than flickering.
            m_SmoothedFrameTime = Mathf.Lerp(m_SmoothedFrameTime, Time.deltaTime, 0.1f);

            if (m_Panel == null || !m_Panel.activeSelf || Time.time < m_NextRefresh)
                return;

            m_NextRefresh = Time.time + k_RefreshInterval;
            m_Text.text = Compose();
        }

        string Compose()
        {
            m_Builder.Length = 0;

            AppendSession();
            AppendSurfaces();
            AppendCharacter();
            AppendDetection();

            return m_Builder.ToString();
        }

        void AppendSession()
        {
            float fps = m_SmoothedFrameTime > 0.0001f ? 1f / m_SmoothedFrameTime : 0f;

            Line("session", ARSession.state.ToString());
            Line("state", m_GameManager != null ? m_GameManager.State.ToString() : "-");
            Line("fps", fps.ToString("F0"));
            m_Builder.AppendLine();
        }

        void AppendSurfaces()
        {
            int planeCount = 0;
            if (m_PlaneManager != null)
            {
                foreach (ARPlane plane in m_PlaneManager.trackables)
                    planeCount++;
            }

            Line("planes", planeCount.ToString());
            Line("detection on", m_PlaneManager != null ? m_PlaneManager.enabled.ToString() : "-");
            Line("best surface", m_SurfaceManager != null
                ? m_SurfaceManager.BestSurfaceArea.ToString("F3") + " m2"
                : "-");
            m_Builder.AppendLine();
        }

        void AppendCharacter()
        {
            if (m_Director == null || m_Director.Character == null || m_Director.Character.Motor == null)
            {
                Line("character", "not spawned");
                m_Builder.AppendLine();
                return;
            }

            CharacterMotor motor = m_Director.Character.Motor;
            Line("character", motor.State.ToString());
            Line("position", motor.Position.x.ToString("F3") + ", " +
                             motor.Position.y.ToString("F3") + ", " +
                             motor.Position.z.ToString("F3"));
            Line("grounded", motor.IsGrounded.ToString());
            m_Builder.AppendLine();
        }

        void AppendDetection()
        {
            if (m_Director == null || m_Director.Obstacles == null)
            {
                Line("detection", "no level running");
                return;
            }

            EnvironmentChangeDetector detector = m_Director.Detector;

            if (detector == null || !detector.IsAvailable)
            {
                Line("detection", "UNAVAILABLE");
                return;
            }

            Line("watching", detector.WatchedCount + " area(s)");
            Line("waiting", m_Director.SecondsWaiting.ToString("F0") + "s");
            Line("fallback", m_Director.ManualFallbackOffered ? "OFFERED" : "no");

            ObstacleManager obstacles = m_Director.Obstacles;
            for (int i = 0; i < obstacles.Views.Count; i++)
            {
                Obstacle view = obstacles.Views[i];
                if (!view.NeedsRealWorldClearing)
                    continue;

                float coverage = detector.CoverageFor(i);
                string state = view.Cleared
                    ? "cleared"
                    : (detector.HasBaseline(i) ? "armed" : "NO BASELINE");

                Line("  " + view.Label.ToLower(),
                    "cover " + coverage.ToString("F2") + "  " + state);
            }
        }

        void Line(string label, string value)
        {
            m_Builder.Append(label.PadRight(14));
            m_Builder.AppendLine(value);
        }
    }
}
