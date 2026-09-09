using UnityEngine;

namespace DeskQuest.Audio
{
    /// <summary>
    /// The prototype's sound effects: the five the brief asks for, plus a rejection blip
    /// so the climb button is never silent when there is nothing to climb.
    ///
    /// Every clip is generated on first use rather than at start-up, so a run that never
    /// reaches the goal never spends the memory or the milliseconds on the fanfare.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class GameAudio : MonoBehaviour
    {
        [Range(0f, 1f)]
        [SerializeField] float m_Volume = 0.5f;

        AudioSource m_Source;

        AudioClip m_ButtonClip;
        AudioClip m_JumpClip;
        AudioClip m_ClearedClip;
        AudioClip m_GoalClip;
        AudioClip m_CompleteClip;
        AudioClip m_RejectedClip;

        void Awake()
        {
            m_Source = GetComponent<AudioSource>();
            m_Source.playOnAwake = false;
            m_Source.loop = false;
        }

        public void PlayButton()
        {
            if (m_ButtonClip == null)
                m_ButtonClip = ToneGenerator.Build("Button",
                    new[] { ToneGenerator.Note.At(880f, 0f, 0.05f, 0.35f) }, 0.06f);

            Play(m_ButtonClip);
        }

        public void PlayJump()
        {
            if (m_JumpClip == null)
                m_JumpClip = ToneGenerator.BuildSweep("Jump", 320f, 760f, 0.16f, 0.45f);

            Play(m_JumpClip);
        }

        /// <summary>Played when an obstacle is cleared: a rising two-note step.</summary>
        public void PlayObstacleCleared()
        {
            if (m_ClearedClip == null)
                m_ClearedClip = ToneGenerator.Build("Cleared", new[]
                {
                    ToneGenerator.Note.At(523.25f, 0f, 0.12f, 0.5f),
                    ToneGenerator.Note.At(783.99f, 0.10f, 0.20f, 0.5f)
                }, 0.32f);

            Play(m_ClearedClip);
        }

        public void PlayGoalReached()
        {
            if (m_GoalClip == null)
                m_GoalClip = ToneGenerator.Build("Goal", new[]
                {
                    ToneGenerator.Note.At(659.25f, 0.00f, 0.12f, 0.5f),
                    ToneGenerator.Note.At(783.99f, 0.10f, 0.12f, 0.5f),
                    ToneGenerator.Note.At(1046.50f, 0.20f, 0.26f, 0.55f)
                }, 0.5f);

            Play(m_GoalClip);
        }

        /// <summary>The level-complete fanfare: a major chord walked up and held.</summary>
        public void PlayLevelComplete()
        {
            if (m_CompleteClip == null)
                m_CompleteClip = ToneGenerator.Build("Complete", new[]
                {
                    ToneGenerator.Note.At(523.25f, 0.00f, 0.16f, 0.45f),
                    ToneGenerator.Note.At(659.25f, 0.14f, 0.16f, 0.45f),
                    ToneGenerator.Note.At(783.99f, 0.28f, 0.16f, 0.45f),
                    ToneGenerator.Note.At(1046.50f, 0.42f, 0.45f, 0.5f),
                    ToneGenerator.Note.At(783.99f, 0.42f, 0.45f, 0.3f),
                    ToneGenerator.Note.At(659.25f, 0.42f, 0.45f, 0.3f)
                }, 0.95f);

            Play(m_CompleteClip);
        }

        /// <summary>A short low blip for an action that could not be performed.</summary>
        public void PlayRejected()
        {
            if (m_RejectedClip == null)
                m_RejectedClip = ToneGenerator.Build("Rejected",
                    new[] { ToneGenerator.Note.At(160f, 0f, 0.10f, 0.3f) }, 0.12f);

            Play(m_RejectedClip);
        }

        void Play(AudioClip clip)
        {
            if (clip == null || m_Source == null)
                return;

            m_Source.PlayOneShot(clip, m_Volume);
        }
    }
}
