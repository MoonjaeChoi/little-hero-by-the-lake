# 5010 — V1 11종 동화 적 재생성 v1 (디자인)

> **목적**: V1 동화 적 11 sprite (Chapter_01 RedHood / Chapter_02 Beanstalk / Chapter_03 Alice 등) 5008 baseline 으로 재생성.
> **우선순위**: 낮음 (legacy V1 은 V2 시스템 archetype 으로 대체됨, 도감/추억 회고 용)

## 0. 결정 이력

- 2026-05-04: 5008 종결 시 후속 anchor 등록 (이전 5008.2 → 재부여 5010)

## 1. 현재 시스템 분석

### 1.1 V1 11종 분포

| 위치 | 캐릭터 | 동화 |
|---|---|---|
| `V1/Enemy_Basic.png` | Slime | (공통) |
| `V1/Enemy_BeanGiant.png` | Bean Giant | Beanstalk (CH 2) |
| `V1/Enemy_BigWolf.png` | Big Wolf | Red Hood (CH 1) |
| `V1/Enemy_Bear.png` | Bear | (공통) |
| `V1/Enemy_StarCloud.png` | Star Cloud | Beanstalk |
| `V1/Enemy_CardSoldier.png` | Card Soldier | Alice (CH 3) |
| `V1/Enemy_HeartQueen.png` | Heart Queen | Alice |
| `V1/Enemy_RedHood.png` | Red Hood | Red Hood |
| `V1/Enemy_GoldenGoose.png` | Golden Goose | Beanstalk |
| `V1/Enemy_WhiteRabbit.png` | White Rabbit | Alice |
| `V1/Enemy_Cheshire.png` | Cheshire Cat | Alice |
| **합계** | **11** | |

### 1.2 활용

- V2 시스템 (PhaseFContentSeed) 미사용 — archetype 30 만 사용
- 도감 + 추억 시스템 의 회고 sprite 로 잠재적 활용 (currently 미연결)
- Auditor 결과 V1 9 healthy / 2 damaged (Enemy_StarCloud + 1 추가)

### 1.3 prompt 특수성

V1 은 동화 캐릭터 (RedHood / Beanstalk / Alice) — 명확한 캐릭터 IP. 일반 V5 sticker tone 보다 **동화책 illustration 톤** 이 적합:
- "fairy tale storybook illustration, classic children's book style"
- 카테고리 prefix 없음 (각 캐릭터 고유 톤)

## 2. 옵션 비교

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| A | 11 PNG 일괄 재생성 (1 commit) | 단순 | review 11 sprite 한 번에 부담 |
| **B (★권장)** | **3 batch (RedHood 4 / Beanstalk 4 / Alice 3)** | 동화별 review | 3 commit |
| C | 손상 2 PNG 만 재생성 | 최소 작업 | 9 PNG 구식 톤 유지 |

> 권장 **B** — 동화별 톤 일관성 + 사용자 review 단위 명확.

## 3. 권장 설계

### 3.1 동화별 prompt prefix

| 동화 | prefix | 시드 |
|---|---|---|
| Red Hood (CH 1) | `Red Riding Hood fairy tale, classic storybook illustration, warm forest tones` | 11111 |
| Beanstalk (CH 2) | `Jack and the Beanstalk fairy tale, sky kingdom illustration, golden vibrant tones` | 22221 |
| Alice (CH 3) | `Alice in Wonderland fairy tale, whimsical curious illustration, pastel surreal tones` | 33331 |

### 3.2 캐릭터별 prompt

```
RedHood: "Red Riding Hood character, young girl with red cloak, basket in hand"
BigWolf: "big bad wolf character, friendly cartoon wolf, no fear no menace"
BeanGiant: "Bean Giant character, friendly giant in cloud kingdom"
GoldenGoose: "Golden Goose character, plump golden goose, gentle look"
StarCloud: "Star Cloud character, fluffy cloud creature with star eyes"
WhiteRabbit: "White Rabbit character, cute rabbit in vest with pocket watch"
Cheshire: "Cheshire Cat character, smiling cat with purple stripes, no scary"
CardSoldier: "Card Soldier character, playing card knight, friendly pose"
HeartQueen: "Heart Queen character, friendly cartoon queen with heart crown"
Bear: "friendly bear character, big round body, wave paw"
Basic: "small slime creature, blob shape, gentle smile"
```

### 3.3 검증된 baseline 재사용

- 768×1024 portrait, cfg 6, steps 25, euler/normal
- Negative 17종 grid + 6종 인물 수
- "no fear no menace, no scary" 추가 (PG-12 정책 강화)

## 4. Sprint 분해

| Sprint | 작업 | commit |
|---|---|---|
| 1 | RedHood 4 (RedHood / BigWolf / Bear / Basic) | 1 |
| 2 | Beanstalk 4 (BeanGiant / GoldenGoose / StarCloud + extra) | 1 |
| 3 | Alice 3 (WhiteRabbit / Cheshire / CardSoldier / HeartQueen — 4 실제) | 1 |
| 4 | Auditor 검증 + docs | 1 |
| **총** | **11 PNG** | **4** |

## 5. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| 동화 IP 가 SDXL 생성 시 캐릭터 형태 너무 다양 | 시드 고정 + 명확한 캐릭터 명사 (Red Riding Hood / Cheshire 등) |
| Cheshire Cat 가 무서운 톤 가능 | "no scary, friendly" + cfg 6.0 |
| V1 사용처 미정 (도감/추억) | 5010 종결 후 V1 sprite wiring 별도 사이클 (future) |

## 6. 변경 이력

- 2026-05-04: v1 작성.
