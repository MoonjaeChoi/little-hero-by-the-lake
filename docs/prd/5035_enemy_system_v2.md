# 5035 enemy_system_v2 — 디자인 문서

> **목적**: 적 시스템 객체지향 재설계 — IEnemyBehavior Strategy + IDeathReaction + 5 신규 적 + WaveComposition SO. 메이플 키우기 + 탕탕특공대 정합.
> **참조**: `chapter_curve_v1.md` / `5001_boss_system_v2_prd.md` (IBossPhase 답습) / `5034_balance_integration_v1.md`
> **상태**: PRD (디자인 + Sprint)
> **시작**: 2026-05-07

## 0. 결정 이력

| 일자 | 결정 |
|---|---|
| 2026-05-07 | 사용자 요청 — 위치 고정 적 + 적 특성 다양화 + OOP 재사용 |
| 2026-05-07 | 탕탕특공대 / Survivor.io 조사 — 5 신규 특성 (Stationary/Charger/Splitter/Ranged/Bomber) |

## 1. 현재 시스템 진단

### 1.1 EnemyController + EnemyAI 구조 (현재)
- EnemyController (외형 + HP) + EnemyAI (이동) — 분리 OK
- EnemyAI.Update — 단순 추적 (Plant `_target.position` 향)
- 적 종류 분기 = EnemyData SO 의 stat 만 — behavior 분기 X

### 1.2 결함
- 단일 behavior (단순 추적) — 다양성 X
- "분열 적 (곰형)" 도입 시 EnemyController 에 OnDeath hook 추가 필요 — 책임 분산
- 챕터 wave 구성 = ChapterData 직접 참조 — 재사용 ↓

## 2. 메이플 / 탕탕특공대 조사 답습

### 2.1 적 특성 (탕탕특공대 / Survivor.io 정합)
- 단순 추적 (현재 LittleHero 와 동일)
- 엘리트 (빨간 오라 + 사이즈)
- 곰: 넉백 저항 + 사망 시 분열
- 개 좀비: 8분 대량 러시
- 보스: 5분 사이클 + 텔레그래프 + 차지 공격

### 2.2 LittleHero 도입 1차 (5종)
1. **Stationary** — 위치 고정 (★ 사용자 요청)
2. **Charger** — 주기적 돌진
3. **Splitter** — 사망 시 분열 (곰형)
4. **Ranged** — 원거리 발사체
5. **Bomber** — 자폭 (지연)

## 3. 객체지향 재설계

### 3.1 IEnemyBehavior interface (Strategy 패턴, 5001 IBossPhase 답습)

```csharp
public interface IEnemyBehavior {
    void Initialize(EnemyController owner, EnemyData data, Transform target);
    void Tick(float deltaTime);
    void OnEnter();   // 활성 시 1회
    void OnExit();    // 비활성 시 1회 (Pool 반환)
}
```

### 3.2 5 신규 + 1 기존 (총 6 behavior)

```
IEnemyBehavior
  ├ FollowBehavior      (기존 EnemyAI 마이그레이션)
  ├ StationaryBehavior  (NEW 위치 고정 + 주기적 발사체)
  ├ ChargerBehavior     (NEW 주기적 플레이어 향 돌진)
  ├ SplitterBehavior    (NEW 추적 + 사망 분열은 IDeathReaction)
  ├ RangedBehavior      (NEW 거리 유지 + 발사체)
  └ BomberBehavior      (NEW 가까이 가면 폭발 텔레그래프)
```

### 3.3 IDeathReaction interface (사망 시 별도 책임)

```csharp
public interface IDeathReaction {
    void OnDeath(EnemyController owner, Vector3 deathPosition);
}

// 구현체:
//   NoneReaction      (기본)
//   SplitterReaction  (NEW 작은 적 N마리 소환)
//   ExplodeReaction   (NEW Bomber 폭발 — 플레이어/적 데미지)
```

### 3.4 EnemyBehaviorFactory + EnemyDeathReactionFactory

```csharp
public static class EnemyBehaviorFactory {
    public static IEnemyBehavior Create(EnemyBehaviorType type) => type switch {
        EnemyBehaviorType.Follow      => new FollowBehavior(),
        EnemyBehaviorType.Stationary  => new StationaryBehavior(),
        EnemyBehaviorType.Charger     => new ChargerBehavior(),
        EnemyBehaviorType.Splitter    => new SplitterBehavior(),
        EnemyBehaviorType.Ranged      => new RangedBehavior(),
        EnemyBehaviorType.Bomber      => new BomberBehavior(),
        _                              => new FollowBehavior(),
    };
}
```

### 3.5 EnemyData 신규 필드

```csharp
public enum EnemyBehaviorType { Follow, Stationary, Charger, Splitter, Ranged, Bomber }
public enum EnemyDeathReactionType { None, Splitter, Explode }

public class EnemyData : ScriptableObject {
    // 기존 stats 외
    public EnemyBehaviorType behaviorType;
    public EnemyDeathReactionType deathReactionType;
    public EnemyData splitterChildData;  // SplitterReaction 시 소환 prefab
    public int splitterChildCount = 3;
    public float bomberRadius = 1.5f;
    public float chargerInterval = 3f;
    public float rangedFireInterval = 1.5f;
    public GameObject projectilePrefab;  // Ranged/Stationary 발사체
}
```

### 3.6 WaveComposition + EnemyComposition SO (재사용 ↑)

```csharp
[CreateAssetMenu(menuName = "LittleHero/EnemyComposition")]
public class EnemyComposition : ScriptableObject {
    public EnemyData enemyData;
    public int count = 10;
    public SpawnPattern pattern;  // Continuous / Burst / Timed
    public float interval = 0.5f;
}

public enum WaveType { Standard, Rush, Elite, Boss, SplitterRush }

[CreateAssetMenu(menuName = "LittleHero/WaveComposition")]
public class WaveComposition : ScriptableObject {
    public WaveType waveType;
    public EnemyComposition[] compositions;
    public float waveDuration = 30f;
}

public class ChapterData : ScriptableObject {
    // 기존 외
    public WaveComposition[] waves = new WaveComposition[20];  // 20 wave 별
}
```

**재사용**: WaveComposition / EnemyComposition 별도 SO → 다른 챕터 / 모드 (Endgame / 무한 다락방) 에서 reuse.

## 4. Sprint 분해 (~7 Sprint, ~3.5h)

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | IEnemyBehavior + IDeathReaction interface + EnemyData enum/필드 | 1 | 30분 |
| 2 | FollowBehavior (기존 EnemyAI 마이그레이션) + EnemyBehaviorFactory | 1 | 30분 |
| 3 | 4 신규 behavior (Stationary/Charger/Ranged/Bomber) | 1 | 45분 |
| 4 | SplitterReaction + ExplodeReaction + Factory | 1 | 30분 |
| 5 | EnemyComposition + WaveComposition SO + ChapterData 통합 | 1 | 30분 |
| 6 | EnemyController 통합 (behavior + deathReaction Tick + OnDeath) | 1 | 30분 |
| 7 | NUnit 8+ + changelog + CLAUDE.md | 1 | 30분 |
| **합계** | | **7** | **~3h 30m** |

## 5. Discuss 항목 (mediator 결정)

| ID | 항목 | 옵션 | Claude 권장 |
|---|---|---|---|
| **D1** | 1차 도입 적 특성 | A 5종 (Stationary+Charger+Splitter+Ranged+Bomber) / B 3종만 (Stationary+Splitter+Charger) / C 10종 모두 | A — 균형 |
| **D2** | StationaryBehavior 동작 | A 발사체 X (단순 정지) / B 주기적 발사체 / C 발사체 + 회복 (Healer 통합) | B — 긴장감 |
| **D3** | SplitterReaction 분열 수 | A 2마리 / B 3마리 (탕탕 답습) / C 5마리 | B |
| **D4** | BomberBehavior 폭발 반경 | A 1.0u / B 1.5u / C 2.0u | B |
| **D5** | 기존 EnemyAI 마이그레이션 | A FollowBehavior 전환 + EnemyAI 삭제 / B 둘 다 보존 | A — 깔끔 |
| **D6** | WaveComposition vs ChapterData 통합 방식 | A WaveComposition 신규 SO + ChapterData 참조 / B ChapterData 안에 직접 | A — 재사용 |
| **D7** | 적용 챕터 범위 | A CH1~5 시범 / B 35 챕터 모두 / C 신규 보너스 모드 | A — 점진 |
| **D8** | NUnit 범위 | A 8 (각 behavior 단위) / B 12 (behavior + reaction + factory + composition) / C 15+ | B |

## 6. 검증 시나리오

### 시나리오 1 — Stationary 적 출현
- CH3 wave 5 → Stationary 적 등장 → 발사체 공격
- 사용자 회피 가능 (이동 패턴)

### 시나리오 2 — Splitter 적 (곰형)
- CH4 wave 10 → Splitter 1마리 → 사망 → 작은 적 3마리 분열
- Hierarchy 잔존 0 (Pool 정상)

### 시나리오 3 — Bomber 적
- CH5 wave 15 → Bomber 적 → 플레이어 가까이 → 1초 텔레그래프 → 폭발 1.5u
- 회피 가능 (이동 거리)

## 7. §15 OOP 정합

| 정책 | 적용 |
|---|---|
| §15.1 SRP | EnemyController (외형) / IEnemyBehavior (이동) / IDeathReaction (사망) 3 책임 분리 |
| §15.1 OCP | EnemyBehaviorFactory + Strategy — 신규 behavior 추가 시 기존 코드 변경 X |
| §15.4 SO 우선 | EnemyData / EnemyComposition / WaveComposition 모두 SO |
| §15.6 100L | 각 behavior ~50L, 200L 룰 통과 |
| §15.7 DI | Initialize(controller, data, target) 명시 |
| §15.9 Test | NUnit 8+ (D8 결정 후) |
| §15.11 안티 | private 필드 + 정적 buffer (성능 critical) |

## 8. 위험 + 완화

| 위험 | 완화 |
|---|---|
| EnemyController 통합 시 기존 EnemyAI 사용처 영향 | D5=A (마이그레이션), 호출처 1~2 파일만 |
| Splitter 분열 시 Pool 부담 ↑ | EnemyData.splitterChildData 별도 prefab Pool |
| Bomber 텔레그래프 시각 — Hovl 자산 활용 가능 | 5033 Hovl Magic Effects FREE 답습 (warning prefab) |
| WaveComposition SO 다수 신규 | 시드 도구 (5 챕터 시범 + 점진 확대) |
| OOP 재설계 — 회귀 위험 | NUnit 8+ + 사용자 Play Mode |

## 9. 후속 backlog

- **5035.1** 추가 적 특성 (Healer / Summoner / Armored / Knockback Resist)
- **5035.2** 35 챕터 모두 적용 (D7=A → 점진)
- **5035.3** Endgame / 무한 다락방 의 WaveComposition reuse

## 10. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-07 | 초기 — IEnemyBehavior + IDeathReaction Strategy + 5 신규 적 + Composition SO |
