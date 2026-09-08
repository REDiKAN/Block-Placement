using TMPro;
using UniRx;
using UnityEngine;
using Zenject;
using Game.Services.Dialogue;

namespace Game.Views.UI
{
    public class DialogueView : MonoBehaviour
    {
        [field: SerializeField] private TextMeshProUGUI DialogueText { get; set; }
        [field: SerializeField] private GameObject RootObject { get; set; }

        [Inject] private readonly IDialogueService _dialogueService;

        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            if (RootObject is not null)
                RootObject.SetActive(false);

            _dialogueService.CurrentReplica
                .Subscribe(UpdateText)
                .AddTo(_disposables);

            _dialogueService.OnDialogueCompleted
                .Subscribe(_ => Hide())
                .AddTo(_disposables);
        }

        private void UpdateText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                Hide();
                return;
            }

            Show();
            if (DialogueText is not null)
                DialogueText.text = text;
        }

        private void Show()
        {
            if (RootObject is not null)
                RootObject.SetActive(true);
        }

        private void Hide()
        {
            if (RootObject is not null)
                RootObject.SetActive(false);
        }

        private void OnDestroy() => _disposables?.Dispose();
    }
}