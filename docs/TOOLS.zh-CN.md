# 工具参考（68 个）

*[English](TOOLS.md)*

所有工具通过 `tools/call` 调用：`{ "name": "...", "arguments": { ... } }`。
参数标 `*` 为必填、`?` 为可选。`person` 省略时默认使用场景中第一个 Person。数值数组均为 `[x, y, z]`。

`eval_cs` 仅在 `Security.AllowEval=true` 时注册，所以默认安装暴露 62 个工具。

## 状态与会话

### status
VaM 运行时状态：插件版本、VaM 版本、原子/人物数量、端点、eval 开关。

### read_log
VaM 及其插件输出的日志，每行带日期和时间（VaM 自己的 output_log.txt 没有时间戳）。插件启动后在内存中保留最新 5000 行，场景加载过程中也能读取。参数：`since`（默认 `scene_load`，或 `all`）、`level`（默认 `error`：错误、异常、断言；`warning`；`all`）、`contains`、`after`（上次调用返回的 `last_seq`，只取新行）、`group`（默认 true：同一来源的相同行合并，带 `count` 和 `first_time`）、`stack`（默认 true，前 8 行）、`limit`（默认 100）。每条包含 `time`、`level`、`message`，有调用栈时还有 `source`（栈顶帧，可看出是哪个插件）。Unity 引擎原生消息（如 SoundManager）不会出现。

## 场景

| 工具 | 参数 | 说明 |
| --- | --- | --- |
| `list_scenes` | `search?` | 列出 `Saves/scene` 下的场景 |
| `load_scene` | `path`*, `merge?` | 加载场景（`merge=true` 合并进当前场景）；支持 .var 包路径（`Package.1:/Saves/scene/x.json`） |
| `new_scene` | — | 新建空场景（**丢弃当前场景**；空场景无默认灯光，记得打灯/开环境光） |
| `save_scene` | `path?` | 保存场景（默认 `Saves/scene/VaMMCP_<时间戳>.json`；覆盖已存在文件会自动先删后存） |

## Atom

| 工具 | 参数 | 说明 |
| --- | --- | --- |
| `list_atom_types` | — | 可创建的原子类型（按分类，来自 VaM 运行时数据） |
| `list_atoms` | `type?` | 场景原子列表（uid/type/on/位置/旋转） |
| `add_atom` | `type`*, `uid?` | 创建原子，等待其就绪（最长 45s） |
| `remove_atom` | `uid`* | 删除原子 |
| `set_atom_on` | `uid`*, `on`* | 显示/隐藏 |
| `get_atom_transform` | `uid`* | 世界坐标/欧拉角/缩放 |
| `set_atom_transform` | `uid`*, `position?`, `rotation?`, `scale?`, `relative?` | 变换（`relative=true` 为相对位移） |

## 人物

| 工具 | 参数 | 说明 |
| --- | --- | --- |
| `add_person` | `uid?` | 添加人物（uid 可能被 VaM 重命名，以返回值为准） |
| `list_persons` | — | 人物列表（uid/角色名/性别/位置） |
| `list_looks` | `search?` | 外观预设列表（`Saves/Person/Appearance`） |
| `load_look` | `path`*, `person?` | 应用外观预设（角色/服装/发型/morph 增量；异步，稍后验证） |
| `save_look` | `path?`, `person?` | 导出外观预设（**注意**：VaM 的预设保存不含完整材质参数，跨场景搬运后需按需补设皮肤参数） |
| `set_character` | `person?`, `name`* | 切换基础角色网格（名称见 `get_param storable=geometry param=characterSelection` 的 choices） |

## 预设导出

| 工具 | 说明 |
| --- | --- |
| `save_look` | 外观（角色/服装/发型/morph）→ `Saves/Person/Appearance/` |
| `save_pose` | 姿态+物理 → `Saves/Person/Pose/` |
| `save_full_preset` | 全量（外观+姿态+物理）→ `Saves/Person/full/` |

## 捏人（Morph）

| 工具 | 参数 | 说明 |
| --- | --- | --- |
| `list_morphs` | `person?`, `search?`, `region?`, `group?`, `limit?` | 搜索 morph（含名称/uid/区域/分组/当前值） |
| `set_morph` | `person?`, `name`*, `value`* | 设置 morph 值（典型范围 -1..1） |
| `get_morph` | `person?`, `name`* | 读取 morph 值 |
| `reset_morphs` | `person?`, `region?` | 归零（可按区域） |

## 服装 / 发型

| 工具 | 参数 | 说明 |
| --- | --- | --- |
| `list_packages` | — | 已安装的 .var 包 |
| `list_clothing_presets` | `search?` | 服装预设/物品：磁盘 + **递归扫描所有包**的 `Custom/Clothing`（.vam/.json） |
| `load_clothing_preset` | `path`*, `person?` | 应用服装预设 |
| `list_clothing_items` | `person?` | 人物当前服装物品（uid/名称/active） |
| `add_clothing_item` | `path`*, `person?` | 添加服装物品（包路径 `.vam`；等待角色就绪后加载） |
| `remove_clothing_item` | `id`*, `person?` | 移除 |
| `set_clothing_item_on` | `id`*, `on`*, `person?` | 穿/脱 |
| `list_hair_presets` / `load_hair_preset` / `list_hair_items` / `add_hair_item` / `remove_hair_item` / `set_hair_item_on` | 同服装 | 发型（`Custom/Hair`） |

## 姿态 / 表情

| 工具 | 参数 | 说明 |
| --- | --- | --- |
| `list_poses` | `search?` | 姿态预设（`Saves/Person/Pose`） |
| `load_pose` | `path`*, `person?`, `all?` | 应用姿态（`all=true` 应用到所有人） |
| `save_pose` | `path?`, `person?` | 导出姿态 |
| `list_expressions` | `person?` | 表情 morph 列表（启发式过滤） |
| `set_expression` | `person?`, `name`*, `value?`, `reset?` | 设置表情（默认先清空其他表情再设，value 默认 1） |

## 动画

| 工具 | 参数 | 说明 |
| --- | --- | --- |
| `play_animation` | `animation`*, `person?`, `storable?`, `tolerance?`, `timeout?` | 播放 Timeline 动画（`Play <animation>`）前关闭人物碰撞，等臀部、胸部、头部与其控制点的距离都在 `tolerance` 米以内（默认 0.05）后再打开。人物不在动画起始位置时，身体会飞过场景、撞到沿途的一切，所以请用它代替 `call_action`。若 `timeout` 秒内（默认 8）未就位，碰撞保持关闭，并在结果中说明 |
| `list_animations` | `uid?`, `targets?` | 读取 Timeline 中的动画。不带 `uid`：每个带 Timeline 的原子一条摘要（片段数、正在播放的内容、问题）。带 `uid`：列出每个片段的分段、层、时长、循环、速度、是否播放，以及驱动的控制器、浮点参数（形变等）和触发轨道数量；`targets=true` 时同时列出名称。会标出名称、分段或层中隐藏的控制字符（以转义形式显示，如 `\r`），以及同一分段和层中重复的名称 |

## 骨骼控制

| 工具 | 参数 | 说明 |
| --- | --- | --- |
| `list_controls` | `person?` | FreeControllerV3 控制点（headControl / chestControl / …） |
| `get_control` | `person?`, `control`* | 控制点世界位置/欧拉角 |
| `set_control` | `person?`, `control`*, `position?`, `rotation?` | 移动/旋转控制点 |
| `set_gaze` | `person?`, `amount?`, `targetControl?` | 视线（尽力而为；精确控制见 lookAt storable 参数） |

## 通用参数（长尾全覆盖）

| 工具 | 参数 | 说明 |
| --- | --- | --- |
| `list_atom_storables` | `uid`* | 原子全部 storable（含类型） |
| `list_storable_params` | `uid`*, `storable`* | 参数内省：floats（含 min/max/default）/bools/strings/choosers/colors/actions/customParams/properties |
| `get_param` | `uid`*, `storable`*, `param`* | 读参数（float/bool/string/chooser/color/property 自动识别） |
| `set_param` | `uid`*, `storable`*, `param`*, `value`*, `type?` | 写参数（颜色支持 `#RRGGBB` 或 `r,g,b,a`） |
| `call_action` | `uid`*, `storable`*, `action`* | 触发 storable 的动作（按钮） |

`type` 可强制指定：`float|bool|string|chooser|color|property`（`property` 走反射读写公共属性，覆盖 customParamNames 如 `gravityX`）。

## Voxta

| 工具 | 参数 | 说明 |
| --- | --- | --- |
| `voxta_state` | `person?` | 人物身上 Voxta 插件当前掌握的信息：`connection`（是否连接、状态文本、最近错误）、`chat`（是否激活、是否就绪、聊天 ID、用户名、角色、场景剧本）、`state`（off/idle/listening/thinking/speaking）、服务器下发的 `current_states` 和 `available_actions`、`current_action`、`last_message`，以及非空的 `context` 槽位和 `action_lists`。读取的是插件实际收到的内容，因此适用于任何动作配置方式（文本动作列表、Actions 标签页、剧本动作） |

## 相机

| 工具 | 参数 | 说明 |
| --- | --- | --- |
| `get_camera` | — | 监视器相机位置/旋转/FOV |
| `set_camera` | `position?`, `rotation?`, `fov?` | 移动相机 |
| `capture_view` | `width?`, `height?`, `path?`, `return_image?`, `hide_ui?` | 截取显示器画面，在帧末截取，人物皮肤完整（默认 `Saves/PluginData/vam-mcp/preview.png`，宽 1280，高按屏幕比例）。`hide_ui=true` 时截图期间隐藏 VaM 主界面，截完再恢复。`return_image=true` 时同时以 MCP image 块返回 PNG，读不到 VaM 目录的客户端也能"看"到——分辨率别开太大（如 640×360），超过 4 MB 只落盘不内联 |

## 界面

| 工具 | 参数 | 说明 |
| --- | --- | --- |
| `set_ui` | `visible?`, `deselect?` | 显示（`visible=true`）或隐藏（`visible=false`）VaM 主界面；`deselect=true` 取消选中当前 atom，关闭其面板。不带参数时只返回 `visible` |

## 模拟

| 工具 | 参数 | 说明 |
| --- | --- | --- |
| `set_simulation` | `paused?`, `timeScale?` | 暂停/恢复/时间缩放 |
| `reset_simulation` | — | 重置物理模拟（软体/布料抖动） |

## Hub 社区资源

| 工具 | 参数 | 说明 |
| --- | --- | --- |
| `hub_browse` | `search?`, `page?`, `perpage?`, `sort?`, `type?` | 搜索 VaM Hub（返回 id/title/作者/版本/类型/下载量/评分/tags） |
| `hub_detail` | `package?`, `resource_id?` | 资源详情：包名 + 所有版本 .var 文件 + 依赖 |
| `hub_download` | `package`* | 用游戏自带下载器下载安装（处理鉴权/依赖；免费内容匿名可用，付费需在 VaM 登录） |

## 皮肤

| 工具 | 参数 | 说明 |
| --- | --- | --- |
| `set_skin_sss` | `person?`, `color`* | 设置次表面反射色（`_SubdermisColor`；粉调如 `#FFC9C9` 显白皙） |

## 插件管理（VaM 原生插件）

| 工具 | 参数 | 说明 |
| --- | --- | --- |
| `list_plugins` | `uid?` | 插件列表（空 uid = CoreControl 会话插件；传人物 uid 看人物插件） |
| `add_plugin` | `path`*, `uid?` | 挂载 .cs/.cslist/.dll 插件，等待编译完成；加载后插件成为 storable，用 `list_storable_params`/`set_param` 配置（storable id 形如 `plugin#0_MVRPlugin.类名`） |
| `remove_plugin` | `plugin_uid`*, `uid?` | 移除插件 |

## 逃生舱

### eval_cs（默认关闭，`Security.AllowEval=true` 开启）
- 无 `;` → 表达式模式，返回表达式的值
- 含 `;` → 语句模式（需要值时用显式 `return`）
- `sc` 绑定 `SuperController.singleton`
- 受 VaM 运行时沙箱限制：System.IO / System.Reflection / System.AppDomain / UnityEditor / Mono.Cecil 禁用（注意 `GetType().Name` 会引用 System.Reflection，用 `ToString()` 代替）
