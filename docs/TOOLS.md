# Tool reference (69 tools)

*[中文版](TOOLS.zh-CN.md)*

Every tool is invoked through `tools/call`: `{ "name": "...", "arguments": { ... } }`.
A `*` marks a required argument, `?` an optional one. When `person` is omitted the first Person
in the scene is used. Vectors are `[x, y, z]`.

`eval_cs` is only registered when `Security.AllowEval=true`, so a default install exposes 62 tools.

## Status

### status
VaM runtime status: plugin version, VaM version, atom/person counts, endpoint, eval switch.

### read_log
What VaM and its plugins logged, each line stamped with date and time (VaM's own output_log.txt has no timestamps). Captured in memory from plugin start (newest 5000 lines), so it answers even while a scene is loading. Arguments: `since` (`scene_load` default, or `all`), `level` (`error` default: errors, exceptions, asserts; `warning`; `all`), `contains`, `after` (the `last_seq` of an earlier call, to get only new lines), `group` (default true: identical lines from the same source collapsed with `count` and `first_time`), `stack` (default true, first 8 lines), `limit` (default 100). Each entry has `time`, `level`, `message` and, when there is a stack, `source` (its top frame, which names the plugin). Unity's native engine messages (e.g. SoundManager) never reach it.

## Scene

| Tool | Arguments | Notes |
| --- | --- | --- |
| `list_scenes` | `search?` | Scenes under `Saves/scene` |
| `load_scene` | `path`*, `merge?` | Load a scene (`merge=true` merges into the current one); .var paths work (`Package.1:/Saves/scene/x.json`) |
| `new_scene` | — | New empty scene (**discards the current one**; an empty scene has no lights — add one or raise ambient light) |
| `save_scene` | `path?` | Save the scene (default `Saves/scene/VaMMCP_<timestamp>.json`; an existing file is removed first so the save is not swallowed by VaM's confirm dialog) |

## Atom

| Tool | Arguments | Notes |
| --- | --- | --- |
| `list_atom_types` | — | Creatable atom types, grouped by category (from VaM's runtime data) |
| `list_atoms` | `type?` | Atoms in the scene (uid/type/on/position/rotation) |
| `add_atom` | `type`*, `uid?` | Create an atom and wait until it is ready (up to 45 s) |
| `remove_atom` | `uid`* | Delete an atom |
| `set_atom_on` | `uid`*, `on`* | Show/hide |
| `get_atom_transform` | `uid`* | World position / euler angles / scale |
| `set_atom_transform` | `uid`*, `position?`, `rotation?`, `scale?`, `relative?` | Transform (`relative=true` offsets instead of setting) |

## Person

| Tool | Arguments | Notes |
| --- | --- | --- |
| `add_person` | `uid?` | Add a Person (VaM may rename the uid — trust the return value) |
| `list_persons` | — | Persons in the scene (uid/character/gender/position) |
| `list_looks` | `search?` | Appearance presets (`Saves/Person/Appearance`) |
| `load_look` | `path`*, `person?` | Apply an appearance preset (character/clothing/hair/morph deltas; asynchronous, verify a moment later) |
| `save_look` | `path?`, `person?` | Export an appearance preset (**note**: VaM presets do not carry the full material parameter set — re-apply skin parameters after moving a look between scenes) |
| `set_character` | `person?`, `name`* | Switch the base character mesh (names come from the choices of `get_param storable=geometry param=characterSelection`) |

## Preset export

| Tool | Notes |
| --- | --- |
| `save_look` | Appearance (character/clothing/hair/morphs) → `Saves/Person/Appearance/` |
| `save_pose` | Pose + physics → `Saves/Person/Pose/` |
| `save_full_preset` | Everything (appearance + pose + physics) → `Saves/Person/full/` |

## Morphs

| Tool | Arguments | Notes |
| --- | --- | --- |
| `list_morphs` | `person?`, `search?`, `region?`, `group?`, `limit?` | Search morphs (name/uid/region/group/current value) |
| `set_morph` | `person?`, `name`*, `value`* | Set a morph value (typically -1..1) |
| `get_morph` | `person?`, `name`* | Read a morph value |
| `reset_morphs` | `person?`, `region?` | Zero morphs, optionally only one region |

## Clothing / hair

| Tool | Arguments | Notes |
| --- | --- | --- |
| `list_packages` | — | Installed .var packages |
| `list_clothing_presets` | `search?` | Clothing presets and items: disk **plus a recursive scan of every package**'s `Custom/Clothing` (.vam/.json) |
| `load_clothing_preset` | `path`*, `person?` | Apply a clothing preset |
| `list_clothing_items` | `person?` | Clothing currently on the person (uid/name/active) |
| `add_clothing_item` | `path`*, `person?` | Add a clothing item (`.vam` package path; waits for the character to finish loading) |
| `remove_clothing_item` | `id`*, `person?` | Remove an item |
| `set_clothing_item_on` | `id`*, `on`*, `person?` | Wear / take off |
| `list_hair_presets` / `load_hair_preset` / `list_hair_items` / `add_hair_item` / `remove_hair_item` / `set_hair_item_on` | same as clothing | Hair (`Custom/Hair`) |

## Pose / expression

| Tool | Arguments | Notes |
| --- | --- | --- |
| `list_poses` | `search?` | Pose presets (`Saves/Person/Pose`) |
| `load_pose` | `path`*, `person?`, `all?` | Apply a pose (`all=true` applies it to every person) |
| `save_pose` | `path?`, `person?` | Export a pose |
| `list_expressions` | `person?` | Expression morphs (heuristic filter) |
| `set_expression` | `person?`, `name`*, `value?`, `reset?` | Set an expression (clears the other expressions first by default; `value` defaults to 1) |

## Animation

| Tool | Arguments | Notes |
| --- | --- | --- |
| `play_animation` | `animation`*, `person?`, `storable?`, `tolerance?`, `timeout?` | Play a Timeline animation (`Play <animation>`) with the person's collisions off, then turn them back on once hip, chest and head are within `tolerance` metres (default 0.05) of their controls. Started from somewhere else, the body flies across the scene to reach the pose and hits everything on the way, so use this instead of `call_action`. If the body is not there within `timeout` seconds (default 8), collisions stay off and the result says so |
| `list_animations` | `uid?`, `targets?` | Timeline animations as Timeline holds them. Without `uid`: one summary per atom with Timeline (clip count, what is playing, problems). With `uid`: every clip with segment, layer, length, loop, speed, playing, and how many controllers, floats (morphs etc.) and trigger tracks it animates; `targets=true` also lists them by name. Flags a name, segment or layer that hides control characters (shown escaped, e.g. `\r`) and names repeated in the same segment and layer |

## Bone control

| Tool | Arguments | Notes |
| --- | --- | --- |
| `list_controls` | `person?` | FreeControllerV3 control points (headControl / chestControl / …) |
| `get_control` | `person?`, `control`* | Control point world position / euler angles |
| `set_control` | `person?`, `control`*, `position?`, `rotation?` | Move / rotate a control point |
| `set_gaze` | `person?`, `amount?`, `targetControl?` | Gaze (best effort; for precise control use the `lookAt` storable parameters) |

## Generic parameters (the long tail)

| Tool | Arguments | Notes |
| --- | --- | --- |
| `list_atom_storables` | `uid`* | Every storable on an atom, with its type |
| `list_storable_params` | `uid`*, `storable`* | Parameter introspection: floats (with min/max/default) / bools / strings / choosers / colors / actions / customParams / properties |
| `get_param` | `uid`*, `storable`*, `param`* | Read a parameter (float/bool/string/chooser/color/property auto-detected) |
| `set_param` | `uid`*, `storable`*, `param`*, `value`*, `type?` | Write a parameter (colors accept `#RRGGBB` or `r,g,b,a`) |
| `call_action` | `uid`*, `storable`*, `action`* | Trigger a storable action (a button in the UI) |

`type` can force the interpretation: `float|bool|string|chooser|color|property`
(`property` reads/writes a public property through reflection, covering customParamNames such as `gravityX`).

## Dependencies

| Tool | Arguments | Notes |
| --- | --- | --- |
| `scene_dependencies` | `path?`, `list?` | The packages a scene needs. Streams the scene file (default: the one VaM loaded last) and its subscenes for package references, off the main thread; for a scene inside a package, its `meta.json` dependencies count too. Then checks every morph any Timeline animates on a person and asks that person which package the morph came from. Timeline stores morphs by name only and VaM records a morph's package only while it is non-zero at save time, so packages a Timeline needs but nothing references are listed in `missing_from_references` with a suggested `minN` reference; `morphs_not_found` lists morphs the person does not have (package not installed). `list=true` also lists every referenced package with its versions |

## Voxta

| Tool | Arguments | Notes |
| --- | --- | --- |
| `voxta_state` | `person?` | What the Voxta plugin on a person knows right now: `connection` (connected, status text, last error), `chat` (active, ready, chat id, user name, characters, scenario), `state` (off/idle/listening/thinking/speaking), `current_states` and `available_actions` as the server sent them, `current_action`, `last_message`, and the non-empty `context` slots and `action_lists`. Works with any action setup (text action lists, the Actions tab, scenario actions) because it reads what the plugin received |

## Camera

| Tool | Arguments | Notes |
| --- | --- | --- |
| `get_camera` | — | Monitor camera position / rotation / FOV |
| `set_camera` | `position?`, `rotation?`, `fov?` | Move the camera |
| `capture_view` | `width?`, `height?`, `path?`, `return_image?`, `hide_ui?` | Screenshot what the monitor shows, taken at the end of a frame so people are drawn with their skin (default `Saves/PluginData/vam-mcp/preview.png`, 1280 wide, height from the screen's aspect). `hide_ui=true` hides VaM's main UI for the shot and puts it back afterwards. With `return_image=true` the PNG also comes back as an MCP image block, so clients that cannot read the VaM folder can still see it — keep the resolution modest (e.g. 640×360); anything above 4 MB stays on disk only |

## UI

| Tool | Arguments | Notes |
| --- | --- | --- |
| `set_ui` | `visible?`, `deselect?` | Show (`visible=true`) or hide (`visible=false`) VaM's main UI; `deselect=true` deselects the selected atom so its panel closes. With no arguments it only reports `visible` |

## Simulation

| Tool | Arguments | Notes |
| --- | --- | --- |
| `set_simulation` | `paused?`, `timeScale?` | Pause / resume / time scale |
| `reset_simulation` | — | Reset the physics simulation (settles soft-body and cloth jitter) |

## Hub (community content)

| Tool | Arguments | Notes |
| --- | --- | --- |
| `hub_browse` | `search?`, `page?`, `perpage?`, `sort?`, `type?` | Search the VaM Hub (id/title/author/version/type/downloads/rating/tags) |
| `hub_detail` | `package?`, `resource_id?` | Resource detail: package name + every version's .var + dependencies |
| `hub_download` | `package`* | Download and install through VaM's own downloader (handles auth and dependencies; free content works anonymously, paid content needs you signed in inside VaM) |

## Skin

| Tool | Arguments | Notes |
| --- | --- | --- |
| `set_skin_sss` | `person?`, `color`* | Subdermis (subsurface) colour `_SubdermisColor`; a pink tint such as `#FFC9C9` reads as fairer skin |

## Plugin management (native VaM plugins)

| Tool | Arguments | Notes |
| --- | --- | --- |
| `list_plugins` | `uid?` | Loaded plugins (empty uid = CoreControl session plugins; pass a person uid for that person's plugins) |
| `add_plugin` | `path`*, `uid?` | Load a .cs/.cslist/.dll plugin and wait for compilation. Once loaded the plugin is a storable, so `list_storable_params` / `set_param` configure it (storable ids look like `plugin#0_MVRPlugin.ClassName`) |
| `remove_plugin` | `plugin_uid`*, `uid?` | Unload a plugin |

## Escape hatch

### eval_cs (disabled by default, enable with `Security.AllowEval=true`)
- No `;` → expression mode, the value of the expression is returned
- Contains `;` → statement mode (use an explicit `return` when you want a value)
- `sc` is bound to `SuperController.singleton`
- Restricted by VaM's runtime sandbox: System.IO / System.Reflection / System.AppDomain /
  UnityEditor / Mono.Cecil are blocked (note that `GetType().Name` pulls in System.Reflection —
  use `ToString()` instead)
