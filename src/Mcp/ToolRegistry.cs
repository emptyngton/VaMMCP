using System.Collections.Generic;
using SimpleJSON;
using VaMMCP.Api;

namespace VaMMCP.Mcp {
	/// <summary>Builds the complete MCP tool surface.</summary>
	public static class ToolRegistry {
		public static List<Tool> CreateAll(VaMApi api, bool allowEval) {
			List<Tool> tools = new List<Tool>();

			// ---------- status / session ----------
			tools.Add(new Tool("status", "VaM runtime status: plugin version, VaM version, atom/person counts, endpoint info.")
				.Fn(a => api.Status()));
			tools.Add(new Tool("read_log", "Read what VaM and its plugins logged, each line with date and time. Defaults: since the last scene load, errors only, identical lines collapsed with a count. Pass after=<last_seq from the previous call> to get only new lines. Answers even while a scene is loading.")
				.P("since", "string", "scene_load (default) or all", false)
				.P("level", "string", "error (default: errors, exceptions, asserts), warning (also warnings) or all", false)
				.P("contains", "string", "Only lines containing this text (case-insensitive)", false)
				.P("after", "number", "Only lines after this sequence number (last_seq of an earlier call); overrides since", false)
				.P("group", "boolean", "Collapse identical lines into one with a count (default true)", false)
				.P("stack", "boolean", "Include stack traces, first 8 lines (default true)", false)
				.P("limit", "number", "Newest entries to return (default 100, max 1000)", false)
				.Poller()
				.Fn(api.ReadLog));

			// ---------- scene ----------
			tools.Add(new Tool("list_scenes", "List scene files under Saves/scene. Returns path + name for each scene.")
				.P("search", "string", "Optional substring filter", false)
				.Fn(api.ListScenes));
			tools.Add(new Tool("load_scene", "Load a scene. With merge=true the scene is merged into the current one. The load happens over the next few seconds.")
				.P("path", "string", "Scene path from list_scenes", true)
				.P("merge", "boolean", "Merge instead of replace (default false)", false)
				.Fn(api.LoadScene));
			tools.Add(new Tool("new_scene", "Start a new empty scene. Warning: discards the current scene.")
				.Fn(a => api.NewScene()));
			tools.Add(new Tool("save_scene", "Save the current scene to Saves/scene.")
				.P("path", "string", "Optional save path (default Saves/scene/VaMMCP_<timestamp>.json)", false)
				.Fn(api.SaveScene));

			// ---------- atoms ----------
			tools.Add(new Tool("list_atom_types", "List all atom types VaM can create, grouped by category.")
				.Fn(a => api.ListAtomTypes()));
			tools.Add(new Tool("list_atoms", "List atoms in the current scene with uid, type, on/off, position, rotation.")
				.P("type", "string", "Optional atom type filter, e.g. Person", false)
				.Fn(api.ListAtoms));
			tools.Add(new Tool("add_atom", "Create an atom of the given type (Person, Light, AnimationPattern, ...). Waits until it exists, then places it when position, rotation or look_at is given (as set_atom_transform).")
				.P("type", "string", "Atom type (see list_atom_types)", true)
				.P("uid", "string", "Optional uid; auto-generated when omitted", false)
				.P("position", "array", "[x, y, z] world position", false)
				.P("rotation", "array", "[x, y, z] euler angles", false)
				.P("look_at", "any", "[x, y, z] point, or \"Atom\" / \"Atom/control\" (a person aims at its chest), to point the atom's forward axis at (a light or camera shines that way)", false)
				.Timeout(60000).Poller()
				.Fn(api.AddAtom));
			tools.Add(new Tool("remove_atom", "Remove an atom from the scene.")
				.P("uid", "string", "Atom uid", true)
				.Fn(api.RemoveAtom));
			tools.Add(new Tool("set_atom_on", "Show (on=true) or hide (on=false) an atom.")
				.P("uid", "string", "Atom uid", true)
				.P("on", "boolean", "true to show, false to hide", true)
				.Fn(api.SetAtomOn));
			tools.Add(new Tool("set_atom_parent", "Parent an atom to another atom so it follows it, or clear the parent with an empty parent. Parenting to a SubScene atom is how an atom joins that subscene; its uid then becomes \"SubScene/Atom\".")
				.P("uid", "string", "Atom uid", true)
				.P("parent", "string", "Parent atom uid; empty clears the parent", true)
				.Fn(api.SetAtomParent));
			tools.Add(new Tool("get_atom_transform", "Get an atom's world position, rotation and scale.")
				.P("uid", "string", "Atom uid", true)
				.Fn(api.GetAtomTransform));
			tools.Add(new Tool("set_atom_transform", "Move/rotate/scale an atom by its main control, as VaM's move handle does. With relative=true the values are added to the current transform. look_at aims the atom at a point or atom after any move.")
				.P("uid", "string", "Atom uid", true)
				.P("position", "array", "[x, y, z] world position", false)
				.P("rotation", "array", "[x, y, z] euler angles", false)
				.P("look_at", "any", "[x, y, z] point, or \"Atom\" / \"Atom/control\" (a person aims at its chest), to point the atom's forward axis at (a light or camera shines that way)", false)
				.P("scale", "array", "[x, y, z] local scale", false)
				.P("relative", "boolean", "Apply relative to current transform (default false)", false)
				.Fn(api.SetAtomTransform));
			tools.Add(new Tool("atom_bounds", "How big an atom is and where its edges are: a world-aligned box around its visible meshes (visual) and one around its colliders (physical, what bodies rest on), each with min, max, size, center, top and bottom; local_size is the visible size along the atom's own axes (a rotated table still reads as its real length and width). A person also gets top, soles and height. Only the atom's own parts, not atoms parented to it; VaM's control handles are left out.")
				.P("uid", "string", "Atom uid", true)
				.Fn(api.AtomBounds));
			tools.Add(new Tool("probe", "Cast a ray and report what it hits: point, distance, surface normal, atom and collider. Default straight down, to find the floor, a seat or a bed top under a point (cast from below the ceiling). Skips triggers and VaM's control handles; ignore sees through atoms such as a person standing on the spot.")
				.P("from", "any", "Start: [x, y, z] or \"Atom\" / \"Atom/control\"", true)
				.P("direction", "array", "[x, y, z] (default [0, -1, 0], straight down)", false)
				.P("max_distance", "number", "Metres (default 20)", false)
				.P("all", "boolean", "List up to 10 hits along the ray instead of the first (default false)", false)
				.P("ignore", "array", "Atom uids to see through", false)
				.Fn(api.Probe));

			// ---------- persons ----------
			tools.Add(new Tool("add_person", "Add a Person atom to the scene. Waits until it exists, then places it when position or rotation is given.")
				.P("uid", "string", "Optional uid (default auto)", false)
				.P("position", "array", "[x, y, z] world position", false)
				.P("rotation", "array", "[x, y, z] euler angles", false)
				.Timeout(90000).Poller()
				.Fn(api.AddPerson));
			tools.Add(new Tool("list_persons", "List Person atoms: uid, character, gender, on/off, position.")
				.Fn(a => api.ListPersons()));
			tools.Add(new Tool("list_looks", "List appearance presets (looks) as VaM's browser finds them: .vap under Custom/Atom/Person/Appearance, loose and in packages (newest version only), plus legacy .json from Saves/Person/Appearance. Each row: path (pass to load_look), name, folder, package.")
				.P("search", "string", "Optional substring filter on the path (name, folder or package)", false)
				.P("limit", "number", "Rows to return (default 200); count is the total that matched", false)
				.Fn(api.ListLooks));
			tools.Add(new Tool("load_look", "Apply an appearance preset (look) to a person. Replaces their character, morphs and skin. A .vap loads through the person's AppearancePresets manager, as VaM's preset browser does.")
				.P("path", "string", "Look path from list_looks", true)
				.P("person", "string", "Person uid (default: first Person)", false)
				.Fn(api.LoadLook));
			tools.Add(new Tool("save_look", "Save the current appearance of a person as a look preset.")
				.P("path", "string", "Optional save path (default Saves/Person/Appearance/<uid>_<timestamp>.json)", false)
				.P("person", "string", "Person uid (default: first Person)", false)
				.Fn(api.SaveLook));
			tools.Add(new Tool("set_character", "Switch the base character mesh of a person (e.g. 'Female 3'). List available characters with get_param uid=<person> storable=geometry param=characterSelection. Loads asynchronously.")
				.P("person", "string", "Person uid (default: first Person)", false)
				.P("name", "string", "Character name", true)
				.Fn(api.SetCharacter));

			// ---------- morphs (捏人) ----------
			tools.Add(new Tool("list_morphs", "Search body/face morphs of a person. Returns name, uid, region, group and current value. The morphs ARE the character sliders.")
				.P("person", "string", "Person uid (default: first Person)", false)
				.P("search", "string", "Substring filter on morph name or uid", false)
				.P("region", "string", "Exact region filter (e.g. Face, Body)", false)
				.P("group", "string", "Exact group filter", false)
				.P("limit", "number", "Max results (default 500)", false)
				.Fn(api.ListMorphs));
			tools.Add(new Tool("set_morph", "Set a morph value on a person (e.g. breast size, face shape). Typical range -1..1.")
				.P("person", "string", "Person uid (default: first Person)", false)
				.P("name", "string", "Morph display name or uid from list_morphs", true)
				.P("value", "number", "Morph value, typically -1..1", true)
				.Fn(api.SetMorph));
			tools.Add(new Tool("get_morph", "Read a morph value on a person.")
				.P("person", "string", "Person uid (default: first Person)", false)
				.P("name", "string", "Morph display name or uid", true)
				.Fn(api.GetMorph));
			tools.Add(new Tool("reset_morphs", "Reset morphs of a person to 0 (optionally only one region).")
				.P("person", "string", "Person uid (default: first Person)", false)
				.P("region", "string", "Optional region to reset only (e.g. Face)", false)
				.Fn(api.ResetMorphs));

			// ---------- clothing / hair ----------
			tools.Add(new Tool("list_packages", "List installed .var packages (their asset paths are usable in load_clothing_preset / load_hair_preset / add_clothing_item / add_hair_item).")
				.Fn(a => api.ListPackages()));
			tools.Add(new Tool("list_clothing_presets", "List clothing presets: .vap outfits under Custom/Atom/Person/Clothing (loose and in packages, newest version only, with name/folder/package; these load with load_clothing_preset), then clothing items and legacy presets on disk and inside .var packages (path may be a package path like 'Package.1:/Custom/Clothing/...').")
				.P("search", "string", "Optional substring filter on the path", false)
				.P("limit", "number", "Rows of .vap presets to return (default 200); count is the total that matched", false)
				.Fn(api.ListClothingPresets));
			tools.Add(new Tool("load_clothing_preset", "Apply a clothing preset to a person.")
				.P("path", "string", "Preset path from list_clothing_presets", true)
				.P("person", "string", "Person uid (default: first Person)", false)
				.Fn(api.LoadClothingPreset));
			tools.Add(new Tool("list_clothing_items", "List clothing items currently on a person.")
				.P("person", "string", "Person uid (default: first Person)", false)
				.Fn(api.ListClothingItems));
			tools.Add(new Tool("add_clothing_item", "Add a clothing item to a person from its .vam/.json path (package paths work, e.g. from list_clothing_presets). Waits for the character to be ready; loads asynchronously. Use set_clothing_item_on to activate it.")
				.P("path", "string", "Clothing item .vam/.json path", true)
				.P("person", "string", "Person uid (default: first Person)", false)
				.Timeout(60000).Poller()
				.Fn(api.AddClothingItem));
			tools.Add(new Tool("remove_clothing_item", "Remove a clothing item from a person.")
				.P("id", "string", "Clothing item uid from list_clothing_items", true)
				.P("person", "string", "Person uid (default: first Person)", false)
				.Fn(api.RemoveClothingItem));
			tools.Add(new Tool("set_clothing_item_on", "Show or hide a clothing item.")
				.P("id", "string", "Clothing item uid", true)
				.P("on", "boolean", "true to wear, false to hide", true)
				.P("person", "string", "Person uid (default: first Person)", false)
				.Fn(api.SetClothingItemOn));
			tools.Add(new Tool("list_hair_presets", "List hair presets: .vap presets under Custom/Atom/Person/Hair (loose and in packages, newest version only, with name/folder/package; these load with load_hair_preset), then hair items and legacy presets on disk and inside .var packages (path may be a package path).")
				.P("search", "string", "Optional substring filter on the path", false)
				.P("limit", "number", "Rows of .vap presets to return (default 200); count is the total that matched", false)
				.Fn(api.ListHairPresets));
			tools.Add(new Tool("load_hair_preset", "Apply a hair preset to a person.")
				.P("path", "string", "Preset path from list_hair_presets", true)
				.P("person", "string", "Person uid (default: first Person)", false)
				.Fn(api.LoadHairPreset));
			tools.Add(new Tool("list_hair_items", "List hair items currently on a person.")
				.P("person", "string", "Person uid (default: first Person)", false)
				.Fn(api.ListHairItems));
			tools.Add(new Tool("add_hair_item", "Add a hair item to a person from its .vam/.json path (package paths work, e.g. from list_hair_presets). Waits for the character to be ready; loads asynchronously.")
				.P("path", "string", "Hair item .vam/.json path", true)
				.P("person", "string", "Person uid (default: first Person)", false)
				.Timeout(60000).Poller()
				.Fn(api.AddHairItem));
			tools.Add(new Tool("remove_hair_item", "Remove a hair item from a person.")
				.P("id", "string", "Hair item uid from list_hair_items", true)
				.P("person", "string", "Person uid (default: first Person)", false)
				.Fn(api.RemoveHairItem));
			tools.Add(new Tool("set_hair_item_on", "Show or hide a hair item.")
				.P("id", "string", "Hair item uid", true)
				.P("on", "boolean", "true to show, false to hide", true)
				.P("person", "string", "Person uid (default: first Person)", false)
				.Fn(api.SetHairItemOn));

			// ---------- poses / expressions ----------
			tools.Add(new Tool("list_poses", "List pose presets as VaM's browser finds them: .vap under Custom/Atom/Person/Pose, loose and in packages (newest version only), plus legacy .json from Saves/Person/Pose. Each row: path (pass to load_pose), name, folder, package.")
				.P("search", "string", "Optional substring filter on the path (name, folder or package)", false)
				.P("limit", "number", "Rows to return (default 200); count is the total that matched", false)
				.Fn(api.ListPoses));
			tools.Add(new Tool("load_pose", "Apply a pose preset. A .vap loads through the person's PosePresets manager, as VaM's preset browser does, so the pose lands where the person stands. With all=true applies to every Person in the scene.")
				.P("path", "string", "Pose path from list_poses", true)
				.P("person", "string", "Person uid (default: first Person; ignored when all=true)", false)
				.P("all", "boolean", "Apply to all persons (default false)", false)
				.Fn(api.LoadPose));
			tools.Add(new Tool("save_pose", "Save a person's current pose as a pose preset.")
				.P("path", "string", "Optional save path (default Saves/Person/Pose/<uid>_<timestamp>.json)", false)
				.P("person", "string", "Person uid (default: first Person)", false)
				.Fn(api.SavePose));
			tools.Add(new Tool("save_full_preset", "Export a person as a full preset (appearance + pose + physics) — equivalent to VaM's 'Save Full'.")
				.P("path", "string", "Optional save path (default Saves/Person/full/<uid>_<timestamp>.json)", false)
				.P("person", "string", "Person uid (default: first Person)", false)
				.Fn(api.SaveFullPreset));
			tools.Add(new Tool("list_expressions", "List expression-like face morphs of a person with current values.")
				.P("person", "string", "Person uid (default: first Person)", false)
				.Fn(api.ListExpressions));
			tools.Add(new Tool("set_expression", "Set a facial expression: optionally resets all expression morphs first, then sets the named one (default value 1).")
				.P("person", "string", "Person uid (default: first Person)", false)
				.P("name", "string", "Expression morph name from list_expressions", true)
				.P("value", "number", "Morph value (default 1)", false)
				.P("reset", "boolean", "Reset other expression morphs first (default true)", false)
				.Fn(api.SetExpression));

			// ---------- controls ----------
			tools.Add(new Tool("list_controls", "List FreeControllerV3 controls of a person (headControl, chestControl, lHandControl, ...).")
				.P("person", "string", "Person uid (default: first Person)", false)
				.Fn(api.ListControls));
			tools.Add(new Tool("get_control", "Read a person control's world position and rotation.")
				.P("person", "string", "Person uid (default: first Person)", false)
				.P("control", "string", "Control id from list_controls", true)
				.Fn(api.GetControl));
			tools.Add(new Tool("set_control", "Move/rotate a person control (e.g. position a hand, tilt the head).")
				.P("person", "string", "Person uid (default: first Person)", false)
				.P("control", "string", "Control id from list_controls", true)
				.P("position", "array", "[x, y, z] world position", false)
				.P("rotation", "array", "[x, y, z] euler angles", false)
				.Fn(api.SetControl));
			tools.Add(new Tool("set_control_link", "VaM's \"Link To\": make a control follow a rigidbody of another atom (a tracker on a knee, a prop in a hand, a hand on a headset controller). mode=parent follows rigidly (Parent Link, the usual choice); mode=physics follows through springs (Physics Link). The control keeps its current world position and that offset is kept, so place it first. Without link_to it only reports the current link; an empty link_to clears it and puts the states back to On. Not the same as set_atom_parent, which is the atom hierarchy.")
				.P("uid", "string", "Atom uid", true)
				.P("control", "string", "Control id, e.g. lHandControl (default: the atom's main control)", false)
				.P("link_to", "string", "\"Atom:rigidbody\": a control point such as \"Model:rHandControl\" (follows where the hand is told to go, exactly) or a bone such as \"Model:rHand\" (follows where physics puts it, can lag or be pushed); also \"[CameraRig]:CenterEye\". Empty clears the link. A wrong name lists the atom's rigidbodies", false)
				.P("mode", "string", "parent (default) or physics", false)
				.P("position", "boolean", "Link position (default true)", false)
				.P("rotation", "boolean", "Link rotation (default true)", false)
				.Fn(api.SetControlLink));
			tools.Add(new Tool("set_gaze", "Adjust a person's gaze via the lookAt storable (best effort; inspect with list_storable_params storable=lookAt for the full param list).")
				.P("person", "string", "Person uid (default: first Person)", false)
				.P("amount", "number", "gazeAmount 0..1 (optional)", false)
				.P("targetControl", "string", "Target control/atom for the eyes (e.g. None, Camera)", false)
				.Fn(api.SetGaze));

			// ---------- generic param access (long tail) ----------
			tools.Add(new Tool("list_atom_storables", "List every storable (sub-component with params) of an atom.")
				.P("uid", "string", "Atom uid", true)
				.Fn(api.ListAtomStorables));
			tools.Add(new Tool("list_storable_params", "Introspect a storable: all float/bool/string/chooser/color params (with ranges, choices) and actions. This is how an agent can control ANY VaM slider.")
				.P("uid", "string", "Atom uid", true)
				.P("storable", "string", "Storable id from list_atom_storables", true)
				.Fn(api.ListStorableParams));
			tools.Add(new Tool("get_param", "Read a single param value (float|bool|string|chooser|color).")
				.P("uid", "string", "Atom uid", true)
				.P("storable", "string", "Storable id", true)
				.P("param", "string", "Param name", true)
				.Fn(api.GetParam));
			tools.Add(new Tool("set_param", "Set a single param value. Type is auto-detected; pass type to force (float|bool|string|chooser|color|property). 'property' writes a public property/field of the storable (covers custom params like gravityX). Colors accept #RRGGBB or r,g,b,a.")
				.P("uid", "string", "Atom uid", true)
				.P("storable", "string", "Storable id", true)
				.P("param", "string", "Param name", true)
				.P("value", "any", "Value to set", true)
				.P("type", "string", "Optional type override", false)
				.Fn(api.SetParam));
			tools.Add(new Tool("call_action", "Invoke an action button of a storable (e.g. Reset, Stop, Play).")
				.P("uid", "string", "Atom uid", true)
				.P("storable", "string", "Storable id", true)
				.P("action", "string", "Action name from list_storable_params", true)
				.Fn(api.CallAction));

			// ---------- animation ----------
			tools.Add(new Tool("play_animation", "Play a Timeline animation on a person safely: collisions go off, the animation starts, and collisions come back on once the body has reached the animation's pose. Use this instead of call_action 'Play X': started from somewhere else, the body flies across the scene to get there and hits everything on the way. If the body never gets there, collisions are left off and the result says so.")
				.P("animation", "string", "Animation name, as in the Timeline action 'Play <name>'", true)
				.P("person", "string", "Person uid (default: the first person)", false)
				.P("storable", "string", "Timeline storable id (default: the person's only VamTimeline.AtomPlugin)", false)
				.P("tolerance", "number", "How close, in metres, hip, chest and head must be to their controls (default 0.05)", false)
				.P("timeout", "number", "Seconds to wait for the body to get there (default 8, max 30)", false)
				.Timeout(60000).Poller()
				.Fn(api.PlayAnimation));
			tools.Add(new Tool("list_animations", "List Timeline animations as Timeline holds them. Without uid: one summary per atom with Timeline (clip count, what is playing, problems). With uid: every clip with segment, layer, length, loop, speed, playing, and how many controllers, floats (morphs etc.) and trigger tracks it animates. Flags names that hide control characters (shown escaped, e.g. \\r) or repeat in the same segment and layer.")
				.P("uid", "string", "Atom uid for the full clip list (default: summary of every atom with Timeline)", false)
				.P("targets", "boolean", "With uid: also list each clip's controller, float and trigger target names (default false)", false)
				.Fn(api.ListAnimations));

			// ---------- dependencies ----------
			tools.Add(new Tool("scene_dependencies", "The packages the loaded scene really needs. Reads the scene file and its subscenes for package references (streamed, off the main thread), then checks every morph any Timeline animates on a person and asks that person which package the morph came from. Timeline stores morphs by name only and VaM saves a morph's package only while the morph is non-zero, so packages a Timeline needs but no reference covers are listed in missing_from_references with a suggested minN reference; morphs_not_found lists morphs the person doesn't have at all (their package isn't installed).")
				.P("path", "string", "Scene file to read (default: the scene VaM loaded last)", false)
				.P("list", "boolean", "Also list every referenced package with its versions and reference count (default false)", false)
				.Timeout(120000).Poller()
				.Fn(api.SceneDependencies));

			tools.Add(new Tool("find_broken_triggers", "Every trigger action in the scene file (buttons, Timeline trigger tracks, plugin triggers, collision triggers), checked against the live scene. Lists actions whose receiver atom, storable or target no longer exists, so they silently do nothing; an action whose receiver atom was blanked is what VaM leaves when the atom it pointed at is deleted. Identical problems are grouped with a count and up to 5 places that fire them.")
				.P("path", "string", "Scene file to read (default: the scene VaM loaded last)", false)
				.P("limit", "number", "Distinct problems to return (default 100)", false)
				.Timeout(120000).Poller()
				.Fn(api.FindBrokenTriggers));

			// ---------- voxta ----------
			tools.Add(new Tool("voxta_state", "What the Voxta plugin on a person knows right now: connection (connected, status text, last error), chat (active, ready, chat id, characters, scenario), state (idle/listening/thinking/speaking/off), current states and available actions as the server sent them, the current action, the last messages, and the non-empty context slots and action lists. Works with any action setup because it reads what the plugin received.")
				.P("person", "string", "Atom uid that has the Voxta plugin (default: the first one found)", false)
				.Fn(api.VoxtaState));

			// ---------- camera ----------
			tools.Add(new Tool("get_camera", "Read the monitor camera position, rotation and FOV.")
				.Fn(a => api.GetCamera()));
			tools.Add(new Tool("set_camera", "Move the monitor camera: position, rotation (euler) and/or FOV.")
				.P("position", "array", "[x, y, z] world position", false)
				.P("rotation", "array", "[x, y, z] euler angles", false)
				.P("fov", "number", "Field of view in degrees", false)
				.Fn(api.SetCamera));
			tools.Add(new Tool("capture_view", "Screenshot what the monitor shows, taken at the end of a frame so people are drawn with their skin, saved as a PNG (default Saves/PluginData/vam-mcp/preview.png). Set return_image=true to also get the picture back inline, for clients that cannot read the VaM folder themselves.")
				.P("width", "number", "Image width (default 1280, max 4096); the height follows the screen's aspect unless given", false)
				.P("height", "number", "Image height (max 4096); the width follows the screen's aspect unless given", false)
				.P("path", "string", "Optional output path", false)
				.P("return_image", "boolean", "Return the PNG inline as an MCP image (default false; keep the resolution modest, e.g. 640x360, and note that images above 4 MB stay on disk only)", false)
				.P("hide_ui", "boolean", "Hide VaM's main UI for the shot and put it back afterwards (default false)", false)
				.Timeout(30000).Poller()
				.Fn(api.CaptureView));
			tools.Add(new Tool("snapshot", "Pictures from a camera of the tool's own, without touching the user's view: it is a plain Unity camera, not an atom, so it never ends up in the scene. Give explicit views and/or an orbit around a target; all views go into one grid image (left to right, top to bottom) at path. A target is \"Atom\" or \"Atom/control\" (a person aims at chestControl by default); orbit angle 0 is in front of the target, along its facing direction. VaM's UI layer is left out.")
				.P("views", "array", "List of {position:[x,y,z], look_at:[x,y,z] or \"Atom[/control]\", rotation:[x,y,z] (instead of look_at), fov? (default 45), label?}", false)
				.P("orbit", "object", "{target:\"Atom[/control]\", distance? (default 2), height? (metres above the target point, default 0), angles? (degrees, default [0,90,180,270]), fov?}", false)
				.P("width", "number", "Width of each view (default 640, max 2048)", false)
				.P("height", "number", "Height of each view (default 360, max 2048)", false)
				.P("path", "string", "Output PNG (default Saves/PluginData/vam-mcp/snapshot.png)", false)
				.P("return_image", "boolean", "Also return the PNG inline (default false)", false)
				.Timeout(45000).Poller()
				.Fn(api.Snapshot));
			tools.Add(new Tool("capture_sequence", "Frames over time in one grid image (left to right, top to bottom), to see motion: a sit-down, a fade, a face while she speaks. source=camera (default) uses the tool's own camera from the user's current viewpoint (copied once; the user's view never moves) or from view; source=screen grabs the screen itself, which is what shows screen overlays such as fades. Frame 0 is taken before the optional action or animation fires; animation is played the safe way (collisions off until the body is in place, as play_animation). The result lists each frame's time.")
				.P("frames", "number", "Number of frames (default 8, 2-16)", false)
				.P("interval", "number", "Seconds between frames (default 0.5)", false)
				.P("source", "string", "camera (default) or screen", false)
				.P("view", "object", "For source=camera: {position, look_at ([x,y,z] or \"Atom[/control]\") or rotation, fov?}; default: the user's current viewpoint", false)
				.P("action", "object", "Fire after frame 0: {uid, storable, action}, as call_action", false)
				.P("animation", "string", "Fire after frame 0: play this Timeline animation safely, as play_animation", false)
				.P("person", "string", "Person for animation (default: the first one)", false)
				.P("storable", "string", "Timeline storable for animation (default: the person's only one)", false)
				.P("width", "number", "Width of each frame (default 480)", false)
				.P("height", "number", "Height of each frame (default 270)", false)
				.P("hide_ui", "boolean", "For source=screen: hide VaM's main UI while capturing (default true)", false)
				.P("path", "string", "Output PNG (default Saves/PluginData/vam-mcp/sequence.png)", false)
				.Timeout(120000).Poller()
				.Fn(api.CaptureSequence));

			// ---------- ui ----------
			tools.Add(new Tool("set_ui", "Show or hide VaM's main UI and/or deselect the selected atom so its panel closes. With no arguments it only reports whether the UI is visible.")
				.P("visible", "boolean", "true shows the main UI, false hides it", false)
				.P("deselect", "boolean", "Deselect the selected atom, closing its panel (default false)", false)
				.Fn(api.SetUi));
			tools.Add(new Tool("pending_dialogs", "Everything on screen that waits for the user: plugin trust prompts (\"Allow load of plugins from ...?\"; a pending one also holds a scene load open), alerts, load and overwrite confirmations, open file browsers, the Hub browser and its missing packages panel, the error splash and error log, and the Voxta API key prompt. Each with its kind, text and the buttons it offers. Only reports, never clicks: trust prompts are the user's to answer. Also returns whether a scene is loading and the error count.")
				.Fn(a => api.PendingDialogs()));

			// ---------- simulation ----------
			tools.Add(new Tool("set_simulation", "Pause/resume the simulation or set the global time scale.")
				.P("paused", "boolean", "true pauses, false resumes", false)
				.P("timeScale", "number", "Global time scale (1 = normal)", false)
				.Fn(api.SetSimulation));
			tools.Add(new Tool("reset_simulation", "Reset the physics simulation (collapses soft-body / cloth jitter).")
				.Fn(a => api.ResetSimulation()));

			// ---------- hub / community resources ----------
			tools.Add(new Tool("hub_browse", "Browse the VaM Hub (community content: looks, clothing, scenes, plugins...). Queries the game's own Hub API.")
				.P("search", "string", "Search text", false)
				.P("page", "number", "Page number (default 1)", false)
				.P("perpage", "number", "Results per page (default 20)", false)
				.P("sort", "string", "Sort order (e.g. resource_update_date, downloads, rating)", false)
				.P("type", "string", "Content type filter", false)
				.Timeout(90000).Poller()
				.Fn(api.HubBrowse));
			tools.Add(new Tool("hub_detail", "Get full detail of one Hub resource: package name, author, version and all downloadable .var files + dependencies. Use the package name from here with hub_download.")
				.P("package", "string", "Package name (e.g. AcidBubbles.Timeline)", false)
				.P("resource_id", "string", "Hub resource id from hub_browse (alternative to package)", false)
				.Timeout(90000).Poller()
				.Fn(api.HubDetail));
			tools.Add(new Tool("hub_download", "Download a package from the VaM Hub using the game's own downloader (auth + install into AddonPackages). Package name from hub_browse/hub_detail results.")
				.P("package", "string", "Package name, e.g. AcidBubbles.Timeline", true)
				.Timeout(300000).Poller()
				.Fn(api.HubDownload));

			// ---------- skin ----------
			tools.Add(new Tool("set_skin_sss", "Set the skin subsurface-scattering (SSS) tint of a person via the skin material's 'Subsurface Color' (_SubdermisColor).")
				.P("person", "string", "Person uid (default: first Person)", false)
				.P("color", "string", "#RRGGBB color (e.g. #C06060 reddish subsurface tint)", true)
				.Fn(api.SetSkinSss));

			// ---------- plugins (VaM native plugin system) ----------
			tools.Add(new Tool("list_plugins", "List plugins on an atom's PluginManager. Empty uid = CoreControl (session plugins); pass uid=<person> for a person's plugins. A loaded plugin also registers as a storable of that atom — configure it via list_storable_params/set_param with storable=<plugin_uid>.")
				.P("uid", "string", "Atom uid (default CoreControl = session plugins)", false)
				.Fn(api.ListPlugins));
			tools.Add(new Tool("add_plugin", "Add a VaM plugin (.cs/.cslist/.dll) to an atom's PluginManager and wait until it compiles/loads. Default target = CoreControl (session plugins); pass uid=<person> to attach it to a person.")
				.P("path", "string", "Plugin path, e.g. Custom/Scripts/MyPlugin.cs or a package path", true)
				.P("uid", "string", "Atom uid (default CoreControl = session plugins)", false)
				.Timeout(90000).Poller()
				.Fn(api.AddPlugin));
			tools.Add(new Tool("remove_plugin", "Remove a plugin from an atom's PluginManager.")
				.P("plugin_uid", "string", "Plugin uid from list_plugins", true)
				.P("uid", "string", "Atom uid (default CoreControl = session plugins)", false)
				.Fn(api.RemovePlugin));

			// ---------- eval escape hatch ----------
			if (allowEval) {
				int evalTimeoutMs = Plugin.cfgEvalTimeoutSec.Value > 0 ? Plugin.cfgEvalTimeoutSec.Value * 1000 : 30000;
				tools.Add(new Tool("eval_cs", "Execute arbitrary C# inside the VaM process. 'sc' is bound to SuperController.singleton. Without ';' the code is treated as an expression and its value is returned; with ';' it runs as statements (use an explicit return if you want a value). VaM's runtime sandbox prohibits System.IO, System.Reflection, System.AppDomain, UnityEditor and Mono.Cecil; avoid GetType().Name (use ToString()). USE AT YOUR OWN RISK.")
					.P("code", "string", "C# expression or statements to evaluate", true)
					.Timeout(evalTimeoutMs)
					.Fn(api.EvalCs));
			}

			return tools;
		}
	}
}
