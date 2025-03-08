using Audio_System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class RevolverSoundHandler : WeaponSoundHandler<RevolverSoundConstraints, RevolverSoundsData>
{
    [SerializeField] private Vector2 _dropShellsDelay;
    
    protected override void Subscribe()
    {
        base.Subscribe();

        _constraints.revolverVisual.OnBulletInsert += OnBulletInsertPerformed;
        _constraints.revolverVisual.OnCylinderSpin += OnCylinderSpinPerformed;
        _constraints.revolverVisual.OnDropShells += OnDropShellsPerformed;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();
        
        _constraints.revolverVisual.OnBulletInsert -= OnBulletInsertPerformed;
        _constraints.revolverVisual.OnCylinderSpin -= OnCylinderSpinPerformed;
        _constraints.revolverVisual.OnDropShells -= OnDropShellsPerformed;
    }
    
    private void OnCylinderSpinPerformed() => PlaySoundOnWeapon(_soundsData.cylinderSpinSounds);

    private void OnBulletInsertPerformed() => PlaySoundOnWeapon(_soundsData.bulletInsertSounds);

    private void OnDropShellsPerformed(int count)
    {
        for (int i = 0; i < count; i++)
        {
            float delay = Random.Range(_dropShellsDelay.x, _dropShellsDelay.y);
            PlaySoundOnWeaponAsync(_soundsData.dropShellSounds, delay).Forget();
        }
    }
}