namespace Gameplay.Interfaces
{
    public interface IPlayerWeaponActions
    {
        public float ShootToIdleDelay { get; }
        bool CanFire();
        void Shoot();
    }
}