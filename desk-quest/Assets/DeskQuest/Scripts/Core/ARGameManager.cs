using DeskQuest.AR;
using DeskQuest.InputHandling;
using DeskQuest.UI;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace DeskQuest.Core
{
    /// <summary>
    /// Drives the whole flow and owns the single source of truth for game state: start the
    /// session, scan for a desk, let the player tap it, run the level, finish.
    ///
    /// Other components report facts. This one decides what those facts mean and what the
    /// player is told, so there is exactly one place to look when the game says something
    /// unexpected.
    /// </summary>
    public class ARGameManager : MonoBehaviour
    {
        [SerializeField] ARSurfaceManager m_SurfaceManager;
        [SerializeField] ARDeskPlacement m_Placement;
        [SerializeField] UIManager m_UI;
        [SerializeField] GameplayDirector m_Director;
        [SerializeField] ARPlaneManager m_PlaneManager;
        [SerializeField] Camera m_ARCamera;

        GameState m_State = GameState.Initializing;
        bool m_LevelComplete;

        public GameState State { get { return m_State; } }

        void Awake()
        {
            if (m_SurfaceManager == null)
                m_SurfaceManager = FindObjectOfType<ARSurfaceManager>();

            if (m_Placement == null)
                m_Placement = FindObjectOfType<ARDeskPlacement>();

            if (m_UI == null)
                m_UI = FindObjectOfType<UIManager>();

            if (m_Director == null)
                m_Director = FindObjectOfType<GameplayDirector>();

            if (m_PlaneManager == null)
                m_PlaneManager = FindObjectOfType<ARPlaneManager>();

            if (m_ARCamera == null)
                m_ARCamera = Camera.main;
        }

        void OnEnable()
        {
            if (m_UI != null)
            {
                m_UI.RescanRequested += OnRescanRequested;
                m_UI.PlayAgainRequested += OnPlayAgainRequested;
            }

            if (m_Placement != null)
                m_Placement.Placed += OnPlaced;

            if (m_Director != null)
            {
                m_Director.LevelCompleted += OnLevelCompleted;
                m_Director.ObjectiveChanged += OnObjectiveChanged;
            }
        }

        void OnDisable()
        {
            if (m_UI != null)
            {
                m_UI.RescanRequested -= OnRescanRequested;
                m_UI.PlayAgainRequested -= OnPlayAgainRequested;
            }

            if (m_Placement != null)
                m_Placement.Placed -= OnPlaced;

            if (m_Director != null)
            {
                m_Director.LevelCompleted -= OnLevelCompleted;
                m_Director.ObjectiveChanged -= OnObjectiveChanged;
            }
        }

        void Update()
        {
            SetState(EvaluateState());

            if (m_State == GameState.Playing)
                HandleWorldTap();
        }

        GameState EvaluateState()
        {
            if (ARSession.state == ARSessionState.Unsupported)
                return GameState.Unsupported;

            if (m_LevelComplete)
                return GameState.Complete;

            // Checked before session state on purpose. Tracking dips in and out as the
            // player moves the phone, and an anchored play area should not send the flow
            // back to "starting camera" every time it does.
            if (m_Placement != null && m_Placement.HasPlacement)
                return m_Director != null && m_Director.IsRunning ? GameState.Playing : GameState.Placed;

            // Anything below SessionTracking means the session is still starting or the
            // user has not granted the camera yet.
            if (ARSession.state < ARSessionState.SessionTracking)
                return GameState.Initializing;

            if (m_SurfaceManager != null && m_SurfaceManager.HasSuitableSurface)
                return GameState.SurfaceFound;

            return GameState.Scanning;
        }

        /// <summary>
        /// Passes a tap on the world to the level, for the manual fallback when change
        /// detection has not fired. Taps that land on the controls are filtered out before
        /// they get here.
        /// </summary>
        void HandleWorldTap()
        {
            if (m_Director == null || !m_Director.ManualFallbackOffered)
                return;

            Vector2 tapPosition;
            if (TouchInput.TryGetTapThisFrame(out tapPosition))
                m_Director.TryHandleWorldTap(tapPosition);
        }

        void SetState(GameState next)
        {
            if (next == m_State)
                return;

            m_State = next;
            ApplyState();
        }

        void ApplyState()
        {
            switch (m_State)
            {
                case GameState.Unsupported:
                    Configure(placement: false, controls: false, rescan: false, completion: false);
                    Report("AR is not available on this device.",
                        "Desk Quest needs a phone with ARCore or ARKit support.");
                    break;

                case GameState.Initializing:
                    Configure(placement: false, controls: false, rescan: false, completion: false);
                    Report("Starting camera...", "Allow camera access if you are asked.");
                    break;

                case GameState.Scanning:
                    Configure(placement: false, controls: false, rescan: false, completion: false);
                    Report("Point your camera at your desk.",
                        "Move the phone slowly across the surface.");
                    break;

                case GameState.SurfaceFound:
                    Configure(placement: true, controls: false, rescan: false, completion: false);
                    Report("Desk found. Tap it to start.",
                        "Aim the green ring where you want the play area.");
                    break;

                case GameState.Placed:
                    Configure(placement: false, controls: false, rescan: true, completion: false);
                    Report("Play area locked to your desk.", string.Empty);
                    break;

                case GameState.Playing:
                    Configure(placement: false, controls: true, rescan: false, completion: false);
                    Report("Clear the path!",
                        m_Director != null ? m_Director.CurrentObjective : string.Empty);
                    break;

                case GameState.Complete:
                    Configure(placement: false, controls: false, rescan: true, completion: true);
                    Report("Desk Quest complete!", string.Empty);
                    break;
            }
        }

        void Configure(bool placement, bool controls, bool rescan, bool completion)
        {
            if (m_Placement != null)
                m_Placement.SetPlacementEnabled(placement);

            if (m_UI == null)
                return;

            m_UI.SetControlsVisible(controls);
            m_UI.SetRescanVisible(rescan);
            m_UI.SetCompletionVisible(completion);
        }

        void Report(string status, string objective)
        {
            SetStatus(status);

            if (m_UI != null)
                m_UI.SetObjective(objective);
        }

        void SetStatus(string status)
        {
            if (m_UI != null)
                m_UI.SetStatus(status);
        }

        void OnObjectiveChanged(string objective)
        {
            if (m_UI != null && m_State == GameState.Playing)
                m_UI.SetObjective(objective);
        }

        void OnPlaced(DeskPlayArea playArea)
        {
            if (m_SurfaceManager != null)
                m_SurfaceManager.EnterPlayMode();

            StartLevel(playArea);
        }

        void StartLevel(DeskPlayArea playArea)
        {
            m_LevelComplete = false;

            if (m_Director != null)
                m_Director.StartLevel(playArea, m_UI != null ? m_UI.ControlPad : null,
                    m_PlaneManager, m_ARCamera);
        }

        void OnLevelCompleted()
        {
            m_LevelComplete = true;
        }

        void OnPlayAgainRequested()
        {
            if (m_Placement == null || !m_Placement.HasPlacement)
                return;

            StartLevel(m_Placement.PlayArea);
        }

        void OnRescanRequested()
        {
            if (m_Director != null)
                m_Director.StopLevel();

            m_LevelComplete = false;

            if (m_Placement != null)
                m_Placement.Clear();

            if (m_SurfaceManager != null)
                m_SurfaceManager.ResumeDetection();

            SetState(GameState.Scanning);
        }
    }
}
