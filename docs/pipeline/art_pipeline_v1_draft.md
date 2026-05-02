# Little Hero — 아트 파이프라인 초안 (Claude Code + AI 통합)

> ⚠️ **본 문서는 초안(draft).** 실제 도입 전 도구 라이선스·생성물 톤·시간 추정 검증 필요.
>
> 2026-05-01 조사 — Claude Code (Opus 4.7) 와 함께 Phase G2 "아트 스프라이트 (단색 더미 → 실 에셋 교체)" 작업을 효과적으로 진행하기 위한 파이프라인 안.
>
> 본 게임 컨텍스트: Unity 6.3 LTS / URP 2D / 비폭력 키즈 톤 / 파스텔 60~70% 채도 / 둥글둥글 실루엣 / 별·하트·꽃잎·비눗방울 파티클 / 4+ 등급 (CLAUDE.md §2).

---

## 1. 핵심 발견 — Claude가 직접 아트 도구 제어 (MCP)

### 🥇 Aseprite MCP Pro (1순위 추천)
**자연어로 Aseprite 직접 제어.** 121개 도구 — 스프라이트 생성 / 4프레임 워크사이클 / 팔레트 관리 / 디더링 / 안티앨리어싱 규칙 / Godot·Unity export. **AI가 픽셀아트 규칙을 "따르도록" 강제하는 skill guide 내장** (이게 핵심: 단순 그리기가 아니라 "올바른 픽셀아트").

```
"Create a 32x32 Game Boy sprite of a character"
"Add 4 frames for a walk cycle"
```

### 🥈 pixel-plugin (willibrandon, Claude Code 전용)
Claude Code 공식 플러그인. **Unity JSON 메타데이터 export 지원** — 스프라이트시트 생성 시 바로 Unity Sprite Editor 에서 import 가능. 본 프로젝트에 직접 호환.

### 🥉 ComfyUI MCP — Stable Diffusion 통합
4종 구현체 존재 (Peleke / SamuraiBuddha / MatthewSnow2 / Artokun). **Comfy Cloud MCP** 사용 시 로컬 GPU 불필요 (`https://cloud.comfy.org/mcp`). ControlNet·IP-Adapter·인페인팅·아웃페인팅 모두 자연어로.

배경·UI 텍스처·일러스트레이션에 적합. 픽셀아트는 **RetroDiffusion 모델** 로드.

---

## 2. Claude의 직접 능력

### Vision API (Opus 4.7)
- **최대 2576px (long edge)** — 이전 1568px 대비 향상. 고해상도 스프라이트시트 분석 가능.
- 다중 이미지 동시 비교 — 41 archetype 적의 일관성 검사에 직접 활용.
- 형식: PNG/JPEG/GIF/WebP

**활용**: 생성된 에셋의 톤·팔레트·실루엣 일관성을 Claude가 직접 review.

### Code 생성 측면
- ScriptableObject 자동 생성 (이미 본 프로젝트에서 활용 중)
- 셰이더·VFX·파티클 시스템 코드
- Tilemap 자동 배치 스크립트

---

## 3. 보완 도구 (Claude 외부)

| 도구 | 역할 | 본 프로젝트 적합도 |
|---|---|---|
| **Retro Diffusion** (Aseprite 확장) | 픽셀아트 생성 모델, 인앱 직접 | 높음 (Aseprite 흐름과 통합) |
| **PixelLab** | 브라우저·Aseprite 플러그인 | 중간 |
| **Ludo.ai** | 정적 이미지 → 애니 스프라이트시트 | 높음 (Player/Enemy 애니 추가 변환) |
| **Layer.ai** | 크리에이티브 팀 AI OS, 일관성 유지 | 중간 (1인 개발이라 오버킬 가능) |
| **Makko** | AI 통합 2D 게임 스튜디오 (4만+ 에셋 베타) | 검토 가치 |

---

## 4. 본 프로젝트(Little Hero) 추천 파이프라인

### 단계 A — 톤 잠금 (Day 1, 2~4시간)
1. Claude로 **무드보드 텍스트 사양** 작성 — "파스텔 60~70%, 둥글둥글, 별·하트·꽃잎, 비폭력, 4+ 등급, 채도 sweet spot"
2. **고정 팔레트 16색** 생성 — Aseprite Color Palette 로 export, 모든 후속 에셋이 이 팔레트로 quantize
3. 핵심 reference 5종 그림: Player (소년) / Enemy_Meta_Small (꼬마 슬라임) / Enemy_Nature_Boss / Boss / 챕터 1 배경

### 단계 B — 핵심 캐릭터 (Day 2~3)
- pixel-plugin 또는 Aseprite MCP Pro 설치
- 자연어로 4 코어 SO 생성:
  - 3 캐릭터 SO (용감한 소년 / 씩씩한 소녀 / 조용한 친구) — 각 idle + walk 4프레임
  - 본 게임은 Enemy_Basic 등 11 기존 SO + 41 신규 archetype = 52종
  - **5 카테고리(Meta/Nature/Animal/Civ/Fantasy) × 6 archetype = 30 generic** 만 우선 생성

### 단계 C — 일관성 검사 (Claude Vision)
- 생성된 30 archetype을 Claude Opus 4.7에 일괄 입력
- "이 30종 중 톤·실루엣·팔레트가 다른 항목 식별. 비폭력 키즈 톤 위반 없는지 검사" 프롬프트
- drift 발견 시 해당 항목만 재생성

### 단계 D — UI 아이콘 (Day 4)
- ComfyUI MCP — 16 장비 + 12 슬롯 + 4 화폐 = 32 아이콘
- 또는 Aseprite MCP 로 픽셀아트 아이콘 일괄
- 본 게임은 이미 `Generated/Icon_Equip_*.png` 16종 placeholder 존재

### 단계 E — 챕터 배경 (Day 5~7)
- ComfyUI MCP + RetroDiffusion 모델 (또는 일반 SDXL)
- 5 카테고리 × 5~7 챕터 = 35 배경
- Negative prompt: `blood, gore, dark, scary, weapons, violence, sharp objects` 강제

### 단계 F — VFX 파티클 (Day 8)
본 게임 절대 원칙: 별·하트·꽃잎·비눗방울. 
- Aseprite로 8프레임 파티클 시트 직접 생성 (작아서 Claude 단독 생성 가능)
- Unity URP 2D Particle Renderer 셰이더 코드는 Claude Code가 직접

### 단계 G — Ludo.ai 애니메이션 보강 (선택)
정적 캐릭터 이미지 → 4프레임 walk·idle·attack 자동 변환

---

## 5. 비폭력 키즈 톤 — AI 활용의 함정

본 프로젝트의 톤이 **AI 학습 데이터 분포와 어긋남**:
- 대부분 모델이 액션·다크 게임 학습 → 기본 출력이 폭력적
- "blood splash" 류 효과가 default로 나오는 경향

**대응**:
1. **Negative prompt 강제 라이브러리화** — 모든 생성 호출에 표준 negative prompt 자동 첨부
2. **Vision 자동 review** — 생성 직후 Claude Vision으로 "blood/dark/scary 요소 검출" 1차 필터
3. **Reference image 매번 첨부** — 한 번 톤 잡힌 reference를 IP-Adapter로 모든 생성에 입혀서 drift 차단
4. **CLAUDE.md §2 절대 원칙 인용** — Claude Code 호출 시 매 프롬프트 헤더에 톤 규칙 명시

### 표준 Negative Prompt (초안)

```
blood, gore, violence, weapons, sharp objects, scary, dark, horror,
realistic, photorealistic, gritty, edgy, mature, adult,
muscular, sexy, bikini, exposed skin, blood splatter,
death, skull, demon, evil, sinister, menacing,
high saturation, neon, oversaturated, harsh shadows
```

### 표준 Positive Prompt 헤더 (초안)

```
pastel colors saturation 60-70%, soft rounded silhouettes,
cute kawaii, children-friendly age 4+, Studio Ghibli inspired,
warm gentle palette, soft outlines, plush toy aesthetic,
non-violent, sparkles and hearts and flower petals
```

---

## 6. 산업 동향 (2026 기준)

- **95% 게임 스튜디오가 AI 워크플로우 도입** (2026 Unity Game Development Report)
- **"vibe coding"** — concept → impl 즉시
- 인디 트렌드: AI를 **collaborator** 로 (대체자 X)
- 가장 큰 함정: 시리즈 간 drift — 한 도구를 깊게 익히는 게 여러 도구 얕게보다 효과적

---

## 7. 본 프로젝트 적용 권장 순서 (우선순위)

| 우선 | 작업 | 도구 | 예상 시간 |
|---|---|---|---|
| 🔴 | Aseprite MCP Pro 또는 pixel-plugin 설치 | Claude Code 플러그인 | 30분 |
| 🔴 | 톤 잠금 (팔레트 + reference 5종) | Aseprite + Claude 무드보드 | 4시간 |
| 🟡 | 30 archetype 적 일괄 생성 + Vision review | pixel-plugin + Vision | 1~2일 |
| 🟡 | 3 캐릭터 idle/walk 애니 | pixel-plugin or Ludo.ai | 1일 |
| 🟠 | 16 장비 + 4 화폐 아이콘 | Aseprite MCP | 1일 |
| 🟠 | 35 챕터 배경 | ComfyUI MCP + SDXL/Retro | 2~3일 |
| 🟢 | 비폭력 VFX 파티클 8종 | Aseprite + Claude shader | 1일 |
| 🟢 | UI 패널 시각 디테일 | Aseprite MCP | 2~3일 |

총 **약 9~12일** (1인 풀타임 기준) — 기존 Phase G2 "매우 복잡" 평가를 AI 활용 시 1~2주로 압축 가능.

---

## 8. 본 프로젝트 현재 placeholder 상태 (기준선)

Phase F 완료 시점 보유 에셋:

| 카테고리 | placeholder 수 | 실 에셋 필요 |
|---|---|---|
| 적 archetype (5 카테고리 × 6 역할) | 30 | 30+ |
| 일일 보스 | 7 | 7 |
| 주간 보스 | 4 | 4 |
| 기존 11 적 (Phase 1·2 동화책 모델) | 11 | 보존·재활용 |
| 캐릭터 | 3 (단색 원) | 3 (idle/walk/attack) |
| 장비 아이콘 | 16 (Generated PNG) | 16 (재생성 권장) |
| 추억 픽업 | 4 (단색 별) | 4 |
| 챕터 35 V2 배경 | 0 | 35 |
| 챕터 36~50 hardcore 배경 | 0 (V2 재활용 예정) | 0 |
| UI 아이콘 (장비 슬롯 12 + 화폐 4) | 0 | 16 |
| VFX 파티클 (별/하트/꽃잎/비눗방울/+@) | 0 | 8~12 |
| **합계 추정** | — | **약 130~150 unique 에셋** |

---

## 9. 미해결 / 결정 필요 항목

- **픽셀아트 vs 벡터/일러스트** — 본 게임 톤(둥글둥글 파스텔)은 픽셀아트보다 **벡터·플랫 일러스트** 가 더 자연스러울 수 있음. 결정 후 도구 선택 (Aseprite=픽셀, ComfyUI=일러스트).
- **해상도 가이드** — iOS Retina 대응 (2x/3x 자산). 캐릭터 64x64? 128x128? 정해야 일괄 생성 가능.
- **애니메이션 프레임 수** — idle 2프레임 / walk 4프레임 / attack 6프레임 (표준)
- **Aseprite 라이센스** — $19.99 1회 구매 (또는 무료 빌드 직접 컴파일)
- **ComfyUI Cloud 가격** — Comfy Cloud 종량 vs 로컬 GPU 트레이드오프 (현재 환경에 GPU 있는지)
- **Retro Diffusion 모델 라이센스** — astropulse.itch.io 유료 ($X)
- **Ludo.ai 가격** — 무료 tier 한도 + 유료 플랜
- **저작권 / 상업 이용권** — 각 도구의 생성물 상업 이용 약관 확인 필수 (특히 SD 모델별 라이센스 분리)

---

## 10. 사용자 승인 필요 사항

본 초안 진행 전 사용자 결정 필요:

1. **아트 스타일 확정**: 픽셀 / 벡터 일러스트 / 하이브리드 중 선택
2. **유료 도구 도입 여부**: Aseprite / Retro Diffusion / Ludo.ai 등
3. **MCP 추가 설치 동의**: Aseprite MCP Pro / pixel-plugin / ComfyUI MCP 중 우선순위
4. **현재 placeholder를 즉시 교체할지 단계적으로 교체할지**: "출시 전 일괄" vs "Phase G 단계적"
5. **시간 투자 우선순위**: 캐릭터 우선 / 적 우선 / 배경 우선 (사용자 선호)

---

## 11. 출처

조사 일자: 2026-05-01

### MCP / 도구
- [Aseprite MCP Pro — 121 AI Tools for Pixel Art Creation](https://aseprite-mcp.abyo.net/)
- [pixel-plugin (Claude Code Aseprite 플러그인)](https://github.com/willibrandon/pixel-plugin)
- [ComfyUI MCP (artokun)](https://github.com/artokun/comfyui-mcp)
- [ComfyUI MCP (Peleke)](https://github.com/Peleke/comfyui-mcp)
- [ComfyUI Cloud MCP Server (공식)](https://docs.comfy.org/development/cloud/mcp-server)

### Claude API
- [Claude Vision API Docs](https://platform.claude.com/docs/en/build-with-claude/vision)

### 이미지 생성 (Aseprite 확장 + 외부)
- [Retro Diffusion (Aseprite extension)](https://astropulse.itch.io/retrodiffusion)
- [PixelLab (Aseprite + 브라우저)](https://www.pixellab.ai/)
- [Ludo.ai — 정적→애니 스프라이트시트](https://ludo.ai/blog/introducing-ludo-ai-api-mcp-integration)

### 가이드 / 분석
- [Accelerate Unity Game Development with Claude Code](https://claudelab.net/en/articles/claude-code/claude-code-unity-game-development-guide)
- [Claude + Gemini Game Art Workflow (YouTube)](https://www.youtube.com/watch?v=9XsX7CFljkk)
- [Using Claude AI in Game Development — Industry Statistics](https://kevurugames.com/blog/using-claude-ai-in-game-development-tools-use-cases-and-industry-statistics/)
- [How to Create 2D Game Art in 2026](https://kevurugames.com/blog/how-to-create-2d-game-art-everything-you-need-to-know/)
- [Top Game Art Trends 2026 (RetroStyle)](https://retrostylegames.com/blog/top-game-art-trends-2026/)
- [Game Art Trends 2026 (VSQUAD)](https://vsquad.art/blog/new-trends-in-game-art-industry)
- ['Draw me a swordsman' — Tool-calling LLM 픽셀아트 실험](https://ljvmiranda921.github.io/notebook/2025/07/20/draw-me-a-swordsman/)

### 산업 동향
- [AI for Indie Game Development (2026 Guide)](https://www.aitechboss.com/how-to-use-ai-for-indie-game-development/)
- [Makko — AI 통합 2D 게임 스튜디오](https://www.globenewswire.com/news-release/2026/04/20/3276723/0/en/Makko-launches-AI-powered-2D-game-studio-with-over-40-000-game-assets-created-in-Beta.html)
