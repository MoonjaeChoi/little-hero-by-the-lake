# T01 — LayerDiffuse 워크플로 + 카테고리 prompt 가이드 + fallback 한계

> **레이어**: 스킬 (외부)
> **의존성**: 없음
> **예상 시간**: 50분 / 1 commit
> **PRD 출처**: `5008_enemy_visual_pipeline_v2_v2.md` §2 Sprint 1

## 1. 작업 항목

### 1.1 신규 파일

#### 1.1.1 `.claude/skills/comfyui-image/assets/workflow_layerdiffuse.json`

ComfyUI 워크플로 JSON. DreamShaperXL Turbo + huchenlei/ComfyUI-layerdiffuse Custom Node 사용.

**핵심 노드 구성**:
- CheckpointLoaderSimple (DreamShaperXL Turbo)
- LayerDiffuse Apply (huchenlei) — RGBA native transparency
- KSamplerAdvanced (8 steps, cfg 2.0, dpmpp_sde, karras)
- VAEDecode → SaveImage (PNG RGBA)

**검증**: workflow JSON 유효 + LayerDiffuse 노드 클래스명 매핑 정확

#### 1.1.2 `docs/little_hero/enemy_sprite_prompts_v1.md`

5 카테고리 × 6 archetype = 30 prompt 매핑. 디자인 §3.3 의 카테고리 prefix + archetype 캐릭터 제안 통합.

**구조**:
- 공통 베이스 prompt (V5 sticker tone)
- 카테고리 prefix 5종 (Meta/Nature/Animal/Civ/Fantasy)
- archetype 캐릭터 6종 × 5 카테고리 = 30 매핑
- 시드 매핑 (Meta=22222 / Nature=33333 / Animal=44444 / Civ=55555 / Fantasy=66666)

### 1.2 수정 파일

#### 1.2.1 `.claude/skills/comfyui-image/SKILL.md`

LayerDiffuse 워크플로 사용 가이드 + fallback 한계 명시 (Rebuttal accepted).

**추가 섹션**:
```markdown
## LayerDiffuse 워크플로

### 사전 조건
- ComfyUI 에 huchenlei/ComfyUI-layerdiffuse Custom Node 설치
- URL: https://github.com/huchenlei/ComfyUI-layerdiffuse

### 사용
`/comfyui-image --workflow workflow_layerdiffuse.json --prompt "..."`

### Fallback 한계
LayerDiffuse 노드 미설치 시 기본 워크플로 사용. **한계**:
- DreamShaperXL Turbo는 native alpha 출력 X
- 기본 워크플로는 dark BG 생성 후 flood-fill 후처리 (정확도 ~80%)
- 완전한 RGBA 결과를 원하면 LayerDiffuse 권장
```

#### 1.2.2 `.claude/skills/comfyui-image/references/comfyui_api.md`

LayerDiffuse 노드 명세 추가 — 노드 타입 + 입력/출력 매핑.

## 2. 검증 기준

### 2.1 컴파일
- 스킬 외부 파일 (Unity 컴파일 영향 X)

### 2.2 스킬 동작
- `comfyui-image --workflow workflow_layerdiffuse.json` 인자 정상 인식
- 노드 미설치 시 fallback (기존 워크플로) 정상 동작
- 1 sprite 테스트 (Enemy_Meta_Small) → alpha transparent ratio 30~70%

### 2.3 docs 검증
- `enemy_sprite_prompts_v1.md` 30 매핑 모두 존재
- SKILL.md fallback 한계 명시 검증

## 3. commit 메시지

```
5008 Sprint 1 / T01 — comfyui-image LayerDiffuse 워크플로 + 카테고리 prompt 가이드 (1/6)

[변경]
- .claude/skills/comfyui-image/assets/workflow_layerdiffuse.json 신규
- .claude/skills/comfyui-image/SKILL.md 가이드 추가 (fallback 한계 명시)
- .claude/skills/comfyui-image/references/comfyui_api.md LayerDiffuse 노드 명세
- docs/little_hero/enemy_sprite_prompts_v1.md 신규 (5 카테고리 × 6 archetype = 30 prompt)
- 사전 조건: huchenlei/ComfyUI-layerdiffuse Custom Node 설치

[검증]
- 스킬 정상 동작 (LayerDiffuse + fallback)
- 1 sprite 테스트 alpha 정상
- fallback 한계 (Turbo native alpha 없음 + dark BG flood-fill 정확도 ~80%) 사용자 인지

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 4. 관련 문서

- PRD §2 Sprint 1
- 디자인 §3.3 카테고리별 prompt template
- decisions D1=C (참고 — Auditor 의 alpha 검증 후 prompt 미세 조정 가능)

## 5. 알려진 위험

- **LayerDiffuse 노드 설치 사용자 책임** → SKILL.md URL 명시 + fallback 워크플로 + fallback 한계 명시
- **Turbo native alpha 부재** → fallback 정확도 ~80% 명시 (사용자 기대치 정렬)
- **prompt template 카테고리 톤 불일치 가능** → T03~T05 review 사이클로 보정
