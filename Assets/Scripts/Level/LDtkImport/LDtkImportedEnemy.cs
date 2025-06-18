using System.Collections.Generic;
using LDtkUnity;
using MarioGame.Core.Data;
using MarioGame.Core.Extensions;
using UnityEngine;

namespace MarioGame.Level.LDtkImport
{
    public class LDtkImportedEnemy : MonoBehaviour, ILDtkImportedEntity
    {
        private LDtkFields _ldtkFields;
        
        private PatrolData _patrolData;
        
        public PatrolData PatrolData => GetPatrolPointsFromLDtk();

        public void OnLDtkImportEntity(EntityInstance entityInstance)
        {
            
        }
        
        private PatrolData GetPatrolPointsFromLDtk()
        {
            _ldtkFields = GetComponent<LDtkFields>();

            var points = new List<Vector2>();
            if (!_ldtkFields.TryGetField("patrol", out var field))
            {
                Debug.LogWarning("Failed to get patrol field");
                return null;
            }

            if (!field.TryGetArray(out var elements))
            {
                Debug.LogWarning("Failed to get array elements");
                return null;
            }
            
            
            foreach (var element in elements)
            {
                points.Add(element.GetPoint());
            }

            points.Add(transform.position.ToVector2());
            return new PatrolData(points.ToArray());
        }
    }
}