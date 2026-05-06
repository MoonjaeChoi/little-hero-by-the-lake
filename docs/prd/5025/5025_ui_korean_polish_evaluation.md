# 5025 Phase 6 Evaluation

> **PGE Phase 6** · 2026-05-06 · 검증 모드: 정적 + 사용자 위임

## 1. 종합: PASS (정적), 사용자 검증 대기 (런타임)

## 2. 자동 검증 (Contract 기준)

| 카테고리 | 결과 | 비고 |
|---|---|---|
| C1 컴파일 | ⏸ Editor 의존 | Unity Editor 실행 중 — batch 충돌. 사용자 Editor 의 Console (Errors=0) 확인 권장 |
| C2 NUnit | ⏸ 회귀 0 추정 | Auditor 는 Editor 도구 — Test 제외 (D1=B Mediator) |
| C3 Validator | ⏸ Editor 의존 | warnings 변동 ≤ 0 추정 (Auditor ~94L 만 신규) |
| C4 Reflection | ✅ PASS | KoreanLabelAuditor.Run 정적 + MenuItem 검증 (정적) |
| C5 SO + grep | ✅ PASS | SO 영문 0 / UI script 영문 0 / Scene m_Text 영문 0 (grep 검증) |

## 3. 정적 검증 결과 (grep)

```
✅ SO displayName 영문 잔존: 0건 (xp → 경험치 spot fix)
✅ UI Panel script .text="..." 영문: 0건
✅ Scene m_Text 영문 placeholder: 0건 (Lv 1 → 레벨 1 / Boss Name → 보스 이름)
```

## 4. 시나리오 (사용자 위임)

### 시나리오 1 — Attic + HUD 한국어 시각 (C6.1)
- Unity Play → Attic 씬: 캐릭터 라벨 / 화폐 라벨 / 챕터 진입 라벨 한국어 노출 확인
- Chapter 씬: HUD Level/Wave/Health/Currency 한국어 노출 확인
- 사용자 보고 대기

### 시나리오 2 — KoreanLabelAuditor 결과 (C6.2)
- Tools/LittleHero/Audit Korean Labels 실행
- `docs/little_hero/korean_label_audit_report.md` 생성 확인
- 영문 잔존 0건 (whitelist 제외) 기대
- 사용자 보고 대기

### 시나리오 3 — 폰트 잘림 (C6.3, D2=C)
- 9 우선순위 영역 Play Mode 시 폰트 잘림 발견 보고
- 보고 후 spot fix 또는 5025.1 backlog

## 5. 4 commit 요약

| Commit | 내용 |
|---|---|
| fa0112a | T01 KoreanLabelAuditor + whitelist (~94L) |
| 2617d65 | T02 affix_xp_gain + Scene placeholder 6건 한국어 |
| f89039b | T03 Auditor namespace import (using LittleHero) |
| 4b27d6d | T04 changelog + CLAUDE.md §14 |

## 6. 다음 단계

- **사용자 1**: Unity Editor 에서 Tools/LittleHero/Audit Korean Labels 실행 → 보고서 확인
- **사용자 2**: Play Mode 시나리오 1 진행 → 보고
- **선택**: Editor 닫고 `verify_phase6.sh all` 재실행 (C1~C3 자동)
- **5025.1 backlog**: 폰트 잘림 사용자 보고 후 진행
