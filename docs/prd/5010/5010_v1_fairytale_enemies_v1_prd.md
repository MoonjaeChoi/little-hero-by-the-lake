# 5010 — V1 11종 동화 적 — Sprint PRD

> **출처**: `docs/prd/5010_v1_fairytale_enemies_v1_prd.md` (디자인) — 옵션 B (3 동화 batch)
> **범위**: V1/Enemy_*.png × 11, 동화별 prompt prefix, baseline 재사용

## 1. 전체 개요

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | RedHood 4 (RedHood / BigWolf / Bear / Basic, 시드 11111+) | 1 | 30분 |
| 2 | Beanstalk 4 (BeanGiant / GoldenGoose / StarCloud + 1, 22221+) | 1 | 30분 |
| 3 | Alice 4 (WhiteRabbit / Cheshire / CardSoldier / HeartQueen, 33331+) | 1 | 30분 |
| 4 | Auditor 검증 + docs | 1 | 15분 |
| **총** | **11 PNG** (V1 11종) | **4** | **약 2시간** |

## 2. Sprint 명세

### Sprint 1 — RedHood 4 (CH 1 동화)

**대상**: `V1/Enemy_RedHood.png` / `Enemy_BigWolf.png` / `Enemy_Bear.png` / `Enemy_Basic.png`

**프로세스**:
- prefix `Red Riding Hood fairy tale, classic storybook illustration, warm forest tones`
- 시드 11111~11114
- 각 캐릭터 prompt = 디자인 §3.2 매핑

**검증 기준**:
- 4 PNG transparent 30~80%, asym ≤ 50, RGB std ≥ 25
- 동화 톤 일관 (warm forest)
- Auditor — V1 카테고리 healthy

#### Sprint 1 commit
```
5010 Sprint 1 / T01 — RedHood 동화 4 sprite 재생성 (1/4)

[변경]
- V1/Enemy_RedHood.png + BigWolf + Bear + Basic 재생성
- prefix Red Riding Hood + classic storybook + warm forest
- 시드 11111~11114, 768x1024 cfg 6 steps 25

[검증]
- 4 PNG healthy + 동화 톤 일관

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

### Sprint 2 — Beanstalk 4 (CH 2 동화)

**대상**: `Enemy_BeanGiant.png` / `Enemy_GoldenGoose.png` / `Enemy_StarCloud.png` + 추가 1
**프로세스**: prefix `Jack and the Beanstalk fairy tale, sky kingdom illustration, golden vibrant tones`, 시드 22221~22224
**검증**: 4 PNG healthy + sky kingdom 톤 일관

### Sprint 3 — Alice 4 (CH 3 동화)

**대상**: `Enemy_WhiteRabbit.png` / `Enemy_Cheshire.png` / `Enemy_CardSoldier.png` / `Enemy_HeartQueen.png`
**프로세스**: prefix `Alice in Wonderland fairy tale, whimsical curious illustration, pastel surreal tones`, 시드 33331~33334
**검증**: 4 PNG healthy + Wonderland 톤 일관, Cheshire "no scary" 검증

### Sprint 4 — Auditor + docs

**작업**:
- Auditor 재실행 → V1 카테고리 11/11 healthy 확인
- changelog_v1.md 5010 항목
- CLAUDE.md §11/§12/§14 갱신

#### Sprint 4 commit
```
5010 Sprint 4 / T04 — Auditor 검증 + docs (4/4)

[검증]
- Auditor V1 11/11 healthy
- changelog + CLAUDE.md 갱신

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 3. 의존성 그래프

```
Sprint 1 → Sprint 2 → Sprint 3 → Sprint 4
```

순차 (사용자 review).

## 4. 검증 시나리오

### S1 — V1 동화 sprite 시각 검토
1. `V1/Enemy_*.png` 11 sprite 시각 확인
- **검증**: 동화 IP 식별 가능 (RedHood / Wolf / Cheshire 등), Cheshire 무섭지 X, 동화별 톤 일관

### S2 — Auditor V1 카테고리
1. `Tools > LittleHero > Audit Enemy Sprite Quality`
- **검증**: report 의 V1 카테고리 11/11 healthy

## 5. Definition of Done

- [ ] 11 V1 sprite 재생성 + 사용자 review PASS
- [ ] 동화별 톤 일관 + Cheshire 친근 톤
- [ ] Auditor V1 healthy 11/11
- [ ] changelog + CLAUDE.md

## 6. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| Cheshire Cat 무서운 톤 | "friendly, no scary, gentle smile" + 시드 변경 |
| 동화 IP 가 SDXL 학습 분포 부족 | 명사 명시 (Red Riding Hood / Cheshire 등 직접 명시) |
| V1 미사용 — 후속 wiring 부담 | 5010 = 자산만, wiring 은 5010.1 후속 |

## 7. 변경 이력

- 2026-05-04: v1 작성.
