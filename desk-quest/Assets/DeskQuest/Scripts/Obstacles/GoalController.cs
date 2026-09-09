using System;
using DeskQuest.Common;
using UnityEngine;

namespace DeskQuest.Obstacles
{
    /// <summary>
    /// The treasure at the far end of the desk, and the check for reaching it.
    ///
    /// Arrival is a distance test rather than a physics trigger, because nothing else in
    /// the game uses Unity colliders and adding one here would mean maintaining a physics
    /// setup for a single sphere overlap.
    /// </summary>
    public class GoalController : MonoBehaviour
    {
        Transform m_Mesh;
        Material m_Material;
        float m_Radius;
        bool m_Reached;
        float m_Elapsed;

        /// <summary>Raised once, when the character first arrives.</summary>
        public event Action Reached;

        public bool IsReached { get { return m_Reached; } }

        /// <summary>Local position of the goal on the desk.</summary>
        public Vector3 LocalPosition { get { return transform.localPosition; } }

        /// <summary>Builds the treasure at a point on the desk, sized to the character.</summary>
        public void Build(Vector3 localPosition, float characterHeight)
        {
            transform.localPosition = localPosition;

            float size = characterHeight * 0.55f;
            m_Radius = characterHeight * 0.9f;

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "Treasure";
            cube.transform.SetParent(transform, false);
            cube.transform.localPosition = new Vector3(0f, size * 0.6f, 0f);
            cube.transform.localScale = new Vector3(size, size * 0.7f, size);

            Collider collider = cube.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);

            m_Material = MaterialLibrary.CreateOpaque(new Color(1f, 0.82f, 0.20f, 1f));
            MeshRenderer renderer = cube.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = m_Material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            m_Mesh = cube.transform;
        }

        /// <summary>
        /// Tests the character's position against the goal. Called by the game flow each
        /// frame rather than polling from here, so arrival cannot fire while the game is
        /// in a state that should ignore it.
        /// </summary>
        public void CheckArrival(Vector3 characterLocalPosition)
        {
            if (m_Reached)
                return;

            float dx = characterLocalPosition.x - transform.localPosition.x;
            float dz = characterLocalPosition.z - transform.localPosition.z;

            if (dx * dx + dz * dz > m_Radius * m_Radius)
                return;

            m_Reached = true;

            if (Reached != null)
                Reached();
        }

        public void Reset()
        {
            m_Reached = false;
        }

        void Update()
        {
            if (m_Mesh == null)
                return;

            m_Elapsed += Time.deltaTime;

            // Spinning and bobbing so it reads as the objective rather than another block.
            m_Mesh.Rotate(Vector3.up, 70f * Time.deltaTime, Space.Self);
        }

        void OnDestroy()
        {
            if (m_Material != null)
                Destroy(m_Material);
        }
    }
}
