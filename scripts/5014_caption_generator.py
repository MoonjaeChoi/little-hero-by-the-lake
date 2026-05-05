#!/usr/bin/env python3
"""5014 V5 sticker LoRA caption 자동 생성 (Kohya 형식 .txt).

- 5009/5010/5011 batch script 의 prompt 매핑 활용
- 각 PNG 옆에 동일 이름 .txt (예: Enemy_Meta_Boss.png + Enemy_Meta_Boss.txt)
- 학습 대상 262 = archetype 30 + V1 11 + chapter override 210 + Daily 7 + Weekly 4
"""
import re
from pathlib import Path

REPO = Path(__file__).resolve().parent.parent
ENEMIES_DIR = REPO / "LittleHero/Assets/_Project/Art/Generated_v2/Enemies"
BOSSES_DIR = REPO / "LittleHero/Assets/_Project/Art/Generated_v2/Bosses"

# Caption 베이스 (5008 검증 V5 baseline 압축)
BASE = "v5 sticker tone, cute cartoon, chunky thick white outline, soft cartoon shading, single character, transparent background"

# 5 카테고리 prefix (5009 검증)
META_PREFIX = "village friendly creature, warm beige and brown tones"
NATURE_PREFIX = "forest nature spirit, green and brown tones"
ANIMAL_PREFIX = "domestic animal companion, warm orange and white tones"
CIV_PREFIX = "mechanical wind-up toy creature, brass and metallic pastels"
FANTASY_PREFIX = "fairy tale character, lavender and pastel purple"

# Archetype 6 form (카테고리별 base)
META_FORMS = {
    "Small":  "tiny mushroom child", "Medium": "village kid", "Large":  "burly village man",
    "MiniA":  "village guardian",    "MiniB":  "village elder", "Boss":   "village patriarch",
}
NATURE_FORMS = {
    "Small":  "tiny moss sprite", "Medium": "acorn faerie", "Large":  "bramble guardian",
    "MiniA":  "forest watcher",  "MiniB":  "root spirit",  "Boss":   "forest king",
}
ANIMAL_FORMS = {
    "Small":  "playful kitten", "Medium": "cheerful puppy", "Large":  "golden retriever",
    "MiniA":  "tabby cat",      "MiniB":  "sheepdog",       "Boss":   "majestic lion",
}
CIV_FORMS = {
    "Small":  "wind-up mouse toy",  "Medium": "clockwork robot",  "Large":  "mechanical butler",
    "MiniA":  "tin soldier",        "MiniB":  "wind-up wizard",   "Boss":   "steam-punk mech",
}
FANTASY_FORMS = {
    "Small":  "tiny fairy",        "Medium": "apprentice wizard", "Large":  "crystal creature",
    "MiniA":  "young witch",       "MiniB":  "elder wizard",      "Boss":   "wizard king",
}

CATEGORY_BY_CHAPTER = {}  # CH 1~5 Meta / 6~14 Nature / 15~20 Animal / 21~27 Civ / 28~35 Fantasy
for ch in range(1, 6): CATEGORY_BY_CHAPTER[ch] = ("Meta", META_PREFIX, META_FORMS)
for ch in range(6, 15): CATEGORY_BY_CHAPTER[ch] = ("Nature", NATURE_PREFIX, NATURE_FORMS)
for ch in range(15, 21): CATEGORY_BY_CHAPTER[ch] = ("Animal", ANIMAL_PREFIX, ANIMAL_FORMS)
for ch in range(21, 28): CATEGORY_BY_CHAPTER[ch] = ("Civ", CIV_PREFIX, CIV_FORMS)
for ch in range(28, 36): CATEGORY_BY_CHAPTER[ch] = ("Fantasy", FANTASY_PREFIX, FANTASY_FORMS)

# V1 동화
V1_CAPTIONS = {
    "Enemy_Basic":       "small slime creature, blob shape, gentle smile",
    "Enemy_BeanGiant":   "Bean Giant character, towering kind giant, beanstalk armor",
    "Enemy_Bear":        "friendly bear character, big round body, brown fur",
    "Enemy_BigWolf":     "Big Bad Wolf character, gray fur cartoon wolf",
    "Enemy_CardSoldier": "Card Soldier character, playing card knight, red and black uniform",
    "Enemy_CloudGiant":  "Cloud Giant character, fluffy white cloud creature",
    "Enemy_Firefly":     "Firefly character, glowing insect, soft yellow light",
    "Enemy_Pumpkin":     "friendly Pumpkin character, cute jack-o-lantern, orange round",
    "Enemy_QueenHearts": "Queen of Hearts character, friendly cartoon queen with heart crown",
    "Enemy_Rabbit":      "White Rabbit character, cute rabbit in vest with pocket watch",
    "Enemy_StarCloud":   "Star Cloud character, fluffy cloud with star eyes",
}
V1_PREFIX = "fairy tale storybook illustration, classic children's book character"

# Daily 7 (요일 → 카테고리)
DAILY_CAPTIONS = [
    ("DailyBoss_00", "Meta", META_PREFIX, "village patriarch boss, golden brown attire"),
    ("DailyBoss_01", "Animal", ANIMAL_PREFIX, "majestic lion boss, golden mane"),
    ("DailyBoss_02", "Nature", NATURE_PREFIX, "forest king boss, antlers like branches"),
    ("DailyBoss_03", "Fantasy", FANTASY_PREFIX, "wizard king boss, regal purple robes"),
    ("DailyBoss_04", "Civ", CIV_PREFIX, "steam-punk mech boss, brass and teal armor"),
    ("DailyBoss_05", "Animal", ANIMAL_PREFIX, "metallic mythical shark boss, diamond scales, jeweled turtle shell back"),
    ("DailyBoss_06", "Meta", META_PREFIX, "grand village patriarch boss, peaceful crown, golden tone"),
]

# Weekly 4 (cycle)
WEEKLY_CAPTIONS = [
    ("WeeklyBoss_00", "Fantasy", FANTASY_PREFIX, "radiant divine wizard king boss, white-gold robes"),
    ("WeeklyBoss_01", "Civ", CIV_PREFIX, "shadow realm mech boss, deep purple armor"),
    ("WeeklyBoss_02", "Nature", NATURE_PREFIX, "life forest king boss, vibrant green leafy antlers"),
    ("WeeklyBoss_03", "Fantasy", FANTASY_PREFIX, "cosmic wizard boss, deep blue starscape robes"),
]


def write_caption(png_path: Path, caption: str):
    txt_path = png_path.with_suffix(".txt")
    txt_path.write_text(caption + "\n", encoding="utf-8")


def caption_archetype(name: str) -> str:
    """Enemy_Meta_Boss / Enemy_Animal_Small 등 archetype PNG."""
    m = re.match(r"^Enemy_(Meta|Nature|Animal|Civ|Fantasy)_(Small|Medium|Large|MiniA|MiniB|Boss)$", name)
    if not m: return None
    cat, arch = m.group(1), m.group(2)
    forms_map = {"Meta": META_FORMS, "Nature": NATURE_FORMS, "Animal": ANIMAL_FORMS, "Civ": CIV_FORMS, "Fantasy": FANTASY_FORMS}
    prefix_map = {"Meta": META_PREFIX, "Nature": NATURE_PREFIX, "Animal": ANIMAL_PREFIX, "Civ": CIV_PREFIX, "Fantasy": FANTASY_PREFIX}
    return f"{BASE}, {prefix_map[cat]}, {forms_map[cat][arch]}"


def caption_chapter(name: str) -> str:
    """CH01_Small / CH22_Boss 등 chapter override PNG."""
    m = re.match(r"^CH(\d{2})_(Small|Medium|Large|MiniA|MiniB|Boss)$", name)
    if not m: return None
    ch_num = int(m.group(1))
    arch = m.group(2)
    if ch_num not in CATEGORY_BY_CHAPTER: return None
    cat, prefix, forms = CATEGORY_BY_CHAPTER[ch_num]
    return f"{BASE}, {prefix}, {forms[arch]}"


def caption_v1(name: str) -> str:
    """V1/Enemy_*.png."""
    if name not in V1_CAPTIONS: return None
    return f"{BASE}, {V1_PREFIX}, {V1_CAPTIONS[name]}"


def caption_daily_weekly(name: str) -> str:
    """DailyBoss_NN / WeeklyBoss_NN."""
    for n, cat, prefix, form in DAILY_CAPTIONS + WEEKLY_CAPTIONS:
        if n == name:
            return f"{BASE}, {prefix}, {form}"
    return None


def main():
    targets = []
    # Archetype 30 + Chapter 210
    for png in sorted(ENEMIES_DIR.glob("*.png")):
        targets.append(("archetype", png))
    for png in sorted((ENEMIES_DIR / "Chapters").glob("*.png")):
        targets.append(("chapter", png))
    for png in sorted((ENEMIES_DIR / "V1").glob("*.png")):
        targets.append(("v1", png))
    # Daily 7 + Weekly 4
    for png in sorted(BOSSES_DIR.glob("*.png")):
        targets.append(("boss", png))

    print(f"=== 5014 Caption 자동 생성 ({len(targets)} PNG) ===")
    success, fails = 0, []
    for kind, png in targets:
        name = png.stem
        caption = None
        if kind == "archetype":
            caption = caption_archetype(name)
        elif kind == "chapter":
            caption = caption_chapter(name)
        elif kind == "v1":
            caption = caption_v1(name)
        elif kind == "boss":
            caption = caption_daily_weekly(name)
        if caption is None:
            fails.append((kind, name))
            print(f"  ❌ unmatched: {kind} / {name}")
            continue
        write_caption(png, caption)
        success += 1
    print(f"\nCaption 생성: {success}/{len(targets)} 성공 / {len(fails)} 실패")
    if fails:
        for k, n in fails[:10]:
            print(f"  {k}: {n}")
    return 0 if not fails else 1


if __name__ == "__main__":
    import sys
    sys.exit(main())
