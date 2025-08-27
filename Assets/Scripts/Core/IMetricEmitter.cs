/// <summary>
/// Interface for emitting metric events. Used to decouple telemetry from game logic.
/// </summary>
public interface IMetricEmitter
{
    void Emit(string eventName, object payload);
}