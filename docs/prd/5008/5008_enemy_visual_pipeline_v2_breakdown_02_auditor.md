# T02 — EnemySpriteAuditor (D1=C 복원 패턴) + V2 폴백 검증 (D2=C)

> **레이어**: Logic (Editor 정적 도구)
> **의존성**: 없음
> **예상 시간**: 45분 / 1 commit
> **PRD 출처**: `5008_enemy_visual_pipeline_v2_v2.md` §2 Sprint 2

## 1. 작업 항목

### 1.1 신규 파일

#### 1.1.1 `LittleHero/Assets/_Project/Scripts/Editor/EnemySpriteAuditor.cs`

**클래스 명세** (D1=C: AssetImporter.SetReadable + try/finally + GetPixels32 + 복원):

```csharp
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LittleHero.Editor {
    /// <summary>
    /// 5008 — Enemy sprite 품질 audit. AssetImporter 임시 readable 복원 패턴 (D1=C).
    /// V2 archetype 폴백 검증 (D2=C) → Sprint 5.5 활성화 결정 input.
    /// </summary>
    public static class EnemySpriteAuditor {
        private const string EnemyRoot = "Assets/_Project/Art/Generated_v2/Enemies";
        private const float TransparentMin = 0.30f;
        private const float TransparentMax = 0.70f;
        private const float TransparentDamageMin = 0.05f;
        private const float TransparentDamageMax = 0.95f;

        [MenuItem("Tools/LittleHero/Audit Enemy Sprite Quality")]
        public static void AuditAll() {
            var entries = new List<AuditEntry>();
            foreach (var path in EnumeratePngPaths(EnemyRoot)) {
                entries.Add(AuditOne(path));
            }
            var fallbackOk = ValidateV2ArchetypeFallback(entries);
            WriteReport(entries, fallbackOk);
        }

        private static AuditEntry AuditOne(string path) {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            var origReadable = importer.isReadable;
            try {
                if (!origReadable) {
                    importer.isReadable = true;
                    importer.SaveAndReimport();
                }
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                var ratio = ComputeTransparentRatio(tex.GetPixels32());
                return new AuditEntry { path = path, transparentRatio = ratio };
            } finally {
                if (!origReadable) {
                    importer.isReadable = false;
                    importer.SaveAndReimport();  // 복원
                }
            }
        }

        private static float ComputeTransparentRatio(Color32[] pixels) {
            int transparent = 0;
            for (int i = 0; i < pixels.Length; i++) {
                if (pixels[i].a < 16) transparent++;
            }
            return (float)transparent / pixels.Length;
        }

        private static bool ValidateV2ArchetypeFallback(List<AuditEntry> entries) {
            // Chapter override 손상 시 V2 시스템이 archetype 으로 폴백하는지 검증
            // (D2=C) — PhaseFContentSeed 의 EnemyData 의 sprite 참조 확인
            // archetype 28/30 정상 + chapter override 손상 → fallback OK
            // archetype 손상 시 → fallback 깨짐 → Sprint 5.5 강제 활성화
            // 구현: EnemyData SO 인스턴스 enumerate → sprite 참조 해상도 검증
            // 실제 코드는 구현 시 EnemyData SO 정확한 필드 확인 후
            return true; // placeholder
        }

        private static IEnumerable<string> EnumeratePngPaths(string root) {
            var fullPath = Path.Combine(Application.dataPath, root.Substring("Assets/".Length));
            foreach (var f in Directory.GetFiles(fullPath, "*.png", SearchOption.AllDirectories)) {
                yield return "Assets/" + f.Substring(Application.dataPath.Length + 1).Replace('\\', '/');
            }
        }

        private static void WriteReport(List<AuditEntry> entries, bool fallbackOk) {
            // docs/little_hero/enemy_sprite_audit_report.md 자동 생성
            // 손상 list + 카테고리 통계 + V2 fallback 결과
        }

        private struct AuditEntry {
            public string path;
            public float transparentRatio;
        }
    }
}
```

**예상 라인**: ~85L (100줄 이내 ✅, §15.6 준수)

### 1.2 수정 파일

(없음 — Editor 도구, 기존 코드 영향 X)

## 2. 검증 기준

### 2.1 컴파일
- `mcp__UnityMCP__refresh_unity` 후 errors=0
- 5초 대기 (도메인 리로드)
- `editor_state.isCompiling == false` 폴링

### 2.2 메뉴 동작
- `Tools/LittleHero/Audit Enemy Sprite Quality` 메뉴 등장
- 실행 시 `docs/little_hero/enemy_sprite_audit_report.md` 자동 생성
- 손상 9 PNG 자동 검출 (이전 식별 list 와 매칭)

### 2.3 D1=C 복원 검증
- audit 실행 전후 모든 PNG 의 TextureImporter.isReadable 변동 0
- (자동 검증) 임의 1 PNG 의 isReadable 변동 0 확인

### 2.4 D2=C 폴백 검증
- report 에 "V2 archetype fallback: OK / 깨짐" 명시
- fallback 깨짐 시 → Sprint 5.5 활성화 사용자 결정 input

### 2.5 ProjectValidator
- `Tools/LittleHero/Validate Project` 실행 → errors=0
- 신규 warnings ≤ 0 (200줄 룰 통과, 100L 이내)

## 3. commit 메시지

```
5008 Sprint 2 / T02 — EnemySpriteAuditor + V2 폴백 검증 (2/6)

[변경]
- Scripts/Editor/EnemySpriteAuditor.cs 신규 (~85L)
- 구현 방식 (D1=C): AssetImporter.SetReadable + try/finally + GetPixels32 + 복원
- V2 archetype 폴백 검증 추가 (D2=C)
- docs/little_hero/enemy_sprite_audit_report.md 자동 생성 (262 PNG 품질 snapshot + fallback 결과)

[검증]
- 컴파일 errors=0
- 손상 9 PNG 자동 검출 (이전 식별 list 와 매칭)
- 임포트 설정 변동 0 (D1=C 복원 검증)
- V2 archetype 폴백 검증 report 정상 (D2=C)
- §15.6 100L / §15.12 Validator 룰 역할

[D2 분기 input]
- fallback OK 시: T05.5 미활성화, 5009 anchor 만
- fallback 깨짐 시: T05.5 활성화 (사용자 결정)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 4. 관련 문서

- PRD §2 Sprint 2
- 디자인 §3.1 신규 파일 (EnemySpriteAuditor)
- decisions D1=C (구현 방식) + D2=C (폴백 검증)

## 5. 알려진 위험

- **AssetImporter.SaveAndReimport 비용** → 임시 readable 변경 시 전체 reimport 비용 ↑. 262 PNG 일괄 audit 시 ~1~2분 소요 가능. 사용자 메뉴 1회 실행 → 허용
- **try/finally 복원 누락** → 컴파일 시 강제 (try 블록 내 throw 시 finally 무조건 실행)
- **V2 archetype 폴백 검증 정확성** → EnemyData SO 의 sprite 참조 필드 정확히 확인 필요. 구현 시 EnemyData 클래스 grep
- **§15.6 100L 위반 위험** → ~85L 추정. 만약 80L 초과 시 Helper 메서드 분리 가능
