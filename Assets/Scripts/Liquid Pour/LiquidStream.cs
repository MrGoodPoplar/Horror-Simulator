using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Liquid_Pour
{
    [RequireComponent(typeof(LineRenderer))]
    public class LiquidStream : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _particles;
        [SerializeField] private float _maxLength = 2.0f;
        [SerializeField] private float _speed = 1.25f;
        
        private LineRenderer _lineRenderer;
        private Vector3 _targetPosition = Vector3.zero;
        private bool _isPouring;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
        }

        private void Start()
        {
            SetPosition(0, transform.position);
            SetPosition(1, transform.position);
        }

        public async UniTaskVoid Toggle(bool toggle)
        {
            _isPouring = toggle;

            if (toggle)
            {
                gameObject.SetActive(_isPouring);
                _particles.Play();
                BeginPour().Forget();
            }
            else
            {
                _particles.Stop();
                await EndPour();
                gameObject.SetActive(_isPouring);
            }
        }
        
        private async UniTask BeginPour()
        {
            SetPosition(1, transform.position);
            
            while (gameObject.activeInHierarchy && _isPouring)
            {
                _targetPosition = FindEndPoint();
                _particles.transform.rotation = Quaternion.LookRotation(_targetPosition - _particles.transform.position);
                
                SetPosition(0, transform.position);
                MoveToPosition(1, _targetPosition);
                
                await UniTask.Yield();
            }
        }

        private async UniTask EndPour()
        {
            while (!HasReachedPosition(0, _targetPosition) && !_isPouring)
            {
                MoveToPosition(0, _targetPosition);
                MoveToPosition(1, _targetPosition);
                
                await UniTask.Yield();
            }
        }
        
        private Vector3 FindEndPoint()
        {
            RaycastHit hit;
            Ray ray = new Ray(transform.position, Vector3.down);
            
            Physics.Raycast(ray, out hit, _maxLength);
            Vector3 endPoint = hit.collider ? hit.point : ray.GetPoint(_maxLength);
            
            return endPoint;
        }

        private void SetPosition(int index, Vector3 targetPosition)
        {
            _lineRenderer.SetPosition(index, targetPosition);
        }

        private void MoveToPosition(int index, Vector3 targetPosition)
        {
            Vector3 currentPosition = _lineRenderer.GetPosition(index);
            Vector3 newPosition = Vector3.MoveTowards(currentPosition, targetPosition, Time.deltaTime * _speed);
            
            SetPosition(index, newPosition);
        }

        private bool HasReachedPosition(int index, Vector3 targetPosition)
        {
            Vector3 currentPosition = _lineRenderer.GetPosition(index);
            return Vector3.Distance(currentPosition, targetPosition) < 0.01f;
        }
    }
}