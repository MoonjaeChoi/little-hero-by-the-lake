# T01: Sprint 1.1 — CharacterPool SO 클래스

> **레이어**: Data
> **의존성**: 없음 (시작 태스크)
> **예상 시간**: 15분 / 1 commit
> **PRD 출처**: v2 §2.1

---

## 1. 작업 항목

### 1.1 신규 파일

**경로**: `LittleHero/Assets/_Project/Scripts/Data/CharacterPool.cs`

**클래스 명세** (CLAUDE.md §15.4 SO 우선 + §15.6 100줄):

```csharp
using UnityEngine;

namespace LittleHero
{
    [CreateAssetMenu(menuName = "Little Hero/Character Pool")]
    public class CharacterPool : ScriptableObject
    {
        public CharacterData[] characters;

        public CharacterData FindById(string id)
        {
            if (string.IsNullOrEmpty(id) || characters == null) return null;
            for (int i = 0; i < characters.Length; i++)
                if (characters[i] != null && characters[i].id == id) return characters[i];
            return null;
        }
    }
}
```

**예상 라인**: ~17 (100줄 룰 ✅)

---

## 2. 검증 기준

### 2.1 컴파일
- `mcp__UnityMCP__refresh_unity` 후 5~10s 대기
- `mcp__UnityMCP__read_console` errors=0

### 2.2 §15 정합
- §15.4 SO 우선 — `[CreateAssetMenu]` 명시
- §15.6 100줄 룰 — 17L
- §15.11 안티 패턴 — public 필드 (`characters`) — SO 패턴 예외 허용 (§5)
- §15.1 SRP — 단일 책임 (id → CharacterData lookup)

### 2.3 회귀
- 기존 CharacterData 사용처 영향 X (신규 클래스만)

---

## 3. commit 메시지

```
5021 Sprint 1.1 / T01 — CharacterPool SO 클래스 (1/5)

[변경]
  + Data/CharacterPool.cs (~17L)
    - CreateAssetMenu "Little Hero/Character Pool"
    - characters[] 배열 + FindById(string) 헬퍼

[검증]
  - 컴파일 errors=0
  - §15.6 100줄 룰 ✅ (17L)
  - §15.4 SO 우선 정합

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 4. 관련 문서

- PRD v2 §2.1
- 디자인 §4.2 옵션 2 (CharacterPool SO 인덱스)
- decisions D1=B (5021 cascade)

---

## 5. 알려진 위험

| 위험 | 완화 |
|---|---|
| 기존 CharacterData lookup path 와 충돌 (예 LimitedShopService) | grep "CharacterData" 으로 lookup 패턴 검사 — 충돌 시 별도 보고 |
| `[CreateAssetMenu]` path 충돌 | "Little Hero/" prefix 정합 (다른 SO 와 통일) |
