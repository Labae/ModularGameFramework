using System;
using System.Diagnostics;
using MarioGame.Core.Attributes;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace MarioGame.Editor.PropertyAttributes
{
    [CustomPropertyDrawer(typeof(ConditionalAttribute))]
    public class ConditionalFieldPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var conditionalAttribute = (ConditionalFieldAttribute)attribute;

            if (ShouldShowProperty(property, conditionalAttribute))
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var conditionalAttribute = (ConditionalFieldAttribute)attribute;

            if (ShouldShowProperty(property, conditionalAttribute))
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }

            return -EditorGUIUtility.standardVerticalSpacing; // 완전히 숨김
        }

        private bool ShouldShowProperty(SerializedProperty property, ConditionalFieldAttribute conditionalAttribute)
        {
            var sourceProperty = GetRelativeProperty(property, conditionalAttribute.ConditionalSourceField);

            if (sourceProperty == null)
            {
                Debug.LogWarning(
                    $"ConditionalField: Could not find property '{conditionalAttribute.ConditionalSourceField}' " +
                    $"for '{property.name}' in '{property.serializedObject.targetObject.name}'");
                return true; // 소스 프로퍼티를 찾을 수 없으면 기본적으로 표시
            }

            return EvaluateCondition(sourceProperty, conditionalAttribute);
        }

        private SerializedProperty GetRelativeProperty(SerializedProperty property, string propertyName)
        {
            // 현재 프로퍼티의 경로에서 상대적으로 다른 프로퍼티 찾기
            var propertyPath = property.propertyPath;
            var lastDotIndex = propertyPath.LastIndexOf('.');

            if (lastDotIndex >= 0)
            {
                var basePath = propertyPath.Substring(0, lastDotIndex + 1);
                return property.serializedObject.FindProperty(basePath + propertyName);
            }

            return property.serializedObject.FindProperty(propertyName);
        }

        private bool EvaluateCondition(SerializedProperty sourceProperty,
            ConditionalFieldAttribute conditionalAttribute)
        {
            switch (conditionalAttribute.ConditionType)
            {
                case ConditionalType.Boolean:
                    return EvaluateBooleanCondition(sourceProperty, conditionalAttribute);

                case ConditionalType.Value:
                    return EvaluateValueCondition(sourceProperty, conditionalAttribute.ExpectedValue);

                case ConditionalType.NotEqual:
                    return !EvaluateValueCondition(sourceProperty, conditionalAttribute.ExpectedValue);

                case ConditionalType.GreaterThan:
                    return EvaluateNumericCondition(sourceProperty, conditionalAttribute.ExpectedValue,
                        (a, b) => a > b);

                case ConditionalType.LessThan:
                    return EvaluateNumericCondition(sourceProperty, conditionalAttribute.ExpectedValue,
                        (a, b) => a < b);

                case ConditionalType.HasFlag:
                    return EvaluateFlagCondition(sourceProperty, conditionalAttribute.ExpectedValue, true);

                case ConditionalType.NotHasFlag:
                    return EvaluateFlagCondition(sourceProperty, conditionalAttribute.ExpectedValue, false);

                default:
                    return true;
            }
        }

        private bool EvaluateBooleanCondition(SerializedProperty sourceProperty,
            ConditionalFieldAttribute conditionalAttribute)
        {
            if (sourceProperty.propertyType != SerializedPropertyType.Boolean)
            {
                Debug.LogWarning(
                    $"ConditionalField: Property '{sourceProperty.name}' is not a boolean but used with boolean condition");
                return true;
            }

            bool sourceValue = sourceProperty.boolValue;
            bool expectedValue = (bool)conditionalAttribute.ExpectedValue;

            return conditionalAttribute.Inverse ? sourceValue != expectedValue : sourceValue == expectedValue;
        }

        private bool EvaluateValueCondition(SerializedProperty sourceProperty, object expectedValue)
        {
            switch (sourceProperty.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    return sourceProperty.boolValue.Equals(expectedValue);

                case SerializedPropertyType.Integer:
                    return sourceProperty.intValue.Equals(expectedValue);

                case SerializedPropertyType.Float:
                    return Mathf.Approximately(sourceProperty.floatValue, Convert.ToSingle(expectedValue));

                case SerializedPropertyType.String:
                    return sourceProperty.stringValue.Equals(expectedValue?.ToString());

                case SerializedPropertyType.Enum:
                    return sourceProperty.enumValueIndex.Equals(expectedValue);

                default:
                    Debug.LogWarning($"ConditionalField: Unsupported property type {sourceProperty.propertyType}");
                    return true;
            }
        }

        private bool EvaluateNumericCondition(SerializedProperty sourceProperty, object expectedValue,
            Func<float, float, bool> comparison)
        {
            float sourceValue = 0f;
            float expectedFloat = Convert.ToSingle(expectedValue);

            switch (sourceProperty.propertyType)
            {
                case SerializedPropertyType.Integer:
                    sourceValue = sourceProperty.intValue;
                    break;

                case SerializedPropertyType.Float:
                    sourceValue = sourceProperty.floatValue;
                    break;

                default:
                    Debug.LogWarning(
                        $"ConditionalField: Property '{sourceProperty.name}' is not numeric but used with numeric condition");
                    return true;
            }

            return comparison(sourceValue, expectedFloat);
        }

        private bool EvaluateFlagCondition(SerializedProperty sourceProperty, object expectedValue, bool shouldHaveFlag)
        {
            if (sourceProperty.propertyType != SerializedPropertyType.Enum)
            {
                Debug.LogWarning(
                    $"ConditionalField: Property '{sourceProperty.name}' is not an enum but used with flag condition");
                return true;
            }

            int sourceFlags = sourceProperty.intValue;
            int expectedFlag = Convert.ToInt32(expectedValue);

            bool hasFlag = (sourceFlags & expectedFlag) == expectedFlag;
            return shouldHaveFlag ? hasFlag : !hasFlag;
        }
    }
}