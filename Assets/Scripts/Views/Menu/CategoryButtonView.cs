using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using Game.Data;

namespace Game.Views.Menu
{
    public class CategoryButtonView : MonoBehaviour
    {
        [field: SerializeField] private TextMeshProUGUI Label { get; set; }
        [field: SerializeField] private Image IconImage { get; set; }
        [field: SerializeField] private Button Button { get; set; }

        private readonly Subject<CategoryConfig> _onClick = new();
        private readonly Subject<bool> _onHover = new();

        public IObservable<CategoryConfig> OnClick => _onClick;
        public IObservable<bool> OnHover => _onHover;

        private CategoryConfig _config;

        public void Initialize(CategoryConfig config)
        {
            _config = config;
            if (Label is not null) Label.text = config.Title;

            if (IconImage is not null)
            {
                if (config.Icon is not null)
                {
                    IconImage.sprite = config.Icon;
                    IconImage.gameObject.SetActive(true);
                }
                else
                {
                    IconImage.gameObject.SetActive(false);
                }
            }

            Button.OnClickAsObservable()
                .Subscribe(_ => _onClick.OnNext(_config))
                .AddTo(this);

            Button.OnPointerEnterAsObservable()
                .Subscribe(_ => _onHover.OnNext(true))
                .AddTo(this);

            Button.OnPointerExitAsObservable()
                .Subscribe(_ => _onHover.OnNext(false))
                .AddTo(this);
        }
    }
}