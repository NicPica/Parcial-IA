# Verificación en Play Mode — Devolución ronda 2 (Boids + Hunter)

## Objective
Cerrar con evidencia en ejecución los 4 puntos de la devolución que no se pueden probar en modo lectura (3 boids dentro del área, 5 Evade+Separation y reagrupamiento, 6 estados instanciados una sola vez, 8 feedback visual), sin tocar código de gameplay.

## Changes
- (sin cambios de código ni de assets) — sólo Play Mode controlado y mediciones por reflexión en memoria.

## Relevant Assets and Quirks
- Escena `Assets/Scenes/EscenaPrincipal.unity`: `Ground` en (0, 0.35, 0) escala (108.1, 1, 117.485) → AABB x ∈ [-54.05, 54.05], z ∈ [-58.7425, 58.7425]. `Bounds` en el origen con `_width = 108.1`, `_height = 117.485` → el wrap coincide exactamente con el piso.
- `Bounds.CalculateBoundPosition` es **wrap (teletransporte)**, no contención (`Assets/Scripts/Shared/Bounds.cs:23-26`). Con `maxSpeed 5` y `fixedDeltaTime` el centro nunca supera el rectángulo en más de ~0.1 u, pero el boid puede quedar parado en el borde exacto y su esfera (radio 1, `Boid.prefab:144`) sobresale del piso visible ~1 u.
- `Boid.prefab` pisa los defaults del script: `separationRadius: 3` (script 1.5), `separationWeight: 3` = `evadeWeight: 3`. **`poiDetectionRadius` no está serializado** → corre con el default del script (12).
- `Boid.neighborTransforms/Positions/Velocities` y `hunterInSight` son privados; para medir hay que leerlos por reflexión desde `execute_code`/`run_play_mode_sequence` (no crear scripts temporales: sería agregar archivos no pedidos).
- 6 boids iniciales, `respawnDelay 3`, POIs cada 4 s con tope de 5 (`HunterNPC` en escena).
- Si el Hunter queda a más de 6 u de todos los boids no hay fase de Evade; forzarla moviendo al Hunter en runtime alteraría el comportamiento, así que las fases se detectan desde el estado real observado.

## Steps
- [x] 1. Baseline: `check_compile_errors` y `get_unity_logs` (limpiar consola) para registrar errores/warnings previos al run. [depends: none] — hecho: `check_compile_errors` = "No compile errors"; único warning de script en la consola era el `CS0414 GatherState.isGathering` ya inexistente (stale).
- [x] 2. `unity_verification_agent`: `play_game`, esperar 5 s de estabilización y muestrear cada 0.5 s durante 60 s — por cada boid `BoidSpawner.Instance.GetAllBoids()`: `position`, `activeInHierarchy` y `hunterInSight != null`; y `hunter.CurrentStateName`; devolver los arrays crudos. [depends: 1] — hecho con `manage_editor(play)` + sampler en memoria (`EditorApplication.update` + `Debug.Log`), sin crear archivos: 3 corridas de 60 s / 60 s / 35 s, 121 + 120 + 140 muestras, 0 errores del sampler.
- [x] 3. Punto 3: con las muestras, calcular máx |x|, máx |z|, dispersión de `y` y cantidad de wraps; confirmar máx |x| ≤ 54.05 y máx |z| ≤ 58.7425 y anotar el sobrepaso del borde visible. [depends: 2] — medido: `maxAbsX=54.00`, `maxAbsZ=40.64`, `yMin=yMax=0.85`, `wraps=11` en 60 s. Centro siempre dentro del piso; el cuerpo (esfera radio 1) sobresale ~1 u del borde cuando está pegado al límite, y el wrap puede apilar dos boids (mínimo par a par entre vivos = 0.21 u).
- [x] 4. Punto 5: particionar las muestras en "sin Evade", "Evade" y "10 s posteriores al último Evade" y calcular la distancia promedio entre pares de boids vivos en cada fase (reagrupamiento = fase 3 ≤ fase 1). [depends: 2] — medido: CALM prom=29.95 / min=0.21 (n=135); EVADE prom=32.22 / min=2.36 (n=5); POST10s prom=15.30 / min=0.31 (n=43). Durante el Evade la distancia mínima par a par fue 2.36 u (sin superposición); los mínimos de las fases sin Evade (0.21/0.31) corresponden a teleports de wrap y respawn.
- [x] 5. Punto 6: leer por reflexión `HunterStateMachine._allStates` al inicio y al final del run e informar los hash/identidad de las 3 instancias + los `[FSM] ... -> X` observados, confirmando que no cambian. [depends: 2] — medido: `ids=-1556245390,-671239412,950263846` idénticos en las 121 muestras (un solo `[MONID]`); los logs muestran `Patrol -> Attack -> ExecuteHit -> Patrol -> Gather -> Patrol` sin `Missing State` ni excepciones.
- [x] 6. Punto 8: `capture_editor_screenshot` (game_view) para leer `UI/DebugCanvas/DebugPanel/DebugText` (estado, objetivo vivo/muerto, POIs, boids cazados) y el label de `Hunter/StateIndicatorCanvas`; `get_unity_logs` para confirmar 0 excepciones y 0 `Missing State`. [depends: 2] — leído en runtime: `debugText=[<b>Estado:</b> <color=#4CD964>Patrol</color> / Objetivo (vivo): ninguno / Objetivo muerto: ninguno / POIs activos: 5 / Boids cazados: 44]`, `label=[Patrol]`; el contador pasó de 40 a 44 y `POIs activos = 5 = maxActivePOIs`. El canvas es ScreenSpaceOverlay y no aparece en `capture_editor_screenshot(game_view)`; 0 excepciones en toda la sesión.
- [x] 7. `stop_game` y volcar la tabla ✅/⚠️/❌ por punto de la devolución con los números medidos. [depends: 3,4,5,6] — hecho: play mode detenido y tabla volcada al usuario.

## Verify
- Punto 3: máx |x| ≤ 54.05 y máx |z| ≤ 58.7425 en todo el run; si el valor queda pegado al límite, reportar el overhang del borde como decisión pendiente (wrap vs contención real).
- Punto 5: durante el Evade la distancia promedio entre boids no se derrumba ni hay superposiciones (mínimo par a par > ~2 u con `separationRadius 3`); 10 s después del Evade la distancia promedio vuelve al orden de la fase sin Evade.
- Punto 6: las 3 instancias de estado son las mismas al inicio y al final; los logs muestran `Patrol -> Attack -> Patrol -> Gather` sin recreaciones ni `Missing State`.
- Punto 8: el texto de debug cambia con el estado y el contador de cazados sube; consola limpia de excepciones.

## Pendientes detectados (fuera de esta verificación)
- Limpieza de comentarios incompleta: 14 bloques `/// <summary>` en 8 archivos + `CameraController.cs:15,45,60`, `CameraModeButtons.cs:5,11`, `DebugFeedbackUI.cs:36-38`, `GatherState.cs:39,45`, `Boid.cs:13`.
- `Boid.prefab` contradice dos razones escritas en el código (`separationRadius 3` vs comentario "menor a propósito"; `separationWeight 3` = `evadeWeight 3` vs "mantiene la prioridad por su peso") y no serializa `poiDetectionRadius`.
- Thrash de FSM Patrol↔Attack / Patrol↔Gather (~250 ms) cuando el objetivo cacheado queda justo en el borde de la visión (observación de la ronda 1, sin corregir).
- `Assets/.Aura/plans/correccion_parcial_boids_hunter_v1.md` sólo documenta la ronda 1.
