# T01+T03 (통합): Seeder 1회 실행 → 6 Variant SO + CH3/4/5 wiring

> **레이어**: Data / **의존**: T02 / **예상**: 15분 / 1 commit
> **PRD 출처**: v2 §3.1 §3.2

## 1. 작업 항목

### 1.1 Tools/LittleHero/Seed Chapter Enemy Variants 메뉴 실행

ChapterVariantSeeder.SeedAndWire() 호출 — Unity Editor 메뉴 또는 reflection 호출.

**결과**:
- `Assets/_Project/Data/Enemies/Variants/` 폴더 생성
- 6 .asset 파일 신규 (PRD v2 §3.1)
- CH3/4/5 ChapterData.miniBossPool[] 갱신 (chapterNumber 스캔 후 추가)
- AssetDatabase.SaveAssets 자동

### 1.2 .meta 파일 + Chapter SO 변경 commit

`git add` — Variants/ 폴더 + Chapter_03/04/05.asset.

## 2. 검증 기준

- 6 Variant SO 모두 존재 + behaviorType 정확
- CH3/4/5 의 miniBossPool 에 변형 SO 포함 (Editor 또는 reflection 검증)
- ProjectValidator warning — `Variant_Stationary_Sentry` + `Variant_Ranged_Archer` 가 projectilePrefab=null → 의도된 warning (5037.1 cascade 에서 보강)

## 3. commit 메시지

```
5037 T01+T03 — 6 Variant SO 일괄 생성 + CH3/4/5 시범 wiring (2/5)

[변경]
  - Data/Enemies/Variants/ 폴더 신규
  - 6 Variant.asset (Stationary/Charger/Splitter+Child/Ranged/Bomber)
  - Chapter_03/04/05.asset miniBossPool[] 갱신

[검증]
  - 도구 실행 errors=0
  - 6 SO 모두 behaviorType 정확
  - 시범 챕터 3개 wiring 완료
  - Validator warning 2 (projectilePrefab=null — 5037.1 cascade 보강)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```
