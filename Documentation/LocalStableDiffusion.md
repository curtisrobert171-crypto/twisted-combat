# Local Stable Diffusion Setup

## Overview

This guide covers how to run Stable Diffusion on your local machine for image experimentation outside the app runtime. It is useful for prototyping photo enhancement workflows, validating prompt ideas, or generating reference images before wiring a hosted provider into the product.

## Prerequisites

- **GPU**: NVIDIA GPU with 4GB+ VRAM (8GB+ recommended); AMD GPUs work on Linux via ROCm; Apple Silicon works via MPS
- **RAM**: 8GB minimum, 16GB recommended
- **Storage**: 10-30GB free
- **Python**: 3.10 or 3.11
- **Git**: required for cloning the UI projects below

## Option 1: AUTOMATIC1111 Web UI

Recommended when you want the fastest path to a familiar browser-based Stable Diffusion interface.

1. Install [Python 3.10 or 3.11](https://www.python.org/) and [Git](https://git-scm.com/).
2. Clone the project:

   ```bash
   git clone https://github.com/AUTOMATIC1111/stable-diffusion-webui
   cd stable-diffusion-webui
   ```

3. Download a model checkpoint (`.safetensors`) from [Civitai](https://civitai.com/) or [Hugging Face](https://huggingface.co/).
4. Place the model file in `models/Stable-diffusion/`.
5. Start the UI:

   - macOS/Linux: `./webui.sh`
   - Windows: `webui-user.bat`

6. Open `http://127.0.0.1:7860` in your browser.

## Option 2: ComfyUI

Recommended when you need more control over generation graphs, reusable workflows, or node-based experimentation.

1. Clone the project:

   ```bash
   git clone https://github.com/comfyanonymous/ComfyUI
   cd ComfyUI
   ```

2. Install dependencies:

   ```bash
   pip install -r requirements.txt
   ```

3. Place model checkpoints in `models/checkpoints/`.
4. Start the server:

   ```bash
   python main.py
   ```

5. Open `http://127.0.0.1:8188` in your browser.

## Option 3: InvokeAI

Recommended when you want the most guided desktop-style installation flow.

1. Download the latest installer from [InvokeAI releases](https://github.com/invoke-ai/InvokeAI/releases).
2. Run the installer. It handles Python and most dependencies automatically.
3. Launch the app and use the built-in model manager to download checkpoints.

## Recommended Starter Models

- **SDXL 1.0**: Higher quality at 1024x1024, but typically needs 8GB+ VRAM
- **SD 1.5**: Lightweight and widely supported; good default for low-memory systems
- **FLUX.1-schnell**: Strong quality and speed, but often needs 12GB+ VRAM or CPU offload

## Low VRAM Tips

- Add `--lowvram` or `--medvram` to AUTOMATIC1111 launch arguments
- Prefer SD 1.5 models over SDXL on lower-spec machines
- Enable CPU offloading in ComfyUI when VRAM is limited

## Project Integration Note

This setup runs independently from the Flutter app and Firebase Functions in this repository. If you later want to integrate locally generated or AI-enhanced images into the product flow, treat that as a separate feature: generate assets externally, then import or upload them through the app's existing photo workflow.
