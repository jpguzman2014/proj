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
        public T[] GetComponentsInChildren<T>(bool includeInactive) where T : Component { return new T[0]; }
        public T GetComponentInParent<T>() where T : Component { return null; }
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
        public bool activeSelf { get { return false; } }
        public bool activeInHierarchy { get { return false; } }
        public static GameObject CreatePrimitive(PrimitiveType type) { return null; }
        public static GameObject Find(string name) { return null; }
    }

    public class Transform : Component, IEnumerable
    {
        public Vector3 position { get; set; }
        public Quaternion rotation { get; set; }
        public Vector3 localPosition { get; set; }
        public Vector3 localScale { get; set; }
        public Quaternion localRotation { get; set; }
        public int childCount { get { return 0; } }
        public Vector3 forward { get { return Vector3.zero; } }
        public Vector3 up { get { return Vector3.up; } }
        public Vector3 right { get { return Vector3.zero; } }
        public Transform parent { get; set; }
        public void SetParent(Transform parent, bool worldPositionStays) { }
        public void SetPositionAndRotation(Vector3 position, Quaternion rotation) { }
        public void Rotate(Vector3 axis, float angle, Space relativeTo) { }
        public Vector3 TransformPoint(Vector3 position) { return position; }
        public Vector3 InverseTransformPoint(Vector3 position) { return position; }
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
        public float magnitude { get { return Mathf.Sqrt(x * x + y * y); } }
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
        public static float Dot(Vector3 a, Vector3 b) { return a.x * b.x + a.y * b.y + a.z * b.z; }
        public float sqrMagnitude { get { return x * x + y * y + z * z; } }
        public float magnitude { get { return Mathf.Sqrt(sqrMagnitude); } }
        public Vector3 normalized
        {
            get
            {
                float m = magnitude;
                return m > 1e-9f ? new Vector3(x / m, y / m, z / m) : zero;
            }
        }
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

    /// <summary>
    /// Really implemented, not stubbed to dummy values. The gameplay logic is pure maths
    /// and is unit-tested against these, so a fake Sin would make the tests meaningless.
    /// </summary>
    public static class Mathf
    {
        public const float PI = 3.14159265358979f;
        public const float Deg2Rad = PI / 180f;
        public const float Rad2Deg = 180f / PI;
        public const float Epsilon = 1.401298E-45f;

        public static float Sin(float f) { return (float)System.Math.Sin(f); }
        public static float Cos(float f) { return (float)System.Math.Cos(f); }
        public static float Sqrt(float f) { return (float)System.Math.Sqrt(f); }
        public static float Atan2(float y, float x) { return (float)System.Math.Atan2(y, x); }
        public static float Pow(float f, float p) { return (float)System.Math.Pow(f, p); }
        public static float Min(float a, float b) { return a < b ? a : b; }
        public static float Max(float a, float b) { return a > b ? a : b; }
        public static int Min(int a, int b) { return a < b ? a : b; }
        public static int Max(int a, int b) { return a > b ? a : b; }
        public static float Abs(float f) { return f < 0f ? -f : f; }
        public static float Sign(float f) { return f < 0f ? -1f : 1f; }
        public static int FloorToInt(float f) { return (int)System.Math.Floor(f); }
        public static float Repeat(float t, float length) { return t - (float)System.Math.Floor(t / length) * length; }
        public static float Clamp(float v, float min, float max) { return v < min ? min : (v > max ? max : v); }
        public static float Clamp01(float v) { return v < 0f ? 0f : (v > 1f ? 1f : v); }
        public static float Lerp(float a, float b, float t) { return a + (b - a) * Clamp01(t); }
        public static float LerpAngle(float a, float b, float t)
        {
            float delta = Repeat(b - a, 360f);
            if (delta > 180f) delta -= 360f;
            return a + delta * Clamp01(t);
        }
        public static float MoveTowards(float current, float target, float maxDelta)
        {
            if (Abs(target - current) <= maxDelta) return target;
            return current + Sign(target - current) * maxDelta;
        }
        public static float MoveTowardsAngle(float current, float target, float maxDelta)
        {
            float delta = Repeat(target - current, 360f);
            if (delta > 180f) delta -= 360f;
            return current + Clamp(delta, -maxDelta, maxDelta);
        }
        public static bool Approximately(float a, float b) { return Abs(b - a) < 1e-5f; }
        public static float DeltaAngle(float current, float target)
        {
            float delta = Repeat(target - current, 360f);
            if (delta > 180f) delta -= 360f;
            return delta;
        }
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

    public struct Ray
    {
        public Vector3 origin;
        public Vector3 direction;
        public Ray(Vector3 origin, Vector3 direction) { this.origin = origin; this.direction = direction; }
        public Vector3 GetPoint(float distance) { return origin + direction * distance; }
    }

    public class Camera : Behaviour
    {
        public static Camera main { get { return null; } }
        public Ray ScreenPointToRay(Vector3 position) { return new Ray(); }
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

    public class AudioClip : Object
    {
        public static AudioClip Create(string name, int lengthSamples, int channels, int frequency, bool stream) { return null; }
        public bool SetData(float[] data, int offsetSamples) { return true; }
        public int samples { get { return 0; } }
    }

    public class AudioSource : Behaviour
    {
        public AudioClip clip { get; set; }
        public float volume { get; set; }
        public bool playOnAwake { get; set; }
        public bool loop { get; set; }
        public void Play() { }
        public void PlayOneShot(AudioClip clip, float volumeScale) { }
    }

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

    [AttributeUsage(AttributeTargets.Field)]
    public class Header : Attribute { public Header(string header) { } }

    [AttributeUsage(AttributeTargets.Field)]
    public class HideInInspector : Attribute { }
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
