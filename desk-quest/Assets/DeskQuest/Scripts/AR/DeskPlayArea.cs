using DeskQuest.Common;
using UnityEngine;

namespace DeskQuest.AR
{
    /// <summary>
    /// The virtual play area built on the chosen desk surface. In Milestone 1 it is a
    /// boundary outline plus a marker cube, which is enough to confirm by eye that the
    /// content stays locked to the real desk as the phone moves. Later milestones add
    /// the stick man, obstacles and goal underneath this same transform, so everything
    /// inherits the anchor's tracking.
    /// </summary>
    public class DeskPlayArea : MonoBehaviour
    {
        [Tooltip("Play area size in metres, X across and Z away from the player.")]
        [SerializeField] Vector2 m_Size = new Vector2(0.6f, 0.4f);

        [SerializeField] float m_BoundaryLineWidth = 0.004f;
        [SerializeField] float m_MarkerSize = 0.05f;

        LineRenderer m_Boundary;
        Transform m_Marker;
        Material m_BoundaryMaterial;
        Material m_MarkerMaterial;

        /// <summary>Play area size in metres. Read by later systems to lay out obstacles.</summary>
        public Vector2 Size { get { return m_Size; } }

        /// <summary>
        /// Builds the visuals. Called by the placement code after the anchor exists, and
        /// sized to whatever the detected plane can actually hold.
        /// </summary>
        public void Build(Vector2 size)
        {
            m_Size = size;
            BuildBoundary();
            BuildMarker();
        }

        void BuildBoundary()
        {
            GameObject boundaryObject = new GameObject("Boundary");
            boundaryObject.transform.SetParent(transform, false);

            m_Boundary = boundaryObject.AddComponent<LineRenderer>();
            m_Boundary.useWorldSpace = false;
            m_Boundary.loop = true;
            m_Boundary.positionCount = 4;
            m_Boundary.startWidth = m_BoundaryLineWidth;
            m_Boundary.endWidth = m_BoundaryLineWidth;
            m_Boundary.numCornerVertices = 0;
            m_Boundary.numCapVertices = 0;
            m_Boundary.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            m_Boundary.receiveShadows = false;

            m_BoundaryMaterial = MaterialLibrary.CreateTransparent(Palette.PlayAreaBoundary);
            m_Boundary.sharedMaterial = m_BoundaryMaterial;

            float halfX = m_Size.x * 0.5f;
            float halfZ = m_Size.y * 0.5f;

            // Lifted a hair off the surface: drawn exactly on the plane, the outline
            // fights with the detected plane mesh and flickers.
            const float surfaceOffset = 0.002f;

            m_Boundary.SetPosition(0, new Vector3(-halfX, surfaceOffset, -halfZ));
            m_Boundary.SetPosition(1, new Vector3(halfX, surfaceOffset, -halfZ));
            m_Boundary.SetPosition(2, new Vector3(halfX, surfaceOffset, halfZ));
            m_Boundary.SetPosition(3, new Vector3(-halfX, surfaceOffset, halfZ));
        }

        void BuildMarker()
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = "PlacementMarker";
            marker.transform.SetParent(transform, false);
            marker.transform.localPosition = new Vector3(0f, m_MarkerSize * 0.5f, 0f);
            marker.transform.localScale = Vector3.one * m_MarkerSize;

            // The collider is dead weight until there is a character to collide with it.
            Collider markerCollider = marker.GetComponent<Collider>();
            if (markerCollider != null)
                Destroy(markerCollider);

            MeshRenderer renderer = marker.GetComponent<MeshRenderer>();
            m_MarkerMaterial = MaterialLibrary.CreateOpaque(Palette.DeskMarker);
            renderer.sharedMaterial = m_MarkerMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            m_Marker = marker.transform;
        }

        void Update()
        {
            // A slow spin makes it obvious at a glance that the object is a live 3D
            // object sitting on the desk and not a still image pasted over the camera.
            if (m_Marker != null)
                m_Marker.Rotate(Vector3.up, 45f * Time.deltaTime, Space.Self);
        }

        void OnDestroy()
        {
            if (m_BoundaryMaterial != null)
                Destroy(m_BoundaryMaterial);
            if (m_MarkerMaterial != null)
                Destroy(m_MarkerMaterial);
        }
    }
}
