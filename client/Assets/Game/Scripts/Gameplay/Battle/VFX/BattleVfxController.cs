using UnityEngine;

namespace Game.Client
{
    /// <summary>
    /// 战斗VFX控制器。管理攻击/受击/死亡/格挡粒子特效的播放。
    /// </summary>
    public class BattleVfxController : MonoBehaviour
    {
        [Header("Hit Effect")]
        [SerializeField] private ParticleSystem _hitEffectPrefab;
        [SerializeField] private Color _hitColor = new Color(1f, 0.3f, 0.1f, 1f);

        [Header("Death Effect")]
        [SerializeField] private ParticleSystem _deathEffectPrefab;
        [SerializeField] private Color _deathColor = new Color(0.2f, 0.2f, 0.2f, 1f);

        [Header("Attack Trail")]
        [SerializeField] private ParticleSystem _attackTrailPrefab;
        [SerializeField] private Color _attackColor = new Color(1f, 0.9f, 0.4f, 1f);

        [Header("Block Effect")]
        [SerializeField] private ParticleSystem _blockEffectPrefab;
        [SerializeField] private Color _blockColor = new Color(0.4f, 0.7f, 1f, 1f);

        [Header("Pool Settings")]
        [SerializeField] private int _poolSize = 5;

        private ParticleSystem[] _hitPool;
        private ParticleSystem[] _deathPool;
        private ParticleSystem[] _attackPool;
        private ParticleSystem[] _blockPool;
        private int _hitIndex, _deathIndex, _attackIndex, _blockIndex;

        private void Awake()
        {
            _hitPool = CreatePool(_hitEffectPrefab, _poolSize);
            _deathPool = CreatePool(_deathEffectPrefab, _poolSize);
            _attackPool = CreatePool(_attackTrailPrefab, _poolSize);
            _blockPool = CreatePool(_blockEffectPrefab, _poolSize);
        }

        public void PlayHitEffect(Vector3 position)
        {
            PlayFromPool(_hitPool, ref _hitIndex, position, _hitColor);
        }

        public void PlayDeathEffect(Vector3 position)
        {
            PlayFromPool(_deathPool, ref _deathIndex, position, _deathColor);
        }

        public void PlayAttackTrail(Vector3 position, Quaternion rotation)
        {
            var ps = GetFromPool(_attackPool, ref _attackIndex);
            if (ps == null) return;

            ps.transform.SetPositionAndRotation(position, rotation);
            SetMainColor(ps, _attackColor);
            ps.Play();
        }

        public void PlayBlockEffect(Vector3 position)
        {
            PlayFromPool(_blockPool, ref _blockIndex, position, _blockColor);
        }

        private void PlayFromPool(ParticleSystem[] pool, ref int index, Vector3 position, Color color)
        {
            var ps = GetFromPool(pool, ref index);
            if (ps == null) return;

            ps.transform.position = position;
            SetMainColor(ps, color);
            ps.Play();
        }

        private ParticleSystem GetFromPool(ParticleSystem[] pool, ref int index)
        {
            if (pool == null || pool.Length == 0) return null;

            var ps = pool[index % pool.Length];
            index++;
            return ps;
        }

        private ParticleSystem[] CreatePool(ParticleSystem prefab, int size)
        {
            if (prefab == null) return System.Array.Empty<ParticleSystem>();

            var pool = new ParticleSystem[size];
            for (int i = 0; i < size; i++)
            {
                var go = Instantiate(prefab.gameObject, transform);
                go.SetActive(true);
                var ps = go.GetComponent<ParticleSystem>();
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                pool[i] = ps;
            }
            return pool;
        }

        private static void SetMainColor(ParticleSystem ps, Color color)
        {
            var main = ps.main;
            main.startColor = color;
        }
    }
}
