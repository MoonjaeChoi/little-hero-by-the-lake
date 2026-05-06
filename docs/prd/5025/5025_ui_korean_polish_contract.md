# Sprint Contract: 5025 UI Korean Polish

> PGE Phase 4 · 2026-05-06

---

## C1 컴파일 기준

- KoreanLabelAuditor.cs 컴파일 errors=0
- 신규: Editor/KoreanLabelAuditor.cs ≤ 100L
- 수정: SO/Panel 텍스트 갱신 (코드 변경 0)

## C2 Editor Test

- 기존 153 회귀 0
- D1=B 신규 NUnit 0 (Auditor 는 Editor 도구 — Test 제외)

## C3 ProjectValidator

- errors=0 (5024 baseline 171 동일)
- warnings 변동 ≤ 0

## C4 Reflection

| Type | 검증 |
|---|---|
| `LittleHero.EditorTools.KoreanLabelAuditor` | `[MenuItem("Tools/LittleHero/Audit Korean Labels")]` + `Run()` static |

## C5 SO+Asset

- 영문 잔존 0건 (KoreanLabelAuditor 보고서 기준 — D1=B whitelist 적용 후)
- ChapterData 35 displayName 한국어 정합

## C6 Play 시나리오

### 시나리오 1 — Attic + HUD 한국어 시각

- Attic Play → 캐릭터 라벨 / 화폐 라벨 / 챕터 진입 라벨 모두 한국어
- HUD: Level/Wave/Health/Currency 라벨 한국어

### 시나리오 2 — KoreanLabelAuditor 결과

- `Tools/LittleHero/Audit Korean Labels` 실행
- `docs/little_hero/korean_label_audit_report.md` 생성
- 영문 잔존 0건 (whitelist 패턴 제외)

### 시나리오 3 — 폰트 잘림 사용자 시각 보고

- 9 우선순위 영역 Play 시 폰트 잘림 발견 보고
- D2=C: 사용자 보고 → spot fix 또는 5025.1 backlog

---

## 합격 기준

### 최소
- C1+C3+C4 ✅
- 시나리오 1+2 PASS

### 완전
- 모든 카테고리 + 시나리오 1+2+3 + 폰트 잘림 0건

---

## 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 |
