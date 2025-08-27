# Contributing & Coding Guidelines

## Non-Negotiables
- **Determinism**: Pass `dt` explicitly; no `Time.deltaTime` inside logic; RNG behind seedable interfaces.
- **No direct input polling inside state logic** (inject input commands; target: refactor `LocomotionController`).
- **Data-Driven**: Read gameplay numbers from ScriptableObjects.
- **FSM Discipline**: Transitions only via explicit events; log transitions with reasons.

## Style & Structure
- C# Unity style; self-documenting names; pure logic in controllers; physics elsewhere.
- Tests: write unit tests for math; playmode tests for FSM paths; lock against regressions.

## PR Checklist
- Links added to Docs/README.md if new systems/components introduced.
- Tests cover new math or transitions.
- Determinism checklist passed (no frame-time branches, no hidden time sources).
