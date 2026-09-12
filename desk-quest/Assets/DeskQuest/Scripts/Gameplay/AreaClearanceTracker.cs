using UnityEngine;

namespace DeskQuest.Gameplay
{
    /// <summary>
    /// Decides whether a patch of real desk has been cleared, from a stream of coverage
    /// readings between 0 and 1.
    ///
    /// This is the judgement at the heart of the cleaning mechanic, and it is deliberately
    /// separate from the AR code that produces the readings. Coverage from plane geometry
    /// is noisy: it jumps as the tracker refines its estimate, and it climbs simply because
    /// the player looked at a patch properly for the first time. Deciding well from a noisy
    /// signal is the hard part, and keeping it here as plain logic means it can be tested
    /// against sequences that would take a real desk and a patient person to reproduce.
    ///
    /// Three rules, each earning its place:
    ///
    /// - Nothing clears without a baseline. The patch must first read as occupied. Without
    ///   this, an obstacle placed over already-visible desk clears itself immediately.
    /// - Coverage must stay high, not merely touch high once. A single frame above the
    ///   threshold is noise.
    /// - Clearing happens once. After that the tracker stops caring.
    /// </summary>
    public class AreaClearanceTracker
    {
        readonly float m_OccupiedThreshold;
        readonly float m_ClearedThreshold;
        readonly float m_HoldSeconds;

        /// <summary>Most recent coverage reading, from 0 to 1.</summary>
        public float Coverage { get; private set; }

        /// <summary>True once the patch has been seen as occupied, which arms the tracker.</summary>
        public bool HasBaseline { get; private set; }

        /// <summary>Seconds coverage has been continuously above the cleared threshold.</summary>
        public float HeldFor { get; private set; }

        /// <summary>True once the patch has been judged cleared. Never goes back to false.</summary>
        public bool IsCleared { get; private set; }

        /// <summary>
        /// How close this patch is to clearing, from 0 to 1, for showing the player that
        /// what they are doing is working. Zero while unarmed, because progress towards a
        /// decision that cannot yet be made would be a lie.
        /// </summary>
        public float Progress
        {
            get
            {
                if (IsCleared)
                    return 1f;

                if (!HasBaseline || m_HoldSeconds <= 0f)
                    return 0f;

                return Mathf.Clamp01(HeldFor / m_HoldSeconds);
            }
        }

        /// <param name="occupiedThreshold">Coverage at or below which the patch reads as occupied.</param>
        /// <param name="clearedThreshold">Coverage at or above which the patch reads as clear.</param>
        /// <param name="holdSeconds">How long it must read as clear before the decision is made.</param>
        public AreaClearanceTracker(float occupiedThreshold, float clearedThreshold, float holdSeconds)
        {
            m_OccupiedThreshold = occupiedThreshold;
            m_ClearedThreshold = clearedThreshold;
            m_HoldSeconds = holdSeconds;
        }

        /// <summary>
        /// Feeds in one reading. Returns true on the single call where the patch becomes
        /// cleared, and false every other time, so the caller can act once without
        /// tracking edges itself.
        /// </summary>
        public bool Observe(float coverage, float deltaTime)
        {
            Coverage = Mathf.Clamp01(coverage);

            if (IsCleared)
                return false;

            if (!HasBaseline)
            {
                if (Coverage <= m_OccupiedThreshold)
                    HasBaseline = true;

                // Time spent above the threshold before the patch was ever seen as
                // occupied does not count towards clearing it.
                HeldFor = 0f;
                return false;
            }

            if (Coverage >= m_ClearedThreshold)
                HeldFor += deltaTime;
            else
                HeldFor = 0f;

            if (HeldFor < m_HoldSeconds)
                return false;

            IsCleared = true;
            return true;
        }

        /// <summary>Puts the tracker back to its starting state, for replaying a level.</summary>
        public void Reset()
        {
            Coverage = 0f;
            HasBaseline = false;
            HeldFor = 0f;
            IsCleared = false;
        }
    }
}
