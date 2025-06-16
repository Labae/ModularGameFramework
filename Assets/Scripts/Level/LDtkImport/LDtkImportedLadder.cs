using LDtkUnity;
using UnityEngine;

namespace MarioGame.Level.LDtkImport
{
    public class LDtkImportedLadder : MonoBehaviour, ILDtkImportedEntity
    {
        [SerializeField] private BoxCollider2D _collider = null;

        [Header("Ladder Top Platform")] [SerializeField]
        private GameObject _topPlatform;

        [SerializeField] private float _topPlatformOffset = 0.1f;
        
        public void OnLDtkImportEntity(EntityInstance entityInstance)
        {
            if (_collider == null)
            {
                Debug.LogWarning($"LDtk Sample: An entity's referenced collider component was null. This can happen when importing the examples for the first time. Try reimporting again to fix the samples.\n{name}", gameObject);
                return;
            }

            SetupLadderCollider(entityInstance);
            SetupTopPlatform();
        }

        private void SetupTopPlatform()
        {
            var ladderBounds = _collider.bounds;
            var topPosition = new Vector3(
                ladderBounds.center.x, 
                ladderBounds.max.y + _topPlatformOffset, 
                transform.position.z
            );
            
            _topPlatform.transform.position = topPosition;
        }

        private void SetupLadderCollider(EntityInstance entityInstance)
        {
            //The importer will normally scale the GameObject based on how much it was resized in LDtk.
            //But we don't want that in this instance, so set the scale back to one.
            transform.localScale = Vector3.one;
            
            //The resize factor is the entity instance size divided by the entity definition size.
            //Alternatively if the base prefab was never scaled, the y scale of the entity GameObject can be used too.
            float yScale = entityInstance.Height / (float)entityInstance.Definition.Height;
            
            _collider.size = new Vector2(_collider.size.x, yScale);
            
            //Because the entity's pivot is in the top left, the sprite's pivot was configured accordingly to have the corresponding pivot.
            //However, it's not possible to configure a pivot for a BoxCollider2D, so the colliders offset needs to be manually determined.
            _collider.offset = new Vector2(_collider.offset.x, yScale * 0.5f);
            _collider.isTrigger = true;
            
            gameObject.layer = LayerMask.NameToLayer("Ladder");
        }

        public int GetPostprocessOrder()
        {
            return -1;
        }
    }
}