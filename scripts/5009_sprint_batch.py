#!/usr/bin/env python3
"""5009 Chapter Override batch generator — 5008 baseline 재사용.

Usage: python3 5009_sprint_batch.py <sprint_num>

sprint_num: 1=Meta CH 1~5 / 2=Nature CH 6~14 / 3=Animal CH 15~20
            4=Civ CH 21~27 / 5=Fantasy CH 28~35
"""
import subprocess
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parent.parent
GEN = REPO / ".claude/skills/comfyui-image/scripts/generate.py"
WORKFLOW = REPO / ".claude/skills/comfyui-image/assets/workflow_layerdiffuse.json"
OUT_DIR = REPO / "LittleHero/Assets/_Project/Art/Generated_v2/Enemies/Chapters"

BASE_PROMPT = (
    "cute cartoon sticker illustration, V5 dynamic tone, warm cozy lighting, "
    "soft cartoon shading, chunky thick white outline, "
    "three-quarter view, single character centered in frame, "
    "fully transparent background, isolated subject, "
    "solo, 1character, alone"
)

NEGATIVE = (
    "low quality, blurry, watermark, photorealistic, dark, scary, blood, gore, "
    "text, logo, signature, "
    "multiple subjects, multiple characters, twins, duplicate, copies, "
    "two characters, three characters, four characters, "
    "grid, frame, border, comic panel, sprite sheet, character sheet, "
    "2x2 grid, 4 panels, multi-panel, collage, mosaic, isometric grid, tile"
)

ARCH_ORDER = ["Small", "Medium", "Large", "MiniA", "MiniB", "Boss"]

# Sprint 매핑: (sprint_num, prefix, archetype dict, chapter range, seed_base)
SPRINTS = {
    1: {
        "name": "Meta",
        "prefix": "village friendly creature, warm beige and brown tones, cozy meadow vibes",
        "seed_base": 22222,
        "chapters": range(1, 6),
        "archetypes": {
            "Small":  "tiny mushroom child, beige and white spotted cap, oversized cute eyes, simple body",
            "Medium": "village kid, brown overalls, soft round cheeks, cheerful smile",
            "Large":  "burly village man, gentle giant, soft beard, warm brown apron, kind eyes",
            "MiniA":  "village guardian, simple shield and lantern, brown leather, kind protective expression",
            "MiniB":  "village elder, wise expression, soft white beard, beige robe, gentle posture",
            "Boss":   "village patriarch, large warm presence, golden brown attire, fatherly aura",
        },
    },
    2: {
        "name": "Nature",
        "prefix": "forest nature spirit, lush green and earthy brown tones, leafy moss texture, woodland atmosphere",
        "seed_base": 33333,
        "chapters": range(6, 15),
        "archetypes": {
            "Small":  "tiny moss sprite, leafy details, dewdrops, gentle glow",
            "Medium": "friendly acorn faerie, autumn leaf wings, cute round face",
            "Large":  "bramble guardian, thick vine arms, kind eyes through leafy mask",
            "MiniA":  "forest watcher, owl-like features, mossy cloak, watchful",
            "MiniB":  "gentle root spirit, wood and bark texture, peaceful elder posture",
            "Boss":   "majestic forest king, antlers like branches, deep green presence, regal stature",
        },
    },
    3: {
        "name": "Animal",
        "prefix": "cute domestic animal companion, warm orange and white tones, fluffy fur, friendly gaze",
        "seed_base": 44444,
        "chapters": range(15, 21),
        "archetypes": {
            "Small":  "playful kitten, fluffy tail, big curious eyes",
            "Medium": "cheerful puppy, floppy ears, wagging tail",
            "Large":  "golden retriever, friendly grin, soft golden fur",
            "MiniA":  "tabby cat, watchful pose, sleek fur pattern",
            "MiniB":  "loyal sheepdog, fluffy white and brown coat, gentle eyes",
            "Boss":   "majestic lion, golden mane, kind regal expression",
        },
    },
    4: {
        "name": "Civ",
        # 5009.7 — papercraft → mechanical wind-up toy 톤 변경 (사용자 D=A 옵션, 시인성 강화)
        "prefix": "mechanical wind-up toy creature, vintage brass and metallic pastels, ticking clockwork details, vibrant colorful metal craft",
        "seed_base": 55555,
        "chapters": range(21, 28),
        "archetypes": {
            "Small":  "tiny wind-up mouse toy with brass key, copper details, big shiny eyes",
            "Medium": "clockwork robot, visible gears and rivets, soft metallic teal and copper, cheerful face",
            "Large":  "grand mechanical butler, vintage brass armor, friendly expression, sturdy stature",
            "MiniA":  "tin soldier with crank rifle, navy and gold uniform, alert pose",
            "MiniB":  "wind-up wizard, copper hat with ticking gears, kind eyes, brass robe",
            "Boss":   "massive steam-punk mech, friendly LED face, brass and teal armor, regal stature",
        },
    },
    5: {
        "name": "Fantasy",
        "prefix": "friendly fairy tale character, soft lavender and pastel purple tones, simple cute magical features",
        "seed_base": 77777,
        "chapters": range(28, 36),
        "archetypes": {
            "Small":  "tiny friendly fairy, simple wings, sparkles, cute eyes",
            "Medium": "apprentice wizard, simple star hat, cheerful expression",
            "Large":  "crystal creature, simple gem body, gentle glow, kind eyes",
            "MiniA":  "young witch, simple broom and pointy hat, friendly smile",
            "MiniB":  "elder wizard, soft beard, simple staff, wise gentle eyes",
            "Boss":   "wizard king, regal robes with simple star pattern, kind majestic presence",
        },
    },
}


def generate_one(prompt, negative, seed, output):
    """generate.py 호출 (5008 baseline: 768×1024 + cfg 6 + 25 steps)."""
    cmd = [
        "python3", str(GEN),
        "--workflow", str(WORKFLOW),
        "--prompt", prompt,
        "--negative", negative,
        "--seed", str(seed),
        "--width", "768",
        "--height", "1024",
        "--steps", "25",
        "--cfg", "6.0",
        "--output", str(output),
    ]
    result = subprocess.run(cmd, capture_output=True, text=True)
    if result.returncode != 0:
        print(f"  ❌ FAIL seed={seed}: {result.stderr.strip()}", flush=True)
        return False
    print(f"  ✅ {output.name} (seed={seed})", flush=True)
    return True


def run_sprint(num):
    s = SPRINTS[num]
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    print(f"=== Sprint {num} — {s['name']} ({len(list(s['chapters']))} 챕터 × 6 = {len(list(s['chapters']))*6} PNG) ===", flush=True)

    success = 0
    fails = []
    for chapter in s["chapters"]:
        chapter_seed = s["seed_base"] + (chapter - 1) * 6
        for idx, arch in enumerate(ARCH_ORDER):
            seed = chapter_seed + idx
            prompt = f"{BASE_PROMPT}, {s['prefix']}, {s['archetypes'][arch]}"
            output = OUT_DIR / f"CH{chapter:02d}_{arch}.png"
            ok = generate_one(prompt, NEGATIVE, seed, output)
            if ok:
                success += 1
            else:
                fails.append((chapter, arch, seed))

    print(f"\nSprint {num} ({s['name']}): {success} 성공 / {len(fails)} 실패", flush=True)
    if fails:
        print(f"실패: {fails}", flush=True)
        return 1
    return 0


if __name__ == "__main__":
    if len(sys.argv) != 2 or sys.argv[1] not in ("1", "2", "3", "4", "5"):
        print("Usage: python3 5009_sprint_batch.py <1|2|3|4|5>", file=sys.stderr)
        sys.exit(1)
    sys.exit(run_sprint(int(sys.argv[1])))
