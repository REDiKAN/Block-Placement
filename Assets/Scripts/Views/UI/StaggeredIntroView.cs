using System.Collections.Generic;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Game.Data;

namespace Game.Views.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class StaggeredIntroView : MonoBehaviour
    {
        [field: SerializeField] public StaggeredAnimationConfig Config { get; private set; }

        private readonly List<RectTransform> _targets = new();
        private readonly Dictionary<RectTransform, Vector2> _basePositions = new();
        private readonly Dictionary<RectTransform, Vector3> _baseScales = new();
        private readonly List<CanvasGroup> _canvasGroups = new();

        private Sequence _introSequence;
        private System.IDisposable _frameDelayDisposable;
        private LayoutGroup _layoutGroup;
        private ContentSizeFitter _sizeFitter;
        private CanvasGroup _rootCanvasGroup;

        private void Awake()
        {
            _layoutGroup = GetComponent<LayoutGroup>();
            _sizeFitter = GetComponent<ContentSizeFitter>();

            _rootCanvasGroup = GetComponent<CanvasGroup>();
            if (_rootCanvasGroup is null) _rootCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            if (_rootCanvasGroup is not null)
            {
                _rootCanvasGroup.alpha = 0f;
                _rootCanvasGroup.blocksRaycasts = false;
            }

            _frameDelayDisposable?.Dispose();
            _frameDelayDisposable = Observable.NextFrame(FrameCountType.FixedUpdate)
                .Subscribe(_ => ExecuteIntro())
                .AddTo(this);
        }

        private void ExecuteIntro()
        {
            if (!isActiveAndEnabled || Config is null) return;

            if (_layoutGroup is not null)
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);

            RefreshTargets();

            if (_rootCanvasGroup is not null)
            {
                _rootCanvasGroup.alpha = 1f;
                _rootCanvasGroup.blocksRaycasts = true;
            }

            PlayIntro();
        }

        private void RefreshTargets()
        {
            _targets.Clear();
            _canvasGroups.Clear();
            CleanUpDeadReferences();

            for (var i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i) is not RectTransform rt) continue;

                if (!_baseScales.ContainsKey(rt))
                {
                    _baseScales[rt] = rt.localScale;
                    _basePositions[rt] = rt.anchoredPosition;
                }

                _targets.Add(rt);

                var cg = rt.GetComponent<CanvasGroup>();
                if (cg is null) cg = rt.gameObject.AddComponent<CanvasGroup>();
                _canvasGroups.Add(cg);
            }
        }

        private void CleanUpDeadReferences()
        {
            var deadKeys = new List<RectTransform>();
            foreach (var kvp in _baseScales)
            {
                if (kvp.Key == null) deadKeys.Add(kvp.Key);
            }
            foreach (var key in deadKeys)
            {
                _baseScales.Remove(key);
                _basePositions.Remove(key);
            }
        }

        private void OnDisable()
        {
            _frameDelayDisposable?.Dispose();
            _frameDelayDisposable = null;
            KillAndReset();
        }

        private void PlayIntro()
        {
            if (_targets.Count == 0) return;

            if (_layoutGroup is not null) _layoutGroup.enabled = false;
            if (_sizeFitter is not null) _sizeFitter.enabled = false;

            _introSequence?.Kill();
            _introSequence = DOTween.Sequence();

            for (var i = 0; i < _targets.Count; i++)
            {
                var target = _targets[i];
                var originalPos = _basePositions[target];
                var originalScale = _baseScales[target];
                var cg = _canvasGroups[i];

                var startPos = originalPos + new Vector2(Config.OffsetX, 0f);
                var startScale = originalScale * Config.InitialScale;

                target.anchoredPosition = startPos;
                target.localScale = startScale;
                cg.alpha = 0f;
                cg.blocksRaycasts = false;

                var delay = i * Config.StaggerDelay;

                var posTween = target.DOAnchorPos(originalPos, Config.Duration).SetEase(Config.PositionEase);
                var scaleTween = target.DOScale(originalScale, Config.Duration).SetEase(Config.ScaleEase);
                var fadeTween = cg.DOFade(1f, Config.Duration).SetEase(Config.FadeEase);

                var itemSequence = DOTween.Sequence()
                    .Join(posTween)
                    .Join(scaleTween)
                    .Join(fadeTween)
                    .SetDelay(delay)
                    .OnComplete(() =>
                    {
                        if (cg is not null) cg.blocksRaycasts = true;
                    });

                _introSequence.Join(itemSequence);
            }

            _introSequence.OnComplete(RestoreLayoutGroup);
        }

        private void RestoreLayoutGroup()
        {
            if (_layoutGroup is not null) _layoutGroup.enabled = true;
            if (_sizeFitter is not null) _sizeFitter.enabled = true;
        }

        private void KillAndReset()
        {
            _introSequence?.Kill();
            _introSequence = null;

            for (var i = 0; i < _targets.Count; i++)
            {
                var target = _targets[i];
                if (target is null) continue;

                if (_basePositions.TryGetValue(target, out var pos))
                    target.anchoredPosition = pos;

                if (_baseScales.TryGetValue(target, out var scale))
                    target.localScale = scale;

                if (_canvasGroups[i] is not null)
                {
                    _canvasGroups[i].alpha = 1f;
                    _canvasGroups[i].blocksRaycasts = true;
                }
            }

            if (_rootCanvasGroup is not null)
            {
                _rootCanvasGroup.alpha = 0f;
                _rootCanvasGroup.blocksRaycasts = false;
            }

            RestoreLayoutGroup();
        }

        private void OnDestroy()
        {
            _frameDelayDisposable?.Dispose();
            _introSequence?.Kill();
        }
    }
}