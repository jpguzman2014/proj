using System.Collections.Generic;

namespace DeskQuest.Gameplay
{
    /// <summary>
    /// Supplies the current obstacle set to whatever needs to collide with it.
    ///
    /// The list is live: clearing an obstacle updates the entry in place, so the motor
    /// sees the change on its next step without anything having to notify it.
    /// </summary>
    public interface IObstacleProvider
    {
        List<ObstacleBox> Obstacles { get; }
    }
}
