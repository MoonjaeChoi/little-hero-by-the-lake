# T03: ElementVfxBankTests NUnit + changelog + CLAUDE.md

> **레이어**: Test + Docs
> **의존성**: T02 (ElementVfxBank.Spawn 완성)
> **예상 시간**: ~15분 / 1 commit
> **PRD 출처**: `5033_mage_element_vfx_integration_v2.md` § 4 (Sprint 3)

---

## 1. 작업 항목

### 1.1 신규 파일 — `Tests/Editor/ElementVfxBankTests.cs` (선택)

**경로**: `LittleHero/Assets/_Project/Tests/Editor/ElementVfxBankTests.cs`

**테스트 명세**:
```csharp
using NUnit.Framework;
using UnityEngine;

public class ElementVfxBankTests
{
    [Test]
    public void GetPrefab_Fire_NotNull()
    {
        // Arrange + Act
        var prefab = ElementVfxBank.GetPrefab(MagicElement.Fire);

        // Assert
        Assert.IsNotNull(prefab, "Resources/VFX/Element/Fire.prefab 가 로드되지 않았습니다 (T01 누락 확인).");
    }

    [Test]
    public void GetPrefab_None_ReturnsNull()
    {
        var prefab = ElementVfxBank.GetPrefab(MagicElement.None);
        Assert.IsNull(prefab);
    }

    [Test]
    public void Spawn_None_DoesNotThrow()
    {
        // Spawn(None) 은 early return — exception 미발생
        Assert.DoesNotThrow(() => ElementVfxBank.Spawn(MagicElement.None, Vector3.zero));
    }
}
```

**예상 라인**: ~25L (100줄 이내 ✅)
**NUnit 누적**: 178 → 181 (+3)

**참고**:
- `GetPrefab_Fire_NotNull` — T01 의 Resources Load + EnsureLoaded lazy 검증 (D3=B 정합)
- `GetPrefab_None_ReturnsNull` — switch default null 분기 검증
- `Spawn_None_DoesNotThrow` — None early return 가드

**제외 테스트** (런타임/Play Mode 의존):
- `Spawn_Fire_InstantiatesGameObject` — Instantiate 시 active GameObject Hierarchy 추가 → Editor Test 환경 부적합 (Play Mode Test 별도 필요)
- `Spawn_Destroys_AfterLifetime` — Destroy 지연 검증 시 yield return new WaitForSeconds(3f) 필요 → Editor Test 부적합

### 1.2 갱신 파일 — `docs/little_hero/changelog_v1.md`

**경로**: `/Users/memmem/git/little-hero-by-the-lake/docs/little_hero/changelog_v1.md`

**추가 항목** (상단 또는 5032 다음):
```markdown
- 2026-05-06: **5033 mage_element_vfx_integration** — Hovl Magic Effects FREE 4 element prefab 통합 (PGE 6-Phase, 3 commit + Phase 1~4 docs). **사용자 결정**: D1=B Editor 도구 + D2=A Sprint cascade GC + D3=B EnsureLoaded lazy. **PGE Phase 2**: Critique 8.7/10 → Mediator ai-assisted 권장 3/3 채택. **신규 파일**: Editor/SetupElementVfxVariants.cs (~50L, Tools/LittleHero 메뉴) + Resources/VFX/Element/{Fire,Ice,Lightning,Arcane}.prefab (Hovl variant). **수정**: Core/ElementVfxBank.cs (R1 SerializeField static 제거 + R2 SPAWN_LIFETIME_SECONDS const + D3=B EnsureLoaded lazy + Spawn Hovl Instantiate) + Gameplay/Combat/ElementResolver.cs (ApplyElementEffect 진입 시 Spawn hook). **결과**: 5029 placeholder (StarBurst) → 실 Hovl prefab 통합, ElementResolverTests 6 NUnit 회귀 0, NUnit 178 → 181 (+3). **D2=A 인지**: Instantiate + Destroy GC 발생 → 5033.1 후속 cascade VfxPool Register/Spawn 정착 예정.
```

### 1.3 갱신 파일 — `CLAUDE.md` §14 변경 이력

**경로**: `/Users/memmem/git/little-hero-by-the-lake/CLAUDE.md`

**§14 직전 5건 갱신** (가장 오래된 1건 제거 + 5033 항목 추가, 최신순):

```markdown
- 2026-05-06: **5033 mage_element_vfx_integration** — Hovl Magic Effects FREE 4 element prefab 통합 (PGE 6-Phase, 3 commit). **D1=B Editor 도구 + D2=A Sprint cascade + D3=B EnsureLoaded lazy**. Critique 8.7/10 → ai-assisted 권장 3/3 채택. **신규**: Editor/SetupElementVfxVariants.cs + Resources/VFX/Element/4 prefab. **수정**: ElementVfxBank (R1+R2+D3=B) + ElementResolver hook. **결과**: 5029 placeholder (StarBurst) → Hovl variant, ElementResolverTests 6 회귀 0, NUnit 178 → 181 (+3). 5033.1 backlog: VfxPool Register/Spawn (D2=A cascade).
- 2026-05-06: **5032 unity6_api_migration** — ... (기존 그대로)
- 2026-05-06: **5031 iap_v5_migration** — ... (기존 그대로)
- 2026-05-06: **5029 mage_element_system** — ... (기존 그대로)
- 2026-05-06: **5028 oop_class_split_phase2** — ... (기존 그대로)
```

(5027 oop_class_split 제거 — 5건 maintain 유지)

또한 §11 마일스톤에 항목 추가 검토:
```markdown
| **mage_element_vfx_integration** | Hovl Magic Effects FREE 4 element prefab 통합 + Editor 도구 + GC cascade backlog (5033, 3 commit) | ✅ 2026-05-06 |
```

---

## 2. 검증 기준

### 2.1 컴파일
- T03 commit 전: `mcp__UnityMCP__refresh_unity` + 5초 대기
- `read_console` errors=0
- ElementVfxBankTests.cs 컴파일 성공

### 2.2 Editor Test
- Window > General > Test Runner > EditMode 탭
- `ElementVfxBankTests` 클래스 3 PASS:
  - `GetPrefab_Fire_NotNull` PASS
  - `GetPrefab_None_ReturnsNull` PASS
  - `Spawn_None_DoesNotThrow` PASS
- NUnit 누적 178 → 181 (+3)
- 회귀 — 기존 178 NUnit 모두 PASS (변동 X)

### 2.3 Reflection (Phase 6 Contract)
- Type `ElementVfxBankTests` 존재
- Method `GetPrefab_Fire_NotNull/GetPrefab_None_ReturnsNull/Spawn_None_DoesNotThrow` 모두 `[Test]` attribute

### 2.4 Docs 검증
- `changelog_v1.md` — 5033 항목 존재
- `CLAUDE.md` §14 — 5033 5번째에서 1번째로 진입 + 5028 까지 5건 유지
- `CLAUDE.md` §11 — mage_element_vfx_integration 마일스톤 추가 (선택)

---

## 3. commit 메시지

```
5033 Sprint 3 — NUnit 3 + changelog + CLAUDE.md (3/3)

[변경]
  + Tests/Editor/ElementVfxBankTests.cs (NUnit 3 신규)
    - GetPrefab_Fire_NotNull (Resources Load 검증)
    - GetPrefab_None_ReturnsNull (default 분기)
    - Spawn_None_DoesNotThrow (None early return 가드)
  - changelog_v1.md +5033
  - CLAUDE.md §14 5건 maintain (5027 → 5033 진입)
  - CLAUDE.md §11 mage_element_vfx_integration 마일스톤 추가

[NUnit 누적] 178 → 181 (+3)

[5033 종결]
  - Hovl Magic Effects FREE 통합 완료
  - 4 element 시각 효과 활성
  - 5029 placeholder → 실 prefab
  - PGE 6-Phase 사이클 완료 (Phase 5 종료, Phase 6 evaluate 진행)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 4. 관련 문서

- PRD v2 § 4 Sprint 3
- changelog-manager 스킬 (자동 호출 가능)

---

## 5. 알려진 위험

| 위험 | 완화 |
|---|---|
| ElementVfxBankTests 가 Editor Test 환경에서 Resources.Load 실패 | T01 의 4 prefab 이 Resources/VFX/Element/ 에 정확히 위치 — Editor 테스트는 AssetDatabase 직접 사용 가능 |
| `GetPrefab_Fire_NotNull` 가 EnsureLoaded 호출 순서로 인해 false 발생 | 첫 호출 시 _initialized 가 다른 테스트로 인해 true 일 수 있음 — 각 테스트 SetUp 에서 reflection 으로 _initialized=false 리셋 또는 `[OneTimeSetUp]` 사용 (선택) |
| CLAUDE.md §14 5건 maintain 위반 (4건 또는 6건) | 항목 추가 시 가장 오래된 1건 (5027) 제거 |

**SetUp 권장 (위험 2 완화)**:
```csharp
[SetUp]
public void Reset()
{
    var field = typeof(ElementVfxBank).GetField("_initialized",
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    field.SetValue(null, false);
}
```

---

## 6. Unity MCP 안전 패턴

```
1. ElementVfxBankTests.cs 작성 (Write 도구)
2. mcp__UnityMCP__refresh_unity
3. 5초 대기
4. read_console errors=0 확인
5. mcp__UnityMCP__run_tests (filter "ElementVfxBankTests")
   또는 Editor Test Runner 직접 실행 → PASS 확인
6. changelog_v1.md / CLAUDE.md Edit (Edit 도구)
7. (refresh 불필요 — 코드 변경 X)
8. git commit
9. PGE Phase 5 종료 → Phase 6 evaluate 진입
```
