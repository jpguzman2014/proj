using DeskQuest.Common;
using UnityEngine;

namespace DeskQuest.Obstacles
{
    /// <summary>
    /// A square drawn on the desk showing the patch the game is watching for change.
    ///
    /// Without it the mechanic is guesswork. The barrier spans the whole desk, but the
    /// patch that has to be cleared is one spot on it, and the player has no way to know
    /// which spot unless the game says so. This is the game saying so.
    /// </summary>
    public class ClearZoneMarker : MonoBehaviour
    {
        static readonly Color k_Waiting = new Color(0.95f, 0.35f, 0.30f, 0.85f);
        static readonly Color k_Progress = new Color(1.00f, 0.80f, 0.25f, 0.90f);

        LineRenderer m_Outline;
        Material m_Material;

        public void Build(Vector3 localCentre, Vector3 localSize, float lineWidth)
        {
            transform.localPosition = new Vector3(localCentre.x, 0.0015f, localCentre.z);

            m_Outline = gameObject.AddComponent<LineRenderer>();
            m_Outline.useWorldSpace = false;
            m_Outline.loop = true;
            m_Outline.positionCount = 4;
            m_Outline.startWidth = lineWidth;
            m_Outline.endWidth = lineWidth;
            m_Outline.numCornerVertices = 0;
            m_Outline.numCapVertices = 0;
            m_Outline.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            m_Outline.receiveShadows = false;

            m_Material = MaterialLibrary.CreateTransparent(k_Waiting);
            m_Outline.sharedMaterial = m_Material;

            float halfX = localSize.x * 0.5f;
            float halfZ = localSize.z * 0.5f;

            m_Outline.SetPosition(0, new Vector3(-halfX, 0f, -halfZ));
            m_Outline.SetPosition(1, new Vector3(halfX, 0f, -halfZ));
            m_Outline.SetPosition(2, new Vector3(halfX, 0f, halfZ));
            m_Outline.SetPosition(3, new Vector3(-halfX, 0f, halfZ));
        }

        /// <summary>
        /// Tints the square towards the progress colour as the desk underneath becomes
        /// visible, so the player can see that taking the object away is doing something
        /// before the obstacle actually clears.
        /// </summary>
        public void SetProgress(float coverage01)
        {
            if (m_Material == null)
                return;

            float t = Mathf.Clamp01(coverage01);
            Color color = new Color(
                Mathf.Lerp(k_Waiting.r, k_Progress.r, t),
                Mathf.Lerp(k_Waiting.g, k_Progress.g, t),
                Mathf.Lerp(k_Waiting.b, k_Progress.b, t),
                Mathf.Lerp(k_Waiting.a, k_Progress.a, t));

            MaterialLibrary.ApplyColor(m_Material, color);
        }

        public void SetVisible(bool visible)
        {
            if (m_Outline != null)
                m_Outline.enabled = visible;
        }

        void OnDestroy()
        {
            if (m_Material != null)
                Destroy(m_Material);
        }
    }
}
