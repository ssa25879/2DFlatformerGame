## 10. UI 시스템: Stone Tablet (1A)

> 이 섹션은 게임 UI 시안 검토 결과 확정된 방향과 실제 배치 지침을 정리한 것이다.
> `DESIGN_GUIDE.md` 최하단에 그대로 이어 붙여 사용한다.

### 10.1 확정 방향

- 채택 시안: **1A "Stone Tablet"** — 상단 통합 석판 바 + 골드 룬 강조.
- 강조색은 화면당 1개(rune gold `#e8b24a`)만 사용하고, 나머지는 석재 그레이스케일 + 의미 색(위험 레드, 룬 시안)으로 정리한다. (StyleSeed)
- 텍스트/표면에 순수 검정 `#000000`, 순수 흰색 `#ffffff`을 쓰지 않는다. 기본 텍스트는 cream `#eef2ec`, 수치 강조는 `#f3d38a`.
- 폰트: 제목·수치는 두꺼운 픽셀 계열(시안 제작 시 Pixelify Sans), 라벨·캡션은 촘촘한 픽셀 계열(Silkscreen). Unity에서는 이에 대응하는 픽셀 TMP 폰트 에셋으로 매핑한다.
- 수치 텍스트(스코어/타임/룬/스테이지명)는 스프라이트 PNG에 굽지 않는다. **프레임/게이지/아이콘만 스프라이트**로 두고, 값은 **TextMeshPro**로 얹는다.

### 10.2 에셋 배치 위치 (프로젝트 폴더)

```
Assets/Sprites/UI/
├─ HUD/
│  ├─ HUD_Bar_Frame.png          1120x96   (9-slice)  상단 통합 석판 바
│  ├─ HUD_ScorePlaque.png        220x68    (9-slice)  스코어 플라크 + 룬 아이콘 슬롯
│  ├─ HUD_RuneDiamond.png        48x48                룬 다이아 아이콘(단독)
│  ├─ HUD_StagePanel.png         440x68    (9-slice)  스테이지 정보 패널
│  ├─ HUD_ProgressBar_Track.png  400x16    (9-slice)  진행도 빈 트랙
│  ├─ HUD_ProgressBar_Fill.png   400x12    (Filled)   진행도 골드 채움
│  ├─ HUD_DashGauge_Track.png    250x26    (9-slice)  대시 게이지 빈 바(노치)
│  ├─ HUD_DashGauge_Fill.png     246x22    (Filled)   대시 게이지 골드 채움
│  ├─ HUD_DashGauge_Empty.png    250x26               대시 소진(레드) 상태
│  └─ HUD_Divider.png            6x64                 섹션 세로 구분선
└─ Result/
   ├─ Panel_Result_Frame.png     560x380   (9-slice)  결과 오버레이 패널
   ├─ Button_Gold.png            480x56    (9-slice)  기본 골드 버튼
   ├─ Button_Gold_Pressed.png    480x56    (9-slice)  눌림 상태 버튼
   ├─ StatRow_Frame.png          480x52    (9-slice)  스탯 행 프레임
   ├─ Title_Band.png             492x60               제목 상하 룰 + 룬 다이아
   └─ Rule_Horizontal.png        400x6                가로 구분 룰
```

- 룬 시드 아이콘은 신규 제작하지 않고 기존 `Assets/Sprites/DashRecharge_RuneSeed.png`를 재사용한다.
- Import 설정: **Filter Mode = Point (no filter)**, **Compression = None**, PPU는 게임 기준과 통일, Mesh Type = Full Rect.
- `(9-slice)` 표기 스프라이트는 Sprite Editor에서 Border를 지정한다(권장 Border: 프레임류 상하좌우 8px, 버튼 12px, 트랙류 6px). 폭·높이가 가변이어도 모서리 픽셀이 늘어나지 않는다.
- `(Filled)` 표기 스프라이트는 Image 컴포넌트 **Image Type = Filled / Horizontal / Origin Left**로 두고 `fillAmount`(0~1)로 게이지·진행도를 제어한다.

### 10.3 인게임 HUD 배치 (Canvas: Screen Space – Overlay)

상단 전용 영역. 강한 실선/복잡한 장식을 넣지 않고, 플레이 판정 레이어와 겹치지 않게 화면 최상단에만 둔다.

- **HUD_Bar_Frame** — Anchor: Top-Stretch. 좌우 stretch(Left/Right = 0), Top = 0, Height = 96. 9-slice로 어떤 해상도에서도 가로 전체를 덮는다.
- **ScorePlaque** — Anchor: Top-Left. Pos ≈ (16, -14). 내부에 룬 아이콘 슬롯(좌) + TMP "SCORE" 라벨 + TMP 스코어 수치(우).
- **Divider** — ScorePlaque 오른쪽, 그리고 대시 영역 왼쪽에 각각 1개.
- **StagePanel(중앙)** — Anchor: Top-Center. 스테이지명 TMP + `STAGE x-y` 칩 TMP + 그 아래 ProgressBar(Track+Fill). 진행도는 목표까지 거리(GoalTrigger 기준)를 `fillAmount`에 매핑.
- **DashGauge** — Anchor: Top-Right. Track+Fill 가로 에너지 바 + 오른쪽 끝에 룬 시드 아이콘. Pos ≈ (-16, -14).
- 모든 인터랙션/아이콘 요소는 최소 44x44px 조작 영역을 유지한다.

상태 연동:
- 대시 충전량 → `HUD_DashGauge_Fill`의 `fillAmount`.
- 대시 소진(`PlayerController.CanDash == false`) → Fill 숨기고 `HUD_DashGauge_Empty`로 교체하거나 tint를 레드로.
- 룬 시드 획득 시 게이지 채움 피드백(짧은 flash/scale, 모션은 짧고 일관되게).

### 10.4 결과 화면 배치 (게임오버 / 스테이지 클리어)

GameManager의 `gameoverUI` / `clearUI` GameObject에 각각 대응. 동일한 패널 구조를 공유하고 제목·버튼·스탯만 달라진다.

공통 구조 (화면 중앙, 세로 스택):
1. **Dim Overlay** — 전체 화면 Image, `rgba(8,16,15,0.82)` 정도의 반투명. 배경 아트를 어둡게 눌러 패널 가독성 확보.
2. **Panel_Result_Frame** — Anchor: Center, 560x380, 9-slice.
3. 패널 내부(위→아래):
   - (클리어 전용) 룬 시드 크레스트 아이콘 — 상단 중앙.
   - **Title_Band** + TMP 제목 — 게임오버 `GAME OVER`(레드 다이아 + `#e7b6ab`), 클리어 `STAGE CLEAR`(골드 다이아 + `#f3d38a`).
   - **StatRow_Frame** + TMP — 게임오버: `SCORE`, `BEST`. 클리어: `SCORE` 1행 + `TIME`/`RUNES` 2분할.
   - **Button_Gold** — 게임오버 `RETRY`, 클리어 `NEXT STAGE`. Height ≥ 56px. 눌림 시 `Button_Gold_Pressed`.
   - 재시작 안내 TMP — `SPACE / CLICK TO RESTART`(또는 CONTINUE), 느린 blink. GameManager의 재시작 입력(좌/우 클릭·Space)과 일치시킨다.

### 10.5 색 토큰

| 토큰 | 값 | 용도 |
|---|---|---|
| stone.top / mid / bottom | `#3a4a47` / `#2b3a38` / `#233230` | 석재 패널 그라디언트 |
| border.light / dark | `#56635f` / `#1a2624` | 베벨/외곽선 |
| accent.gold / light / dark | `#e8b24a` / `#f3d38a` / `#c1892c` | 주 강조, 게이지, 버튼 |
| moss | `#4c7d2b` `#5a9433` `#6fae3e` | 상단 이끼 트림 |
| rune.cyan | `#7fd6e0` | 룬/획득 수치 강조 |
| danger.red | `#c15c4a` | 게임오버, 대시 소진 |
| text.cream / sub | `#eef2ec` / `#8fb7ac` | 기본 텍스트 / 보조 라벨 |
| track.bg | `#1c2a28` / `#1a2624` | 게이지·스탯 배경 |

### 10.6 상태 처리 체크리스트

- 게이지: Full(ready) / Charging(진행 stripe) / Empty(레드) 3상태를 모두 정의한다.
- 버튼: Normal / Pressed(눌림) 최소 2상태 + 포커스 가시 표시.
- 결과 화면: 스코어 0, 신기록 갱신, 값 미표시 등 edge 상태에서도 레이아웃이 깨지지 않게 한다.
- 검증: 해상도 변경 시 9-slice 늘어남, TMP 오버플로우, HUD가 상단 플레이 판정과 겹치지 않는지 확인한다.


## 11. RELIC DASH — Unity 반영 가이드 (실전 수치)

> Section 10의 방향을 실제 씬에 붙이기 위한 구체 수치. 기준 캔버스 해상도는 **1920×1080**.
> 좌표는 `(Anchor / AnchoredPosition / SizeDelta)` 형식이며, 앵커는 해당 코너·엣지에 고정한다.

### 11.0 캔버스 / 임포트 공통

- Canvas: **Screen Space – Overlay**. CanvasScaler: **Scale With Screen Size**, Reference Resolution `1920 x 1080`, Match `0.5`.
- 스프라이트 Import: Texture Type `Sprite (2D and UI)`, **Filter Mode = Point**, **Compression = None**, Mesh Type `Full Rect`, PPU는 게임 스프라이트와 통일.
- 9-slice: Sprite Editor에서 아래 Border(px)를 입력. Image 컴포넌트는 해당 스프라이트에 대해 Image Type = **Sliced**.
- 게이지/바 채움: Image Type = **Filled**, Fill Method `Horizontal`, Fill Origin `Left`, 코드에서 `image.fillAmount = 0~1`.
- 모든 텍스트는 **TextMeshProUGUI**. 스프라이트에는 텍스트를 굽지 않는다.

### 11.1 9-slice Border 표

| 스프라이트 | 원본 크기 | Border L,T,R,B | Image Type |
|---|---|---|---|
| HUD/HUD_Bar_Frame | 1120×96 | 12, 14, 12, 8 | Sliced |
| HUD/HUD_ScorePlaque | 220×68 | 60, 10, 12, 10 | Sliced |
| HUD/HUD_StagePanel | 440×68 | 12, 10, 12, 10 | Sliced |
| HUD/HUD_ProgressBar_Track | 400×16 | 6, 6, 6, 6 | Sliced |
| HUD/HUD_ProgressBar_Fill | 400×12 | 0 | **Filled** (H) |
| HUD/HUD_DashGauge_Track | 250×26 | 8, 6, 8, 6 | Sliced |
| HUD/HUD_DashGauge_Fill | 246×22 | 0 | **Filled** (H) |
| HUD/HUD_DashGauge_Empty | 250×26 | 8, 6, 8, 6 | Sliced |
| HUD/HUD_RuneDiamond | 48×48 | 0 | Simple |
| HUD/HUD_Divider | 6×64 | 0, 8, 0, 8 | Sliced (세로) |
| Result/Panel_Result_Frame | 560×380 | 14, 20, 14, 14 | Sliced |
| Result/Button_Gold(_Pressed) | 480×56 | 12, 12, 12, 14 | Sliced |
| Result/StatRow_Frame | 480×52 | 10, 8, 10, 10 | Sliced |
| Result/Title_Band_Gold/_Red | 492×60 | 24, 4, 24, 4 | Sliced |
| Result/Rule_Horizontal | 400×6 | 8, 0, 8, 0 | Sliced |
| Menu/Button_Stone(_Pressed) | 340×52 | 12, 12, 12, 14 | Sliced |
| Menu/Panel_Menu_Frame | 440×460 | 14, 20, 14, 14 | Sliced |
| Menu/Chip_Best | 200×44 | 10, 8, 10, 8 | Sliced |
| Menu/Icon_Play | 24×24 | 0 | Simple |
| Loading/LoadingBar_Track | 600×26 | 8, 6, 8, 6 | Sliced |
| Loading/LoadingBar_Fill | 596×22 | 0 | **Filled** (H) |

### 11.2 인게임 HUD (1920×1080)

부모: `HUDCanvas` 최상단. 각 요소 RectTransform.

- **BarFrame** — Anchor `Top-Stretch(0,1)~(1,1)` · Left/Right `0` · Top `0` · Height `120` (Pivot y=1).
- **ScorePlaque** — Anchor `Top-Left(0,1)` · Pos `(28, -18)` · Size `(300, 92)`.
  - 자식 `Icon`(RuneDiamond) Pos `(46, 0)` Size `(60,60)`; `Label`(TMP "SCORE") 좌상단; `ScoreValue`(TMP, 우측 정렬, 44pt) — GameManager.scoreText 로 연결.
- **Divider ×2** — Anchor `Top-Left` Size `(6,86)`; ScorePlaque 우측, DashGauge 좌측.
- **StagePanel** — Anchor `Top-Center(0.5,1)` · Pos `(0, -18)` · Size `(620, 92)`.
  - `StageName`(TMP) / `StageChip`(TMP) 상단, `ProgressTrack` 하단 Size `(560,20)`, 자식 `ProgressFill`(Filled) stretch. `fillAmount = 현재거리/목표거리`.
- **DashGauge** — Anchor `Top-Right(1,1)` · Pos `(-28, -18)` · Size `(360, 92)`.
  - `Label`(TMP "DASH"/"READY"), `GaugeTrack` Size `(280,30)` + 자식 `GaugeFill`(Filled), `RuneIcon` 우측 Size `(56,56)`.
  - 소진 시: GaugeFill 숨기고 `DashGauge_Empty`로 스왑(또는 tint 레드). `PlayerController.CanDash` 참조(DashStatusUI 확장).

### 11.3 결과 화면 — Game Over / Stage Clear

부모: `GameOverUI` / `ClearUI` (GameManager의 `gameoverUI`/`clearUI`에 연결, 기본 비활성).

- **DimOverlay** — Anchor `Stretch` 전체, `Result/Overlay_Vignette` (Image Type Simple, 전체 stretch).
- **Panel** — Anchor `Center` · Pos `(0,0)` · Size `(700, 480)` (Panel_Result_Frame, Sliced).
  - **TitleBand** — 상단, Size `(620, 76)`. Game Over = `Title_Band_Red` + TMP `GAME OVER`(#e7b6ab); Clear = `Title_Band_Gold` + TMP `STAGE CLEAR`(#f3d38a). (Clear는 상단에 RuneSeed 크레스트 Size `(80,80)`.)
  - **StatRows** — StatRow_Frame Size `(600, 66)`. Game Over: `SCORE`, `BEST` 2행. Clear: `SCORE` 1행 + `TIME`/`RUNES` 2분할.
  - **PrimaryButton** — Button_Gold Size `(600, 72)`. Game Over `RETRY`, Clear `NEXT STAGE`. Pressed 시 Button_Gold_Pressed. 높이 ≥ 60.
  - **HintText**(TMP) — 하단, `SPACE / CLICK TO RESTART`(또는 CONTINUE), 느린 blink. GameManager 재시작 입력과 일치.

### 11.4 타이틀 / 일시정지

**Title** (부모 `TitleUI`)
- **BestChip** — Anchor `Top-Right` Pos `(-28,-24)` Size `(260,56)` (Chip_Best) + TMP.
- **LogoLockup** — Anchor `Top-Center` Pos `(0,-220)`. RuneSeed 크레스트 + TMP `RELIC DASH`(66~70pt) + Rule + TMP `FOREST RUINS`(부제).
- **MenuButtons** — Anchor `Bottom-Center` Pos `(0,110)`. `PLAY`(Button_Gold Size 440×76, Icon_Play 포함), 아래 `OPTIONS`/`QUIT`(Button_Stone, 2열).

**Pause** (부모 `PauseUI`, Time.timeScale=0 시 활성)
- **DimOverlay** — 전체 stretch (Overlay_Vignette).
- **Panel** — Center Size `(560, 560)` (Panel_Menu_Frame).
  - TitleBand + TMP `PAUSED`, 세로 버튼: `RESUME`(Button_Gold), `RESTART`/`QUIT TO TITLE`(Button_Stone), HintText `ESC TO RESUME`.

### 11.5 로딩 — Splash / Stage Loading

**Splash** (부모 `SplashUI`)
- DimOverlay(Vignette) 전체.
- LogoLockup Center Pos `(0,-40)` (RELIC DASH + FOREST RUINS).
- LoadingBlock Anchor `Bottom-Center` Pos `(0,150)`: RuneSeed 스피너(회전) + TMP `LOADING`/`%`, LoadingBar_Track Size `(760,32)` + Filled Fill, TMP TIP.

**Stage Loading** (부모 `StageLoadingUI`)
- 배경 BackgroundLayer(Image, opacity ~0.28) + Dim.
- 중앙 상단: TMP `NOW ENTERING` / `STAGE x-y`(대형) / `FOREST RUINS`.
- 하단: LoadingBar_Track+Fill Size `(800,28)`, `LOADING` + 룬 도트(LoadingDot ×3 pulse), TMP TIP.
- 러너 프리뷰: Explorer_Run 스프라이트를 화면 하단 라인에서 재생(선택).

### 11.6 색 토큰 (재확인)

`gold #e8b24a` / `goldL #f3d38a` / `goldD #c1892c` · `stone #3a4a47~#233230` · `border #56635f / #1a2624` · `moss #4c7d2b/#5a9433/#6fae3e` · `cyan #7fd6e0` · `red #c15c4a` · `text #eef2ec / #8fb7ac`. 순수 검정/흰색 금지, 화면당 강조색 1개(gold).

### 11.7 스크립트 연동 요약

- `GameManager.scoreText` → HUD ScoreValue(TMP). `gameoverUI`/`clearUI` → 11.3 패널 루트.
- `DashStatusUI`(확장) → DashGauge Fill.fillAmount / Empty 스왑을 `PlayerController.CanDash` 및 충전량으로 갱신.
- 진행도 → GoalTrigger까지 거리 기반으로 ProgressFill.fillAmount.
- 버튼: 각 버튼에 Button 컴포넌트 + Transition을 Sprite Swap(Normal/Pressed) 또는 Animation으로. 포커스 가시화 유지.
