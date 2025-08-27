# Core Formulas (Authoritative)

## Combat: RSB → Mitigation
- Raw damage: `rawDamage = floor(R * Attack + S * (Level – 1) + B)`
- Mitigation: `damageTaken = floor(rawDamage * 600 / (600 + Defense))`

## Effective HP (eHP)
- `eHP = MaxHP * (1 + Defense / 600)`

## Scoring Channel Time
- Baseline by points carried (see `ScoringDef.thresholds/baseTimes`).
- Additive-on-speed: `speedMultiplier = 1 + Σ(additiveFactors)`
- Team synergy (multiplicative on time): `channelTime = (baseTime / speedMultiplier) * teamSynergyMultiplier`

> See `GDD.md`, `CombatFormulas.md`, `EffectiveHP.md`, `ScoringFormulas.md` for rationale and examples.
