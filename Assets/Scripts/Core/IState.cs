using UnityEngine;

namespace MOBA.Core
{
    /// <summary>
    /// Interface for finite state machine states.  Each state has lifecycle
    /// methods that are invoked when the state is entered, ticked and exited.
    /// Implementations should be lightweight and free of side effects outside
    /// of their intended responsibility.
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Called when the state becomes active.  Use this method to
        /// initialize stateful values or register callbacks.
        /// </summary>
        void Enter();

        /// <summary>
        /// Called every simulation tick while the state is active.  The
        /// dt parameter is the fixed timestep configured on the server.
        /// </summary>
        /// <param name="dt">Delta time in seconds.</param>
        void Tick(float dt);

        /// <summary>
        /// Called when the state is about to be deactivated.  Use this
        /// method to clean up stateful data or unregister callbacks.
        /// </summary>
        void Exit();
    }
}