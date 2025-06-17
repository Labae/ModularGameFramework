using System;
using UnityEngine;

namespace MarioGame.Core.Attributes
{
/// <summary>
    /// 조건부 필드 표시 속성
    /// 다른 필드의 값에 따라 Inspector에서 필드를 보이거나 숨깁니다
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class ConditionalFieldAttribute : PropertyAttribute
    {
        public string ConditionalSourceField { get; }
        public object ExpectedValue { get; }
        public bool Inverse { get; }
        public ConditionalType ConditionType { get; }

        /// <summary>
        /// bool 필드 기반 조건부 표시
        /// </summary>
        /// <param name="conditionalSourceField">조건이 되는 필드명</param>
        /// <param name="inverse">조건을 반전할지 여부 (false면 true일 때 표시, true면 false일 때 표시)</param>
        public ConditionalFieldAttribute(string conditionalSourceField, bool inverse = false)
        {
            ConditionalSourceField = conditionalSourceField;
            ExpectedValue = !inverse;
            Inverse = inverse;
            ConditionType = ConditionalType.Boolean;
        }

        /// <summary>
        /// 특정 값 기반 조건부 표시
        /// </summary>
        /// <param name="conditionalSourceField">조건이 되는 필드명</param>
        /// <param name="expectedValue">예상 값 (이 값과 같을 때 표시)</param>
        public ConditionalFieldAttribute(string conditionalSourceField, object expectedValue)
        {
            ConditionalSourceField = conditionalSourceField;
            ExpectedValue = expectedValue;
            Inverse = false;
            ConditionType = ConditionalType.Value;
        }

        /// <summary>
        /// 다중 조건 기반 표시 (enum 플래그 등)
        /// </summary>
        /// <param name="conditionalSourceField">조건이 되는 필드명</param>
        /// <param name="expectedValue">예상 값</param>
        /// <param name="conditionType">조건 타입</param>
        public ConditionalFieldAttribute(string conditionalSourceField, object expectedValue, ConditionalType conditionType)
        {
            ConditionalSourceField = conditionalSourceField;
            ExpectedValue = expectedValue;
            Inverse = false;
            ConditionType = conditionType;
        }
    }

    /// <summary>
    /// 조건부 필드의 조건 타입
    /// </summary>
    public enum ConditionalType
    {
        Boolean,        // bool 값 기반
        Value,          // 특정 값과 일치
        NotEqual,       // 특정 값과 불일치
        GreaterThan,    // 더 큰 값
        LessThan,       // 더 작은 값
        HasFlag,        // enum 플래그 포함
        NotHasFlag      // enum 플래그 미포함
    }
}