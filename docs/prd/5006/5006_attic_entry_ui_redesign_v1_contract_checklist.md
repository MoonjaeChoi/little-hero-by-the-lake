# Evaluator Checklist: 5006 attic_entry_ui_redesign_v1

## 자동 검증 (스크립트 / MCP)

### C1 컴파일
- [ ] `mcp__UnityMCP__refresh_unity` 후 errors=0
- [ ] `editor_state.isCompiling == false`

### C2 Editor Test
- [ ] Test Runner > Edit Mode > Run All → 6/6 PASS
- [ ] AtticEntryUITests 클래스 존재
- [ ] 누적 NUnit 73

### C3 Validator
- [ ] `Tools/LittleHero/Validate Project` 실행
- [ ] errors=0
- [ ] §15.6 LINE_LIMIT 200 통과 — 신규 모두 100L 이내

### C4 Reflection 정적
- [ ] AtticGroupLayoutPresenter Type + 4 RectTransform field + OnValidate
- [ ] AtticPanelEntryView Type + nested PanelEntry + OnEnable/OnDisable
- [ ] EndgameMenuPanel.OnEnable
- [ ] 5005 7 Type 회귀 (AtticController + 6 신규)

### C5 SO
- [ ] 5006 신규 SO 0
- [ ] 기존 SO 회귀 X

## 수동 검증 (사용자 Play Mode)

### S1 — 4 그룹 시각 구분
- [ ] 13 버튼 4 색 구분 (G1~G4)
- [ ] 좌우 분산 (G1+G2 / G3+G4)
- [ ] 44pt warning 0

### S2 — 10 panel 진입
- [ ] 10 버튼 → panel 활성
- [ ] 5005 PanelToggleManager 자동 비활
- [ ] 5005 잔여 3 (가챠/마법의돌/한정상점) 회귀 X

### S3 — EndgameMenu (D3=C)
- [ ] CH 35 미클리어 → 즉시 비활 + 토스트
- [ ] CH 35 클리어 후 정상

### S4 — 44pt
- [ ] OnValidate warning 0

### S5 — 5005 회귀
- [ ] 6 컴포넌트 모두 정상
- [ ] 5002 D2 toast 정상

## 비기능
- [ ] 60fps 유지
- [ ] 빌드 성공 (해당 시)

## 합격 판정
- 자동 모두 ✅ + 시나리오 S1+S2 ✅ → **최소 합격**
- 자동 모두 ✅ + 시나리오 S1~S5 모두 ✅ → **완전 합격**
- 자동 1개라도 ❌ → **FAIL** (Phase 5 수정 → 반복 max 2회)
