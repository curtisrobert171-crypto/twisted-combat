# Hardware Requirements — Empire of Glass Development

Recommended workstation specifications for developing **Empire of Glass** across Unity 6, Unreal Engine 5, and DevOps workflows.

---

## Minimum Specifications

These specs will allow basic development but may struggle with large scenes, Nanite/Lumen previews, or parallel CI builds.

| Component | Specification |
|-----------|--------------|
| **CPU** | 6-core / 12-thread (e.g., AMD Ryzen 5 5600X, Intel Core i5-12600K) |
| **RAM** | 32 GB DDR4-3200 |
| **GPU** | NVIDIA RTX 3060 12 GB or AMD RX 6700 XT 12 GB |
| **Storage** | 512 GB NVMe SSD |
| **OS** | Windows 10/11 64-bit or macOS (Unity only) |

## Recommended Specifications

Suitable for full-scene editing in Unity 6, UE5 Nanite/Lumen previews, shader compilation, and running local CI pipelines.

| Component | Specification |
|-----------|--------------|
| **CPU** | 8-core / 16-thread (e.g., AMD Ryzen 7 7700X, Intel Core i7-13700K) |
| **RAM** | 64 GB DDR5-5600 |
| **GPU** | NVIDIA RTX 4070 Ti 12 GB or AMD RX 7800 XT 16 GB |
| **Storage** | 1 TB NVMe Gen 4 SSD + secondary drive for asset cache |
| **OS** | Windows 11 64-bit |

## High-End / Build Server Specifications

For UE5 Nanite/Lumen at full fidelity, shader farm compilation, lightmap baking, large swarm simulations (500+ units), and dedicated CI/CD build agents.

| Component | Specification |
|-----------|--------------|
| **CPU** | 16-core / 32-thread (e.g., AMD Ryzen 9 7950X, Intel Core i9-13900K) |
| **RAM** | 128 GB DDR5-5600 |
| **GPU** | NVIDIA RTX 4090 24 GB |
| **Storage** | 2 TB NVMe Gen 4 SSD (OS + projects) + 4 TB HDD (archives) |
| **OS** | Windows 11 64-bit or Ubuntu 22.04 LTS (headless CI) |

---

## Engine-Specific Notes

### Unity 6 (Current Project Engine)

- Unity 6 (`6000.0.23f1`) is the editor version used by this project.
- GPU instancing for the 500+ shardling swarm benefits from 8 GB+ VRAM.
- Shader compilation is CPU-bound; more cores reduce iteration time.
- IL2CPP Android/iOS builds are memory-intensive — 32 GB RAM minimum.

### Unreal Engine 5 (Target Visual Fidelity)

- Nanite virtualized geometry requires an RTX 3060 or better; RTX 4070+ recommended.
- Lumen global illumination is GPU-intensive; 12 GB+ VRAM strongly recommended.
- Shader compilation on first open can exceed 30 minutes without enough CPU cores.
- UE5 editor alone can consume 12–16 GB RAM before loading any project.
- DirectX 12 or Vulkan capable GPU required; DX12 Ultimate for full feature support.

---

## DevOps & CI/CD Workloads

| Workload | Key Requirement |
|----------|----------------|
| Unity batch-mode builds (Android/iOS) | Fast multi-core CPU, 32 GB+ RAM, SSD |
| UE5 cooking & packaging | 16+ cores, 64 GB+ RAM, large SSD |
| Shader compilation farm | High core-count CPU (Ryzen 9 / Threadripper) |
| Lightmap / reflection probe baking | GPU compute (RTX 4070+) |
| Docker-based CI runners | 32 GB+ RAM, NVMe SSD, Linux recommended |
| Artifact storage & caching | Fast network + large secondary drive |

### Recommended CI/CD Runner Configuration

For self-hosted GitHub Actions or Jenkins agents building this project:

- **CPU:** 8+ cores
- **RAM:** 64 GB
- **Storage:** 500 GB NVMe SSD (build workspace) + network/cloud artifact storage
- **GPU:** Optional for headless Unity builds; required for bake/cook jobs
- **OS:** Ubuntu 22.04 LTS (headless) or Windows Server 2022

---

## Laptop Recommendations

For developers who need portability:

| Tier | Examples | Notes |
|------|----------|-------|
| **Good** | ASUS ROG Zephyrus G14, Lenovo Legion 5 | RTX 4060 Laptop, 32 GB RAM — adequate for Unity |
| **Better** | Razer Blade 16, MSI Creator Z16 | RTX 4070 Laptop, 64 GB RAM — comfortable for both engines |
| **Best** | ASUS ROG Strix 18, MSI Titan 18 HX | RTX 4090 Laptop, 64 GB RAM — full UE5 Nanite/Lumen on the go |

> **Tip:** Pair a portable laptop with a desktop or cloud build machine for heavy tasks like UE5 cooking and lightmap baking.

---

## Summary

| Use Case | Recommended Tier |
|----------|-----------------|
| Unity 6 scripting & scene editing | Minimum |
| Unity 6 full builds + local testing | Recommended |
| UE5 Nanite/Lumen development | Recommended or High-End |
| DevOps / CI build agent | Recommended or High-End |
| Shader compilation & baking | High-End |
