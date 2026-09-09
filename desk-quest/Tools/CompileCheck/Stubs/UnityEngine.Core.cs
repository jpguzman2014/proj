// Stub declarations of the Unity APIs this project uses.
// See ../README.md: these exist only so a C# compiler can type-check the game scripts
// in an environment with no Unity installation. They are never shipped or built into
// the app, and they are a reconstruction of Unity's API, not Unity itself.
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine
{
    public class Object
    {
        public string name;
        public static void Destroy(Object o) { }
        public static void DestroyImmediate(Object o) { }
        public static T FindObjectOfType<T>() where T : Object { return null; }
    }

    public class Component : Object
    {
        public GameObject gameObject { get { return null; } }
        public Transform transform { get { return null; } }
        public T GetComponent<T>() where T : Component { return null; }
        public Component GetComponent(Type type) { return null; }
    }

    public class Behaviour : Component { public bool enabled { get; set; } }
    public class MonoBehaviour : Behaviour { }

    public class GameObject : Object
    {
        public GameObject() { }
        public GameObject(string name) { }
        public GameObject(string name, params Type[] components) { }
        public string tag;
        public Transform transform { get { return null; } }
        public T AddComponent<T>() where T : Component { return null; }
        public Component AddComponent(Type type) { return null; }
        public T GetComponent<T>() where T : Component { return null; }
        public void SetActive(bool active) { }
        public static GameObject CreatePrimitive(PrimitiveType type) { return null; }
        public static GameObject Find(string name) { return null; }
    }

    public class Transform : Component, IEnumerable
    {
        public Vector3 position { get; set; }
        public Quaternion rotation { get; set; }
        public Vector3 localPosition { get; set; }
        public Vector3 localScale { get; set; }
        public Vector3 forward { get { return Vector3.zero; } }
        public Transform parent { get; set; }
        public void SetParent(Transform parent, bool worldPositionStays) { }
        public void SetPositionAndRotation(Vector3 position, Quaternion rotation) { }
        public void Rotate(Vector3 axis, float angle, Space relativeTo) { }
        public IEnumerator GetEnumerator() { yield break; }
    }

    public enum Space { World, Self }
    public enum PrimitiveType { Sphere, Capsule, Cylinder, Cube, Plane, Quad }

    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public static Vector2 zero { get { return new Vector2(0, 0); } }
        public static Vector2 one { get { return new Vector2(1, 1); } }
        public float sqrMagnitude { get { return x * x + y * y; } }
        public static Vector2 operator *(Vector2 a, float b) { return new Vector2(a.x * b, a.y * b); }
        public static Vector2 operator -(Vector2 a, Vector2 b) { return new Vector2(a.x - b.x, a.y - b.y); }
        public static Vector2 operator +(Vector2 a, Vector2 b) { return new Vector2(a.x + b.x, a.y + b.y); }
        public static implicit operator Vector3(Vector2 v) { return new Vector3(v.x, v.y, 0f); }
    }

    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 zero { get { return new Vector3(0, 0, 0); } }
        public static Vector3 one { get { return new Vector3(1, 1, 1); } }
        public static Vector3 up { get { return new Vector3(0, 1, 0); } }
        public Vector3 normalized { get { return this; } }
        public float sqrMagnitude { get { return x * x + y * y + z * z; } }
        public static Vector3 operator *(Vector3 a, float b) { return new Vector3(a.x * b, a.y * b, a.z * b); }
        public static Vector3 operator -(Vector3 a, Vector3 b) { return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z); }
        public static Vector3 operator +(Vector3 a, Vector3 b) { return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z); }
        public static implicit operator Vector2(Vector3 v) { return new Vector2(v.x, v.y); }
    }

    public struct Quaternion
    {
        public static Quaternion identity { get { return new Quaternion(); } }
        public static Quaternion LookRotation(Vector3 forward, Vector3 upwards) { return identity; }
        public static Quaternion Euler(float x, float y, float z) { return identity; }
    }

    public struct Pose
    {
        public Vector3 position;
        public Quaternion rotation;
        public Pose(Vector3 position, Quaternion rotation) { this.position = position; this.rotation = rotation; }
    }

    public struct Color
    {
        public float r, g, b, a;
        public Color(float r, float g, float b) { this.r = r; this.g = g; this.b = b; this.a = 1f; }
        public Color(float r, float g, float b, float a) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public static Color white { get { return new Color(1, 1, 1); } }
    }

    public static class Mathf
    {
        public const float PI = 3.14159265f;
        public static float Sin(float f) { return 0f; }
        public static float Cos(float f) { return 0f; }
        public static float Min(float a, float b) { return a < b ? a : b; }
        public static float Max(float a, float b) { return a > b ? a : b; }
        public static float Abs(float f) { return f < 0 ? -f : f; }
        public static float Clamp(float v, float min, float max) { return v; }
        public static float Clamp01(float v) { return v; }
        public static float MoveTowards(float current, float target, float maxDelta) { return target; }
    }

    public static class Debug
    {
        public static void Log(object message) { }
        public static void LogWarning(object message) { }
        public static void LogError(object message) { }
    }

    public static class Time
    {
        public static float time { get { return 0f; } }
        public static float deltaTime { get { return 0f; } }
    }

    public static class Screen
    {
        public static int width { get { return 1080; } }
        public static int height { get { return 1920; } }
    }

    public static class Application
    {
        public static bool isPlaying { get { return false; } }
    }

    public class Camera : Behaviour
    {
        public static Camera main { get { return null; } }
        public float nearClipPlane { get; set; }
        public float farClipPlane { get; set; }
    }

    public class Shader : Object
    {
        public static Shader Find(string name) { return null; }
    }

    public class Material : Object
    {
        public Material(Shader shader) { }
        public Color color { get; set; }
        public bool HasProperty(string name) { return false; }
        public void SetColor(string name, Color value) { }
    }

    public class Mesh : Object
    {
        public void SetVertices(List<Vector3> vertices) { }
        public void SetUVs(int channel, List<Vector2> uvs) { }
        public void SetTriangles(List<int> triangles, int submesh) { }
        public void RecalculateNormals() { }
        public void RecalculateBounds() { }
    }

    public class MeshFilter : Component { public Mesh sharedMesh { get; set; } }

    public class Renderer : Component
    {
        public Material sharedMaterial { get; set; }
        public bool enabled { get; set; }
        public bool receiveShadows { get; set; }
        public Rendering.ShadowCastingMode shadowCastingMode { get; set; }
    }

    public class MeshRenderer : Renderer { }

    public class LineRenderer : Renderer
    {
        public bool useWorldSpace { get; set; }
        public bool loop { get; set; }
        public int positionCount { get; set; }
        public float startWidth { get; set; }
        public float endWidth { get; set; }
        public int numCornerVertices { get; set; }
        public int numCapVertices { get; set; }
        public void SetPosition(int index, Vector3 position) { }
    }

    public class Collider : Component { public bool isTrigger { get; set; } }
    public class BoxCollider : Collider { }
    public class CapsuleCollider : Collider { }
    public class Rigidbody : Component { public bool useGravity { get; set; } public bool isKinematic { get; set; } }
    public class CharacterController : Collider { }

    public static class Resources
    {
        public static T GetBuiltinResource<T>(string path) where T : Object { return null; }
    }

    public class Font : Object
    {
        public static Font CreateDynamicFontFromOSFont(string fontname, int size) { return null; }
    }

    public class AudioClip : Object { }
    public class AudioSource : Behaviour { }

    [AttributeUsage(AttributeTargets.Field)]
    public class SerializeField : Attribute { }

    [AttributeUsage(AttributeTargets.Field)]
    public class TooltipAttribute : Attribute { public TooltipAttribute(string tooltip) { } }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RequireComponent : Attribute
    {
        public RequireComponent(Type a) { }
        public RequireComponent(Type a, Type b) { }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class RangeAttribute : Attribute { public RangeAttribute(float min, float max) { } }
}

namespace UnityEngine.Rendering
{
    public enum ShadowCastingMode { Off, On, TwoSided, ShadowsOnly }
    public enum GraphicsDeviceType { OpenGLES3, Vulkan, Metal }
}

namespace UnityEngine.SceneManagement
{
    public struct Scene { public string name { get { return ""; } } }
    public class SceneManager { public static Scene GetActiveScene() { return new Scene(); } }
}

namespace UnityEngine.Events
{
    public delegate void UnityAction();
    public class UnityEvent { public void AddListener(UnityAction call) { } public void Invoke() { } }
}
