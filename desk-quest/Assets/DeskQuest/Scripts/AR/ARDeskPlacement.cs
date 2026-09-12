using System;
using System.Collections.Generic;
using DeskQuest.InputHandling;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace DeskQuest.AR
{
    /// <summary>
    /// Turns a tap on a detected plane into an anchored play area.
    ///
    /// The play area is parented to an <see cref="ARAnchor"/> rather than left at a fixed
    /// world position. AR Foundation continuously corrects its estimate of where surfaces
    /// are as tracking improves; an anchor is moved along with those corrections, so the
    /// game stays stuck to the real desk instead of drifting off it.
    /// </summary>
    public class ARDeskPlacement : MonoBehaviour
    {
        [SerializeField] ARRaycastManager m_RaycastManager;
        [SerializeField] ARAnchorManager m_AnchorManager;
        [SerializeField] Camera m_ARCamera;
        [SerializeField] PlacementIndicator m_Indicator;

        [Tooltip("Preferred play area size in metres. Shrunk to fit if the detected surface is smaller.")]
        [SerializeField] Vector2 m_PreferredPlayAreaSize = new Vector2(0.6f, 0.4f);

        [Tooltip("Smallest usable play area in metres. Below this the surface is rejected as too small for a level.")]
        [SerializeField] Vector2 m_MinimumPlayAreaSize = new Vector2(0.25f, 0.18f);

        readonly List<ARRaycastHit> m_Hits = new List<ARRaycastHit>();

        ARAnchor m_Anchor;
        DeskPlayArea m_PlayArea;
        bool m_PlacementEnabled;

        /// <summary>Raised once the play area is anchored.</summary>
        public event Action<DeskPlayArea> Placed;

        /// <summary>True once a desk has been chosen and anchored.</summary>
        public bool HasPlacement { get { return m_PlayArea != null; } }

        /// <summary>The anchored play area, or null before placement.</summary>
        public DeskPlayArea PlayArea { get { return m_PlayArea; } }

        void Awake()
        {
            if (m_ARCamera == null)
                m_ARCamera = Camera.main;

            if (m_RaycastManager == null)
                Debug.LogError("[DeskQuest] ARDeskPlacement has no ARRaycastManager. Taps cannot hit the desk.");

            if (m_AnchorManager == null)
                Debug.LogWarning("[DeskQuest] No ARAnchorManager in the scene. The play area will be placed but not anchored, so it will drift as tracking updates.");
        }

        /// <summary>
        /// Enables or disables tap-to-place. Placement is off until the surface manager
        /// reports a surface worth tapping, so an early tap on a sliver of floor cannot
        /// start a level that will not fit.
        /// </summary>
        public void SetPlacementEnabled(bool enabled)
        {
            m_PlacementEnabled = enabled;

            if (!enabled && m_Indicator != null)
                m_Indicator.Hide();
        }

        void Update()
        {
            if (!m_PlacementEnabled || HasPlacement)
                return;

            UpdateIndicator();

            Vector2 tapPosition;
            if (TouchInput.TryGetTapThisFrame(out tapPosition))
                TryPlaceAt(tapPosition);
        }

        void UpdateIndicator()
        {
            if (m_Indicator == null)
                return;

            Pose pose;
            Vector2 size;
            Vector2 screenCentre = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

            if (TryGetSurfacePose(screenCentre, out pose, out size))
                m_Indicator.ShowAt(pose, IsLargeEnough(size));
            else
                m_Indicator.Hide();
        }

        void TryPlaceAt(Vector2 screenPosition)
        {
            Pose pose;
            Vector2 size;

            if (!TryGetSurfacePose(screenPosition, out pose, out size))
                return;

            if (!IsLargeEnough(size))
            {
                Debug.Log("[DeskQuest] Surface is too small for a play area. Keep scanning.");
                return;
            }

            Place(pose, size);
        }

        /// <summary>
        /// Raycasts the screen point against detected planes and returns a pose whose
        /// forward axis points away from the player, plus the play area size that fits
        /// on the plane that was hit.
        /// </summary>
        bool TryGetSurfacePose(Vector2 screenPosition, out Pose pose, out Vector2 size)
        {
            pose = default(Pose);
            size = Vector2.zero;

            if (m_RaycastManager == null)
                return false;

            m_Hits.Clear();
            if (!m_RaycastManager.Raycast(screenPosition, m_Hits, TrackableType.PlaneWithinPolygon))
                return false;

            ARRaycastHit hit = m_Hits[0];

            // The hit carries the trackable it landed on, so the plane comes straight
            // from the hit rather than from a second lookup through a manager.
            ARPlane plane = hit.trackable as ARPlane;

            if (plane != null && plane.alignment != PlaneAlignment.HorizontalUp)
                return false;

            pose = new Pose(hit.pose.position, BuildYawRotation(hit.pose.position));
            size = FitToPlane(plane);
            return true;
        }

        /// <summary>
        /// The play area is levelled to world up and yawed so its +Z runs away from the
        /// camera. That gives the on-screen UP button a meaning the player expects:
        /// "away from me", not "along some axis the tracker happened to pick".
        /// </summary>
        Quaternion BuildYawRotation(Vector3 surfacePoint)
        {
            if (m_ARCamera == null)
                return Quaternion.identity;

            Vector3 awayFromCamera = surfacePoint - m_ARCamera.transform.position;
            awayFromCamera.y = 0f;

            // Looking straight down leaves no usable horizontal direction; keep the
            // previous orientation rather than snapping to an arbitrary one.
            if (awayFromCamera.sqrMagnitude < 0.0001f)
                return Quaternion.identity;

            return Quaternion.LookRotation(awayFromCamera.normalized, Vector3.up);
        }

        Vector2 FitToPlane(ARPlane plane)
        {
            if (plane == null)
                return m_PreferredPlayAreaSize;

            // Leave a margin so the play area does not sit right on the plane edge,
            // where the surface estimate is least reliable.
            const float margin = 0.9f;
            Vector2 available = plane.size * margin;

            return new Vector2(
                Mathf.Min(m_PreferredPlayAreaSize.x, available.x),
                Mathf.Min(m_PreferredPlayAreaSize.y, available.y));
        }

        bool IsLargeEnough(Vector2 size)
        {
            return size.x >= m_MinimumPlayAreaSize.x && size.y >= m_MinimumPlayAreaSize.y;
        }

        void Place(Pose pose, Vector2 size)
        {
            GameObject anchorObject = new GameObject("DeskAnchor");
            anchorObject.transform.SetPositionAndRotation(pose.position, pose.rotation);

            // Adding the component is the version-stable way to create an anchor: the
            // manager-side helpers were renamed between AR Foundation 5 and 6.
            m_Anchor = anchorObject.AddComponent<ARAnchor>();

            GameObject playAreaObject = new GameObject("DeskPlayArea");
            playAreaObject.transform.SetParent(anchorObject.transform, false);

            m_PlayArea = playAreaObject.AddComponent<DeskPlayArea>();
            m_PlayArea.Build(size);

            SetPlacementEnabled(false);

            if (Placed != null)
                Placed(m_PlayArea);
        }

        /// <summary>Removes the current placement so the player can choose a different desk.</summary>
        public void Clear()
        {
            if (m_Anchor != null)
                Destroy(m_Anchor.gameObject);

            m_Anchor = null;
            m_PlayArea = null;
        }
    }
}
