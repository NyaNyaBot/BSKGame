# PlayMode Tests

PlayMode tests should cover Unity adapter behavior that cannot be proven in pure domain tests:

- scene lifecycle binding and cleanup
- input hit area registration through UGUI / EventSystem adapters
- HUD lifecycle and safe-area layout
- feedback request routing and hit stop integration
- WebGL-specific smoke scenes when available

Create `BSK.Game.PlayModeTests.asmdef` when the first PlayMode test is implemented.
