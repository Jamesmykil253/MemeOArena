using UnityEngine;

namespace MOBA.Core
{
    /// <summary>
    /// A simple finite state machine that holds a single active state.
    /// The machine delegates per‑tick updates to the active state's Tick
    /// method and coordinates transitions via the Change method.  States
    /// implementing <see cref="IState"/> should not reference the machine
    /// directly; the machine is responsible for invoking Enter and Exit.
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
        /// Change to a new state.  If the new state is the same as the
        /// current state the call is ignored.  Otherwise the current state's
        /// Exit method is invoked, the current reference is updated and
        /// Enter is invoked on the new state.
        /// </summary>
        /// <param name="next">The state to transition into.</param>
        /// <param name="reason">Optional reason string for debugging.</param>
        public void Change(IState next, string reason = null)
        {
            if (ReferenceEquals(Current, next) || next == null)
            {
                return;
            }
            // Update transition reason
            LastTransitionReason = reason;
            Current?.Exit();
            Current = next;
            Current.Enter();
        }

        /// <summary>
        /// Update the active state.  If there is no active state this
        /// method does nothing.
        /// </summary>
        /// <param name="dt">Delta time in seconds.</param>
        public void Update(float dt)
        {
            Current?.Tick(dt);
        }
    }
}
