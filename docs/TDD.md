# Technical Design Document (TDD)

## Architecture Overview

The game employs a **server‑authoritative, predictive client architecture**.  All game logic – state machines, physics, combat and scoring – runs on a server tick loop.  Clients send input commands containing analogue values and buttons; the server applies these inputs deterministically, simulates the world at a fixed rate, and returns snapshots.  To hide latency, clients use **client‑side prediction** and **server reconciliation**【221759658252397†L14-L46】.  They immediately simulate inputs locally, then correct any divergence when authoritative snapshots arrive.

### Deterministic Tick Loop

* The server runs a **fixed timestep** simulation (e.g. 50 Hz).  A constant delta time ensures the same behaviour on all machines and allows deterministic replay【410662956192693†L33-L53】.  Variable timesteps are avoided because they make physics behave differently at different frame rates and can cause the “spiral of death” under heavy load【410662956192693†L85-L97】.
* Clients also maintain a fixed simulation rate for prediction and interpolation.  Rendering is decoupled from simulation; interpolation smooths between authoritative snapshots.
* All random events use deterministic random seeds keyed by the tick count.  This ensures that clients can replay the server’s logic exactly when predicting.

### Update Order (Server)

The server performs the following steps on each tick:

1. **Gather inputs** – process queued input commands from clients; each input includes a sequence number and a bitmask of actions.
2. **Process FSMs** – update the locomotion, ability, scoring and other state machines in a deterministic order.  Each FSM uses `Enter`, `Tick` and `Exit` methods and may queue transitions.
3. **Physics step** – integrate velocities and resolve collisions.  Use the fixed timestep so results are deterministic【410662956192693†L33-L53】.
4. **Combat resolution** – apply RSB formulas to generate raw damage; mitigate damage using defense curves (600/(600 + Def))【77869965928650†L85-L99】; emit combat events for energy gains.
5. **Resource updates** – update ultimate energy accumulation; process cooldowns and energy spent.
6. **Snapshot broadcast** – package authoritative positions, velocities, FSM states and counters into a snapshot and send to clients along with the last processed input sequence number.

### Module Breakdown

* **StateMachine runtime** – a lightweight generic FSM that holds a current state and exposes `Change` and `Update` methods.  States implement `Enter`, `Tick` and `Exit`.  Each gameplay subsystem (locomotion, ability, scoring, spawn) has its own state machine using this runtime.  Because states are explicit and transitions are logged, behaviour is deterministic and testable.

* **LocomotionController** – reads movement and jump inputs, applies deterministic kinematics from `JumpPhysicsDef`, and transitions between `Grounded`, `Airborne`, `Knockback` and `Disabled` states.  When the player is knocked back, input is ignored until recovery.

* **AbilityController** – manages the ability FSM (`Idle → Casting → Executing → Cooldown`).  Casting can be cancelled by movement or damage before completion.  Each ability references its `AbilityDef` for cast time, cooldown, RSB coefficients and other data.  When `UltimateReady`, the ability FSM exposes an ultimate sub‑state.

* **ScoringController** – handles the scoring FSM (`Carrying → Channeling → Deposited/Interrupted`).  It uses `ScoringDef` to look up base channel times, additive speed factors and ally synergy multipliers.  During channeling, it monitors movement and damage events; if interrupted, it drops all carried points and transitions back to `Carrying`.  On successful deposit, it awards energy to the carrier per `UltimateEnergyDef` and resets carried points.

* **CombatSystem** – applies the RSB formula to compute raw damage and uses the 600/(600 + Def) curve to determine damage taken【77869965928650†L85-L99】.  It then fires combat events that other systems listen to (e.g. energy gains, kill feeds).  The system is stateless, so it can be unit‑tested easily.

* **UltimateEnergySystem** – tracks each player’s ultimate energy.  It accumulates energy from passive regen, combat events, scoring deposits and comebacks.  When energy ≥ ultimate requirement, it signals the ability system to enable the ultimate.  After an ultimate is used, it computes the cooldown as `energyRequirement / CooldownConstant`【77869965928650†L148-L149】 and locks out energy gain until the cooldown expires.

* **SpawnMachine** – orchestrates player spawning.  Its states include `Idle`, `InitialSetup`, `AssignBaseStats`, `ValidateStats`, `FinalizeSpawn` and `Error`.  It instantiates the player prefab, assigns stats from `BaseStatsTemplate`, clamps values, registers the entity with physics and network subsystems and broadcasts spawn events.  On any failure it transitions to `Error`, cleans up and allows retrying.

* **Networking** – includes message structs for `InputCmd`, `Snapshot` and `Event`.  Input commands include sequence numbers.  Snapshots contain positions, velocities, FSM state tags and other compressed data.  Events include kills, deposits and ultimates.  Messages are serialised with checksums for idempotence.

### Determinism & Data

* All balance and timing data live in ScriptableObjects.  The runtime reads values from `BaseStatsTemplate`, `AbilityDef`, `ScoringDef` and `UltimateEnergyDef` so there are no magic numbers.  ScriptableObjects are reusable data containers independent of scenes【507835656987322†L185-L194】.  Designers adjust values in the inspector to iterate rapidly without code changes【507835656987322†L196-L214】.  Multiple actors referencing the same asset share memory【507835656987322†L223-L231】.
* Physics uses a fixed timestep, not `Time.deltaTime`, so results do not depend on framerate【410662956192693†L33-L53】.  Branches based on real‑time or `deltaTime` are disallowed.  All random numbers use deterministic seeds keyed on tick count to support deterministic replay.

### Networking Messages

* **InputCmd(seq, inputs)** – sent from client to server each tick; contains a sequence number and input bits (movement vector, ability buttons, scoring command).  The server processes commands in order; out‑of‑order commands are queued and processed when their sequence arrives.
* **Snapshot(seqAck, state)** – sent from server to clients at a configurable rate (e.g. every second tick).  Includes the last processed input sequence (`seqAck`) and compressed state data: positions, velocities, FSM tags, HP, energy.  Clients store snapshots for interpolation.
* **Event(type, payload)** – broadcast from server when a discrete event occurs (kill, deposit, ultimate used, objective captured).  These events are deterministic and can be replayed offline.

### Client Prediction & Reconciliation

Clients do not wait for the server to respond before updating.  When a player presses movement or ability buttons, the client immediately simulates the action locally.  It stores a history of unacknowledged inputs.  When the server responds with a snapshot, it acknowledges inputs up to a specific sequence number and instructs the client to discard them.  The client then rewinds to the authoritative state, reapplies any remaining inputs and continues【221759658252397†L107-L128】.  This “roll back and replay” process ensures that the client’s state converges to the server’s state while hiding most corrections from the player.  To avoid visible pops, the client interpolates between states over a short window.

### Telemetry & Logs

* **FSM transition logs** – each state machine logs `(entityId, oldState, newState, reason, tick)` to a structured log.  These logs enable debugging and allow detection of invalid transitions.
* **Metrics** – the server collects statistics on average deposit time vs. predicted channelTime, interrupt rates, ultimate uptime and time‑to‑kill.  These metrics feed into dashboards to verify that matches align with tuning targets.

### Testing

Testing is divided into unit, integration and soak phases:

* **Unit tests** – verify formula implementations.  For example, tests for `CombatSystem` check that `rawDamage` and `damageTaken` follow the RSB and defense formulas【77869965928650†L52-L99】.  `ScoringSystem` tests compute channel time for various point brackets, additive factors and ally counts and compare them to theoretical values【77869965928650†L182-L197】.  `UltimateEnergySystem` tests verify energy accumulation, cooldown calculation and ultimate gating.
* **Playmode tests** – verify correct FSM transitions.  For locomotion, tests confirm that jumping sets the entity airborne, that knockback interrupts locomotion, and that disabled state overrides others.  Ability tests ensure casting can be cancelled by movement or damage and that cooldowns behave correctly.  Scoring tests check that carrying orbs enters the `Carrying` state, channeling begins when the deposit button is held, interruptions cancel channeling and orbs drop.
* **Deterministic replay** – record server inputs and random seeds for a match; replay offline to confirm that the simulation produces identical snapshots over thousands of ticks.  Any divergence indicates nondeterministic code.
* **Soak tests** – run matches with simulated bots for hours; measure illegal state transitions, deposit time errors (must be within ±5 % of the formula), and tick drift (must be zero over 10 k ticks).  The acceptance criteria for v0.1 require 0 % illegal transitions, deposit time error ≤5 % and perfect deterministic replay over 10 k ticks.

### Milestones & Deliverables

The MVP is broken into milestones that build on each other:

| Milestone | Timeline | Scope |
|---|---|---|
| **M1 (weeks 1–2)** | Implement the FSM runtime and the Locomotion system.  Write unit tests for deterministic jump physics and transitions. | FSM runtime, LocomotionController, base movement & jump physics. |
| **M2 (weeks 3–4)** | Implement the Ability FSM, RSB & defense formula, and energy events.  Add unit tests for damage and mitigation. | AbilityController, CombatSystem, UltimateEnergySystem base. |
| **M3 (week 5)** | Implement the Scoring FSM with ally synergy; handle interruptions; write scoring tests. | ScoringController, channel time computation, scoring events. |
| **M4 (week 6)** | Polish netcode: reconciliation, interpolation, and performance.  Conduct soak tests. | Networking layer, client prediction & reconciliation improvements, spawn pipeline integration. |

### Non‑Goals

The MVP intentionally excludes ranked play, cosmetics, advanced objectives, loadouts, replays and matchmaking.  These features will be considered after core systems are stable.

### Acceptance Criteria

* **State machine correctness** – All FSM transitions are logged; no illegal transitions occur in soak tests.
* **Scoring accuracy** – Measured deposit times are within ±5 % of the theoretical channel time across all point brackets and speed factor combinations.
* **Deterministic replay** – Replaying recorded tick inputs yields identical snapshots for at least 10 k ticks.
* **Netcode stability** – Under packet loss, clients recover gracefully; interpolation hides corrections; no simulation drift.
