#!/usr/bin/env python3
"""5010 V1 11 fairytale enemies batch generator (5008 baseline + 5009 단일 char 강조)."""
import subprocess
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parent.parent
GEN = REPO / ".claude/skills/comfyui-image/scripts/generate.py"
WORKFLOW = REPO / ".claude/skills/comfyui-image/assets/workflow_layerdiffuse.json"
OUT = REPO / "LittleHero/Assets/_Project/Art/Generated_v2/Enemies/V1"

BASE = (
    "cute cartoon sticker illustration, V5 dynamic tone, warm cozy lighting, "
    "soft cartoon shading, chunky thick white outline, "
    "three-quarter view, single character centered in frame, "
    "fully transparent background, isolated subject, solo, 1character, alone"
)
PREFIX = "fairy tale storybook illustration, classic children's book character"
NEG = (
    "low quality, blurry, watermark, photorealistic, dark, scary, blood, gore, "
    "text, logo, signature, "
    "multiple subjects, multiple characters, twins, duplicate, copies, "
    "two characters, three characters, four characters, "
    "grid, frame, border, comic panel, sprite sheet, character sheet, "
    "2x2 grid, 4 panels, multi-panel, collage, mosaic, isometric grid, tile"
)

ITEMS = [
    ("Enemy_Basic",       11001, "small slime creature, blob shape, gentle smile, light blue tone"),
    ("Enemy_BeanGiant",   11002, "friendly Bean Giant character, towering kind giant, beanstalk armor, no menace"),
    ("Enemy_Bear",        11003, "friendly bear character, big round body, waving paw, brown fur"),
    ("Enemy_BigWolf",     11004, "friendly Big Bad Wolf character, cartoon wolf, gray fur, no fear no menace"),
    ("Enemy_CardSoldier", 11005, "Card Soldier character, playing card knight, friendly pose, red and black uniform"),
    ("Enemy_CloudGiant",  11006, "Cloud Giant character, fluffy white cloud creature, gentle giant, no menace"),
    ("Enemy_Firefly",     11007, "Firefly character, glowing insect, soft yellow light, friendly"),
    ("Enemy_Pumpkin",     11008, "friendly Pumpkin character, cute jack-o-lantern, no scary, orange round"),
    ("Enemy_QueenHearts", 11009, "Queen of Hearts character, friendly cartoon queen with heart crown, no menace"),
    ("Enemy_Rabbit",      11010, "White Rabbit character, cute rabbit in vest with pocket watch"),
    ("Enemy_StarCloud",   11011, "Star Cloud character, fluffy cloud with star eyes, sparkling"),
]


def run():
    OUT.mkdir(parents=True, exist_ok=True)
    print(f"=== V1 11 PNG 5008 baseline 재생성 (768×1024) ===", flush=True)
    success = 0
    fails = []
    for name, seed, form in ITEMS:
        prompt = f"{BASE}, {PREFIX}, {form}"
        output = OUT / f"{name}.png"
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
            fails.append((name, seed))
        else:
            print(f"  ✅ {name} (seed={seed})", flush=True)
            success += 1
    print(f"\nV1 11 재생성: {success} 성공 / {len(fails)} 실패", flush=True)
    return 0 if not fails else 1


if __name__ == "__main__":
    sys.exit(run())
