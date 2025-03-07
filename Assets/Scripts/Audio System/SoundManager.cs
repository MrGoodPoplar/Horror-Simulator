using System.Collections.Generic;
using Library.UnityUtils;
using UnityEngine;
using UnityEngine.Pool;

namespace Audio_System
{
    public class SoundManager : PersistentSingleton<SoundManager>
    {
        [SerializeField] private SoundEmitter _soundEmitterPrefab;
        [SerializeField] private bool _collectionCheck = true;
        [SerializeField] private int _defaultCapacity = 10;
        [SerializeField] private int _maxPoolSize = 100;
        [SerializeField] private int _maxSoundInstances = 20;
        
        private IObjectPool<SoundEmitter> _soundEmitterPool;
        private readonly List<SoundEmitter> _activeSoundEmitters = new();

        public readonly Queue<SoundEmitter> frequentSoundEmitters = new();
        
        private void Start()
        {
            InitializePool();
        }

        public SoundBuilder CreateSound() => new SoundBuilder(this);
        
        public SoundEmitter Get() => _soundEmitterPool.Get();

        public void ReturnToPool(SoundEmitter soundEmitter) => _soundEmitterPool.Release(soundEmitter);

        public void StopAll() {
            foreach (var soundEmitter in _activeSoundEmitters) {
                soundEmitter.Stop();
            }

            frequentSoundEmitters.Clear();
        }
        
        public bool CanPlaySound(SoundData data)
        {
            if (!data.isFrequent)
                return true;
            
            if (frequentSoundEmitters.Count > _maxSoundInstances && frequentSoundEmitters.TryDequeue(out var soundEmitter))
            {
                try
                {
                    soundEmitter.Stop();
                    return true;
                }
                catch
                {
                    Debug.LogWarning("SoundEmitter is already released!");
                    return false;
                }
            }

            return true;
        }
        private void InitializePool()
        {
            _soundEmitterPool = new ObjectPool<SoundEmitter>(
                CreateSoundEmitter,
                OnTakeFromPool,
                OnReturnedToPool,
                OnDestroyPoolObject,
                _collectionCheck,
                _defaultCapacity,
                _maxPoolSize);
        }

        private void OnDestroyPoolObject(SoundEmitter soundEmitter)
        {
            Destroy(soundEmitter);
        }

        private void OnReturnedToPool(SoundEmitter soundEmitter)
        {
            soundEmitter.gameObject.SetActive(false);
            _activeSoundEmitters.Remove(soundEmitter);
        }

        private void OnTakeFromPool(SoundEmitter soundEmitter)
        {
            soundEmitter.gameObject.SetActive(true);
            _activeSoundEmitters.Add(soundEmitter);
        }

        private SoundEmitter CreateSoundEmitter()
        {
            var soundEmitter = Instantiate(_soundEmitterPrefab);
            soundEmitter.gameObject.SetActive(false);
            return soundEmitter;
        }
    }
}