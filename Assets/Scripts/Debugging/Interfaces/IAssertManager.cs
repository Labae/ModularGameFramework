using System.Collections.Generic;
using UnityEngine;

namespace MarioGame.Debugging.Interfaces
{
    public interface IAssertManager
    {
        /// <summary>
        /// 조건이 true인지 Assert
        /// </summary>
        /// <param name="condition">검증할 조건</param>
        /// <param name="identifier"></param>
        /// <param name="message">실패 시 메세지</param>
        void AssertIsTrue(bool condition, string identifier, string message = "");

        /// <summary>
        /// 객체가 null이 아닌지 Assert
        /// </summary>
        /// <param name="obj">검증할 객체</param>
        /// <param name="message">실패 시 메세지</param>
        void AssertIsNotNull<T>(T obj, string identifier, string message = "") where T : class;

        /// <summary>
        /// Unity Object가 null이 아닌지 Assert
        /// </summary>
        /// <param name="obj">검증할 Unity Object</param>
        /// <param name="message">실패 시 메세지</param>
        void AssertIsNotNull(Object obj, string identifier, string message = "");

        /// <summary>
        /// 객체가 null인지 Assert
        /// </summary>
        /// <param name="obj">검증할 객체</param>
        /// <param name="message">실패 시 메세지</param>
        void AssertIsNull<T>(T obj, string identifier, string message = "") where T : class;

        /// <summary>
        /// Unity Object가 null인지 Assert
        /// </summary>
        /// <param name="obj">검증할 Unity Object</param>
        /// <param name="message">실패 시 메세지</param>
        protected void AssertIsNull(Object obj, string identifier, string message = "");

        /// <summary>
        /// 두 값이 같은지 Assert
        /// </summary>
        /// <param name="expected">예상값</param>
        /// <param name="actual">실제값</param>
        /// <param name="message">실패 시 메세지</param>
        void AssetAreEqual<T>(T expected, T actual, string identifier, string message = "");

        /// <summary>
        /// 두 값이 다른지 Assert
        /// </summary>
        /// <param name="expected">예상값</param>
        /// <param name="actual">실제값</param>
        /// <param name="message">실패 시 메세지</param>
        void AssetAreNotEqual<T>(T expected, T actual, string identifier, string message = "");

        /// <summary>
        /// Collection이 비어있지 않은지 Assert
        /// </summary>
        /// <param name="collection">검증할 Collection</param>
        /// <param name="message">실패 시 메세지</param>
        /// <typeparam name="T"></typeparam>
        void AssertIsNotEmpty<T>(ICollection<T> collection, string identifier, string message = "");

        /// <summary>
        /// 문자열이 null이거나 비어있지 않은지 Assert
        /// </summary>
        /// <param name="str">검증할 문자열d</param>
        /// <param name="message">실패 시 메세지</param>
        void AssertIsNotNullOrEmpty(string str, string identifier, string message = "");
    }
}