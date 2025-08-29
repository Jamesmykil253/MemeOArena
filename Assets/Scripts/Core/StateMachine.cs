using System;
using UnityEngine;

namespace MOBA.Core
{
    /// <summary>
    /// Enhanced finite state machine with validation, telemetry, and error handling.
    /// The machine delegates per‑tick updates to the active state's Tick method and 
    /// coordinates transitions via the Change method. All state transitions are validated
    /// for correctness and logged for debugging and telemetry purposes.
    /// </summary>
    public class StateMachine
    {
        /// <summary>
        /// The currently active state.  Can be null if the machine has not
        /// been started yet.
        /// </summary>
        public IState Current { get; private set; }

        /// <summary>
        /// The reason for the most recent state transition.  This is optional
        /// and may be null if no reason was provided.
        /// </summary>
        public string LastTransitionReason { get; private set; }

        /// <summary>
        /// Name of this state machine for logging purposes.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Player ID associated with this state machine for telemetry.
        /// </summary>
        public string PlayerId { get; private set; }

        /// <summary>
        /// Validation system for state transitions.
        /// </summary>
        public StateTransitionValidator Validator { get; private set; }

        /// <summary>
        /// The last validation error message, if any.
        /// </summary>
        public string LastValidationError { get; private set; }

        /// <summary>
        /// Count of failed transition attempts.
        /// </summary>
        public int FailedTransitions { get; private set; }

        public StateMachine(string name = "FSM", string playerId = "")
        {
            Name = name;
            PlayerId = playerId;
            Validator = new StateTransitionValidator();
            LastValidationError = null;
            FailedTransitions = 0;
        }

        /// <summary>
        /// Change to a new state with enhanced validation and error handling.
        /// If validation fails, the transition is rejected and the error is logged.
        /// </summary>
        /// <param name="next">The state to transition into.</param>
        /// <param name="reason">Optional reason string for debugging.</param>
        /// <returns>True if transition succeeded, false if validation failed.</returns>
        public bool Change(IState next, string reason = null)
        {
            // Basic null/same-state check
            if (ReferenceEquals(Current, next))
            {
                return true; // No-op, but not an error
            }
            
            if (next == null)
            {
                Debug.LogWarning($"[{Name}][{PlayerId}] Attempted transition to null state");
                return false;
            }

            // Validate transition
            if (!Validator.ValidateTransition(Current, next, reason, out string validationError))
            {
                LastValidationError = validationError;
                FailedTransitions++;
                
                Debug.LogError($"[{Name}][{PlayerId}] Invalid state transition: {validationError}");
                
                // In development, also log the stack trace for debugging
                if (Application.isEditor)
                {
                    Debug.LogError($"Transition context: {Current?.GetType().Name ?? "null"} -> {next.GetType().Name}");
                }
                
                return false;
            }

            // Clear previous validation error on successful validation
            LastValidationError = null;

            // Log the state transition for debugging
            string fromState = Current?.GetType().Name ?? "null";
            string toState = next.GetType().Name;
            
            // In development, log to Unity console
            if (Application.isEditor)
            {
                Debug.Log($"[{Name}][{PlayerId}] {fromState} -> {toState}" + 
                         (!string.IsNullOrEmpty(reason) ? $" ({reason})" : ""));
            }

            // Perform the transition
            try
            {
                LastTransitionReason = reason;
                Current?.Exit();
                Current = next;
                Current.Enter();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{Name}][{PlayerId}] Error during state transition: {ex.Message}");
                // Don't update Current if Enter() failed - stay in previous state
                return false;
            }
        }

        /// <summary>
        /// Update the active state.  If there is no active state this
        /// method does nothing.
        /// </summary>
        /// <param name="dt">Delta time in seconds.</param>
        public void Update(float dt)
        {
            try
            {
                Current?.Tick(dt);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{Name}][{PlayerId}] Error in state {Current?.GetType().Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Add a validation rule for specific state transitions.
        /// </summary>
        public void AddTransitionRule<TFrom, TTo>(StateTransitionValidator.ValidationRule rule)
            where TFrom : IState where TTo : IState
        {
            Validator.AddRule<TFrom, TTo>(rule);
        }

        /// <summary>
        /// Get telemetry data for this state machine.
        /// </summary>
        public StateMachineTelemetry GetTelemetry()
        {
            return new StateMachineTelemetry
            {
                Name = Name,
                PlayerId = PlayerId,
                CurrentState = Current?.GetType().Name ?? "null",
                LastTransitionReason = LastTransitionReason,
                LastValidationError = LastValidationError,
                FailedTransitions = FailedTransitions
            };
        }
    }
}
