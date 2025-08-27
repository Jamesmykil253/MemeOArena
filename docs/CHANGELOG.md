# Changelog

## v0.1.0 (MVP Complete - M1-M4)

### Core Systems Implemented ✅
- **FSM Runtime**: Complete IState/StateMachine with telemetry logging
- **Locomotion**: Input-injected movement with velocity computation
- **Abilities**: Energy regeneration and ultimate readiness gating
- **Combat**: RSB damage formulas with defense mitigation and critical hits  
- **Scoring**: Full FSM with synergy, speed factors, and energy rewards
- **Spawn**: State machine pipeline with comprehensive error handling
- **Data Layer**: All ScriptableObjects with proper MOBA.* namespacing

### Networking Foundation ✅
- **Message Structs**: InputCmd, Snapshot, GameEvent for client-server communication
- **Tick Management**: Deterministic fixed-timestep simulation with rollback support
- **Telemetry**: Structured logging for FSM transitions and gameplay metrics
- **Input System**: Abstracted interface with Unity InputSystem integration

### Testing & Quality ✅  
- **Unit Tests**: Formula validation, energy systems, scoring calculations
- **Integration Tests**: GameBootstrapper, FSM transitions, spawn pipeline
- **Namespace Consistency**: All code properly organized under MOBA.* hierarchy
- **Compilation**: Zero errors, ready for Unity 2023.3+ deployment

### Architecture Compliance ✅
- **Deterministic**: Fixed timestep, data-driven design, no magic numbers
- **FSM-Driven**: Explicit state machines for locomotion, abilities, scoring, spawn
- **Server-Authoritative**: Clean separation, input injection, snapshot synchronization  
- **Observable**: Structured logging, metrics collection, telemetry hooks

### Next Phase: M5 (Live Multiplayer)
- Client prediction and reconciliation
- Network transport layer
- Match coordinator and lobbies
- Performance optimization and stress testing

---

## v0.0.1 (Docs spine established)
- Added Docs/ index and architecture/formulas/contributing/glossary.
- Defined next steps for tests and input refactor (no code changes yet).
