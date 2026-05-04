---
name: comfyui-image
description: Generate a single PNG image via the ComfyUI server at 192.168.75.194 (accessed through SSH tunnel localhost:8188). Use this skill whenever the user asks to generate, render, or create an image, illustration, sprite, icon, sticker, button asset, or art from a text description — including phrases like "ComfyUI 로 이미지 생성", "프롬프트 X 로 이미지 생성", "5006.1 PNG 생성", "V5 sticker 톤으로 X 그려줘", "버튼 아이콘 만들어줘", "그림 그려줘", or any request to render an image from a prompt. The skill submits a workflow to ComfyUI, polls for completion, downloads the resulting PNG, and saves it locally. Use even if the user doesn't explicitly mention "ComfyUI" — if they want a generated image and a tunnel is available, this is the right tool.
---

# ComfyUI Image Generation (single)

## When to use

Trigger when the user asks to **generate one image** from a text prompt and wants the result saved locally. Typical phrases:

- "ComfyUI 로 X 이미지 생성"
- "프롬프트 X 로 이미지 만들어줘"
- "버튼 아이콘 (gacha / inventory / diary / etc) 만들어줘"
- "5006.1 PNG 생성" (Little Hero Attic button icons, V5 sticker tone)
- "V5 sticker 톤으로 X 그려줘"

For batch (N images at once) the user can call this skill in a loop, or write a wrapper script. The skill itself handles **one image per call** — keep it simple.

## Prerequisites (verify before first run)

The skill assumes the user has already set up a SSH tunnel from local `8188` to remote ComfyUI. Verify:

```bash
# Is the tunnel alive?
lsof -i :8188 | grep LISTEN
# Or curl the ComfyUI endpoint
curl -s http://localhost:8188/system_stats | head -c 200
```

If not alive, instruct the user to start it (this skill does NOT establish the tunnel itself):

```bash
ssh -L 8188:localhost:8188 pro301@192.168.75.194 -N
```

The skill also requires a **ComfyUI workflow JSON in API format**. ComfyUI's UI exports this via `Settings → Save (API Format)`. The bundled `assets/workflow_template.json` is a generic SDXL placeholder — replace it with a project-specific workflow (V5 sticker tone, model checkpoint, LoRAs) for production use.

## Quick path

The bundled `scripts/generate.py` does everything needed. Run it via Bash:

```bash
python3 .claude/skills/comfyui-image/scripts/generate.py \
  --prompt "cute cartoon diary book with floating photos, V5 sticker dynamic tone, transparent background" \
  --output ./output/btn_diary.png
```

Output is the saved PNG path (printed on success). If the command exits 0 and the file exists, success — show the path to the user. If it errors, surface the message and suggest re-checking the tunnel.

## Parameters (script CLI)

Required:
- `--prompt "<text>"` — positive prompt. Describes the image.
- `--output <path>` — where to save the resulting PNG (parent dir auto-created).

Optional (sensible defaults):
- `--negative "<text>"` — negative prompt. Default: `"low quality, blurry, watermark, text"`
- `--seed <int>` — sampler seed. Default `22222` (Little Hero V5 tone batch consistency). Use `-1` for random.
- `--width <int>` / `--height <int>` — image size. Default `1024 1024`.
- `--steps <int>` — sampling steps. Default `8` (DreamShaperXL Turbo). Use `25` for SDXL base, `4` for SDXL Turbo, `~30` for SD1.5.
- `--cfg <float>` — classifier-free guidance scale. Default `2.0` (DreamShaperXL Turbo). Use `7.0` for SDXL base, `1.0` for SDXL Turbo.
- `--workflow <path>` — workflow JSON. Default: bundled `assets/workflow_template.json`.
- `--server <url>` — ComfyUI base URL. Default `http://localhost:8188`.
- `--timeout <sec>` — max wait for completion. Default `300`.
- `--positive-node <id>` / `--negative-node <id>` / `--seed-node <id>` / `--latent-node <id>` — override workflow node IDs if your workflow uses non-standard ones. The script auto-detects standard SDXL node IDs (positive `6` / negative `7` / sampler `3` / latent `5`) but custom workflows may differ.

## How the script works

1. Loads the workflow JSON and patches the prompt / negative / seed / size fields by node id.
2. POSTs the workflow to `/prompt` — receives `prompt_id`.
3. Polls `/history/{prompt_id}` every 1s up to `--timeout`.
4. Once `outputs` contains an `images` entry, downloads the first image via `/view?filename=...`.
5. Writes the bytes to `--output` and prints the absolute path.

Stdlib only (no `requests` dependency) — uses `urllib.request` so it runs anywhere with Python 3.8+.

## LayerDiffuse 워크플로 (5008 — RGBA native transparency)

For sticker / sprite generation that needs **clean RGBA output** (no dark BG / flood-fill post-processing), use the LayerDiffuse workflow shipped at `assets/workflow_layerdiffuse.json`.

### Prerequisites

Install the [huchenlei/ComfyUI-layerdiffuse](https://github.com/huchenlei/ComfyUI-layerdiffuse) Custom Node on the ComfyUI server (192.168.75.194). Once installed, the workflow's `LayeredDiffusionApply` + `LayeredDiffusionDecode` nodes become available.

### Usage

```bash
python3 .claude/skills/comfyui-image/scripts/generate.py \
  --workflow .claude/skills/comfyui-image/assets/workflow_layerdiffuse.json \
  --prompt "<your sticker prompt>" \
  --seed 22222 \
  --output ./output/sprite.png
```

The workflow keeps the standard SDXL node ids (positive `6`, negative `7`, sampler `3`, latent `5`) so the script's auto-detection still works.

### Fallback limit (when LayerDiffuse node not installed)

If the LayerDiffuse node is unavailable on the server, fall back to the default `assets/workflow_template.json`. **Be aware of the limit**:

- DreamShaperXL Turbo has **no native alpha output**.
- The fallback workflow generates with a **dark background** and relies on **flood-fill post-processing** to recover transparency.
- Flood-fill alpha accuracy is **~80%** (anti-aliased edges on dark hair / dark colors can leak).
- For full RGBA fidelity (especially on sprites with dark interior pixels), **install LayerDiffuse**.

The skill itself does not auto-fallback — passing `--workflow workflow_layerdiffuse.json` to a server without the node will return a 400. Choose the workflow explicitly based on what the server supports.

## Customizing for Little Hero V5 tone

The default `assets/workflow_template.json` is a placeholder. To produce actual V5 sticker dynamic tone images:

1. Open ComfyUI in a browser (forwarded `http://localhost:8188`).
2. Build or load the V5 sticker workflow (checkpoint + LoRAs + your tone settings).
3. `Settings (gear) → Save (API Format)` — saves a JSON.
4. Copy that JSON over `assets/workflow_template.json` (or pass `--workflow <path>`).
5. Note the node ids of:
   - the positive `CLIPTextEncode` (default assumes `"6"`)
   - the negative `CLIPTextEncode` (default assumes `"7"`)
   - the `KSampler` for seed (default `"3"`)
   - the `EmptyLatentImage` for width/height (default `"5"`)
   If they differ, pass `--positive-node N` etc.

## Worked example — generating an Attic button icon

```bash
python3 .claude/skills/comfyui-image/scripts/generate.py \
  --prompt "diary book with floating polaroid photos, warm beige tones, soft cartoon shading, transparent background, sticker style, no text" \
  --negative "low quality, blurry, watermark, photorealistic, dark, scary, blood" \
  --seed 22222 \
  --width 512 --height 512 \
  --output LittleHero/Assets/_Project/Art/Generated_v2/UI/AtticButtons/btn_diary.png
```

The seed `22222` matches the documented Little Hero V5 batch (changelog references). PG-12 tone — keep negative prompts blocking dark / scary / blood content (CLAUDE.md §2).

## Connection / failure handling

| Symptom | Cause | Fix |
|---|---|---|
| `Connection refused` on localhost:8188 | SSH tunnel down | `ssh -L 8188:localhost:8188 user@192.168.75.194 -N` |
| `400` from `/prompt` | Workflow JSON has missing/invalid nodes | Re-export from ComfyUI (Save API Format) |
| Polling times out | Server busy / model loading | Raise `--timeout` (default 300s) |
| `'images' not in outputs` | Workflow has no SaveImage node | Ensure workflow ends in `SaveImage` |

When the script fails, **report the error to the user verbatim** rather than retrying silently — the user usually needs to fix the tunnel or workflow.

## Out of scope

- Establishing or restarting the SSH tunnel (the user owns this).
- Multi-image batch (call the script N times instead — single-image keeps each call simple and lets the user inspect intermediate results).
- Image post-processing (cropping, scaling, alpha cleanup) — chain other tools.

## Sprite Generation Rules (single-character RGBA)

For sprite work — Little Hero 적/캐릭터/아이콘 등 **단일 객체 RGBA** — **always read `references/sprite_generation_rules.md` first**. It documents the verified combination (SDXL base 1.0 + steps 20 + cfg 7.0 + euler/normal + negative prompt with 11 grid-prevention keywords) that fixes silhouette / 4-character grid / RGB 평탄화 문제 (5008 T05.5 fix sycle 검증).

Quick rule of thumb for sprite work:
- Always pass `--workflow workflow_layerdiffuse.json --steps 20 --cfg 7.0`
- Use the negative prompt template from `references/sprite_generation_rules.md` §4
- Use `three-quarter view` instead of `isometric playful angle` (grid-prone)
- After generation, verify alpha + RGB std + 4-quadrant distribution per §6

## References

- `references/comfyui_api.md` — ComfyUI REST API endpoints used by the script (read this if the script breaks or needs extension).
- `references/sprite_generation_rules.md` — **Sprite RGBA 단일 캐릭터 생성 규칙** (5008 T05.5 fix 검증). 적/캐릭터/아이콘 sprite 작업 시 필독.
