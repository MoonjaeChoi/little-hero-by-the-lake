# Little Hero by the Lake — 문서 인덱스

> 호숫가 작은 영웅 (Unity 6.3, iOS 1차) — 설계·파이프라인·테스트·출시 문서.
>
> **재정리**: 2026-05-02. 카테고리별 디렉토리 분리 (`design/` `pipeline/` `little_hero/` `release/` `testing/`).

---

## 0. 빠른 진입 — 무엇부터 읽을까

| 목적 | 진입 문서 |
|---|---|
| **게임 설계 전체 그림** 이해 | [`design/part_0_overview.md`](./design/part_0_overview.md) → [`design/direction_v2.md`](./design/direction_v2.md) |
| **Little Hero 구체 시스템** (구현 직접 활용) | [`little_hero/README.md`](./little_hero/README.md) |
| **현재 구현 상태** 점검 | [`little_hero/implementation_plan_v1.md`](./little_hero/implementation_plan_v1.md) §0 |
| **iOS 빌드 + TestFlight 출시** | [`release/ios_build_v1.md`](./release/ios_build_v1.md) |
| **Play 모드 시각 검증 가이드** | [`testing/TEST_GUIDE.md`](./testing/TEST_GUIDE.md) |
| **아트 작업 (G2)** | [`pipeline/art_pipeline_v2.md`](./pipeline/art_pipeline_v2.md) |
| **BGM 작업 (G1)** | [`pipeline/bgm_pipeline_v2.md`](./pipeline/bgm_pipeline_v2.md) |

---

## 1. `design/` — 잭팟 ARPG 설계서 (8 Part 시리즈)

> 메이플스토리 깊이 + 디아블로 빌드 다양성 + 탕탕특공대 진행 명확성을 합친 ARPG 설계.
> 모든 후속 결정의 추상 기반. 구체 게임 적용은 `little_hero/` 에서.

### 상위 지침
- [`direction_v2.md`](./design/direction_v2.md) — **2026-04-30 사용자 결정**. 4대 방향성 (모바일 전용 / 캐릭터 XP / 자부심 코어 / 챕터 동조 성장). 충돌 시 최우선.

### 3대 설계 철학
1. **결정론적 진행 위에 잭팟을 쌓는다** — 갈면 무조건 오르는 바닥 + RNG 잭팟.
2. **단일 진실원에서 모든 것을 흐르게 한다** — 서버 권위·단일 쓰기 경로·동일 효과 데이터 형태.
3. **콘텐츠는 데이터로 관리한다** — 운영팀이 코드 배포 없이 메타 회전 가능.

### Part 인덱스 (✦ = direction_v2 반영)

| Part | 제목 | 핵심 내용 |
|---|---|---|
| 0 | [프로젝트 개요](./design/part_0_overview.md) ✦ | 게임 컨셉·타겟·핵심 루프 |
| 1 | [장비 시스템 정체성](./design/part_1_equipment_identity.md) ✦ | 5층 아이템 아키텍처·12슬롯 |
| 1b | [캐릭터 진행 (XP·레벨)](./design/part_1b_character_progression.md) ✦ | 결정론적 성장 축 (장비와 직교) |
| 2 | [아이템 구조와 강화](./design/part_2_item_enhancement.md) ✦ | +0~+25 강화·천장·옵션 리롤 |
| 3 | [경제 시스템](./design/part_3_economy.md) ✦ | 화폐 카탈로그·곡선·정합 |
| 4 | [옵션 라이브러리 (세트·신화)](./design/part_4_options_library.md) ✦ | 신화 ID·발현 조건 |
| 5 | [UI/UX 설계](./design/part_5_ui_ux.md) ✦ | 모바일 카드 6단·정보 밀도 |
| 6 | [시즌제·캐치업](./design/part_6_season_catchup.md) ✦ | 시즌 vs 챕터 직교성 |
| 7 | [시스템 아키텍처](./design/part_7_architecture.md) ✦ | 서버·DB·클라이언트 책임 |
| 8 | [MVP 우선순위](./design/part_8_mvp_roadmap.md) ✦ | 1차 출시 범위 |

> 각 파트는 **5단 구조** — (1) 핵심 결정 (2) 설계 원칙 (3) 구체적 사양 (4) 미해결 질문 (5) 다음 단계.

---

## 2. `little_hero/` — Little Hero 게임 적용 (구현 직접 활용)

> 잭팟 ARPG 설계 + Little Hero 비폭력 키즈 톤 (CLAUDE.md §2) 통합.
> 11 시스템 설계 + 구현 계획 + 시뮬 검증.

→ **[`little_hero/README.md`](./little_hero/README.md)** 인덱스 참조.

핵심 11 시스템:
- equipment / stats / affix_pool / item_drop / character_progression
- chapter_curve / chapter_structure_v2 / economy / endgame / monetization / ui

→ **구현 진척**: [`little_hero/implementation_plan_v1.md`](./little_hero/implementation_plan_v1.md) §0 (Phase A~G, 51 태스크 100%)

---

## 3. `pipeline/` — 콘텐츠 파이프라인

| 문서 | 상태 | 핵심 |
|---|---|---|
| [`art_pipeline_v1_draft.md`](./pipeline/art_pipeline_v1_draft.md) | 초안 (참조) | 도구·라이선스·시간 추정 일반론 |
| [`art_pipeline_v2.md`](./pipeline/art_pipeline_v2.md) | **정식** | RTX 5070 + DreamShaperXL Turbo SFW + Krita |
| [`art_review_v2.md`](./pipeline/art_review_v2.md) | review | Phase G2 P0~P6 일관성 검증 (131장) |
| [`bgm_pipeline_v1_draft.md`](./pipeline/bgm_pipeline_v1_draft.md) | 초안 (참조) | 도구 비교 (시나리오 A/B/C) |
| [`bgm_pipeline_v2.md`](./pipeline/bgm_pipeline_v2.md) | **정식** | RTX 5070 + ACE-Step v1.5 셀프호스팅 |

---

## 4. `release/` — 출시 준비

| 문서 | 핵심 |
|---|---|
| [`release/ios_build_v1.md`](./release/ios_build_v1.md) | iOS 빌드·서명·TestFlight·App Store 심사 10 섹션 가이드 (§9 알려진 후속 이슈 포함) |

---

## 5. `testing/` — 테스트 가이드

| 문서 | 핵심 |
|---|---|
| [`testing/TEST_GUIDE.md`](./testing/TEST_GUIDE.md) | Phase A~F 자동 검증 결과 + Play 모드 시각 검증 흐름 |

---

## 6. 디렉토리 구조 한눈에

```
docs/
├── README.md (이 파일)
├── design/                — 잭팟 ARPG 설계서 (Part 0~8 + direction_v2)
├── pipeline/              — 아트·BGM 콘텐츠 파이프라인
├── little_hero/           — Little Hero 게임 적용 (11 시스템 + 구현 계획 + 시뮬)
├── release/               — iOS 빌드·App Store 출시
└── testing/               — 사용자 테스트 가이드
```

## 7. 메모 — 두 모델의 관계

이 프로젝트는 두 단계로 발전:

1. **초기**: `CLAUDE.md` 의 단순 Auto-Shooter Roguelite (Phase 1·2 — 16 장비·3 챕터)
2. **확장**: 잭팟 ARPG 설계서 도입 → `little_hero/` 11 시스템 설계 → Phase A~G 구현 (210 신규 적·12슬롯·강화 +0~+25·신화·시즌패스)

**현재 상태** (2026-05-02): Phase A~F 51 태스크 100% 완료 + G1·G2·G3·G4 자동 보강 완료. iOS 빌드 직전.

자세한 진척: [`little_hero/implementation_plan_v1.md`](./little_hero/implementation_plan_v1.md) §0 + [`CLAUDE.md`](../CLAUDE.md) §14 변경 이력.
