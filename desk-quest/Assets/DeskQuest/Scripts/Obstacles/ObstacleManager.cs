using System;
using System.Collections.Generic;
using DeskQuest.Gameplay;
using UnityEngine;

namespace DeskQuest.Obstacles
{
    /// <summary>
    /// Owns the level's obstacles: builds them from a layout, hands the collision boxes to
    /// the motor, and clears them.
    ///
    /// The boxes and the views are kept as parallel lists indexed the same way. The motor
    /// reads the boxes every step, so marking one cleared takes effect on the next frame
    /// with nothing to notify.
    /// </summary>
    public class ObstacleManager : MonoBehaviour, IObstacleProvider
    {
        readonly List<ObstacleBox> m_Boxes = new List<ObstacleBox>();
        readonly List<Obstacle> m_Views = new List<Obstacle>();

        // The patch of desk watched for real-world change under each obstacle, which is
        // smaller than the obstacle itself for the barriers that span the whole desk.
        readonly List<ObstacleBox> m_DetectionFootprints = new List<ObstacleBox>();
        readonly List<ClearZoneMarker> m_Markers = new List<ClearZoneMarker>();

        /// <summary>Raised when an obstacle is cleared, with the obstacle that went.</summary>
        public event Action<Obstacle> Cleared;

        public List<ObstacleBox> Obstacles { get { return m_Boxes; } }

        /// <summary>The obstacle views, in the same order as the boxes.</summary>
        public IList<Obstacle> Views { get { return m_Views; } }

        /// <summary>How many obstacles still need the matching real object removing.</summary>
        public int RemainingToClear
        {
            get
            {
                int remaining = 0;
                for (int i = 0; i < m_Views.Count; i++)
                {
                    if (m_Views[i].NeedsRealWorldClearing && !m_Views[i].Cleared)
                        remaining++;
                }

                return remaining;
            }
        }

        public bool PathIsOpen { get { return RemainingToClear == 0; } }

        /// <summary>Builds the level's obstacles under this transform.</summary>
        public void Build(LevelDefinition level, Vector2 playAreaSize, float characterHeight)
        {
            ClearAll();

            for (int i = 0; i < level.Obstacles.Count; i++)
            {
                ObstacleDefinition definition = level.Obstacles[i];
                ObstacleBox box = LevelLayout.ToBox(definition, i, playAreaSize, characterHeight);

                GameObject obstacleObject = new GameObject("Obstacle " + i + " " + definition.Label);
                obstacleObject.transform.SetParent(transform, false);

                Obstacle view = obstacleObject.AddComponent<Obstacle>();
                view.Build(box, definition.Label);

                ObstacleBox footprint = LevelLayout.ToDetectionFootprint(
                    definition, i, playAreaSize, characterHeight);

                m_Boxes.Add(box);
                m_Views.Add(view);
                m_DetectionFootprints.Add(footprint);
                m_Markers.Add(view.NeedsRealWorldClearing
                    ? BuildMarker(footprint, characterHeight)
                    : null);
            }
        }

        ClearZoneMarker BuildMarker(ObstacleBox footprint, float characterHeight)
        {
            GameObject markerObject = new GameObject("ClearZone");
            markerObject.transform.SetParent(transform, false);

            ClearZoneMarker marker = markerObject.AddComponent<ClearZoneMarker>();
            marker.Build(footprint.Center, footprint.Size, characterHeight * 0.05f);
            return marker;
        }

        /// <summary>The patch of desk watched for change under an obstacle.</summary>
        public ObstacleBox DetectionFootprintFor(int index)
        {
            return m_DetectionFootprints[index];
        }

        /// <summary>Shows how close an area is to counting as cleared, from 0 to 1.</summary>
        public void SetClearProgress(int index, float coverage01)
        {
            if (index < 0 || index >= m_Markers.Count || m_Markers[index] == null)
                return;

            m_Markers[index].SetProgress(coverage01);
        }

        void ClearAll()
        {
            for (int i = 0; i < m_Views.Count; i++)
            {
                if (m_Views[i] != null)
                    Destroy(m_Views[i].gameObject);
            }

            for (int i = 0; i < m_Markers.Count; i++)
            {
                if (m_Markers[i] != null)
                    Destroy(m_Markers[i].gameObject);
            }

            m_Boxes.Clear();
            m_Views.Clear();
            m_DetectionFootprints.Clear();
            m_Markers.Clear();
        }

        /// <summary>
        /// Marks an obstacle cleared. Returns false if the index is unknown or it was
        /// already cleared, so callers can tell a real change from a repeat.
        /// </summary>
        public bool ClearObstacle(int index)
        {
            if (index < 0 || index >= m_Boxes.Count)
                return false;

            if (m_Boxes[index].Cleared)
                return false;

            ObstacleBox box = m_Boxes[index];
            box.Cleared = true;
            m_Boxes[index] = box;

            m_Views[index].Clear();

            if (m_Markers[index] != null)
                m_Markers[index].SetVisible(false);

            if (Cleared != null)
                Cleared(m_Views[index]);

            return true;
        }

        /// <summary>Puts every obstacle back, for restarting without rebuilding.</summary>
        public void RestoreAll()
        {
            for (int i = 0; i < m_Boxes.Count; i++)
            {
                ObstacleBox box = m_Boxes[i];
                box.Cleared = false;
                m_Boxes[i] = box;
                m_Views[i].Restore();

                if (m_Markers[i] != null)
                {
                    m_Markers[i].SetVisible(true);
                    m_Markers[i].SetProgress(0f);
                }
            }
        }

        /// <summary>
        /// The next obstacle the player has to tidy away, or null when the path is open.
        /// Used for the status line, so the game can say what it is waiting for.
        /// </summary>
        public Obstacle NextToClear()
        {
            for (int i = 0; i < m_Views.Count; i++)
            {
                if (m_Views[i].NeedsRealWorldClearing && !m_Views[i].Cleared)
                    return m_Views[i];
            }

            return null;
        }

        /// <summary>Index of the obstacle whose footprint contains a play-area point, or -1.</summary>
        public int IndexAt(float x, float z)
        {
            for (int i = 0; i < m_Boxes.Count; i++)
            {
                if (!m_Boxes[i].Cleared && m_Boxes[i].HorizontalDistanceTo(x, z) <= 0f)
                    return i;
            }

            return -1;
        }
    }
}
