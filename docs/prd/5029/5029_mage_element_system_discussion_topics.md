# 5029 Discussion Topics

> Phase 2.2 · 합계 **4**

## D1. 신규 4 pattern effect 강도 (Medium)

PRD §3.1 element 효과 수치:
- Fire: DOT 10% × 3 tick = 30% 추가
- Ice: MoveSpeed × 0.5 for 0.5s
- Lightning: 인접 적 1 추가 데미지 50%
- Arcane: 저항 무시 (1.0× 강제)

- **A** ★: PRD 수치 그대로 — 디자인 의도 우선, 후속 BalanceConfig 조정
- **B**: 보수 (DOT 5% × 3 / Ice 0.3s / Lightning 30%) — 약하게 시작, 강화 후속
- **C**: SerializeField + BalanceConfig — 디자이너 후속 조정 가능

★ Claude 권장: A — PRD 명시 수치, 5029.1 BalanceConfig 후속 (5022 D2=A SerializeField 답습)

## D2. weapon_thunder_wand baseStats (Medium)

신규 무기 baseStats 결정:

- **A** ★: weapon_meteor_rod 답습 (Wand 카테고리 정합)
- **B**: 8 Mage 무기 평균
- **C**: 신규 디자인 (Lightning 정체성 — AS 빠름, Dmg 약간 낮음)

★ Claude 권장: A — 5020/5021 Wand 카테고리 정합 + 단순화

## D3. PNG icon (Medium)

weapon_thunder_wand 의 무기 sprite icon:

- **A** ★: placeholder 임시 (기존 weapon_meteor_rod icon 재사용 + 색조) → 5029.2 후속 5008 baseline
- **B**: 5029 내 5008 baseline 생성 (Sprint 추가, ComfyUI generation, ~30분)
- **C**: 무 icon (UI 빈칸)

★ Claude 권장: A — 5029 scope 최소, 5029.2 후속 (5008 baseline 정착 패턴)

## D4. Sprint 3 a+b 통합 (Low)

Sprint 3a (Beam + Chain) + Sprint 3b (Vortex + Meteor) 통합:

- **A**: 분리 (PRD 원안 — 2 commit)
- **B** ★: 통합 — 4 pattern 1 commit (5022 답습)

★ Claude 권장: B — 4 pattern 모두 같은 의미 단위, 5022/5027 통합 답습

## 권장 답변

```
D1=A, D2=A, D3=A, D4=B
```

또는 "권장 그대로 진행".
