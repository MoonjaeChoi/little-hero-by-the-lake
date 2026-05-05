# T05: Sprint 3c — Sprite pivot + displayName + PSB ref Editor/ 이동

> **레이어**: Data
> **의존성**: T03 (PNG/SO rename 완료)
> **예상 시간**: 20분 / 1 commit (T04 와 병렬 가능)
> **PRD 출처**: Sprint PRD §6 (Sprint 3c)

---

## 1. 작업 항목

### 1.1 Sprite pivot (0.5, 0.1) — 3 idle PNG 만

**대상**: 3 wired PNG 의 `.meta` 파일 (T03 후 신 파일명)
- `Char_HeroWarrior.png.meta`
- `Char_HeroArcher.png.meta`
- `Char_HeroMage.png.meta`

**변경**:
```yaml
# Before (5008 default)
spritePivot: {x: 0.5, y: 0.5}
alignment: 0  # Center

# After (5020 보고서 §3-A)
spritePivot: {x: 0.5, y: 0.1}
alignment: 9  # Custom
```

**작업 방법**:
- Unity Sprite Editor → 3 PNG 각각 열기 → Pivot Custom (0.5, 0.1) → Apply (자동 .meta 갱신)
- 또는 .meta YAML 직접 편집 (3 파일)

> psb_ref PNG 는 변경 X (in-game 미사용).

### 1.2 displayName 갱신 — 3 SO

| SO | displayName |
|---|---|
| `Char_HeroWarrior.asset` | **전사 시우** |
| `Char_HeroArcher.asset` | **궁수 가영** |
| `Char_HeroMage.asset` | **마법사 연우** |

**작업**: Unity Inspector 에서 직접 편집 또는 .asset YAML 직접 편집.

> T03 에서 이미 같은 commit 에 적용했으면 본 단계 skip 가능. 미적용 시 본 commit 에서.

### 1.3 D5-B — `References/Characters_PSB/` → `Editor/References/Characters_PSB/` 이동

**작업**: PSB reference 폴더를 Editor 자동 빌드 제외 위치로 이동.

```bash
mkdir -p LittleHero/Assets/_Project/Art/Editor/References/
git mv LittleHero/Assets/_Project/Art/References/Characters_PSB \
       LittleHero/Assets/_Project/Art/Editor/References/Characters_PSB
```

→ Unity 의 `Editor/` 폴더 자동 빌드 제외 룰 활용. iOS 빌드 용량 영향 0.

---

## 2. 검증 기준

### 2.1 자동 검증 (refresh_unity + 5s + read_console)
- [ ] 3 idle sprite pivot (0.5, 0.1) 적용 — Unity Sprite Editor 또는 .meta 확인
- [ ] CharacterData Inspector 에서 displayName 한글 정상 노출
- [ ] `Editor/References/Characters_PSB/` 3 PSB ref PNG 존재
- [ ] `References/Characters_PSB/` 폴더 부재 (이동 완료)
- [ ] Unity 콘솔 errors=0

### 2.2 회귀
- [ ] 다른 sprite (적/UI) pivot 영향 X (3 hero idle 만 변경)
- [ ] PSB ref 폴더 이동 후 Build Settings → Asset Bundles 정합 (영향 없음)

---

## 3. commit 메시지

```
5020 Sprint 3c / T05 — pivot 발 밑 + displayName + PSB ref Editor/ 이동 (5/7)

[변경]
  ~ 3 idle PNG .meta — pivot (0.5, 0.5) → (0.5, 0.1) (보고서 §3-A)
  ~ 3 SO displayName: "전사 시우" / "궁수 가영" / "마법사 연우" (D1)
  R References/Characters_PSB → Editor/References/Characters_PSB (D5-B 빌드 제외)

[검증]
  - Sprite Editor pivot 적용 확인
  - displayName 한글 정상
  - PSB ref Editor/ 자동 빌드 제외
  - Unity 콘솔 errors=0

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 4. 관련 문서

- 보고서 `docs/little_hero/character/character_animation_research_v1.md` §3-A (pivot 발 밑)
- 디자인 §4.5 (D3-B SO 갱신) + §4.6 (sprite import 설정)
- v2 §차분 (D5-B PSB ref Editor/ 이동)

---

## 5. 알려진 위험

| 위험 | 완화 |
|---|---|
| pivot 변경 후 Player spawn 위치 어긋남 (발이 땅 아래) | T07 Play 시나리오 2 검증, PlayerController spawn Y 보정 가능 |
| PSB ref Editor/ 이동 후 임의 코드가 ref 사용 시 NRE | grep "Characters_PSB" 로 코드 사용 0건 확인 (Editor 자료라 코드 사용 없음) |
| displayName 한글 폰트 잘림 (Attic UI) | T07 Play 시나리오 1 에서 검증 |
| sprite alignment=9 (Custom) 가 다른 패턴 | 5008 답습 — Unity 표준 |
