# T02: Seeder + Factory + Validator Rule 신규

> **레이어**: Editor / **의존**: 없음 / **예상**: 35분 / 1 commit
> **PRD 출처**: v2 §2.1 §2.2

## 1. 작업 항목

### 1.1 신규 — `Scripts/Editor/ChapterVariantSeeder.cs` (~60L)

진입점 + WireChapter + LoadChapterByNumber. PRD v2 §2.1 A 코드 답습.

### 1.2 신규 — `Scripts/Editor/VariantSoFactory.cs` (~60L)

EnsureAllVariants + EnsureBase + 6 Ensure<Type>. PRD v2 §2.1 B 코드 답습.

### 1.3 신규 — `Scripts/Editor/ProjectValidator/EnemyVariantConsistencyRule.cs` (~50L)

IValidationRule 구현. Splitter + Projectile 검사. PRD v2 §2.2 코드 답습.

### 1.4 ProjectValidator orchestrator 등록

`ProjectValidator.cs` 의 룰 list 에 `EnemyVariantConsistencyRule` 추가 (1 줄 변경).

## 2. 검증 기준

- 컴파일 errors=0
- ProjectValidator 메뉴 실행 시 신규 룰 호출 (warnings 변동 — 5 archetype SO 가 아직 없으므로 warning 0)

## 3. commit 메시지

```
5037 T02 — ChapterVariantSeeder + VariantSoFactory + EnemyVariantConsistencyRule (1/5)

[변경]
  - Editor/ChapterVariantSeeder.cs 신규 (60L) — 진입점 + WireChapter + LoadChapterByNumber
  - Editor/VariantSoFactory.cs 신규 (60L) — EnsureAllVariants + 6 archetype
  - Editor/ProjectValidator/EnemyVariantConsistencyRule.cs 신규 (50L) — Splitter+Projectile 일관성
  - ProjectValidator.cs 룰 list 에 신규 룰 등록 (1줄)

[검증]
  - 컴파일 errors=0
  - §15.6 100L 룰 — Seeder/Factory/Rule 모두 정합

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```
