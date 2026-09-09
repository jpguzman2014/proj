using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace DeskQuest.AR
{
    /// <summary>
    /// Owns horizontal plane detection and answers one question for the rest of the game:
    /// is there a surface big enough to be a desk yet?
    ///
    /// Planes are polled rather than event-subscribed. AR Foundation renamed the plane
    /// change event between 5.x and 6.x, and polling a handful of trackables a few times
    /// a second costs nothing while working unchanged on both.
    /// </summary>
    [RequireComponent(typeof(ARPlaneManager))]
    public class ARSurfaceManager : MonoBehaviour
    {
        [Tooltip("Smallest plane area, in square metres, that counts as a desk. " +
                 "0.05 is roughly a 22cm square, which ARCore reaches within a few seconds of scanning.")]
        [SerializeField] float m_MinimumSurfaceArea = 0.05f;

        [Tooltip("Seconds between plane scans. Planes grow gradually, so there is nothing to gain from checking every frame.")]
        [SerializeField] float m_ScanInterval = 0.25f;

        ARPlaneManager m_PlaneManager;
        float m_NextScanTime;
        readonly List<ARPlane> m_HorizontalPlanes = new List<ARPlane>();

        /// <summary>The largest horizontal-up plane found so far, or null.</summary>
        public ARPlane BestSurface { get; private set; }

        /// <summary>True when <see cref="BestSurface"/> is at least the minimum area.</summary>
        public bool HasSuitableSurface { get; private set; }

        /// <summary>Area of the best surface in square metres. Zero when there is none.</summary>
        public float BestSurfaceArea { get; private set; }

        void Awake()
        {
            m_PlaneManager = GetComponent<ARPlaneManager>();
        }

        void OnEnable()
        {
            // Vertical planes cost tracking budget and a desk is never one of them.
            m_PlaneManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
        }

        void Update()
        {
            if (Time.time < m_NextScanTime)
                return;

            m_NextScanTime = Time.time + m_ScanInterval;
            ScanPlanes();
        }

        void ScanPlanes()
        {
            m_HorizontalPlanes.Clear();
            BestSurface = null;
            BestSurfaceArea = 0f;

            foreach (ARPlane plane in m_PlaneManager.trackables)
            {
                if (plane.alignment != PlaneAlignment.HorizontalUp)
                    continue;

                m_HorizontalPlanes.Add(plane);

                float area = plane.size.x * plane.size.y;
                if (area > BestSurfaceArea)
                {
                    BestSurfaceArea = area;
                    BestSurface = plane;
                }
            }

            HasSuitableSurface = BestSurface != null && BestSurfaceArea >= m_MinimumSurfaceArea;
        }

        /// <summary>
        /// Hides the plane overlay once the desk is chosen, so the player sees their real
        /// desk and the game on it rather than a blue sheet over everything.
        ///
        /// Detection itself keeps running, which is deliberate and costs battery. The
        /// change detector's only signal is the desk plane growing into the space a real
        /// object used to occupy, so switching the plane manager off here would silently
        /// disable the mechanic the whole game is built on.
        /// </summary>
        public void EnterPlayMode()
        {
            SetPlanesVisible(false);
        }

        /// <summary>Shows the plane overlay again, for the rescan flow.</summary>
        public void ResumeDetection()
        {
            m_PlaneManager.enabled = true;
            SetPlanesVisible(true);
        }

        /// <summary>
        /// Shows or hides the plane overlay by switching renderers, not by deactivating
        /// the plane objects. A deactivated plane stops receiving geometry updates, and
        /// the change detector reads that geometry.
        /// </summary>
        public void SetPlanesVisible(bool visible)
        {
            foreach (ARPlane plane in m_PlaneManager.trackables)
            {
                Renderer[] renderers = plane.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < renderers.Length; i++)
                    renderers[i].enabled = visible;
            }
        }
    }
}
