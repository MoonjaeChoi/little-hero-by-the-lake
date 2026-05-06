# T02: ElementVfxBank.Spawn + ElementResolver hook

> **레이어**: Logic
> **의존성**: T01 (ElementVfxBank.GetPrefab + 4 prefab + SPAWN_LIFETIME_SECONDS const)
> **예상 시간**: ~30분 / 1 commit
> **PRD 출처**: `5033_mage_element_vfx_integration_v2.md` § 3 (Sprint 2)

---

## 1. 작업 항목

### 1.1 갱신 파일 — `Core/ElementVfxBank.cs` (Spawn 메서드 추가)

**경로**: `LittleHero/Assets/_Project/Scripts/Core/ElementVfxBank.cs`

**추가 메서드**:
```csharp
public static void Spawn(MagicElement element, Vector3 position)
{
    if (element == MagicElement.None) return;
    var prefab = GetPrefab(element);  // EnsureLoaded 자동 호출
    if (prefab == null) return;
    var instance = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity);
    UnityEngine.Object.Destroy(instance, SPAWN_LIFETIME_SECONDS);  // R2: const 사용
}
```

**5029 기존 Spawn 시그니처**: `public static void Spawn(MagicElement element, Vector3 position)` 유지 (placeholder StarBurst → Hovl prefab 으로 본체만 변경).

**§15 정합**:
- §15.4 Magic Number 0 — SPAWN_LIFETIME_SECONDS 사용 ✅
- §15.11 — Instantiate + Destroy GC 발생 (D2=A 결정 — 5033.1 cascade)

**예상 라인**: ElementVfxBank 누적 ~85L (100줄 이내 ✅)

### 1.2 갱신 파일 — `Gameplay/Combat/ElementResolver.cs`

**경로**: `LittleHero/Assets/_Project/Scripts/Gameplay/Combat/ElementResolver.cs`

**추가 hook** (`ApplyElementEffect` 진입부):
```csharp
public static void ApplyElementEffect(EnemyController target, MagicElement element, float baseDamage)
{
    if (target == null || target.IsDead) return;
    if (element == MagicElement.None) return;

    // 5033 — element VFX spawn (placeholder StarBurst → Hovl prefab)
    ElementVfxBank.Spawn(element, target.transform.position);

    // 5029 기존 element 효과 (변동 X)
    switch (element)
    {
        case MagicElement.Fire:
            // ... 기존 코드
            break;
        // ...
    }
}
```

**5029 보존**:
- `ApplyElementEffect` 시그니처 변동 X
- switch 분기 4건 (Fire/Ice/Lightning/Arcane) 변동 X
- 5029 ElementResolverTests 6 NUnit 회귀 0

**갱신**: 메서드 진입부에 `ElementVfxBank.Spawn(element, target.transform.position);` 1 줄 추가만.

---

## 2. 검증 기준

### 2.1 컴파일
- T02 commit 전: `mcp__UnityMCP__refresh_unity` + 5초 대기
- `read_console` errors=0
- ElementVfxBank.cs / ElementResolver.cs 컴파일 성공

### 2.2 Reflection (Phase 6 Contract 미리보기)
- Method `ElementVfxBank.Spawn(MagicElement, Vector3)` (public static void) 존재
- Method `ElementResolver.ApplyElementEffect(EnemyController, MagicElement, float)` 시그니처 5029 보존
- ElementResolver.ApplyElementEffect 본문에 `ElementVfxBank.Spawn` 호출 존재 (정적 분석)

### 2.3 ElementResolverTests 회귀
- 5029 NUnit 6 모두 PASS
- 시그니처 변동 X — 컴파일 통과 시 회귀 0

### 2.4 Play Mode 시각 검증 (사용자 보고)
- **시나리오 2 (PRD § 7)**: 마법사 캐릭터 → CH1 진입 → weapon_firebottle (Fire) 활성화 → 적 처치 시 → Red energy explosion VFX 발동 → SPAWN_LIFETIME_SECONDS (3s) 후 자동 사라짐
- **시나리오 3 (PRD § 7)**: URP 2D 호환 — 분홍/검정 표시 시 미합격 → `Edit > Render Pipeline > Universal RP > Convert URP Materials`
- **시나리오 4 (PRD § 7)**: PG-12 톤 — Red energy explosion 만화풍 합격 / 사실적 미합격

### 2.5 메모리 누수 검증
- Profiler — Instantiate 후 SPAWN_LIFETIME_SECONDS 후 GameObject 풀에서 사라짐 (Destroy 동작 확인)
- 적 ~100 처치 후 Hierarchy "Element_*" 잔존 GameObject 0

### 2.6 ProjectValidator
- warnings 변동 0

---

## 3. commit 메시지

```
5033 Sprint 2 — ElementVfxBank.Spawn 분기 + ElementResolver hook (2/3)

[변경]
  - Core/ElementVfxBank.cs
    + Spawn(MagicElement, Vector3) — Hovl prefab Instantiate + SPAWN_LIFETIME_SECONDS Destroy
    (T01 의 GetPrefab + EnsureLoaded 활용)
  - Gameplay/Combat/ElementResolver.cs
    + ApplyElementEffect 진입 시 ElementVfxBank.Spawn 호출 (1줄 추가)

[검증]
  - 마법사 무기 Fire/Ice/Lightning/Arcane 활성화 시 Hovl prefab 발동
  - 메모리 누수 0 (SPAWN_LIFETIME_SECONDS Destroy)
  - 5029 ElementResolverTests 6 NUnit 회귀 0
  - 컴파일 errors=0

[D2=A 인지] Instantiate + Destroy GC 발생 — 5033.1 후속 cascade VfxPool 정착 예정

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 4. 관련 문서

- PRD v2 § 3 Sprint 2
- 디자인 § 3.2 Spawn / § 3.4 §15 정합
- Decisions D2=A (Sprint cascade GC 인지)
- Rebuttal R2 (SPAWN_LIFETIME_SECONDS const)

---

## 5. 알려진 위험

| 위험 | 완화 |
|---|---|
| Hovl prefab URP 2D 호환 (분홍/검정) | T02 commit 후 사용자 Play Mode 검증 → 미합격 시 Convert URP Materials |
| Instantiate + Destroy GC 누적 | D2=A 결정 — 5033.1 cascade VfxPool. PRD § 9 backlog 명시 |
| ElementResolver.ApplyElementEffect 호출 빈도 ↑ → Spawn 호출 빈도 ↑ | 적 처치당 1회만 호출 (DamageDealer.ApplyMulti 의 마지막 hit 일 때) — 검증 필요 |
| target.IsDead 체크 후 Spawn → IsDead 변경 시 race | 기존 5029 코드 그대로 (기존 NUnit 6 가드) |

---

## 6. Unity MCP 안전 패턴

```
1. ElementVfxBank.cs Spawn 메서드 추가 (Edit 도구)
2. ElementResolver.cs ApplyElementEffect hook 추가 (Edit 도구, 1 줄)
3. mcp__UnityMCP__refresh_unity
4. 5초 대기 (작은 변경 — domain reload 짧음)
5. read_console errors=0 확인
6. (선택) Play Mode 진입 → 사용자 시각 검증 시 사용자에게 보고
7. git commit
8. 다음 태스크 진입 전 5초 대기
```

---

## 7. 후속 태스크

T02 commit 후 사용자 Play Mode 검증 (시나리오 2/3/4) 권장. 미합격 시:
- 시나리오 3 미합격: Convert URP Materials 적용 → SetupElementVfxVariants 재실행 → T01 prefab 재생성 (하나의 commit cascade)
- 시나리오 4 미합격: PRD § 8 위험에 따라 prefab 4 GameObject ref 만 교체
