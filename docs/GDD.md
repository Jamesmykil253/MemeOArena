# Game Design Document (GDD)

## Vision

A **fast**, **readable**, team‑fight‑first multiplayer online battle arena (MOBA).  Decisions are meant to be clear, actions crisp, and victories earned through coordination and timing.  The game rewards players who farm efficiently, choose when to engage or disengage fights, coordinate with allies to deposit points, and control their ultimate usage.  The goal is to deliver a competitive yet approachable experience where mechanics are easy to read but difficult to master.

### Pillars

1. **Clarity over chaos.**  Combat readability is paramount.  Attacks and abilities have clear telegraphs, time‑to‑kill (TTK) bursts are short but allow counterplay, and there are no hidden modifiers.  Skills are designed with explicit state machines, so cancelling or interrupting an action always follows well‑defined rules rather than unpredictable “animation blending”.

2. **Deterministic feel.**  The simulation runs at a fixed tick rate on the server; updates are processed in discrete steps rather than depending on rendering framerate.  Using a fixed timestep ensures that the simulation behaves identically across machines【410662956192693†L33-L53】, avoids physics spiralling out of control at extreme delta times【410662956192693†L85-L97】, and makes the game deterministic and replayable.  Clients perform **client‑side prediction** and **server reconciliation**.  When the player sends an input, their local client predicts the outcome and displays it immediately.  Once the authoritative server responds, the client may need to correct its prediction and replay pending inputs【221759658252397†L14-L46】.  This technique eliminates most visible latency while preserving server authority.

3. **Data‑driven design via ScriptableObjects.**  Game parameters – base stats, ability coefficients, jump physics, match settings, scoring baselines, ultimate energy – live in **ScriptableObject** assets.  ScriptableObjects are Unity’s reusable data containers that exist in the project folder independent of any scene【507835656987322†L185-L194】.  Designers can modify values in the inspector without writing code【507835656987322†L196-L214】; all game systems read these assets at runtime so there are no hidden constants.  Multiple objects can reference the same asset without duplicating memory【507835656987322†L223-L231】.

4. **Low‑friction teamplay.**  The core loop encourages cooperation.  Allies who stand on the scoring pad with a carrier accelerate the deposit, and the scoring system rewards grouping with multiplicative reductions in channel time.  Communication tools are generous: quick‑pings, contextual emotes, and radial voice commands.  Abilities and ultimates often scale with ally proximity to encourage team‑fight positioning.

## Core Loop

The moment‑to‑moment loop is **Farm → Fight → Score → Regroup**.  Players collect orbs by defeating creeps and enemies.  Carrying many orbs increases the risk of dying before depositing; the more points you carry, the longer you must channel to deposit.  Fights occur when teams contest neutral objectives or catch carriers out of position.  Successful teams escort carriers to scoring pads, deposit orbs, regroup, and repeat.

## Data Model

All balance data lives in ScriptableObject assets.  Designers iterate by adjusting these assets in the Unity inspector; no code changes are required for balance tuning【507835656987322†L196-L214】.  The key assets are:

| Asset (SO) | Purpose |
|---|---|
| **BaseStatsTemplate** | Defines an archetype’s maximum HP, attack, defense, move speed and jump physics.  Multiple heroes can reference the same template to minimise duplication. |
| **AbilityDef** | Specifies cast time, cooldown, input keys, RSB coefficients (`R`, `S`, `B`), knockback values and other ability‑specific parameters. |
| **JumpPhysicsDef** | Stores deterministic jump parameters: initial velocity, gravity, coyote time, double‑jump windows.  The physics system samples these parameters on a fixed timestep. |
| **MatchModeDef** | Configures match length, team sizes and milestone events. |
| **ScoringDef** | Contains base channel times by points carried and additive speed factors.  It also stores ally synergy multipliers used to reduce channel time when allies assist. |
| **UltimateEnergyDef** | Defines passive energy regeneration, energy gains from combat and scoring, comeback energy on KO and the global constant used to convert an ultimate’s energy requirement into a cooldown. |

Because these assets are Unity ScriptableObjects, they persist as project files and can be shared across multiple scenes.  When designers adjust numbers, all systems referencing the asset automatically update【507835656987322†L203-L211】.

## Network Model

The game uses a **server‑authoritative** architecture.  All gameplay (movement, combat, scoring, ultimate usage) is simulated on the server.  Clients send input frames tagged with sequence numbers; the server processes the simulation on a fixed tick (e.g. 50 Hz) and sends periodic snapshots back to the clients.  To reduce perceived latency, clients perform **client‑side prediction** and **server reconciliation**【221759658252397†L14-L46】:

1. When a player presses a key, the client immediately applies the input locally, predicting the outcome.  This means movement and attacks feel responsive even though the server has not yet confirmed them.
2. Each input message carries a sequence identifier.  Server snapshots include the sequence number of the last processed input.  When a snapshot arrives, the client rewinds its local state to the authoritative state, discards acknowledged inputs and reapplies unacknowledged inputs【221759658252397†L107-L128】.
3. If the local prediction differs from the authoritative state, the client corrects the discrepancy and smoothly interpolates the difference over a small window.  This ensures the server remains the source of truth while hiding most corrections from players.

A fixed timestep is critical.  Simulation updates use a constant delta time (e.g. `1/50 s`), not the render frame time.  Using a fixed timestep ensures reproducibility【410662956192693†L33-L53】 and avoids the physics “spiral of death” that occurs when variable frame times accumulate【410662956192693†L85-L97】.  Even if the rendering framerate drops, the simulation remains stable.

Network messages are idempotent and include sequence numbers.  Input messages include pressed keys, analogue values and any stateful toggles.  Server snapshots contain positions, velocities, FSM states and counters.  Event messages (such as kills, deposits or ultimate usage) carry minimal data and are fully deterministic.

## Core State Machines

Explicit finite state machines (FSMs) control all gameplay subsystems.  States have `Enter`, `Tick` and `Exit` methods; transitions occur only through defined events, making behaviour deterministic and easy to reason about.  The major FSMs are:

* **Locomotion FSM** – States: `Grounded ↔ Airborne`, with interrupts to `Knockback` and a global `Disabled` state.  The controller reads movement and jump inputs, applies deterministic velocities from `JumpPhysicsDef` and transitions accordingly.

* **Ability FSM** – States: `Idle → Casting → Executing → Cooldown → Idle`.  Abilities can be cancelled by movement or damage during their cast time.  Each state consults `AbilityDef` values for timings and coefficients.

* **Scoring FSM** – States: `Carrying → Channeling → (Deposited | Interrupted)`.  When a player carries orbs they can begin channeling on a scoring pad.  Channeling is cancelled on movement or damage.  Ally proximity reduces channel time multiplicatively.

* **Spawn FSM** – States: `Idle → InitialSetup → AssignBaseStats → ValidateStats → FinalizeSpawn → (Spawned | Error)`.  The spawn pipeline instantiates the player root, assigns base stats from `BaseStatsTemplate`, clamps and derives stats, registers the entity with subsystems and handles errors gracefully.  An explicit `Error` state cleans up and allows retries.

* **Match Loop FSM** – States: `Lobby → Loading → MatchStart → InMatch → MatchEnd → Results`.  The match loop orchestrates team allocation, loading screens, timers and scoreboard display.

Because each subsystem has its own FSM with clear transitions, there are no hidden state changes.  Deterministic simulation requires that behaviour changes only through these transitions.

## Combat System

Damage uses a **Ratio–Slider–Base (RSB)** model.  When a hero uses an ability, the raw damage is calculated as:

```
rawDamage = floor(R * Attack + S * (Level – 1) + B)
```

where `R` is a scaling ratio, `S` is a per‑level slider and `B` is a flat base.  This formula is similar to how Pokémon UNITE calculates move damage【77869965928650†L52-L74】.  After raw damage is computed, **defense mitigation** reduces it according to:

```
damageTaken = floor( rawDamage * 600 / (600 + Defense) )
```

This diminishing‑returns curve is used in Pokémon UNITE【77869965928650†L85-L99】 and ensures that increasing defense meaningfully increases effective HP without making damage useless.  Designers can adjust the global constant (600) to tilt the balance between health and defense.  Optionally, further flat reductions may apply multiplicatively after defense.

### Effective HP

For balance purposes it is useful to convert defense into an effective HP multiplier.  Effective HP is defined as:

```
EffectiveHP = MaxHP * ( 1 + Defense / 600 )
```

The Pokémon UNITE FAQ derives the same relationship【77869965928650†L121-L129】.  This makes it easier to tune time‑to‑kill by adjusting base HP and defense together.

### Balance Levers

The RSB model exposes several levers:

* **Ratio (`R`)** – controls how strongly the ability scales with the attack stat.
* **Slider (`S`)** – controls level‑based scaling; larger values increase pressure to level up.
* **Base (`B`)** – ensures abilities have a minimum damage floor.
* **Defense constant (600)** – adjusting this constant makes defense more or less valuable globally.

Designers can use these levers in `AbilityDef` and `BaseStatsTemplate` to achieve target TTK for different classes.

## Scoring System

Scoring (depositing orbs) uses a data‑driven formula defined in `ScoringDef`.  Each bracket of points carried has a base channel time.  For example, the default baselines may be:

| Points carried | Base channel time (seconds) |
|---:|---:|
| 1–6 | 0.5 |
| 7–12 | 1.0 |
| 13–18 | 1.5 |
| 19–24 | 2.0 |
| 25–33 | 3.0 |

**Additive speed factors** apply on **speed**, not time.  When a player receives bonuses such as buffs or items, each bonus contributes an additive factor (`+1`, `+3`, etc.).  The Pokémon UNITE FAQ explains that scoring speed is treated as a fraction like `1/1`, and adding factors adds to the numerator【77869965928650†L182-L195】.  For example, the Goal Getter item adds a factor of 1 (doubling scoring speed), while the Rayquaza buff adds 3 (quadrupling scoring speed)【77869965928650†L182-L197】.  The total **speed multiplier** is computed as:

```
speedMultiplier = 1 + Σ(additiveFactors)
```

Channel time is then derived by dividing the base time by the speed multiplier and applying team synergy:

```
channelTime = baseTime / speedMultiplier × teamSynergyMultiplier
```

**Team synergy** is multiplicative on time.  Allies standing on the scoring pad reduce the channel time by a percentage.  According to Unite‑DB, one ally reduces score time by roughly 30 %; two allies by 35 %; three allies by 40 %; and four allies by 60 %【77869965928650†L205-L210】.  These values are configurable in `ScoringDef`.  The scoring FSM cancels channeling if the carrier moves or takes damage.

Designers should cap additive speed stacks to avoid extreme values.  Base times and synergy multipliers can be tuned to encourage grouping without making solo scoring impossible.

## Ultimate Energy

Ultimate abilities draw from a shared energy pool.  Energy sources include:

* **Passive regeneration** – players gain a small amount of energy each second (e.g. ~900 units per second in Pokémon UNITE【77869965928650†L148-L160】).
* **Combat** – dealing damage or assisting in kills awards energy.  The exact amount scales with damage dealt and is configurable in `UltimateEnergyDef`.
* **Scoring** – depositing orbs grants a large energy bonus; bigger deposits yield more energy【77869965928650†L148-L160】.
* **Comeback** – being defeated grants a portion of energy back to the defeated player (and optionally a smaller amount to the killer).  This keeps games competitive.

Each ultimate has an **energy requirement**.  After using the ultimate, energy resets to zero and a **cooldown** is enforced.  The cooldown in seconds is computed by dividing the energy requirement by a global constant (`CooldownConstant`)【77869965928650†L148-L149】:

```
ultimateCooldown = energyRequirement / CooldownConstant
```

Thus, adjusting the constant controls how quickly players can cycle ultimates relative to match length.  Energy modifiers from items or emblems apply multiplicatively to all gains.  The ability FSM includes a sub‑state `UltimateReady` when energy ≥ requirement; pressing the ultimate key triggers the ability, resets energy and enters the cooldown sub‑state.

## Content & UX

**Readability**.  Visual effects use a consistent language of colours and shapes; ability ranges are clearly telegraphed.  Palettes are colour‑blind safe.  Cast times are short but obvious.  Damage numbers and debuff icons display in unobtrusive ways.  Tooltips derive their text from ScriptableObjects to ensure consistency.

**Counterplay**.  Every “power moment” has at least one systemic counter.  For example, a channeling deposit can be interrupted by damage or movement; an ultimate can be dodged or baited; long‑range abilities have line‑of‑sight requirements.

**Onboarding**.  New players are introduced via a practice range that displays live numbers.  Tooltips and training missions highlight how RSB and defense interact and how to work with allies for scoring.

## Scope and MVP

The **MVP (v0.1)** focuses on delivering the core loop with minimal content:

* **Content** – one mirrored map, two archetypes and four abilities (two per hero).  There are no ranked modes, cosmetics, advanced objectives or replays.
* **Systems** – locomotion, ability and scoring FSMs; spawn pipeline; match loop; server‑authoritative netcode with client prediction; RSB and defense; scoring formula; ultimate energy; logging and metrics.
* **Tuning targets** – matches last 8–12 minutes; each player scores about four deposits; teams fire 1–2 ultimates per minute.  Designers will adjust `ScoringDef` and `UltimateEnergyDef` accordingly.

### Risks

* **Netcode reconciliation readability** – predictive clients can diverge from the server if the simulation is not perfectly deterministic.  Mitigation: use a small interpolation window, deterministic physics, and generous client aim assist.
* **Balance tuning** – making the scoring formula feel fair across all point brackets.  Mitigation: collect telemetry on deposit times versus theoretical predictions; adjust base times and synergy multipliers.
* **Onboard clarity** – players may struggle to understand additive speed and synergy.  Mitigation: provide UI feedback showing the current channel time and the contribution of allies and buffs.

## Glossary

* **Orb** – The unit of scoring; players collect orbs from creeps and enemy players and deposit them to earn points.
* **Channeling** – The stationary action to deposit orbs on a scoring pad; channeling can be interrupted by movement or damage.
* **Neutral Objective** – A creep or structure that grants buffs or points when defeated.
* **State Machine** – A model where an entity is in exactly one of a finite set of states, and transitions occur only through explicit events.
* **ScriptableObject** – A Unity asset type used to store shared data; it allows designers to edit values in the inspector and is independent of scenes【507835656987322†L185-L204】.
