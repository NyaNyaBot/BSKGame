# Integration Tests

Use this directory to map cross-system test evidence for:

- scene context and stale event rejection
- input timestamp mapping into `CombatClockMs`
- parry -> damage -> feedback event flow
- PerfectParry -> CounterEntryOpened -> CounterAction flow
- Unity adapter lifecycle cleanup

Unity-discovered integration tests should live under `client/Assets/Tests/PlayMode/`.
