# Uni-Run Design Guide

이 문서는 현재까지 정한 Uni-Run의 배경, 환경, 플랫폼, 트랩, 캐릭터 디자인 방향을 프로젝트 최상단에서 빠르게 확인하기 위한 작업 지침이다. 더 상세한 변경 이력과 시안 흐름은 `docs/superpowers/specs/2026-06-24-pixel-stage-art-direction.md`를 함께 참고한다.

## 1. 전체 디자인 방향

- 기본 그래픽은 도트 스프라이트 형식을 유지한다.
- 도트는 너무 크게 보이지 않게 하고, 현재 프로젝트의 `Toko_Run.png`, `Toko_Jump.png`와 비슷한 촘촘한 픽셀 밀도를 목표로 한다.
- 현재 메인 스테이지의 핵심 분위기는 `Mystical Forest Ruins`이다.
- 테라리아처럼 2~3개의 레이어를 이용해 원근감을 표현하되, 실제 플레이에 필요한 플랫폼과 트랩은 배경에 합쳐 넣지 않는다.
- 배경, 환경, 플랫폼, 캐릭터, 지형지물/트랩 레이어는 서로 분리해서 관리한다.
- 화면 상단은 스코어, 스테이지 정보, UI가 들어갈 수 있으므로 강한 실선, 검은 선, 진한 초록 선, 복잡한 장식이 방해되지 않게 둔다.
- 배경은 좌우 반복 스크롤이 가능해야 하며, 끊김이 보이지 않는 seamless 구조를 우선한다.
- 위로 점프하거나 아래로 떨어지는 상황에서도 이미지의 끝이 적나라하게 드러나지 않도록 상하 확장 레이어를 함께 고려한다.

## 2. StyleSeed UI 적용 규칙

UI를 새로 만들거나 수정할 때는 StyleSeed 규칙을 적용한다.

- 접근성을 우선한다. 포커스 상태, 충분한 대비, 의미 있는 컨트롤, 최소 44x44px 수준의 조작 영역을 유지한다.
- UI 텍스트와 표면에는 순수 검정 `#000000`을 사용하지 않는다.
- 한 화면에서 주 강조색은 하나만 사용하고, 나머지는 그레이스케일과 의미 색상으로 정리한다.
- Unity에서는 색상, 패널, 프리팹, TextMeshPro 스타일, 머티리얼, 스프라이트 설정을 디자인 토큰처럼 취급한다.
- UI 패널은 카드 기반으로 정리하되, 게임 플레이 화면의 배경 아트와 충돌하지 않게 한다.
- 데이터나 상태를 보여주는 UI는 empty, loading, error, success 상태를 고려한다.
- 모션은 짧고 일관되게 사용하며, 상호작용을 이해시키는 목적일 때만 넣는다.

## 3. 레이어 구조

### 3.1 Background Layer

대표 파일:

- `Assets/Sprites/Sky.png`
- `Assets/Sprites/BackgroundLayer_ForestRuins.png`
- `Assets/Sprites/SkyUpperLayer_ForestRuins.png`

역할:

- 충돌 판정이 없는 원거리 배경이다.
- 하늘, 안개, 먼 산, 원거리 유적 실루엣을 담당한다.
- 좌우 반복 스크롤 시 끊김이 없어야 한다.
- 산은 너무 각지지 않게 부드러운 실루엣으로 만든다.
- 상단 HUD 영역에는 강한 선이나 복잡한 장식을 배치하지 않는다.

### 3.2 Environment Layer

대표 파일:

- `Assets/Sprites/EnvironmentLayer_ForestRuins.png`
- `Assets/Sprites/EnvironmentLowerLayer_ForestRuins.png`
- `Assets/Sprites/AbyssLayer_ForestRuins.png`
- `Assets/Sprites/EnvironmentTrees/*`

역할:

- 플레이어와 충돌하지 않는 장식 레이어다.
- 덩굴, 나뭇잎, 큰 나무, 폐허 기둥, 부서진 아치, 룬 문양, 잔해 실루엣을 배치한다.
- `EnvironmentLayer_ForestRuins`는 명확한 Ruin 느낌이 나야 한다.
- 나무는 현재 배경에 어울리는 큰 숲 오브젝트 느낌으로 배치한다.
- Idle 흔들림은 바람에 천천히 흔들리는 정도로 느리게 유지한다.
- 상단 HUD 영역을 방해하는 초록색/검은색 실선은 삭제하거나 덩굴 장식처럼 자연스럽게 정리한다.

### 3.3 Platform Layer

대표 파일:

- `Assets/Sprites/Platform_Long.png`
- `Assets/Sprites/Platform.png`

역할:

- 실제 이동, 점프, 착지, 기믹, 업그레이드가 가능한 플레이 레이어다.
- 배경 이미지에 플랫폼을 박아 넣지 않는다.
- 이동 플랫폼이나 업그레이드 플랫폼을 만들 수 있도록 개별 스프라이트/오브젝트로 유지한다.
- 플랫폼 상단은 플레이어가 밟을 수 있는 면이 명확하게 읽혀야 한다.
- 현재 방향은 이끼 낀 석재 유적, 깨진 블록, 뿌리가 감긴 폐허 플랫폼이다.

### 3.4 Character Layer

대표 파일:

- `Assets/Sprites/Explorer/*`
- 기존 참고: `Assets/Sprites/Toko_Run.png`, `Assets/Sprites/Toko_Jump.png`

역할:

- 플레이어 캐릭터와 캐릭터 관련 이펙트가 위치한다.
- 캐릭터는 진행 방향을 바라보는 side-facing 스프라이트로 제작한다.
- 좌우 반전은 가능하면 `SpriteRenderer.flipX`로 처리하고, Transform scale 반전은 피한다.
- 캐릭터의 발 위치, 이펙트 위치, 피격 판정이 시각적으로 맞아야 한다.

### 3.5 Terrain / Trap Layer

대표 파일:

- `Assets/Sprites/Obstacle.png`
- `Assets/Sprites/Trap_RootSpikes.png`

역할:

- 가시, 루트 스파이크, 위험 지형, 오브젝트 판정을 담당한다.
- 가시의 시각적 위험 영역과 실제 사망 판정이 일치해야 한다.
- 가시가 땅에 박힌 표현이 필요하면 스프라이트 하단을 플랫폼보다 뒤에 보내 숨기는 방식으로 처리한다.
- 판정은 보이는 가시 끝과 위험 면에 맞추고, 플랫폼 아래에 묻힌 부분은 판정에서 제외한다.

## 4. 현재 스테이지: Mystical Forest Ruins

핵심 키워드:

- 숲속 폐허
- 고대 유적
- 이끼 낀 석재
- 덩굴과 큰 나무
- 청록 안개
- 작게 빛나는 룬/금빛 포인트

권장 팔레트:

- 짙은 숲 녹색
- 청록 계열 안개
- 탁한 석재 회색
- 이끼 색
- 제한적인 cyan/gold 포인트

주의 사항:

- 전체 화면이 단일 녹색 톤으로만 보이지 않게 석재 회색, 어두운 갈색, 청록 안개를 섞는다.
- 산과 원거리 지형은 너무 각지지 않게 만든다.
- 상단 HUD 영역을 복잡한 선으로 채우지 않는다.
- 환경 나무는 플랫폼과 겹칠 수 있지만, 플레이 판정을 가리는 위치는 피한다.
- 배경과 환경 레이어의 하단은 플레이어가 아래로 떨어질 때도 자연스럽게 이어져야 한다.

## 5. 보류 중인 차기 스테이지: Sky-Island Ruins

이 콘셉트는 현재 메인 적용 대상이 아니라, 이후 스테이지 후보로 정보를 유지한다.

핵심 키워드:

- 하늘섬 폐허
- 떠 있는 섬
- 밝은 구름
- 깨진 아이보리/회색 석판
- 청량한 cyan 포인트
- 수직 이동이 많은 스테이지

현재 Forest Ruins와 섞지 말고, 별도 스테이지로 확장할 때 사용한다.

## 6. 캐릭터 디자인 지침

캐릭터는 Toko와 비슷한 사양과 밀도를 기준으로 제작한다.

### 6.1 외형 방향

- 성별이 눈에 띄게 고정되지 않는 탐험가 외형을 유지한다.
- 우주 탐사자처럼 보이지 않게 한다.
- 카메라를 정면으로 바라보지 않고, 진행 방향을 바라본다.
- 인디아나 존스의 분위기처럼 고전 탐험가 느낌을 참고하되, 직접 복제하지 않는다.
- 챙 있는 천/가죽 모자, 카키 셔츠, 올리브색 목 천, 가죽 가방, 밧줄 장식, 어두운 부츠를 사용한다.
- 색감은 현재 숲 폐허 배경과 어울리는 흙색, 이끼색, 어두운 가죽색을 중심으로 한다.

### 6.2 스프라이트 사양

- 프레임은 현재 Explorer 기준 64x64를 유지한다.
- PPU는 32 기준으로 관리한다.
- 필터링은 도트가 흐려지지 않게 Point 계열을 사용한다.
- 실제 캐릭터가 차지하는 영역은 대략 폭 33~35px, 높이 49px 수준을 기준으로 한다.
- Toko 참고 캐릭터는 대략 폭 24~29px, 높이 43~46px 수준이므로, Explorer는 너무 크고 둔탁해 보이지 않게 스케일과 실루엣을 조정한다.
- 픽셀 블록이 너무 커 보이지 않도록 내부 명암, 실루엣, 장비 디테일을 촘촘하게 구성한다.

### 6.3 애니메이션 방향

필요 모션:

- Idle
- Run / Walk
- Dash
- Die

방향:

- Idle은 호흡과 작은 장비 흔들림이 느껴지는 정도로 만든다.
- Run/Walk는 측면 진행감이 명확해야 하며, 발 접지 위치가 떠 보이지 않아야 한다.
- Dash는 앞으로 기울어진 자세와 짧은 천/밧줄 움직임으로 표현한다.
- Die는 현재 스프라이트와 어울리게 길이를 조금 더 확보하고, 즉시 사라지는 느낌보다 stumble/fall 흐름을 준다.
- 마지막으로 적용한 방향은 Die 8프레임, 약 5fps, non-loop, 약 1.6초 길이다.

### 6.4 씬 적용 기준

- 플레이어 스케일은 기존 Toko와 비슷하게 보이도록 조정한다.
- 마지막 기준값은 Player scene scale `0.58`이다.
- 마지막 기준 Collider2D 값은 offset `(0, -0.04)`, size `(0.76, 1.34)`이다.
- 캐릭터가 붕 떠 보이면 pivot, collider, 발 위치, 이펙트 기준점을 함께 확인한다.
- 판정이 시각보다 먼저 닿거나 늦게 닿지 않도록 콜라이더를 스프라이트 기준으로 재검토한다.

## 7. 대시 충전 아이템

대표 파일:

- `Assets/Sprites/DashRecharge_RuneSeed.png`

방향:

- 캐릭터와 Forest Ruins 분위기에 맞는 teal 계열 rune seed 형태를 사용한다.
- 지나치게 현대적이거나 캐릭터와 동떨어진 아이콘처럼 보이지 않게 한다.
- 마지막 기준은 white tint, collider radius `0.78`이다.

## 8. 작업 및 검증 지침

- 기존 파일을 수정할 때는 먼저 백업한다.
- Unity 에셋 백업은 `.meta` 충돌을 만들지 않도록 `Assets` 안에 단순 복제하지 않는다.
- 가능하면 기존 파일명과 GUID를 유지해서 씬 참조가 끊기지 않게 한다.
- 스프라이트 수정 후에는 import 설정, PPU, pivot, filter mode, sprite rect를 확인한다.
- 캐릭터 교체 후에는 씬의 scale, collider, animation clip, runtime controller 연결을 확인한다.
- 가시나 트랩을 수정한 뒤에는 보이는 위험 영역과 Collider2D 영역이 일치하는지 확인한다.
- 배경을 수정한 뒤에는 좌우 반복, 위쪽 확장, 아래쪽 추락 시 노출 영역을 확인한다.
- 문서만 수정한 경우 Unity 컴파일은 필수 검증 대상이 아니다.

## 9. 주요 파일 목록

- `Assets/Scene/Main.unity`
- `Assets/Sprites/Sky.png`
- `Assets/Sprites/BackgroundLayer_ForestRuins.png`
- `Assets/Sprites/SkyUpperLayer_ForestRuins.png`
- `Assets/Sprites/EnvironmentLayer_ForestRuins.png`
- `Assets/Sprites/EnvironmentLowerLayer_ForestRuins.png`
- `Assets/Sprites/AbyssLayer_ForestRuins.png`
- `Assets/Sprites/Platform_Long.png`
- `Assets/Sprites/Platform.png`
- `Assets/Sprites/Obstacle.png`
- `Assets/Sprites/Trap_RootSpikes.png`
- `Assets/Sprites/EnvironmentTrees/*`
- `Assets/Sprites/Explorer/*`
- `Assets/Sprites/DashRecharge_RuneSeed.png`
- `docs/superpowers/specs/2026-06-24-pixel-stage-art-direction.md`
