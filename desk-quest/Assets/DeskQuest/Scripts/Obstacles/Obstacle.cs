using DeskQuest.Common;
using DeskQuest.Gameplay;
using UnityEngine;

namespace DeskQuest.Obstacles
{
    /// <summary>
    /// The visible half of an obstacle. The collision half is an <see cref="ObstacleBox"/>
    /// held by the manager; this draws it and plays the clearing animation.
    ///
    /// Colour carries meaning, since the prototype has no icons: green can be climbed,
    /// amber can be jumped, red cannot be beaten by playing and only opens when the
    /// matching real object leaves the desk.
    /// </summary>
    public class Obstacle : MonoBehaviour
    {
        const float k_ClearDuration = 0.4f;

        [SerializeField] int m_Id;
        [SerializeField] ObstacleType m_Type;
        [SerializeField] string m_Label;

        Transform m_Mesh;
        Material m_Material;
        Vector3 m_FullScale;
        bool m_Clearing;
        float m_ClearElapsed;

        public int Id { get { return m_Id; } }
        public ObstacleType Type { get { return m_Type; } }

        /// <summary>The category of real object this stands for, for the status line.</summary>
        public string Label { get { return m_Label; } }

        public bool Cleared { get; private set; }

        /// <summary>
        /// True for obstacles that exist to be tidied away rather than played around.
        /// Only these gate the level.
        /// </summary>
        public bool NeedsRealWorldClearing { get { return m_Type == ObstacleType.BlockedPath; } }

        public void Build(ObstacleBox box, string label)
        {
            m_Id = box.Id;
            m_Type = box.Type;
            m_Label = label;

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "Mesh";
            cube.transform.SetParent(transform, false);
            cube.transform.localPosition = Vector3.zero;
            cube.transform.localScale = box.Size;

            Collider collider = cube.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);

            m_Material = MaterialLibrary.CreateOpaque(ColorFor(box.Type));
            MeshRenderer renderer = cube.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = m_Material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            m_Mesh = cube.transform;
            m_FullScale = box.Size;

            transform.localPosition = box.Center;
        }

        static Color ColorFor(ObstacleType type)
        {
            switch (type)
            {
                case ObstacleType.Climbable:
                    return new Color(0.25f, 0.78f, 0.42f, 1f);
                case ObstacleType.LowObstacle:
                    return new Color(0.95f, 0.72f, 0.22f, 1f);
                case ObstacleType.BlockedPath:
                    return new Color(0.90f, 0.30f, 0.28f, 1f);
                case ObstacleType.Wall:
                    return new Color(0.45f, 0.48f, 0.55f, 1f);
                default:
                    return new Color(0.55f, 0.42f, 0.78f, 1f);
            }
        }

        /// <summary>
        /// Starts the clearing animation. The obstacle stops blocking immediately in the
        /// collision list; this is only the visual catching up.
        /// </summary>
        public void Clear()
        {
            if (Cleared)
                return;

            Cleared = true;
            m_Clearing = true;
            m_ClearElapsed = 0f;
        }

        /// <summary>Puts a cleared obstacle back, for restarting the level.</summary>
        public void Restore()
        {
            Cleared = false;
            m_Clearing = false;

            if (m_Mesh != null)
            {
                m_Mesh.localScale = m_FullScale;
                m_Mesh.gameObject.SetActive(true);
            }
        }

        void Update()
        {
            if (!m_Clearing || m_Mesh == null)
                return;

            m_ClearElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(m_ClearElapsed / k_ClearDuration);

            // Shrinks into the desk rather than fading, because the materials are opaque
            // and switching them to transparent for one animation is not worth a second
            // shader variant on a phone.
            m_Mesh.localScale = m_FullScale * (1f - t);

            if (t >= 1f)
            {
                m_Clearing = false;
                m_Mesh.gameObject.SetActive(false);
            }
        }

        void OnDestroy()
        {
            if (m_Material != null)
                Destroy(m_Material);
        }
    }
}
