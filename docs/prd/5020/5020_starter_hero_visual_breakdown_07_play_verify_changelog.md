# T07: Sprint 5 — Play 검증 + changelog

> **레이어**: Docs
> **의존성**: T06 (Auditor + NUnit + Validator)
> **예상 시간**: 20분 / 1 commit
> **PRD 출처**: Sprint PRD §8 + Decisions D-D=B

---

## 1. 작업 항목

### 1.1 Play 검증 시나리오 3종 (사용자 보고)

**시나리오 1: Attic 진입 → 3 캐릭터 portrait 확인**
1. Attic 씬 Play 시작
2. AtticCharacterSelector 진입
3. **기대**: 3 portrait V5 sticker 직업 정체성 + 한국어 이름 라벨 ("전사 시우" 등)
4. 좌우 스와이프 → 3 캐릭터 부드럽게 전환

**시나리오 2: 캐릭터 선택 → 챕터 1 진입**
1. 시우(전사) 선택 → 챕터 1
2. **기대**: PlayerController 가 신규 portrait 사용, 발 밑 pivot 으로 정상 배치
3. 이동 시 flipX 좌우 반전 정상

**시나리오 3: 구 SaveData 마이그레이션 (D3-B 핵심)**
1. 사전 준비: `Tools/LittleHero/Reset SaveData` 후 `save.json` 직접 편집:
   ```json
   {
     "selectedCharacterId": "default_boy",
     "unlockedCharacterIds": ["default_boy", "brave_girl"]
   }
   ```
2. Game 재시작 → SaveSystem.Load() → SaveDataMigration.Apply()
3. **기대**:
   - 콘솔 NRE 없이 정상 로드
   - selectedCharacterId 가 `hero_warrior` 로 자동 변환
   - unlockedCharacterIds 가 `["hero_warrior", "hero_archer"]` 로 자동 변환
   - Attic 진입 시 시우(전사) 가 선택된 상태로 노출

### 1.2 changelog 추가

**경로**: `docs/little_hero/changelog_v1.md` 최상단

**항목** (5019 형식 답습):

```markdown
- 2026-05-06: **5020 starter_hero_visual** — 시작 주인공 3종(전사 시우/궁수 가영/마법사 연우) V5 sticker 직업 정체성 재생성 (D1=시우/가영/연우 / D2-A idle 1장 / D3-B id+파일명 정합 / D4-A 코드 분리 / D5-B PSB ref 별도, 7 commit + Phase 1~6 docs). **사전조사**: SO className(전사/궁수/마법사) ↔ portrait(키즈) 미스매치 발견. **PGE 6-Phase 적용**: Critique 8.9/10 → Rebuttal Accepted 4 (R1/I1/D-A/D-B 사실 검증) → Mediator D-C=A 권장 채택 + D-D=B 사용자 오버라이드. **신규 산출**: docs/prd/5020/ (prompt_table + png_guid_map + breakdown 8 + critique/rebuttal/decisions/v2 + contract + evaluation) + scripts/5020_starter_hero_batch.py (5019 답습) + Core/SaveDataMigration.cs (D3-B id 매핑 + unlockedCharacterIds[]) + Editor/StarterHeroAuditor.cs + 2 NUnit 클래스. **결과**: 6/6 PNG 갱신 (idle 3 in Generated_v2 + psb_ref 3 in Editor/References), 3 SO rename (Char_DefaultBoy → Char_HeroWarrior 등), id `default_boy` → `hero_warrior` SaveData 자동 마이그레이션, sprite pivot (0.5, 0.1) 발 밑 적용, displayName "전사 시우/궁수 가영/마법사 연우". NUnit 108 → 117. PSB reference Editor/ 폴더 자동 빌드 제외. References/Selected_Characters → _v1_legacy rename. Phase 4 출시 전 시각·데이터 정합성 핵심
```

### 1.3 (선택) CLAUDE.md §11 마일스톤 갱신

```markdown
| **starter_hero_visual_v1** | 시작 주인공 3종 V5 직업 정체성 재생성 (5020) | ✅ 2026-05-06 |
```

---

## 2. 검증 기준

### 2.1 사용자 시나리오 보고
- [ ] 시나리오 1 PASS (portrait + 한국어 이름 라벨 정상)
- [ ] 시나리오 2 PASS (캐릭터 선택 → 챕터 진입 → 발 밑 pivot 정상)
- [ ] 시나리오 3 PASS (SaveData 마이그레이션 자동 — selectedCharacterId + unlockedCharacterIds[])

### 2.2 changelog
- [ ] `docs/little_hero/changelog_v1.md` 최상단에 5020 항목 추가
- [ ] CLAUDE.md §14 5건 maintain (가장 오래된 항목 제거 또는 6건 임시 유지)

### 2.3 (선택) CLAUDE.md §11 마일스톤
- [ ] starter_hero_visual_v1 행 추가

---

## 3. commit 메시지

```
5020 Sprint 5 / T07 — Play 검증 + changelog (7/7) — 5020 종결

[변경]
  ~ docs/little_hero/changelog_v1.md (5020 항목 추가, 5019 형식 답습)
  ~ CLAUDE.md §11 마일스톤 표 (선택, starter_hero_visual_v1)

[검증]
  - Play 시나리오 1~3 사용자 보고 PASS
  - SaveData 구 id (default_boy/brave_girl/quiet_friend) → 신 id 자동 변환
  - 5020 전체 commit 7종 완료 (T01~T07)

[종합]
  - PGE 6-Phase 완수
  - NUnit 117 (전 시스템 회귀 0)
  - GUID 보존 (CharacterData wiring 정상)
  - Phase 4 출시 시각·데이터 정합성 확보

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 4. 관련 문서

- Sprint PRD §8 (Play 검증 시나리오)
- Decisions D-D=B (Selected_Characters_v1_legacy)
- 5019 changelog 형식 답습
- changelog-manager 스킬 (자동 등재 시 활용)

---

## 5. 알려진 위험

| 위험 | 완화 |
|---|---|
| Play 시나리오 3 (SaveData 마이그레이션) 실패 | T04 SaveDataMigration + T06 NUnit 5 사전 검증 → 자동 차단 |
| changelog 5건 maintain 초과 | changelog-manager 자동 정리 또는 수동 |
| CLAUDE.md §11 마일스톤 행 추가 시 표 정합 깨짐 | 기존 행 형식 답습 (이름 + 설명 + ✅ 일자) |
| 한글 폰트 Attic UI 잘림 | T07 시나리오 1 에서 발견 시 폰트/maxWidth 보정 (별도 commit 또는 본 commit 안에) |
