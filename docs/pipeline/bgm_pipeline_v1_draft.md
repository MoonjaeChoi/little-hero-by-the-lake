# Little Hero — BGM 파이프라인 조사 (draft v1)

> 2026-05-01 — Claude Code + RTX 5070 12GB ComfyUI 서버를 활용한 BGM 트랙 작업 도구 조사.
>
> 본 문서는 **초안**이다. 검토 후 `docs/bgm_pipeline_v2.md` 정식 계획으로 승격.

---

## 0. TL;DR — 추천 우선순위

| 순위 | 도구 | 톤 | 비용 | 워크플로 적합성 |
|---|---|---|---|---|
| ⭐ 1 | **ACE-Step v1.5** (오픈소스, 자체 호스팅) | 범용 (지브리 톤 가능) | $0 (전기료) | RTX 5070 + ComfyUI 같은 서버 재활용 |
| ⭐ 2 | **Suno Pro** ($10/월) | 매우 우수 (보컬 가능) | $10/월 | API 또는 웹 → wget → Unity 임포트 |
| 3 | **Stable Audio 2.5** (ComfyUI Partner) | 우수 (구조적) | API 종량제 (미공개) | ComfyUI MCP 연동 가능, 단 클라우드 API |
| 4 | **AIVA** | 오케스트라 강함 | $11/월 (Standard) | MIDI 익스포트 → DAW 후처리 |
| 5 | **MusicGen** (Meta, 오픈소스) | 보통 (older) | $0 | 12GB 안정적, 대안 |

**Little Hero 1차 권장: ACE-Step v1.5 + Suno Pro 1개월 트라이얼 병행** (셀프호스팅 품질 검증 + 안전망 확보).

---

## 1. 평가 기준 — Claude Code 워크플로 적합성

| 기준 | 가중치 | 설명 |
|---|---|---|
| **자동화 가능성** | ⭐⭐⭐ | Claude Code 가 prompt → audio file → Unity import 까지 단일 사이클 가능한가 (스크립트/MCP) |
| **상업적 라이선스** | ⭐⭐⭐ | iOS App Store 수익화·IAP 안전, 학습 데이터 소송 리스크 |
| **품질** | ⭐⭐ | 지브리 톤 4+ 등급, 32 비트 PCM 또는 OGG 192kbps 이상 |
| **루프 가능성** | ⭐⭐ | 게임 BGM 핵심 — seamless loop 또는 intro+loop 분리 |
| **비용** | ⭐⭐ | 1인 인디·Phase 3 단계, 초기 비용 최소화 |
| **다양성** | ⭐ | 7~35 챕터별 분위기 + 보스 + 허브 = 최소 10 트랙 필요 |

---

## 2. 셀프호스팅 옵션 (RTX 5070 12GB 활용)

### ⭐ 2.1 ACE-Step v1.5 — **2026 최강 오픈소스 음악 모델**

- **저장소**: `github.com/ace-step/ACE-Step-1.5`
- **라이선스**: MIT (상업적 사용 허용, indemnification 없음)
- **VRAM**:
  - **6GB 이하**: 2B turbo + INT8 + CPU offload
  - **8~16GB (RTX 5070 해당)**: 2B turbo/sft + 0.6B/1.7B LM, **vLLM 백엔드**
  - 16~20GB: XL turbo + 1.7B
  - 20GB+: XL sft + 4B (최고 품질)
- **속도**: RTX 3090 24GB 에서 풀곡 < 10초. RTX 5070 12GB + INT8 quant + vLLM = 곡당 ~30초 추정 (RTX 3060 12GB turbo 벤치 기준)
- **품질**: Suno v3 vs ACE-Step XL 블라인드 테스트에서 비등 또는 우세 주장 (제3자 리뷰)
- **보컬 + 가사**: 가능 (영어/일본어/중국어/한국어 지원)
- **ComfyUI 연동**: Partner 노드 + "Awesome ACE-Step" 커뮤니티 노드
- **API**: REST (`uv run acestep-api` → localhost:8001) — Claude Code 가 직접 호출 가능
- **설치**:
  ```bash
  ssh staging
  git clone https://github.com/ACE-Step/ACE-Step-1.5
  cd ACE-Step-1.5
  uv sync       # PyTorch + dependencies
  uv run acestep-api  # 서버 기동
  ```
- **장점**:
  - 무제한 생성 (월 비용 0)
  - ComfyUI 서버 재활용 (같은 RTX 5070)
  - Claude Code → SSH → REST API → 결과 다운로드 자동화
  - 학습 데이터 소송 리스크 낮음 (오픈소스, 사용자 책임 명시)
- **단점**:
  - 12GB 에서 INT8 quant + offload 필요 → 품질 약간 손실
  - 보컬 한국어는 라벨 데이터 부족 가능성 (인스트루멘탈 권장)
  - 초기 모델 다운로드 ~10GB

### 2.2 Meta MusicGen (AudioCraft)

- **저장소**: `github.com/facebookresearch/audiocraft`
- **라이선스**: MIT (코드) + CC-BY-NC 4.0 (가중치) — **상업 사용 제한**
- **VRAM**: 8GB+ (300M), 16GB+ (3.3B Large)
- **품질**: 2023 모델, ACE-Step 보다 한 세대 뒤
- **비고**: **CC-BY-NC 가중치 라이선스 때문에 게임 출시용으로는 부적합**. 프로토타입·내부 테스트만 가능.

### 2.3 Stable Audio Open 1.0

- **저장소**: `huggingface.co/stabilityai/stable-audio-open-1.0`
- **라이선스**: Stability AI Community License (상업 사용 가능, 매출 $1M 이하 또는 별도 계약)
- **VRAM**: 8GB+
- **출력 길이**: 47초 (모델 한계)
- **비고**: 인디 게임은 매출 제한 안 걸리지만, 출력 길이 짧아 BGM 본격용은 부족. **SFX 변형용으로 후보**.

---

## 3. 클라우드 API 옵션

### ⭐ 3.1 Suno AI Pro/Premier

- **가격**: Pro $10/월 (500크레딧) / Premier $30/월 (2000크레딧 + **stem 분리**)
- **품질**: 2026년 현재 SaaS 1위 (보컬·가사·구조 우수)
- **상업 사용**:
  - **Pro 이상에서 게임 BGM OK**: "Using AI-generated music for game menus, levels, cutscenes, and ambient audio in indie or commercial game development." (TOS 명시)
  - **소유권**: 유료 구독자가 출력물 소유
  - **indemnification 없음**: 저작권 침해 시 Suno 가 법적 비용 부담 X
- **워크플로**: 웹 UI 또는 API. Premier 의 stem 분리 → Unity 다층 BGM 가능
- **소송 리스크**: Universal/Warner Music Group 소송 진행 중 (2026 기준). 합의 진행되어 라이선스 모델 전환 중. 단기·인디용은 큰 위험 낮음.
- **비고**: Pro 1개월 트라이얼로 빠르게 35챕터 시드 확보 후 셀프호스팅 전환 가능.

### 3.2 Udio

- **가격**: $10/월 (Standard) / $30/월 (Pro)
- **품질**: 48kHz 출력 (Suno 44.1kHz 보다 고품질) + audio inpainting
- **상업 사용**: 게임 배포 OK, **펌웨어/ROM/오프라인 전용 임베딩 금지** (사전 동의 없이는 불가)
- **비고**: Universal Music Group 합의 (2025-10) → 2026 라이선스 플랫폼 출시 예정. 게임 음원은 inpainting 으로 패시지 미세 수정 가능.

### 3.3 Stable Audio 2.5 (ComfyUI Partner Node)

- **가격**: API 종량제 (Stability AI Console)
- **품질**: 최대 3분, 음악 구조(intro/dev/outro) 학습됨
- **워크플로**: ComfyUI 서버에서 노드로 호출 → API 키 필요 (즉, **셀프호스팅 아님**)
- **장점**: 라이선스된 데이터로 학습 — 소송 리스크 낮음, audio-to-audio inpainting
- **단점**: API 비용, 인터넷 의존, 셀프호스팅 우위 약화

### 3.4 AIVA — 오케스트라 특화

- **가격**: Standard $11/월 (1080GB 다운로드/월) / Pro $33/월
- **품질**: 클래식·시네마틱·게임 RPG 톤 매우 우수
- **상업 사용**: Standard 이상 OK, **MIDI 익스포트** 가능 (DAW 후처리·악기 교체 자유)
- **비고**: Little Hero 처럼 **지브리 톤** 추구하면 AIVA 의 "Cinematic" "Anime" 프리셋 적합. MIDI → DAW 에서 일본 동요 음계 맞춤화 가능.

---

## 4. 무료 SaaS (백업 옵션)

| 도구 | 라이선스 | 한계 |
|---|---|---|
| **SOUNDRAW** | 영구 상업 라이선스 (구독 중 생성분만) | 월 무제한 다운로드 $19.99 |
| **Mubert** | 무료 티어 attribution 필요, Creator $14/월 | AI 스트림 — seamless loop 아님 |
| **Beatoven.ai** | $20/월 (Pro) | 게임 BGM 적합, 한정 다양성 |
| **Soundverse** | 무료 티어 + 유료 | 텍스트→음악, 신생 |

**평가**: 워크플로 자동화 약함 (대부분 웹 UI), 가격 대비 ACE-Step 셀프호스팅 우위.

---

## 5. Little Hero 1차 권장 시나리오

### A. 셀프호스팅 우선 (비용 최소화) — **권장**

```
[1] 서버 ACE-Step 설치 (20분)
    ssh staging → git clone → uv sync → 모델 다운로드
[2] Claude Code 가 prompt 템플릿 작성
    - 허브: "calm warm acoustic guitar + flute, lakeside village, daylight, 90 BPM, looping"
    - 챕터1 빨간모자: "playful pizzicato strings + woodwind, gentle adventure, 110 BPM"
    - 보스: "bright orchestral fanfare, no menace, 120 BPM"
[3] REST API 호출 (Claude Code Bash → curl)
    - 곡당 30초 생성, 4 변형 한 번에 → 베스트 1 선택 (Vision 평가 어려움 → 사용자 청취 필수)
[4] 후처리 (선택)
    - Audacity (CLI 가능) crossfade loop 처리
    - 또는 Unity Introloop 플러그인 ($15) 으로 intro+loop 분리
[5] Unity 임포트
    - ./Assets/_Project/Audio/BGM/ 에 .ogg 배치
    - AudioManager 확장 (BGM 트랙 슬롯 + AudioSource 2번째)
```

**예상 시간**: 환경 설치 1시간 + 10 트랙 생성·선별 3시간 + 루프 처리 2시간 = **6시간**

### B. SaaS 우선 (품질 안정성)

```
[1] Suno Pro $10 1개월 가입
[2] 35 챕터 + 허브·보스 + 다락방 = 40 트랙 텍스트 prompt → 웹/API 생성
    - 500크레딧 / 5크레딧/곡 = 100곡 가능 → 2 변형씩 200곡 시드
[3] 베스트 선별 후 다운로드 (mp3 → ogg 변환)
[4] 루프 처리 + Unity 임포트
[5] 1개월 후 구독 해지, 셀프호스팅으로 전환
```

**비용**: $10. **시간**: 환경 0 + 생성·선별 4시간 + 루프 처리 2시간 = 6시간.

### C. 하이브리드 (권장)

```
[1] Suno Pro $10 → 우선 허브·보스·CH1~10 시드 확보 (검증된 품질)
[2] ACE-Step 설치 → CH11~35 셀프호스팅
[3] Audacity 일괄 루프 처리 자동화 (Claude Code Bash 스크립트)
[4] AudioManager 에 BGMController 통합
```

장점: 핵심 트랙은 안정 품질 + 대량 생성은 무료 + 점진적 전환.

---

## 6. 게임 BGM 특수 요구사항 — 잊기 쉬운 사항

| 항목 | 설명 | 도구별 지원 |
|---|---|---|
| **Seamless loop** | 끝과 시작이 매끄럽게 이어짐 | Suno: stem 분리 후 수동 / ACE-Step: 후처리 / Stable Audio: inpainting |
| **Intro + Loop** | 도입부 1회 + 본 루프 무한 (전투 진입감 ↑) | Unity Introloop 플러그인 / Doremi (오픈소스) |
| **Layered (adaptive)** | 웨이브 진행에 따라 악기 추가 (Brotato 풍) | Suno Premier stem / 또는 별도 트랙 N개 + AudioMixer |
| **Sting** | 보스 등장·웨이브 클리어 5~10초 짧은 효과 | 어떤 도구든 짧게 prompt — 또는 P9 절차합성 확장 |
| **포맷** | iOS/Android: OGG Vorbis 192kbps 권장 (용량 vs 품질) | mp3 → ogg 변환: `ffmpeg -i in.mp3 -c:a libvorbis -q:a 5 out.ogg` |
| **메모리** | 모바일 60fps + 200MB 이하 (CLAUDE.md §7) → BGM 트랙당 2~3MB 목표 |  |

### 6.1 Audacity CLI 루프 처리 (Claude Code 자동화)

```bash
# crossfade loop: 끝 2초 + 시작 2초 교차 페이드
ffmpeg -i input.ogg -filter_complex "
  [0:a]asplit[a][b];
  [a]atrim=0:30,asetpts=PTS-STARTPTS[main];
  [b]atrim=28:30,asetpts=PTS-STARTPTS,afade=t=out:st=0:d=2[end_fade];
  [main][end_fade]amix=inputs=2:duration=longest
" output_loop.ogg
```

또는 Audacity Python script (audacity-scripting): 수동 베스트.

### 6.2 Unity AudioManager 확장 안

```csharp
// Core/AudioManager.cs 에 추가
[SerializeField] private AudioClip[] _bgmTracks;  // 트랙 SO 또는 직접
private AudioSource _bgmSrc;

public void PlayBgm(int trackIdx, bool loop = true)
{
    if (_bgmSrc == null)
    {
        _bgmSrc = gameObject.AddComponent<AudioSource>();
        _bgmSrc.loop = loop;
        _bgmSrc.volume = 0.4f;
    }
    if (trackIdx < 0 || trackIdx >= _bgmTracks.Length) return;
    if (_bgmSrc.clip == _bgmTracks[trackIdx]) return;  // 동일 트랙 재시작 방지
    _bgmSrc.Stop();
    _bgmSrc.clip = _bgmTracks[trackIdx];
    _bgmSrc.Play();
}

// Crossfade 전환
public IEnumerator CrossFadeBgm(int newIdx, float duration = 1.5f) { /* ... */ }
```

ChapterRunner.Start 에서 챕터별 인덱스로 호출. AtticController 가 허브 트랙. WaveManager 가 보스 웨이브 시작 시 보스 트랙 crossfade.

---

## 7. 위험·주의사항

| 위험 | 평가 | 완화 |
|---|---|---|
| **저작권 소송** | Suno/Udio 진행 중. 합의 진행. | Pro 이상 사용 + 셀프호스팅 백업 + 출시 직전 라이선스 재검증 |
| **AI 표시 의무** | EU AI Act, 한국·일본 자율 가이드 | App Store 설명에 "음악 일부 AI 보조 생성" 명시 (간결) |
| **품질 편차** | 같은 prompt 도 매번 다름 | 곡당 4 변형 생성 → 사용자 청취 베스트 |
| **루프 부자연** | seamless 어려움, 보통 1.5~3초 fade 필요 | Audacity 후처리 표준화 |
| **Unity 메모리** | OGG 압축 사용, 트랙당 2~3MB | DecompressOnLoad 끄고 Streaming 또는 CompressedInMemory |

---

## 8. 다음 단계 (사용자 결정 후)

1. **시나리오 선택** (A/B/C)
2. ACE-Step 설치 (시나리오 A/C) — Claude Code Bash 자동화 가능
3. Suno Pro 가입 (시나리오 B/C) — 사용자 직접
4. 10 트랙 prompt 템플릿 작성 (Claude Code)
5. 생성 → 사용자 청취 → 베스트 선별
6. Audacity/ffmpeg 루프 처리
7. AudioManager.PlayBgm API 추가 + 챕터 wiring
8. **bgm_pipeline_v2.md** 정식 계획 승격

---

## 9. 출처

- ACE-Step v1.5: [GitHub](https://github.com/ace-step/ACE-Step-1.5) / [GPU 호환성](https://github.com/ace-step/ACE-Step-1.5/blob/main/docs/en/GPU_COMPATIBILITY.md)
- Stable Audio 2.5 ComfyUI: [Comfy 블로그](https://blog.comfy.org/p/stable-audio-25-is-now-in-comfyui)
- Suno 상업 사용: [terms.law 가이드](https://terms.law/ai-output-rights/suno/)
- Suno vs Udio 2026: [Margabagus 비교](https://margabagus.com/udio-vs-suno-2025-comparison/) / [Neuronad](https://neuronad.com/suno-vs-udio/)
- AudioCraft / MusicGen: [Meta AI](https://ai.meta.com/resources/models-and-libraries/audiocraft/)
- Unity 루프: [Andrew Mushel](https://andrewmushel.com/articles/looping-music-in-unity/) / [Introloop](https://exceed7.com/introloop/) / [Doremi](https://github.com/Meorge/Doremi)
- 종합 비교: [Soundverse 블로그](https://www.soundverse.ai/blog/article/best-free-ai-music-generators) / [Full Stack Creators 2026](https://fullstackcreators.com/ai-music-generation-in-2026-every-tool-ranked-honestly/)
