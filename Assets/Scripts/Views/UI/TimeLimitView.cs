using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Game.Services.Time;

namespace Game.Views.UI
{
    public class TimeLimitView : MonoBehaviour
    {
        [field: SerializeField] private Slider ProgressBar { get; set; }
        [field: SerializeField] private TextMeshProUGUI TimeText { get; set; }
        [field: SerializeField] private GameObject RootObject { get; set; }
        [field: SerializeField] private Image FillImage { get; set; }
        [field: SerializeField] private float ColorTransitionDuration { get; set; } = 0.5f;

        [Inject] private readonly ITimeLimitService _timeLimitService;

        private static readonly Color FullColor = new(0.2f, 0.8f, 0.2f, 1f);
        private static readonly Color HalfColor = new(0.9f, 0.8f, 0.1f, 1f);
        private static readonly Color EmptyColor = new(0.9f, 0.15f, 0.15f, 1f);

        private float _initialTime;
        private Tween _colorTween;

        private void Start()
        {
            if (RootObject is not null)
                RootObject.SetActive(false);

            _timeLimitService.IsRunning
                .Subscribe(HandleRunningState)
                .AddTo(this);

            _timeLimitService.RemainingTime
                .Subscribe(UpdateTimeDisplay)
                .AddTo(this);
        }

        private void HandleRunningState(bool isRunning)
        {
            if (RootObject is not null)
                RootObject.SetActive(isRunning || (_timeLimitService.RemainingTime.Value > 0f));
        }

        private void UpdateTimeDisplay(float remaining)
        {
            if (_initialTime == 0f && remaining > 0f)
                _initialTime = remaining;

            if (TimeText is not null)
                TimeText.text = remaining.ToString("F3");

            if (ProgressBar is not null && _initialTime > 0f)
                ProgressBar.value = remaining / _initialTime;

            UpdateFillColor(remaining);
        }

        private void UpdateFillColor(float remaining)
        {
            if (FillImage is null || _initialTime <= 0f) return;

            var ratio = remaining / _initialTime;
            var targetColor = ratio > 0.5f
                ? Color.Lerp(HalfColor, FullColor, (ratio - 0.5f) * 2f)
                : Color.Lerp(EmptyColor, HalfColor, ratio * 2f);

            _colorTween?.Kill();
            _colorTween = FillImage.DOColor(targetColor, ColorTransitionDuration);
        }

        private void OnDestroy()
        {
            _colorTween?.Kill();
        }
    }
}