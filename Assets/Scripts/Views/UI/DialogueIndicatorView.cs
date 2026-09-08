using System;
using DG.Tweening;
using UniRx;
using UnityEngine;
using Zenject;
using Game.Data;
using Game.Services.Dialogue;

namespace Game.Views.UI
{
    public class DialogueIndicatorView : MonoBehaviour
    {
        [field: SerializeField] private CanvasGroup CanvasGroup { get; set; }
        [field: SerializeField] private RectTransform[] Chevrons { get; set; }

        [Inject] private readonly IDialogueService _dialogueService;
        [Inject] private readonly DialogueAnimationConfig _config;

        private readonly CompositeDisposable _disposables = new();
        private readonly Tween[] _bobTweens = new Tween[3];
        private readonly Tween[] _rotationTweens = new Tween[3];
        private readonly Vector3[] _initialPositions = new Vector3[3];

        private void Start()
        {
            if (CanvasGroup is not null)
            {
                CanvasGroup.alpha = 0f;
                CanvasGroup.blocksRaycasts = false;
            }

            if (Chevrons is not null)
            {
                for (int i = 0; i < Chevrons.Length && i < 3; i++)
                {
                    if (Chevrons[i] is not null)
                    {
                        _initialPositions[i] = Chevrons[i].localPosition;
                        Chevrons[i].localEulerAngles = new Vector3(0f, 0f, 90f);
                    }
                }
            }

            _dialogueService.CurrentReplica
                .Subscribe(OnReplicaChanged)
                .AddTo(_disposables);

            _dialogueService.IsCurrentReplicaFullyRevealed
                .Subscribe(OnFullyRevealedChanged)
                .AddTo(_disposables);

            _dialogueService.OnDialogueCompleted
                .Subscribe(_ => Hide())
                .AddTo(_disposables);
        }

        private void OnReplicaChanged(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                Hide();
                return;
            }

            Show();
            SetPrintingState();
        }

        private void OnFullyRevealedChanged(bool isFullyRevealed)
        {
            if (string.IsNullOrEmpty(_dialogueService.CurrentReplica.Value)) return;

            if (isFullyRevealed)
                SetWaitingState();
            else
                SetPrintingState();
        }

        private void SetPrintingState()
        {
            RotateChevrons(90f);
            StartBobbing();
        }

        private void SetWaitingState()
        {
            RotateChevrons(0f);
        }

        private void StartBobbing()
        {
            if (Chevrons is null) return;

            for (int i = 0; i < Chevrons.Length && i < 3; i++)
            {
                _bobTweens[i]?.Kill();

                if (Chevrons[i] is null) continue;

                Chevrons[i].localPosition = _initialPositions[i];
                float targetY = _initialPositions[i].y + _config.IndicatorBobAmplitude;

                _bobTweens[i] = Chevrons[i]
                    .DOLocalMoveY(targetY, _config.IndicatorBobDuration)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine)
                    .SetDelay(i * _config.IndicatorStaggerDelay)
                    .SetUpdate(true);
            }
        }

        private void RotateChevrons(float targetZ)
        {
            if (Chevrons is null) return;

            for (int i = 0; i < Chevrons.Length && i < 3; i++)
            {
                _rotationTweens[i]?.Kill();

                if (Chevrons[i] is null) continue;

                Vector3 targetRotation = new Vector3(0f, 0f, targetZ);

                _rotationTweens[i] = Chevrons[i]
                    .DORotate(targetRotation, _config.IndicatorRotationDuration)
                    .SetEase(_config.IndicatorRotationEase)
                    .SetUpdate(true);
            }
        }

        private void Show()
        {
            if (CanvasGroup is null) return;

            CanvasGroup.blocksRaycasts = true;
            CanvasGroup.DOFade(1f, _config.IndicatorFadeDuration).SetUpdate(true);
        }

        private void Hide()
        {
            KillAllTweens();

            if (CanvasGroup is null) return;

            CanvasGroup.blocksRaycasts = false;
            CanvasGroup.DOFade(0f, _config.IndicatorFadeDuration).SetUpdate(true);
        }

        private void KillAllTweens()
        {
            for (int i = 0; i < 3; i++)
            {
                _bobTweens[i]?.Kill();
                _bobTweens[i] = null;

                _rotationTweens[i]?.Kill();
                _rotationTweens[i] = null;

                if (Chevrons is not null && i < Chevrons.Length && Chevrons[i] is not null)
                {
                    Chevrons[i].localPosition = _initialPositions[i];
                }
            }
        }

        private void OnDestroy()
        {
            KillAllTweens();
            _disposables?.Dispose();
        }
    }
}