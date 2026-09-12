using System;
using DeskQuest.Common;
using DeskQuest.Gameplay;
using DeskQuest.InputHandling;
using UnityEngine;
using UnityEngine.UI;

namespace DeskQuest.UI
{
    /// <summary>
    /// The on-screen controls: a four-way direction pad on the left, action buttons on
    /// the right.
    ///
    /// Both clusters sit in the bottom third and are translucent, because the camera feed
    /// behind them is the game. The pad is thumb-reachable on a phone held in one hand,
    /// which is why it is a cross of large targets rather than a compact joystick.
    ///
    /// Combinations come free: run, crouch and the directions are held states read in the
    /// same frame, so direction with run is a sprint and direction with crouch is a
    /// crouch-walk without either being a special case.
    /// </summary>
    public class ControlPad : MonoBehaviour, IMotorInputSource
    {
        const float k_ButtonSize = 150f;
        const float k_ActionSize = 145f;
        const float k_JumpSize = 175f;

        static readonly Color k_DirectionIdle = new Color(0.10f, 0.14f, 0.22f, 0.55f);
        static readonly Color k_DirectionHeld = new Color(0.30f, 0.62f, 0.95f, 0.80f);
        static readonly Color k_ActionIdle = new Color(0.16f, 0.12f, 0.22f, 0.55f);
        static readonly Color k_ActionHeld = new Color(0.85f, 0.55f, 0.25f, 0.85f);

        HoldButton m_Up;
        HoldButton m_Down;
        HoldButton m_Left;
        HoldButton m_Right;
        HoldButton m_Run;
        HoldButton m_Crouch;
        HoldButton m_Jump;
        HoldButton m_Climb;
        HoldButton m_Interact;

        bool m_JumpLatched;
        bool m_ClimbLatched;

        /// <summary>Raised when interact is pressed. Nothing consumes it yet; the actions it will drive are Milestone 9 and beyond.</summary>
        public event Action InteractPressed;

        /// <summary>Builds the controls under a canvas.</summary>
        public void Build(Transform canvas)
        {
            BuildDirectionPad(canvas);
            BuildActionButtons(canvas);
        }

        void BuildDirectionPad(Transform canvas)
        {
            // Cross centred low on the left, sized so a thumb can reach all four without
            // the hand shifting.
            Vector2 centre = new Vector2(240f, 300f);
            float reach = 152f;

            m_Up = CreateButton(canvas, "UP", "UP",
                new Vector2(0f, 0f), new Vector2(centre.x, centre.y + reach), k_ButtonSize,
                k_DirectionIdle, k_DirectionHeld);

            m_Down = CreateButton(canvas, "DOWN", "DOWN",
                new Vector2(0f, 0f), new Vector2(centre.x, centre.y - reach), k_ButtonSize,
                k_DirectionIdle, k_DirectionHeld);

            m_Left = CreateButton(canvas, "LEFT", "LEFT",
                new Vector2(0f, 0f), new Vector2(centre.x - reach, centre.y), k_ButtonSize,
                k_DirectionIdle, k_DirectionHeld);

            m_Right = CreateButton(canvas, "RIGHT", "RIGHT",
                new Vector2(0f, 0f), new Vector2(centre.x + reach, centre.y), k_ButtonSize,
                k_DirectionIdle, k_DirectionHeld);
        }

        void BuildActionButtons(Transform canvas)
        {
            Vector2 rightEdge = new Vector2(1f, 0f);

            m_Jump = CreateButton(canvas, "JUMP", "JUMP",
                rightEdge, new Vector2(-160f, 300f), k_JumpSize, k_ActionIdle, k_ActionHeld);

            m_Run = CreateButton(canvas, "RUN", "RUN",
                rightEdge, new Vector2(-350f, 250f), k_ActionSize, k_ActionIdle, k_ActionHeld);

            m_Crouch = CreateButton(canvas, "CROUCH", "CROUCH",
                rightEdge, new Vector2(-160f, 495f), k_ActionSize, k_ActionIdle, k_ActionHeld);

            m_Climb = CreateButton(canvas, "CLIMB", "CLIMB",
                rightEdge, new Vector2(-340f, 435f), k_ActionSize, k_ActionIdle, k_ActionHeld);

            m_Interact = CreateButton(canvas, "INTERACT", "USE",
                rightEdge, new Vector2(-160f, 665f), k_ActionSize, k_ActionIdle, k_ActionHeld);

            m_Jump.Pressed += delegate { m_JumpLatched = true; };
            m_Climb.Pressed += delegate { m_ClimbLatched = true; };
            m_Interact.Pressed += delegate
            {
                if (InteractPressed != null)
                    InteractPressed();
            };
        }

        HoldButton CreateButton(Transform parent, string name, string label,
            Vector2 anchor, Vector2 position, float size, Color idle, Color held)
        {
            RectTransform rect = UIFactory.CreateRect(parent, name);

            Image background = rect.gameObject.AddComponent<Image>();
            background.color = idle;

            UIFactory.Anchor(rect, anchor, new Vector2(0.5f, 0.5f), position, new Vector2(size, size));

            Text text = UIFactory.CreateText(rect, "Label", label, 30, Palette.UiText, TextAnchor.MiddleCenter);
            UIFactory.Stretch(text.rectTransform);

            HoldButton button = rect.gameObject.AddComponent<HoldButton>();
            button.Configure(background, idle, held);
            return button;
        }

        /// <summary>
        /// Reads one frame of intent. One-shot presses are cleared here, so this must be
        /// called exactly once per frame by the controller.
        /// </summary>
        public MotorInput Read()
        {
            MotorInput input = new MotorInput();

            // Opposite directions held together cancel, which is what a physical d-pad does.
            input.MoveZ = Axis(m_Up, m_Down);
            input.MoveX = Axis(m_Right, m_Left);

            input.Run = IsHeld(m_Run);
            input.Crouch = IsHeld(m_Crouch);

            input.JumpPressed = m_JumpLatched;
            input.ClimbPressed = m_ClimbLatched;

            m_JumpLatched = false;
            m_ClimbLatched = false;

            return input;
        }

        static float Axis(HoldButton positive, HoldButton negative)
        {
            float value = 0f;
            if (IsHeld(positive))
                value += 1f;
            if (IsHeld(negative))
                value -= 1f;
            return value;
        }

        static bool IsHeld(HoldButton button)
        {
            return button != null && button.IsHeld;
        }

        /// <summary>Shows or hides the whole control cluster, for the phases with nothing to control.</summary>
        public void SetVisible(bool visible)
        {
            SetActive(m_Up, visible);
            SetActive(m_Down, visible);
            SetActive(m_Left, visible);
            SetActive(m_Right, visible);
            SetActive(m_Run, visible);
            SetActive(m_Crouch, visible);
            SetActive(m_Jump, visible);
            SetActive(m_Climb, visible);
            SetActive(m_Interact, visible);
        }

        static void SetActive(HoldButton button, bool active)
        {
            if (button != null)
                button.gameObject.SetActive(active);
        }
    }
}
