using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DeskQuest.UI
{
    /// <summary>
    /// A button that reports being held down, not just clicked.
    ///
    /// uGUI's Button only fires on release, which is useless for a direction pad: the
    /// player holds a direction, they do not tap it once per step. This tracks the press
    /// itself and exposes both the held state and a one-shot press for actions like jump.
    /// </summary>
    public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        /// <summary>True for as long as a finger is down on this button.</summary>
        public bool IsHeld { get; private set; }

        /// <summary>Raised once when the button goes down.</summary>
        public event Action Pressed;

        Graphic m_Background;
        Color m_IdleColor;
        Color m_HeldColor;

        public void Configure(Graphic background, Color idleColor, Color heldColor)
        {
            m_Background = background;
            m_IdleColor = idleColor;
            m_HeldColor = heldColor;
            ApplyColor();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            IsHeld = true;
            ApplyColor();

            if (Pressed != null)
                Pressed();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Release();
        }

        /// <summary>
        /// A finger that slides off the button counts as releasing it. Without this the
        /// character keeps walking after the player's thumb has wandered off the control.
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            Release();
        }

        void OnDisable()
        {
            Release();
        }

        void Release()
        {
            IsHeld = false;
            ApplyColor();
        }

        void ApplyColor()
        {
            if (m_Background != null)
                m_Background.color = IsHeld ? m_HeldColor : m_IdleColor;
        }
    }
}
