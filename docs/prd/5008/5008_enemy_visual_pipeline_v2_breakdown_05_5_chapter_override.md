# T05.5 — (조건부 D2=C) 손상 8 chapter override 흡수

> **레이어**: 자산 (Generated_v2/Enemies/Chapters)
> **의존성**: T02 결과 V2 archetype 폴백 검증 = **깨짐**
> **예상 시간**: 0~30분 / 0~1 commit (조건부)
> **PRD 출처**: `5008_enemy_visual_pipeline_v2_v2.md` §2 Sprint 5.5

## 1. 활성화 조건

**활성화**: T02 EnemySpriteAuditor 의 `ValidateV2ArchetypeFallback()` 결과 = **fallback 깨짐**
**비활성화**: fallback OK → 본 태스크 skip, 5008.1 anchor 만 (5008 종료)

> **사용자 결정 input**: T02 commit 후 enemy_sprite_audit_report.md 의 "V2 archetype fallback" 섹션 확인.
> - **OK**: T05.5 미실행
> - **깨짐**: T05.5 실행 (사용자 명시 결정 후)

## 2. 작업 항목 (활성화 시)

### 2.1 자산 재생성

**대상**: `LittleHero/Assets/_Project/Art/Generated_v2/Enemies/Chapters/`
- `CH14_*.png` (Nature 카테고리, 시드 33333)
- `CH22_*.png` (Civ, 55555)
- `CH25_*.png` (Civ, 55555)
- `CH30_*.png` (Fantasy, 66666)
- `CH33_*.png` × 2 (Fantasy, 66666)
- `CH34_*.png` (Fantasy, 66666)

> 손상 PNG 의 정확한 archetype suffix (Small/Medium/Large/MiniA/MiniB/Boss) 는 T02 audit report 에서 확인.
> 사용자 보고 list 와 audit 결과 매칭 후 8 PNG 정확히 식별.

**프로세스**: T03/T04/T05 와 동일. 카테고리별 시드 + LayerDiffuse + prompt template (해당 chapter 의 archetype 사용).

### 2.2 사용자 review

8 sprite batch 한 번에 review. iter cap 3.

## 3. 검증 기준

### 3.1 자산
- 8 PNG 모두 alpha transparent 30~70%
- 카테고리 톤 일관 (해당 카테고리 시드)
- 1라운드 ~ CH 35 시각 회귀 X

### 3.2 Auditor 재실행
- T02 Auditor 재실행 시 손상 8 PNG 사라짐
- V2 archetype 폴백 = OK (또는 chapter override 정상 사용)

### 3.3 SaveData / EnemyData
- CH 14, 22, 25, 30, 33, 34 Play Mode 진입 시 적 sprite 정상 표시

## 4. commit 메시지

```
5008 Sprint 5.5 / T05.5 — 손상 8 chapter override 흡수 (조건부 — D2=C fallback 깨짐 시)

[변경]
- Art/Generated_v2/Enemies/Chapters/CH{14,22,25,30,33A,33B,34}_*.png 재생성
- LayerDiffuse + 카테고리별 prompt prefix
- 시드 분배 (Nature 33333 / Civ 55555 / Fantasy 66666)

[검증]
- 8 PNG transparent 30~70%
- 카테고리 톤 일관
- Auditor 재실행 시 fallback OK (회귀 X)
- 1라운드~CH 35 시각 정상

[활성화 조건 (D2=C)]
- T02 V2 archetype 폴백 검증 = 깨짐 → 본 commit 활성화
- fallback OK 시 → skip (5008.1 anchor 만)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 5. 관련 문서

- PRD §2 Sprint 5.5
- decisions D2=C (Auditor 검증 후 결정)

## 6. 알려진 위험

- **fallback 검증 부정확** → T02 의 ValidateV2ArchetypeFallback() 정확성에 의존. EnemyData SO 의 sprite 참조 필드 정확히 확인 필요
- **chapter override sprite 톤 불일치** → 카테고리 시드 통일로 보정
- **PRD 범위 확장 (6 → 7 commit)** → fallback 깨짐 시 정당화 (회귀 방지). fallback OK 시 미실행으로 PRD 범위 보존
