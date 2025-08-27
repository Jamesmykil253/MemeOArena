# Architecture

## Core Principles
- **Server-authoritative, fixed-timestep simulation** (determinism; client prediction + reconciliation).
- **Explicit FSMs** for locomotion, abilities, scoring, spawn.
- **Data-driven** via ScriptableObjects (no magic numbers in code).
- **Injected inputs** in logic (no direct polling inside state logic for deterministic netplay).

## Runtime & FSM Spine
- FSM Runtime: `IState` (Enter/Tick/Exit), `StateMachine` (Change/Update).
- Major FSMs:
  - **LocomotionController**: Grounded ↔ Airborne, with Knockback and Disabled interrupts.
  - **AbilityController**: Idle → Casting → Executing → Cooldown (+Ultimate readiness gate).
  - **ScoringController**: Carrying → Channeling → (Deposited | Interrupted).
  - **SpawnMachine**: Idle → InitialSetup → AssignBaseStats → ValidateStats → FinalizeSpawn → (Spawned | Error).

## Tick Order (Server)
1) Gather inputs → 2) Update FSMs → 3) Physics integrate → 4) Combat resolution (RSB + defense) →
5) Resources (ult energy, cooldowns) → 6) Snapshot broadcast.

## Data (ScriptableObjects)
- Base stats (HP/Attack/Defense/MoveSpeed/Jump), Abilities (RSB), JumpPhysics, Scoring baselines & synergy, UltimateEnergy, MatchMode.

## Observability & Tests
- Structured FSM transition logs.
- Unit tests for formulas (Combat, eHP, Scoring).
- Playmode tests for FSM transitions & spawn pipeline.
