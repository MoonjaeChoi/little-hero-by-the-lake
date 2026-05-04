# ComfyUI REST API — quick reference

> The `scripts/generate.py` uses these endpoints. Read this only when you need to extend
> the script or debug an unexpected response.

The remote ComfyUI server runs at `192.168.75.194:8188`, exposed locally via SSH tunnel:

```
ssh -L 8188:localhost:8188 pro301@192.168.75.194 -N
```

So `http://localhost:8188` from the dev machine reaches it.

## Why the SSH tunnel?

ComfyUI on the remote machine binds to **`127.0.0.1:8188`** (loopback), not `0.0.0.0:8188`. That means port 8188 is **not reachable** from any other machine — only from `pro301@192.168.75.194` itself. This is the safe default: ComfyUI has no auth, so exposing it on a LAN/public interface would let anyone queue prompts, exhaust GPU, or read other users' outputs.

The SSH tunnel solves this without changing ComfyUI's bind:

```
[dev mac]  http://localhost:8188   ──┐
                                     │ encrypted SSH connection (port 22)
                                     ▼
[192.168.75.194]  sshd  ──forwards──▶  127.0.0.1:8188 (ComfyUI)
```

`ssh -L 8188:localhost:8188 ...` tells ssh: "open a listener on **my** localhost:8188; whenever something connects, tunnel that connection through ssh and hand it to **the remote's** localhost:8188". So:
- The dev machine treats ComfyUI as if it were running locally.
- ComfyUI never sees external traffic — it only ever sees its own loopback.
- All traffic between dev mac and remote is wrapped in the SSH session (encrypted, authenticated by your SSH key/password).
- When you close ssh, the tunnel closes — no lingering exposure.

The script connects to `http://localhost:8188` because that's the dev-side end of the tunnel. If the tunnel is down, the script fails fast with `Connection refused` — that's the signal to re-establish ssh.

If you ever wanted to skip the tunnel, you'd have to either (a) bind ComfyUI to `0.0.0.0` (insecure on shared networks) or (b) put a real auth proxy in front (nginx + basic auth, etc.). The tunnel is simpler and inherits your SSH credentials.

## Endpoints

### `POST /prompt` — queue a workflow

Request body:
```json
{
  "prompt": { "<node_id>": { "class_type": "...", "inputs": { ... } }, ... },
  "client_id": "<uuid>"
}
```

Response (202):
```json
{
  "prompt_id": "<uuid>",
  "number": 0,
  "node_errors": {}
}
```

400 if the workflow has invalid nodes — surface the body as the error message.

### `GET /history/{prompt_id}` — poll execution status

Returns `{}` while still queued/running, or:
```json
{
  "<prompt_id>": {
    "prompt": [...],
    "outputs": {
      "<save_node_id>": {
        "images": [
          {"filename": "ComfyUI_00001_.png", "subfolder": "", "type": "output"}
        ]
      }
    },
    "status": {"status_str": "success", "completed": true, "messages": [...]}
  }
}
```

Polling cadence: 1 second is fine. Keep total wait bounded (default 300s).

### `GET /view?filename=X&subfolder=&type=output` — fetch image bytes

Response is `image/png` (or whatever the SaveImage node produced).

### `GET /system_stats` — health check

Cheap probe to verify the server is up. Use this to confirm tunnel + ComfyUI are alive.

### `GET /object_info` — node schema (rarely needed)

Returns metadata for every node type. Useful when debugging unknown node ids.

## Workflow API format vs UI format

ComfyUI exports two JSON formats:

| Format | Source | What it is |
|---|---|---|
| **API format** | `Settings (gear) → Save (API Format)` | Map of `{node_id: {class_type, inputs}}`. **This is what `/prompt` accepts.** |
| **UI format** | `Save` button (top toolbar) | Includes layout, links, group boxes, widget UI state. NOT directly accepted by `/prompt`. |

Always export API format for use with this skill.

## Standard SDXL node id assumptions (script defaults)

| Role | id | class_type |
|---|---|---|
| Positive prompt | `6` | `CLIPTextEncode` |
| Negative prompt | `7` | `CLIPTextEncode` |
| KSampler (seed/steps/cfg) | `3` | `KSampler` |
| EmptyLatentImage (size) | `5` | `EmptyLatentImage` or `EmptySD3LatentImage` |
| SaveImage (output) | `9` | `SaveImage` |

The script auto-detects (first CLIPTextEncode = positive, second = negative, first KSampler = seed). Pass `--positive-node`, `--negative-node`, `--seed-node`, `--latent-node` to override if your workflow differs.

## LayerDiffuse 노드 (huchenlei/ComfyUI-layerdiffuse)

The `assets/workflow_layerdiffuse.json` workflow uses the [huchenlei/ComfyUI-layerdiffuse](https://github.com/huchenlei/ComfyUI-layerdiffuse) Custom Node for native RGBA SDXL output. Two nodes are added on top of the standard SDXL pipeline:

| Node id | class_type | Role |
|---|---|---|
| `10` | `LayeredDiffusionApply` | Patches the model with transparent diffusion (config: `"SDXL, Conv Injection"` for SDXL) |
| `11` | `LayeredDiffusionDecode` | Decodes latent + RGB image to RGBA (replaces / supplements VAEDecode for the final SaveImage input) |

### Node wiring (LayerDiffuse)

```
CheckpointLoader (4) ──────────┐
                               ▼
                 LayeredDiffusionApply (10) ────┐
                                                ▼
CLIPTextEncode pos (6) ──┐                 KSampler (3)
CLIPTextEncode neg (7) ──┤                      │
EmptyLatentImage (5) ────┘                      ▼
                                           VAEDecode (8) ──┐
                                                           ▼
                                  LayeredDiffusionDecode (11) ──→ SaveImage (9)
```

The `LayeredDiffusionDecode` node takes both the sampler latent AND the VAE-decoded RGB to produce final RGBA — this is how the alpha channel is recovered without dark BG flood-fill.

### Class names by version

The exact class_type names can change between LayerDiffuse versions. The shipped JSON uses `LayeredDiffusionApply` / `LayeredDiffusionDecode` (huchenlei v1.0+). If the server returns a 400 with "node class not found", export a fresh workflow from the server's ComfyUI UI (Settings → Save API Format) after dropping the LayerDiffuse nodes from the node browser, then copy that JSON over `workflow_layerdiffuse.json`.

### Fallback workflow alpha limit

When LayerDiffuse is not available, `workflow_template.json` runs without it. Output is RGB on a (model-implied) dark background — **not** native RGBA. Recovering alpha requires post-processing (flood-fill from corners with tolerance), which has ~80% accuracy and can leak on dark interior pixels (eyes, hair, shadows). This was the root cause of the "1라운드 적 흰 박스" report before 5008.

## Common failure modes

- **Connection refused on localhost:8188** → tunnel is not running. Re-establish.
- **400 on POST /prompt** → workflow JSON has missing/invalid inputs (e.g. unloaded checkpoint name, missing model file). Re-export from a known-good ComfyUI session.
- **History `outputs` empty after timeout** → workflow is running too slow (large image / many steps), or no SaveImage node. Raise `--timeout` or check the workflow ends in SaveImage.
- **`/view` 404** → file expired from ComfyUI's output cache, or wrong subfolder. Check the `images[0]` entry from `/history`.
