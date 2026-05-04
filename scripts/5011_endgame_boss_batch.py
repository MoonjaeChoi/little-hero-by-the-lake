#!/usr/bin/env python3
"""5011 Endgame Boss sprite batch generator (D2=A+ V5 sticker 강화 + 인형 실사판 회피).

Usage: python3 5011_endgame_boss_batch.py <sprint>
  sprint: 1=Daily 7 / 2=Weekly 4
"""
import subprocess
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parent.parent
GEN = REPO / ".claude/skills/comfyui-image/scripts/generate.py"
WORKFLOW = REPO / ".claude/skills/comfyui-image/assets/workflow_layerdiffuse.json"
OUT_DIR = REPO / "LittleHero/Assets/_Project/Art/Generated_v2/Bosses"

BASE = (
    "solo, 1character, alone, single subject, "
    "2D flat cartoon sticker illustration, hand-drawn V5 dynamic tone, "
    "chunky thick white outline, soft cartoon shading, vivid bold colors, "
    "three-quarter view, single character centered in frame, "
    "fully transparent background, isolated subject, "
    "epic boss creature, large impressive character, royal pose, friendly powerful aura, no menace"
)

NEG = (
    "low quality, blurry, watermark, photorealistic, photo, photograph, 3D render, octane render, "
    "plush toy photography, plushie, soft toy, realistic doll, doll photography, toy photo, "
    "3D model, realistic, photographic, dark, scary, blood, gore, "
    "text, logo, signature, "
    "multiple subjects, multiple characters, twins, duplicate, copies, "
    "two characters, three characters, four characters, "
    "grid, frame, border, comic panel, sprite sheet, character sheet, "
    "2x2 grid, 4 panels, multi-panel, collage, mosaic, isometric grid, tile"
)

DAILY = [
    ("DailyBoss_00", 99000, "friendly Monday morning blue cartoon boss, dawn aurora colors, calm awakening tone"),
    ("DailyBoss_01", 99001, "vibrant Tuesday red cartoon boss, energetic spark, motivated playful pose"),
    ("DailyBoss_02", 99002, "verdant Wednesday green cartoon boss, fresh leafy growth, nature friendly"),
    ("DailyBoss_03", 99003, "royal Thursday purple cartoon boss, mystical wisdom, flowing robe"),
    ("DailyBoss_04", 99004, "warm Friday pink cartoon boss, gentle joyful smile, soft glow"),
    ("DailyBoss_05", 99005, "bright Saturday yellow cartoon boss, sunny round playful"),
    ("DailyBoss_06", 99006, "golden Sunday cartoon boss, peaceful crown, restful smile"),
]

WEEKLY = [
    ("WeeklyBoss_00", 99100, "Light Realm cartoon boss, radiant white-gold guardian, divine wings, friendly powerful"),
    ("WeeklyBoss_01", 99101, "Shadow Realm cartoon boss, deep purple sentinel, mysterious cape, no scary"),
    ("WeeklyBoss_02", 99102, "Life Realm cartoon boss, vibrant green forest king, leafy antlers branching"),
    ("WeeklyBoss_03", 99103, "Cosmic Realm cartoon boss, deep blue starscape lord, galaxy crown"),
]


def generate_one(name, seed, prefix):
    prompt = f"{BASE}, {prefix}"
    output = OUT_DIR / f"{name}.png"
    cmd = [
        "python3", str(GEN),
        "--workflow", str(WORKFLOW),
        "--prompt", prompt,
        "--negative", NEG,
        "--seed", str(seed),
        "--width", "768", "--height", "1024",
        "--steps", "25", "--cfg", "6.0",
        "--output", str(output),
    ]
    r = subprocess.run(cmd, capture_output=True, text=True)
    if r.returncode != 0:
        print(f"  ❌ {name} (seed={seed}): {r.stderr.strip()}", flush=True)
        return False
    print(f"  ✅ {name} (seed={seed})", flush=True)
    return True


def run_sprint(num):
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    if num == 1:
        items = DAILY
        label = "Daily 7"
    elif num == 2:
        items = WEEKLY
        label = "Weekly 4"
    else:
        print(f"Usage: python3 5011_endgame_boss_batch.py <1|2>", file=sys.stderr)
        return 1

    print(f"=== Sprint {num} — {label} ({len(items)} PNG) ===", flush=True)
    success = 0
    fails = []
    for name, seed, prefix in items:
        if generate_one(name, seed, prefix):
            success += 1
        else:
            fails.append((name, seed))
    print(f"\nSprint {num} ({label}): {success} 성공 / {len(fails)} 실패", flush=True)
    return 0 if not fails else 1


if __name__ == "__main__":
    if len(sys.argv) != 2 or sys.argv[1] not in ("1", "2"):
        print("Usage: python3 5011_endgame_boss_batch.py <1|2>", file=sys.stderr)
        sys.exit(1)
    sys.exit(run_sprint(int(sys.argv[1])))
