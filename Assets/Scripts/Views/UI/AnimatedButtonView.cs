using DG.Tweening;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Views.UI
{
    [RequireComponent(typeof(Button))]
    public class AnimatedButtonView : MonoBehaviour
    {
        [SerializeField] private Image _targetImage;
        [SerializeField] private Image _backgroundUpBar, _backgroundLowBar;

        private Sequence _hoverSequence;
        private Sequence _clickSequence;
        private Button _button;

        private Color _baseColorTargetImage;


        private void Awake()
        {
            _button = GetComponent<Button>();

            _baseColorTargetImage = _targetImage.color;
        }

        private void Start()
        {
            if (_button is null) return;

            _button.OnPointerEnterAsObservable()
                .Subscribe(_ => AnimateHoverEnter())
                .AddTo(this);

            _button.OnPointerExitAsObservable()
                .Subscribe(_ => AnimateHoverExit())
                .AddTo(this);

            _button.OnPointerDownAsObservable()
                .Subscribe(_ => AnimatePress())
                .AddTo(this);

            _button.OnPointerUpAsObservable()
                .Subscribe(_ => AnimateRelease())
                .AddTo(this);
        }

        private void AnimateHoverEnter()
        {
            _hoverSequence?.Kill();
            _hoverSequence = DOTween.Sequence();

            _hoverSequence.Join(_backgroundUpBar.DOFillAmount(1f, 0.2f).SetEase(Ease.OutBack));
            _hoverSequence.Join(_backgroundLowBar.DOFillAmount(1f, 0.2f).SetEase(Ease.OutBack));
            _hoverSequence.Join(_targetImage.DOColor(new Color(_baseColorTargetImage.r, _baseColorTargetImage.g, _baseColorTargetImage.b, 1f), 0.2f));
        }
        private void AnimateHoverExit()
        {
            _hoverSequence?.Kill();
            _hoverSequence = DOTween.Sequence();

            _hoverSequence.Join(_backgroundUpBar.DOFillAmount(0f, 0.2f).SetEase(Ease.OutBack));
            _hoverSequence.Join(_backgroundLowBar.DOFillAmount(0f, 0.2f).SetEase(Ease.OutBack));
            _hoverSequence.Join(_targetImage.DOColor(new Color(_baseColorTargetImage.r, _baseColorTargetImage.g, _baseColorTargetImage.b, 0.5f), 0.2f));
        }

        private void AnimatePress()
        {
            _clickSequence?.Kill();
            _clickSequence = DOTween.Sequence();
        }

        private void AnimateRelease()
        {
            _clickSequence?.Kill();
            _clickSequence = DOTween.Sequence();
        }

        private void OnDisable()
        {
            _hoverSequence?.Kill();
            _clickSequence?.Kill();
        }
    }
}