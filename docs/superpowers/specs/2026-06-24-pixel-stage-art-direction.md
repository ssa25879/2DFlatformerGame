# Pixel Stage Art Direction

## Applied Stage: Mystical Forest Ruins

The current stage art uses a pixel-sprite direction built around a mystical forest ruin. It is intended for a 2D dash platformer where gameplay readability matters more than dense decoration.

- Background: `Sky.png` / `BackgroundLayer_ForestRuins.png` contain only sky, clouds, and far mountains.
- Environment: `EnvironmentLayer_ForestRuins.png` is transparent and contains non-colliding trees, ruin silhouettes, lower vines, and ambience pixels.
- UI safety: the top 360px of `EnvironmentLayer_ForestRuins.png` must stay fully transparent because score and stage information will be displayed there.
- Top readability: apply the upper UI-zone rule to both background and environment layers. Avoid dark/green solid lines, long vines, trunks, canopy edges, mountain hatching, or strong diagonal mountain edges in the upper UI zone. Use only quiet sky and low-contrast clouds there; keep mountain silhouettes lower on the screen.
- Mountain silhouette: background mountains should use rounded rolling ridges rather than sharp triangular peaks.
- Platforms: `Platform_Long.png` and `Platform.png` are separated collidable moss-covered stone ruin beams with readable horizontal tops and repeated block seams.
- Traps: `Obstacle.png` / `Trap_RootSpikes.png` are separated root-and-stone spike clusters with sharp silhouettes, cyan crystals, and transparent background.
- Palette: deep forest green, teal mist, muted stone gray, moss green, small cyan/gold accent pixels.
- Readability rule: playable objects stay higher contrast than distant background details; no pure black is used as the core palette.

## Runtime Layer Intent

Use separate Unity sorting/parallax layers rather than baking gameplay objects into the background.

1. Background layer: `Sky.png` or `BackgroundLayer_ForestRuins.png`, slowest parallax, no collision.
2. Environment layer: `EnvironmentLayer_ForestRuins.png`, transparent overlay, slower than gameplay, no collision.
3. Platform layer: `Platform_Long.png` / `Platform.png`, collidable and movable/upgradable.
4. Character layer: player and interactable actors.
5. Terrain-object layer: `Obstacle.png` / `Trap_RootSpikes.png`, hazards and foreground props.

## Current Main Scene Application

`Assets/Scene/Main.unity` includes a `Pixel Stage Art Layers` root with three background chunks and three environment chunks. Legacy `BackgroundLoop` renderers are disabled when `CodexDashPlatformerSceneSetup.ApplyPixelStageArtMap()` is run.

Existing stage platform objects keep their collider positions and sizes, but their `SpriteRenderer` uses `Platform_Long.png` in sliced mode. Trap objects keep their trigger colliders and use `Obstacle.png` in white color, not a black test tint.

## Preview-Style Polish Pass

The current 2026-06-24 polish pass keeps the separated runtime files intact while moving the art closer to the original preview direction: brighter teal sky, rounded distant ridges, denser lower ruin silhouettes, mossy stone platforms, gold rune pixels, and cyan root-spike crystals.

`Sky.png`, `BackgroundLayer_ForestRuins.png`, and `EnvironmentLayer_ForestRuins.png` remain horizontally seamless. `EnvironmentLayer_ForestRuins.png` still keeps the top 360px fully transparent for score and stage UI.

The later Toko scale adjustment reduces oversized block pixels and uses smaller 1-2px highlights, 2-4px texture marks, and finer platform/trap details so the stage art reads closer to the pixel density of `Toko_Run.png`.

The environment foliage polish keeps the HUD-safe transparent top area while adding mid/lower-layer leaf clusters, short segmented hanging vines, moss on ruin fragments, and small ambient pixels. These details should enrich the non-colliding environment layer without covering platform readability.

The environment tree object pass adds separated 4-frame idle sprite sheets for round, tall, and vine-covered trees under `Assets/Sprites/EnvironmentTrees`. These sprites are intended to be placed as independent environment objects after preview approval, with subtle 1-2px leaf sway rather than baking the trees into `EnvironmentLayer_ForestRuins.png`.

The large-tree revision changes those sheets from small decorative trees into larger ancient forest objects that better match the current stage scale. Proposed placement should keep them behind gameplay sprites on the Environment sorting layer, using a mix of opacity and scale so they feel embedded in the background rather than blocking platform readability.

The ruin environment revision makes `EnvironmentLayer_ForestRuins.png` read more clearly as a forest ruin by adding broken arches, explicit stone pillars, rune blocks, moss caps, and lower rubble silhouettes. Environment tree placement should use platform-adjacent positions behind gameplay objects, with the environment layer drawn over parts of the trees so they feel integrated with the ruin layer.

The background bounds revision extends horizontal stage coverage from three chunks to seven chunks and adds `AbyssLayer_ForestRuins.png` as a darker below-stage fill. This prevents the camera from exposing empty edges when the player moves left or falls below the main platform area.

The environment lower extension adds `EnvironmentLowerLayer_ForestRuins.png` below the main environment layer. It continues vines, roots, broken ruin walls, and deeper stone silhouettes downward so falling below the main platforms does not expose a hard cutoff in the environment art.

The sky upper extension adds two rows of `Sky.png` above the main background row. This keeps clean sky visible when the camera follows a high jump above the original background image bounds.

The sky seam fix replaces the upper rows with `SkyUpperLayer_ForestRuins.png`, a flatter sky tile whose top and bottom edges share the same tone. This avoids the visible vertical gradient cut that happened when stacking the original `Sky.png`.

The explorer character pass adds gender-ambiguous hooded explorer sprite sheets under `Assets/Sprites/Explorer`, matching the current Toko sprite scale: 64px frame height, PPU 32, Point filtering, and separate Idle, Run, Dash, and Die sheets. Trap hit preparation keeps the spike renderer behind platforms while moving the trigger area upward to better cover the visible spike tips.

The explorer player swap applies those sheets to the active player controller: `Idle.anim`, `Run.anim`, `Jump.anim`, `Dash.anim`, and `Die.anim` now reference the explorer frames, while the Main scene player default sprite uses `Explorer_Idle_0`. Rollback snapshots for the pre-swap state are stored under `Assets/Scene/Backup/20260624_before_explorer_player_swap` and `Assets/Animations/Backup/20260624_before_explorer_player_swap`.

The character feedback pass fixes the player floating issue by returning Explorer sprite pivots to center alignment and tuning the Main scene player collider to `0.76 x 1.34` with a slight downward offset. Explorer sheets were redrawn with clearer hood, backpack, scarf, boot, and rune-accent details. Dash recharge pickups now use `DashRecharge_RuneSeed.png`, a teal ruin-seed sprite with white tint and smaller `0.78` trigger radius so the pickup reads closer to the current forest ruin palette.

The forest scout character revision removes the space-explorer read from the player sprite by replacing the square hood/teal glow language with a grounded ruin-scout silhouette: brimmed cloth/leather hat, khaki shirt, olive neck cloth, leather satchel, rope detail, dark boots, and muted earth colors. This keeps the character gender-ambiguous while moving the identity closer to forest ruin exploration than sci-fi equipment.

The side-facing player revision changes the forest scout sheets from camera-facing poses to right-facing movement poses. Gameplay direction is handled by `PlayerController` through `SpriteRenderer.flipX`, so the same right-facing sheets mirror left without changing transform scale or collider behavior.

The fine-pixel character revision keeps the same 64px frame size and PPU 32 but reduces the blocky read by replacing large rectangular fills with smaller polygon silhouettes, 1-2px highlights, thinner limbs, finer hat/face edges, and smaller rope/satchel details. This should keep the scout readable while matching the denser pixel feel of the current stage art more closely.

The Toko-scale character revision keeps the Explorer sprite sheet filenames and animation structure but redraws the visible character closer to the original Toko footprint: about 50px tall inside the 64px frame, with more top/bottom breathing room and finer limb/detail pixels. The Main scene Player transform scale is reduced from `0.66` to `0.58`, which makes the forest scout read closer to the old Toko on-screen size while preserving existing controller and animation wiring.

The Toko-density character revision further compresses the Explorer silhouette inside the same 64px frame so the visible body reads closer to Toko's clustered pixel density. Explorer now uses roughly 33-35px width and 49px height for Idle/Run frames, preserving the forest scout identity while reducing large visual blocks and keeping `PPU 32`, center pivots, and existing animation references stable.

The animation polish revision updates all Explorer motion sheets to better match the current compact scout sprite. Idle now reads as subtle breathing, Run uses smaller side-facing steps, Dash leans forward with restrained rope/cloth streaks, and Die expands from 6 to 8 frames at 5fps for a longer 1.6s stumble-and-fall sequence. The Die clip remains non-looping.

## Deferred Stage: Sky-Island Ruins

Keep this direction for a later stage, not for the current forest pass.

- Background: 2-3 parallax layers with pale clouds, far floating islands, and distant broken arches.
- Platforms: cracked ivory-gray floating stone slabs with moss only on the underside, lighter than the forest stage.
- Traps: crystal spikes or fractured ruin shards, using cool blue/cyan highlights.
- Palette: sky blue, soft cloud white, desaturated stone, cyan crystal accent, small warm sunlit edge pixels.
- Gameplay identity: more vertical gaps and airborne routes than the forest stage; hazards should feel sharper and more crystalline.

## Style Rules

- Use pixel-art silhouettes with crisp edges and no painterly blur.
- Preserve Unity sprite import settings and existing GUIDs where possible so scene references remain stable.
- Keep platform tops visually flat and obvious.
- Keep hazards readable at gameplay scale before adding ornament.
- Prefer reusable textures over one-off decoration.
