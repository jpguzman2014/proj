using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace DeskQuest.Detection
{
    /// <summary>
    /// Asks a simple question about a patch of the real desk: can the tracker see the desk
    /// surface there, or is something sitting on it?
    ///
    /// The signal is plane coverage. ARCore and ARKit grow a horizontal plane across the
    /// flat surface they can see. An object standing on the desk is not flat desk, so the
    /// plane stops at its edge and the footprint underneath stays outside the plane
    /// polygon. Take the object away and, once the tracker has had a look at the newly
    /// visible surface, the polygon grows to include it.
    ///
    /// So coverage rising from low to high over a footprint means "the desk is visible
    /// here now, and it was not before". That is a proxy for the object being removed. It
    /// is not object recognition and it must not be described as any. What it cannot tell
    /// you:
    ///
    /// - what the object was, or whether it was tidied away or merely nudged aside
    /// - the difference between an object being removed and the player simply looking at
    ///   that patch of desk properly for the first time
    /// - anything at all about a flat object, a sheet of paper say, that the tracker
    ///   already treated as part of the desk surface
    ///
    /// All of it runs on tracked geometry the AR session already produces. No camera image
    /// is read, stored or sent anywhere.
    /// </summary>
    public class DeskOccupancyProbe
    {
        readonly ARPlaneManager m_PlaneManager;

        public DeskOccupancyProbe(ARPlaneManager planeManager)
        {
            m_PlaneManager = planeManager;
        }

        public bool IsUsable { get { return m_PlaneManager != null; } }

        /// <summary>
        /// Fraction of a footprint that detected horizontal planes cover, from 0 to 1.
        ///
        /// <paramref name="playAreaRoot"/> is the anchored play area, and the centre and
        /// size are in its local space. Samples are taken on a grid across the footprint;
        /// more samples cost more but read a partially cleared area more honestly.
        /// </summary>
        public float CoverageOf(Transform playAreaRoot, Vector3 localCentre, Vector3 localSize, int samplesPerAxis)
        {
            if (m_PlaneManager == null || playAreaRoot == null)
                return 0f;

            if (samplesPerAxis < 2)
                samplesPerAxis = 2;

            int covered = 0;
            int total = 0;

            for (int ix = 0; ix < samplesPerAxis; ix++)
            {
                for (int iz = 0; iz < samplesPerAxis; iz++)
                {
                    // Spread samples evenly across the footprint, inset from the edges so
                    // a plane that stops exactly at the boundary does not swing the result.
                    float fx = (ix + 0.5f) / samplesPerAxis - 0.5f;
                    float fz = (iz + 0.5f) / samplesPerAxis - 0.5f;

                    Vector3 localPoint = new Vector3(
                        localCentre.x + fx * localSize.x * 0.8f,
                        0f,
                        localCentre.z + fz * localSize.z * 0.8f);

                    Vector3 worldPoint = playAreaRoot.TransformPoint(localPoint);

                    total++;
                    if (IsCoveredByAnyPlane(worldPoint))
                        covered++;
                }
            }

            return total == 0 ? 0f : covered / (float)total;
        }

        bool IsCoveredByAnyPlane(Vector3 worldPoint)
        {
            foreach (ARPlane plane in m_PlaneManager.trackables)
            {
                if (plane.alignment != PlaneAlignment.HorizontalUp)
                    continue;

                if (IsInsidePlane(plane, worldPoint))
                    return true;
            }

            return false;
        }

        static bool IsInsidePlane(ARPlane plane, Vector3 worldPoint)
        {
            NativeArray<Vector2> boundary = plane.boundary;
            if (!boundary.IsCreated || boundary.Length < 3)
                return false;

            // The boundary is a polygon in the plane's local space, with the plane's
            // normal along local Y, so the polygon lives in local X and Z.
            Vector3 localPoint = plane.transform.InverseTransformPoint(worldPoint);

            // Reject points well above or below the plane, so an object's footprint is not
            // matched against a plane at a completely different height, such as the floor.
            const float heightTolerance = 0.05f;
            if (Mathf.Abs(localPoint.y) > heightTolerance)
                return false;

            return IsInsidePolygon(boundary, localPoint.x, localPoint.z);
        }

        /// <summary>
        /// Standard crossing-number test: count how many polygon edges a ray from the
        /// point crosses. An odd count means inside.
        /// </summary>
        static bool IsInsidePolygon(NativeArray<Vector2> polygon, float x, float y)
        {
            bool inside = false;
            int count = polygon.Length;

            for (int i = 0, j = count - 1; i < count; j = i++)
            {
                Vector2 a = polygon[i];
                Vector2 b = polygon[j];

                bool straddles = (a.y > y) != (b.y > y);
                if (!straddles)
                    continue;

                float denominator = b.y - a.y;
                if (Mathf.Abs(denominator) < 1e-9f)
                    continue;

                float crossingX = a.x + (y - a.y) / denominator * (b.x - a.x);
                if (x < crossingX)
                    inside = !inside;
            }

            return inside;
        }
    }
}
