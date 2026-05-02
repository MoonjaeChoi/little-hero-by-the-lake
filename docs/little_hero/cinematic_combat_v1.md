# Cinematic Combat v1 — PIVOT v4 (cap 300 + Premium 피격 + 데미지 수치)

> **목표**: 적 동시 cap 1,500 → 300 으로 축소하고, 절약된 frame budget 을 **Premium 피격 효과 + Critical 연출 + 데미지 수치 표시** 로 전환. Brotato / Survivor.io / Maplestory Idle 수준 폴리시.

## 0. 결정 이력

- 2026-05-03: 사용자 결정 — 옵션 E (cap 300 Cinematic) + 데미지 수치 표현
- PIVOT v3.1 (cap 1,500, 작은 무리 ×20) 폐기 → PIVOT v4 (cap 300, Premium 피격)

## 1. 합산 보존 — PIVOT v3.1 vs PIVOT v4

| 지표 | PIVOT v3.1 | PIVOT v4 | 비고 |
|---|---|---|---|
| 동시 cap | 1,500 | **300** | -80% |
| 챕터 마리수 | 20,000 | **4,000** | cap 비례 |
| 마리당 ★ | ×0.1 | **×0.5** | ×5 인상 |
| 마리당 XP | ×0.1 | **×0.5** | ×5 인상 |
| 마리당 드랍 | ×0.1 | **×0.5** | ×5 인상 |
| 챕터 ★ 합산 | 20,000 × 0.1 = 2,000 | 4,000 × 0.5 = **2,000** | ✅ 동일 |
| spawnIntervalMul | 0.05 (×20 빈도) | **0.15** (×6.7) | cap 300 정합 |
| enemyScaleMul | 0.3 | **0.5** | cap 적어 더 큰 적 OK (식별성 ↑) |

## 2. Frame budget 분석 (60fps = 16.67ms)

### 2.1 baseline (cap 300)

| 항목 | 비용 |
|---|---|
| EnemyAI Update (300) | ~0.3ms |
| EnemyAnimationManager | ~0.1ms |
| Rigidbody2D Physics | ~0.4ms |
| SpriteRenderer (SpriteAtlas SRP) | ~0.5ms |
| **baseline** | **~1.3ms** |

→ 여유 budget **15ms**, 피격 효과에 12ms 할당 가능.

### 2.2 Premium 피격 효과 비용 (마리당)

| 효과 | 비용 | 비고 |
|---|---|---|
| Punch scale (DOTween) | 0.10ms | tween pool 재사용 |
| Color flash (SRP Batcher) | 0.05ms | Material instance 회피 |
| Hit particle (VfxPool) | 0.20ms | 별 흩어짐 작은 |
| Hit sound (AudioMgr pool) | 0.50ms | 동시 16개 cap |
| Damage floating text | 1.00ms | 1 draw call/text |
| **Premium 1마리** | **1.85ms** | |
| Critical: slow-mo | 글로벌 0.05ms | timeScale 0.05s |
| Critical: screen shake | 글로벌 0.10ms | Cinemachine impulse |
| Critical: scale 1.7× | +0.05ms | DOTween punch |

→ **동시 6 피격** = 11ms (안전), **동시 4 + 크리 1** = 9.4ms (안전)

## 3. Premium 피격 패키지 — EnemyController.OnHit

### 3.1 일반 피격
1. **Punch scale** 1.0 → 1.4 → 1.0 (DOTween, 0.05s + 0.10s)
2. **Color flash** 흰 → 빨강 → 흰 (SpriteRenderer.color, 0.05s + 0.10s)
3. **Hit particle** 작은 별 3~5개 흩어짐 (VfxPool 재사용)
4. **Hit sound** AudioManager.SfxType.EnemyHit (절차적)
5. **Damage number** 흰 색, 중간 크기 (DamageNumberFloater)

### 3.2 Critical 피격 (PlayerStats.CritChance)
1. **Punch scale** 1.0 → **1.7** → 1.0 (확장 0.07s + 0.15s)
2. **Color flash** 흰 → **노랑** → 흰 (확장 0.07s + 0.15s)
3. **Hit particle** 큰 별 6~10개 + 골드 색조 (VfxPool 재사용 변형)
4. **Hit sound** AudioManager.SfxType.CritHit (절차적 합성, 신규)
5. **Damage number** 골드 + 큰 + 진동 (DOTween punch 0.5)
6. **Slow-mo** Time.timeScale = 0.05 → 1.0, 실시간 0.08s
7. **Screen shake** Cinemachine ImpulseSource amplitude 0.3
8. **(보스만)** Hit pause — 0.05s 글로벌 정지

## 4. 데미지 수치 표시 — DamageNumberFloater

### 4.1 시각 디자인

| 종류 | 색 | 크기 | 부가 효과 |
|---|---|---|---|
| 일반 데미지 | 흰 (#FFFFFF) | 24pt | 위로 1.0 unit, 0.6s |
| Critical | 골드 (#FFD700) | 36pt | 진동 + 위로 1.5 unit, 1.0s |
| DOT (지속) | 보라 (#A66BC9) | 20pt | 작은 fade |
| Heal (회복) | 녹색 (#4ADE80) | 24pt | + 기호 prefix |
| Block (방어) | 회색 (#888888) | 20pt | "0" 표시 |

### 4.2 위치 산정
- 시작: `enemy.position + (Random(-0.3, +0.3), +0.5, 0)` (적 머리 위, 약간 산포)
- 부동: 위로 1.0 unit (Critical: 1.5 unit)
- Fade out: 0.4s linear after delay 0.2s

### 4.3 수치 포맷
- `0~999`: `"100"` (숫자 그대로)
- `1,000~9,999`: `"1.2K"` (소수점 1자리 K 단위)
- `10,000+`: `"15K"` (정수 K)
- `1,000,000+`: `"1.5M"`

### 4.4 컴포넌트 구조
```
DamageNumberFloater (MonoBehaviour, WorldSpace)
├── Canvas (WorldSpace, sortOrder 100)
│   └── Text (UnityEngine.UI.Text Legacy) — fontSize 24, alignment Center
└── CanvasGroup (alpha fade)

API:
  Initialize(damage int, type DamageType, worldPos Vector3)
  - DamageType enum: Normal / Critical / DOT / Heal / Block
```

### 4.5 Pool 패턴
- `DamageFloaterPool` 정적 (VfxPool 답습)
- 풀 크기 50 (cap 300 × 동시 피격 ~10% × buffer 1.6)
- `Spawn(pos, dmg, type)` / `Return(floater)`
- prefab `Resources/UI/DamageNumberFloater.prefab`

## 5. CinematicEffects (글로벌 매니저)

```
CinematicEffects (MonoBehaviour Singleton, DontDestroyOnLoad)
├── PlayHitPause(duration float) — Time.timeScale 0.05 0.08s
├── PlayScreenShake(amplitude float) — CinemachineImpulseSource
└── PlayCriticalFlash(duration float) — 화면 가장자리 노란 vignette (옵션)

bootstrap: RuntimeInitializeOnLoadMethod
의존: Cinemachine 3.x ImpulseSource (CinemachineCamera 자식 또는 별도 GameObject)
```

## 6. 기존 시스템 hook

### 6.1 DamageSystem
```csharp
public static void DealDamage(EnemyController enemy, float damage, bool isCrit) {
    enemy.TakeDamage(damage);
    enemy.OnHit(damage, isCrit);  // ← 신규
}
```

### 6.2 DamageCalculator (이미 isCrit out 파라미터 있음)
- `RollDamage(...)` 의 `out bool isCrit` 그대로 활용
- PlayerStats.CritChance (기본 5%, stats_v1 §3) 으로 확률 결정

### 6.3 EnemyController
```csharp
public void OnHit(float damage, bool isCrit) {
    PlayPunchAndFlash(isCrit);
    VfxPool.SpawnHitStar(transform.position, isCrit);
    AudioManager.Instance?.Play(isCrit ? SfxType.CritHit : SfxType.EnemyHit);
    DamageFloaterPool.Spawn(transform.position + Vector3.up * 0.5f,
        Mathf.RoundToInt(damage),
        isCrit ? DamageType.Critical : DamageType.Normal);
    if (isCrit) {
        CinematicEffects.Instance?.PlayHitPause(0.08f);
        CinematicEffects.Instance?.PlayScreenShake(0.3f);
    }
}
```

## 7. BalanceConfig PIVOT v4 갱신

```csharp
[Header("PIVOT v4 — Cinematic Combat")]
public float enemyHpMul = 0.3f;          // PIVOT v3.1 그대로 (식별성)
public float enemyAtkMul = 0.3f;         // PIVOT v3.1 그대로
public float enemyScaleMul = 0.5f;       // PIVOT v3.1 0.3 → 0.5 (cap 적어 큰 적 OK)
public float spawnIntervalMul = 0.15f;   // PIVOT v3.1 0.05 → 0.15 (cap 비례)
public float enemyStarDustMul = 0.5f;    // PIVOT v3.1 0.1 → 0.5 (×5 인상)
public float enemyXpMul = 0.5f;          // PIVOT v3.1 0.1 → 0.5
public float enemyDropChanceMul = 0.5f;  // PIVOT v3.1 0.1 → 0.5
```

## 8. WaveManager 갱신

```csharp
private const int MAX_CONCURRENT_ENEMIES = 300;  // PIVOT v4 (1500 → 300)
```

## 9. 신규 파일

```
Scripts/
├── Combat/
│   ├── CinematicEffects.cs      ← Singleton hit pause + screen shake
│   ├── DamageNumberFloater.cs   ← MonoBehaviour, WorldSpace Canvas
│   ├── DamageFloaterPool.cs     ← 정적 pool (VfxPool 패턴)
│   └── DamageType.cs            ← enum Normal/Critical/DOT/Heal/Block
└── Resources/UI/
    └── DamageNumberFloater.prefab
```

## 10. 변경 파일

```
- Scripts/Data/BalanceConfig.cs    — PIVOT v4 multipliers
- Scripts/Gameplay/Waves/WaveManager.cs  — cap 300
- Scripts/Gameplay/Combat/DamageSystem.cs  — OnHit hook
- Scripts/Gameplay/Enemies/EnemyController.cs  — OnHit() Premium 패키지
- Scripts/Core/AudioManager.cs     — SfxType.EnemyHit, CritHit 추가
- Scripts/Core/VfxPool.cs          — SpawnHitStar(pos, isCrit) 추가
```

## 11. 시뮬레이션 영향

- **챕터 5분 클리어 가정**: PIVOT v3.1 20,000 / 5분 = 67마리/초 → PIVOT v4 4,000 / 5분 = **13마리/초**
- **별가루 합산**: 보존 (×5 인상)
- **frame budget**: cap 300 baseline 1.3ms → Premium 피격 동시 6 = 12.4ms ✅
- **GC pressure**: 1500 → 300 으로 ObjectPool / EnemyAnimationManager 부담 ↓

## 12. UX 가이드

### 12.1 작은 무리감 보존
- cap 300 만으로 시각이 한산해 보일 수 있음
- 적 size 0.3 → 0.5 (식별성 ↑) + 색 진하게 (saturated palette)
- spawn 간격 짧게 (0.15 multiplier 그대로) → 화면 출현 빈도 유지

### 12.2 처치 만족감
- Premium 피격 (×5 효과) + Critical (5% 확률) → 처치 1회당 시각·청각 풍부
- "마리당 가치 ×5" 인플레이션 = 처치 1회의 보상도 ×5 → 도파민 동기 ↑

### 12.3 데미지 수치 가독성
- 화면 가득 데미지 텍스트 = 노이즈 위험 → cap 300 + Critical 5% 만 큰 텍스트 + 일반 작게
- 동일 위치 중복 시 Y offset stagger (Random 산포)

## 13. 변경 이력

- 2026-05-03: 초기 작성 (PIVOT v4 cinematic_combat_v1)

## 14. 다음 단계

1. **본 문서 사용자 검토** + 결정 (즉시 구현 / 추가 조정)
2. **구현 사이클 1** (1 commit): BalanceConfig + WaveManager cap + DamageType enum + DamageNumberFloater + Pool + Resources prefab
3. **구현 사이클 2** (1 commit): EnemyController.OnHit + CinematicEffects + AudioManager 신규 SFX + VfxPool.SpawnHitStar + DamageSystem hook
4. **검증**: Play 모드 진입 → 적 처치 시 punch + flash + 숫자 표시 + Critical slow-mo
