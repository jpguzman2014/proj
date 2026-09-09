using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace DeskQuest.InputHandling
{
    /// <summary>
    /// Reads taps without caring which input backend the project is set to.
    /// A project created from Unity's AR template defaults to the new Input System,
    /// where the legacy <c>UnityEngine.Input</c> calls throw at runtime; a project
    /// created from a plain 3D template usually defaults to the old one. Routing every
    /// read through here means the rest of the game never has to know.
    /// Mouse input is included so the scene is still testable in the Editor.
    /// </summary>
    public static class TouchInput
    {
        /// <summary>
        /// True on the frame a tap starts, ignoring taps that land on UI.
        /// </summary>
        public static bool TryGetTapThisFrame(out Vector2 screenPosition)
        {
            screenPosition = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen != null && touchscreen.primaryTouch.press.wasPressedThisFrame)
            {
                screenPosition = touchscreen.primaryTouch.position.ReadValue();
                return !IsPointerOverUi(screenPosition);
            }

            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                screenPosition = mouse.position.ReadValue();
                return !IsPointerOverUi(screenPosition);
            }
#else
            if (UnityEngine.Input.touchCount > 0)
            {
                Touch touch = UnityEngine.Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    screenPosition = touch.position;
                    return !IsPointerOverUi(screenPosition);
                }
            }
            else if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                screenPosition = UnityEngine.Input.mousePosition;
                return !IsPointerOverUi(screenPosition);
            }
#endif

            return false;
        }

        /// <summary>
        /// The EventSystem raycast is the only reliable way to tell a tap on a control
        /// from a tap on the world. Without it, pressing a UI button would also place
        /// the desk behind it.
        /// </summary>
        public static bool IsPointerOverUi(Vector2 screenPosition)
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
                return false;

            PointerEventData pointerData = new PointerEventData(eventSystem);
            pointerData.position = screenPosition;

            System.Collections.Generic.List<RaycastResult> results =
                new System.Collections.Generic.List<RaycastResult>();
            eventSystem.RaycastAll(pointerData, results);
            return results.Count > 0;
        }
    }
}
