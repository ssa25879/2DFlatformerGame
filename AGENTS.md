# Uni-Run Agent Instructions

## StyleSeed UI Rules

Source checked: https://styleseed-demo.vercel.app/llms.txt and linked full context on 2026-06-18.

Apply StyleSeed's design rules to every UI built or modified in this project.

### Core Rules

- Prioritize accessibility first: visible focus states, semantic controls, contrast that meets WCAG AA, and interactive targets at least 44x44px.
- Keep UI surfaces card-based: important content belongs inside clear panels/cards, not floating on a bare background.
- Use one primary accent color and grayscale for the rest. Do not scatter multiple accent colors across a screen.
- Do not use pure black (`#000000`) for UI text or surfaces; use the project's darkest semantic text color instead.
- Prefer semantic color/style tokens over hardcoded colors. In Unity UI, this means shared palette constants, ScriptableObjects, prefabs, or documented theme values rather than one-off inspector colors.
- Use restrained shadows and elevation. If a shadow is visually loud, reduce it.
- Use consistent spacing rhythm: 24px-like section spacing, aligned page margins, and repeated spacing units instead of arbitrary offsets.
- Keep typography intentional. Large metrics should keep a clear 2:1 number-to-unit relationship where applicable.
- Vary section rhythm. Avoid repeating the same section type or same-height cards consecutively; alternate hero, KPI, chart/progress, list, and state/feedback sections when building dashboards.
- Every data surface needs empty, loading, error, and success states. Empty states should guide the user toward the next action.
- Charts or progress visuals must include contextual labels/numbers; visuals without context are decoration.
- Forms should be single-column when possible, grouped by topic, with labels above fields and errors that explain recovery.
- Motion should be named, consistent, brief, and respectful of reduced-motion preferences. Use subtle press/hover/entry feedback only where it clarifies interaction.

### Unity Adaptation

- Treat canvases, panels, prefabs, TextMeshPro styles, sprites, materials, and serialized color fields as the Unity equivalent of StyleSeed tokens/components.
- When adding UI scripts or prefabs, keep controls touch-friendly and controller-friendly, with clear selected/hover/pressed/disabled states.
- Before finishing UI work, review the screen against the StyleSeed priorities: accessibility, prohibited patterns, page/screen job, hierarchy, domain fit, then local aesthetics.
