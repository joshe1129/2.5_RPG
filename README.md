# Sanctuary of the Flame 🔥 | Game Development Portfolio

[![Unity](https://img.shields.io/badge/Unity-2022.3.25f1-000000.svg?logo=unity)](https://unity.com/)
[![C#](https://img.shields.io/badge/C%23-Programming-239120.svg?logo=c-sharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Architecture](https://img.shields.io/badge/Architecture-SOLID%20%7C%20Event--Driven-blue.svg)]()
[![Status](https://img.shields.io/badge/Status-Production%20Ready-success.svg)]()

**Sanctuary of the Flame** is a 2.5D RPG developed in Unity. This repository serves as a **Code Portfolio**, showcasing advanced software engineering principles, clean code architecture, and robust game systems design.

> **Note:** This repository contains the source code. Asset files (art, audio, models) are excluded. You can play the full compiled game on [Itch.io](https://josesalinas-dev.itch.io/sanctuary-of-the-flame).

---

## 🏗️ Architectural Highlights

The project was heavily refactored to adhere to professional industry standards, focusing on decoupling and performance:

- **Service Locator Pattern (`ServiceLocator.cs`)**: Replaced tight coupling and brittle Singletons with a globally accessible registry for core managers (`IPartyManager`, `IGameManager`, `IAudioManager`, etc.).
- **Event-Driven Architecture (`GameEvents.cs`)**: Implemented an Event Bus pattern using C# `Action` delegates to handle cross-system communication (e.g., triggering battles, winning scenes) without direct dependencies.
- **Object Pooling (`ObjectPooler.cs`)**: Eliminated Garbage Collection spikes during combat by pooling visual effects and battle entities, dramatically improving runtime performance.
- **Centralized Constants (`GameConstants.cs`)**: Eradicated "magic strings" for Scene names, Animator parameters, and PlayerPrefs keys to ensure type safety and ease of refactoring.

---

## 🎮 Core Game Systems

| System | Key Classes | Responsibility |
|--------|-------------|-------------------|
| **Turn-Based Combat** | `BattleSystem.cs`, `CombatResolver.cs` | Manages turn order, action execution, and state transitions using Coroutines. |
| **Dynamic UI Feedback** | `BattleUIManager.cs`, `UIHoverHandler.cs` | Handles reactive combat UI, including dynamic portrait switching and targeted enemy spotlights using event delegates. |
| **Data Persistence** | `PartyManager.cs`, `EnemyManager.cs` | Manages party composition, stats, and overworld persistence across scene loads. |
| **Enemy AI** | `EnemySimpleAI.cs`, `MemberFollowAI.cs` | State-based AI with NavMesh pathfinding for overworld pursuit and party following mechanics. |

---

## 📂 Code Organization

All scripts are located in `Assets/Scripts/` and utilize interfaces to ensure SOLID principles (specifically Dependency Inversion):

- **Interfaces**: `IPartyManager`, `IGameManager`, `IEnemyManager`, `IAudioManager`, `IObjectPooler`
- **Core Managers**: `ServiceLocator`, `GameManager`, `AudioManager`, `GameEvents`, `GameConstants`
- **Battle Systems**: `BattleSystem`, `BattleEntities`, `BattleVisuals`, `BattleUIManager`
- **Overworld Systems**: `PlayerController`, `CharacterManager`, `OverworldVisuals`, `EncounterSystem`

---

## 🚀 Key Programming Skills Demonstrated

1. **SOLID Principles**: Heavy use of interfaces (`Dependency Inversion`) and distinct manager responsibilities (`Single Responsibility`).
2. **Memory Management**: Custom Object Pooler for high-frequency instantiation.
3. **Reactive UI**: UI elements update strictly via event subscriptions, keeping logic decoupled from the view.
4. **Data-Driven Design**: Use of `ScriptableObjects` (`EnemyInfo`, `PartyMemberInfo`) to define stats without hardcoding.

---

## 🔗 Links

- 👉 **[Play the Game on Itch.io](https://josesalinas-dev.itch.io/sanctuary-of-the-flame)**
- 📧 **Contact**: jose.salinas.dev@outlook.com | [LinkedIn: José Salinas](https://www.linkedin.com/in/josesalinas-dev/)
