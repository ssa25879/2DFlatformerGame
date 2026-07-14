# 2D Platformer Game

Unity 2D 러너 예제를 기반으로 만든 마우스 대시 중심 2D 플랫포머 프로토타입입니다.

## 프로젝트 개요

- 좌우 이동과 마우스 방향 대시를 중심으로 진행하는 2D 플랫포머
- 점프 대신 좌클릭/우클릭 대시를 사용
- 착지 및 대시 충전 아이템으로 대시 재충전
- Trap, DeadZone, Goal, Stage Clear 흐름 구현
- Intro, Stage Select, FR_Stage01~FR_Stage04 씬 구성
- 스테이지 클리어 시 다음 스테이지 잠금 해제

## 개발 환경

- Unity: `6000.3.10f1`
- 주요 패키지:
  - Unity 2D Sprite
  - Unity 2D Tilemap
  - Unity UI
  - Unity Test Framework
  - TextMeshPro

## 실행 방법

1. Unity Hub에서 이 프로젝트 폴더를 연다.
2. Unity `6000.3.10f1` 또는 호환 버전으로 프로젝트를 연다.
3. Build Settings의 첫 씬은 `Assets/Scene/Intro.unity`이다.
4. Play Mode를 실행하면 `Intro -> Stage Select -> Stage` 흐름으로 시작된다.

## 조작

- 이동: 좌/우 방향키 또는 기존 프로젝트 입력 설정의 수평 이동
- 대시: 좌클릭 또는 우클릭
- 재시작: 게임오버/클리어 후 좌클릭, 우클릭, Space

## 주요 폴더

- `Assets/Scripts`: 런타임 게임 로직
- `Assets/Editor`: 씬 생성/검증용 에디터 유틸리티
- `Assets/Scene`: Intro, Stage Select, 스테이지 씬
- `Assets/Tests/EditMode`: EditMode 테스트
- `ProjectSettings`: Unity 프로젝트 설정

## Git 관리

업로드 대상은 Unity 프로젝트 원본 파일 중심입니다. `Library`, `Temp`, `Logs`, `UserSettings`, 백업 폴더, 로컬 도구 설정은 `.gitignore`로 제외합니다.
