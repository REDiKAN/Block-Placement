using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Game.Services.Dev;
using Game.Services.Input;

namespace Game.Views
{
    public class LevelGeneratorView : MonoBehaviour
    {
        [field: SerializeField] private Slider DifficultySlider { get; set; }
        [field: SerializeField] private TMP_Dropdown StrategyDropdown { get; set; }
        [field: SerializeField] private Button GenerateButton { get; set; }
        [field: SerializeField] private Button ValidateButton { get; set; }
        [field: SerializeField] private LevelGeneratorProgressView ProgressView { get; set; }

        [Inject] private ILevelGeneratorService _generatorService;
        [Inject] private IInputContextService _contextService;

        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            if (StrategyDropdown is not null)
            {
                StrategyDropdown.ClearOptions();
                StrategyDropdown.AddOptions(new List<string> { "Additive", "Subtractive" });
            }

            if (GenerateButton is not null)
            {
                GenerateButton.OnClickAsObservable()
                    .Subscribe(_ => HandleGenerate())
                    .AddTo(_disposables);
            }

            if (ValidateButton is not null)
            {
                ValidateButton.OnClickAsObservable()
                    .Subscribe(_ => HandleValidate())
                    .AddTo(_disposables);
            }

            if (_generatorService is not null)
            {
                _generatorService.OnProgress
                    .Subscribe(data => ProgressView?.UpdateProgress(data.Current, data.Total))
                    .AddTo(_disposables);

                _generatorService.OnGenerationCompleted
                    .Subscribe(_ => ProgressView?.Hide())
                    .AddTo(_disposables);

                _generatorService.OnValidationCompleted
                    .Subscribe(_ => ProgressView?.Hide())
                    .AddTo(_disposables);
            }
        }

        private void HandleGenerate()
        {
            var difficulty = DifficultySlider is not null ? (int)DifficultySlider.value : 5;
            var strategy = StrategyDropdown is not null ? StrategyDropdown.value : 0;

            ProgressView?.Show();
            _contextService.SetContext(InputContext.Generating);
            _generatorService.Generate(difficulty, strategy);
        }

        private void HandleValidate()
        {
            ProgressView?.Show();
            _contextService.SetContext(InputContext.Generating);
            _generatorService.ValidateSolvability();
        }

        private void OnDestroy() => _disposables?.Dispose();
    }
}