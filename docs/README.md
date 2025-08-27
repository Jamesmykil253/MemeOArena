# Project Docs (Spine)

Authoritative index for design, tech, math, and workflows.

## Start Here
- **Game Design Document (GDD)** → `../GDD.md`
- **Technical Design Document (TDD)** → `../TDD.md`
- **Architecture Overview** → `./Architecture.md`
- **Core Formulas (Combat, eHP, Scoring)** → `./Formulas.md`
- **Spawn FSM Spec** → `../SpawnFSM.md`
- **Contributing & Coding Guidelines** → `./Contributing.md`
- **Glossary** → `./Glossary.md`
- **Changelog** → `./CHANGELOG.md`

## Source of Truth (Key Runtime Files)
- FSM Runtime: `../IState.cs`, `../StateMachine.cs`
- Controllers: `../LocomotionController.cs`, `../AbilityController.cs`, `../ScoringController.cs`
- Data (ScriptableObjects): `../BaseStatsTemplate.cs`, `../AbilityDef.cs`, `../JumpPhysicsDef.cs`, `../ScoringDef.cs`, `../UltimateEnergyDef.cs`, `../MatchModeDef.cs`
- Player Runtime: `../PlayerContext.cs`
- Spawn Machine: `../SpawnMachine.cs`

> Determinism, data-driven design, and explicit FSMs are non-negotiable pillars. See TDD for tick discipline and testing.
