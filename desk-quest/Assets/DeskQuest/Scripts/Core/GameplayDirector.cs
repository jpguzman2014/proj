using System;
using DeskQuest.AR;
using DeskQuest.Audio;
using DeskQuest.Character;
using DeskQuest.Detection;
using DeskQuest.Gameplay;
using DeskQuest.InputHandling;
using DeskQuest.Obstacles;
using DeskQuest.UI;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace DeskQuest.Core
{
    /// <summary>
    /// Runs a level once the desk has been chosen: builds the obstacles, the character and
    /// the goal, watches the real desk for change, and closes the loop the whole game is
    /// built around.
    ///
    /// That loop is: an obstacle blocks the path, the player takes the matching real object
    /// off their desk, the tracker sees the desk surface where the object was, the virtual
    /// obstacle clears, and the character can go on. Everything else here exists to make
    /// that legible to a child playing it.
    ///
    /// Detection can fail, and on some desks it will. A manual fallback opens after a wait
    /// so a level is never unfinishable, and it is offered rather than hidden.
    /// </summary>
    public class GameplayDirector : MonoBehaviour
    {
        [Tooltip("Seconds an obstacle can be the next one to clear before the manual fallback is offered.")]
        [SerializeField] float m_ManualFallbackDelay = 25f;

        [SerializeField] EnvironmentChangeDetector m_Detector;
        [SerializeField] GameAudio m_Audio;

        DeskPlayArea m_PlayArea;
        ObstacleManager m_Obstacles;
        StickManController m_Character;
        GoalController m_Goal;
        ControlPad m_ControlPad;
        Camera m_Camera;

        float m_WaitingSince;
        int m_WaitingFor = -1;
        bool m_Running;

        /// <summary>Raised with the line to show the player, whenever it changes.</summary>
        public event Action<string> ObjectiveChanged;

        /// <summary>Raised once the character reaches the treasure.</summary>
        public event Action LevelCompleted;

        /// <summary>True when the manual fallback is currently offered for the waiting obstacle.</summary>
        public bool ManualFallbackOffered { get; private set; }

        public bool IsRunning { get { return m_Running; } }

        /// <summary>
        /// The line the player should currently be shown. Readable as well as raised as
        /// an event, because the flow enters the playing state a frame after the level
        /// starts and would otherwise miss the first one.
        /// </summary>
        public string CurrentObjective { get { return BuildObjective(); } }

        /// <summary>
        /// Builds and starts a level on an anchored play area.
        /// </summary>
        public void StartLevel(DeskPlayArea playArea, ControlPad controlPad,
            ARPlaneManager planeManager, Camera arCamera)
        {
            StopLevel();

            m_PlayArea = playArea;
            m_ControlPad = controlPad;
            m_Camera = arCamera != null ? arCamera : Camera.main;

            Vector2 size = playArea.Size;
            LevelDefinition level = LevelLayout.BuildDefaultLevel();

            GameObject obstacleRoot = new GameObject("Obstacles");
            obstacleRoot.transform.SetParent(playArea.transform, false);
            m_Obstacles = obstacleRoot.AddComponent<ObstacleManager>();

            GameObject characterRoot = new GameObject("Character");
            characterRoot.transform.SetParent(playArea.transform, false);
            m_Character = characterRoot.AddComponent<StickManController>();
            m_Character.Initialise(size, controlPad, m_Obstacles);

            m_Obstacles.Build(level, size, m_Character.Height);

            GameObject goalRoot = new GameObject("Goal");
            goalRoot.transform.SetParent(playArea.transform, false);
            m_Goal = goalRoot.AddComponent<GoalController>();
            m_Goal.Build(LevelLayout.ToLocalPosition(level.Goal, size), m_Character.Height);

            m_Character.PlaceAt(LevelLayout.ToLocalPosition(level.Start, size), 0f);

            m_Obstacles.Cleared += OnObstacleCleared;
            m_Goal.Reached += OnGoalReached;
            m_Character.Jumped += OnJumped;
            m_Character.ClimbRejected += OnClimbRejected;

            if (m_Detector != null)
            {
                m_Detector.SetPlaneManager(planeManager);
                m_Detector.AreaCleared += OnAreaDetectedClear;
                m_Detector.Begin(playArea.transform, m_Obstacles);
            }

            m_Running = true;
            ResetWait();
            PushObjective();
        }

        public void StopLevel()
        {
            if (m_Obstacles != null)
            {
                m_Obstacles.Cleared -= OnObstacleCleared;
                Destroy(m_Obstacles.gameObject);
            }

            if (m_Goal != null)
            {
                m_Goal.Reached -= OnGoalReached;
                Destroy(m_Goal.gameObject);
            }

            if (m_Character != null)
            {
                m_Character.Jumped -= OnJumped;
                m_Character.ClimbRejected -= OnClimbRejected;
                Destroy(m_Character.gameObject);
            }

            if (m_Detector != null)
            {
                m_Detector.AreaCleared -= OnAreaDetectedClear;
                m_Detector.Stop();
            }

            m_Obstacles = null;
            m_Goal = null;
            m_Character = null;
            m_PlayArea = null;
            m_Running = false;
            ManualFallbackOffered = false;
        }

        void Update()
        {
            if (!m_Running)
                return;

            if (m_Goal != null && m_Character != null && m_Character.Motor != null)
                m_Goal.CheckArrival(m_Character.Motor.Position);

            UpdateWait();
        }

        /// <summary>
        /// Tracks how long the player has been stuck on the same obstacle, and offers the
        /// manual fallback once that passes the limit.
        /// </summary>
        void UpdateWait()
        {
            Obstacle next = m_Obstacles != null ? m_Obstacles.NextToClear() : null;

            if (next == null)
            {
                ResetWait();
                return;
            }

            if (next.Id != m_WaitingFor)
            {
                m_WaitingFor = next.Id;
                m_WaitingSince = Time.time;
                SetFallbackOffered(false);
                return;
            }

            bool overdue = Time.time - m_WaitingSince > m_ManualFallbackDelay;
            SetFallbackOffered(overdue);
        }

        void ResetWait()
        {
            m_WaitingFor = -1;
            m_WaitingSince = Time.time;
            SetFallbackOffered(false);
        }

        void SetFallbackOffered(bool offered)
        {
            if (ManualFallbackOffered == offered)
                return;

            ManualFallbackOffered = offered;
            PushObjective();
        }

        /// <summary>
        /// Handles a tap on the world during play. Only does anything while the manual
        /// fallback is offered, and only on the obstacle the player is actually stuck on,
        /// so it cannot be used to skip the level.
        /// </summary>
        public bool TryHandleWorldTap(Vector2 screenPosition)
        {
            if (!m_Running || !ManualFallbackOffered || m_Camera == null || m_PlayArea == null)
                return false;

            Vector3 localPoint;
            if (!TryProjectOntoPlayArea(screenPosition, out localPoint))
                return false;

            int index = m_Obstacles.IndexAt(localPoint.x, localPoint.z);
            if (index < 0)
                return false;

            Obstacle tapped = m_Obstacles.Views[index];
            if (!tapped.NeedsRealWorldClearing || tapped.Id != m_WaitingFor)
                return false;

            m_Obstacles.ClearObstacle(index);
            return true;
        }

        /// <summary>
        /// Intersects a screen ray with the plane of the desk and returns the hit in play
        /// area local space. Plain geometry rather than a physics raycast, since nothing in
        /// the play area has a collider.
        /// </summary>
        bool TryProjectOntoPlayArea(Vector2 screenPosition, out Vector3 localPoint)
        {
            localPoint = Vector3.zero;

            Ray ray = m_Camera.ScreenPointToRay(new Vector3(screenPosition.x, screenPosition.y, 0f));
            Transform surface = m_PlayArea.transform;

            float denominator = Vector3.Dot(ray.direction, surface.up);

            // Looking along the desk rather than at it: no usable intersection.
            if (Mathf.Abs(denominator) < 1e-5f)
                return false;

            float distance = Vector3.Dot(surface.position - ray.origin, surface.up) / denominator;
            if (distance <= 0f)
                return false;

            localPoint = surface.InverseTransformPoint(ray.GetPoint(distance));
            return true;
        }

        void OnAreaDetectedClear(int obstacleIndex)
        {
            m_Obstacles.ClearObstacle(obstacleIndex);
        }

        void OnObstacleCleared(Obstacle obstacle)
        {
            if (m_Audio != null)
                m_Audio.PlayObstacleCleared();

            ResetWait();
            PushObjective();
        }

        void OnGoalReached()
        {
            if (m_Character != null)
                m_Character.Celebrate();

            if (m_ControlPad != null)
                m_ControlPad.SetVisible(false);

            if (m_Audio != null)
            {
                m_Audio.PlayGoalReached();
                m_Audio.PlayLevelComplete();
            }

            if (m_Detector != null)
                m_Detector.Stop();

            m_Running = false;

            if (LevelCompleted != null)
                LevelCompleted();
        }

        void OnJumped()
        {
            if (m_Audio != null)
                m_Audio.PlayJump();
        }

        void OnClimbRejected()
        {
            if (m_Audio != null)
                m_Audio.PlayRejected();
        }

        void PushObjective()
        {
            if (ObjectiveChanged == null)
                return;

            ObjectiveChanged(BuildObjective());
        }

        string BuildObjective()
        {
            if (m_Obstacles == null)
                return string.Empty;

            Obstacle next = m_Obstacles.NextToClear();
            if (next == null)
                return "Path clear. Get to the treasure!";

            if (ManualFallbackOffered)
                return "Still blocked? Tap the red block to clear it.";

            return "Take the " + next.Label.ToLower() + " off your desk to clear the red block.";
        }
    }
}
