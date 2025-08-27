# Scoring Formulas

## Purpose

Scoring is the act of converting collected orbs into points by **channeling** on a scoring pad.  Channeling takes time, and the time required scales with the number of points carried, buffs that increase scoring speed and the presence of allies.  The goal of these formulas is to provide designers with a data‑driven mechanism for tuning deposit times and encouraging team play.

## Baseline Channel Times

Base times are defined in `ScoringDef` as a lookup table keyed by the points carried.  Designers can adjust these values without modifying code.  A typical starting baseline might be:

| Points Carried | Base Time (seconds) |
|---:|---:|
| 1–6 | 0.5 |
| 7–12 | 1.0 |
| 13–18 | 1.5 |
| 19–24 | 2.0 |
| 25–33 | 3.0 |

These values echo the approximate scoring times in Pokémon UNITE, where depositing 1–6 points takes about 0.5 s and 25–33 points takes about 3 s【77869965928650†L216-L223】.  The breakpoints and durations are completely configurable via the ScriptableObject.

## Speed Factors (Additive on Speed)

Scoring buffs (e.g. items, map objectives) affect **scoring speed**, not time.  Each buff contributes an **additive factor** to the numerator of a fraction.  According to the Unite‑DB FAQ, scoring speed is treated as `1/1`, and adding factors adds to the numerator【77869965928650†L182-L195】.  For example, the Goal Getter item adds +1, doubling the scoring speed; the Rayquaza buff adds +3, quadrupling speed【77869965928650†L182-L197】.  The formula to compute the **speed multiplier** is:

```
speedMultiplier = 1 + Σ(additiveFactors)
```

If multiple buffs apply, sum their factors.  For example, Goal Getter (+1) and Rayquaza (+3) together yield a factor of +4, so scoring speed becomes 5 × the base.  To convert this into channel time, divide the base time by the speed multiplier:

```
timeAfterSpeed = baseTime / speedMultiplier
```

Designers should cap the total additive factors to prevent extremes.

## Team Synergy

Allies standing on the scoring pad further reduce channel time.  This reduction is **multiplicative on time**, not additive on speed.  Unite‑DB notes that one ally reduces scoring time by roughly 30 %, two allies by 35 %, three allies by 40 % and four allies by 60 %【77869965928650†L205-L210】.  Let `teamSynergyMultiplier` be the time multiplier for the number of allies.  The final channel time is:

```
channelTime = ( baseTime / speedMultiplier ) × teamSynergyMultiplier
```

For example, depositing 20 points (base time 2 s) with the Goal Getter (+1 speed factor) and one ally present yields:

```
speedMultiplier = 1 + 1 = 2
timeAfterSpeed = 2.0 / 2 = 1.0 s
channelTime = 1.0 × 0.70 = 0.70 s
```

Designers can tune the synergy multipliers per ally in `ScoringDef`.  Encouraging grouping rewards teamwork but should not make solo scoring impossible.

## Interruptions

Channeling can be cancelled by movement, taking damage, being knocked back or defeated.  When a channel is interrupted, all carried orbs drop on the ground and can be picked up by anyone.  The scoring FSM handles interruption by transitioning from `Channeling` to `Interrupted` and then back to `Carrying` with zero points.

## Tuning Guidance

* **Generous synergy** – Team synergy multipliers should substantially reduce channel time.  This rewards grouping and encourages players to escort carriers.  Diminishing returns can apply to avoid making four‑person escorts mandatory.  The example multipliers (0.70, 0.65, 0.60, 0.40) provide such a diminishing series.
* **Caps on speed** – If additive speed factors stack too high, channel times can approach zero.  Designers should impose a cap on the total speed multiplier or include a minimum channel time to maintain risk.
* **Match pacing** – Use telemetry to measure actual deposit times versus theoretical predictions.  Adjust base times, additive factor sources and synergy multipliers to hit pacing targets (e.g. four deposits per player per 10 min match).
