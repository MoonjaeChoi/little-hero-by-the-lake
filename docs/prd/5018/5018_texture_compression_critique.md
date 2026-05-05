# 5018 PRD 리뷰

## 종합: 9.0/10 — Accept 7 / Reject 0 / Discuss 2

## Accept (7)
- ✅ TextureCompressionConfigurer Editor 도구 (786 PNG 일괄)
- ✅ DefaultPlatform Compressed + Crunched
- ✅ iOS Override ASTC 6×6 + Crunched
- ✅ EnemyAtlas iOS Override (Editor 화질 유지 + 빌드 압축)
- ✅ Validator 5018 룰 신규
- ✅ AssetDatabase batch (StartAssetEditing/StopAssetEditing)
- ✅ §15 정합 (Editor 도구 + 정적 헬퍼)

## Discuss

### D1. ASTC 압축 quality (4×4 vs 6×6)

- 4×4: 시각 ↑, 용량 ×2.25 (~150 KB/sprite vs 67 KB)
- **6×6 (★권장)**: balance, App Store 임계 통과 충분
- 8×8: 용량 ×0.7 (~47 KB), 시각 저하 위험

### D2. SpriteAtlas Crunched 적용 영역

- A: EnemyAtlas 만 (5009.4 기존)
- **B (★권장)**: Atlas + 개별 sprite 모두 Crunched
- C: Atlas 만 압축, 개별 sprite raw 유지

## 권장 일괄 (사용자 "권장안대로" 시): D1=6×6 / D2=B (Atlas + 개별 모두)

## 변경 이력
- 2026-05-06: 초기 작성
