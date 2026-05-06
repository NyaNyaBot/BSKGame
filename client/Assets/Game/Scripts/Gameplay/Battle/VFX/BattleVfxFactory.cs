using UnityEngine;

namespace Game.Client
{
    /// <summary>
    /// 运行时生成基础粒子系统（无需预制体时使用）。
    /// 可作为BattleVfxController的fallback。
    /// </summary>
    public static class BattleVfxFactory
    {
        public static ParticleSystem CreateHitBurst(Transform parent)
        {
            var go = new GameObject("VFX_HitBurst");
            go.transform.SetParent(parent, false);

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.3f;
            main.startLifetime = 0.4f;
            main.startSpeed = 5f;
            main.startSize = 0.15f;
            main.startColor = new Color(1f, 0.4f, 0.1f, 1f);
            main.maxParticles = 30;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 20) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.2f;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, 0.3f, 0f), 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = gradient;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = GetDefaultParticleMaterial();

            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return ps;
        }

        public static ParticleSystem CreateDeathDissolve(Transform parent)
        {
            var go = new GameObject("VFX_DeathDissolve");
            go.transform.SetParent(parent, false);

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 1f;
            main.startLifetime = 1.5f;
            main.startSpeed = 1f;
            main.startSize = 0.1f;
            main.startColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
            main.maxParticles = 50;
            main.loop = false;
            main.playOnAwake = false;
            main.gravityModifier = -0.3f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 40;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(1f, 2f, 0.5f);

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.5f, 1f, 0f));

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = GetDefaultParticleMaterial();

            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return ps;
        }

        public static ParticleSystem CreateBlockSparks(Transform parent)
        {
            var go = new GameObject("VFX_BlockSparks");
            go.transform.SetParent(parent, false);

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.2f;
            main.startLifetime = 0.3f;
            main.startSpeed = 8f;
            main.startSize = 0.08f;
            main.startColor = new Color(0.5f, 0.8f, 1f, 1f);
            main.maxParticles = 15;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 12) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 45f;
            shape.radius = 0.1f;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = 3f;
            renderer.material = GetDefaultParticleMaterial();

            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return ps;
        }

        private static Material GetDefaultParticleMaterial()
        {
            return new Material(Shader.Find("Particles/Standard Unlit"))
            {
                color = Color.white
            };
        }
    }
}
