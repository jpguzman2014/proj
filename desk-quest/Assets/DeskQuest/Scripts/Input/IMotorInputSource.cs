using DeskQuest.Gameplay;

namespace DeskQuest.InputHandling
{
    /// <summary>
    /// Anything that can produce a frame of player intent.
    ///
    /// The character controller depends on this rather than on the on-screen pad, so the
    /// pad can be replaced or joined by something else, a gamepad or a test script, without
    /// the controller changing.
    /// </summary>
    public interface IMotorInputSource
    {
        /// <summary>
        /// Reads intent for this frame. Called once per frame; one-shot presses such as
        /// jump are consumed by the call, so calling it twice in a frame loses them.
        /// </summary>
        MotorInput Read();
    }
}
