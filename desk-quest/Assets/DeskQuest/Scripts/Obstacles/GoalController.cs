using System;
using DeskQuest.Common;
using UnityEngine;

namespace DeskQuest.Obstacles
{
    /// <summary>
    /// The treasure at the far end of the desk.
    ///
    /// Walking up to it is not enough: the player presses the USE button to open it. That
    /// gives the interact control something real to do in this version, and it makes
    /// finishing an act rather than an accident of walking into a radius.
    ///
    /// It opens on its own after a few seconds in range as well. The interact button has
    /// never been pressed on a real device, and a level that cannot be finished because one
    /// button does not work would be a bad way to find that out.
    ///
    /// Proximity is a distance test rather than a physics trigger, because nothing else in
    /// the game uses Unity colliders and adding one here would mean maintaining a physics
    /// setup for a single sphere overlap.
    /// </summary>
    public class GoalController : MonoBehaviour
    {
        [Tooltip("Seconds in range before the treasure opens without the interact button.")]
        [SerializeField] float m_AutoOpenSeconds = 4f;

        Transform m_Mesh;
        Material m_Material;
        float m_Radius;
        float m_BaseScale;
        bool m_Opened;
        float m_Elapsed;
        float m_InRangeFor;

        /// <summary>Raised once, when the treasure is opened.</summary>
        public event Action Opened;

        /// <summary>True while the character is close enough to open it.</summary>
        public bool PlayerInRange { get; private set; }

        public bool IsOpened { get { return m_Opened; } }

        /// <summary>Local position of the goal on the desk.</summary>
        public Vector3 LocalPosition { get { return transform.localPosition; } }

        /// <summary>Builds the treasure at a point on the desk, sized to the character.</summary>
        public void Build(Vector3 localPosition, float characterHeight)
        {
            transform.localPosition = localPosition;

            float size = characterHeight * 0.55f;
            m_Radius = characterHeight * 0.9f;
            m_BaseScale = size;

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
        /// Updates proximity, and opens the treasure if the player has been in range long
        /// enough without pressing the button. Called by the game flow each frame rather
        /// than polling from here, so it cannot fire in a state that should ignore it.
        /// </summary>
        public void UpdateProximity(Vector3 characterLocalPosition, float deltaTime)
        {
            if (m_Opened)
                return;

            float dx = characterLocalPosition.x - transform.localPosition.x;
            float dz = characterLocalPosition.z - transform.localPosition.z;
            PlayerInRange = dx * dx + dz * dz <= m_Radius * m_Radius;

            if (!PlayerInRange)
            {
                m_InRangeFor = 0f;
                return;
            }

            m_InRangeFor += deltaTime;

            if (m_InRangeFor >= m_AutoOpenSeconds)
                Open();
        }

        /// <summary>
        /// Opens the treasure if the character is close enough. Returns false when it is
        /// not, so the caller can give the same rejection feedback as a failed climb.
        /// </summary>
        public bool TryOpen()
        {
            if (m_Opened || !PlayerInRange)
                return false;

            Open();
            return true;
        }

        void Open()
        {
            m_Opened = true;

            if (Opened != null)
                Opened();
        }

        public void Reset()
        {
            m_Opened = false;
            PlayerInRange = false;
            m_InRangeFor = 0f;
        }

        void Update()
        {
            if (m_Mesh == null)
                return;

            m_Elapsed += Time.deltaTime;

            // Spinning so it reads as the objective rather than another block, and growing
            // when the player is close enough to open it, so the prompt has something to
            // point at.
            m_Mesh.Rotate(Vector3.up, 70f * Time.deltaTime, Space.Self);

            float target = PlayerInRange && !m_Opened ? 1.25f : 1f;
            float pulse = PlayerInRange && !m_Opened ? 1f + Mathf.Sin(m_Elapsed * 6f) * 0.06f : 1f;
            float scale = m_BaseScale * target * pulse;
            m_Mesh.localScale = new Vector3(scale, scale * 0.7f, scale);
        }

        void OnDestroy()
        {
            if (m_Material != null)
                Destroy(m_Material);
        }
    }
}
