namespace DeskQuest.Core
{
    /// <summary>
    /// The flow from opening the app to finishing a level.
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

        /// <summary>The level is running: the player is moving the character and tidying the desk.</summary>
        Playing,

        /// <summary>The character reached the treasure.</summary>
        Complete,

        /// <summary>The device cannot run AR, or the session failed. Terminal for this run.</summary>
        Unsupported
    }
}
