using System;
using MarioGame.Core.Utilities;
using UnityEngine;

namespace MarioGame.Core.Data
{
    public class PatrolData
    {
        public enum PatrolType
        {
            OneTime,
            Loop,
            PingPong,
        }
        
        private Vector2[] _patrolPoints;
        private int _currentIndex;
        private bool _isReversing;
        private bool _isCompleted;
        
        private PatrolType _patrolType;
        
        public Vector2 CurrentPatrolPoint => _patrolPoints[_currentIndex];
        public bool HasPatrolPoint => _patrolPoints?.Length > 0;
        public bool IsCompleted => _isCompleted;

        public PatrolData(Vector2[] patrolPoints, PatrolType patrolType = PatrolType.Loop)
        {
            _patrolType = patrolType;
            _isReversing = false;
            _isCompleted = false;
            SetPatrolPoints(patrolPoints);
        }

        public void SetPatrolPoints(Vector2[] patrolPoints)
        {
            _patrolPoints = patrolPoints;
            _currentIndex = 0;
            _isReversing = false;
        }

        public void SetPatrolType(PatrolType patrolType)
        {
            _patrolType = patrolType;
        }

        public void MoveToNext()
        {
            if (!HasPatrolPoint || _isCompleted)
            {
                return;
            }

            switch (_patrolType)
            {
                case PatrolType.OneTime:
                    _currentIndex++;
                    if (_currentIndex >= _patrolPoints.Length)
                    {
                        _isCompleted = true;
                    }
                    break;
                case PatrolType.Loop:
                    _currentIndex = (_currentIndex + 1) % _patrolPoints.Length;
                    break;
                case PatrolType.PingPong:
                    if (_isReversing)
                    {
                        _currentIndex--;
                        if (_currentIndex <= 0)
                        {
                            _currentIndex = 0;
                            _isReversing = false;
                        }
                    }
                    else
                    {
                        _currentIndex++;
                        if (_currentIndex >= _patrolPoints.Length)
                        {
                            _currentIndex = _patrolPoints.Length - 1;
                            _isReversing = true;
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool HasReachedTarget(Vector2 currentPosition, float threshold = FloatUtility.POSITION_THRESHOLD)
        {
            if (!HasPatrolPoint || _isCompleted)
            {
                return false;
            }
            
            var distance = Vector2.Distance(currentPosition, CurrentPatrolPoint);
            return distance <= threshold;
        }
        
        public bool HasReachedTargetX(Vector2 currentPosition, float threshold = FloatUtility.POSITION_THRESHOLD)
        {
            if (!HasPatrolPoint || _isCompleted)
            {
                return false;
            }
            
            var distance = Mathf.Abs(currentPosition.x - CurrentPatrolPoint.x);
            return distance <= threshold;
        }
        

        public Vector2? GetPatrolPoint(int index)
        {
            if (!HasPatrolPoint)
            {
                return null;
            }

            if (index < 0 || index >= _patrolPoints.Length)
            {
                return null;
            }
            
            return _patrolPoints[index];
        }

        public void SetToNearestPoint(Vector2 currentPosition)
        {
            if (!HasPatrolPoint)
            {
                return;
            }
            
            float minDistance = float.MaxValue;
            int nearestIndex = 0;

            for (int i = 0; i < _patrolPoints.Length; i++)
            {
                var distance = Vector2.Distance(currentPosition, _patrolPoints[i]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestIndex = i;
                }
            }
            
            _currentIndex = nearestIndex;
            _isCompleted = false;
        }
    }
}