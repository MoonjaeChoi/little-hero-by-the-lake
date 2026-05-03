# Changelog Manager — 변경 이력 분리 운영 스킬

> **목적**: CLAUDE.md 가 비대해지는 문제 해결. 변경 이력을 별도 `docs/little_hero/changelog_v1.md` 에 누적하고, CLAUDE.md 는 핵심 (Phase·아키텍처·운영 규칙) 만 유지한다.
>
> **출처**: `architecture_analysis_v1.md` §3.3.E (CLAUDE.md 비대 문제 + Claude 컨텍스트 비용 ↑) + 사용자 결정 2026-05-03.

---

## 0. 단일 진실원

| 파일 | 역할 |
|---|---|
| `CLAUDE.md` | **핵심만** — Phase·아키텍처·운영 규칙·**최근 5건** |
| `docs/little_hero/changelog_v1.md` | 모든 변경 이력 누적 (시간순) |
| `docs/little_hero/changelog_archive.md` | 60일+ 이상 압축 보관 (선택) |

**원칙**: CLAUDE.md 의 §14 "최근 핵심 결정" 은 5건만 유지. 6건+ 시점에 가장 오래된 항목을 changelog_v1.md 로 이동.

---

## 1. 워크플로 — 한 사이클

스킬 활성화 시 다음 단계 수행.

### Step 1 — 컨텍스트 로드
- `CLAUDE.md` §14 (최근 5건) 읽기
- `docs/little_hero/changelog_v1.md` 마지막 5건 읽기
- 직전 git log 5건 (`git log --oneline -5`)

### Step 2 — 신규 변경 분석
사용자가 새로운 작업을 commit 했거나 의미 단위 마무리 시:
1. commit 메시지 + git diff 분석
2. 핵심 결정 / 시스템 도입 / 주요 fix 인지 판단
3. **포함 기준** (changelog 등재):
   - 사용자 결정 (옵션 X/Y/Z 채택)
   - 시스템 도입 (PlayerStats v2, GachaService 등)
   - 큰 fix (10+ 파일 변경 또는 디자인 영향)
   - 디자인 문서 신규 (stats_v2_infinite, gacha_v1 등)
4. **제외 기준** (changelog 미등재):
   - 단순 버그 fix (1~2 파일)
   - 오타 수정
   - 코멘트 갱신만

### Step 3 — 항목 작성
`changelog_v1.md` 끝에 추가:

```markdown
- YYYY-MM-DD: **<제목>** — <상세>. <변경 파일 수> + <영향 시스템>.
  참조: <design 문서 경로>. 사용자 결정 (있으면 명시).
```

**예시**:
```markdown
- 2026-05-04: **InventorySelectionPanel v3 페이지 자동 갱신** — 가챠 결과 즉시 반영 (이벤트 구독 추가). UI/InventorySelectionPanel.cs 1 파일. 참조: ui_v1.md §8.
```

### Step 4 — CLAUDE.md §14 정리
1. CLAUDE.md §14 의 "최근 핵심 결정" 검사
2. **5건 초과** 시 가장 오래된 항목 제거
3. 신규 항목 (방금 등재) 추가 (위쪽)
4. CLAUDE.md commit 시 `docs:` prefix 사용

### Step 5 — Archive (옵션, 90일 1회)
- `changelog_v1.md` 의 60일 이상 항목 → `changelog_archive.md` 이동
- `changelog_v1.md` 헤더에 archive 링크 유지

### Step 6 — Commit
**메시지 형식**:
```
docs: changelog <date> <제목>

- changelog_v1.md 추가 항목
- CLAUDE.md §14 최근 5건 갱신
```

**원칙**:
- changelog 변경은 별도 commit (코드 commit 과 분리)
- 또는 코드 commit 의 마지막 단계로 묶음 가능 (한 사이클 종료 시)

---

## 2. 사용 패턴 (사용자 발화 → 동작)

| 사용자 발화 | 동작 |
|---|---|
| "changelog 등재" / "변경 이력 기록" | Step 1~6 자동 (직전 commit 기반) |
| "이력 정리" / "CLAUDE.md 압축" | Step 4 만 (CLAUDE.md §14 5건 maintain) |
| "archive 분리" | Step 5 (60일+ 항목 archive 이동) |
| "5건 초과 이력 보여줘" | CLAUDE.md §14 검사 + 6건+ 시 최오래된 1건 표시 |

자동 트리거 — 사용자 명시 없어도 다음 시점에 changelog 등재 권장:
- 디자인 문서 신규 commit 직후
- 5+ commit 묶음 Sprint 종료 시
- 사용자 결정 (옵션 X 채택 등) 적용 commit 직후

---

## 3. CLAUDE.md §14 형식 (정형)

```markdown
## 14. 변경 이력

> **위치 변경 (2026-05-03)**: 변경 이력은 `docs/little_hero/changelog_v1.md` 로 분리.
> CLAUDE.md 는 핵심만 유지 (architecture_analysis_v1.md §3.3.E + changelog-manager 스킬).

### 최근 핵심 결정 (직전 5건)
- YYYY-MM-DD: **<제목>** — <한 줄 요약>
- ...

자세한 이력은 → [docs/little_hero/changelog_v1.md](docs/little_hero/changelog_v1.md)
```

---

## 4. changelog_v1.md 항목 형식

### 4.1 표준 형식
```markdown
- YYYY-MM-DD: **<제목>** — <상세 설명>. <변경 핵심>. 참조: <문서>.
```

### 4.2 풍부한 형식 (Sprint·시스템 도입)
```markdown
- YYYY-MM-DD: **<제목>** — <개요>. **사용자 결정** (있으면): <결정 항목>. **변경**: <코드/SO/문서 영향>. **검증**: <Play 또는 시뮬 결과>. **알려진 후속**: <남은 작업>. 참조: <문서>.
```

### 4.3 영역별 분류 (선택)
- 시스템 도입: `**stats_v2_infinite.md**` 같은 굵은 표시
- 사용자 결정: `**사용자 결정 (3건)**:` 명시
- Phase: `**Sprint N — <단계명>**` 명시

---

## 5. 자동화 권장 (claude code 활용)

### 5.1 commit 메시지 → changelog 자동 생성
- Claude 가 git log 분석 + commit 메시지 → changelog 항목 draft
- 사용자 검토 후 적용

### 5.2 검색 헬퍼
```bash
# 특정 시스템 결정 이력 검색
grep -i "stats_v2\|gacha_v1\|damage_system" docs/little_hero/changelog_v1.md

# 사용자 결정만 추출
grep -i "사용자 결정" docs/little_hero/changelog_v1.md
```

### 5.3 CLAUDE.md 5건 검증
```bash
# §14 의 최근 결정 행 수 (- 시작) 검사
sed -n '/^### 최근 핵심 결정/,/^---\|^##/p' CLAUDE.md | grep -c "^- "
# 5 초과 시 정리 필요
```

---

## 6. 흔한 실수

- ❌ CLAUDE.md §14 에 직접 신규 항목 추가 (changelog_v1.md 아닌)
  - → 기본은 changelog_v1.md 에 등재, CLAUDE.md 는 5건 maintain
- ❌ 모든 commit 등재
  - → 단순 버그 fix·오타는 제외. 핵심 결정만
- ❌ archive 분리 없이 changelog_v1.md 무한 누적
  - → 60일+ 분기 Step 5 권장 (선택)
- ❌ 사용자 결정 누락
  - → "**사용자 결정**" 명시 필수 (회수 시 컨텍스트 보존)

---

## 7. 출처

- `architecture_analysis_v1.md` §3.3.E
- 사용자 결정 2026-05-03 — "CLAUDE.md changelog 관리 X, 핵심만 저장, 별도 스킬"
- 직전 little-hero-dev skill 패턴 답습
- Claude Code Game Studios 49 sub-agent 모델 (단순화)
