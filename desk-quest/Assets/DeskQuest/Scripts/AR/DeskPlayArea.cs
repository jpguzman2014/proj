using DeskQuest.Common;
using UnityEngine;

namespace DeskQuest.AR
{
    /// <summary>
    /// The virtual play area built on the chosen desk surface: a boundary outline, and
    /// the parent that the character, obstacles and goal all hang from.
    ///
    /// Everything being under one transform is what keeps the game stuck to the real
    /// desk. The anchor corrects this transform as tracking improves, and the whole level
    /// moves with it without any gameplay code knowing that happened.
    /// </summary>
    public class DeskPlayArea : MonoBehaviour
    {
        [Tooltip("Play area size in metres, X across and Z away from the player.")]
        [SerializeField] Vector2 m_Size = new Vector2(0.6f, 0.4f);

        [SerializeField] float m_BoundaryLineWidth = 0.004f;

        LineRenderer m_Boundary;
        Material m_BoundaryMaterial;

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

        void OnDestroy()
        {
            if (m_BoundaryMaterial != null)
                Destroy(m_BoundaryMaterial);
        }
    }
}
