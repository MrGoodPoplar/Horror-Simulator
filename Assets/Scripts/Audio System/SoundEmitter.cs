using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Audio_System
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundEmitter : MonoBehaviour
    {
        public SoundData data { get; private set; }
        
        private AudioSource _audioSource;
        private Coroutine _playingCoroutine;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        public void Play()
        {
            if (_playingCoroutine != null)
                StopCoroutine(_playingCoroutine);
            
            _audioSource.Play();
            _playingCoroutine = StartCoroutine(WaitForSoundToEnd());
        }

        private IEnumerator WaitForSoundToEnd()
        {
            yield return new WaitWhile(() => _audioSource.isPlaying);
            SoundManager.Instance.ReturnToPool(this);
        }

        public void Stop()
        {
            if (_playingCoroutine != null)
            {
                StopCoroutine(_playingCoroutine);
                _playingCoroutine = null;
            }
            
            _audioSource.Stop();
            SoundManager.Instance.ReturnToPool(this);
        }

        public void Initialize(SoundData data)
        {
            _audioSource.clip = data.GetClip();
            _audioSource.outputAudioMixerGroup = data.mixerGroup;
            _audioSource.loop = data.loop;
            _audioSource.playOnAwake = data.playOnAwake;
            _audioSource.spatialBlend = data.spatialBlend;
            
            this.data = data;
        }

        public void WithRandomPitch(float min = -0.05f, float max = 0.05f)
        {
            _audioSource.pitch += Random.Range(min, max);
        }
    }
}