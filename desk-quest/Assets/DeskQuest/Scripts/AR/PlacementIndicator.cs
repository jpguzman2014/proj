using DeskQuest.Common;
using UnityEngine;

namespace DeskQuest.AR
{
    /// <summary>
    /// The ring drawn on the surface under the centre of the screen, showing where a tap
    /// would place the play area. Built from a procedural mesh so there is no prefab or
    /// texture to import.
    /// </summary>
    public class PlacementIndicator : MonoBehaviour
    {
        [SerializeField] float m_InnerRadius = 0.045f;
        [SerializeField] float m_OuterRadius = 0.06f;
        [SerializeField] int m_Segments = 48;

        MeshRenderer m_Renderer;
        Material m_Material;
        bool m_Visible;

        void Awake()
        {
            MeshFilter filter = gameObject.AddComponent<MeshFilter>();
            filter.sharedMesh = MeshBuilder.CreateRing(m_InnerRadius, m_OuterRadius, m_Segments);

            m_Renderer = gameObject.AddComponent<MeshRenderer>();
            m_Material = MaterialLibrary.CreateTransparent(Palette.ReticleSearching);
            m_Renderer.sharedMaterial = m_Material;

            // A reticle that casts or receives shadows reads as a solid object rather
            // than an overlay, and costs draw calls the phone does not need to spend.
            m_Renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            m_Renderer.receiveShadows = false;

            // Set directly rather than through SetVisible: the cached flag already reads
            // false, so SetVisible would return without touching the renderer and the
            // ring would sit at the world origin until the first ShowAt call.
            m_Renderer.enabled = false;
            m_Visible = false;
        }

        /// <summary>Moves the ring onto a surface pose and shows it.</summary>
        public void ShowAt(Pose pose, bool valid)
        {
            transform.SetPositionAndRotation(pose.position, pose.rotation);
            MaterialLibrary.ApplyColor(m_Material, valid ? Palette.ReticleValid : Palette.ReticleSearching);
            SetVisible(true);
        }

        public void Hide()
        {
            SetVisible(false);
        }

        void SetVisible(bool visible)
        {
            if (m_Visible == visible)
                return;

            m_Visible = visible;
            m_Renderer.enabled = visible;
        }

        void OnDestroy()
        {
            if (m_Material != null)
                Destroy(m_Material);
        }
    }
}
