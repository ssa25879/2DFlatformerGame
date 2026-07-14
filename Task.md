# Uni-Run 작업 현황 (2026-07-09 기준)

세션 종료 전 남기는 인수인계 문서. 다음 세션에서 이 문서를 기준으로 이어서 진행한다.

## 이번 세션(2026-07-09, 4차 — 미완료 상태로 종료)에서 진행한 것

### Intro/Stage Select 메뉴 흐름 (Codex 메인 코딩 + Claude Code 검증) — **검증 미완료**

사용자 결정사항(재확인 불필요):
- 게임 진입점을 Intro 씬으로 변경(Build Settings 0번). 흐름: Intro → Stage Select → FR_StageNN.
- Stage Select는 순차 잠금 해제(클리어해야 다음 스테이지 활성화), PlayerPrefs로 세션 간 유지.
- "COMING SOON"은 FR_Stage04의 기존 Clear UI에 추가 후 `nextSceneName="Intro"`로 자동 전환(기존 스테이지 전환 패턴 재사용).

**구현된 것**
- `Assets/Scripts/StageProgress.cs`(신규): PlayerPrefs 기반 순차 잠금 해제. `GetUnlockedStageCount()`(기본 1), `UnlockStage(n)`(값을 올리기만 함), `IsStageUnlocked(n)`.
- `Assets/Scripts/IntroController.cs`(신규): 클릭/Space 입력 시 Stage Select 씬 로드.
- `Assets/Scripts/StageSelectButton.cs`(신규): 잠금 여부에 따라 버튼 활성화/`LOCKED` 텍스트 전환.
- `Assets/Scripts/GameManager.cs`: `OnStageCleared()`에서 활성 씬 이름의 끝자리 숫자를 파싱(`TryGetStageNumberForTest`)해 다음 스테이지 잠금 해제(`TryGetStageToUnlockForTest`). `isCleared` 가드 추가로 중복 실행 방지.
- `Assets/Editor/StageSceneConfig.cs`: `IntroName/Path`, `StageSelectName/Path` 상수 추가.
- `Assets/Editor/CodexDashPlatformerSceneSetup.cs`: `CreateOrUpdateMenuScenes()`(Intro/StageSelect 씬을 fresh 생성), `RegisterMenuAndStageScenesInBuildSettings()`(Intro→StageSelect→FR_Stage01~04 순서 등록), `ConfigureComingSoonClearUi()`(Stage04 Clear UI에 "COMING SOON" 라벨 추가).
- 테스트: `StageProgressEditModeTests.cs`, `IntroControllerEditModeTests.cs`(신규), `GameManagerEditModeTests.cs`에 3건 추가.

**버그 발견 및 수정 이력(이번 세션 중)**
1. `ConfigureComingSoonClearUi`가 `FindSceneObject("Clear UI")`(공백 포함)로 찾았는데 실제 오브젝트 이름은 `ClearUI`(공백 없음)라 조용히 no-op됨 → Codex가 `GameManager.clearUI` 필드 참조 방식으로 수정, 재실행 후 `Coming Soon Label` 정상 생성 확인(`grep`으로 FR_Stage04.unity에만 1건 존재 확인).

**Unity Editor에서 실행 완료된 것**
1. `CodexDashPlatformerSceneSetup.CreateOrUpdateStageProgressionScenes()` — Stage02~04 재생성(Stage04 nextSceneName=Intro, Coming Soon 라벨 포함) 확인.
2. `CodexDashPlatformerSceneSetup.CreateOrUpdateMenuScenes()` — `Assets/Scene/Intro.unity`, `Assets/Scene/StageSelect.unity` 생성 확인(파일 존재).
3. `CodexDashPlatformerSceneSetup.RegisterMenuAndStageScenesInBuildSettings()` — Build Settings 순서 확인(Intro→StageSelect→FR_Stage01~04).

### ⚠️ 미완료 — 다음 세션 최우선 작업
1. **Unity Editor 연결이 세션 종료 시점에 끊김**(list_unity_project_roots가 0건 반환, 크래시 또는 종료 추정). 이로 인해 다음이 **미검증 상태**:
   - EditMode 테스트 전체 재실행(마지막 확인된 결과는 62/62이지만 **이번 메뉴 흐름 작업 이전** 시점 것. 새 테스트 8건 포함 재실행 필요)
   - 컴파일 오류 여부 최종 재확인
   - Intro/Stage Select 씬 스크린샷으로 레이아웃 확인(패널 크기, 버튼 겹침 여부)
   - Stage04 Clear UI에서 "CLEAR"와 "COMING SOON" 텍스트가 겹치지 않는지 시각 확인
   - 실제 Play Mode로 Intro → Stage Select → Stage 클리어 → 잠금 해제 → Intro 복귀까지 전체 흐름 확인
2. **Unity Editor가 응답하는지 먼저 확인**(`get_unity_editor_state`) 후 위 검증 진행.
3. 검증 통과 후 커밋(이번 메뉴 흐름 변경은 아직 전혀 커밋되지 않음).

## 이전 세션에서 완료 및 커밋/푸시된 것 (참고, 재작업 불필요)
- 코드 리뷰, 이동 버튼 레이아웃, Stage 1-2~1-4 진행 기능, 씬 이름 축약(FR_StageNN), 하드코딩 정리(StageSceneConfig), 백업 정리 — 모두 커밋 후 `origin/main` 푸시 완료(커밋 `c00b69e`~`bc2231f`).
- 스테이지 난이도(플랫폼 간격/트랩 배치)는 구조적으로만 검증됨. 실제 플레이테스트는 여전히 미수행.

## 현재 Git 상태 (미커밋, 메뉴 흐름 작업분)

```
 M Assets/Editor/CodexDashPlatformerSceneSetup.cs
 M Assets/Editor/StageSceneConfig.cs
 M Assets/Scene/FR_Stage02~04.unity(.meta)   (Coming Soon 등 재생성분)
 M Assets/Scripts/GameManager.cs
 M Assets/Tests/EditMode/GameManagerEditModeTests.cs
 M ProjectSettings/EditorBuildSettings.asset
?? Assets/Scene/Intro.unity(.meta)
?? Assets/Scene/StageSelect.unity(.meta)
?? Assets/Scripts/IntroController.cs(.meta)
?? Assets/Scripts/StageProgress.cs(.meta)
?? Assets/Scripts/StageSelectButton.cs(.meta)
?? Assets/Tests/EditMode/IntroControllerEditModeTests.cs(.meta)
?? Assets/Tests/EditMode/StageProgressEditModeTests.cs(.meta)
```

## 참고: 작업 로그

세부 작업 이력: `D:\Codex\Log\20260707_Uni-Run_Log.md`(2026-07-07), `D:\Codex\Log\20260709_Uni-Run_Log.md`(2026-07-09).
