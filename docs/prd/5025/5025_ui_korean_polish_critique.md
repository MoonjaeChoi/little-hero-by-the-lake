# 5025 PRD 리뷰 (Critique)

> **검토자**: Claude (prd-critiquer Phase 2.1)
> **대상**: `5025_ui_korean_polish_prd.md`
> **일시**: 2026-05-06

## 0. 결정 이력

| 일자 | 검토자 | 검토 |
|---|---|---|
| 2026-05-06 | Claude | 초기 — Phase 2.1 critique. 마지막 backlog 시스템 |

---

## 1. 종합 점수: 9.0/10

| 카테고리 | 점수 | 비고 |
|---|---|---|
| OOP §15 정합 | 9.5/10 | KoreanLabelAuditor 80L 정합 + §15.4 SO 우선 |
| PG-12 톤 | 10/10 | UI 텍스트만 — 영향 0 |
| 성능 | 10/10 | Editor 도구 — 런타임 0 |
| 시스템 통합 | 8.5/10 | 35 챕터 + 30+ Panel 검수 — 영문 잔존 의도된 영역 처리 결정 필요 |
| Sprint 분해 | 8.5/10 | 4 Sprint 균형. 우선순위 1~2 (Attic + HUD) 사용자 시각 review 의존 |

---

## 2. Accept 항목 (5)

### 2.1 ✅ KoreanLabelAuditor Editor 도구 (§15.12)
- Tools/LittleHero/Audit Korean Labels 메뉴 + 보고서 생성
- 영문 잔존 자동 검출 + Sprint 2~4 baseline

### 2.2 ✅ 5 우선순위 영역 분배 적절
- Sprint 2 (Attic + HUD) 첫 화면 → Sprint 3 (Map/Inv/Equip/Gacha) → Sprint 4 (잔여)
- 사용자 첫인상 우선

### 2.3 ✅ Legacy Text 폰트 잘림 검사 명시
- CLAUDE.md §10 (TMP X / Legacy Text) 정합
- maxWidth / Vertical Overflow 조정

### 2.4 ✅ §15.4 SO 우선 정합
- 한국어 displayName 모두 SO 필드 (이미 정합)
- 5020.1 패턴 답습 (scene 하드코딩 라벨은 SO 바인딩 점진 전환)

### 2.5 ✅ 동작 변경 0
- UI 텍스트 + 폰트만 — 코드 변경 0
- C2 NUnit 회귀 안전망 (5024 153 PASS) 충분

---

## 3. Reject 항목 (0)

(없음)

---

## 4. Discuss 항목 (3)

### 4.1 🟡 D1 — 영문 잔존 의도된 영역 처리

**배경**: PRD §6 알려진 위험 — "의도된 영문 (예 affix targetName 내부 enum) → 변경 시 코드 영향". KoreanLabelAuditor 가 displayName 만 검사 (targetName 제외) 명시.

**그러나 다른 의도된 영문 영역**:
- "Lv. 50" 표기 (HUD)
- "DPS / 치명타" 같은 게임 어휘 (CLAUDE.md §2 허용)
- 영문 ID (hero_warrior 등 — 코드 키)

**옵션**:
- **A** (PRD 원안): KoreanLabelAuditor 가 displayName 만 + 사용자 시각 보고
- **B** ★ Claude 권장: 의도된 영문 whitelist 명시 — Auditor 가 알려진 패턴 (예 "Lv\\.", DPS, ID format) 자동 skip
- **C**: 보고만 + 의도/오해 수동 분류

**Discuss**: B 권장 — 자동화 ↑.

### 4.2 🟡 D2 — 폰트 잘림 fix 자동 vs 수동

**배경**: PRD Sprint 4 의 폰트 잘림 spot fix 가 "사용자 Play 시 사례별 보고 후 수정" 명시. 자동 검출 어려움.

**옵션**:
- **A** (PRD 원안): 사용자 시각 보고 후 수동 (RectTransform 조정)
- **B**: Editor 자동 검출 (Text 컴포넌트 width vs 텍스트 길이 추정)
  - 장점: 자동
  - 단점: 추정 불정확 (font / wrap 모드 변수)
- **C** ★ Claude 권장: 사용자 보고 + 검출 실패 시 5025.1 후속 (미보고 영역 수동 검수)

**Discuss**: C 권장.

### 4.3 🟡 D3 — Sprint 4 잔여 영역 우선순위 (Endgame/Forge/Memory)

**배경**: PRD §1 우선순위 7~9 (Endgame / Forge / Memory) 가 Low — "사용자 첫인상 영향 작음".

**옵션**:
- **A** (PRD 원안): 4 Sprint 모두 진행 (잔여 영역 포함)
- **B**: Sprint 1~3 만 진행 + Sprint 4 잔여 5025.1 backlog (출시 전 검토)
- **C** ★ Claude 권장: PRD 원안 (옵션 A) — 4 Sprint 모두 진행, 출시 직전 일괄 검수 완료

**Discuss**: C 권장.

---

## 5. CLAUDE.md §15 정책 위반 점검

| §15 항목 | 상태 |
|---|---|
| §15.1 SRP | ✅ KoreanLabelAuditor 단일 책임 (영문 검출 + 보고) |
| §15.4 SO 우선 | ✅ |
| §15.6 100줄 | ✅ Auditor 80L |
| §15.12 Validator | ✅ Editor 도구 인프라 |

---

## 6. 권장 변경

### 즉시 변경 (Reject 없음)

(없음)

### 사용자 결정 후 변경 (Discuss)

- **D1**: B 권장 (whitelist 자동 skip)
- **D2**: C 권장 (사용자 보고 + 5025.1 후속)
- **D3**: C 권장 (PRD 원안 4 Sprint)

---

## 7. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 작성. 점수 9.0/10. Accept 5 / Reject 0 / Discuss 3 |
