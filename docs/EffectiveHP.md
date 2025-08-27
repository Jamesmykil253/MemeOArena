# Effective HP (eHP)

## Definition

**Effective HP** (eHP) measures how much raw damage a unit can survive when accounting for its defense.  It translates defense into an equivalent increase in maximum health, allowing designers to reason about survivability and time‑to‑kill.

## Formula

Given a unit with maximum HP (`MaxHP`) and defense (`Defense`), effective HP is:

```
EffectiveHP = MaxHP × ( 1 + Defense / 600 )
```

This relationship follows directly from the mitigation curve `damageTaken = rawDamage × 600/(600 + Defense)` used in Pokémon UNITE【77869965928650†L85-L99】.  When a unit’s defense increases, the fraction of raw damage that penetrates decreases, effectively increasing the HP pool by `Defense/600` of its original value.

## Example

Suppose a hero has 10 000 MaxHP and 200 Defense.  The effective HP is:

```
EffectiveHP = 10 000 × ( 1 + 200/600 ) = 10 000 × 1.333… ≈ 13 333
```

This matches the Pokémon UNITE FAQ example【77869965928650†L121-L132】 and means that, against attackers with no defense penetration, the hero can absorb 13 333 raw damage before dying.

## Design Use

Designers often target a desired time‑to‑kill (TTK) for each archetype.  By computing effective HP, they can adjust MaxHP and Defense together to hit the target.  Increasing MaxHP raises effective HP linearly; increasing Defense increases it by a fraction.  Adjusting the defense constant (600) tilts how valuable Defense is relative to HP.  Because mitigation uses diminishing returns, large defense values produce only incremental effective HP gains.  Balance decisions should consider both raw HP and defense when setting archetype templates.
