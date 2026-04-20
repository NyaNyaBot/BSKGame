// PROTOTYPE - NOT FOR PRODUCTION
// Question: Can real-time parry feel precise and satisfying on WebGL touch screens?
// Date: 2026-04-12

using UnityEngine;
using System.Collections.Generic;

namespace Prototype.ParryCombat
{
    public enum ParryPhase
    {
        Idle,
        WindUp,
        ParryWindow,
        Strike,
        Cooldown
    }

    public enum ParryResult
    {
        None,
        Perfect,
        Good,
        Miss
    }

    public class ParryPrototypeController : MonoBehaviour
    {
        [Header("Enemy Attack Timing (seconds)")]
        public float idleDuration = 1.5f;
        public float windUpDuration = 0.8f;
        public float parryWindowDuration = 0.3f;
        public float perfectWindowDuration = 0.1f;
        public float strikeDuration = 0.3f;
        public float cooldownDuration = 1.0f;

        [Header("References")]
        public Transform enemyCube;
        public Transform playerCube;

        ParryPhase _phase = ParryPhase.Idle;
        float _phaseTimer;
        float _parryWindowStartTime;
        bool _parryAttempted;

        // Stats
        int _totalAttacks;
        int _perfectCount;
        int _goodCount;
        int _missCount;
        readonly List<float> _inputLatencies = new List<float>();
        float _lastInputLatency;
        float _lastParryOffset;
        ParryResult _lastResult = ParryResult.None;

        Vector3 _enemyStartPos;
        Vector3 _playerStartPos;
        Color _enemyBaseColor;
        Material _enemyMat;
        Material _playerMat;

        ParryFeedback _feedback;

        public ParryPhase CurrentPhase => _phase;
        public float PhaseProgress => _phase == ParryPhase.Idle ? 0f : _phaseTimer / GetPhaseDuration(_phase);
        public int TotalAttacks => _totalAttacks;
        public int PerfectCount => _perfectCount;
        public int GoodCount => _goodCount;
        public int MissCount => _missCount;
        public float LastInputLatency => _lastInputLatency;
        public float LastParryOffset => _lastParryOffset;
        public ParryResult LastResult => _lastResult;
        public List<float> InputLatencies => _inputLatencies;

        void Awake()
        {
            if (enemyCube == null)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "Enemy";
                go.transform.position = new Vector3(0, 0.5f, 3f);
                go.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
                enemyCube = go.transform;
            }

            if (playerCube == null)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "Player";
                go.transform.position = new Vector3(0, 0.5f, -2f);
                go.transform.localScale = Vector3.one;
                playerCube = go.transform;
            }

            _enemyStartPos = enemyCube.position;
            _playerStartPos = playerCube.position;

            _enemyMat = enemyCube.GetComponent<Renderer>().material;
            _playerMat = playerCube.GetComponent<Renderer>().material;
            _enemyBaseColor = new Color(0.3f, 0.3f, 0.3f);
            _enemyMat.color = _enemyBaseColor;
            _playerMat.color = new Color(0.2f, 0.5f, 0.8f);

            _feedback = GetComponent<ParryFeedback>();
            if (_feedback == null)
                _feedback = gameObject.AddComponent<ParryFeedback>();

            SetupScene();
        }

        void SetupScene()
        {
            if (Camera.main != null)
            {
                Camera.main.transform.position = new Vector3(0, 3f, -6f);
                Camera.main.transform.rotation = Quaternion.Euler(15, 0, 0);
                Camera.main.backgroundColor = new Color(0.05f, 0.05f, 0.1f);
            }

            if (FindObjectOfType<Light>() == null)
            {
                var lightGO = new GameObject("Directional Light");
                var light = lightGO.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.2f;
                light.color = new Color(0.9f, 0.85f, 0.8f);
                lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);
            }

            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(2, 1, 2);
            floor.GetComponent<Renderer>().material.color = new Color(0.15f, 0.15f, 0.2f);
        }

        void Update()
        {
            _phaseTimer += Time.deltaTime;

            switch (_phase)
            {
                case ParryPhase.Idle:
                    UpdateIdle();
                    break;
                case ParryPhase.WindUp:
                    UpdateWindUp();
                    break;
                case ParryPhase.ParryWindow:
                    UpdateParryWindow();
                    break;
                case ParryPhase.Strike:
                    UpdateStrike();
                    break;
                case ParryPhase.Cooldown:
                    UpdateCooldown();
                    break;
            }

            if (_phase == ParryPhase.ParryWindow || _phase == ParryPhase.WindUp)
                CheckParryInput();
        }

        void CheckParryInput()
        {
            bool inputDetected = false;
            float inputTime = Time.realtimeSinceStartup;

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                inputDetected = true;
            else if (Input.GetMouseButtonDown(0))
                inputDetected = true;

            if (!inputDetected || _parryAttempted) return;

            _parryAttempted = true;
            float frameLatency = Time.unscaledDeltaTime * 1000f;
            _lastInputLatency = frameLatency;
            _inputLatencies.Add(frameLatency);

            if (_phase == ParryPhase.WindUp)
            {
                _lastResult = ParryResult.Miss;
                _lastParryOffset = -(GetPhaseDuration(ParryPhase.WindUp) - _phaseTimer);
                _missCount++;
                _totalAttacks++;
                _feedback.PlayFeedback(ParryResult.Miss, playerCube);
                TransitionTo(ParryPhase.Cooldown);
                return;
            }

            float windowElapsed = _phaseTimer;
            float windowCenter = parryWindowDuration * 0.5f;
            float offsetFromCenter = Mathf.Abs(windowElapsed - windowCenter);
            _lastParryOffset = (windowElapsed - windowCenter) * 1000f;

            if (offsetFromCenter <= perfectWindowDuration * 0.5f)
            {
                _lastResult = ParryResult.Perfect;
                _perfectCount++;
                _feedback.PlayFeedback(ParryResult.Perfect, enemyCube);
            }
            else
            {
                _lastResult = ParryResult.Good;
                _goodCount++;
                _feedback.PlayFeedback(ParryResult.Good, enemyCube);
            }

            _totalAttacks++;
            TransitionTo(ParryPhase.Cooldown);
        }

        void UpdateIdle()
        {
            _enemyMat.color = _enemyBaseColor;
            enemyCube.position = _enemyStartPos;

            if (_phaseTimer >= idleDuration)
                TransitionTo(ParryPhase.WindUp);
        }

        void UpdateWindUp()
        {
            float t = _phaseTimer / windUpDuration;
            _enemyMat.color = Color.Lerp(_enemyBaseColor, new Color(0.9f, 0.2f, 0.1f), t);

            float pullBack = Mathf.Sin(t * Mathf.PI * 0.5f) * 0.5f;
            enemyCube.position = _enemyStartPos + Vector3.back * pullBack;

            float shake = Mathf.Sin(t * 30f) * t * 0.05f;
            enemyCube.position += Vector3.right * shake;

            if (_phaseTimer >= windUpDuration)
                TransitionTo(ParryPhase.ParryWindow);
        }

        void UpdateParryWindow()
        {
            float t = _phaseTimer / parryWindowDuration;
            _enemyMat.color = new Color(1f, 0.8f, 0.1f);

            float lunge = Mathf.Lerp(0, 2.5f, t);
            enemyCube.position = _enemyStartPos + Vector3.forward * (-0.5f + lunge);

            if (_phaseTimer >= parryWindowDuration)
            {
                if (!_parryAttempted)
                {
                    _lastResult = ParryResult.Miss;
                    _lastParryOffset = parryWindowDuration * 500f;
                    _missCount++;
                    _totalAttacks++;
                    _feedback.PlayFeedback(ParryResult.Miss, playerCube);
                }
                TransitionTo(ParryPhase.Strike);
            }
        }

        void UpdateStrike()
        {
            float t = _phaseTimer / strikeDuration;
            _enemyMat.color = Color.Lerp(new Color(1f, 0.3f, 0.1f), _enemyBaseColor, t);
            enemyCube.position = Vector3.Lerp(
                _enemyStartPos + Vector3.forward * 2f,
                _enemyStartPos,
                t);

            if (_phaseTimer >= strikeDuration)
                TransitionTo(ParryPhase.Cooldown);
        }

        void UpdateCooldown()
        {
            enemyCube.position = Vector3.Lerp(enemyCube.position, _enemyStartPos, Time.deltaTime * 5f);
            _enemyMat.color = Color.Lerp(_enemyMat.color, _enemyBaseColor, Time.deltaTime * 5f);

            if (_phaseTimer >= cooldownDuration)
                TransitionTo(ParryPhase.Idle);
        }

        void TransitionTo(ParryPhase next)
        {
            _phase = next;
            _phaseTimer = 0f;

            if (next == ParryPhase.Idle)
                _parryAttempted = false;

            if (next == ParryPhase.ParryWindow)
                _parryWindowStartTime = Time.realtimeSinceStartup;
        }

        float GetPhaseDuration(ParryPhase phase)
        {
            return phase switch
            {
                ParryPhase.Idle => idleDuration,
                ParryPhase.WindUp => windUpDuration,
                ParryPhase.ParryWindow => parryWindowDuration,
                ParryPhase.Strike => strikeDuration,
                ParryPhase.Cooldown => cooldownDuration,
                _ => 1f
            };
        }

        public void ResetStats()
        {
            _totalAttacks = 0;
            _perfectCount = 0;
            _goodCount = 0;
            _missCount = 0;
            _inputLatencies.Clear();
            _lastResult = ParryResult.None;
        }
    }
}
