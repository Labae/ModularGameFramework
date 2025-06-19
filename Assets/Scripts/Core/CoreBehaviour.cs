using System.Text;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Unity의 자주 사용되는 컴포넌트들을 캐싱하는 기본 클래스
    /// Transform, GameObject 등의 반복적인 접근을 최적화
    /// </summary>
    public abstract class CoreBehaviour : MonoBehaviour
    {
        private Transform _cachedTransform;
        private GameObject _cachedGameObject;
        private string _cachedName;
        
        protected static readonly StringBuilder _stringBuilder = new(256);

        /// <summary>
        /// 캐싱된 Transform
        /// </summary>
        public new Transform transform
        {
            get
            {
                if (_cachedTransform == null)
                {
                    _cachedTransform = base.transform;
                }

                return _cachedTransform;
            }
        }

        /// <summary>
        /// 캐싱된 GameObject
        /// </summary>
        public new GameObject gameObject
        {
            get
            {
                if (_cachedGameObject == null)
                {
                    _cachedGameObject = base.gameObject;
                }

                return _cachedGameObject;
            }
        }

        /// <summary>
        /// 캐싱된 GameObject 이름
        /// </summary>
        public new string name
        {
            get
            {
                if (_cachedName == null)
                {
                    _cachedName = base.name;
                }

                return _cachedName;
            }
        }

        public Vector3 position3D => transform.position;
        public Vector2 position2D => transform.position;

        protected virtual void Awake()
        {
            CacheComponents();
        }

        protected virtual void CacheComponents()
        {
            _ = transform;
            _ = gameObject;
            _ = name;
        }
    }
}