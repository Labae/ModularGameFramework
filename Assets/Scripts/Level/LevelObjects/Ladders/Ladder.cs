using MarioGame.Core.Entities;
using MarioGame.Debugging.Interfaces;
using Reflex.Attributes;
using UnityEngine;

namespace MarioGame.Level.LevelObjects.Ladders
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Ladder : Entity
    {
        private BoxCollider2D _boxCollider;
        
        [Header("Ladder Settings")]
        [SerializeField, Min(0.1f)] private float _endPointThreshold = 0.3f;

        [Header("Debug")] [SerializeField] private bool _drawGizmos = true;
        
        public Vector2 TopPoint => _boxCollider.bounds.max;
        public Vector2 BottomPoint => _boxCollider.bounds.min;
        
        public float CenterX => _boxCollider.bounds.center.x;
        public Bounds LadderBounds => _boxCollider.bounds;

        protected override void CacheComponents()
        {
            base.CacheComponents();
            _boxCollider = GetComponent<BoxCollider2D>();
            _assertManager.AssertIsNotNull(_boxCollider, "BoxCollider2D required");
            if (!_boxCollider.isTrigger)
            {
                _boxCollider.isTrigger = true;
                _debugLogger.Warning("Ladder should have isTrigger = true");
            }
        }

        public bool IsEntityAtTop(Vector2 position)
        {
            return position.y >= (TopPoint.y - _endPointThreshold);
        }
        
        public bool IsEntityAtBottom(Vector2 position)
        {
            return position.y <= (BottomPoint.y + _endPointThreshold);
        }

        public bool IsEntityInLadderRange(Vector2 position)
        {
            return _boxCollider.bounds.Contains(position);
        }

        public float GetMaxClimbY()
        {
            return TopPoint.y;
        }
        
        public float GetMinClimbY()
        {
            return BottomPoint.y;
        }

        #if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            if (!_drawGizmos || _boxCollider == null)
            {
                return;
            }
            
            var bounds = _boxCollider.bounds;
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
            
            Gizmos.color = Color.red;
            var topCenter = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
            var topSize = new Vector3(bounds.size.x, _endPointThreshold * 2, 0.1f);
            Gizmos.DrawWireCube(topCenter, topSize);
            
            Gizmos.color = Color.green;
            var bottomCenter = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            var bottomSize = new Vector3(bounds.size.x, _endPointThreshold * 2, 0.1f);
            Gizmos.DrawWireCube(bottomCenter, bottomSize);
            
            Gizmos.color = Color.cyan;
            var lineStart = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            var lendEnd = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
            Gizmos.DrawLine(lineStart, lendEnd);
        }

#endif
    }
}