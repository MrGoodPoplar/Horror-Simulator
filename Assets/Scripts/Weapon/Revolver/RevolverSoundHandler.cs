using Audio_System;

public class RevolverSoundHandler : WeaponSoundHandler<RevolverSoundConstraints, RevolverSoundsData>
{
    protected override void Subscribe()
    {
        base.Subscribe();

        _constraints.revolverVisual.OnBulletInsert += OnBulletInsertPerformed;
        _constraints.revolverVisual.OnCylinderSpin += OnCylinderSpinPerformed;
    }

    protected override void Unsubscribe()
    {
        base.Unsubscribe();
        
        _constraints.revolverVisual.OnBulletInsert -= OnBulletInsertPerformed;
        _constraints.revolverVisual.OnCylinderSpin -= OnCylinderSpinPerformed;
    }
    
    private void OnCylinderSpinPerformed() => PlaySoundOnWeapon(_soundsData.cylinderSpinSounds);

    private void OnBulletInsertPerformed() => PlaySoundOnWeapon(_soundsData.bulletInsertSounds);
}