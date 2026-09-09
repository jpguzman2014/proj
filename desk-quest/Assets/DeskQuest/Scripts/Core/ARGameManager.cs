using DeskQuest.AR;
using DeskQuest.UI;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace DeskQuest.Core
{
    /// <summary>
    /// Drives the Milestone 1 flow and owns the single source of truth for game state:
    /// start the session, scan for a desk, let the player tap it, keep the play area
    /// anchored. Every other component reports facts; this one decides what they mean
    /// and what the player is told.
    /// </summary>
    public class ARGameManager : MonoBehaviour
    {
        [SerializeField] ARSurfaceManager m_SurfaceManager;
        [SerializeField] ARDeskPlacement m_Placement;
        [SerializeField] UIManager m_UI;

        GameState m_State = GameState.Initializing;

        /// <summary>Current flow state.</summary>
        public GameState State { get { return m_State; } }

        void Awake()
        {
            if (m_SurfaceManager == null)
                m_SurfaceManager = FindObjectOfType<ARSurfaceManager>();

            if (m_Placement == null)
                m_Placement = FindObjectOfType<ARDeskPlacement>();

            if (m_UI == null)
                m_UI = FindObjectOfType<UIManager>();
        }

        void OnEnable()
        {
            if (m_UI != null)
                m_UI.RescanRequested += OnRescanRequested;

            if (m_Placement != null)
                m_Placement.Placed += OnPlaced;
        }

        void OnDisable()
        {
            if (m_UI != null)
                m_UI.RescanRequested -= OnRescanRequested;

            if (m_Placement != null)
                m_Placement.Placed -= OnPlaced;
        }

        void Update()
        {
            SetState(EvaluateState());
        }

        GameState EvaluateState()
        {
            ARSessionState sessionState = ARSession.state;

            if (sessionState == ARSessionState.Unsupported)
                return GameState.Unsupported;

            // Checked before session state on purpose. Tracking dips in and out as the
            // player moves the phone, and an anchored play area should not send the flow
            // back to "starting camera" every time it does.
            if (m_Placement != null && m_Placement.HasPlacement)
                return GameState.Placed;

            // Anything below SessionTracking means the session is still starting or the
            // user has not granted the camera yet.
            if (sessionState < ARSessionState.SessionTracking)
                return GameState.Initializing;

            if (m_SurfaceManager != null && m_SurfaceManager.HasSuitableSurface)
                return GameState.SurfaceFound;

            return GameState.Scanning;
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
                    SetPlacementEnabled(false);
                    Report("AR is not available on this device.",
                        "Desk Quest needs a phone with ARCore or ARKit support.");
                    ShowRescan(false);
                    break;

                case GameState.Initializing:
                    SetPlacementEnabled(false);
                    Report("Starting camera...",
                        "Allow camera access if you are asked.");
                    ShowRescan(false);
                    break;

                case GameState.Scanning:
                    SetPlacementEnabled(false);
                    Report("Point your camera at your desk.",
                        "Move the phone slowly across the surface.");
                    ShowRescan(false);
                    break;

                case GameState.SurfaceFound:
                    SetPlacementEnabled(true);
                    Report("Desk found. Tap it to start.",
                        "Aim the green ring where you want the play area.");
                    ShowRescan(false);
                    break;

                case GameState.Placed:
                    SetPlacementEnabled(false);
                    Report("Play area locked to your desk.",
                        "Walk around it. The cube should stay on the desk.");
                    ShowRescan(true);
                    break;
            }
        }

        void SetPlacementEnabled(bool enabled)
        {
            if (m_Placement != null)
                m_Placement.SetPlacementEnabled(enabled);
        }

        void Report(string status, string hint)
        {
            if (m_UI == null)
                return;

            m_UI.SetStatus(status);
            m_UI.SetHint(hint);
        }

        void ShowRescan(bool visible)
        {
            if (m_UI != null)
                m_UI.SetRescanVisible(visible);
        }

        void OnPlaced(DeskPlayArea playArea)
        {
            if (m_SurfaceManager != null)
                m_SurfaceManager.StopDetection();

            Debug.Log("[DeskQuest] Play area anchored, size " + playArea.Size.x.ToString("F2") +
                      "m x " + playArea.Size.y.ToString("F2") + "m.");
        }

        void OnRescanRequested()
        {
            if (m_Placement != null)
                m_Placement.Clear();

            if (m_SurfaceManager != null)
                m_SurfaceManager.ResumeDetection();

            // Force a re-evaluation on the next frame rather than assuming which state
            // we land in; the surface that was chosen may no longer be the best one.
            SetState(GameState.Scanning);
        }
    }
}
