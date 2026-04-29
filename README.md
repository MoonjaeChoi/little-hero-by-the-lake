# 🌊 Little Hero by the Lake

> 호숫가 초등학교에 다니는 한 어린이.
> 복층 아파트 다락방에서 아빠의 어린 시절 동화책을 발견한다.
> 책장을 넘기자, 그림 속으로 빨려들어갔다 —
> 아빠도 한때 이 책으로 모험을 떠난 작은 영웅이었다는 걸, 아직은 모른 채.

귀엽고 비폭력적인 자동 전투 로그라이트(Auto-Shooter Roguelite). Vampire Survivors / Brotato 계열의 메커닉에 **장비 = 무기** 시스템과 **세대를 잇는 동화 모험**으로 차별화합니다.

## 🎮 게임 정보

| 항목 | 값 |
|---|---|
| 게임명 | **Little Hero by the Lake** (호숫가 작은 영웅) |
| 장르 | Auto-Shooter Roguelite |
| 진행 방식 | Brotato식 (20웨이브 / 챕터) |
| 플랫폼 | iOS (1차), Android / macOS (추후) |
| 엔진 | Unity 6.3 LTS, URP 2D |
| 등급 | 4+ (가족·아동 친화) |
| 개발 기간 | 약 3.5개월 (1인) |

## ✨ 핵심 차별 포인트

1. **세대를 잇는 영웅** — "Little Hero"는 주인공이자, 어린 시절의 아빠. 챕터를 진행하며 아빠도 같은 책으로 모험했음을 알게 됨
2. **현실 ↔ 동화 이중 구조** — 호숫가 마을(허브) ↔ 동화책 챕터(모험)의 자연스러운 분리
3. **장비 = 무기 시스템** — 7개 슬롯(머리/상의/손/발/액세서리×2/가방) 모두에 능동 효과 부착 가능
4. **아빠의 추억 발굴** — 동화책 속 낙서·메모·스티커가 단서가 되고, 모두 모으면 **마지막 페이지(히든 챕터)** 언락

## 📖 챕터 구성

7권 + 1 히든:
1. 빨간 모자
2. 잭과 콩나무
3. 이상한 나라의 앨리스
4. 헨젤과 그레텔
5. 오즈의 마법사
6. 인어공주
7. 피노키오
- **마지막 페이지** — 아빠가 미처 못 끝낸 챕터 (진엔딩)

## 🛠 기술 스택

- **엔진**: Unity 6.3 LTS
- **렌더링**: Universal Render Pipeline (2D)
- **언어**: C# (.NET Standard 2.1)
- **데이터**: ScriptableObject 기반
- **아키텍처**: 3계층 분리 (Data → Logic → UI) + EventBus
- **개발 도구**: Claude Code (AI 어시스턴트)

## 📁 폴더 구조

```
Assets/_Project/
├── Scripts/
│   ├── Core/           # GameManager, EventBus, ObjectPool
│   ├── Data/           # ScriptableObject 정의
│   ├── Gameplay/       # Player, Equipment, Enemies, Waves
│   ├── Hub/            # 호숫가 마을 + 다락방
│   ├── Meta/           # 메타 진행, 도감, 추억
│   └── UI/             # HUD, 패널, 일기장
├── Data/               # SO 인스턴스
├── Prefabs/
├── Art/   Audio/   Scenes/
```

자세한 규칙·시스템 설계는 `CLAUDE.md` 참고.

## 🚀 시작하기

### 사전 요구 사항
- macOS Ventura (13.0) 이상, Apple Silicon 권장
- Unity Hub + Unity 6.3 LTS (iOS Build Support 모듈 포함)
- Xcode 16+
- Apple Developer 계정 (배포 단계에서 필요)

### 로컬 셋업
```bash
git clone https://github.com/<your-username>/little-hero-by-the-lake.git
cd little-hero-by-the-lake
# Unity Hub에서 폴더 열기
```

### Claude Code 사용
```bash
cd little-hero-by-the-lake
claude
# > "이 프로젝트 처음 시작이야. CLAUDE.md 읽고 Phase 1 시작해줘"
```

## 📅 마일스톤

- [ ] **Phase 0**: 환경 셋업 + iOS 빌드 성공 (1주)
- [ ] **Phase 1**: 프로토타입 (3주차) — 다락방 → 1챕터 3웨이브 + 상점 1회
- [ ] **Phase 2**: 1챕터 완성 (8주차) — 20웨이브 + 장비 12종 + 적 6종
- [ ] **Phase 3**: 콘텐츠 확장 (12주차) — 챕터 3권 + 캐릭터 3명 + 추억 시스템
- [ ] **Phase 4**: TestFlight + 출시 (15주차)

## 🎨 아트 톤

- 파스텔 컬러 (채도 60~70%)
- 둥글둥글 실루엣
- 비폭력 이펙트 (별 / 하트 / 꽃잎 / 비눗방울)

**레퍼런스**: 쿠키런: 킹덤, 동물의 숲, Slime Rancher, Sky: Children of the Light

## 📜 라이선스

미정 (출시 전 결정)

## 🙏 영감을 준 작품

- **Vampire Survivors** (poncle) — 장르 표준
- **Brotato** (Blobfish) — 웨이브 + 인벤토리 모델
- **지브리 작품들** — 정서, 어린이 시점, 일상과 모험의 경계
- **쿠키런: 킹덤** — 한국 캐주얼 톤
