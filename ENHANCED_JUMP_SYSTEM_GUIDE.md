# Enhanced Jump System Setup Guide

## Overview
The Enhanced Jump System provides variable jump heights and advanced mechanics including:
- **Normal Jump (1x)**: Quick press/release of jump button
- **High Jump (1.5x)**: Hold jump button for 0.2+ seconds
- **Double Jump (2x)**: Second jump while airborne
- **Apex Boost (1.8x + lateral)**: Double jump at the peak of a high jump

## Components

### 1. JumpPhysicsDef (ScriptableObject)
Configuration asset that defines all jump physics parameters.

**Key Settings:**
- `BaseJumpVelocity`: 10-15f (base upward force)
- `NormalJumpMultiplier`: 1.0f (normal jump = 1x)
- `HighJumpMultiplier`: 1.5f (high jump = 1.5x)
- `DoubleJumpMultiplier`: 2.0f (double jump = 2x)
- `ApexBoostMultiplier`: 1.8f (apex boost = 1.8x)
- `MinHoldTime`: 0.2f (minimum hold for high jump)
- `ApexWindow`: 0.3f (time window at apex for boost)

### 2. EnhancedJumpController (Component)
Main jump system controller that handles:
- Jump state management
- Input processing
- Apex detection
- Jump type determination
- Event broadcasting

### 3. PhysicsLocomotionController (Component)
Enhanced locomotion system with jump integration:
- FSM state management
- Physics integration
- Movement handling
- Jump execution

## Setup Instructions

### Step 1: Create Jump Physics Definition
1. Right-click in Project window
2. Create > MOBA > Jump Physics Definition
3. Name it "DefaultJumpPhysics" or similar
4. Configure the settings as needed

### Step 2: Setup Player GameObject
1. Add `PhysicsLocomotionController` component
2. Add `EnhancedJumpController` component
3. Assign the JumpPhysicsDef to both components
4. Configure references between components

### Step 3: Configure Physics Integration
Ensure your player has:
- DeterministicPhysics integration
- Proper PhysicsBody registration
- Ground detection setup

### Step 4: Input Setup
Configure input system to provide:
- Jump button state (pressed/held/released)
- Movement vector for apex boost direction

## Usage Examples

### Basic Setup Code
```csharp
// Get components
var jumpController = GetComponent<EnhancedJumpController>();
var locomotionController = GetComponent<PhysicsLocomotionController>();

// Subscribe to events
jumpController.OnJumpPerformed += (jumpType, velocity) => {
    Debug.Log($"Jump: {jumpType} with velocity {velocity}");
};

jumpController.OnApexReached += () => {
    Debug.Log("Apex reached - double jump available!");
};

// Initialize with context
var playerContext = new PlayerContext { playerId = "player1" };
var inputSource = GetComponent<IInputSource>();
locomotionController.Initialize(playerContext, inputSource);
```

### Demo Scene Setup
1. Add `JumpSystemDemo` component to player
2. Configure demo settings
3. Run scene and press 1-4 keys for demonstrations
4. Use Tab to toggle advanced statistics
5. Use R to reset position

## Jump Mechanics Explained

### 1. Normal Jump (1x)
- **Input**: Quick press and release of jump button
- **Calculation**: `BaseJumpVelocity * NormalJumpMultiplier`
- **Use Case**: Standard platforming movement

### 2. High Jump (1.5x)
- **Input**: Hold jump button for MinHoldTime or longer
- **Calculation**: `BaseJumpVelocity * HighJumpMultiplier * HoldCurve`
- **Use Case**: Reaching higher platforms

### 3. Double Jump (2x)
- **Input**: Press jump while airborne (within DoubleJumpWindow)
- **Calculation**: `BaseJumpVelocity * DoubleJumpMultiplier`
- **Use Case**: Air mobility and recovery

### 4. Apex Boost (1.8x + lateral)
- **Input**: Double jump at the apex of a high jump
- **Calculation**: `BaseJumpVelocity * ApexBoostMultiplier + LateralBoost`
- **Use Case**: Advanced aerial maneuvers

## Performance Considerations

### Zero-Allocation Design
- Uses object pooling for events
- Minimal GC allocations in Update loops
- Efficient state machine transitions

### Deterministic Physics
- All calculations use fixed-point math when needed
- Consistent results across different framerates
- Network-safe for multiplayer

## Debugging Tools

### Debug Visuals
Enable debug visuals in EnhancedJumpController:
```csharp
[SerializeField] private bool enableDebugVisuals = true;
```

### Statistics Tracking
Get real-time statistics:
```csharp
var stats = jumpController.GetStatistics();
Debug.Log($"Total jumps: {stats.TotalJumps}");
Debug.Log($"Apex boost rate: {stats.ApexBoostRate:P1}");
```

### Event Logging
All jump events are logged through EnterpriseLogger:
- Jump performed events
- State transitions
- Apex detection
- Landing events

## Integration with Other Systems

### Animation System
Connect to jump events for animation triggers:
```csharp
jumpController.OnJumpPerformed += (jumpType, velocity) => {
    string animationTrigger = $"Jump_{jumpType}";
    animator.SetTrigger(animationTrigger);
};
```

### Audio System
Different sounds for different jump types:
```csharp
jumpController.OnJumpPerformed += (jumpType, velocity) => {
    audioSource.pitch = GetJumpPitch(jumpType);
    audioSource.PlayOneShot(jumpSounds[(int)jumpType]);
};
```

### Particle Effects
Visual feedback for jump states:
```csharp
jumpController.OnApexBoostReady += () => {
    apexBoostEffect.Play();
};
```

## Common Issues & Solutions

### Issue: Jumps not registering
**Solution**: Check input source configuration and jump buffer timing

### Issue: Double jump not working
**Solution**: Verify DoubleJumpWindow and AllowDoubleJump settings

### Issue: Apex boost never triggers
**Solution**: Check ApexWindow timing and ensure high jumps are performed

### Issue: Physics integration problems
**Solution**: Verify DeterministicPhysics setup and body registration

## Advanced Configuration

### Custom Jump Types
Extend the system with custom jump types:
```csharp
public enum CustomJumpType
{
    WallJump,
    DashJump,
    SuperJump
}
```

### Jump Combos
Chain different jump types for advanced mechanics:
```csharp
private void CheckJumpCombo(JumpType currentJump)
{
    // Implement combo logic
    if (lastJumpType == JumpType.High && currentJump == JumpType.Double)
    {
        // Trigger combo bonus
        ApplyComboBonus();
    }
}
```

This setup guide provides everything needed to implement the enhanced jump system with the exact formula requested: jump = 1, high jump (hold button) 1.5, double jump 2.0, plus apex boost mechanics for double jumping at the height apex of the high jump.
