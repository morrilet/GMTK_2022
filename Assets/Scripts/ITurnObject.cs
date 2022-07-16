/// <summary>
/// An object in the world that interacts with the turn system to queue actions.
/// </summary>
public interface ITurnObject {
    public void QueueTurn();  // Queue the objects turn actions with the turn manager.
}