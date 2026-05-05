# 5018 — iOS Texture Compression — App 용량 cleanup (디자인)

> **목적**: Mac Standalone 894 MB / iOS App Store 임계 150 MB 위반 6배 → 일괄 Texture compression + Crunched + Platform override 적용으로 ~140 MB 도달.
> **우선순위**: ★★★ (TestFlight 진입 임계 — Phase 4 마일스톤 필수)

## 0. 결정 이력

- 2026-05-05: 5016 D2=A Player 빌드 측정 시 Mac 894 MB / Development 1,001 MB 발견
- 2026-05-06: sharedassets0.assets.resS = **798 MB (89% 비중)** — Texture compression 미적용 원인 식별
- 2026-05-06: 5018 — Texture import settings 일괄 변경 PRD (본 문서)

## 1. 사전조사 결과

### 1.1 Mac Player 빌드 용량 분포

| 영역 | 용량 | 비율 |
|---|---:|---:|
| **sharedassets0.assets.resS** (sprite/sound) | **798 MB** | **89%** |
| Managed (.NET) | 35 MB | 4% |
| Frameworks (Burst native) | 33 MB | 4% |
| 기타 | ~28 MB | 3% |
| **합계** | **894 MB** | |

### 1.2 PNG sprite 분포

| 폴더 | 용량 | 개수 |
|---|---:|---:|
| **Enemies** | **174 MB** | 262 |
| Backgrounds | 83 MB | 70 (35 챕터 × 2) |
| UI | 18 MB | 30+ |
| Weapons | 9.9 MB | 18 |
| Bosses | 8.0 MB | 11 |
| 기타 (Characters/Frames/...) | 11 MB | 35+ |
| **합계 raw PNG** | **304 MB** | **786** |

### 1.3 Texture import 현재 설정 (대표)

`Enemy_Animal_Boss.png.meta`:
```yaml
platformSettings:
- buildTarget: DefaultTexturePlatform
  maxTextureSize: 2048
  textureCompression: 0       # ← None (비압축)
  compressionQuality: 50
  crunchedCompression: 0      # ← Crunched 비활성
  overridden: 0
```

**iOS override 없음** + **압축 0** + **Crunched 0** → Unity 가 RGBA32 비압축 또는 약한 압축 적용 → 798 MB.

### 1.4 SpriteAtlas

- `Art/Atlases/EnemyAtlas.spriteatlas` 존재 — 5009.4 EnemyAtlasConfigurer 가 8192 maxSize + Uncompressed 설정 (5008 sprite quality 보존 의도)
- 이 정책이 빌드 용량 부풀림 원인 — **iOS Override 로 압축 강제 필요**

## 2. 옵션 비교

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| **A (★권장)** | **iOS Override 로 ASTC + Crunched 압축** + DefaultPlatform 도 Compressed | 안전 (sprite raw 보존, build 시점 압축) | Editor 도구 작성 필요 |
| B | 모든 sprite raw resize (768×1024 → 512×768) | 영구 작은 PNG | 시각 품질 ↓ + 5008 baseline 변동 |
| C | 일부 sprite (큰 Background) 만 압축 | 부분 cleanup | 효과 작음 (Backgrounds 83 MB / Total 798 MB = 10%) |
| D | SpriteAtlas 통합 + Crunched | atlas 효율 ↑ | 5009.4 EnemyAtlas 정책 변경 필요 |

> 권장 **A** — Texture import 의 Platform Override 일괄 적용 (Editor 도구). raw PNG 보존 + 빌드 시점 압축.

## 3. 권장 설계 (CLAUDE.md §15 정합)

### 3.1 Platform 별 압축 정책

| Platform | textureCompression | Format | crunched | 효과 (sprite ~768×1024) |
|---|---|---|---|---|
| DefaultPlatform | 2 (Compressed) | 자동 | 1 | ~150 KB (vs 3 MB raw) |
| **iOS Override** | 2 (Compressed) | ASTC 6×6 | 1 (Q 50) | ~80 KB |
| Android Override | 2 (Compressed) | ETC2 / ASTC | 1 | ~80 KB |
| Mac/Standalone Override | 2 (Compressed) | DXT5 / BC7 | 1 | ~150 KB |

### 3.2 신규 Editor 도구

`Scripts/Editor/TextureCompressionConfigurer.cs` (~120L):

```csharp
[MenuItem("Tools/LittleHero/Apply Texture Compression Policy (5018)")]
public static void ApplyPolicy()
{
    string[] folders = {
        "Assets/_Project/Art/Generated_v2/Enemies",
        "Assets/_Project/Art/Generated_v2/Backgrounds",
        "Assets/_Project/Art/Generated_v2/Bosses",
        "Assets/_Project/Art/Generated_v2/UI",
        "Assets/_Project/Art/Generated_v2/Weapons",
        // ... 모든 Generated_v2 하위 폴더
    };
    int processed = 0;
    foreach (var folder in folders) {
        var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
        foreach (var guid in guids) {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            // Default platform — Compressed + Crunched
            var defaultSettings = importer.GetDefaultPlatformTextureSettings();
            defaultSettings.textureCompression = TextureImporterCompression.Compressed;
            defaultSettings.crunchedCompression = true;
            defaultSettings.compressionQuality = 50;
            importer.SetPlatformTextureSettings(defaultSettings);

            // iOS Override — ASTC 6x6 + Crunched
            var iosSettings = importer.GetPlatformTextureSettings("iPhone");
            iosSettings.overridden = true;
            iosSettings.format = TextureImporterFormat.ASTC_6x6;
            iosSettings.textureCompression = TextureImporterCompression.Compressed;
            iosSettings.crunchedCompression = true;
            iosSettings.compressionQuality = 50;
            iosSettings.maxTextureSize = 2048;
            importer.SetPlatformTextureSettings(iosSettings);

            importer.SaveAndReimport();
            processed++;
        }
    }
    Debug.Log($"[5018] Texture compression policy applied to {processed} textures.");
}
```

### 3.3 SpriteAtlas Override

EnemyAtlas (8192 Uncompressed) → iOS Override 로 ASTC 6×6 + Crunched. 신규 Editor 메뉴.

### 3.4 ProjectValidator 룰 (5018)

```csharp
[5018] ValidateTextureCompressionPolicy
- Generated_v2 의 PNG 중 textureCompression == 0 (None) 검출 → 경고
- crunchedCompression == 0 검출 → 경고
- iOS Override 없는 sprite 검출 → 경고
```

## 4. Sprint 분해

| Sprint | 작업 | commit | 예상 시간 |
|---|---|---|---|
| 1 | TextureCompressionConfigurer Editor 도구 + 786 PNG 일괄 적용 | 1 | 40분 |
| 2 | EnemyAtlas SpriteAtlas iOS Override 적용 | 1 | 25분 |
| 3 | ProjectValidator 5018 룰 + NUnit | 1 | 30분 |
| 4 | iOS 빌드 + App 용량 측정 + Mac 빌드 비교 | 1 | 30분 (사용자 측정 포함) |
| 5 | 결과 분석 + docs (changelog + CLAUDE.md §11/§14) | 1 | 25분 |
| **총** | **5 commit** | **5** | **약 2.5시간** |

## 5. CLAUDE.md §15 정합

- §15.4 — SO/import 설정 일괄 적용 (Editor 도구)
- §15.6 — Editor 도구 ~120L (100L 룰 임박 — 헬퍼 분리 검토)
- §15.7 — 정적 헬퍼 + AssetImporter API 활용
- §15.9 — NUnit (TextureCompressionPolicy 검증)
- §15.12 — Validator 5018 룰 신규

## 6. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| ASTC 6×6 시각 품질 저하 | iOS device 시각 검증 (TestFlight) — 시각 회귀 시 ASTC 4×4 (더 큰 용량, 품질 ↑) |
| Crunched 의 import 시간 ↑ | 첫 import 시 1회 — 이후 cache |
| EnemyAtlas Uncompressed 정책 충돌 | Sprint 2 — Atlas iOS Override 만 압축 (Editor mode 미충돌) |
| Background CH 의 시각 품질 (1024×768 PNG) | iOS Override 압축 시 시각 OK (배경 — UI 시각 임계 ↓) |
| 786 PNG 일괄 처리 시 30분+ | AssetDatabase.StartAssetEditing/StopAssetEditing batch — 10분 |

## 7. 예상 효과

| 항목 | 현재 | 5018 후 (예상) | 절감 |
|---|---:|---:|---:|
| sharedassets0.assets.resS | 798 MB | ~50 MB | -94% |
| Mac 빌드 합계 | 894 MB | ~140 MB | -84% |
| **iOS App Store 임계 (150 MB)** | ❌ 6× 초과 | ✅ < 150 MB | **TestFlight 진입 가능** |

## 8. 변경 이력

- 2026-05-06: 초기 작성 (5016 D2=A 측정 결과 기반).
