using System;
using UniRx;
using Zenject;
using Game.Services.Input;

namespace Game.Services.Dialogue
{
    public class DialogueService : IDialogueService, IInitializable, IDisposable
    {
        public IReadOnlyReactiveProperty<string> CurrentReplica => _currentReplica;
        public IReadOnlyReactiveProperty<bool> IsCurrentReplicaFullyRevealed => _isCurrentReplicaFullyRevealed;
        public IObservable<Unit> OnDialogueCompleted => _onDialogueCompleted;

        private readonly ReactiveProperty<string> _currentReplica = new();
        private readonly ReactiveProperty<bool> _isCurrentReplicaFullyRevealed = new(true);
        private readonly Subject<Unit> _onDialogueCompleted = new();
        private readonly CompositeDisposable _disposables = new();
        private readonly IInputContextService _contextService;

        private string[] _replicas;
        private int _currentIndex;

        public DialogueService(IInputContextService contextService)
        {
            _contextService = contextService;
        }

        public void Initialize() { }

        public void StartDialogue(string[] replicas)
        {
            UnityEngine.Debug.Log($"[DialogueService] Started with {replicas.Length} replicas.");
            _replicas = replicas;
            _currentIndex = 0;
            _isCurrentReplicaFullyRevealed.Value = false;
            _currentReplica.Value = _replicas[_currentIndex];
        }

        public void NotifyRevealCompleted()
        {
            _isCurrentReplicaFullyRevealed.Value = true;
        }

        public void AdvanceOrSkip()
        {
            if (_contextService.CurrentContext.Value != InputContext.Dialogue) return;
            if (_replicas is null || _replicas.Length == 0) return;
            if (!_isCurrentReplicaFullyRevealed.Value) return;

            _currentIndex++;
            if (_currentIndex >= _replicas.Length)
            {
                _currentReplica.Value = string.Empty;
                _onDialogueCompleted.OnNext(Unit.Default);
                _replicas = null;
                return;
            }

            _isCurrentReplicaFullyRevealed.Value = false;
            _currentReplica.Value = _replicas[_currentIndex];
        }

        public void Dispose() => _disposables?.Dispose();
    }
}