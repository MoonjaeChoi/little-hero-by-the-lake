# ComfyUI REST API — quick reference

> The `scripts/generate.py` uses these endpoints. Read this only when you need to extend
> the script or debug an unexpected response.

The remote ComfyUI server runs at `192.168.75.194:8188`, exposed locally via SSH tunnel:

```
ssh -L 8188:localhost:8188 <user>@192.168.75.194 -N
```

So `http://localhost:8188` from the dev machine reaches it.

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

## Common failure modes

- **Connection refused on localhost:8188** → tunnel is not running. Re-establish.
- **400 on POST /prompt** → workflow JSON has missing/invalid inputs (e.g. unloaded checkpoint name, missing model file). Re-export from a known-good ComfyUI session.
- **History `outputs` empty after timeout** → workflow is running too slow (large image / many steps), or no SaveImage node. Raise `--timeout` or check the workflow ends in SaveImage.
- **`/view` 404** → file expired from ComfyUI's output cache, or wrong subfolder. Check the `images[0]` entry from `/history`.
