using MarioGame.Core.ObjectPooling;
using MarioGame.Gameplay.Config.Weapon;
using MarioGame.Gameplay.Effects.HitEffects;
using MarioGame.Gameplay.Interfaces.Weapon;

namespace MarioGame.Gameplay.Weapons
{
    /// <summary>
    /// 무기 이펙트 관리
    /// </summary>
    public class WeaponEffectManager : IWeaponEffectManager
    {
        private readonly WeaponConfiguration _config;

        public WeaponEffectManager(WeaponConfiguration config)
        {
            _config = config;
        }

        public void PlayFireEffect(WeaponFireData fireData)
        {
            // 머즐 플래시 이펙트
            if (_config?.MuzzleFlashPrefab != null)
            {
                // 이펙트 재생 로직
            }
        }

        public void Dispose()
        {
            // 필요한 정리 작업
        }
    }
}