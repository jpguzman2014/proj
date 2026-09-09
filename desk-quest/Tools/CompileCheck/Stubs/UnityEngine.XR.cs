// Stub declarations. See ../README.md.
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.XR.ARSubsystems
{
    public struct TrackableId { }

    public enum PlaneAlignment { None, Vertical, HorizontalUp, HorizontalDown, NotAxisAligned }

    [System.Flags]
    public enum PlaneDetectionMode { None = 0, Horizontal = 1, Vertical = 2 }

    [System.Flags]
    public enum TrackableType
    {
        None = 0,
        PlaneWithinPolygon = 8,
        PlaneWithinBounds = 4,
        FeaturePoint = 2,
        AllTypes = 255
    }

    public enum TrackingState { None, Limited, Tracking }
}

namespace UnityEngine.XR.ARFoundation
{
    using UnityEngine.XR.ARSubsystems;

    public enum ARSessionState
    {
        None = 0,
        Unsupported = 1,
        CheckingAvailability = 2,
        NeedsInstall = 3,
        Installing = 4,
        Ready = 5,
        SessionInitializing = 6,
        SessionTracking = 7
    }

    public class ARSession : MonoBehaviour
    {
        public static ARSessionState state { get { return ARSessionState.None; } }
    }

    public class ARInputManager : MonoBehaviour { }

    public class ARTrackable : MonoBehaviour
    {
        public TrackableId trackableId { get { return new TrackableId(); } }
        public TrackingState trackingState { get { return TrackingState.None; } }
    }

    public class ARPlane : ARTrackable
    {
        public PlaneAlignment alignment { get { return PlaneAlignment.None; } }
        public Vector2 size { get { return Vector2.zero; } }
        public Vector3 center { get { return Vector3.zero; } }
        public Unity.Collections.NativeArray<Vector2> boundary
        {
            get { return new Unity.Collections.NativeArray<Vector2>(); }
        }
    }

    public class ARAnchor : ARTrackable { }

    public class TrackableCollection<T> : IEnumerable<T>
    {
        public int count { get { return 0; } }
        public IEnumerator<T> GetEnumerator() { yield break; }
        IEnumerator IEnumerable.GetEnumerator() { yield break; }
    }

    public class ARPlaneManager : MonoBehaviour
    {
        public PlaneDetectionMode requestedDetectionMode { get; set; }
        public GameObject planePrefab { get; set; }
        public TrackableCollection<ARPlane> trackables { get { return null; } }
        public ARPlane GetPlane(TrackableId trackableId) { return null; }
    }

    public class ARAnchorManager : MonoBehaviour { }

    public struct ARRaycastHit
    {
        public Pose pose { get { return new Pose(); } }
        public TrackableId trackableId { get { return new TrackableId(); } }
        public ARTrackable trackable { get { return null; } }
        public float distance { get { return 0f; } }
    }

    public class ARRaycastManager : MonoBehaviour
    {
        public bool Raycast(Vector2 screenPoint, List<ARRaycastHit> hitResults, TrackableType trackableTypes) { return false; }
    }

    public class ARCameraManager : MonoBehaviour { }
    public class ARCameraBackground : MonoBehaviour { }
    public class ARPlaneMeshVisualizer : MonoBehaviour { }
}

namespace Unity.XR.CoreUtils
{
    using UnityEngine;

    public class XROrigin : MonoBehaviour
    {
        public Camera Camera { get; set; }
        public GameObject CameraFloorOffsetObject { get; set; }
        public Transform TrackablesParent { get { return null; } }
    }
}

namespace Unity.Collections
{
    using UnityEngine;

    /// <summary>Only the read surface the occupancy probe uses.</summary>
    public struct NativeArray<T>
    {
        public int Length { get { return 0; } }
        public bool IsCreated { get { return false; } }
        public T this[int index] { get { return default(T); } set { } }
        public void Dispose() { }
    }
}
