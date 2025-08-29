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

### Core Architecture (~75 Production Scripts)
- **FSM Runtime**: `Assets/Scripts/Core/IState.cs`, `Assets/Scripts/Core/StateMachine.cs`
- **Tick Management**: `Assets/Scripts/Core/TickManager.cs` (50Hz deterministic simulation)
- **Input Abstraction**: `Assets/Scripts/Core/IInputSource.cs`, `Assets/Scripts/Input/UnityInputSource.cs`

### Controllers (11 files)
- **Primary Locomotion**: `Assets/Scripts/Controllers/UnifiedLocomotionController.cs` ✅
- **Legacy Locomotion**: `Assets/Scripts/Controllers/LocomotionController.cs` ⚠️ *Deprecated*
- **Abilities**: `Assets/Scripts/Controllers/AbilityController.cs`, `Assets/Scripts/Controllers/EnhancedAbilityController.cs`
- **Scoring**: `Assets/Scripts/Controllers/ScoringController.cs`
- **Physics Integration**: `Assets/Scripts/Controllers/PhysicsLocomotionController.cs`

### Data Layer (8 ScriptableObjects)
- `Assets/Scripts/Data/BaseStatsTemplate.cs` - Player archetypes
- `Assets/Scripts/Data/AbilityDef.cs` - RSB coefficients & ability data
- `Assets/Scripts/Data/JumpPhysicsDef.cs` - Deterministic jump physics
- `Assets/Scripts/Data/ScoringDef.cs` - Channel times & team synergy
- `Assets/Scripts/Data/UltimateEnergyDef.cs` - Ultimate energy system
- `Assets/Scripts/Data/MatchModeDef.cs` - Match configuration
- `Assets/Scripts/Data/PlayerContext.cs` - Runtime player state

### Systems
- **Combat**: `Assets/Scripts/Combat/CombatSystem.cs` (RSB formulas)
- **Networking**: `Assets/Scripts/Networking/` (3 files - prediction & messages)
- **Spawn Pipeline**: `Assets/Scripts/Spawn/SpawnMachine.cs`
- **Energy**: `Assets/Scripts/Energy/UltimateEnergySystem.cs`

### Quality Assurance
- **Tests**: 24 files (Editor + PlayMode)
- **Tools**: 8 development utilities
- **Demo**: 12 integrated demo systems

> Determinism, data-driven design, and explicit FSMs are non-negotiable pillars. See TDD for tick discipline and testing.
