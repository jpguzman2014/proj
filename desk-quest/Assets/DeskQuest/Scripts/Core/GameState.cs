namespace DeskQuest.Core
{
    /// <summary>
    /// States for Milestone 1. Later milestones add Playing and Complete after Placed;
    /// the flow up to Placed does not change when they do.
    /// </summary>
    public enum GameState
    {
        /// <summary>AR session is starting, or the device has not reported support yet.</summary>
        Initializing,

        /// <summary>Session is running and looking for a horizontal surface.</summary>
        Scanning,

        /// <summary>A large enough horizontal surface exists; waiting for the player to tap it.</summary>
        SurfaceFound,

        /// <summary>The play area is anchored to the chosen surface.</summary>
        Placed,

        /// <summary>The device cannot run AR, or the session failed. Terminal for this run.</summary>
        Unsupported
    }
}
