using System;
using System.Collections.Generic;
using DeskQuest.Gameplay;
using DeskQuest.Obstacles;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace DeskQuest.Detection
{
    /// <summary>
    /// Watches the patches of real desk under the obstacles that need tidying, and reports
    /// when one of them has been cleared.
    ///
    /// Coverage from <see cref="DeskOccupancyProbe"/> is noisy: it jumps around as the
    /// tracker refines the plane, and it rises as soon as the player merely looks at a
    /// patch properly. So a single high reading means nothing. An area counts as cleared
    /// only when it starts out mostly uncovered, then goes above the threshold and stays
    /// there while the player keeps looking at it. Both halves matter. The baseline stops
    /// an area that was already clear from firing on the first frame, and the hold stops a
    /// flicker of coverage from opening the path.
    ///
    /// This is the least certain part of the prototype and the part most likely to need
    /// rethinking after it meets a real desk. It is a proxy for tidying up, not a
    /// recognition system, and it should not be described to a player as one.
    /// </summary>
    public class EnvironmentChangeDetector : MonoBehaviour
    {
        /// <summary>What the detector knows about one watched area.</summary>
        class WatchedArea
        {
            public int ObstacleIndex;
            public Vector3 LocalCentre;
            public Vector3 LocalSize;
            public float Coverage;
            public float HeldFor;
            public bool BaselineEstablished;
            public bool Reported;
        }

        [SerializeField] ARPlaneManager m_PlaneManager;

        [Tooltip("Coverage above this counts as the desk being visible under the obstacle.")]
        [Range(0f, 1f)]
        [SerializeField] float m_ClearedThreshold = 0.6f;

        [Tooltip("Coverage below this on first look establishes that something is there to move.")]
        [Range(0f, 1f)]
        [SerializeField] float m_OccupiedThreshold = 0.35f;

        [Tooltip("Seconds coverage must stay above the threshold before the obstacle clears.")]
        [SerializeField] float m_HoldSeconds = 1.5f;

        [Tooltip("Seconds between samples. Plane geometry updates slowly, so there is nothing to gain from every frame.")]
        [SerializeField] float m_SampleInterval = 0.35f;

        [Tooltip("Samples across each axis of a footprint. Nine points total at three.")]
        [SerializeField] int m_SamplesPerAxis = 3;

        DeskOccupancyProbe m_Probe;
        Transform m_PlayAreaRoot;
        ObstacleManager m_ObstacleManager;
        readonly List<WatchedArea> m_Watched = new List<WatchedArea>();
        float m_NextSampleTime;
        bool m_Running;

        /// <summary>Raised with the obstacle index when an area is judged cleared.</summary>
        public event Action<int> AreaCleared;

        /// <summary>False when there is no plane manager, in which case detection cannot run at all.</summary>
        public bool IsAvailable { get { return m_Probe != null && m_Probe.IsUsable; } }

        public void SetPlaneManager(ARPlaneManager planeManager)
        {
            m_PlaneManager = planeManager;
        }

        /// <summary>
        /// Starts watching the areas under every obstacle that needs real-world clearing.
        /// </summary>
        public void Begin(Transform playAreaRoot, ObstacleManager obstacles)
        {
            m_PlayAreaRoot = playAreaRoot;
            m_Probe = new DeskOccupancyProbe(m_PlaneManager);
            m_Watched.Clear();

            m_ObstacleManager = obstacles;

            List<ObstacleBox> boxes = obstacles.Obstacles;
            for (int i = 0; i < boxes.Count; i++)
            {
                if (!obstacles.Views[i].NeedsRealWorldClearing)
                    continue;

                ObstacleBox footprint = obstacles.DetectionFootprintFor(i);

                WatchedArea area = new WatchedArea();
                area.ObstacleIndex = i;
                area.LocalCentre = footprint.Center;
                area.LocalSize = footprint.Size;
                m_Watched.Add(area);
            }

            m_Running = m_Watched.Count > 0 && IsAvailable;

            if (!IsAvailable)
                Debug.LogWarning("[DeskQuest] No plane manager for change detection. " +
                                 "Obstacles will only clear through the manual fallback.");
        }

        public void Stop()
        {
            m_Running = false;
        }

        /// <summary>
        /// Current coverage for an obstacle, from 0 to 1, or -1 if it is not watched.
        /// The interface uses this to show progress rather than leaving the player
        /// wondering whether anything is happening.
        /// </summary>
        public float CoverageFor(int obstacleIndex)
        {
            for (int i = 0; i < m_Watched.Count; i++)
            {
                if (m_Watched[i].ObstacleIndex == obstacleIndex)
                    return m_Watched[i].Coverage;
            }

            return -1f;
        }

        void Update()
        {
            if (!m_Running || Time.time < m_NextSampleTime)
                return;

            m_NextSampleTime = Time.time + m_SampleInterval;
            Sample();
        }

        void Sample()
        {
            float elapsed = m_SampleInterval;

            for (int i = 0; i < m_Watched.Count; i++)
            {
                WatchedArea area = m_Watched[i];

                if (area.Reported)
                    continue;

                area.Coverage = m_Probe.CoverageOf(
                    m_PlayAreaRoot, area.LocalCentre, area.LocalSize, m_SamplesPerAxis);

                if (m_ObstacleManager != null)
                    m_ObstacleManager.SetClearProgress(area.ObstacleIndex, area.Coverage);

                // Until the tracker has seen the area as occupied at least once, there is
                // no change to detect. Without this an obstacle placed over already-clear
                // desk would clear itself the moment the level started.
                if (!area.BaselineEstablished)
                {
                    if (area.Coverage <= m_OccupiedThreshold)
                        area.BaselineEstablished = true;

                    area.HeldFor = 0f;
                    continue;
                }

                if (area.Coverage >= m_ClearedThreshold)
                    area.HeldFor += elapsed;
                else
                    area.HeldFor = 0f;

                if (area.HeldFor < m_HoldSeconds)
                    continue;

                area.Reported = true;

                if (AreaCleared != null)
                    AreaCleared(area.ObstacleIndex);
            }
        }

        /// <summary>
        /// True when an area has been seen as occupied and is therefore capable of being
        /// detected as cleared. False means the detector never got a baseline, usually
        /// because the player has not pointed the camera at that patch of desk.
        /// </summary>
        public bool HasBaseline(int obstacleIndex)
        {
            for (int i = 0; i < m_Watched.Count; i++)
            {
                if (m_Watched[i].ObstacleIndex == obstacleIndex)
                    return m_Watched[i].BaselineEstablished;
            }

            return false;
        }
    }
}
