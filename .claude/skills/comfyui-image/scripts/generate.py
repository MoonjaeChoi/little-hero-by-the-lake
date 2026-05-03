#!/usr/bin/env python3
"""
ComfyUI single-image generator.

Submits a workflow to a ComfyUI server (default http://localhost:8188 — assumed to be
a SSH tunnel forwarded port), polls until completion, downloads the first output PNG,
and writes it to --output.

Stdlib only (no `requests`). Python 3.8+.
"""
import argparse
import json
import os
import sys
import time
import uuid
import urllib.request
import urllib.parse
import urllib.error
from pathlib import Path


def http_post_json(url: str, body: dict, timeout: int = 30) -> dict:
    data = json.dumps(body).encode("utf-8")
    req = urllib.request.Request(url, data=data, method="POST")
    req.add_header("Content-Type", "application/json")
    with urllib.request.urlopen(req, timeout=timeout) as resp:
        return json.loads(resp.read().decode("utf-8"))


def http_get_json(url: str, timeout: int = 30) -> dict:
    with urllib.request.urlopen(url, timeout=timeout) as resp:
        return json.loads(resp.read().decode("utf-8"))


def http_get_bytes(url: str, timeout: int = 60) -> bytes:
    with urllib.request.urlopen(url, timeout=timeout) as resp:
        return resp.read()


def load_workflow(path: Path) -> dict:
    with open(path, "r", encoding="utf-8") as f:
        wf = json.load(f)
    if not isinstance(wf, dict):
        raise ValueError(f"Workflow JSON must be a dict (API format), got {type(wf)}")
    return wf


def auto_detect_nodes(wf: dict) -> dict:
    """Best-effort: scan workflow for standard node types and return id mapping."""
    detected = {"positive": None, "negative": None, "seed": None, "latent": None}
    text_nodes = []
    for node_id, node in wf.items():
        ctype = node.get("class_type") if isinstance(node, dict) else None
        if ctype == "CLIPTextEncode":
            text_nodes.append((node_id, node.get("inputs", {}).get("text", "")))
        elif ctype == "KSampler" and detected["seed"] is None:
            detected["seed"] = node_id
        elif ctype in ("EmptyLatentImage", "EmptySD3LatentImage") and detected["latent"] is None:
            detected["latent"] = node_id
    # Heuristic: assume the first CLIPTextEncode is positive, second is negative.
    if len(text_nodes) >= 1:
        detected["positive"] = text_nodes[0][0]
    if len(text_nodes) >= 2:
        detected["negative"] = text_nodes[1][0]
    return detected


def patch_workflow(wf: dict, args, ids: dict) -> dict:
    """Mutate workflow in place to inject prompt / negative / seed / width / height."""
    def set_input(node_id, key, value):
        if not node_id:
            return
        node = wf.get(node_id)
        if not isinstance(node, dict):
            print(f"WARN: node id '{node_id}' not in workflow", file=sys.stderr)
            return
        node.setdefault("inputs", {})[key] = value

    set_input(ids.get("positive"), "text", args.prompt)
    set_input(ids.get("negative"), "text", args.negative)
    seed_value = args.seed if args.seed != -1 else int.from_bytes(os.urandom(4), "big")
    set_input(ids.get("seed"), "seed", seed_value)
    set_input(ids.get("seed"), "steps", args.steps)
    set_input(ids.get("seed"), "cfg", args.cfg)
    set_input(ids.get("latent"), "width", args.width)
    set_input(ids.get("latent"), "height", args.height)
    return wf


def queue_prompt(server: str, workflow: dict, client_id: str) -> str:
    url = f"{server.rstrip('/')}/prompt"
    body = {"prompt": workflow, "client_id": client_id}
    try:
        result = http_post_json(url, body, timeout=30)
    except urllib.error.HTTPError as e:
        detail = e.read().decode("utf-8", errors="replace")
        raise RuntimeError(f"POST /prompt {e.code}: {detail}") from e
    pid = result.get("prompt_id")
    if not pid:
        raise RuntimeError(f"No prompt_id in response: {result}")
    return pid


def poll_for_image(server: str, prompt_id: str, timeout: int) -> dict:
    url = f"{server.rstrip('/')}/history/{prompt_id}"
    deadline = time.time() + timeout
    while time.time() < deadline:
        try:
            hist = http_get_json(url, timeout=10)
        except urllib.error.HTTPError:
            time.sleep(1.0)
            continue
        entry = hist.get(prompt_id)
        if entry and entry.get("outputs"):
            for node_id, output in entry["outputs"].items():
                images = output.get("images") or []
                if images:
                    return images[0]
        time.sleep(1.0)
    raise TimeoutError(f"No image after {timeout}s (prompt_id={prompt_id})")


def download_image(server: str, image_info: dict) -> bytes:
    params = urllib.parse.urlencode({
        "filename": image_info["filename"],
        "subfolder": image_info.get("subfolder", ""),
        "type": image_info.get("type", "output"),
    })
    url = f"{server.rstrip('/')}/view?{params}"
    return http_get_bytes(url, timeout=60)


def main(argv=None) -> int:
    parser = argparse.ArgumentParser(description="Generate a single image via ComfyUI.")
    parser.add_argument("--prompt", required=True)
    parser.add_argument("--output", required=True, help="Output PNG path")
    parser.add_argument("--negative", default="low quality, blurry, watermark, text")
    parser.add_argument("--seed", type=int, default=22222, help="Sampler seed (-1 = random)")
    parser.add_argument("--width", type=int, default=1024)
    parser.add_argument("--height", type=int, default=1024)
    parser.add_argument("--steps", type=int, default=25)
    parser.add_argument("--cfg", type=float, default=7.0)
    parser.add_argument("--workflow", default=None,
                        help="Workflow JSON (API format). Default: assets/workflow_template.json next to this script.")
    parser.add_argument("--server", default="http://localhost:8188")
    parser.add_argument("--timeout", type=int, default=300, help="Max wait for completion (seconds)")
    parser.add_argument("--positive-node", default=None, help="Override positive CLIPTextEncode node id")
    parser.add_argument("--negative-node", default=None, help="Override negative CLIPTextEncode node id")
    parser.add_argument("--seed-node", default=None, help="Override KSampler node id")
    parser.add_argument("--latent-node", default=None, help="Override EmptyLatentImage node id")
    args = parser.parse_args(argv)

    here = Path(__file__).resolve().parent
    workflow_path = Path(args.workflow) if args.workflow else (here.parent / "assets" / "workflow_template.json")
    if not workflow_path.exists():
        print(f"ERROR: workflow not found: {workflow_path}", file=sys.stderr)
        return 2

    wf = load_workflow(workflow_path)
    ids = auto_detect_nodes(wf)
    if args.positive_node: ids["positive"] = args.positive_node
    if args.negative_node: ids["negative"] = args.negative_node
    if args.seed_node:     ids["seed"]     = args.seed_node
    if args.latent_node:   ids["latent"]   = args.latent_node

    missing = [k for k in ("positive", "seed") if not ids.get(k)]
    if missing:
        print(f"ERROR: workflow missing required nodes: {missing}. "
              f"Detected ids: {ids}", file=sys.stderr)
        return 2

    print(f"[comfyui] node ids: {ids}", file=sys.stderr)
    wf = patch_workflow(wf, args, ids)

    client_id = str(uuid.uuid4())
    print(f"[comfyui] queueing prompt @ {args.server} ...", file=sys.stderr)
    try:
        pid = queue_prompt(args.server, wf, client_id)
    except (urllib.error.URLError, ConnectionRefusedError) as e:
        print(f"ERROR: cannot reach {args.server} ({e}). "
              f"Is the SSH tunnel up?  ssh -L 8188:localhost:8188 pro301@192.168.75.194 -N",
              file=sys.stderr)
        return 3

    print(f"[comfyui] prompt_id={pid}, polling ...", file=sys.stderr)
    image_info = poll_for_image(args.server, pid, args.timeout)
    print(f"[comfyui] image ready: {image_info['filename']}", file=sys.stderr)

    img_bytes = download_image(args.server, image_info)

    out_path = Path(args.output).expanduser().resolve()
    out_path.parent.mkdir(parents=True, exist_ok=True)
    with open(out_path, "wb") as f:
        f.write(img_bytes)

    print(str(out_path))
    return 0


if __name__ == "__main__":
    sys.exit(main())
