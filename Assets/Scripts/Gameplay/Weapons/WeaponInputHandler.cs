using MarioGame.Gameplay.Interfaces;
using MarioGame.Gameplay.Interfaces.Weapon;

namespace MarioGame.Gameplay.Weapons
{
    public class WeaponInputHandler : IWeaponInputHandler
    {
        private readonly IInputProvider _inputProvider;

        public WeaponInputHandler(IInputProvider inputProvider)
        {
            _inputProvider = inputProvider;
        }

        public bool ShouldFire()
        {
            return _inputProvider?.FirePressed ?? false;
        }
    }
}