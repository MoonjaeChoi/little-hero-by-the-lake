# T02: Sprint 2 — ComfyUI 배치 스크립트 + 6 PNG 생성

> **레이어**: Asset
> **의존성**: T01 (prompt 표 + GUID 매핑)
> **예상 시간**: 90~150분 (서버 시간 + 사용자 review iteration)
> **PRD 출처**: v2 §차분 2 (wired 베이스명 통일) + Sprint PRD §3
> **★ 게이트**: 사용자 시각 review 필수

---

## 1. 작업 항목

### 1.1 신규 파일

**경로**: `scripts/5020_starter_hero_batch.py` (5019 답습)

**스크립트 구조**:
```python
HEROES = {
    "HeroWarrior": {
        "name": "시우 / Siu",
        "wired_basename": "Char_DefaultBoy",  # T01 매핑 표 결과
        "idle":    {"seed": 50200, "out": "Generated_v2/Characters/Char_DefaultBoy.png",
                    "prompt": "...young boy hero named Siu, light leather chest plate..."},
        "psb_ref": {"seed": 50210, "out": "References/Characters_PSB/Char_HeroWarrior_psb_ref.png",
                    "prompt": "...young boy hero T-pose, body parts clearly separated..."},
    },
    "HeroArcher": { ... wired_basename: "Char_BraveGirl" ... },
    "HeroMage":   { ... wired_basename: "Char_QuietFriend" ... },
}

def main(target=None, kind=None):
    # CLI: python3 5020_starter_hero_batch.py [hero_id] [kind]
    # 단일 retry: HeroArcher idle
    ...
```

**예상 라인**: ~150 라인 (Python, 5019 답습 — 100줄 룰 적용 X)

### 1.2 신규 폴더

**경로**: `LittleHero/Assets/_Project/Art/References/Characters_PSB/`

**작업**: 폴더 생성 후 `.gitkeep` (Editor/ 이동은 T05 에서 처리).

### 1.3 ComfyUI 실행

```bash
# SSH 터널 활성화 확인 (192.168.75.194:8188 → localhost:8188)
python3 scripts/5020_starter_hero_batch.py
```

**출력**:
- `Char_DefaultBoy.png` (wired 베이스명, idle, 시우 V5)
- `Char_BraveGirl.png` (wired 베이스명, idle, 가영 V5)
- `Char_QuietFriend.png` (wired 베이스명, idle, 연우 V5)
- `References/Characters_PSB/Char_HeroWarrior_psb_ref.png` (T-pose)
- `References/Characters_PSB/Char_HeroArcher_psb_ref.png` (T-pose)
- `References/Characters_PSB/Char_HeroMage_psb_ref.png` (T-pose)

### 1.4 사용자 시각 review iteration (★ 게이트)

5011 답습 패턴:
1. 1차 6장 생성 → 사용자 시각 review
2. 피드백 (예 "어른 톤", "직업 식별 약함", "비율 깨짐") → prompt 보강 + retry
3. healthy 6/6 도달 시 종결
4. 부분 retry CLI: `python3 scripts/5020_starter_hero_batch.py HeroArcher idle`

---

## 2. 검증 기준

### 2.1 자동 검증
- [ ] 6 PNG 파일 모두 생성됨
- [ ] 각 PNG 768 × 1024 RGBA (transparent background)
- [ ] PNG 만 덮어쓰기 (`.meta` 변경 X — `git status` 에 .meta D 또는 M 0건)

### 2.2 시각 review (사용자 게이트)
- [ ] 직업별 무기 식별 가능 (검·활·지팡이)
- [ ] 어린이 비율 (3-head tall) 일관
- [ ] 단일 캐릭터 (multiple/twin 없음)
- [ ] PG-12 톤 (피·잔인 X, 따뜻한 어린이 친화)
- [ ] psb_ref 3장: T-pose 또는 명확한 부위 분리
- [ ] **사용자 OK** ✅

---

## 3. commit 메시지

```
5020 Sprint 2 / T02 — ComfyUI 6 PNG (idle 3 + psb_ref 3) (2/7)

[변경]
  + scripts/5020_starter_hero_batch.py (~150L Python, 5019 답습)
  + Generated_v2/Characters/Char_DefaultBoy.png    (시우, idle, wired 베이스명 덮어쓰기)
  + Generated_v2/Characters/Char_BraveGirl.png     (가영, idle, wired 베이스명 덮어쓰기)
  + Generated_v2/Characters/Char_QuietFriend.png   (연우, idle, wired 베이스명 덮어쓰기)
  + References/Characters_PSB/Char_HeroWarrior_psb_ref.png (시우 T-pose)
  + References/Characters_PSB/Char_HeroArcher_psb_ref.png  (가영 T-pose)
  + References/Characters_PSB/Char_HeroMage_psb_ref.png    (연우 T-pose)
  - .meta 변경 0 (GUID 보존, R1)

[검증]
  - 6/6 healthy (768×1024 RGBA, 직업 정체성, PG-12 톤)
  - 사용자 시각 review 승인
  - psb_ref T-pose Phase B (5022) 출발 자료 OK

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 4. 관련 문서

- T01 5020_png_guid_map.md (wired 베이스명)
- T01 5020_prompt_table.md (6 prompt)
- 디자인 §4.1.1~4.1.3 (직업 prompt)
- 5019 batch 패턴 (`scripts/5019_chapter_narrative_batch.py`)

---

## 5. 알려진 위험

| 위험 | 완화 |
|---|---|
| ComfyUI 어린이 비율 흔들림 | seed 고정 + retry, 또는 5014 V5 LoRA 보조 |
| 직업 식별 약함 (검 작음 / 활 안 보임) | NEGATIVE 에 "no weapon hidden behind body" 추가 |
| 사용자 review iteration 길어짐 | 부분 retry CLI 활용 (`HeroArcher idle` 단일) |
| ComfyUI 서버 (192.168.75.194:8188) 미가동 | comfyui-image 스킬로 서버 시작 가능 |
