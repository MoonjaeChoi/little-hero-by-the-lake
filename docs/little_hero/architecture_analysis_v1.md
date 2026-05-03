# Architecture Analysis v1 — Unity OOP + Claude Code 방법론

> **목표**: Unity 6 OOP best practices + Claude Code 페어 프로그래밍 + ScriptableObject 아키텍처 종합. 현재 Little Hero 코드베이스를 평가하고 개선 가능 영역 정리.

## 0. 결정 이력
- 2026-05-03: 사용자 요청 — Unity OOP + Claude Code 방법론 깊이 조사 + 현재 설계 반영 가능 부분 분석

## 1. Unity 6 OOP 방법론 (웹 종합)

### 1.1 5 핵심 원칙
1. **Composition over Inheritance** — Component 기반 (MonoBehaviour) 가 OOP 상속보다 모듈성 ↑
2. **SOLID** — 단일 책임 / 개방-폐쇄 / 리스코프 / 인터페이스 분리 / 의존성 역전
3. **Data-Driven Design (DDD)** — ScriptableObject 로 데이터/로직 분리
4. **Event-Driven Architecture** — 직접 참조 대신 EventBus / GameEvent
5. **Dependency Injection** — Constructor injection / Initialize / Composition Root

### 1.2 Unity 6 권장 패턴 (e-book "Level up your code")
| 패턴 | 용도 | Little Hero 적용도 |
|---|---|---|
| **ScriptableObject Architecture** | 데이터 + 이벤트 채널 | ✅ 강함 (SO 200+) |
| **State Pattern** | NPC AI / 게임 상태 | △ 약함 (boss attacks 만 인터페이스) |
| **Object Pool** | 풀링 (탄환·적·VFX) | ✅ 강함 (ObjectPool<T> + VfxPool) |
| **Observer (EventBus)** | 시스템 간 통신 | ✅ 강함 (GameEvents 정적 facade) |
| **Singleton** | 글로벌 매니저 | ✅ 사용 중 (GameManager / MetaProgression / AudioManager) |
| **Factory** | prefab 인스턴스화 | △ 부분 (CreatePattern switch — Factory 미사용) |
| **MVC / MVP** | UI 분리 | ❌ 모놀리식 (MonoBehaviour Panel 자체가 Controller) |
| **Strategy** | 알고리즘 교체 | ✅ IAttackPattern (8 패턴) / IBossAttackPattern (7 패턴) |
| **Composite** | 복합 객체 | △ 약함 |
| **Command** | 입력·undo | ❌ 미사용 |
| **Service Locator** | 정적 facade | ✅ 강함 (CurrencyService / ItemDropService / GachaService 등 13 정적) |

### 1.3 ScriptableObject 패턴 (Ryan Hipple 강연 → Unity 6 표준)

**3 종류**:
1. **Data SO** — EquipmentTemplate / EnemyData / BalanceConfig (정적 데이터)
2. **Variable SO** — IntVariable / FloatVariable (런타임 공유 값) ❌ Little Hero 미사용
3. **Event Channel SO** — GameEventChannel (decoupled 이벤트) ❌ Little Hero 정적 GameEvents 만

**장점**:
- 디자이너 친화 (Inspector 편집)
- 메모리 효율 (1회 로드, 다수 컴포넌트 공유)
- 테스트 용이 (Mock SO 주입)
- 씬/scene 외부 이벤트 체인

## 2. Claude Code Unity 워크플로 (웹 종합)

### 2.1 표준 패턴 (Claude Lab + 공식 가이드)
1. **MCP 서버** — Unity Editor 직접 조작 (씬 wiring / SO 생성 / Play 모드 검증)
2. **다단계 Sprint** — 디자인 문서 → 코드 → SO → 검증 → commit (작은 commit)
3. **CLAUDE.md** — 프로젝트 규칙 + 변경 이력 (Claude 컨텍스트 영구 보존)
4. **단일 책임 commit** — 한 commit = 한 의미 단위 (회수 용이)
5. **Reflection 검증** — Reflection 으로 클래스/메서드 존재 자동 확인

### 2.2 49 AI agent 시스템 (Claude Code Game Studios)
- Director (비전 보호) → Department Lead (도메인 owner) → Specialist (구현)
- Little Hero 는 단일 agent (현재) — 향후 confirmed sub-agent 패턴 도입 가능

### 2.3 Pair Programming 원칙
- **Director model**: 사용자 = 비전 / Claude = C# 구현
- **Iterative refinement**: Play 검증 → 피드백 → 재구현
- **Long context**: CLAUDE.md §14 (변경 이력) 전체 컨텍스트 유지

## 3. Little Hero 현재 아키텍처 평가

### 3.1 코드 통계
- 클래스 약 **128개** (정적 + MonoBehaviour + SO)
- Scripts/ 7 폴더 (Core / Data / Editor / Gameplay / Hub / Meta / UI)
- SO 인스턴스 약 **400+** (Affix 22 + SubAffix 12 + Equipment 16 + Template 34 + Enemy 262 + Chapter 50 + ...)

### 3.2 강점 (✅ 잘 적용된 패턴)

| 패턴 | 코드 위치 | 평가 |
|---|---|---|
| **ScriptableObject 데이터 분리** | Data/ 폴더 전체 | ✅ 모범적 — BalanceConfig / EquipmentTemplate / AffixDefinition |
| **EventBus** | Core/GameEvents.cs | ✅ 14+ 이벤트, 정적 facade |
| **Strategy 패턴** | Gameplay/Equipment/IAttackPattern.cs | ✅ 8 구현체 (Projectile/Aura/Pulse/...) |
| **Object Pool** | Core/ObjectPool.cs / VfxPool.cs / DamageFloaterPool.cs | ✅ GC critical 영역 모두 풀링 |
| **Service Locator** | Core/ 정적 13개 (CurrencyService 등) | ✅ Static facade |
| **Component 분리** | EnemyController + EnemyAI + EnemyAnimationManager | ✅ SRP 준수 |

### 3.3 약점 (❌ 개선 영역)

#### A. **MVC/MVP 부재** (UI 모놀리식)
- `InventorySelectionPanel.cs` 가 View + Controller + Model 모두 보유
- View (Refresh / DrawPage), Controller (HandleCardClicked / ToggleEquip), Model (CollectMatches)
- 200+ 라인 단일 클래스 — CLAUDE.md §5 "100줄 룰" 위반

**개선 제안**: View / Presenter / Model 분리 — Sprint 작업
```
InventorySelectionView (UI Image/Text 갱신만)
  ↑
InventorySelectionPresenter (장착 토글 / 페이징 로직)
  ↓
InventoryModel (CollectMatches / EquipmentInstance list / 정렬)
```

#### B. **Factory 패턴 미사용** (CreatePattern switch)
`EquipmentManager.CreatePattern` 의 9 분기 switch — 신규 패턴 추가 시 2곳 수정 필요 (CreatePattern + GetPatternType).

**개선 제안**: AttackPatternFactory ScriptableObject — 디자이너가 SO 등록만 하면 자동 매핑
```csharp
[CreateAssetMenu] public class AttackPatternRegistry : ScriptableObject {
    public AttackPatternType type;
    public string typeName;  // "ProjectileAttack" — Reflection 으로 인스턴스화
}
```

#### C. **GameEvents 정적 → SO Event Channel 전환 가능**
현재 `GameEvents.OnEnemyKilled += handler` 정적 이벤트.
**SO Event Channel** (Ryan Hipple 패턴) 로 전환 시:
- 디자이너가 Inspector 에서 이벤트 wiring (코드 X)
- 이벤트 별 ScriptableObject (e.g. `EnemyKilledChannel.asset`)
- GameEventListener MonoBehaviour 가 UnityEvent 발행

**Little Hero 적용 가능성**: 50% (현재 정적 GameEvents 가 충분히 동작 — 단 디자이너 작업 ↑ 시 도입)

#### D. **Dependency Injection 없음**
`MetaProgression.Instance` / `GameManager.Instance` 직접 참조 다수.
- MonoBehaviour 의 `Start()` 에서 `Object.FindFirstObjectByType<>` 호출
- `DamageDealer.GetStats()` 캐시 패턴

**개선 제안 (Lightweight DI)**:
- Constructor injection (정적 service)
- 또는 `Initialize(MetaProgression meta, ...)` 명시적 주입
- Zenject 같은 IoC 컨테이너 도입은 **과도** (모바일 + GC 우려)

#### E. **CLAUDE.md §14 변경 이력 700+ 라인**
- Claude 컨텍스트에 매 turn 로드 → 토큰 비용 ↑
- 검색 비용 (grep / Read) ↑

**개선 제안**:
- 직전 60일만 §14 보존
- 이전 항목은 `docs/little_hero/changelog_archive.md` 분리
- 또는 압축 (요약만, 세부 git log 참조)

### 3.4 OOP 원칙 SOLID 평가

| 원칙 | 평가 | 위반 사례 |
|---|---|---|
| **S (단일 책임)** | △ | InventorySelectionPanel 200+ 라인 (View + Controller + Model) |
| **O (개방-폐쇄)** | △ | EquipmentManager.CreatePattern switch (수정 필요) |
| **L (리스코프)** | ✅ | IAttackPattern 8 구현체 모두 호환 |
| **I (인터페이스 분리)** | ✅ | IAttackPattern / IBossAttackPattern 작은 interface |
| **D (의존성 역전)** | △ | 상위 모듈 (DamageSystem) 이 하위 (PlayerStats 직접 참조) |

## 4. 현재 설계 반영 가능 개선 (우선순위)

### 우선순위 A — 즉시 적용 가능 (작은 commit)
1. **GachaService → 사용자 콜백 인터페이스** (현재 정적 → 이벤트 발행 만)
2. **InventorySelectionPanel View/Controller/Model 분리** (200+ 라인 SRP)
3. **CLAUDE.md §14 archive 분리** (60일 이상 항목 → changelog_archive.md)

### 우선순위 B — Sprint 단위 작업
4. **AttackPatternFactory SO** — switch 폐기, 디자이너 친화
5. **SO Event Channel** — 핵심 이벤트만 (예 OnEquipmentChanged) SO 전환 (디자이너 wiring 가능)
6. **Lightweight DI** — `Initialize(meta, gm, ...)` 명시 주입 패턴 (Singleton → Service)

### 우선순위 C — 구조적 변경 (큰 작업)
7. **MVP 도입** — 모든 UI Panel 을 View / Presenter 분리 (Sprint 5+)
8. **Editor 자동화 강화** — ProjectValidator 룰 +20 (현재 12)
9. **Sub-agent 패턴** — Claude Code Game Studios 49 agent 답습 (Director / Lead / Specialist)

### 우선순위 D — 관망 (도입 비용 > 이득)
10. **Zenject DI 컨테이너** — 모바일 GC 우려 + 학습 비용
11. **ECS / DOTS** — 작은 적 무리는 PIVOT v3.1 cap 1500 → 300 으로 충분, ECS 불필요
12. **Variable SO** — IntVariable 등 — 정적 facade 가 더 명확

## 5. 즉시 권장 — InventorySelectionPanel MVP 분리 예시

**현재 (모놀리식 200+ 라인)**:
```csharp
public class InventorySelectionPanel : MonoBehaviour {
    [SerializeField] _gridContainer; _cardPrefab; ...  // View
    private List<...> _matches;  // Model
    private void HandleCardClicked() {...}  // Controller
    private void ToggleEquip() {...}  // Controller (장착 로직)
    private void DrawPage() {...}  // View
}
```

**개선 (View/Presenter/Model 분리)**:
```csharp
// Model — 데이터 + 비즈니스 로직
public class InventoryModel {
    public List<...> CollectMatches(EquipSlot slot);
    public void Equip(EquipmentInstance inst);
    public void Unequip(EquipmentInstance inst);
}

// Presenter — Model + View 중재
public class InventoryPresenter {
    private InventoryModel _model;
    private IInventoryView _view;
    public void OnCardClicked(int idx) {
        var inst = _model.GetMatch(idx);
        _view.UpdateDetailPanel(inst);
    }
}

// View — Unity UI 갱신 만 (인터페이스)
public interface IInventoryView {
    void DrawCards(List<CardData> cards);
    void UpdateDetailPanel(EquipmentInstance inst);
    void SetPageText(int current, int total);
}
public class InventorySelectionView : MonoBehaviour, IInventoryView { ... }
```

## 6. Claude Code 워크플로 개선 제안

### 6.1 현재 잘 적용된 부분 ✅
- **CLAUDE.md** 영구 컨텍스트 (§11~14)
- **단일 책임 commit** (10 commit Sprint 패턴)
- **MCP 자동화** (씬 wiring / SO 생성 / Reflection 검증)
- **디자인 문서 → 코드 → 검증** 3단계

### 6.2 추가 도입 가능
- **Sub-agent 패턴** — 디자이너 / 기획자 / 검증자 역할 분리 (현재 단일 agent)
  - 예: Sprint 1 시작 시 "stats-designer" sub-agent 호출 → 곡선 검증 → 메인 agent 가 코드 작성
- **Reflection-based 자동 wiring** — `[AutoWire]` attribute → SerializedObject 자동 매핑 (현재 수동 .meta 코드)
- **Visual diff 자동화** — Play 모드 스크린샷 + AI vision 비교 (이전 Phase 와 차이 자동 보고)

## 7. 구체적 적용 권장 (3 후보)

### 후보 A — InventorySelectionPanel MVP (1 Sprint, 3 commit)
- Model 분리 (InventoryModel.cs)
- Presenter 분리 (InventoryPresenter.cs)
- View 인터페이스 + 기존 Panel 을 View 로 격하
- **이득**: 200+ 라인 → 3 클래스 각 ~70 라인. 테스트 가능.

### 후보 B — SO Event Channel 도입 (1 Sprint, 4 commit)
- `GameEventChannel<T>` 제네릭 SO 베이스
- 핵심 이벤트 5개 SO 전환: OnEnemyKilled / OnEquipmentChanged / OnLevelUp / OnPlayerDied / OnChapterCleared
- Inspector wiring 으로 UI 자동 갱신
- **이득**: 디자이너 친화 + 정적 GameEvents 보존 (호환)

### 후보 C — CLAUDE.md changelog_archive 분리 (1 commit, 즉시)
- §14 의 2026-05-02 이전 항목 → `docs/little_hero/changelog_archive.md`
- 잔존 60일 이내만 CLAUDE.md
- **이득**: Claude 컨텍스트 비용 ↓ + grep 속도 ↑

## 8. 결론

### Little Hero 아키텍처 종합 평가
- **점수**: 8/10 (Unity 6 모범 사례 다수 적용)
- **강점**: ScriptableObject + EventBus + Strategy + ObjectPool + Service Locator
- **약점**: MVC/MVP / Factory / DI / SOLID-O / 변경 이력 비대

### 권장 즉시 작업 (3 후보 중 선택)
1. ✅ **후보 A** — InventorySelectionPanel MVP (코드 품질 ↑)
2. ✅ **후보 B** — SO Event Channel (디자이너 친화)
3. ✅ **후보 C** — changelog_archive (Claude 효율 ↑)

### 관망 권장 (도입 비용 > 이득)
- ❌ Zenject IoC (모바일 GC 우려)
- ❌ ECS/DOTS (cap 300 환경 불필요)
- ❌ 49 sub-agent (단일 agent 충분)

## 9. Sources

- [Unity Manual — Best practice guides](https://docs.unity3d.com/6000.3/Documentation/Manual/best-practice-guides.html)
- [Design Patterns (Unity 6) — Unity Learn](https://learn.unity.com/course/design-patterns-unity-6?version=6.0)
- [Unity Architecture: Scriptable Object Pattern — Simon Nordon Medium](https://medium.com/@simon.nordon/unity-architecture-scriptable-object-pattern-0a6c25b2d741)
- [Create modular game architecture with ScriptableObjects — Unity](https://unity.com/resources/create-modular-game-architecture-scriptableobjects-unity-6)
- [Build a modular codebase with MVC and MVP — Unity Learn](https://learn.unity.com/course/design-patterns-unity-6/tutorial/build-a-modular-codebase-with-mvc-and-mvp-programming-patterns)
- [Use ScriptableObjects as Event Channels — Unity](https://unity.com/how-to/scriptableobjects-event-channels-game-code)
- [Unity × Claude Code — AI Pair Programming — Claude Lab](https://claudelab.net/en/articles/claude-code/unity-claude-code-game-dev-accelerate)
- [Claude Code Game Studios — GitHub](https://github.com/Donchitos/Claude-Code-Game-Studios)
- [Unity MCP — CoplayDev GitHub](https://github.com/CoplayDev/unity-mcp)
- [How to Automate Game Development with Claude Code × unity-mcp — Claude Lab](https://claudelab.net/en/articles/claude-code/claude-code-unity-mcp-game-dev-workflow)
- [Building an Event Bus in Unity — Outscal](https://outscal.com/blog/building-an-event-bus-in-unity-for-decoupled-communication)
- [Dependency Injection Patterns in Game Code — PulseGeek](https://pulsegeek.com/articles/dependency-injection-patterns-in-game-code/)
- [Unity-Programming-Patterns — Habrador GitHub](https://github.com/Habrador/Unity-Programming-Patterns)
- [Idle Game: Scriptable Object Event System — Deylinia Design](https://deylinia.design.blog/2020/02/07/idle-game-scriptable-object-event-system/)

## 10. 변경 이력
- 2026-05-03: 초기 작성 (Unity 6 OOP + Claude Code 방법론 + Little Hero 평가)
