# Environment Tree Idle Sprites Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add separated pixel tree object sprites with simple idle frames and a placement preview for user approval.

**Architecture:** Keep the current baked background and environment layers intact. Add new tree sprite sheets under `Assets/Sprites/EnvironmentTrees`, configure them as Multiple sprites in Unity, and generate a non-runtime placement preview before committing to scene placement.

**Tech Stack:** Unity sprites, C# editor import runner, System.Drawing PNG generation.

---

### Task 1: Generate Tree Sprite Sheets

**Files:**
- Create: `Assets/Sprites/EnvironmentTrees/EnvironmentTree_Round_Idle.png`
- Create: `Assets/Sprites/EnvironmentTrees/EnvironmentTree_Tall_Idle.png`
- Create: `Assets/Sprites/EnvironmentTrees/EnvironmentTree_Vine_Idle.png`
- Create: `Assets/Sprites/EnvironmentTreePlacementPreview.png`

- [ ] Create three transparent 4-frame pixel sprite sheets using the current forest palette.
- [ ] Keep idle motion subtle: leaf mass shifts 1-2px, vine tips shift 1-3px.
- [ ] Compose a preview using current background/environment, proposed tree placements, current platforms, and current traps.

### Task 2: Configure Unity Sprite Imports

**Files:**
- Create: `Assets/Editor/CodexEnvironmentTreeImportRunner.cs`

- [ ] Add an editor runner that imports the three tree sheets as Multiple sprites.
- [ ] Use Point filtering, no mipmaps, uncompressed texture settings, and PPU 32 to match `Toko_Run.png`.
- [ ] Do not place objects into `Main.unity` until the user approves the preview.

### Task 3: Verify And Report

**Files:**
- Modify: `docs/superpowers/specs/2026-06-24-pixel-stage-art-direction.md`
- Append: `D:\Codex\Log\20260624_Uni-Run_Log.md`

- [ ] Verify generated image dimensions and transparent backgrounds.
- [ ] Run the Unity import runner and confirm there are no compilation errors.
- [ ] Show `EnvironmentTreePlacementPreview.png` to the user for approval.
