# T06: Auditor + SpriteAtlas + docs

> **레이어**: Test + Docs
> **의존성**: T01~T05 완료
> **예상 시간**: 25분

## 1. 작업 항목

### 1.1 Auditor 재실행

`Tools > LittleHero > Audit Enemy Sprite Quality` 실행:
- 210 PNG 모두 transparent 30~80% 통과 확인
- V2 fallback (archetype) OK 확인
- 손상 0 확인

### 1.2 SpriteAtlas 재빌드 (Rebuttal accepted)

- Window > 2D > Sprite Atlas
- EnemyAtlas 선택 → Pack Atlases (또는 Unity 자동 빌드)
- Profiler 진입 — CH 1 wave 1 적 출현 시 draw call <10/frame 검증 (PIVOT v4 §7)

### 1.3 changelog_v1.md 등재

```markdown
- 2026-05-04: **5009 chapter_overrides_v1 — 210 PNG 5008 baseline 재생성 (RESERVED 해제, 5012/5013 종결 후 진행, 6 commit)** — `docs/prd/5009_*.md` (디자인) + `5009/5009_*_v2.md` (PRD) + Phase 2~6 산출. **카테고리별 batch 6 sprint**: Meta CH 1~5 30 PNG / Nature CH 6~14 54 PNG / Animal CH 15~20 36 PNG / Civ CH 21~27 42 PNG / Fantasy CH 28~35 48 PNG. **시드 공식**: base + (CH-1)*6 (Rebuttal accepted). **5008 baseline 재사용**: SDXL base 1.0 + 768×1024 + cfg 6 + LayerDiffuse RGBA + 17 grid keyword negative + Danbooru "solo" tags. **Auditor 사후**: 210 PNG transparent 30~80%, asym ≤ 50, V2 fallback OK. **SpriteAtlas 재빌드**: EnemyAtlas Pack — draw call <10/frame 유지. **사용자 contextual fix**: 5012 hue shift 효과가 chapter 1 wave 1 visible X 보고 → 5009 RESERVED 해제 + chapter override 자체 갱신으로 근본 해결.
```

### 1.4 CLAUDE.md §11/§12/§14

- §11: chapter_overrides_v1 마일스톤 행 추가
- §12: Art V5 sticker 통일 — 누적 412 → +210 갱신 (chapter override 항목 갱신)
- §14: 직전 5건 회전 — 5009 추가, 5006 제거 (또는 5012/5012.1 통합)

## 2. 검증 기준

- Auditor 손상 0
- SpriteAtlas 재빌드 성공
- Profiler draw call <10/frame
- 사용자 review 5 batch 모두 PASS 확인

## 3. commit 메시지

```
5009 Sprint 6 / T06 — Auditor + SpriteAtlas + docs (6/6) — 5009 종결

[변경]
- Auditor 재실행 — 210 PNG transparent 30~80% 모두 통과
- EnemyAtlas SpriteAtlas 재빌드 (Pack Atlases) — draw call <10/frame 검증
- changelog_v1.md 5009 항목
- CLAUDE.md §11 마일스톤 + §12 시스템 현황 + §14 직전 5건 회전
- 후속 anchor 5010~5015 명시

[5009 누적 6 commit]
- T01 Meta CH 1~5 30 PNG
- T02 Nature CH 6~14 54 PNG
- T03 Animal CH 15~20 36 PNG
- T04 Civ CH 21~27 42 PNG
- T05 Fantasy CH 28~35 48 PNG
- T06 Auditor + Atlas + docs

[검증]
- 210 PNG 5008 baseline 정합
- 사용자 5 batch review PASS
- 5012 hue shift 효과 visible 가능 (chapter override sprite 갱신 후)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 4. 알려진 위험
- SpriteAtlas 재빌드 시 메모리 spike (Unity 자동 처리)
- Auditor 가 신규 PNG 잘못 fallback 처리 가능 — 5008 D2=C 정합 검증
