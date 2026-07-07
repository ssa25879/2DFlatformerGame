RELIC DASH · Stone Tablet (1A) — 사용 폰트 안내
====================================================

이 폴더는 실제 폰트 바이너리 대신, 각 폰트의 정식 CDN/배포처를 정리한 안내입니다.
(자동화 도구가 외부 폰트 파일을 직접 내려받을 수 없어, 정식 출처 링크로 대체합니다.
아래 링크에서 TTF/WOFF2를 직접 내려받아 Unity TMP Font Asset으로 변환해 사용하세요.)

1) Pixelify Sans (제목/수치 — 라틴 전용)
   - Google Fonts: https://fonts.google.com/specimen/Pixelify+Sans
   - 라이선스: SIL Open Font License 1.1
   - 용도: 화면 타이틀, 스코어/수치 등 굵은 픽셀 표기

2) Silkscreen (라벨/캡션 — 라틴 전용)
   - Google Fonts: https://fonts.google.com/specimen/Silkscreen
   - 라이선스: SIL Open Font License 1.1
   - 용도: 좁은 라벨, 캡션, 뱃지 텍스트

3) Galmuri11 / Galmuri9 (한글 — 픽셀/비트맵, 위 두 폰트와 페어링)
   - 배포처: https://galmuri.quiple.dev  (GitHub: https://github.com/quiple/galmuri-site)
   - 웹폰트 CDN(참고용): https://cdn.jsdelivr.net/npm/galmuri@latest/dist/galmuri.css
   - 라이선스: SIL Open Font License 1.1 (저작자 이민서)
   - 용도: 한글 UI 텍스트 전체. Pixelify Sans/Silkscreen에는 한글 글리프가 없어
     본 시안에서는 "Pixelify Sans","Galmuri11" / "Silkscreen","Galmuri9" 순서로
     폰트 스택을 지정해 라틴은 원 폰트, 한글은 Galmuri로 자동 대체되게 했습니다.
   - Galmuri11은 제목/수치용(Pixelify Sans 대응), Galmuri9은 라벨/캡션용(Silkscreen 대응).

Unity 적용 메모
- 각 TTF를 TextMeshPro Font Asset Creator로 변환(한글은 필요한 문자만 서브셋 권장).
- 두 언어가 섞인 문구는 TMP Fallback Font Assets에 Galmuri를 등록해
  라틴은 기본 폰트, 한글은 자동으로 Galmuri로 대체되게 구성 가능.
