using System;

/// <summary>
/// Simplified spawn state machine to illustrate the spawn pipeline for tests. Transitions
/// sequentially through setup, assignment, validation and finalization states.
/// </summary>
public class SpawnMachine
{
    public enum State { Idle, InitialSetup, AssignBaseStats, ValidateStats, FinalizeSpawn, Spawned, Error }
    private State _state = State.Idle;
    private readonly PlayerContext context;

    public bool Spawned => _state == State.Spawned;
    public bool IsFinished => _state == State.Spawned || _state == State.Error;

    public SpawnMachine(PlayerContext ctx)
    {
        context = ctx;
    }

    public void TriggerSpawn()
    {
        if (_state != State.Idle) return;
        _state = State.InitialSetup;
    }

    public void Update(float dt)
    {
        switch (_state)
        {
            case State.InitialSetup:
                // Perform any initialization logic here
                _state = State.AssignBaseStats;
                break;
            case State.AssignBaseStats:
                // Ensure base stats are assigned
                if (context.baseStats == null)
                {
                    _state = State.Error;
                    throw new InvalidOperationException("Missing BaseStatsTemplate");
                }
                _state = State.ValidateStats;
                break;
            case State.ValidateStats:
                // Perform validation of assigned stats
                _state = State.FinalizeSpawn;
                break;
            case State.FinalizeSpawn:
                // Finalize spawn process
                _state = State.Spawned;
                break;
        }
    }
}