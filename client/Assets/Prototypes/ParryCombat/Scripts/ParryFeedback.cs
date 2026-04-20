// PROTOTYPE - NOT FOR PRODUCTION
// Question: Can real-time parry feel precise and satisfying on WebGL touch screens?
// Date: 2026-04-12

using UnityEngine;
using System.Collections;

namespace Prototype.ParryCombat
{
    public class ParryFeedback : MonoBehaviour
    {
        [Header("Hit Stop")]
        public float perfectHitStopDuration = 0.12f;
        public float goodHitStopDuration = 0.05f;

        [Header("Screen Shake")]
        public float perfectShakeIntensity = 0.25f;
        public float goodShakeIntensity = 0.1f;
        public float missShakeIntensity = 0.15f;
        public float shakeDuration = 0.2f;
        public float shakeFrequency = 25f;

        [Header("Flash")]
        public Color perfectFlashColor = new Color(1f, 0.85f, 0.2f, 0.6f);
        public Color goodFlashColor = new Color(1f, 1f, 1f, 0.3f);
        public Color missFlashColor = new Color(1f, 0.1f, 0.1f, 0.4f);
        public float flashDuration = 0.15f;

        [Header("Scale Punch")]
        public float perfectPunchScale = 1.5f;
        public float goodPunchScale = 1.2f;
        public float punchDuration = 0.15f;

        Camera _cam;
        Vector3 _camOriginalPos;
        Coroutine _shakeCoroutine;
        Coroutine _hitStopCoroutine;

        // Screen flash overlay
        Texture2D _flashTexture;
        Color _currentFlashColor = Color.clear;
        float _flashTimer;
        float _flashDurationCurrent;

        // Particle system (runtime created)
        ParticleSystem _burstParticles;

        void Awake()
        {
            _cam = Camera.main;
            if (_cam != null)
                _camOriginalPos = _cam.transform.position;

            _flashTexture = new Texture2D(1, 1);
            _flashTexture.SetPixel(0, 0, Color.white);
            _flashTexture.Apply();

            CreateParticleSystem();
        }

        void CreateParticleSystem()
        {
            var go = new GameObject("ParryBurstParticles");
            go.transform.SetParent(transform);
            _burstParticles = go.AddComponent<ParticleSystem>();

            _burstParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = _burstParticles.main;
            main.duration = 0.3f;
            main.startLifetime = 0.4f;
            main.startSpeed = 8f;
            main.startSize = 0.15f;
            main.maxParticles = 50;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = new Color(1f, 0.85f, 0.3f);

            var emission = _burstParticles.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 30) });

            var shape = _burstParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.3f;

            var colorOverLifetime = _burstParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] {
                    new GradientColorKey(new Color(1f, 0.9f, 0.3f), 0f),
                    new GradientColorKey(new Color(1f, 0.4f, 0.1f), 1f)
                },
                new[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;

            var sizeOverLifetime = _burstParticles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 1, 1, 0));

            var psr = go.GetComponent<ParticleSystemRenderer>();
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader != null)
                psr.material = new Material(shader);
        }

        public void PlayFeedback(ParryResult result, Transform target)
        {
            switch (result)
            {
                case ParryResult.Perfect:
                    TriggerHitStop(perfectHitStopDuration);
                    TriggerShake(perfectShakeIntensity);
                    TriggerFlash(perfectFlashColor);
                    TriggerScalePunch(target, perfectPunchScale);
                    TriggerParticleBurst(target.position, new Color(1f, 0.85f, 0.2f));
                    break;

                case ParryResult.Good:
                    TriggerHitStop(goodHitStopDuration);
                    TriggerShake(goodShakeIntensity);
                    TriggerFlash(goodFlashColor);
                    TriggerScalePunch(target, goodPunchScale);
                    break;

                case ParryResult.Miss:
                    TriggerShake(missShakeIntensity);
                    TriggerFlash(missFlashColor);
                    break;
            }
        }

        void TriggerHitStop(float duration)
        {
            if (_hitStopCoroutine != null)
                StopCoroutine(_hitStopCoroutine);
            _hitStopCoroutine = StartCoroutine(HitStopRoutine(duration));
        }

        IEnumerator HitStopRoutine(float duration)
        {
            Time.timeScale = 0.02f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f;
            _hitStopCoroutine = null;
        }

        void TriggerShake(float intensity)
        {
            if (_shakeCoroutine != null)
                StopCoroutine(_shakeCoroutine);
            _shakeCoroutine = StartCoroutine(ShakeRoutine(intensity));
        }

        IEnumerator ShakeRoutine(float intensity)
        {
            float elapsed = 0;
            while (elapsed < shakeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float decay = 1f - (elapsed / shakeDuration);
                float x = Mathf.Sin(elapsed * shakeFrequency) * intensity * decay;
                float y = Mathf.Cos(elapsed * shakeFrequency * 0.7f) * intensity * decay * 0.6f;
                if (_cam != null)
                    _cam.transform.position = _camOriginalPos + new Vector3(x, y, 0);
                yield return null;
            }
            if (_cam != null)
                _cam.transform.position = _camOriginalPos;
            _shakeCoroutine = null;
        }

        void TriggerFlash(Color color)
        {
            _currentFlashColor = color;
            _flashTimer = 0;
            _flashDurationCurrent = flashDuration;
        }

        void TriggerScalePunch(Transform target, float scale)
        {
            StartCoroutine(ScalePunchRoutine(target, scale));
        }

        IEnumerator ScalePunchRoutine(Transform target, float maxScale)
        {
            Vector3 original = target.localScale;
            Vector3 punched = original * maxScale;
            float elapsed = 0;
            float halfDuration = punchDuration * 0.5f;

            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                target.localScale = Vector3.Lerp(original, punched, elapsed / halfDuration);
                yield return null;
            }

            elapsed = 0;
            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                target.localScale = Vector3.Lerp(punched, original, elapsed / halfDuration);
                yield return null;
            }
            target.localScale = original;
        }

        void TriggerParticleBurst(Vector3 position, Color color)
        {
            if (_burstParticles == null) return;
            _burstParticles.transform.position = position;
            var main = _burstParticles.main;
            main.startColor = color;
            _burstParticles.Play();
        }

        void Update()
        {
            if (_flashTimer < _flashDurationCurrent)
            {
                _flashTimer += Time.unscaledDeltaTime;
                float alpha = _currentFlashColor.a * (1f - _flashTimer / _flashDurationCurrent);
                _currentFlashColor.a = alpha;
            }
        }

        void OnGUI()
        {
            if (_currentFlashColor.a > 0.01f)
            {
                GUI.color = _currentFlashColor;
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _flashTexture);
                GUI.color = Color.white;
            }
        }

        void OnDestroy()
        {
            if (_flashTexture != null)
                Destroy(_flashTexture);
        }
    }
}
