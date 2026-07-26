using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Game.Data;

namespace Game.Views.Menu
{
    public class CategoryButtonView : MonoBehaviour
    {
        [field: SerializeField] private TextMeshProUGUI Label { get; set; }
        [field: SerializeField] private Button Button { get; set; }

        private readonly Subject<CategoryConfig> _onClick = new();
        public IObservable<CategoryConfig> OnClick => _onClick;

        private CategoryConfig _config;

        public void Initialize(CategoryConfig config)
        {
            _config = config;
            if (Label is not null) Label.text = config.Title;

            Button.OnClickAsObservable()
                .Subscribe(_ => _onClick.OnNext(_config))
                .AddTo(this);
        }
    }
}