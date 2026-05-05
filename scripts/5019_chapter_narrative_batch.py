#!/usr/bin/env python3
"""5019 Chapter Narrative-specific batch generator.

35 챕터 × 6 archetype = 210 sprite narrative-specific 재생성.
5008 baseline + 5009 batch 답습 + chapter narrative dict.

Usage: python3 5019_chapter_narrative_batch.py <sprint_num>

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
    "v5 sticker tone, cute cartoon, chunky thick white outline, "
    "soft cartoon shading, single character, transparent background, "
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

# 35 챕터 × 6 archetype narrative dict
# Source: docs/prd/5019/5019_prompt_table.md
NARRATIVES = {
    1: {  # 호숫가 작은 마을
        "Boss":   "lake giant guardian with reed crown, friendly water spirit, lakeside sunset tones",
        "Large":  "big rainbow lake fish, scaly trout with bubbles, pond water tones",
        "Medium": "village dog with brown fur, friendly puppy character, warm beige tones",
        "MiniA":  "small sparrow bird with white belly, cheerful flying creature",
        "MiniB":  "lakeside green frog with lily pad hat, pond friend",
        "Small":  "tiny firefly with glowing amber light, twinkling sprite",
    },
    2: {  # 다락방 깊숙한 곳
        "Boss":   "dusty attic giant, papercraft body with old cloth, sepia and brown tones",
        "Large":  "giant book fairy with pages for wings, dusty library tones",
        "Medium": "dust mote spirit, fluffy gray cloud creature with sleepy eyes",
        "MiniA":  "friendly attic spider with tiny boots, web-spinner",
        "MiniB":  "tiny paper crane spirit, origami friend, cream tones",
        "Small":  "small mouse with red bow tie, attic dweller",
    },
    3: {  # 잊힌 학교
        "Boss":   "giant chalk monster teacher, white chalkboard body, classroom tones",
        "Large":  "flying schoolbag spirit with eyes, leather brown tones",
        "Medium": "chalk doll with sailor uniform, school child character",
        "MiniA":  "paper airplane scout with smile, white and blue",
        "MiniB":  "ink spirit with feather pen, dark blue droplet character",
        "Small":  "tiny eraser friend, pink rubber buddy",
    },
    4: {  # 마당의 옛 우물
        "Boss":   "well guardian giant, mossy stone body with vines, deep green tones",
        "Large":  "big bucket spirit with rope hair, wooden brown",
        "Medium": "well pond frog elder with crown, deep green",
        "MiniA":  "moss sprite with tiny leaves, emerald green character",
        "MiniB":  "water droplet spirit with smile, blue translucent",
        "Small":  "tiny pebble friend, gray rock buddy with eyes",
    },
    5: {  # 아빠의 비밀 기지
        "Boss":   "secret base bear giant with lantern, camping companion, warm tones",
        "Large":  "big paper plane glider with wings, white and blue",
        "Medium": "flashlight spirit with glowing head, yellow and silver",
        "MiniA":  "tiny camping bear with backpack, brown fur friend",
        "MiniB":  "small map spirit, parchment yellow with compass",
        "Small":  "tiny torch flame, orange flickering character",
    },
    6: {  # 안개 골짜기
        "Boss":   "misty valley giant deer, antlers with glowing fog, misty gray-blue",
        "Large":  "giant misty owl with starry eyes, smoke gray",
        "Medium": "mist sprite with cloud body, soft white-blue",
        "MiniA":  "small forest deer fawn, tan with white spots",
        "MiniB":  "foggy mushroom friend, gray cap with eyes",
        "Small":  "tiny dewdrop spirit, sparkling clear sphere",
    },
    7: {  # 깊은 숲
        "Boss":   "forest bear giant with leaf crown, mighty woodland guardian",
        "Large":  "big fox with bushy tail, autumn orange and red",
        "Medium": "forest squirrel with acorn, warm brown",
        "MiniA":  "small bunny with leaf hat, soft beige",
        "MiniB":  "tiny hedgehog friend, brown spiky",
        "Small":  "small acorn sprite with eyes, tan and brown",
    },
    8: {  # 노래하는 폭포
        "Boss":   "waterfall spirit giant with flowing hair, aquamarine and white",
        "Large":  "big rainbow trout fish with bubbles, river tones",
        "Medium": "singing bluebird with musical notes, sky blue",
        "MiniA":  "small water sprite with droplet body, translucent blue",
        "MiniB":  "tiny river otter with fish, brown wet fur",
        "Small":  "small bubble friend, iridescent sphere",
    },
    9: {  # 반딧불 풀밭
        "Boss":   "giant firefly queen with glowing crown, warm amber and green",
        "Large":  "big grass spirit with leaf body, fresh green",
        "Medium": "meadow rabbit with daisy crown, white fur",
        "MiniA":  "small firefly with twinkling tail, golden glow",
        "MiniB":  "tiny grasshopper friend, leaf green",
        "Small":  "small dandelion spirit, fluffy white seeds",
    },
    10: {  # 모래 회오리 사막
        "Boss":   "sand giant with cactus crown, sandstorm body, golden yellow",
        "Large":  "big sand fox with golden fur, fennec character",
        "Medium": "desert lizard with patterned scales, sandy beige",
        "MiniA":  "small cactus sprite with flower, green and pink",
        "MiniB":  "tiny sand snake with smile, golden coil",
        "Small":  "small sand pebble friend, beige round character",
    },
    11: {  # 얼음 동굴
        "Boss":   "ice cave giant penguin king with crystal crown, cool blue and white",
        "Large":  "big polar bear with snowflakes, fluffy white fur",
        "Medium": "snow spirit with icy body, translucent blue-white",
        "MiniA":  "small penguin chick with scarf, black and white",
        "MiniB":  "tiny snow rabbit with red nose, fluffy white",
        "Small":  "small ice crystal sprite, sparkling cyan",
    },
    12: {  # 산호초 바다
        "Boss":   "coral reef king octopus with pearl crown, magenta and orange",
        "Large":  "big colorful clownfish with bubbles, orange and white stripes",
        "Medium": "jellyfish sprite with translucent body, soft pink glow",
        "MiniA":  "small seahorse friend, golden yellow",
        "MiniB":  "tiny starfish character, pink with smile",
        "Small":  "small coral polyp, magenta with eyes",
    },
    13: {  # 화산 분화구 가장자리
        "Boss":   "volcano salamander king with lava mane, deep red and orange",
        "Large":  "big lava lizard with glowing belly, dark red and orange",
        "Medium": "flame spirit with fiery hair, warm orange-red",
        "MiniA":  "small black raven with red eyes, dark feathers",
        "MiniB":  "tiny ember sprite, glowing red coal",
        "Small":  "small ash spirit, gray smoky character",
    },
    14: {  # 하늘 구름 다리
        "Boss":   "giant cloud sheep with rainbow wool, fluffy pastel pink",
        "Large":  "big sky bird with wide wings, white and blue",
        "Medium": "rainbow spirit with arch body, multicolor pastel",
        "MiniA":  "small cloud lamb, fluffy white",
        "MiniB":  "tiny rain sprite with droplet, sky blue",
        "Small":  "small sun ray sprite, golden warm",
    },
    15: {  # 토끼굴 깊은 곳
        "Boss":   "giant white rabbit lord with pocket watch, tea party tones",
        "Large":  "big card soldier rabbit, red heart pattern",
        "Medium": "tea party fairy rabbit, pastel pink dress",
        "MiniA":  "small white rabbit with vest, classic Alice character",
        "MiniB":  "tiny pocket watch sprite, golden gear",
        "Small":  "small teapot friend with eyes, blue ceramic",
    },
    16: {  # 새들의 둥지 도시
        "Boss":   "giant magpie king with golden crown, black white and gold",
        "Large":  "big owl scholar with glasses, brown feathers",
        "Medium": "bluebird postman with mail bag, sky blue",
        "MiniA":  "small sparrow chick, brown speckled",
        "MiniB":  "tiny robin with red breast, autumn red",
        "Small":  "small egg with face, cream white",
    },
    17: {  # 빛나는 해파리 정원
        "Boss":   "giant glowing jellyfish queen with tentacles, neon pink and purple",
        "Large":  "big bioluminescent fish, electric blue glow",
        "Medium": "jellyfish dancer with flowing body, soft pink translucent",
        "MiniA":  "small glow shrimp with antennae, electric green",
        "MiniB":  "tiny sea sprite, glowing purple orb",
        "Small":  "small luminous coral, pink phosphorescent",
    },
    18: {  # 고래 노래 바다
        "Boss":   "giant whale singer with musical bubbles, deep ocean blue",
        "Large":  "big dolphin friend with smile, silver-blue",
        "Medium": "sea turtle elder with mossy shell, deep green",
        "MiniA":  "small fish school, silver schooling",
        "MiniB":  "tiny seal pup, gray fluffy",
        "Small":  "small bubble note, blue musical",
    },
    19: {  # 거대 거북이 등
        "Boss":   "giant ancient turtle with mossy shell, deep green and brown",
        "Large":  "big monitor lizard with scales, sandy brown",
        "Medium": "hermit crab with spiral shell, warm orange",
        "MiniA":  "small green turtle baby, soft jade",
        "MiniB":  "tiny crab friend with claws, red-orange",
        "Small":  "small barnacle friend, gray clustered",
    },
    20: {  # 별빛 늑대 산골
        "Boss":   "giant starlight wolf with constellation fur, deep purple and silver",
        "Large":  "big mountain bear, dark brown fur",
        "Medium": "mountain deer with antlers, tan brown",
        "MiniA":  "small wolf cub with star marking, gray fur",
        "MiniB":  "tiny mountain goat, white and brown",
        "Small":  "small starlight sprite, twinkling silver",
    },
    21: {  # 등불 시장
        "Boss":   "lantern festival giant, paper lantern body, warm orange-red",
        "Large":  "big festival drummer, traditional drum and ribbon",
        "Medium": "lantern spirit with flickering light, paper and gold",
        "MiniA":  "small market vendor sprite with hat, beige and red",
        "MiniB":  "tiny rice cake friend, white round soft",
        "Small":  "small candle flame sprite, warm yellow",
    },
    22: {  # 사탕 공장
        "Boss":   "candy factory giant gummy bear, rainbow pastel sweet tones",
        "Large":  "big chocolate bunny with cocoa fur, brown and pink",
        "Medium": "candy doll with lollipop hair, pastel mint and pink",
        "MiniA":  "small caramel sprite, golden brown sweet",
        "MiniB":  "tiny gumdrop friend, jelly red sphere",
        "Small":  "small sprinkle character, multicolor confetti",
    },
    23: {  # 인형 공방
        "Boss":   "porcelain doll queen with pink dress, fragile white ceramic",
        "Large":  "big rag doll with patch eyes, beige fabric",
        "Medium": "wooden marionette doll, painted brown",
        "MiniA":  "small button-eye doll, soft cotton",
        "MiniB":  "tiny thread spool sprite, colorful yarn",
        "Small":  "small needle pin friend, silver shiny",
    },
    24: {  # 우편 비행기 마을
        "Boss":   "giant mail plane pilot bear, leather brown and red",
        "Large":  "big paper plane with letter wings, white and gold",
        "Medium": "mailbox spirit with letter slot, red metal",
        "MiniA":  "small postman sprite with cap, navy uniform",
        "MiniB":  "tiny envelope friend, cream paper",
        "Small":  "small stamp sprite, postal red",
    },
    25: {  # 두루마리 미로
        "Boss":   "giant scroll keeper with parchment robes, ancient sepia tones",
        "Large":  "big book fairy with floating pages, dusty brown",
        "Medium": "ink spirit with brush body, deep black-blue",
        "MiniA":  "small parchment sprite, aged cream",
        "MiniB":  "tiny quill pen friend, feather white",
        "Small":  "small ink droplet, deep blue character",
    },
    26: {  # 음악 동굴
        "Boss":   "music cave giant conductor, musical note crown, deep purple",
        "Large":  "big song bird with sheet music wings, golden yellow",
        "Medium": "musical note sprite with smile, treble clef shape",
        "MiniA":  "small bell friend, brass golden chime",
        "MiniB":  "tiny harp sprite, golden strings",
        "Small":  "small piano key character, black and white",
    },
    27: {  # 종이 인형 무대
        "Boss":   "paper puppet master giant with strings, dramatic red and black",
        "Large":  "big paper marionette with sequined dress, sparkly silver",
        "Medium": "stage mask spirit, dramatic gold and red",
        "MiniA":  "small paper doll dancer, pastel pink dress",
        "MiniB":  "tiny ribbon spirit, flowing red silk",
        "Small":  "small balloon friend, helium pink",
    },
    28: {  # 살아있는 정원
        "Boss":   "living garden giant pumpkin king, vine crown, autumn orange",
        "Large":  "big fantasy butterfly with patterned wings, lavender purple",
        "Medium": "plant fairy with flower hair, emerald green and pink",
        "MiniA":  "small mushroom sprite, red cap with white spots",
        "MiniB":  "tiny dewdrop fairy, sparkling translucent",
        "Small":  "small flower bud, pastel pink unfurling",
    },
    29: {  # 거울 갤러리
        "Boss":   "mirror gallery giant reflection spirit, silver and crystal",
        "Large":  "big shadow figure with cloak, deep purple-black",
        "Medium": "crystal shard sprite, prism rainbow",
        "MiniA":  "small light fairy, glowing white-yellow",
        "MiniB":  "tiny mirror frame friend, gold ornate",
        "Small":  "small reflection orb, silver mercury",
    },
    30: {  # 거대한 시계탑 안
        "Boss":   "clock tower giant with gear armor, bronze and copper mechanical",
        "Large":  "big pendulum spirit, brass swinging",
        "Medium": "gear sprite with cogwheel body, antique brown",
        "MiniA":  "small clock hand fairy, golden minute",
        "MiniB":  "tiny bell spirit, deep bronze ringing",
        "Small":  "small spring coil sprite, silver tight wound",
    },
    31: {  # 시간이 멈춘 마을
        "Boss":   "time-stopped giant, frozen pose with hourglass, sepia tones",
        "Large":  "big frozen statue figure, gray stone with eyes",
        "Medium": "paused villager doll, soft beige standing still",
        "MiniA":  "small clockwork bird, mechanical golden",
        "MiniB":  "tiny hourglass sprite, sand falling slowly",
        "Small":  "small frozen leaf, autumn red suspended",
    },
    32: {  # 거꾸로 매달린 지하 도시
        "Boss":   "upside-down cave giant bat king, dark purple wings",
        "Large":  "big cave miner with helmet, gray and yellow",
        "Medium": "crystal cave spirit, glowing geode purple",
        "MiniA":  "small bat with friendly face, dark fluffy",
        "MiniB":  "tiny cave mushroom, glowing blue luminescent",
        "Small":  "small stalactite friend, gray pointed",
    },
    33: {  # 달의 뒷면
        "Boss":   "moon giant rabbit with mochi, pearly white and silver",
        "Large":  "big moon spirit with crescent crown, soft glow silver",
        "Medium": "lunar fairy with star wand, pale blue-white",
        "MiniA":  "small moon rabbit baby, white fluffy",
        "MiniB":  "tiny shooting star, golden trailing",
        "Small":  "small moon dust sprite, sparkly gray",
    },
    34: {  # 노을 들판
        "Boss":   "sunset field giant golden deer with horn rays, orange-pink",
        "Large":  "big sunset bird with flame feathers, warm orange-red",
        "Medium": "sunflower spirit with smiley face, golden yellow",
        "MiniA":  "small sunset bunny with red cheeks, peach orange",
        "MiniB":  "tiny gold cloud, fluffy peach",
        "Small":  "small wheat sprite, golden grain",
    },
    35: {  # 별과 별 사이
        "Boss":   "constellation guardian giant with starry cape, deep purple and gold cosmic",
        "Large":  "big galaxy whale with starry skin, purple-blue cosmic",
        "Medium": "star spirit with five-point body, twinkling gold",
        "MiniA":  "small comet sprite with tail, silver streak",
        "MiniB":  "tiny planet friend, ringed Saturn-like",
        "Small":  "small stardust sprite, sparkling multi-color",
    },
}

# Sprint 매핑: (sprint_num, name, chapter range)
SPRINTS = {
    1: ("Meta",    range(1, 6)),
    2: ("Nature",  range(6, 15)),
    3: ("Animal",  range(15, 21)),
    4: ("Civ",     range(21, 28)),
    5: ("Fantasy", range(28, 36)),
}

SEED_BASE = 90000  # 5009 (22222) / 5010 (11001) / 5011 (자체) 충돌 회피


def generate_one(prompt, negative, seed, output):
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
    name, chapters = SPRINTS[num]
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    chapter_count = len(list(chapters))
    print(f"=== Sprint {num} — {name} ({chapter_count} 챕터 × 6 = {chapter_count*6} PNG) ===", flush=True)

    success = 0
    fails = []
    for chapter in chapters:
        narrative = NARRATIVES.get(chapter)
        if narrative is None:
            print(f"  ⚠️ CH{chapter:02d} narrative 누락", flush=True)
            continue
        chapter_seed = SEED_BASE + (chapter - 1) * 6
        for idx, arch in enumerate(ARCH_ORDER):
            seed = chapter_seed + idx
            arch_narrative = narrative[arch]
            prompt = f"{BASE_PROMPT}, {arch_narrative}"
            output = OUT_DIR / f"CH{chapter:02d}_{arch}.png"
            ok = generate_one(prompt, NEGATIVE, seed, output)
            if ok:
                success += 1
            else:
                fails.append((chapter, arch, seed))

    print(f"\nSprint {num} ({name}): {success} 성공 / {len(fails)} 실패", flush=True)
    if fails:
        print(f"실패: {fails}", flush=True)
        return 1
    return 0


if __name__ == "__main__":
    if len(sys.argv) != 2 or sys.argv[1] not in ("1", "2", "3", "4", "5"):
        print("Usage: python3 5019_chapter_narrative_batch.py <1|2|3|4|5>", file=sys.stderr)
        sys.exit(1)
    sys.exit(run_sprint(int(sys.argv[1])))
