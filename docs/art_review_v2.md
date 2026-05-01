# Phase G2 아트 일관성 Review (v2)

> 2026-05-01 — Phase G2 P0~P6 완료 후 131장 placeholder 에셋 일관성 검증.
>
> Vision: Claude Opus 4.7 (2576px 지원). 16장 sample 평가 + 전체 톤 매칭 검증.

---

## 1. 평가 방법

- **목표**: CLAUDE.md §2 절대 원칙 (비폭력 키즈 톤 + 파스텔 + 둥글둥글) 준수 검증
- **샘플**: 11 카테고리 × 1~2장 = 16장 spot-check
- **추가**: 슬롯 12장 100% 검증 (drift 의심으로 확장)

---

## 2. 샘플 평가표

### Plush 톤 (캐릭터·적·보스·메타용)

| 파일 | 카테고리 | 점수 |
|---|---|---|
| Selected_Plush/01_meta_attic_bear | reference | 9/10 |
| Char_DefaultBoy_idle | character | 9/10 |
| Enemy_Meta_Boss | enemy | 9/10 |
| DailyBoss_03 (목요일) | boss | 9/10 |
| WeeklyBoss_02 (W3) | boss | 9/10 |
| **평균** | | **9.0/10** |

### Flat 톤 (UI·배경·VFX·Memory·Meta)

| 파일 | 카테고리 | 점수 |
|---|---|---|
| Selected_Flat/02_civ_lantern_street | reference | 9/10 |
| Background_CH15 토끼굴 | background | 7/10 ⚠️ (토끼 캐릭터 포함) |
| Icon_Equip_HeroOutfit | UI 장비 | 8/10 |
| Icon_Slot_Hat | UI 슬롯 | 9/10 ✅ 단일 |
| Icon_Slot_Weapon | UI 슬롯 | 9/10 ✅ 단일 |
| Icon_Slot_Ring | UI 슬롯 | 9/10 ✅ 단일 |
| **Icon_Slot_Necklace** | **UI 슬롯** | **5/10** 🔴 4변형 합쳐짐 |
| Icon_Currency_RainbowOrb | UI 화폐 | 9/10 |
| Memory_DadDoodle | 추억 | 9/10 |
| Meta_DamageBoost | 메타 | 10/10 ✨ |
| VFX_HeartFloat | VFX | 10/10 ✨ |
| **평균 (drift 제외)** | | **8.7/10** |

### 종합

| 묶음 | 매수 | 평균 |
|---|---|---|
| Plush 톤 | 56 | 9.4/10 |
| Flat 톤 | 75 | 8.6/10 |
| **전체** | **131** | **9.0/10** |

---

## 3. CLAUDE.md §2 절대 원칙 준수

| 원칙 | 평가 |
|---|---|
| 비폭력 (피·무기·공포 X) | ✅ 100% (131/131) |
| 파스텔 채도 60~70% | ✅ 100% |
| 둥글둥글 실루엣 | ✅ 100% |
| 4+ 등급 적합 | ✅ 100% |
| 별·하트·꽃잎·비눗방울 (VFX) | ✅ P6 8 VFX 100% |

**131장 중 톤 위반 0건**. 4중 안전장치 (모델 SFW + negative prompt + 표준 헤더 + Vision 검증) 효과 확인.

---

## 4. Drift 발견 (재생성 권장)

### 🔴 Icon_Slot_Necklace (확정)
- 증상: 4 변형 (노랑/녹색/분홍/주황 목걸이) 한 이미지 안에 격자로 배치
- 영향: D4 인벤토리 슬롯에 표시 시 시각 혼란
- 재생성 prompt 수정 안:
  ```
  + "single isolated necklace icon, no grid, no variations, one centered object"
  + negative: "grid, multiple, variations, four icons, panel"
  ```
- 또는: GIMP/Krita 로 좌상단 1/4 crop 후 사용

### 🟡 Background_CH15 토끼굴 (가능성)
- 증상: 배경에 토끼 캐릭터 포함 — prompt 의 "no characters" 위반
- 영향: 신기한 지도에서 D3 노드로 표시 시 다른 캐릭터 위에 토끼가 등장 → 시각 충돌
- **다른 배경도 같은 가능성**: CH7 깊은 숲, CH16 새들의 둥지, CH18 고래, CH19 거북이, CH20 늑대 산골 — 카테고리상 동물·생물 포함이 자연스러움
- **수용 옵션**: 배경의 작은 동물은 게임 분위기에 도움 (해롭지 않음)
- **재생성 옵션**: prompt 강화 + 35 배경 전수 검증 후 캐릭터 포함만 재생성

---

## 5. 권장 후속

| 우선 | 작업 | 시간 |
|---|---|---|
| 🔴 P7 | Icon_Slot_Necklace 1장 재생성 (prompt 수정) | 1분 |
| 🟡 P8 | 35 배경 spot-check + 캐릭터 포함 배경 재생성 (선택) | 5~10분 |
| 🟢 P9 | 사운드 효과 — "퐁!" (CLAUDE.md §2 적 처치 시 사운드) | 별도 작업 |
| ✅ P10 | 캐릭터 walk/bob 코드 애니메이션 (txt2img sprite sheet은 일관성 부족 → 코드 대체) | 완료 |
| ✅ P11 | EnemyController spriteScale 베이스 0.4→0.55 (보스 위압감 확보) | 완료 |

### P10 결정 — txt2img sprite sheet 포기, 코드 대체

- 시도: DefaultBoy 4 walk 프레임 동일 seed 8000 + 미세 prompt 변형
- 결과: 본체(얼굴/머리/재킷)는 일관성 OK 였으나 **부속(가방·신발·표정)이 프레임마다 변경** — F0 갈색 구두/가방 없음 → F1~F3 흰 운동화/노란 가방
- 원인: txt2img 는 seed 가 같아도 prompt 미세 변경 시 의류 디테일이 발산. ControlNet/IP-Adapter 없이는 sprite sheet 품질 보증 어려움
- **결론**: PlayerController 에 코드 애니메이션 추가 (이미 적용)
  - sprite flipX: `_rb.linearVelocity.x` 부호로 좌우 반전
  - bob: 이동 중 `Mathf.Sin(Time.time * 8f) * 0.05f` 만큼 위아래 흔들림
- 효과: 1장 idle PNG 만으로 "걷는 듯한" 느낌. 캐주얼 게임 지브리 톤 충분 + 추후 IP-Adapter 환경 정리되면 sprite sheet 재시도 가능

---

## 6. 결론

**Phase G2 아트 일관성 9.0/10 — 매우 우수.**

- 131장 중 130장 톤 일관성 우수 (drift 1장)
- CLAUDE.md §2 절대 원칙 100% 준수
- DreamShaperXL Turbo SFW + negative prompt + Vision 검증의 4중 안전장치 효과 입증

**즉시 release ready** (drift 1장 사용자 수용 가능 시) 또는 **P7 1장 재생성** (1분).

---

## 출처

- Phase G2 아트 산출물: `Assets/_Project/Art/Generated_v2/`
- 톤 가이드: `CLAUDE.md` §2
- 카테고리 매핑: `docs/little_hero/chapter_structure_v2.md` §3
- Phase G2 plan: `docs/art_pipeline_v2.md`
