# Boss Attack Pattern v1 (옵션 A — 카테고리 풀)

> **2026-05-02 사용자 결정**: 옵션 A (카테고리 풀) + CH6~35 (30 보스) + 2~3 사이클 분할.

## 1. 디자인 원칙

### 1.1 적용 범위
- **챕터 보스** (Wave 20) CH6~35 = 30 보스
- **미니보스** (Wave 5/10/15) 카테고리 풀 — 후속 사이클
- **Daily 7 + Weekly 4** = 11 — 후속 사이클
- **CH1~5 보스 변경 X** (초기 학습 곡선 보호)

### 1.2 카테고리 매핑
| 카테고리 | 챕터 | 패턴 | 핵심 동작 |
|---|---|---|---|
| **Animal** | CH13/15/16/19/20 | **Charge** | 1.5s telegraph → 0.5s 직선 돌진 |
| **Civ** | CH21~28 | **Pulse** | 3s 주기 광역 폭발 (반경 5) |
| **Fantasy** | CH14/17/29~35 | **Spread** | 5발 부채꼴 탄막 |
| **Meta** | CH1~5 (변경 X) | **Summon** | 작은 적 3마리 소환 |
| **Nature** | CH6~12/18 | **Vine** | 4 영역 식물 함정 (지속 데미지) |
| **Daily Boss** | 일일 7 | **Tornado** | 회전 영역 (5초 지속) |
| **Weekly Boss** | 주간 4 | **BlackHole** | 흡인 + 폭발 |

## 2. 시스템 설계

### 2.1 IBossAttackPattern interface
```csharp
public interface IBossAttackPattern
{
    void Initialize(EnemyController owner, EnemyData data);
    void Tick(float deltaTime);
    void Cleanup();
}
```

### 2.2 EnemyData 확장
```csharp
[Header("보스 공격 패턴 (CH6+ chapter boss)")]
public BossAttackType bossAttackType = BossAttackType.None;
public float bossAttackCooldown = 4f;
public float bossAttackDamage = 20f;
public float bossAttackRange = 5f;  // 패턴별 의미
```

### 2.3 EnemyController hook
- Initialize 시 `data.isBoss && bossAttackType != None` → IBossAttackPattern 인스턴스 + Initialize
- Update (또는 EnemyAnimationManager 의 Tick 콜백) 매 프레임 Tick(deltaTime)
- Die 시 Cleanup

## 3. 7 패턴 구현 (2 사이클 분할)

### 사이클 1 (이번): 2 패턴
1. **ChargeAttack** (Animal)
   - cooldown 4s
   - 1.5s telegraph: sprite color 적색 flash + ScaleX × 1.2
   - 0.5s 돌진: transform.position 직선 이동 (player 방향, 속도 12)
   - 충돌 시 PlayerHealth.Damage(bossAttackDamage)
   - 돌진 끝 → cooldown reset

2. **PulseAttack** (Civ)
   - cooldown 3s
   - 0.8s telegraph: sprite color 청색 flash + 반경 표시 (별도 telegraph circle 후속)
   - 폭발: Physics2D.OverlapCircle (radius 5) → Player 데미지
   - 자동 reset

### 사이클 2: 3 패턴
3. **SpreadAttack** (Fantasy) — 5발 부채꼴
4. **SummonAttack** (Meta) — 작은 적 3마리 소환
5. **VineAttack** (Nature) — 4 영역 식물 함정

### 사이클 3: 2 패턴 + 마무리
6. **TornadoAttack** (Daily) — 회전 영역
7. **BlackHoleAttack** (Weekly) — 흡인 + 폭발
8. **telegraph 시각 효과** (warning circle prefab + fade-in/out)
9. **카테고리 매핑 SO 갱신** (CH6~35 30 보스 자동 매핑)
10. **BalanceConfig** — 보스 cooldown/damage 곡선

## 4. 시각 effect

### 4.1 Telegraph (warning) — MVP
- sprite color 적색 flash (Color.red lerp 0.5)
- scale 1.2 → 1.0 fade
- (후속) 반경 circle prefab 별도

### 4.2 Hit feedback
- 적 처치 별 흩어짐 (기존 VfxPool)
- 보스 공격 적중 — PlayerHealth.RaisePlayerHit (기존)

## 5. 위험·완화

| 위험 | 완화 |
|---|---|
| 1500 동시 적 + 보스 30개 active 시 GC | 보스 1개만 active (chapter 보스) — 안전 |
| 패턴 cooldown 동기 | 보스별 다른 cooldown phase (random offset) |
| Player 가드 시각 X | telegraph 1.5s 충분 (player 회피 가능) |

## 6. 다음 사이클

- 사이클 2: Spread + Summon + Vine + EnemyController hook
- 사이클 3: Tornado + BlackHole + telegraph 시각 효과 + 카테고리 매핑 SO 갱신
