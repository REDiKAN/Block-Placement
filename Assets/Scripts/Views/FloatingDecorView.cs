using System;
using DG.Tweening;
using UniRx;
using UnityEngine;
using Zenject;
using Game.Data;
using Game.Services.Input;
using Game.Services.Water;
using Game.Services.Achievements;

namespace Game.Views
{
    public class FloatingDecorView : MonoBehaviour
    {
        [field: SerializeField] private Transform WaterTransform { get; set; }
        [field: SerializeField, Range(0f, 45f)] private float MaxTiltDegrees { get; set; } = 10f;
        [field: SerializeField, Range(0f, 1f)] private float BobStrength { get; set; } = 1f;
        [field: SerializeField, Range(0f, 1f)] private float HorizontalFollow { get; set; } = 0.25f;
        [field: SerializeField, Range(0f, 3f)] private float SettleDuration { get; set; } = 1.5f;
        [field: SerializeField] public bool IsInteractable { get; private set; } = true;

        [InjectOptional] private IWaterShaderService _waterShaderService;
        [InjectOptional] private IInputService _inputService;
        [InjectOptional] private IInputContextService _contextService;
        [InjectOptional] private FloatingDecorInteractionConfig _interactionConfig;
        [Inject(Id = "GameCamera", Optional = true)] private Camera _gameCamera;
        [InjectOptional] private IAchievementEventBus _achievementEventBus;

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

        private float _diveOffsetY;
        private Sequence _diveSequence;
        private readonly CompositeDisposable _disposables = new();
        private Collider _collider;

        private void Start()
        {
            _collider = GetComponent<Collider>();
            if (_inputService is null) return;

            _inputService.OnPrimaryClick
                .Subscribe(TryDive)
                .AddTo(_disposables);
        }

        private void TryDive(Vector2 mousePosition)
        {
            if (!IsInteractable) return;
            if (_interactionConfig is null || _gameCamera is null || _collider is null) return;

            var ctx = _contextService?.CurrentContext.Value ?? InputContext.None;
            if (ctx != InputContext.PlaceBlock && ctx != InputContext.None) return;

            if (_diveSequence is not null && _diveSequence.IsPlaying()) return;

            var ray = _gameCamera.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out var hit, _interactionConfig.MaxDistance, _interactionConfig.InteractableLayer))
            {
                if (hit.collider == _collider)
                {
                    _achievementEventBus?.Publish(DecorSunkEvent.Default);
                    PlayDiveAnimation();
                }
            }
        }

        private void PlayDiveAnimation()
        {
            _diveSequence?.Kill();
            _diveSequence = DOTween.Sequence();

            _diveSequence.Append(
                DOTween.To(() => _diveOffsetY, x => _diveOffsetY = x, _interactionConfig.DiveDepth, _interactionConfig.DiveDownDuration)
                    .SetEase(_interactionConfig.DiveDownEase)
            );

            _diveSequence.Append(
                DOTween.To(() => _diveOffsetY, x => _diveOffsetY = x, _interactionConfig.JumpHeight, _interactionConfig.JumpUpDuration)
                    .SetEase(_interactionConfig.JumpUpEase)
            );

            _diveSequence.Append(
                DOTween.To(() => _diveOffsetY, x => _diveOffsetY = x, 0f, _interactionConfig.SettleDownDuration)
                    .SetEase(_interactionConfig.SettleDownEase)
            );

            _diveSequence.OnComplete(() => _diveSequence = null);
        }

        private void LateUpdate()
        {
            if (_waterShaderService?.CurrentConfig.Value is null)
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

            var finalPosition = _basePosition + WaterTransform.TransformVector(_offset);
            finalPosition += Vector3.up * _diveOffsetY;
            transform.position = finalPosition;

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

        private void OnDestroy()
        {
            _diveSequence?.Kill();
            _disposables?.Dispose();
        }
    }
}