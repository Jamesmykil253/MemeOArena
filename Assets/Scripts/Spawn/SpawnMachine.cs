using System;
using MOBA.Core;
using MOBA.Data;

namespace MOBA.Spawn
{
    /// <summary>
    /// Simplified spawn state machine to illustrate the spawn pipeline for tests. Transitions
    /// sequentially through setup, assignment, validation and finalization states.
    /// </summary>
    public class SpawnMachine
    {
        public enum State { Idle, InitialSetup, AssignBaseStats, ValidateStats, FinalizeSpawn, Spawned, Error }
        
        private State _state = State.Idle;
        private readonly PlayerContext context;
        private string lastError = "";

        public bool Spawned => _state == State.Spawned;
        public bool IsFinished => _state == State.Spawned || _state == State.Error;
        public string LastError => lastError;

        public SpawnMachine(PlayerContext ctx)
        {
            context = ctx;
        }

        public void SpawnRequest()
        {
            if (_state != State.Idle) return;
            _state = State.InitialSetup;
        }

        public void SetupError()
        {
            lastError = "Setup failed";
            _state = State.Error;
        }

        public void AssignmentError()
        {
            lastError = "Assignment failed";
            _state = State.Error;
        }

        public void ValidationFailure()
        {
            lastError = "Validation failed";
            _state = State.Error;
        }

        public void FinalizationError()
        {
            lastError = "Finalization failed";
            _state = State.Error;
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
                        AssignmentError();
                        return;
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
}