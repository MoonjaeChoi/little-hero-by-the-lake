# 5008 Chapter Enemy Asset Quality v1 — PRD 리뷰

## 0. 결정 이력

- 2026-05-04: Claude Code (자동) 리뷰 — Phase 2.1 Critique
- 입력: `5008_enemy_visual_pipeline_v2.md` (215L) + `5008_enemy_visual_pipeline_v2_prd.md` (277L)
- 컨텍스트: CLAUDE.md §15 OOP 정책 + 5006.1 ComfyUI batch 사례 + 5007 cinematic effect

## 1. 종합 점수: 9.0 / 10

| 카테고리 | 점수 | 비고 |
|---|---|---|
| OOP §15 정합 | 9/10 | EnemySpriteAuditor ~80L ✅, Test 누락은 Editor tool 이라 정당 |
| PG-12 톤 | 10/10 | prompt 자체에 "cute cartoon", "playful", "magical sparkles" 명시 ✅ |
| 성능 | 9/10 | Editor 도구 — 런타임 영향 X. SpriteAtlas 재구성 영향 명시 부족 (-1) |
| 시스템 통합 | 9/10 | EnemyData SO path 그대로 (참조 깨짐 X) ✅. 다만 손상 9 PNG 중 chapter override 8개 미처리 추적 anchor 만 |
| Sprint 분해 | 9/10 | 의존성 그래프 명확 (1+2 병렬, 3→4→5 순차). 사용자 review 의존 → 시간 예측 어려움 (-1) |

## 2. Accept 항목 (10)

- ✅ **D1=B (30 archetype 우선)** — V2 시스템에서 archetype 가 모든 챕터 재사용 → ROI 최고. 점진 확장 (B → C → D) 명시
- ✅ **D2=A (comfyui-image 스킬에 LayerDiffuse 통합)** — 기존 스킬 확장이 신규 스킬 분리보다 유지보수 효율
- ✅ **D3=B (카테고리별 prompt prefix)** — 일관성 + 카테고리 식별성 균형. 시드 고정 (22222/33333/44444/55555/66666) 으로 카테고리 통일
- ✅ **D4=A (카테고리 batch 6 sprite review)** — 일괄 30 sprite (B) 는 사용자 부담, 1개씩 (C) 는 30 사이클 — A가 균형
- ✅ **D5=A (30 archetype 재생성 시 손상 1개 함께 처리)** — Enemy_Nature_Medium 본 PRD 범위 내 자연스럽게 흡수
- ✅ **D6=A (Editor 메뉴 자동 검증)** — Validator 역할 + 262 PNG 일괄 audit
- ✅ **Sprint 의존성 그래프** — Sprint 1+2 병렬, 3→4→5 순차 (사용자 review 사이클), 6 docs 종결
- ✅ **§15.4 SO 우선** — prompt template = MD 문서 (`enemy_sprite_prompts_v1.md`), 코드 hardcode X
- ✅ **§15.6 100L 룰** — EnemySpriteAuditor ~80L 추정 통과
- ✅ **Play 검증 시나리오 4종 (S1~S4)** — 카테고리 일관성 / 차별성 / Alpha 정확도 / V5 톤 통일

## 3. Reject 항목 (0)

(없음 — 본 PRD 는 자산 품질 PRD, OOP 직접 영향 적음)

## 4. Discuss 항목 (3)

### 🟡 D1. EnemySpriteAuditor alpha 검증 구현 방식

**현재 PRD**: "Pillow 알고리즘 C# 구현 또는 Texture2D.GetPixels32 사용" (Sprint 2.1)

**문제**: 두 옵션이 모호함. 구현 차이 큼.

**옵션**:
- **A. Texture2D.GetPixels32**: 간단하지만 Read/Write Enabled 필요 (메모리 2× 사용 + 임포트 설정 변경). 262 PNG 임포트 옵션 일괄 변경 필요
- **B. PNG byte 직접 파싱**: System.IO.File.ReadAllBytes + IDAT chunk decode → alpha channel 추출. Read/Write Enabled 불필요, 메모리 효율
- **C. 외부 Python script**: Editor 외부 Pillow 호출. 가장 정확하지만 Unity 통합 깨짐

**Claude 권장**: **B (PNG byte 파싱)** — 임포트 설정 변경 회피 + 메모리 효율. SixLabors.ImageSharp 또는 자체 IDAT parser. 그러나 구현 복잡도 ↑.

**대안 권장**: **A (GetPixels32) + 임시 readableTrue 변환**: AssetImporter.SetReadable(true) 일시 적용 → audit → 복원. 단순.

**사용자 결정 필요**: A 단순 vs B 효율 vs A+복원 패턴

---

### 🟡 D2. 손상 9 PNG 중 chapter override 8개 처리 우선순위

**현재 PRD**: D5=A 로 30 archetype 1개 (Enemy_Nature_Medium) 만 본 PRD. 나머지 8 chapter override (CH14/CH22/CH25/CH30/CH33×2/CH34) 는 5009 후속 anchor 만 명시.

**문제**: 사용자 직전 보고 ("1라운드 적 흰 박스") 의 직접 원인은 chapter override sprite 인 것이 일부 (PhaseFContentSeed V2 시스템에서 archetype 폴백 작동 시 archetype 사용). 5008 PRD 가 PASS 되어도 손상 8 chapter override 가 남아있으면 사용자 회귀 리스크.

**옵션**:
- A. 본 PRD 진행 후 즉시 5009 진행 (현재 설계 — anchor 만)
- **B. Sprint 5.5 추가 — 손상 8 chapter override 즉시 재생성** (5008 범위 내 흡수, 1 commit, 30분)
- C. EnemySpriteAuditor 가 V2 시스템에서 archetype 폴백 가능 여부 검증 → fallback OK 면 8 PNG 무시 / 아니면 Sprint 5.5 강제

**Claude 권장**: **C (검증 후 결정)** — Auditor (Sprint 2) 가 V2 시스템 archetype 폴백 검증 추가. fallback 정상 시 8 PNG 미처리 정당화 (5009 anchor). 깨짐 시 Sprint 5.5 강제 추가.

**사용자 결정 필요**: A (anchor 만) / B (즉시 흡수) / C (검증 후 결정)

---

### 🟡 D3. Sprint 3~5 max 3 iteration 후 "skip" sprite 의 후속 정책

**현재 PRD**: "max 3 iteration 내 PASS 못 하면 사용자 결정 (다른 시드 / 다른 prompt / skip)" (Sprint 3 프로세스 4)

**문제**: skip 처리한 sprite 의 후속 처리 정책 미명시. 5008 종료 후 해당 sprite 는?

**옵션**:
- A. 기존 sprite 유지 (재생성 X) — Sprint 6 docs 에 skip list 명시
- **B. 5009 후속 자동 enroll** — skip list 자동으로 5009 anchor 에 우선 처리 항목으로
- C. 5008 자체 종료 차단 — skip 0 까지 반복 (사용자 부담 ↑)

**Claude 권장**: **A + B 결합** — Sprint 6 docs 에 skip list 명시 + 5009 anchor 에 우선 처리 표시. 사용자 부담 없이 추적성 확보.

**사용자 결정 필요**: A (skip 무시) / B (5009 우선 enroll) / C (skip 0 강제)

## 5. CLAUDE.md §15 정책 위반 점검

| 정책 | 결과 | 비고 |
|---|---|---|
| §15.1 SRP | ✅ | EnemySpriteAuditor 단일 책임 (audit + report) |
| §15.1 OCP | ✅ | switch 신규 0건, 카테고리 enum 사용 가능 |
| §15.4 SO 우선 | ✅ | prompt = MD 문서 (코드 hardcode X) |
| §15.6 100L | ✅ | Auditor ~80L 추정 |
| §15.7 DI | △ | Editor 정적 도구 — DI 적용 X 정당 |
| §15.9 Test | △ | NUnit 누락 — Editor tool 정적 helper 라 가치 낮음 (정당) |
| §15.11 안티 | ✅ | public 필드 X, Update X, GC 영향 없음 |
| §15.12 Validator | ✅ | EnemySpriteAuditor 가 Validator 역할 |

## 6. 권장 변경

### 즉시 변경 (Reject 없음 — 변경 0)

(없음)

### 사용자 결정 후 변경 (3 Discuss)

- **D1**: Auditor 구현 방식 선택 (A/B/C) → Sprint 2 commit 메시지 + 코드 명세 갱신
- **D2**: 손상 8 chapter override 처리 (A/B/C) → Sprint 5.5 추가 여부 결정
- **D3**: skip sprite 후속 정책 (A/B/C) → Sprint 6 docs 추가 또는 5009 anchor 우선표시

## 7. 추가 관찰 (정보)

### 7.1 SpriteAtlas 영향 (참고)

- 현재 EnemyAtlas SpriteAtlas SRP Batcher 사용 중 (CLAUDE.md §7)
- 30 PNG 재생성 시 SpriteAtlas 자동 재빌드 필요 (Unity 자동 또는 수동)
- Sprint 6 docs 에 SpriteAtlas 재빌드 단계 명시 권장 (사용자 빌드 시 누락 방지)

### 7.2 Sprint 1 fallback 워크플로 명시도

- "LayerDiffuse 노드 미설치 시 fallback (기존 워크플로 사용)" — 좋음
- 그러나 fallback 워크플로의 alpha 처리 한계 (DreamShaperXL Turbo native alpha 없음, dark BG flood-fill 후처리) 명시되면 사용자 기대치 정렬

### 7.3 prompt template 카테고리 매핑 (D3)

- 30 매핑 (5×6) 모두 enemy_sprite_prompts_v1.md 에 명시 — 좋음
- 카테고리별 색상 명시 (Meta=warm beige / Nature=green earth / Animal=warm orange / Civ=pink pastel / Fantasy=purple sparkle) 일관 ✅
- archetype 별 캐릭터 제안 (small mushroom / village kid / burly man / ...) 구체적 — 좋음

## 8. 변경 이력

- 2026-05-04: 초기 작성 (Phase 2.1 Critique). Accept 10 / Reject 0 / Discuss 3.
