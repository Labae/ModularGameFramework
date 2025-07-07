using System.Collections.Generic;
using System.Text;
using MarioGame.Debugging.Interfaces;
using UnityEngine;
using UnityEngine.Assertions;

namespace MarioGame.Debugging.Core
{
    public class AssertManager : IAssertManager
    {
        public bool EnableAsserts { get; set; } = true;
        
        private readonly StringBuilder _stringBuilder = new(256);

        private string FormatMessage(string identifier, string message)
        {
            _stringBuilder.Clear();
            _stringBuilder.Append($"[{identifier}] ");
            _stringBuilder.Append(message);
            return _stringBuilder.ToString();
        }

        public void AssertIsTrue(bool condition, string identifier, string message = "")
        {
            Assert.IsTrue(condition, FormatMessage(identifier, message));
        }

        public void AssertIsNotNull<T>(T obj, string identifier, string message = "") where T : class
        {
            Assert.IsNotNull(obj, FormatMessage(identifier, message));
        }

        public void AssertIsNotNull(Object obj, string identifier, string message = "")
        {
            Assert.IsNotNull(obj, FormatMessage(identifier, message));
        }

        public void AssertIsNull<T>(T obj, string identifier, string message = "") where T : class
        {
            Assert.IsNull(obj, FormatMessage(identifier, message));
        }

        void IAssertManager.AssertIsNull(Object obj, string identifier, string message)
        {
            Assert.IsNull(obj, FormatMessage(identifier, message));
        }

        public void AssetAreEqual<T>(T expected, T actual, string identifier, string message = "")
        {
            Assert.AreEqual(expected, actual, FormatMessage(identifier, message));
        }

        public void AssetAreNotEqual<T>(T expected, T actual, string identifier, string message = "")
        {
            Assert.AreNotEqual(expected, actual, FormatMessage(identifier, message));
        }

        public void AssertIsNotEmpty<T>(ICollection<T> collection, string identifier, string message = "")
        {
            AssertIsNotNull(collection, FormatMessage(identifier, message));
            Assert.IsTrue(collection.Count > 0, FormatMessage(identifier, message));
        }

        public void AssertIsNotNullOrEmpty(string str, string identifier, string message = "")
        {
            Assert.IsFalse(string.IsNullOrEmpty(str), FormatMessage(identifier, message));
        }
    }
}