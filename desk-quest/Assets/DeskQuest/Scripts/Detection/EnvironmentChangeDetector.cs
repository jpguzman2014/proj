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
    /// This class does two things and neither is the judgement itself: it samples coverage
    /// through <see cref="DeskOccupancyProbe"/> on a timer, and it feeds each reading to an
    /// <see cref="AreaClearanceTracker"/>. The decision of what a sequence of readings means
    /// lives in the tracker, which has no Unity dependency and is unit-tested, because that
    /// is the part most likely to be wrong and the part hardest to check on a real desk.
    ///
    /// The whole approach is a proxy for tidying up rather than recognition. See
    /// <see cref="DeskOccupancyProbe"/> for what it cannot tell you.
    /// </summary>
    public class EnvironmentChangeDetector : MonoBehaviour
    {
        class WatchedArea
        {
            public int ObstacleIndex;
            public Vector3 LocalCentre;
            public Vector3 LocalSize;
            public AreaClearanceTracker Tracker;
        }

        [SerializeField] ARPlaneManager m_PlaneManager;

        [Tooltip("Coverage above this counts as the desk being visible under the obstacle.")]
        [Range(0f, 1f)]
        [SerializeField] float m_ClearedThreshold = 0.6f;

        [Tooltip("Coverage at or below this on first look establishes that something is there to move.")]
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
        float m_LastSampleTime;
        bool m_Running;

        /// <summary>Raised with the obstacle index when an area is judged cleared.</summary>
        public event Action<int> AreaCleared;

        /// <summary>False when there is no plane manager, in which case detection cannot run at all.</summary>
        public bool IsAvailable { get { return m_Probe != null && m_Probe.IsUsable; } }

        /// <summary>How many areas are being watched. Zero means the mechanic is inert.</summary>
        public int WatchedCount { get { return m_Watched.Count; } }

        public void SetPlaneManager(ARPlaneManager planeManager)
        {
            m_PlaneManager = planeManager;
        }

        /// <summary>Starts watching the patch under every obstacle that needs real-world clearing.</summary>
        public void Begin(Transform playAreaRoot, ObstacleManager obstacles)
        {
            m_PlayAreaRoot = playAreaRoot;
            m_ObstacleManager = obstacles;
            m_Probe = new DeskOccupancyProbe(m_PlaneManager);
            m_Watched.Clear();

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
                area.Tracker = new AreaClearanceTracker(
                    m_OccupiedThreshold, m_ClearedThreshold, m_HoldSeconds);
                m_Watched.Add(area);
            }

            m_LastSampleTime = Time.time;
            m_Running = m_Watched.Count > 0 && IsAvailable;

            if (!IsAvailable)
                Debug.LogWarning("[DeskQuest] No plane manager for change detection. " +
                                 "Obstacles will only clear through the manual fallback.");
        }

        public void Stop()
        {
            m_Running = false;
        }

        /// <summary>Latest coverage for an obstacle, from 0 to 1, or -1 if it is not watched.</summary>
        public float CoverageFor(int obstacleIndex)
        {
            WatchedArea area = Find(obstacleIndex);
            return area != null ? area.Tracker.Coverage : -1f;
        }

        /// <summary>
        /// True when an area has been seen as occupied and is therefore capable of being
        /// detected as cleared. False means the detector never got a baseline, usually
        /// because the player has not pointed the camera at that patch of desk.
        /// </summary>
        public bool HasBaseline(int obstacleIndex)
        {
            WatchedArea area = Find(obstacleIndex);
            return area != null && area.Tracker.HasBaseline;
        }

        WatchedArea Find(int obstacleIndex)
        {
            for (int i = 0; i < m_Watched.Count; i++)
            {
                if (m_Watched[i].ObstacleIndex == obstacleIndex)
                    return m_Watched[i];
            }

            return null;
        }

        void Update()
        {
            if (!m_Running || Time.time < m_NextSampleTime)
                return;

            // Real elapsed time rather than the nominal interval, so a frame hitch does
            // not quietly shorten or lengthen the hold the tracker is measuring.
            float elapsed = Time.time - m_LastSampleTime;
            m_LastSampleTime = Time.time;
            m_NextSampleTime = Time.time + m_SampleInterval;

            Sample(elapsed);
        }

        void Sample(float elapsed)
        {
            for (int i = 0; i < m_Watched.Count; i++)
            {
                WatchedArea area = m_Watched[i];

                if (area.Tracker.IsCleared)
                    continue;

                float coverage = m_Probe.CoverageOf(
                    m_PlayAreaRoot, area.LocalCentre, area.LocalSize, m_SamplesPerAxis);

                bool justCleared = area.Tracker.Observe(coverage, elapsed);

                if (m_ObstacleManager != null)
                    m_ObstacleManager.SetClearProgress(area.ObstacleIndex, area.Tracker.Progress);

                if (justCleared && AreaCleared != null)
                    AreaCleared(area.ObstacleIndex);
            }
        }
    }
}
