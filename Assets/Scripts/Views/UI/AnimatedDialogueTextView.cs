using System;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;
using Game.Data;
using Game.Services.Dialogue;
using Game.Services.Input;

namespace Game.Views.UI
{
    public class AnimatedDialogueTextView : MonoBehaviour
    {
        [field: SerializeField] private TextMeshProUGUI TextComponent { get; set; }
        [field: SerializeField] private GameObject RootObject { get; set; }

        [Inject] private readonly IDialogueService _dialogueService;
        [Inject] private readonly IInputService _inputService;
        [Inject] private readonly DialogueAnimationConfig _animationConfig;

        private readonly CompositeDisposable _disposables = new();
        private Tween _animationTween;
        private Vector3[][] _originalVertices;
        private Vector3[] _originalCenters;
        private bool _isAnimating;
        private float _currentTime;
        private int _characterCount;

        private void Start()
        {
            if (RootObject is not null)
                RootObject.SetActive(false);

            _dialogueService.CurrentReplica
                .Subscribe(OnReplicaChanged)
                .AddTo(_disposables);

            _dialogueService.OnDialogueCompleted
                .Subscribe(_ => Hide())
                .AddTo(_disposables);

            _inputService.OnPrimaryClick
                .Subscribe(_ => HandleClick())
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
            StartAnimation(text);
        }

        private void StartAnimation(string text)
        {
            _animationTween?.Kill();

            if (TextComponent is null) return;

            TextComponent.text = text;
            TextComponent.ForceMeshUpdate();

            var textInfo = TextComponent.textInfo;
            _characterCount = textInfo.characterCount;
            _originalVertices = new Vector3[_characterCount][];
            _originalCenters = new Vector3[_characterCount];

            for (int i = 0; i < _characterCount; i++)
            {
                var charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible)
                {
                    _originalVertices[i] = null;
                    continue;
                }

                int matIndex = charInfo.materialReferenceIndex;
                int vIndex = charInfo.vertexIndex;
                Vector3[] meshVerts = textInfo.meshInfo[matIndex].vertices;

                _originalVertices[i] = new Vector3[4];
                Vector3 center = Vector3.zero;
                for (int v = 0; v < 4; v++)
                {
                    _originalVertices[i][v] = meshVerts[vIndex + v];
                    center += meshVerts[vIndex + v];
                }
                _originalCenters[i] = center / 4f;
            }

            float totalDuration = (_characterCount - 1) * _animationConfig.StaggerDelay + _animationConfig.LetterDuration;
            if (totalDuration <= 0f) totalDuration = 0.01f;

            _currentTime = 0f;
            _isAnimating = true;

            _animationTween = DOTween.To(
                () => _currentTime,
                x => _currentTime = x,
                totalDuration,
                totalDuration
            ).SetEase(Ease.Linear).SetUpdate(true).OnUpdate(UpdateVertices).OnComplete(OnAnimationComplete);
        }

        private void UpdateVertices()
        {
            if (TextComponent is null || TextComponent.textInfo is null) return;

            var textInfo = TextComponent.textInfo;

            for (int i = 0; i < _characterCount; i++)
            {
                if (_originalVertices[i] == null) continue;

                var charInfo = textInfo.characterInfo[i];
                int matIndex = charInfo.materialReferenceIndex;
                int vIndex = charInfo.vertexIndex;
                Vector3[] meshVerts = textInfo.meshInfo[matIndex].vertices;

                float letterStartTime = i * _animationConfig.StaggerDelay;
                float localProgress = Mathf.Clamp01((_currentTime - letterStartTime) / _animationConfig.LetterDuration);

                float scale = DOVirtual.EasedValue(0f, 1f, localProgress, _animationConfig.ScaleEase);
                float yOffset = DOVirtual.EasedValue(_animationConfig.OffsetY, 0f, localProgress, _animationConfig.PositionEase);

                Vector3 center = _originalCenters[i];
                for (int v = 0; v < 4; v++)
                {
                    Vector3 vert = _originalVertices[i][v];
                    vert = center + (vert - center) * scale;
                    vert.y += yOffset;
                    meshVerts[vIndex + v] = vert;
                }
            }

            TextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
        }

        private void OnAnimationComplete()
        {
            _isAnimating = false;
            _dialogueService.NotifyRevealCompleted();
        }

        private void HandleClick()
        {
            if (string.IsNullOrEmpty(_dialogueService.CurrentReplica.Value)) return;

            if (_isAnimating)
            {
                CompleteInstantly();
            }
            else
            {
                _dialogueService.AdvanceOrSkip();
            }
        }

        private void CompleteInstantly()
        {
            _animationTween?.Kill();
            _animationTween = null;
            _isAnimating = false;

            if (TextComponent is null || TextComponent.textInfo is null) return;

            var textInfo = TextComponent.textInfo;
            for (int i = 0; i < _characterCount; i++)
            {
                if (_originalVertices[i] == null) continue;

                var charInfo = textInfo.characterInfo[i];
                int matIndex = charInfo.materialReferenceIndex;
                int vIndex = charInfo.vertexIndex;
                Vector3[] meshVerts = textInfo.meshInfo[matIndex].vertices;

                for (int v = 0; v < 4; v++)
                {
                    meshVerts[vIndex + v] = _originalVertices[i][v];
                }
            }

            TextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
            _dialogueService.NotifyRevealCompleted();
        }

        private void Show()
        {
            if (RootObject is not null)
                RootObject.SetActive(true);
        }

        private void Hide()
        {
            _animationTween?.Kill();
            _isAnimating = false;
            if (TextComponent is not null)
                TextComponent.text = string.Empty;
            if (RootObject is not null)
                RootObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _animationTween?.Kill();
            _disposables?.Dispose();
        }
    }
}