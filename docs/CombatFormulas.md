# Combat Formulas

Combat in this MOBA is driven by a simple yet flexible **Ratio–Slider–Base (RSB)** model combined with a defense curve.  The formulas draw inspiration from games like Pokémon UNITE【77869965928650†L52-L74】【77869965928650†L85-L99】 and allow designers to tune damage and survivability via exposed parameters.

## Raw Damage (RSB)

Every damaging ability defines three coefficients: **Ratio (`R`)**, **Slider (`S`)** and **Base (`B`)**.  Given an attacker’s relevant stat (`AttackStat`) and level (`Level`), raw damage is computed as:

```
rawDamage = floor( R × AttackStat + S × (Level – 1) + B )
```

This formula mirrors the one used for physical and special moves in Pokémon UNITE, where the ratio scales with the attacker’s stat, the slider adds per level, and the base ensures a minimum damage floor【77869965928650†L52-L74】.  Designers adjust the coefficients in `AbilityDef` to achieve the desired scaling.

## Mitigation (Defense)

Raw damage is mitigated by the defender’s **Defense** stat according to the curve:

```
damageTaken = floor( rawDamage × 600 / ( 600 + Defense ) )
```

The 600 constant produces diminishing returns: increasing defense always reduces damage taken, but the benefit of each additional point decreases.  This same curve is used for physical and special defense in Pokémon UNITE【77869965928650†L85-L99】.  Adjusting the constant tilts the balance between MaxHP and Defense.

### Optional Flat Damage Reduction

After defense mitigation, certain buffs may apply a flat reduction percentage.  This is implemented as:

```
damageTaken = floor( damageTaken × (1 – flatReduction) )
```

Multiple reductions stack additively on the percentage.  For example, a 35 % reduction from Giga Drain and another 35 % reduction from Aurora Veil combine for a 70 % reduction【77869965928650†L106-L119】.

## Effective HP

Effective HP (eHP) expresses survivability in terms of MaxHP.  It is defined as:

```
EffectiveHP = MaxHP × ( 1 + Defense / 600 )
```

Deriving this formula from the mitigation curve shows that a unit with 200 defense and 10 000 HP has an effective HP of 13 333【77869965928650†L121-L129】.  Designers can adjust MaxHP and Defense together to achieve a desired time‑to‑kill (TTK) across archetypes.

## Design Levers

The combat system exposes multiple levers for tuning:

* **Ratio (`R`)** – determines how strongly damage scales with the attacker’s stat.
* **Slider (`S`)** – adds pressure to level up; larger values increase per‑level gains.
* **Base (`B`)** – sets a minimum damage floor so abilities remain useful at low stats.
* **Defense constant (600)** – changes the steepness of the mitigation curve; decreasing it makes defense more impactful.
* **Flat reductions** – grants special buffs that temporarily reduce damage after mitigation.

By adjusting these parameters in ScriptableObjects designers can quickly iterate on combat feel without code changes.  Unit tests should verify that damage calculations match the formulas to avoid regression.
