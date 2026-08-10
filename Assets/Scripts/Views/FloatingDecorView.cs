using UnityEngine;
using Zenject;
using Game.Services.Water;

namespace Game.Views
{
    public class FloatingDecorView : MonoBehaviour
    {
        [field: SerializeField] private Transform WaterTransform { get; set; }
        [field: SerializeField, Range(0f, 45f)] private float MaxTiltDegrees { get; set; } = 10f;
        [field: SerializeField, Range(0f, 1f)] private float BobStrength { get; set; } = 1f;
        [field: SerializeField, Range(0f, 1f)] private float HorizontalFollow { get; set; } = 0.25f;
        [field: SerializeField, Range(0f, 3f)] private float SettleDuration { get; set; } = 1.5f;

        [Inject] private IWaterShaderService _waterShaderService;

        private const float DegenerateEpsilon = 1e-6f;

        private Vector3 _basePosition;
        private Vector3 _baseForward;
        private Vector2 _localBaseXZ;
        private float _maxTiltRadians;
        private float _settleStartTime;
        private bool _isInitialized;
        private Vector3 _offset;
        private Vector3 _up;
        private Vector3 _forward;
        private Vector3 _right;

        private void LateUpdate()
        {
            if (_waterShaderService.CurrentConfig.Value is null)
                return;

            if (!_isInitialized)
                InitializeBase();

            var settle = Mathf.Clamp01((Time.time - _settleStartTime) / Mathf.Max(SettleDuration, DegenerateEpsilon));
            var parameters = _waterShaderService.CurrentParameters.Value;

            GerstnerWaveEvaluator.Evaluate(_localBaseXZ, Time.time, in parameters, out var displacement, out var normal);

            _offset.Set(
                displacement.x * HorizontalFollow * settle,
                displacement.y * BobStrength * settle,
                displacement.z * HorizontalFollow * settle);
            transform.position = _basePosition + WaterTransform.TransformVector(_offset);

            _up = Vector3.RotateTowards(Vector3.up, WaterTransform.TransformDirection(normal).normalized, _maxTiltRadians * settle, 1f);
            _forward = Vector3.ProjectOnPlane(_baseForward, _up);
            if (_forward.sqrMagnitude < DegenerateEpsilon)
                _forward = _baseForward;
            _forward.Normalize();
            _right = Vector3.Cross(_up, _forward).normalized;
            _forward = Vector3.Cross(_right, _up).normalized;
            transform.rotation = Quaternion.LookRotation(_forward, _up);
        }

        private void InitializeBase()
        {
            if (WaterTransform is null)
            {
                Debug.LogError("[FloatingDecorView] WaterTransform is not assigned.");
                enabled = false;
                return;
            }

            _basePosition = transform.position;
            _baseForward = transform.rotation * Vector3.forward;
            _maxTiltRadians = MaxTiltDegrees * Mathf.Deg2Rad;
            var localBase = WaterTransform.InverseTransformPoint(_basePosition);
            _localBaseXZ = new Vector2(localBase.x, localBase.z);
            _settleStartTime = Time.time;
            _isInitialized = true;
        }
    }
}