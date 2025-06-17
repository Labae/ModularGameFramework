using MarioGame.Core.ObjectPooling;
using MarioGame.Gameplay.Config.Weapon;
using MarioGame.Gameplay.Interfaces.Weapon;
using MarioGame.Gameplay.Projectiles.HitEffects;

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

        public void Initialize()
        {
            // 이펙트 프리팹들의 오브젝트 풀 생성
            if (_config?.HitEffectPrefab != null)
            {
                var hitEffect = _config.HitEffectPrefab.GetComponent<ProjectileEffect>();
                ObjectPoolManager.Instance.CreatePool(hitEffect);
            }
            
            if (_config?.HitEffectPrefab != null)
            {
                var penetrateEffect = _config.HitEffectPrefab.GetComponent<ProjectileEffect>();
                ObjectPoolManager.Instance.CreatePool(penetrateEffect);
            }
        }
        

        public void PlayFireEffect(WeaponFireData fireData)
        {
            // 머즐 플래시 이펙트
            if (_config?.MuzzleFlashPrefab != null)
            {
                // 이펙트 재생 로직
            }
            
            // 발사 사운드
            if (_config?.FireSound != null)
            {
                // 사운드 재생 로직
            }
        }

        public void Dispose()
        {
            // 필요한 정리 작업
        }
    }
}