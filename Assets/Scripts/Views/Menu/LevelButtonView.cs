using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using Game.Data;

namespace Game.Views.Menu
{
    public class LevelButtonView : MonoBehaviour
    {
        [field: SerializeField] private TextMeshProUGUI Label { get; set; }
        [field: SerializeField] private Button Button { get; set; }

        private readonly Subject<LevelConfig> _onClick = new();
        private readonly Subject<LevelConfig> _onHover = new();
        private readonly Subject<Unit> _onHoverExit = new();

        public IObservable<LevelConfig> OnClick => _onClick;
        public IObservable<LevelConfig> OnHover => _onHover;
        public IObservable<Unit> OnHoverExit => _onHoverExit;

        private LevelConfig _config;

        public void Initialize(LevelConfig config, int index)
        {
            _config = config;
            if (Label is not null) Label.text = $"LVL {index + 1}";

            Button.OnClickAsObservable()
                .Subscribe(_ => _onClick.OnNext(_config))
                .AddTo(this);

            Button.OnPointerEnterAsObservable()
                .Subscribe(_ => _onHover.OnNext(_config))
                .AddTo(this);

            Button.OnPointerExitAsObservable()
                .Subscribe(_ => _onHoverExit.OnNext(Unit.Default))
                .AddTo(this);
        }
    }
}