using System.Collections.Generic;
using System.Text;
using MarioGame.Core.Extensions;
using UnityEngine;
using UnityEngine.Assertions;

namespace MarioGame.Core
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
        
        private static readonly StringBuilder _stringBuilder = new(256);

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
        public Vector2 position2D => transform.position.ToVector2();

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

        /// <summary>
        /// 로그 출력
        /// </summary>
        /// <param name="messages"></param>
        protected static void Log(params object[] messages)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (messages.Length == 0)
            {
                return;
            }

            Debug.Log(FormatLogMessage(messages));
#endif
        }

        /// <summary>
        /// 경고 로그 출력
        /// </summary>
        /// <param name="messages"></param>
        protected static void LogWarning(params object[] messages)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (messages.Length == 0)
            {
                return;
            }

            Debug.LogWarning(FormatLogMessage(messages));
#endif          
        }

        /// <summary>
        /// 에러 로그 출력
        /// </summary>
        /// <param name="messages"></param>
        protected static void LogError(params object[] messages)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (messages.Length == 0)
            {
                return;
            }

            Debug.LogError(FormatLogMessage(messages));
#endif
        }


        private static string FormatLogMessage(object[] messages)
        {
            lock (_stringBuilder)
            {
                _stringBuilder.Clear();
                for (int i = 0; i < messages.Length; i++)
                {
                    if (i > 0)
                    {
                        _stringBuilder.Append(' ');
                    }
                    _stringBuilder.Append(messages[i]);
                }
                return _stringBuilder.ToString();
            }
        }

        #region Assert Methods

        /// <summary>
        /// Assert에서 사용할 식별자
        /// </summary>
        /// <returns></returns>
        protected virtual string GetAssertIdentifier()
        {
            return name;
        }

        /// <summary>
        /// 조건이 true인지 Assert
        /// </summary>
        /// <param name="condition">검증할 조건</param>
        /// <param name="message">실패 시 메세지</param>
        protected void AssertIsTrue(bool condition, string message = "")
        {
            var fullMessage = string.Join(" ", $"[{GetAssertIdentifier()}]", message);
            Assert.IsTrue(condition, fullMessage);
        }

        /// <summary>
        /// 객체가 null이 아닌지 Assert
        /// </summary>
        /// <param name="obj">검증할 객체</param>
        /// <param name="message">실패 시 메세지</param>
        protected void AssertIsNotNull<T>(T obj, string message = "") where T : class
        {
            var fullMessage = string.Join(" ", $"[{GetAssertIdentifier()}]", message);
            Assert.IsNotNull(obj, fullMessage);
        }

        /// <summary>
        /// Unity Object가 null이 아닌지 Assert
        /// </summary>
        /// <param name="obj">검증할 Unity Object</param>
        /// <param name="message">실패 시 메세지</param>
        protected void AssertIsNotNull(Object obj, string message = "")
        {
            var fullMessage = string.Join(" ", $"[{GetAssertIdentifier()}]", message);
            Assert.IsNotNull(obj, fullMessage);
        }

        /// <summary>
        /// 객체가 null인지 Assert
        /// </summary>
        /// <param name="obj">검증할 객체</param>
        /// <param name="message">실패 시 메세지</param>
        protected void AssertIsNull<T>(T obj, string message = "") where T : class
        {
            var fullMessage = string.Join(" ", $"[{GetAssertIdentifier()}]", message);
            Assert.IsNull(obj, fullMessage);
        }

        /// <summary>
        /// Unity Object가 null인지 Assert
        /// </summary>
        /// <param name="obj">검증할 Unity Object</param>
        /// <param name="message">실패 시 메세지</param>
        protected void AssertIsNull(Object obj, string message = "")
        {
            var fullMessage = string.Join(" ", $"[{GetAssertIdentifier()}]", message);
            Assert.IsNull(obj, fullMessage);
        }

        /// <summary>
        /// 두 값이 같은지 Assert
        /// </summary>
        /// <param name="expected">예상값</param>
        /// <param name="actual">실제값</param>
        /// <param name="message">실패 시 메세지</param>
        protected void AssetAreEqual<T>(T expected, T actual, string message = "")
        {
            var fullMessage = string.Join(" ", $"[{GetAssertIdentifier()}]", message);
            Assert.AreEqual(expected, actual, fullMessage);
        }
        
        /// <summary>
        /// 두 값이 다른지 Assert
        /// </summary>
        /// <param name="expected">예상값</param>
        /// <param name="actual">실제값</param>
        /// <param name="message">실패 시 메세지</param>
        protected void AssetAreNotEqual<T>(T expected, T actual, string message = "")
        {
            var fullMessage = string.Join(" ", $"[{GetAssertIdentifier()}]", message);
            Assert.AreNotEqual(expected, actual, fullMessage);
        }

        /// <summary>
        /// Collection이 비어있지 않은지 Assert
        /// </summary>
        /// <param name="collection">검증할 Collection</param>
        /// <param name="message">실패 시 메세지</param>
        /// <typeparam name="T"></typeparam>
        protected void AssertIsNotEmpty<T>(ICollection<T> collection, string message = "")
        {
            var fullMessage = string.Join(" ", $"[{GetAssertIdentifier()}]", message);
            AssertIsNotNull(collection, fullMessage);
            Assert.IsTrue(collection.Count > 0, fullMessage);
        }

        /// <summary>
        /// 문자열이 null이거나 비어있지 않은지 Assert
        /// </summary>
        /// <param name="str">검증할 문자열d</param>
        /// <param name="message">실패 시 메세지</param>
        protected void AssertIsNotNullOrEmpty(string str, string message = "")
        {
            var fullMessage = string.Join(" ", $"[{GetAssertIdentifier()}]", message);
            Assert.IsFalse(string.IsNullOrEmpty(str), fullMessage);
        }

        #endregion
    }
}