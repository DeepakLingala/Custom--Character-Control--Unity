# Custom Character Control — Unity

A lightweight, fully-commented custom character controller for Unity that replaces the default `CharacterController`-based movement with a configurable, game-ready system.

## Features

| Feature | Details |
|---------|---------|
| Walking | Configurable walk speed |
| Sprinting | Hold **Left Shift** while moving |
| Crouching | Hold **Left Ctrl** — height smoothly transitions; can't stand if blocked by ceiling |
| Jumping | Single jump by default; configurable extra jumps (double jump, etc.) |
| Gravity | Custom gravity magnitude, applied every frame |
| Ground check | Sphere-cast under the feet; works on slopes and stairs |
| Animation | Optional `CharacterAnimatorController` drives any Animator with standard float/bool parameters |

---

## Scripts

All scripts live in `Assets/Scripts/CharacterControl/` and share the `CharacterControl` namespace.

### `PlayerInputHandler.cs`
Reads Unity's legacy **Input Manager** axes and key states every frame and exposes them as clean properties consumed by the controller.

| Property | Type | Description |
|----------|------|-------------|
| `MoveInput` | `Vector2` | Normalized horizontal + vertical input |
| `JumpPressed` | `bool` | True for the single frame the jump key is pressed |
| `SprintHeld` | `bool` | True while the sprint key is held |
| `CrouchHeld` | `bool` | True while the crouch key is held |

Configurable serialised fields: axis names, `jumpKey`, `sprintKey`, `crouchKey`.

---

### `CustomCharacterController.cs`
The core controller — requires `CharacterController` and `PlayerInputHandler` on the same GameObject.

**Inspector groups:**

| Group | Key fields |
|-------|-----------|
| Movement | `walkSpeed`, `sprintSpeed`, `crouchSpeed`, `groundAcceleration` |
| Jumping | `jumpForce`, `extraJumps` |
| Gravity | `gravity`, `groundedGravity` |
| Crouching | `standHeight`, `crouchHeight`, `crouchTransitionSpeed` |
| Ground Check | `groundMask`, `groundCheckRadius`, `groundCheckOffset` |

Public read-only state: `IsGrounded`, `IsCrouching`, `IsSprinting`, `HorizontalSpeed`, `VerticalVelocity`.

---

### `CharacterAnimatorController.cs`
Optional companion script — drives an `Animator` from the controller's state.

**Required Animator parameters** (create these in the Animator window):

| Name | Type |
|------|------|
| `Speed` | Float |
| `VerticalVelocity` | Float |
| `IsGrounded` | Bool |
| `IsCrouching` | Bool |
| `IsSprinting` | Bool |

Parameter names are configurable in the Inspector in case your Animator uses different names.

---

## Quick Start

1. Create a **new GameObject** in your scene (e.g. *Player*).
2. Add a **CharacterController** component and set *Height* / *Radius* to match your character mesh.
3. Add **PlayerInputHandler**, **CustomCharacterController**, and (optionally) **CharacterAnimatorController** scripts.
4. If using animations, attach an **Animator** with a controller that contains the parameters listed above.
5. Set the **Ground Mask** on `CustomCharacterController` to the layer(s) your floor uses.
6. Press **Play** — use **WASD** to move, **Space** to jump, **Left Shift** to sprint, **Left Ctrl** to crouch.

> **Tip:** Select the Player at runtime to see the ground-check sphere Gizmo drawn in the Scene view (green = grounded, red = airborne).

---

## Default Key Bindings

| Action | Key |
|--------|-----|
| Move | W / A / S / D or arrow keys |
| Jump | Space |
| Sprint | Left Shift |
| Crouch | Left Ctrl |

All bindings are configurable via the `PlayerInputHandler` Inspector.

---

## Requirements

- Unity **2020.3 LTS** or newer (legacy Input Manager enabled)
- No additional packages required
