// Stub declarations. See ../README.md.
// Covers both input backends so TouchInput compiles under either.
namespace UnityEngine
{
    public enum TouchPhase { Began, Moved, Stationary, Ended, Canceled }

    public struct Touch
    {
        public Vector2 position { get; set; }
        public TouchPhase phase { get; set; }
        public int fingerId { get; set; }
    }

    public static class Input
    {
        public static int touchCount { get { return 0; } }
        public static Touch GetTouch(int index) { return new Touch(); }
        public static bool GetMouseButtonDown(int button) { return false; }
        public static Vector3 mousePosition { get { return Vector3.zero; } }
    }
}

namespace UnityEngine.InputSystem
{
    public class InputControl { }
    public class ButtonControl : InputControl { public bool wasPressedThisFrame { get { return false; } } }
    public class Vector2Control : InputControl { public Vector2 ReadValue() { return Vector2.zero; } }

    public class TouchControl
    {
        public ButtonControl press { get { return null; } }
        public Vector2Control position { get { return null; } }
    }

    public class Touchscreen
    {
        public static Touchscreen current { get { return null; } }
        public TouchControl primaryTouch { get { return null; } }
    }

    public class Mouse
    {
        public static Mouse current { get { return null; } }
        public ButtonControl leftButton { get { return null; } }
        public Vector2Control position { get { return null; } }
    }
}

namespace UnityEngine.InputSystem.UI
{
    public class InputSystemUIInputModule : UnityEngine.EventSystems.BaseInputModule { }
}
