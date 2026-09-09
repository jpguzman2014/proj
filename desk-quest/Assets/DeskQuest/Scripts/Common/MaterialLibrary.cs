using UnityEngine;

namespace DeskQuest.Common
{
    /// <summary>
    /// Creates materials at runtime from shaders that ship with Unity, so the project
    /// carries no material assets and works under both the Built-in and Universal
    /// render pipelines. Keeping every material unlit also keeps the GPU cost near zero,
    /// which matters on a phone that is already running AR tracking.
    /// </summary>
    public static class MaterialLibrary
    {
        static readonly string[] OpaqueShaderNames =
        {
            "Universal Render Pipeline/Unlit",
            "Unlit/Color",
            "Legacy Shaders/Diffuse"
        };

        static readonly string[] TransparentShaderNames =
        {
            "Sprites/Default",
            "Universal Render Pipeline/Unlit",
            "Unlit/Color"
        };

        static Shader s_Opaque;
        static Shader s_Transparent;

        public static Shader OpaqueShader
        {
            get
            {
                if (s_Opaque == null)
                    s_Opaque = FindFirst(OpaqueShaderNames);
                return s_Opaque;
            }
        }

        public static Shader TransparentShader
        {
            get
            {
                if (s_Transparent == null)
                    s_Transparent = FindFirst(TransparentShaderNames);
                return s_Transparent;
            }
        }

        public static Material CreateOpaque(Color color)
        {
            return Create(OpaqueShader, color, "DeskQuest Opaque");
        }

        public static Material CreateTransparent(Color color)
        {
            return Create(TransparentShader, color, "DeskQuest Transparent");
        }

        static Material Create(Shader shader, Color color, string name)
        {
            if (shader == null)
            {
                Debug.LogError("[DeskQuest] No usable shader found. Materials will render magenta.");
                return null;
            }

            Material material = new Material(shader);
            material.name = name;
            ApplyColor(material, color);
            return material;
        }

        /// <summary>
        /// Built-in shaders expose the tint as _Color, URP shaders as _BaseColor.
        /// Setting whichever exists keeps one code path for both pipelines.
        /// </summary>
        public static void ApplyColor(Material material, Color color)
        {
            if (material == null)
                return;

            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);

            if (material.HasProperty("_Color"))
                material.SetColor("_Color", color);
        }

        static Shader FindFirst(string[] names)
        {
            for (int i = 0; i < names.Length; i++)
            {
                Shader shader = Shader.Find(names[i]);
                if (shader != null)
                    return shader;
            }

            return null;
        }
    }
}
