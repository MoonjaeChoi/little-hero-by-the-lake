# Sprint Contract: 5027

## C1 컴파일

errors=0, 신규 8 파일 + facade 2 갱신

## C2 Editor Test

NUnit 158 회귀 0 (신규 0 — facade 위임)

## C3 Validator

OOPPolicyRule warnings 18 → 16 (-2 GachaService + AudioManager)

## C4 Reflection

- `LittleHero.GachaService` (facade, 정적) — 13 메서드 시그니처 보존
- `LittleHero.GachaResult` (DTO) — 4 필드
- `LittleHero.GachaCanRollService` / GachaProgressService / GachaRollService / GachaTicketService 정적
- `LittleHero.AudioManager` (Singleton) — 4 메서드 시그니처 보존
- `LittleHero.AudioSfxController` / AudioBgmController / AudioChapterBgmMap

## C5 SO 검증

영향 0 (SO 변경 X)

## C6 Play 시나리오

### 시나리오 1 — 가챠 회귀
Attic → 가챠 → 단판/10연차 → 결과 정상

### 시나리오 2 — 오디오 회귀
챕터 진입 BGM + SFX 재생 → 챕터 클리어 BGM 페이드

### 시나리오 3 — Validator 회귀
Validate Project → warnings 75 → 73 (-2)

## 합격 기준

최소: C1+C3+C4 + 시나리오 1+2
완전: 모든 카테고리 + 시나리오 3
