# Player Spawn State Machine

## Purpose

Spawning a player into the world is more complex than instantiating a prefab.  The spawn pipeline must validate inputs, assign stats from data assets, compute derived values, register the entity with subsystems and gracefully handle errors.  A finite state machine (FSM) makes this pipeline deterministic, observable and recoverable.  Only one state is active at a time, and transitions occur via explicit events.

## States and Responsibilities

| State | Responsibilities |
|---|---|
| **Idle** | Wait for a `SpawnRequest(playerId, templateId, position, level, cosmetics…)` event.  Do nothing until a spawn request arrives. |
| **InitialSetup** | Instantiate the player root GameObject and attach a network object.  Set the seed for deterministic random numbers; place the entity at its spawn position; set the initial level.  On success, transition to **AssignBaseStats**; on failure, transition to **Error**. |
| **AssignBaseStats** | Copy data from the chosen `BaseStatsTemplate` into the player’s runtime stats.  Initialize inventory slots and resource counters.  On success, transition to **ValidateStats**; on failure, transition to **Error**. |
| **ValidateStats** | Clamp values to allowed ranges, compute derived attributes (e.g. regeneration rates, move speed) and apply match‑mode modifiers.  On success, transition to **FinalizeSpawn**; on failure, transition to **Error**. |
| **FinalizeSpawn** | Register the player with physics, AI, scoring and networking subsystems.  Broadcast spawn events to teammates and opponents.  Set the player’s state machines (locomotion, ability, scoring) to their initial states.  On success, enter the external “active” state; on failure, transition to **Error**. |
| **Error** | Centralised error handling.  Log the failure reason, clean up partially created objects and free resources.  Optionally allow a retry by transitioning back to **Idle** with corrected parameters. |

### Transition Summary

The transition graph is:

```
Idle --(SpawnRequest)--> InitialSetup
InitialSetup --(SetupComplete)--> AssignBaseStats
InitialSetup --(SetupError)--> Error
AssignBaseStats --(StatsAssigned)--> ValidateStats
AssignBaseStats --(AssignmentError)--> Error
ValidateStats --(ValidationOK)--> FinalizeSpawn
ValidateStats --(ValidationFailure)--> Error
FinalizeSpawn --(Spawned)--> [External Active]
FinalizeSpawn --(FinalizationError)--> Error
Error --(Retry)--> Idle
```

Each arrow represents an event that triggers the transition.  Only these events may cause state changes.  If no event occurs, the state machine remains in its current state.

## Implementation Notes

* Use the shared `IState` interface and `StateMachine` runtime.  Each state implements `Enter`, `Tick` and `Exit`.  Avoid embedding complex logic in the state machine; each state should perform only its designated work.
* Because asset loads or network registration may be asynchronous, states should be written to handle callbacks.  If asynchronous operations fail, they should call back with a specific error event.
* The `PlayerContext` holds references to the player’s data assets, runtime stats, inventory and network identifiers.  States mutate only fields relevant to them and must not leak side effects into other subsystems.
* Use an event queue to decouple the spawn FSM from the network and UI layers.  When an external system requests a spawn or signals completion/failure, it enqueues the corresponding event for the FSM to process.

## Pseudocode Skeleton

```csharp
public sealed class SpawnMachine : StateMachine
{
    public SpawnMachine(PlayerContext ctx)
    {
        // create states and pass context
        idle = new IdleState(ctx, this);
        setup = new InitialSetupState(ctx, this);
        assignStats = new AssignBaseStatsState(ctx, this);
        validate = new ValidateStatsState(ctx, this);
        finalize = new FinalizeSpawnState(ctx, this);
        error = new ErrorState(ctx, this);
        Change(idle);
    }

    private readonly IdleState idle;
    private readonly InitialSetupState setup;
    private readonly AssignBaseStatsState assignStats;
    private readonly ValidateStatsState validate;
    private readonly FinalizeSpawnState finalize;
    private readonly ErrorState error;

    public void OnSpawnRequest(SpawnRequest req) => Change(setup);
    public void OnSetupComplete() => Change(assignStats);
    public void OnSetupError() => Change(error);
    public void OnStatsAssigned() => Change(validate);
    public void OnAssignmentError() => Change(error);
    public void OnValidationOK() => Change(finalize);
    public void OnValidationFailure() => Change(error);
    public void OnSpawned() { /* external; leave FSM */ }
    public void OnFinalizationError() => Change(error);
    public void OnRetry() => Change(idle);
}
```

Each state performs its work in `Enter` or during `Tick`, then signals completion or failure via the event methods on `SpawnMachine`.

## Observability and Telemetry

* **Structured logging** – log each transition `(playerId, from, to, reason, tick)` with a reason code.  Include `templateId` and any error codes in the log entry for quick diagnosis.  This allows dashboards to visualise spawn success rates and reasons for failure.
* **Metrics** – measure spawn latencies (time spent in each state), error frequencies and retry rates.  Acceptance criteria require that ≤ 0.5 % of spawn attempts result in retries.

## Testing

* **Unit tests** – test each state in isolation.  For example, feed invalid template IDs into `AssignBaseStats` and assert that it transitions to **Error**.  Pass missing network registration into `FinalizeSpawn` and confirm it logs the appropriate error and does not leave orphaned objects.
* **Playmode tests** – run the entire spawn pipeline in a simulated match.  Introduce random failures (missing templates, network timeouts) and ensure that the system recovers cleanly.  Repeated retries must be idempotent.
* **Deterministic replay** – spawn the same player multiple times in deterministic replay and verify that the resulting snapshot (position, stats, inventory) is identical across runs.  This ensures that the spawn FSM and underlying systems are deterministic.

## Rationale and Extensibility

Explicit state machines remove tangled conditionals and make adding future steps trivial.  For example, adding a **CosmeticsLoad** state between `InitialSetup` and `AssignBaseStats` requires only creating a new state class and wiring transitions; existing states do not change.  Because transitions are explicit, designers can reason about the spawn pipeline and testers can verify each branch.  If new data must be validated (e.g. loadouts, tutorial progress), additional states can be inserted without modifying existing logic.
