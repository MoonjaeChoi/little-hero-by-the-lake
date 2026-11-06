# Part 7. 시스템 아키텍처

> [← Part 6](./part_6_season_catchup.md) · [README](../README.md) · [다음: Part 8 →](./part_8_mvp_roadmap.md)
>
> ✦ `direction_v2.md` 1번 항목(모바일 전용) 반영. 웹 스택(Next.js, ui-web) 제거.

---

## 🔎 구현 현황 (2026-07-17 검증) — ⚠️ 본 파트는 실제 구현과 전면 상이

> 범례: ✅ 설계대로 구현 · 🔄 사양 변경 · ❌ 미구현/폐기 · ⛔ 아키텍처/PIVOT 으로 무효
>
> **이 문서가 상정한 웹/서버 아키텍처는 실제로 채택되지 않았다.** 프로젝트는 **Unity 6.3 / C# 로컬 클라이언트** 로 구현되었고, 백엔드 서버가 존재하지 않는다. 아래 사양은 대부분 ⛔ 무효이며, 실제 아키텍처는 `CLAUDE.md §4` 를 참조한다.

| 설계 (본 문서) | 상태 | 실제 구현 |
|---|---|---|
| 클라이언트: React Native (Expo) | ⛔ | **Unity 6.3 LTS, URP 2D, C# (.NET Standard 2.1)** |
| 엣지: API Gateway (인증/레이트리밋) | ⛔ | 없음 (로컬 앱) |
| 서비스: FastAPI 모놀리스 (player/game/realtime) | ⛔ | **MonoBehaviour Manager + 정적 EventBus**. `GameManager`/`MetaProgression` 등 |
| 데이터: PostgreSQL + Redis | ⛔ | **로컬 `save.json`** (`SaveSystem`, `JsonUtility`) + `SaveDataMigration`. DB·캐시 없음 |
| 강화 요청 REST 트랜잭션 (SELECT FOR UPDATE 등) | ⛔ | 정적 호출 `EnhancementService`(로컬), 부수효과는 C# `GameEvents` |
| 운영 어드민(CMS/유저검색/재화지급) | ⛔ | 서버 어드민 없음 — **Editor 메뉴(Tools/LittleHero)** 시드/검증 도구로 대체 |
| AWS 인프라 (RDS/ElastiCache/ECS/S3) | ⛔ | 없음 — App Store/TestFlight 배포 |
| 프론트 모노레포 (api-client/ui-core/game-types) | ⛔ | 단일 Unity 프로젝트, 어셈블리 3종(`Assembly-CSharp`/`-Editor`/`Tests.Editor`) |

**실제 아키텍처 (CLAUDE.md §4, 3계층)**: ① Data Layer = ScriptableObject(밸런스/콘텐츠) · ② Logic Layer = MonoBehaviour Manager + EventBus(정적 `GameEvents` + SO Event Channel) · ③ UI Layer = uGUI Panel/HUD. 데미지 흐름은 `DamageQuery`/`DamageDealer`/`DamageSystem` (Physics2D 기반). OOP 정책은 §15 + `ProjectValidator`(Editor) 로 강제.

> ⛔ 아래 원본 본문(FastAPI 디렉토리 구조·REST 흐름·AWS 견적 등)은 **역사적 설계 기록**으로만 보존한다. 현재 코드에는 대응물이 없다.

---

## 1. 핵심 결정

### 런타임 4계층 구조

| 계층 | 구성요소 | 역할 |
|---|---|---|
| 클라이언트 | ✦ React Native (Expo) — 모바일 단일 | UI 렌더링, 사용자 입력 |
| 엣지 | API Gateway | 인증, 레이트 리밋, 라우팅 |
| 서비스 | Player Service, Game Service, Realtime Service | 비즈니스 로직 |
| 데이터 | PostgreSQL + Redis | 영구 저장 + 캐시/pub-sub |

운영 도구(CMS, 어드민, 분석)는 사이드카로 분리하여 게임 서버와 같은 DB를 보지만 **별도 인증·별도 IP 화이트리스트로 격리**한다. 운영 도구 자체는 내부용 웹(Next.js 가능)이지만 게임 클라이언트와 무관하다.

---

## 2. 설계 원칙

### 백엔드 — 단일 모놀리스로 시작 (FastAPI)

출시 초기엔 트래픽 패턴도 모르고 도메인 경계도 흐릿해서, MSA부터 시작하면 분배 트랜잭션과 서비스 디스커버리에 시간을 잡아먹는다. **도메인 경계를 코드 구조로 강제하고, 정말 필요해질 때 분리**한다.

```
backend/
├─ app/
│  ├─ player/        # Player Service 영역
│  │  ├─ models.py        # Pydantic 모델
│  │  ├─ repository.py    # DB 접근 (한 도메인의 모든 SQL)
│  │  ├─ service.py       # 비즈니스 로직 (강화, 리롤, 지갑)
│  │  └─ router.py        # FastAPI 라우터
│  ├─ game/          # 전투, 던전, 시즌
│  ├─ realtime/      # WebSocket 핸들러
│  ├─ content/       # 마스터 데이터 (세트, 신화 정의)
│  └─ shared/        # 효과 리졸버, RNG, 인증 미들웨어
└─ ops/              # CMS, 어드민 (별도 FastAPI 앱)
```

**비동기 사용 가이드라인**:
- 외부 API 호출 (결제, 푸시 알림): `async`
- WebSocket 핸들러: `async`
- 일반 게임 도메인 트랜잭션: **동기 권장** (디버깅 단순, 일관된 성능)

### 프론트엔드 — 모노레포 + 공유 타입 (✦ 모바일 단일)

```
frontend/
├─ apps/
│  └─ mobile/        # React Native (Expo)
└─ packages/
   ├─ api-client/    # 백엔드 호출 함수 + 타입
   ├─ ui-core/       # 컴포넌트 라이브러리 (ItemCard 등)
   └─ game-types/    # 백엔드 OpenAPI에서 자동 생성된 타입
```

> 단일 앱이라 모노레포가 과한 구성처럼 보일 수 있으나, `game-types`/`api-client`를 패키지로 분리해두면 운영 어드민 웹앱(`ops/`)이 같은 OpenAPI 타입을 공유할 수 있어 그대로 둔다.

`game-types/`를 백엔드 OpenAPI 스키마에서 자동 생성(`openapi-typescript` 사용)하면 응답 타입이 항상 백엔드 진실에 동기화된다. 백엔드 스키마 변경이 프론트의 컴파일 에러로 즉시 잡혀 유지보수 비용이 크게 줄어든다.

`api-client/`는 React Query(TanStack Query)로 감싼다. 캐싱, 낙관적 업데이트, 재시도, 무한 스크롤이 다 들어와있어 직접 만들 게 거의 없다.

---

## 3. 구체적 사양

### 핵심 데이터 흐름 — 강화 요청 1회

```
1. 사용자가 강화 버튼 클릭 (모바일)
2. 클라이언트: POST /items/{id}/enhance + JWT
3. API Gateway: JWT 검증, 레이트 리밋, Player Service로 라우팅
4. Player Service 단일 트랜잭션 처리
    a. SELECT FOR UPDATE on items
    b. 재화 차감 (WalletRepository.deduct)
    c. fail_count 조회 → soft pity 적용해 p_actual 계산
    d. crypto.randomFloat()으로 결정
    e. 성공: level += 1, fail_count = 0 / 실패: fail_count += 1
    f. 효과 리졸버 재계산 → 캐시 갱신
    g. audit_log에 append
    h. 단일 트랜잭션 commit or rollback
5. Player Service → Realtime Service (Redis pub/sub): 인벤토리/스탯 변경 알림
6. 응답 반환: { success, new_level, fail_count, next_probability, ... }
7. 클라이언트: React Query 캐시 머지 → 결과 애니메이션 → UI 갱신
```

**핵심**: 한 트랜잭션 안에 모든 부수효과를 가둠. 재화는 차감됐는데 강화는 안 됐다는 상황이 절대 일어나면 안 된다.

### 운영 도구 — 첫날부터 만들 것

출시 후 1주일이 지나면 CS 요청이 쏟아지는데, 어드민이 없으면 SQL을 직접 짜서 처리하게 되고 한 번의 오타가 영구 데이터 손상으로 이어진다.

**최소 어드민 기능**:
- 유저 검색 + 인벤토리 조회
- 아이템/재화 수동 지급
- 마스터 데이터 편집 (세트/신화 정의)
- 시즌 운영 (시작/종료/이벤트 토글)
- `audit_log` 검색 (유저별, 시간대별)

### 초기 인프라 (5,000~10,000 동접 기준)

- AWS RDS PostgreSQL (Multi-AZ)
- ElastiCache Redis
- ECS Fargate 또는 EC2 + Docker Compose로 FastAPI 컨테이너 2~3개
- Application Load Balancer
- CloudFront (정적 자산 캐싱)
- S3 (아이템 아이콘, 미디어)
- Datadog 또는 Grafana Cloud (모니터링)

---

## 4. 미해결 질문

- 정확한 인프라 견적 (월 비용)
- CI/CD 파이프라인 (GitHub Actions? GitLab? Jenkins?)
- 보안 스캔 / 침투 테스트 시점
- 결제 시스템 통합 (Apple/Google IAP, PG)
- 푸시 알림 인프라 (FCM, APNs)
- 분석 도구 (Mixpanel? Amplitude? 자체 구축?)
- 로그 수집 (CloudWatch Logs? ELK?)

---

## 5. 다음 단계

- 인프라 다이어그램 상세화 (네트워크, VPC, 보안 그룹)
- 도메인별 API 스펙 (OpenAPI yaml 작성)
- 보안 모델 정의 (인증, 권한, 데이터 격리)
- 로깅 / 관측성 표준 (구조화 로그, 트레이싱)
- 결제 PG 비교 + 선정
- DR / 백업 정책
