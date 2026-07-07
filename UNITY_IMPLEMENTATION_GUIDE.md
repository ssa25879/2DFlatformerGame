# RELIC DASH — Unity 적용 & 관리 가이드

> 이 문서는 `DESIGN_GUIDE_UI_Appendix.md`(Section 10, 디자인 규칙)와
> `RELIC_DASH_Unity_Integration.md`(Section 11, 수치 스펙)를 실제로 **Unity 프로젝트에 넣고
> 이후에도 팀이 계속 관리**할 수 있도록 순서·체크리스트 중심으로 정리한 실행 가이드입니다.
> 값(좌표, 색상, Border 등)은 이 문서에서 반복하지 않고 Section 10/11을 참조합니다.

---

## 0. 준비물

- `handoff/` 다운로드 패키지: `Assets/UI_Sprites_1A/*`(스프라이트 47종), `Assets/GameplayLayers/*`(배경 9종), `Fonts/FONTS_README.txt`(폰트 출처).
- 시안 문서 8종(`Uni-Run *.dc.html`) — 브라우저로 열어 레이아웃/좌표/색을 직접 확인하는 용도. `Uni-Run Index.dc.html`에서 전체를 한 번에 탐색 가능.

---

## 1. 프로젝트 폴더 구조 만들기

Unity 프로젝트에 아래 구조를 먼저 만든 뒤 `handoff/Assets/UI_Sprites_1A`의 내용을 그대로 옮겨 담습니다(폴더명 그대로 매칭).

```
Assets/
├─ Sprites/UI/
│  ├─ HUD/        ← UI_Sprites_1A/HUD
│  ├─ Buttons/    ← UI_Sprites_1A/Buttons
│  ├─ Controls/   ← UI_Sprites_1A/Controls   (Slider/Toggle/Segment/Stepper)
│  ├─ Menu/       ← UI_Sprites_1A/Menu
│  ├─ Result/     ← UI_Sprites_1A/Result
│  ├─ Loading/    ← UI_Sprites_1A/Loading
│  └─ Brand/      ← UI_Sprites_1A/Brand
├─ Sprites/Gameplay/  ← GameplayLayers/* (기존 배경·캐릭터 에셋과 통합)
├─ Fonts/
│  └─ TMP/        ← 아래 3장의 TMP Font Asset 저장 위치
└─ Prefabs/UI/
   ├─ HUD/  Menu/  Result/  Loading/  Options/  States/
```

## 2. 스프라이트 임포트 일괄 설정

`Assets/Sprites/UI/` 전체를 선택 → Inspector에서 한 번에 적용:

- Texture Type: **Sprite (2D and UI)**
- Filter Mode: **Point (no filter)**
- Compression: **None**
- Mesh Type: **Full Rect**
- PPU: 게임 스프라이트 기준과 통일

9-slice Border 값과 Filled 여부는 **RELIC_DASH_Unity_Integration.md §11.1 표**를 그대로 따라 Sprite Editor에서 지정합니다. (프레임류 6~14px, 버튼류 12px대 — 표에 전부 정리되어 있음.)

## 3. 폰트 적용 (TextMeshPro)

`Fonts/FONTS_README.txt`의 배포처에서 TTF를 내려받아:

1. **Pixelify Sans** → TMP Font Asset 생성 (라틴 전용, 제목/수치용).
2. **Silkscreen** → TMP Font Asset 생성 (라틴 전용, 라벨/캡션용).
3. **Galmuri11 / Galmuri9** → TMP Font Asset 생성, 한글 문자 서브셋 지정(사용 문구만 넣으면 용량 절감).
4. 각 Pixelify/Silkscreen Font Asset의 **Fallback Font Assets**에 대응하는 Galmuri를 등록
   (Pixelify Sans → Galmuri11, Silkscreen → Galmuri9).
   → 한글이 섞인 TMP 텍스트에서 라틴은 원 폰트, 한글은 자동으로 Galmuri로 렌더링됩니다.

## 4. 화면별 구축 순서 (권장)

시안 문서 → Unity Prefab 매핑. 각 항목을 완료 순서대로 체크하며 진행하세요.

| 순서 | 시안 문서 | Unity Prefab | 참조 스펙 |
|---|---|---|---|
| 1 | Uni-Run HUD.dc.html (1A/1B 중 택1) | `Prefabs/UI/HUD/HUDCanvas` | §11.2 |
| 2 | Uni-Run Menus.dc.html (Title, Pause) | `Prefabs/UI/Menu/TitleUI`, `PauseUI` | §11.4 |
| 3 | Uni-Run Screens.dc.html (Game Over, Clear) | `Prefabs/UI/Result/GameOverUI`, `ClearUI` | §11.3 |
| 4 | Uni-Run Loading.dc.html (Splash, Stage Loading) | `Prefabs/UI/Loading/SplashUI`, `StageLoadingUI` | §11.5 |
| 5 | Uni-Run Options.dc.html | `Prefabs/UI/Options/OptionsUI` | §11.8 Controls 표 |
| 6 | Uni-Run States.dc.html (Tutorial/Confirm/NetworkError) | `Prefabs/UI/States/TutorialUI`, `ConfirmDialogUI`, `NetworkErrorUI` | §11.9 |

각 Prefab을 만들 때: 시안 문서를 브라우저로 열어 **좌표/크기는 §11.x 표**를, **레이아웃 뼈대는 시안 화면**을 보며 그대로 옮기면 됩니다. 시안의 CSS 그라디언트 패널(Options/Pause/States 등 커스텀 패널)은 실제 9-slice 스프라이트가 아직 없는 부분이므로, 기존 `Panel_Menu_Frame`/`Panel_Result_Frame` 재사용 또는 필요 시 신규 패널 스프라이트 제작 여부를 디자인팀과 확인하세요.

## 5. 스크립트 연동 체크리스트

`RELIC_DASH_Unity_Integration.md §11.7`을 기준으로 아래를 순서대로 연결합니다.

- [ ] `GameManager.scoreText` → HUD `ScoreValue`(TMP)
- [ ] `GameManager.gameoverUI` / `clearUI` → §11.3 Result 패널 루트
- [ ] `DashStatusUI` → `DashGauge` Fill.fillAmount / `PlayerController.CanDash` 기반 Empty 스왑
- [ ] GoalTrigger 거리 → `ProgressFill.fillAmount`
- [ ] 버튼 Transition = Sprite Swap, **Normal/Pressed/Disabled 3상태만** 매핑(모바일 터치 기준, §11.8)
- [ ] 신규: `TutorialUI` 최초 실행 플래그(PlayerPrefs 등)로 1회만 노출
- [ ] 신규: `ConfirmDialogUI`를 범용 컴포넌트로 만들어 Pause의 QUIT TO TITLE, 설정 초기화 등에서 콜백만 바꿔 재사용
- [ ] 신규: `NetworkErrorUI`를 연결 상태 감지 로직에 연결, RETRY 시 마지막 요청 재시도

## 6. 이후 화면/상태를 추가할 때 (관리 워크플로)

새 화면이나 상태가 필요해지면 아래 순서를 지켜 일관성을 유지합니다.

1. **시안 문서에 먼저 추가** — 기존 `Uni-Run *.dc.html` 중 성격이 맞는 파일에 새 섹션(`id="9a"`처럼 다음 turn 번호)으로 추가하거나, 완전히 새로운 화면 카테고리면 새 `.dc.html` 파일을 만들고 `Uni-Run Index.dc.html`의 `docs` 목록에 카드를 추가합니다.
2. **색·폰트·간격은 기존 토큰만 사용** — `DESIGN_GUIDE_UI_Appendix.md §10.5` 색 토큰, Pixelify Sans/Silkscreen(+Galmuri) 폰트 페어링을 그대로 씁니다. 새 강조색을 추가하지 않습니다(화면당 강조색은 gold 1개 원칙 유지, 위험 상태만 danger.red).
3. **신규 스프라이트가 필요하면** `RELIC_DASH_Unity_Integration.md §11.1` 표에 원본 크기·Border·Image Type을 추가하고, `hud_sprites/1A/<카테고리>/`에 파일을 넣은 뒤 `Uni-Run UI Sprites.dc.html`(또는 HUD Sprites 문서)에 썸네일 항목을 추가합니다.
4. **Unity 반영** — 위 §1~§3 임포트 규칙 그대로 적용 → §4 순서로 Prefab 제작 → §5 체크리스트에 새 항목 추가.
5. **모바일 터치 원칙 유지** — 새 인터랙션 요소도 Hover/Focus 상태는 만들지 않고 Normal/Pressed/Disabled만 사용, 조작 영역 ≥ 44px.
6. **핸드오프 갱신** — 새 스프라이트/폰트가 생기면 `handoff/` 패키지를 다시 내보내 팀에 재배포합니다.

## 7. 최종 QA 체크리스트

- [ ] 모든 텍스트가 TMP이며 스프라이트에 직접 굽힌 값 텍스트가 없는지
- [ ] 9-slice 스프라이트가 해상도 변경 시 모서리 깨짐 없이 늘어나는지
- [ ] 순수 검정 `#000000`/순수 흰색 `#ffffff` 미사용 확인
- [ ] 화면당 강조색 1개(gold) 원칙 준수, 위험 상태만 danger.red
- [ ] 모든 조작 요소 히트 영역 ≥ 44×44px
- [ ] 한글 포함 TMP 텍스트에서 Galmuri 폴백이 정상 렌더링되는지(글자 깨짐/네모 없음)
- [ ] HUD가 화면 최상단에만 위치, 플레이 판정 영역과 겹치지 않는지
