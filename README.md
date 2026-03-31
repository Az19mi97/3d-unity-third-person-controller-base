# 3D Third-Person Controller Base System for Unity

<img width="946" height="783" alt="Skærmbillede 2026-03-31 kl  23 43 02" src="https://github.com/user-attachments/assets/304726f6-02a2-40a9-9283-635fc44ca780" />

A fully functional **third-person character controller system** built in Unity, featuring smooth movement, camera-relative controls, mobile support, and a fully integrated animation system (Idle / Walk / Run / Jump).

This project demonstrates real-world gameplay programming, animation integration, physics-based movement, and cross-platform input handling.

---

## Features

### Character System
- Fully rigged 3D character model (Mixamo-based)
- Smooth rotation towards movement direction
- Ground detection system (ray/sphere-based)
- Physics-based movement using Rigidbody

### Movement System
- Camera-relative movement (WASD / joystick)
- Sprint system with speed multiplier
- Smooth acceleration handling
- Stable physics movement (no jitter or snapping)

### Animation System
- Animator-driven state machine:
  - Idle
  - Walk
  - Run
  - Jump
- Blend-friendly Speed parameter
- Grounded detection sync
- Jump trigger system

### Mobile Support
- On-screen joystick support
- Mobile sprint & jump buttons
- Auto UI enabling for mobile platforms
- Enhanced Touch Input System integration

### Camera Integration
- Camera-relative movement direction
- Smooth character facing direction
- Supports third-person gameplay feel

---

## Technical Highlights

This project demonstrates:

- Unity **Rigidbody physics movement**
- Custom **input system integration (New Input System)**
- Animator parameter synchronization
- Camera-space movement calculations
- Mobile + PC cross-platform architecture
- Clean separation of movement, animation, and input logic

---

## Controls

### PC
- **WASD** → Move
- **Shift** → Sprint
- **Space** → Jump

### Mobile
- Left joystick → Move
- Sprint button → Sprint
- Jump button → Jump

---

## Architecture Overview

PlayerController
│

├── Input System (Keyboard / Mobile / Joystick)

├── Movement (Rigidbody-based physics)

├── Rotation (Camera-relative)

├── Ground Check (Physics Sphere)

└── Animator Sync

├── Speed (float)

├── IsGrounded (bool)

└── Jump (trigger)

---

## 🎬 Animation Setup (Unity Animator)

The Animator uses:

- **Float:** `Speed`
- **Bool:** `IsGrounded`
- **Trigger:** `Jump`

### States:
- Idle → Walk → Run (based on Speed)
- Any State → Jump
- Jump → Idle/Walk/Run (based on IsGrounded)

---

## What I Learned / Built

This project demonstrates practical game development skills, including:

- Designing a full character controller from scratch
- Handling complex animation-state logic
- Solving physics + animation desync issues
- Supporting multiple input systems (PC + Mobile)
- Debugging real-time movement and Animator problems

---

## Possible Future Improvements

- Blend Tree for smoother animation transitions
- Coyote time + jump buffering
- Sprint stamina system
- Advanced camera system (Cinemachine)
- Root motion animation support
- Combat system integration

---

## Developer

Focused on:
- Gameplay programming
- Animation systems
- Real-time physics interaction
- Mobile + PC cross-platform gameplay
