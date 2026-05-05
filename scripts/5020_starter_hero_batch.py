#!/usr/bin/env python3
"""5020 Starter Hero Visual batch generator.

3 hero × 2 종(idle + psb_ref) = 6 PNG.
5008 baseline LayerDiffuse + 5019 batch 패턴 답습.

Usage:
  python3 5020_starter_hero_batch.py            # 6 PNG 일괄
  python3 5020_starter_hero_batch.py HeroWarrior idle  # 단일 retry
"""
import subprocess
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parent.parent
GEN = REPO / ".claude/skills/comfyui-image/scripts/generate.py"
WORKFLOW = REPO / ".claude/skills/comfyui-image/assets/workflow_layerdiffuse.json"
GEN_DIR = REPO / "LittleHero/Assets/_Project/Art/Generated_v2/Characters"
PSB_DIR = REPO / "LittleHero/Assets/_Project/Art/References/Characters_PSB"

BASE_PROMPT = (
    "v5 sticker tone, cute cartoon, chunky thick white outline, "
    "soft cartoon shading, single character, transparent background, "
    "three-quarter view, full body, arms visible, child proportions, 3-head tall, "
    "soft pastel palette, clean line, vibrant colors, warm lighting, "
    "solo, 1character, alone"
)

NEGATIVE = (
    "low quality, blurry, watermark, photorealistic, dark, scary, blood, gore, "
    "text, logo, signature, "
    "adult, teen, mature, muscular, sexy, "
    "heavy plate armor, heavy weapon, weapon hidden behind body, "
    "multiple subjects, multiple characters, twins, duplicate, copies, "
    "two characters, three characters, four characters, two girls, two boys, "
    "grid, frame, border, comic panel, sprite sheet, character sheet, "
    "2x2 grid, 4 panels, multi-panel, collage, mosaic, "
    "doll, plush figure, 3d render, realistic photo, "
    "cropped, partial body, only hand, only arm, headless, no body, missing torso, "
    "bowing, hunched over, lying down"
)

# 3 hero × 2 kind(idle + psb_ref) 명세
HEROES = {
    "HeroWarrior": {
        "name": "Siu (시우)",
        "wired_basename": "Char_DefaultBoy",  # T01 GUID 매핑 결과
        "idle": {
            "seed": 50220,
            "out": GEN_DIR / "Char_DefaultBoy.png",  # wired 베이스명 덮어쓰기 (GUID 보존)
            "prompt": (
                "1boy, young boy hero warrior named Siu, full body visible, "
                "light brown leather chest plate vest, "
                "holding cartoon shortsword in right hand visible at side, "
                "brave determined smile, light brown hair, "
                "centered standing pose, heroic stance"
            ),
        },
        "psb_ref": {
            "seed": 50210,
            "out": PSB_DIR / "Char_HeroWarrior_psb_ref.png",
            "prompt": (
                "young boy hero T-pose, body parts clearly separated, "
                "head and torso and arms and legs all visible, "
                "light leather chest plate, neutral expression, "
                "arms outstretched horizontally, standing straight, "
                "light brown hair"
            ),
        },
    },
    "HeroArcher": {
        "name": "Gayoung (가영)",
        "wired_basename": "Char_BraveGirl",
        "idle": {
            "seed": 50231,
            "out": GEN_DIR / "Char_BraveGirl.png",
            "prompt": (
                "1girl, single young girl hero archer named Gayoung, full body visible, "
                "wearing green tunic and brown belt, "
                "holding tall wooden bow with bowstring clearly visible in left hand, bow as tall as her body, "
                "small quiver on back with arrows, focused alert expression, "
                "dark brown hair tied back, centered standing pose, heroic stance"
            ),
        },
        "psb_ref": {
            "seed": 50211,
            "out": PSB_DIR / "Char_HeroArcher_psb_ref.png",
            "prompt": (
                "young girl hero T-pose, hood pulled back showing face, "
                "body parts clearly separated, head and torso and arms and legs all visible, "
                "neutral expression, arms outstretched horizontally, standing straight, "
                "dark brown hair"
            ),
        },
    },
    "HeroMage": {
        "name": "Yeonwoo (연우)",
        "wired_basename": "Char_QuietFriend",
        "idle": {
            "seed": 50222,
            "out": GEN_DIR / "Char_QuietFriend.png",
            "prompt": (
                "1child, single young child mage named Yeonwoo, full body visible head to toe, "
                "long star-pattern blue robe down to feet, "
                "holding small wooden staff with golden star tip in right hand, "
                "gentle smile, light blonde hair, "
                "centered standing pose, heroic stance"
            ),
        },
        "psb_ref": {
            "seed": 50212,
            "out": PSB_DIR / "Char_HeroMage_psb_ref.png",
            "prompt": (
                "young child mage T-pose, body parts clearly separated, "
                "head and torso and arms and legs all visible, "
                "star-pattern robe, neutral expression, "
                "arms outstretched horizontally, standing straight, light hair"
            ),
        },
    },
}


def generate_one(prompt, negative, seed, output):
    output.parent.mkdir(parents=True, exist_ok=True)
    cmd = [
        "python3", str(GEN),
        "--workflow", str(WORKFLOW),
        "--prompt", prompt,
        "--negative", negative,
        "--seed", str(seed),
        "--width", "768",
        "--height", "1024",
        "--steps", "30",
        "--cfg", "6.0",
        "--output", str(output),
    ]
    result = subprocess.run(cmd, capture_output=True, text=True)
    if result.returncode != 0:
        print(f"  FAIL seed={seed}: {result.stderr.strip()}", flush=True)
        return False
    print(f"  OK {output.name} (seed={seed})", flush=True)
    return True


def run_one(hero_id, kind):
    spec = HEROES[hero_id][kind]
    full_prompt = f"{BASE_PROMPT}, {spec['prompt']}"
    return generate_one(full_prompt, NEGATIVE, spec["seed"], spec["out"])


def run_all():
    print(f"=== 5020 Starter Hero — 6 PNG (idle 3 + psb_ref 3) ===", flush=True)
    success, fails = 0, []
    for hero_id, hero in HEROES.items():
        print(f"-- {hero_id} ({hero['name']}) --", flush=True)
        for kind in ("idle", "psb_ref"):
            if run_one(hero_id, kind):
                success += 1
            else:
                fails.append((hero_id, kind))
    print(f"\n결과: {success}/6 성공, 실패 {fails}", flush=True)
    return 0 if not fails else 1


if __name__ == "__main__":
    if len(sys.argv) == 1:
        sys.exit(run_all())
    if len(sys.argv) == 3:
        hero_id, kind = sys.argv[1], sys.argv[2]
        if hero_id not in HEROES or kind not in ("idle", "psb_ref"):
            print(f"Usage: python3 {sys.argv[0]} [HeroWarrior|HeroArcher|HeroMage] [idle|psb_ref]", file=sys.stderr)
            sys.exit(1)
        sys.exit(0 if run_one(hero_id, kind) else 1)
    print(f"Usage: python3 {sys.argv[0]} [hero_id] [kind]", file=sys.stderr)
    sys.exit(1)
