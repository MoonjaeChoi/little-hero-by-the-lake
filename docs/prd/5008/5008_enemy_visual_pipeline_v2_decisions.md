# 5008 Chapter Enemy Asset Quality v1 — PRD Decisions

> **Phase 2.3 Mediator 산출** — 미결 항목 3건 결정 기록
> 모드: ai-assisted (Claude 권장 + 사용자 일괄 채택 "권장안대로") · 일시: 2026-05-04

## 1. 요약 표

| Priority | Item | Topic | Decision | Status |
|---|---|---|---|---|
| Medium | D1 | EnemySpriteAuditor 구현 방식 | **C** — A + 임시 readableTrue 복원 패턴 | Done |
| Medium | D2 | 손상 8 chapter override 처리 | **C** — Auditor V2 archetype 폴백 검증 후 결정 | Done |
| Medium | D3 | skip sprite 후속 정책 | **A+B 결합** — Sprint 6 docs skip list + 5008.1 anchor 우선 | Done |

## 2. Detailed Decisions

### 2.1 D1 — EnemySpriteAuditor alpha 검증 구현 방식

- **Decision**: **옵션 C — A + 임시 readableTrue 복원 패턴**
- **Mode**: ai-assisted (Claude 권장 + 사용자 "권장안대로" 채택)
- **Rationale**:
  - GetPixels32 의 단순함 + Unity 표준 API 활용 유지
  - 임포트 설정 영구 변경 회피 (262 PNG Read/Write Enabled 일괄 변경 방지)
  - try/finally 강제로 복원 단계 누락 위험 최소화
  - PNG byte 직접 파싱 (B) 의 IDAT chunk decode 복잡도 + SixLabors.ImageSharp 의존성 회피 (§15.4 외부 패키지 추가 최소화)
- **PRD Impact**:
  - §2.1 Sprint 2 파일 명세 — `EnemySpriteAuditor.cs` 구현 방식 명시 (AssetImporter.SetReadable + try/finally + GetPixels32 + 복원)
  - Sprint 2 commit 메시지 — 구현 방식 명시 ("AssetImporter readableTrue 임시 복원 패턴")
  - Sprint 2 검증 기준 — "임포트 설정 변동 0 (복원 검증)" 추가
- **Trade-off**:
  - PNG byte 파싱 (B) 의 메모리 효율 손실 (262 PNG × 임시 2× 메모리)
  - 다만 audit 은 Editor 1회 실행이라 영구 메모리 영향 없음 → 손실 무시 가능

---

### 2.2 D2 — 손상 8 chapter override 처리 우선순위

- **Decision**: **옵션 C — Auditor (Sprint 2) 가 V2 시스템 archetype 폴백 검증 후 Sprint 5.5 추가 여부 결정**
- **Mode**: ai-assisted (Claude 권장 + 사용자 일괄 채택)
- **Rationale**:
  - 데이터 기반 결정 — fallback 정상 시 8 PNG 미처리 정당화 (5008.1 anchor)
  - fallback 깨짐 시 Sprint 5.5 강제 추가로 회귀 방지
  - 옵션 A (anchor 만) 의 회귀 리스크 + 옵션 B (즉시 흡수) 의 PRD 범위 확장 양극단 회피
  - Auditor 가 어차피 작성되므로 검증 추가 비용 < 사용자 회귀 비용
- **PRD Impact**:
  - §2.1 Sprint 2 산출물 — "V2 시스템 archetype 폴백 검증" 추가 (chapter override 손상 시 archetype 사용 여부 확인)
  - §1 Sprint 표 — Sprint 5.5 (조건부) 명시 (활성화 조건: Auditor fallback 검증 결과 깨짐)
  - §3 의존성 그래프 — Sprint 5.5 조건부 분기 추가
  - §6 알려진 위험 — "손상 8 PNG" 항목 완화 갱신 ("Auditor 검증 결과에 따라 Sprint 5.5 또는 anchor")
  - 총 commit: 6 (fallback 정상) ~ 7 (Sprint 5.5 활성화)
- **Trade-off**:
  - 옵션 A (anchor) 의 PRD 단순성 손실 — Sprint 2 → Sprint 3 진입 전 사용자 확인 1회 추가
  - 다만 Auditor report 자동 생성이라 사용자 결정 부담 < 30초

---

### 2.3 D3 — Sprint 3~5 max 3 iter 후 skip sprite 후속 정책

- **Decision**: **옵션 A+B 결합 — Sprint 6 docs 에 skip list 명시 + 5008.1 anchor 에 우선 처리 표시**
- **Mode**: ai-assisted (Claude 권장 + 사용자 일괄 채택)
- **Rationale**:
  - 사용자 부담 0 (skip 0 강제 옵션 C 의 무한 review 회피)
  - 추적성 확보 (옵션 A 단일 의 5008.1 자동 우선 X 손실 보완)
  - Sprint 6 docs 1줄 + 5008.1 anchor 1줄 추가 비용 최소
- **PRD Impact**:
  - §2 Sprint 3 프로세스 4 — "skip 시 Sprint 6 docs skip list 추가 + 5008.1 anchor 우선 표시" 명시
  - §2 Sprint 6 작업 — "skip list docs 작성" 추가
  - §5 Definition of Done — "skip list docs (있으면) + 5008.1 anchor 우선 표시" 추가
  - §7 본 PRD 범위 외 — 5008.1 anchor 에 "skip list 우선 처리" 1줄 추가
- **Trade-off**:
  - 옵션 C (skip 0 강제) 의 30 archetype 완성 보장 손실 — 사용자 review 사이클 무한 반복 위험이 더 큼
  - 옵션 A 단일 의 5008.1 우선순위 자동 반영 X 손실 — B 결합으로 보완

---

## 3. 통계

- **Total Items**: 3
- **AI Recommendations Followed**: 3/3 (100%)
- **User Overrides**: 0
- **모드**: ai-assisted (사용자 "권장안대로" 일괄 채택)

## 4. v2_prd.md 변경 diff 요약

(rebuttal 단계 v2 초안 → mediator 최종 의 변경사항)

### 신규 명세 추가

- **§2.1 Sprint 2 파일 명세** — EnemySpriteAuditor.cs 구현 방식 명시 (D1=C: AssetImporter.SetReadable + try/finally + GetPixels32 + 복원)
- **§2.1 Sprint 2 산출물** — V2 archetype 폴백 검증 추가 (D2=C)
- **§1 Sprint 표** — Sprint 5.5 (조건부) 활성화 조건 구체화 (D2=C: Auditor 검증 결과)
- **§2 Sprint 3 프로세스** — skip 시 Sprint 6 docs + 5008.1 anchor 정책 명시 (D3=A+B)
- **§2 Sprint 6 작업** — skip list docs 추가 (D3=A+B)

### 검증 기준 갱신

- Sprint 2 검증 — "임포트 설정 변동 0" 추가 (D1=C 복원 검증)
- Sprint 2 검증 — "V2 archetype 폴백 검증 report 정상" 추가 (D2=C)
- Sprint 6 검증 — "skip list docs (있으면) 명시" 추가 (D3=A+B)

### 의존성 그래프 갱신

- Sprint 5.5 조건부 분기 활성화 조건 명시 (D2 결과 기반)

### 위험 + 완화 갱신

- "Auditor 구현 방식 임의 결정" → 해소 (D1=C 채택)
- "손상 8 PNG" → 완화 갱신 (D2=C: Auditor 검증 결과 기반)
- "skip sprite 후속 추적 부재" → 해소 (D3=A+B 채택)

### 총 commit 변동

- 6 commit (fallback 정상) ~ 7 commit (Sprint 5.5 활성화)
- 시간: 약 4~6시간 (변동 없음, Sprint 5.5 +30분 조건부)

## 5. 변경 이력

- 2026-05-04: 초기 작성 (Phase 2.3 Mediator). ai-assisted 모드, 사용자 "권장안대로" 일괄 채택, AI 권장 채택률 3/3.
