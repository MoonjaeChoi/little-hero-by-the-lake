# 5011 — Daily 7 + Weekly 4 보스 sprite 재생성 v1 (디자인)

> **목적**: Endgame 의 Daily 7 + Weekly 4 = 11 보스 sprite 를 5008 baseline 으로 재생성.
> **우선순위**: 보통 (endgame 진입 시 명확한 시각 차별성 ↑).

## 0. 결정 이력

- 2026-05-04: 5008 종결 시 후속 anchor 등록 (이전 5008.3 → 재부여 5011)

## 1. 현재 시스템 분석

### 1.1 Endgame 보스 분포

| 위치 | 카테고리 | 개수 | 출처 |
|---|---|---:|---|
| `Generated_v2/Enemies/DailyBoss_{00~06}.png` | Daily | 7 | 요일 회전 (월화수목금토일) |
| `Generated_v2/Enemies/WeeklyBoss_{00~03}.png` | Weekly | 4 | 4주 사이클 |
| **합계** | | **11** | |

### 1.2 Endgame 시스템

- F4 EndgameModeData (daily_boss / weekly_boss) — bossPool 11 EnemyData
- BossEncounterRunner (Endgame 모드) — Daily/Weekly 보스 출현
- BossLootTable (5001 boss_system_v2): Daily mythicChance 0.5 / Weekly mythicChance 1.0

### 1.3 시각 요구사항 (Endgame 차별성)

**Daily 보스 (요일 톤)**:
- 월: 푸른 새벽 / 화: 빨강 활기 / 수: 녹색 자연 / 목: 자주색 신비 / 금: 분홍 따뜻 / 토: 노랑 활발 / 일: 황금 휴식

**Weekly 보스 (4주 cycle)**:
- 1주: 빛 (white/gold)
- 2주: 그림자 (deep purple)
- 3주: 생명 (green vibrant)
- 4주: 시간 (blue cosmic)

**보스 sticker 요구**:
- 일반 archetype 보다 **larger character + impressive aura**
- "epic boss creature, large impressive character, royal pose"
- 다만 PG-12 — 무서움/위협 X, 카리스마 + 친근

## 2. 옵션 비교

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| A | 11 PNG 일괄 (1 commit) | 단순 | review 부담 |
| **B (★권장)** | **2 batch (Daily 7 + Weekly 4)** | review 명확 분리 | 2 commit |
| C | 사용자 결정 후 즉시 (보류) | 부담 X | endgame 시각 통일 지연 |

> 권장 **B** — Daily/Weekly 톤 차이 명확.

## 3. 권장 설계

### 3.1 Daily 7 prompt template

공통 베이스 + 요일별 톤 + 보스 강조:
```
solo, 1character, alone, single subject, full body portrait, large impressive character filling frame,
royal pose, epic boss creature, friendly powerful aura, no menace,
cute cartoon sticker illustration, V5 dynamic tone, soft cartoon shading,
chunky thick white outline, three-quarter view, centered, fully transparent background,
isolated subject
```

요일 prefix:
- 월요일 (00): `Monday morning blue boss, dawn fresh aurora, calm awakening tone`
- 화요일 (01): `Tuesday fiery red boss, vibrant energetic spark, motivated tone`
- 수요일 (02): `Wednesday verdant green boss, nature growth fresh leaves`
- 목요일 (03): `Thursday royal purple boss, mystical wisdom flowing robe`
- 금요일 (04): `Friday warm pink boss, gentle joy soft glow`
- 토요일 (05): `Saturday bright yellow boss, sunny playful round`
- 일요일 (06): `Sunday golden boss, peaceful rest grand crown`

### 3.2 Weekly 4 prompt template

베이스 동일 + 4주 cycle:
- 0 (1주): `Light Realm Weekly Boss, radiant white-gold guardian, divine grand wings`
- 1 (2주): `Shadow Realm Weekly Boss, deep purple sentinel, mysterious cape no scary`
- 2 (3주): `Life Realm Weekly Boss, vibrant green forest king, antlers branching`
- 3 (4주): `Cosmic Realm Weekly Boss, deep blue starscape lord, galaxy crown`

### 3.3 시드

- Daily: 99000~99006 (요일별 +1)
- Weekly: 99100~99103 (cycle별 +1)

## 4. Sprint 분해

| Sprint | 작업 | commit |
|---|---|---|
| 1 | Daily 7 (DailyBoss_00~06) | 1 |
| 2 | Weekly 4 (WeeklyBoss_00~03) | 1 |
| 3 | Auditor + docs | 1 |
| **총** | **11 PNG** | **3** |

## 5. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| 보스 sticker 가 너무 크거나 frame 잘림 | "filling frame" 강조 + 768×1024 portrait 검증된 사이즈 |
| 요일별 톤 7 변형이 너무 미세 | 시드 + prefix 명확히 분리 |
| Weekly 보스 mythic 보장 (1.0) — 시각도 epic | "epic / divine / cosmic" 키워드 강조 |

## 6. 변경 이력

- 2026-05-04: v1 작성.
