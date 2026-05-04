# 5008 Chapter Enemy Asset Quality v1 — PRD Discussion Topics

> **Phase 2.2 산출** — mediator (Phase 2.3) 에서 결정 필요한 미결 항목.
> 일시: 2026-05-04 · 출처: critique (3 Discuss) + rebuttal 추가 분석 0

## High Priority Items

(없음)

## Medium Priority Items

### D1. EnemySpriteAuditor alpha 검증 구현 방식

- **Original Criticism**: Sprint 2.1 PRD 명세 "Pillow 알고리즘 C# 구현 또는 Texture2D.GetPixels32 사용" — 두 옵션 모호. 구현 차이 큼 (메모리 / 임포트 설정 / 단순도 trade-off).
- **Current Position**: PRD 그대로 진행 시 Sprint 2 commit 시점 구현자 임의 선택 위험.
- **Discussion Points**:
  - **옵션 A. Texture2D.GetPixels32**: Read/Write Enabled 임포트 설정 필요. 메모리 2× 사용 (262 PNG × 2)
    - 장점: 가장 단순, Unity 표준 API
    - 단점: 임포트 설정 일괄 변경 (262 PNG), 메모리 비용
  - **옵션 B. PNG byte 직접 파싱**: System.IO.File.ReadAllBytes + IDAT chunk decode → alpha channel 직접 추출
    - 장점: 임포트 설정 변경 X, 메모리 효율, AssetImporter 의존 X
    - 단점: 구현 복잡도 ↑ (IDAT chunk decode 직접), SixLabors.ImageSharp 패키지 도입 또는 자체 parser
  - **옵션 C. A + 임시 readableTrue 복원 패턴**: AssetImporter.SetReadable(true) 일시 적용 → audit → 복원
    - 장점: 임포트 설정 영구 변경 X, GetPixels32 단순함 유지
    - 단점: 복원 단계 누락 시 메모리 영구 증가 위험
- **Proposed Resolution Path**: **옵션 C 권장** — 단순함 + 임포트 설정 영구 변경 회피. 복원 try/finally 강제로 안전.
- **PRD Impact**:
  - Sprint 2.1 §파일 명세 변경 (EnemySpriteAuditor 구현 방식 명시)
  - Sprint 2 commit 메시지 변경 (구현 방식 명시)
  - 검증 기준 갱신 (임포트 설정 변동 0 확인)

---

### D2. 손상 8 chapter override 처리 우선순위

- **Original Criticism**: 5008 PRD 가 D5=A 로 30 archetype 1개 (Enemy_Nature_Medium) 만 본 PRD. 나머지 8 chapter override (CH14/CH22/CH25/CH30/CH33×2/CH34) 는 5008.1 anchor 만 명시. 5008 PASS 후 사용자 "1라운드 적 흰 박스" 회귀 리스크.
- **Current Position**: PRD 그대로 진행 시 5008.1 anchor 만, V2 시스템 archetype 폴백 작동 여부에 따라 5008 만 PASS 해도 회귀 가능.
- **Discussion Points**:
  - **옵션 A. anchor 만 (현재 PRD)**: 5008 종료 후 5008.1 즉시 진행
    - 장점: 5008 범위 명확, ROI 우선
    - 단점: 사용자 회귀 시 5008.1 강제 진입 (사용자 부담)
  - **옵션 B. Sprint 5.5 추가 — 손상 8 chapter override 즉시 흡수**: 5008 범위 내 1 commit 추가 (30분)
    - 장점: 5008 PASS = 회귀 0 보장
    - 단점: PRD 범위 확장 (6 → 7 commit), 5008 시간 ↑
  - **옵션 C. Auditor (Sprint 2) 가 V2 시스템 archetype 폴백 검증 후 결정**: fallback 정상 시 8 PNG 무시 / 깨짐 시 Sprint 5.5 강제
    - 장점: 데이터 기반 결정, 정당화 명확
    - 단점: Sprint 2 → Sprint 3 진입 전 사용자 결정 1회 추가
- **Proposed Resolution Path**: **옵션 C 권장** — Auditor 가 fallback 검증 결과 report 생성 후 사용자 결정. 데이터 기반 + 5008 범위 보존.
- **PRD Impact**:
  - 옵션 B 채택 시: Sprint 5.5 추가 + Sprint 표 + 의존성 그래프 갱신
  - 옵션 C 채택 시: Sprint 2 검증 기준에 "V2 시스템 archetype 폴백 검증 + 결과에 따라 Sprint 5.5 결정" 추가
  - 옵션 A 채택 시: 변경 X (PRD 그대로)

---

### D3. Sprint 3~5 max 3 iteration 후 skip sprite 후속 정책

- **Original Criticism**: Sprint 3 프로세스 4 "max 3 iteration 내 PASS 못 하면 사용자 결정 (다른 시드 / 다른 prompt / skip)" — skip 처리한 sprite 의 후속 처리 정책 미명시.
- **Current Position**: PRD 그대로 진행 시 skip sprite 가 5008 종료 후 추적 안 됨.
- **Discussion Points**:
  - **옵션 A. 기존 sprite 유지 + skip list docs 명시**: Sprint 6 docs 에 skip list 명시
    - 장점: 단순, 사용자 부담 0
    - 단점: 추적 docs 만, 5008.1 우선순위 자동 반영 X
  - **옵션 B. 5008.1 후속 자동 enroll**: skip list 자동으로 5008.1 anchor 의 우선 처리 항목으로 표시
    - 장점: 추적성 + 5008.1 진입 시 자동 우선
    - 단점: 5008.1 PRD 작성 시점에 anchor 갱신 필요
  - **옵션 C. skip 0 강제**: 사용자 review 무한 반복 (max 3 iter cap 제거)
    - 장점: 5008 PASS = 30 archetype 모두 완성
    - 단점: 사용자 부담 ↑, Sprint 시간 폭증 위험
  - **옵션 A+B 결합**: Sprint 6 docs 에 skip list 명시 + 5008.1 anchor 에 우선 처리 표시
- **Proposed Resolution Path**: **옵션 A+B 결합 권장** — 사용자 부담 없이 추적성 확보 + 5008.1 자동 우선. Sprint 6 docs commit 1줄 + 5008.1 anchor 1줄 추가.
- **PRD Impact**:
  - Sprint 6 commit 메시지 + Definition of Done 에 "skip list docs" 항목 추가
  - §7 본 PRD 범위 외 — 5008.1 anchor 에 "skip list 우선 처리" 1줄 추가

---

## Low Priority Items

(없음 — Low Priority 2건 모두 rebuttal 단계 Accepted, v2 초안 직접 반영)

## 요약

- **Total Items**: 3
- **High**: 0 / **Medium**: 3 / **Low**: 0 (Accepted 처리)
- **권장 mode**: ai-assisted (Claude 추천 + 사용자 확인)
- **권장 결정 일괄 입력**: `D1=C, D2=C, D3=A+B`
