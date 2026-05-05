# 5018 — Sprint Contract

## C1 컴파일
- errors=0 (모든 sprint)

## C2 NUnit
- `TextureCompressionConfigurerTests` 3 신규 (108 → 111)

## C3 Validator
- `ValidateTextureCompressionPolicy` 통과
- Generated_v2 PNG 의 textureCompression != None 검증

## C4 Reflection
- `LittleHero.Editor.TextureCompressionConfigurer` 정적 클래스
- `ApplyPolicy` 정적 메서드

## C5 SO
- 코드 영역만 — SO 변경 0
- 786 PNG .meta 의 platformSettings 갱신 (DefaultPlatform Compressed + iOS Override ASTC 6×6)

## C6 Play 시나리오

### S1 — Editor 시각 회귀 X (T01 후)
- Attic / Chapter 적 sprite 시각 동일
- Editor 모드 압축 X (DefaultPlatform Compressed)

### S2 — Mac 빌드 < 200 MB (T04 후) ⭐
- 894 MB → < 200 MB (예상 ~140 MB)
- sharedassets0.assets.resS < 100 MB

## 합격 기준
- 자동 (C1~C4) ✅ + S2 < 200 MB ✅ → 최소
- + S1 시각 회귀 X (사용자) → 완전

## 변경 이력
- 2026-05-06: 초기 작성
