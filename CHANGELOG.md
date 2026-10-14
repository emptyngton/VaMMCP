# Changelog

All notable changes to this project are documented here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project uses
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Fixed
- `list_poses` and `list_looks` find `.vap` presets under `Custom/Atom/Person`, loose and in packages, as VaM's browser
  does; they only read the legacy `Saves/Person` folders before and found nothing on a current install. `load_pose`,
  `load_look` and the clothing and hair preset loaders load a `.vap` through the person's preset manager.
- `set_atom_transform`, `get_atom_transform` and `list_atoms` use the atom's main control instead of its root
  transform. Moving the root left lights pointing the old way and was not what VaM saves; a parented atom's
  control does not follow the root at all.

### Added
- `set_control_link`: VaM's Link To, a control following a rigidbody of another atom (Parent Link or
  Physics Link), for trackers on bones or props in hands; saved with the scene.
- `set_atom_parent`: parent an atom to another, or clear its parent; parenting to a SubScene atom adds the
  atom to that subscene.
- `pending_dialogs`: everything on screen waiting for the user (plugin trust prompts, alerts, confirmations,
  file and Hub browsers, the error log, the Voxta API key prompt), reported but never clicked.
- `capture_sequence`: frames over time in one grid image, from the tool's own camera or the screen, with an
  optional action or safe Timeline animation fired right after frame 0.
- `snapshot`: pictures from the tool's own camera, at any position, angle and FOV or as an orbit around a
  target, laid out in one grid image, without moving the user's view.
- `find_broken_triggers`: trigger actions whose receiver atom, storable or target no longer exists, checked
  against the live scene.
- `scene_dependencies`: the packages a scene needs, read from the scene, its subscenes and its package's
  meta.json, plus the packages behind every morph a Timeline animates, flagged when nothing references them.
- `voxta_state`: the Voxta plugin's connection, chat, state, current states, available actions, current
  action, last messages and context slots in one call.
- `list_animations`: Timeline clips per atom as Timeline holds them (segment, layer, length, loop, what is
  playing, what they animate), flagging names that hide control characters or repeat.
- `read_log`: what VaM and its plugins logged, with date and time on every line, since the last scene load by
  default; errors only, identical lines collapsed with a count and the source that logged them.
- `set_ui`: show or hide VaM's main UI, and deselect the selected atom so its panel closes.
- `play_animation`: play a Timeline animation with the person's collisions off until the body has reached the
  animation's pose, so it does not fly through the scene to get there.
- `capture_view` takes `hide_ui=true` to take the shot without the main UI in it, then puts the UI back.

### Fixed
- `capture_view` showed people as hair with nothing under it. It rendered the camera from the tool call, which runs in
  `Update`, before VaM draws a person's skin later in the frame. It now grabs the monitor image at the end of the frame,
  so the shot is what the screen shows, scaled to the requested size.

## [1.0.1] - 2026-08-21

### Changed
- Tool payloads carry real JSON types too: counts, coordinates, morph values, parameter
  min/max/default and `on`/`active` flags are numbers and booleans instead of quoted strings, and
  a null value is `null` rather than `"null"`. Timestamps and enum names stay strings, as do the
  parameters sent to the Hub API, which expects them that way.

### Fixed
- **Protocol values are now real JSON types.** VaM's SimpleJSON quotes everything it serialises,
  so `listChanged`, `subscribe`, `isError`, JSON-RPC error codes and — most importantly — the
  JSON-RPC `id` all went out as strings. Clients built on the official MCP SDK validate against
  the spec schema and match responses by id, so they rejected the handshake and fell back to the
  legacy SSE transport until they timed out. opencode could not connect at all; it can now.
- `status` reported `vaMVersion` from `Application.version`, which is the Unity player version
  ("1.0"). It now uses `SuperController.GetVersion()` and reports the actual VaM build.
- `scripts/deploy.sh` and `scripts/install-bepinex.sh` resolved `VAM_ROOT` one level too high
  (the scripts already `cd` to the repo root), so with the repo inside the VaM folder they wrote
  to the *parent* of the VaM install. Both now resolve it correctly and refuse to run against a
  directory with no `VaM_Data` in it.

## [1.0.0] - 2026-08-20

First public release: an MCP server that lives inside Virt-A-Mate as a BepInEx plugin.

### Added
- Streamable HTTP MCP transport (spec 2025-06-18) served from inside the VaM process, targeting net35.
- 63 tools covering scenes, atoms, persons, morphs, clothing, hair, poses, expressions, bone
  controls, camera, simulation, Hub browsing/downloads, skin subsurface colour and VaM plugin
  management (see [docs/TOOLS.md](docs/TOOLS.md)).
- Generic JSONStorable introspection (`list_atom_storables`, `list_storable_params`, `get_param`,
  `set_param`, `call_action`) so any VaM UI slider is reachable without a dedicated tool.
- Preset export/import, `.var` package asset scanning, and native plugin management.
- `eval_cs` escape hatch, disabled by default.
- MCP resources: `vam://status` and `vam://atoms`.
- `capture_view` can return the rendered PNG inline as an MCP image with `return_image=true`.

### Security
- The HTTP listener binds to loopback only and validates the `Origin` header (DNS-rebinding
  protection); CORS echoes only an already-validated loopback origin instead of `*`.
- Concurrent connections are capped and request sizes are bounded.

[Unreleased]: https://github.com/Rorical/VaMMCP/compare/v1.0.1...HEAD
[1.0.1]: https://github.com/Rorical/VaMMCP/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/Rorical/VaMMCP/releases/tag/v1.0.0
