# 5018 — iOS Texture Compression — Sprint PRD

> **출처**: `docs/prd/5018_texture_compression_prd.md` (디자인) — 옵션 A (Editor 도구 일괄)
> **목표**: Mac 894 MB → ~140 MB (-84%, TestFlight 진입)

## 1. Sprint 개요

| Sprint | 작업 | commit | 시간 |
|---|---|---|---:|
| 1 | TextureCompressionConfigurer Editor 도구 + 786 PNG 일괄 적용 | 1 | 40분 |
| 2 | EnemyAtlas SpriteAtlas iOS Override 적용 | 1 | 25분 |
| 3 | ProjectValidator 5018 룰 + NUnit | 1 | 30분 |
| 4 | Mac 빌드 + 용량 측정 | 1 | 30분 |
| 5 | 결과 분석 + docs (changelog + CLAUDE.md §11/§14) | 1 | 25분 |
| **총** | **5 commit** | **5** | **~2.5h** |

## 2. Sprint 명세

### Sprint 1 — TextureCompressionConfigurer

**파일**: `Scripts/Editor/TextureCompressionConfigurer.cs` (~120L 신규)

**핵심 메서드**:
- `[MenuItem("Tools/LittleHero/Apply Texture Compression Policy (5018)")]` — DefaultPlatform + iOS Override + 786 PNG 일괄
- `AssetDatabase.StartAssetEditing/StopAssetEditing` batch — import 시간 단축
- 결과 콘솔: `[5018] Texture compression applied to N textures.`

**적용 정책** (PNG 별):
- DefaultPlatform: textureCompression=Compressed + crunchedCompression=true + quality 50
- iOS Override (`overridden=true`): format=ASTC_6x6 + Compressed + Crunched + quality 50
- 영역: `Assets/_Project/Art/Generated_v2/**`

**검증 기준**:
- 컴파일 errors=0
- 786 PNG 모두 처리 완료
- `Enemy_Animal_Boss.png.meta` 의 platformSettings 갱신 확인

### Sprint 2 — EnemyAtlas iOS Override

**파일**: `Scripts/Editor/EnemyAtlasConfigurer.cs` 수정 (5009.4 기존)

- `[MenuItem("Tools/LittleHero/Configure EnemyAtlas iOS Compression (5018)")]` 신규
- iOS Override: format=ASTC_6x6 + Crunched
- DefaultPlatform 8192 Uncompressed 그대로 (Editor 화질)

### Sprint 3 — Validator 5018 룰 + NUnit

**파일**: `Scripts/Editor/ProjectValidator.cs` 수정

`ValidateTextureCompressionPolicy`:
- Generated_v2 PNG 중 textureCompression == None 검출 → 경고
- iOS Override 없음 검출 → 경고

**NUnit**: `TextureCompressionConfigurerTests` (3 NUnit, Reflection 검증):
- `TextureCompressionConfigurer` 정적 클래스 존재
- `ApplyPolicy` 정적 메서드 존재
- (정합 검증 — 786 PNG 일괄 적용 확인은 수동)

### Sprint 4 — Mac 빌드 + 용량 측정

`manage_build` 자동화:
- Release 빌드 (development=false) — App Store 형식
- Builds/Mac/LittleHero.app 용량 측정
- sharedassets0.assets.resS 용량 측정

**검증 기준**:
- Mac 빌드 < 200 MB (1차 임계, iOS 추정 < 100 MB)
- sharedassets0 < 100 MB

### Sprint 5 — docs

- changelog_v1.md 5018 항목
- CLAUDE.md §7 성능 목표 (App 용량 150MB 임계 회복)
- CLAUDE.md §11 마일스톤 + §14 직전 5건 회전

## 3. 의존성 그래프

```
Sprint 1 (Editor 도구) ──┬→ Sprint 4 (빌드 측정) ──→ Sprint 5 (docs)
Sprint 2 (Atlas) ────────┤
Sprint 3 (Validator) ────┘
```

## 4. Definition of Done

- [ ] 5 commit + 컴파일 errors=0
- [ ] 786 PNG iOS Override 적용 (Validator 검증)
- [ ] EnemyAtlas iOS Override 적용
- [ ] NUnit 3 신규 (108 → 111)
- [ ] **Mac 빌드 < 200 MB 도달** (1차 검증, iOS 추정 < 150 MB)

## 5. 검증 시나리오

### S1 — Editor 도구 (Sprint 1 후)
- Tools/LittleHero/Apply Texture Compression Policy 실행
- 786 PNG meta 갱신 확인
- Editor 시각 회귀 X

### S2 — 빌드 측정 (Sprint 4 후) ⭐
- Mac 빌드 < 200 MB
- sharedassets0 < 100 MB

## 6. 변경 이력
- 2026-05-06: 초기 작성
