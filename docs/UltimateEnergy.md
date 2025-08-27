# Ultimate Energy

## Goal

Ultimate abilities should feel powerful yet earned.  Players accumulate energy from various actions and strategically decide when to unleash their ultimate.  The system must be predictable so players can schedule ultimates around fights and deposits, and it must prevent runaway snowballing.

## Energy Sources

Energy gains are defined in `UltimateEnergyDef` and tuned via telemetry.  Example values inspired by Pokémon UNITE’s hidden mathematics include【77869965928650†L148-L160】:

| Event | Energy Gain (example) |
|---|---:|
| **Passive regeneration** | 900 per second |
| **Kill neutral** | 5 000 |
| **Score deposit** | 12 000 (independent of points) |
| **On KO (defeated player)** | 12 000 |

These numbers are illustrative; designers should adjust them so that teams collectively fire ~1–2 ultimates per minute in a 10‑minute match.  Energy gains may be multiplied by item or emblem bonuses (e.g. +6 % from Energy Amplifier【77869965928650†L148-L160】).

## Ultimate Cost and Cooldown

Each ultimate defines an **energy requirement** (e.g. 90 000).  When a player’s energy ≥ requirement, the ability FSM enters the `UltimateReady` sub‑state.  Upon activation, the ultimate consumes the requirement and resets energy to zero.  A **post‑ultimate cooldown** prevents immediate recharging.  The cooldown in seconds is computed as:

```
ultimateCooldown = energyRequirement / CooldownConstant
```

The Pokémon UNITE FAQ notes that dividing the requirement by 900 yields the cooldown in seconds【77869965928650†L148-L149】.  Designers can tune `CooldownConstant` (e.g. 900) to adjust how quickly ultimates recharge relative to energy gains.  The cooldown begins after the ultimate finishes; during this time, passive and event‑driven gains are either suppressed or stored until the cooldown ends, depending on design choice.

## FSM Integration

The **AbilityController** monitors the player’s `ultimateEnergy`.  When energy ≥ requirement, it unlocks the `UltimateReady` sub‑state.  Pressing the ultimate input triggers the ability, transitions the ability FSM to an `Executing` state for the ultimate, deducts the energy requirement, sets a cooldown timer and emits an `UltimateUsed` event.  After the cooldown timer expires, the player can once again accumulate energy.

On scoring deposits, the scoring FSM must award energy before resetting carried points, ensuring that players who score contribute to their ultimate progression.  Combat events also contribute to energy via the combat system.  Items or emblems that modify energy gains are applied multiplicatively to all sources.
