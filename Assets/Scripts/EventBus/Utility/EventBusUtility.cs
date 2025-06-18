using System;
using System.Collections.Generic;
using System.Reflection;
using EventBus.Core;
using UnityEditor;
using UnityEngine;

namespace EventBus.Utility
{
    public static class EventBusUtility
    {
        public static IReadOnlyList<Type> EventTypes { get; set; }
        public static IReadOnlyList<Type> EventBusTypes { get; set; }

#if UNITY_EDITOR
        public static PlayModeStateChange PlayModeState { get; set; }

        [InitializeOnLoadMethod]
        public static void InitializeEditor()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            PlayModeState = playModeStateChange;
            if (playModeStateChange == PlayModeStateChange.ExitingPlayMode)
            {
                ClearAllBusses();
            }
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            EventTypes = PredefinedAssemblyUtility.GetTypes(typeof(IEvent));
            EventBusTypes = InitializeAllBusses();
        }

        private static List<Type> InitializeAllBusses()
        {
            var eventBusTypes = new List<Type>();
            
            var typedef = typeof(EventBus<>);
            foreach (var eventType in EventTypes)
            {
                var busType = typedef.MakeGenericType(eventType);
                eventBusTypes.Add(busType);
                Debug.Log($"Initialized EventBus<{eventType.Name}>");
            }

            return eventBusTypes;
        }

        public static void ClearAllBusses()
        {
            Debug.Log($"Clear all busses...");
            for (int i = 0; i < EventBusTypes.Count; i++)
            {
                var busType = EventBusTypes[i];
                var clearMethod = busType.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
                clearMethod?.Invoke(null, null);
            }
        }
    }
}