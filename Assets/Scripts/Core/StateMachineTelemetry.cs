namespace MOBA.Core
{
    /// <summary>
    /// Telemetry data for state machine performance and debugging.
    /// </summary>
    [System.Serializable]
    public struct StateMachineTelemetry
    {
        public string Name;
        public string PlayerId;
        public string CurrentState;
        public string LastTransitionReason;
        public string LastValidationError;
        public int FailedTransitions;
    }
}
