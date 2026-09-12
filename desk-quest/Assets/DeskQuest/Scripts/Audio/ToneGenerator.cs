using UnityEngine;

namespace DeskQuest.Audio
{
    /// <summary>
    /// Builds short sound effects as waveforms at runtime.
    ///
    /// The brief said not to spend money on audio and to use placeholders if necessary.
    /// Generating tones is better than a placeholder: there are no audio files to license,
    /// import or ship, each clip is a few kilobytes of memory, and the pitch of a sound can
    /// be changed by editing a number rather than by finding a new asset.
    /// </summary>
    public static class ToneGenerator
    {
        const int k_SampleRate = 44100;

        /// <summary>One note in a generated clip.</summary>
        public struct Note
        {
            public float Frequency;
            public float StartSeconds;
            public float DurationSeconds;
            public float Volume;

            public static Note At(float frequency, float start, float duration, float volume = 0.6f)
            {
                Note note = new Note();
                note.Frequency = frequency;
                note.StartSeconds = start;
                note.DurationSeconds = duration;
                note.Volume = volume;
                return note;
            }
        }

        /// <summary>Renders a set of notes into a mono clip.</summary>
        public static AudioClip Build(string name, Note[] notes, float totalSeconds)
        {
            int sampleCount = Mathf.Max(1, (int)(totalSeconds * k_SampleRate));
            float[] samples = new float[sampleCount];

            for (int n = 0; n < notes.Length; n++)
            {
                Note note = notes[n];
                int start = (int)(note.StartSeconds * k_SampleRate);
                int length = (int)(note.DurationSeconds * k_SampleRate);

                for (int i = 0; i < length; i++)
                {
                    int index = start + i;
                    if (index < 0 || index >= sampleCount)
                        continue;

                    float t = i / (float)k_SampleRate;
                    float progress = i / (float)length;

                    samples[index] += Mathf.Sin(2f * Mathf.PI * note.Frequency * t)
                                      * Envelope(progress)
                                      * note.Volume;
                }
            }

            // Clip rather than let a sum of overlapping notes wrap round into a crackle.
            for (int i = 0; i < sampleCount; i++)
                samples[i] = Mathf.Clamp(samples[i], -1f, 1f);

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, k_SampleRate, false);
            if (clip != null)
                clip.SetData(samples, 0);

            return clip;
        }

        /// <summary>
        /// A short rise and a longer fall. Starting a tone at full volume produces an
        /// audible click on every play, which is the usual giveaway of generated audio.
        /// </summary>
        static float Envelope(float progress)
        {
            const float attack = 0.06f;
            if (progress < attack)
                return progress / attack;

            float remaining = (progress - attack) / (1f - attack);
            return 1f - remaining * remaining;
        }

        /// <summary>A tone that slides from one pitch to another, for jumps and falls.</summary>
        public static AudioClip BuildSweep(string name, float fromHz, float toHz, float seconds, float volume)
        {
            int sampleCount = Mathf.Max(1, (int)(seconds * k_SampleRate));
            float[] samples = new float[sampleCount];
            float phase = 0f;

            for (int i = 0; i < sampleCount; i++)
            {
                float progress = i / (float)sampleCount;
                float frequency = Mathf.Lerp(fromHz, toHz, progress);

                // Integrating frequency into a running phase, rather than recomputing
                // sin(2*pi*f*t) each sample, keeps the waveform continuous as the pitch
                // moves. Recomputing produces a rasp instead of a slide.
                phase += 2f * Mathf.PI * frequency / k_SampleRate;
                samples[i] = Mathf.Sin(phase) * Envelope(progress) * volume;
            }

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, k_SampleRate, false);
            if (clip != null)
                clip.SetData(samples, 0);

            return clip;
        }
    }
}
