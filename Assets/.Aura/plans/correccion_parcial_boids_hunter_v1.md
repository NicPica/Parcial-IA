# Corrección Parcial 1 — Boids + Hunter

## Objective
Corregir los tres puntos de la devolución (estados de FSM instanciados una sola vez con la FSM de cátedra, eliminación de `OverlapSphere` en `Update`/`FixedUpdate` vía escaneo por intervalos con cache, y separación de la lógica visual en scripts propios), más el manejo de bounds con la clase `Bounds` de la cátedra.

## Changes
- `Assets/Scripts/Shared/State.cs` - (create) `State<T>` de la cátedra (`SetFSM`, `Enter`, `Update`, `Exit`).
- `Assets/Scripts/Shared/FiniteStateMachine.cs` - (create) `FiniteStateMachine<T>` de la cátedra con `CurrentID` (`AddState`, `ChangeState`, `Update`).
- `Assets/Scripts/Shared/Bounds.cs` - (create) `Bounds` de la cátedra (`CalculateBoundPosition`, wrap-around, `_width` 60 / `_height` 34).
- `Assets/Scripts/Hunter/HunterStateMachine.cs` - (rewrite) pasa a `HunterStateMachine : FiniteStateMachine<HunterStateID>`; registra Patrol/Attack/Gather **una sola vez** en el constructor.
- `Assets/Scripts/Hunter/States/IHunterState.cs` - (rename → `HunterState.cs`) contiene `enum HunterStateID { Patrol, Attack, Gather }` y `abstract class HunterState : State<HunterStateID>` con `protected HunterNPC hunter`.
- `Assets/Scripts/Hunter/States/PatrolState.cs` / `AttackState.cs` / `GatherState.cs` - (rewrite) derivan de `HunterState`; transiciones vía `_fsm.ChangeState(HunterStateID.X)`.
- `Assets/Scripts/Hunter/HunterNPC.cs` - usa `HunterStateMachine`; percepción por intervalo + cache; wrap con `Bounds`; color delegado a `HunterVisuals`.
- `Assets/Scripts/Hunter/HunterVisuals.cs` - (create) cambio de color por estado (`MaterialPropertyBlock` `_BaseColor`).
- `Assets/Scripts/Boids/Boid.cs` - percepción por intervalo + cache (sin `OverlapSphere` en `FixedUpdate`); wrap con `Bounds`; color delegado a `BoidVisuals`.
- `Assets/Scripts/Boids/BoidVisuals.cs` - (create) `MaterialPropertyBlock` `_BaseColor` + `TrailRenderer`.
- `Assets/Scripts/Boids/BoidSpawner.cs` - posición de spawn envuelta dentro de `Bounds`.
- `Assets/Scenes/EscenaPrincipal.unity` - nuevo GameObject `Bounds` (componente `Bounds`) en el origen; reasignar `HunterVisuals` en `Hunter` y `BoidVisuals` en `Boid.prefab`.

## Relevant Assets and Quirks
- Unity **6000.3.17f1**, URP **17.3.0**, UGUI **2.0**, Input System **1.19**. Sin tags custom; layers `6=Boid`, `7=Hunter`, `8=POI`.
- Escena `EscenaPrincipal`: `Managers` (`BoidSpawner`, `PointOfInterestManager`, `DebugFeedbackUI`, `GameStatsManager`), `Hunter` (layer 7, `Rigidbody`+`BoxCollider`+`HunterNPC`), `WaypointsPath/Waypoint1..4`, `UI/DebugCanvas` (`DebugPanel/DebugText`, `BtnFollowHunter`, `BtnCycleBoids`), `Hunter/StateIndicatorCanvas/StateLabel` (`HunterStateIndicator`). **No existe** GameObject `Bounds` todavía.
- `Boid.prefab`: layer 6, `Rigidbody` (interpolate None), `SphereCollider`, `TrailRenderer` (`TrailTexture.mat`), material **URP Lit** (color vía `_BaseColor`).
- Los agentes se mueven con `rb.linearVelocity` (API Unity 6) y `rb.freezeRotation = true`.
- Percepción actual a corregir: `Boid.cs:137` `Physics.OverlapSphere` **allocante** dentro de `FixedUpdate`; `Boid.cs:98` y `HunterNPC.cs:97`/`:123` `OverlapSphereNonAlloc` ejecutados **cada frame**.
- Los dos métodos de escaneo del Hunter comparten `visionBuffer`: el nuevo escaneo único debe cachear en una sola pasada el boid vivo más cercano y el boid muerto más cercano.
- `Bounds` de cátedra es **origen-céntrico** (60×34). El spawn de POI (±10 alrededor del Hunter, en x≈19 / z≈-11.8) puede salirse y hacer wrap: hay que pasar el resultado por `CalculateBoundPosition`.
- `DebugFeedbackUI` y `HunterStateIndicator` dependen de `hunter.CurrentStateName`, `CurrentTarget` y `DeadBoidTarget` → conservar esos miembros con los mismos nombres.
- `CameraModeButtons.cs` es un stub vacío **no adjunto** a ningún GameObject: no forma parte de esta corrección.
- `PointOfInterest.attackingBoids` nunca quita boids muertos → quirk conocido; no se toca salvo indicación.
- **No se elimina ningún archivo**: `IHunterState.cs` se renombra, no se borra.

## Steps
- [x] 1. Crear `Assets/Scripts/Shared/State.cs` y `Assets/Scripts/Shared/FiniteStateMachine.cs` con el código exacto de la cátedra (+ `CurrentID`) y validarlos con `validate_script` / `check_compile_errors`. [depends: none]
- [x] 2. Renombrar `IHunterState.cs` → `HunterState.cs` (`enum HunterStateID` + `abstract class HunterState : State<HunterStateID>`) y reescribir `HunterStateMachine.cs` como `HunterStateMachine : FiniteStateMachine<HunterStateID>` que registra los 3 estados una sola vez. [depends: 1]
- [x] 3. Reescribir `PatrolState/AttackState/GatherState` derivando de `HunterState` con transiciones `_fsm.ChangeState(HunterStateID.X)` y `Debug.Log` en cada `Enter`; actualizar `HunterNPC` (registro único en `Awake`, `_fsm.Update()`) conservando `CurrentStateName => _fsm.CurrentID.ToString()` para la UI. [depends: 2]
- [x] 4. En `HunterNPC`, mover la percepción a un escaneo por intervalo (coroutine con `WaitForSeconds` + `OverlapSphereNonAlloc` y buffer único) que cachea boid vivo más cercano y boid muerto; `PatrolState`/`AttackState` leen el cache; sin query en `Update`. [depends: 3]
- [x] 5. En `Boid`, sacar de `FixedUpdate` los `OverlapSphere` de vecinos (flocking) y de detección del Hunter hacia un escaneo por intervalo con `OverlapSphereNonAlloc` y buffers reutilizables + cache; `FixedUpdate` solo aplica steering desde el cache; eliminar el `Physics.OverlapSphere` allocante de `DetectHunter`. [depends: none]
- [x] 6. Separar la lógica visual: crear `BoidVisuals.cs` y `HunterVisuals.cs`, mover los `MaterialPropertyBlock`/colores/trail fuera de `Boid` y `HunterNPC`, y reasignar las referencias en `Boid.prefab` y en `Hunter` (`add_component` + `set_property`). [depends: 3, 5]
- [x] 7. Bounds: crear `Assets/Scripts/Shared/Bounds.cs`, agregar el GameObject `Bounds` en el origen de `EscenaPrincipal`, y aplicar `Bounds.Instance.CalculateBoundPosition(...)` en el movimiento de `Boid`/`HunterNPC` (con `Debug.LogWarning` si `Instance` es null) y en los spawns de `BoidSpawner` y `TrySpawnPOI`. [depends: none]
- [x] 8. Verificar: `check_compile_errors` en 0; `grep` confirmando que no queda `Physics.OverlapSphere(` en `Update`/`FixedUpdate` ni la versión allocante. [depends: 4, 5, 6, 7]

## Verify
- `check_compile_errors` sin errores y sin warnings de referencias faltantes.
- Play mode 20–30 s y `get_unity_logs`: se ven las transiciones `Patrol→Attack→Gather→Patrol` (una sola instancia por estado), los logs de escaneo por intervalo y **cero** excepciones ni `Missing State`.
- `grep` de `OverlapSphere` en `Assets/Scripts`: solo `OverlapSphereNonAlloc`, nunca dentro de `Update`/`FixedUpdate`.
- `capture_editor_screenshot` (game_view): se distinguen los colores por estado del Hunter y el label `StateIndicatorCanvas/StateLabel`.
- Los boids y el Hunter hacen wrap al cruzar el borde del `Bounds` de 60×34; los spawns quedan dentro.
- `stop_game` al finalizar.
