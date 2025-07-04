using UnityEngine;

namespace Liquid_Pour
{
    public class PourHandler : MonoBehaviour
    {
        [SerializeField] private int _pourThreshold = 45;
        [SerializeField] private LiquidStream _liquidStream;
    
        private bool _isPouring;
        
        private void Update()
        {
            bool canPour = CalculatePourAngle() > _pourThreshold;

            if (_isPouring != canPour)
            {
                _isPouring = canPour;
                _liquidStream.Toggle(_isPouring).Forget();
            }
        }
        
        private float CalculatePourAngle()
        {
            return Vector3.Angle(transform.forward, Vector3.up);
        }
    }
}