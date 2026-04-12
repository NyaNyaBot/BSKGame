# Unity — Version Reference

| Field | Value |
|-------|-------|
| **Engine Version** | 2022.3.17f1 |
| **Project Pinned** | 2026-04-12 |
| **LLM Knowledge Cutoff** | May 2025 |
| **Risk Level** | LOW — version is within LLM training data |

## Note

This engine version (Unity 2022.3 LTS) is well within the LLM's training data.
Engine reference docs are optional but can be added later if agents suggest incorrect APIs.

Run `/setup-engine refresh` to populate full reference docs at any time.

## Project-Specific Notes

- Platform target: WeChat Mini Game (WebGL WASM)
- Existing export pipeline: HybridCLR hot-reload + WeChat WASM export
- Key constraint: WebGL environment — no threading, limited memory, touch-only input
