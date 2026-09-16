using System;
using UniRx;
using Zenject;
using Game.Data;
using Game.Services.Input;
using UnityEngine;

namespace Game.Services.Dialogue
{
    public class DialogueService : IDialogueService, IInitializable, IDisposable
    {
        public IReadOnlyReactiveProperty<string> CurrentReplica => _currentReplica;
        public IReadOnlyReactiveProperty<AudioClip> CurrentAudioClip => _currentAudioClip;
        public IReadOnlyReactiveProperty<bool> IsCurrentReplicaFullyRevealed => _isCurrentReplicaFullyRevealed;
        public IObservable<Unit> OnDialogueCompleted => _onDialogueCompleted;

        private readonly ReactiveProperty<string> _currentReplica = new();
        private readonly ReactiveProperty<AudioClip> _currentAudioClip = new();
        private readonly ReactiveProperty<bool> _isCurrentReplicaFullyRevealed = new(true);
        private readonly Subject<Unit> _onDialogueCompleted = new();
        private readonly CompositeDisposable _disposables = new();
        private readonly IInputContextService _contextService;

        private DialogueReplica[] _replicas;
        private int _currentIndex;

        public DialogueService(IInputContextService contextService)
        {
            _contextService = contextService;
        }

        public void Initialize() { }

        public void StartDialogue(DialogueReplica[] replicas)
        {
            UnityEngine.Debug.Log($"[DialogueService] Started with {replicas.Length} replicas.");
            _replicas = replicas;
            _currentIndex = 0;
            _isCurrentReplicaFullyRevealed.Value = false;
            _currentReplica.Value = _replicas[_currentIndex].Text;
            _currentAudioClip.Value = _replicas[_currentIndex].Audio;
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
                _currentAudioClip.Value = null;
                _onDialogueCompleted.OnNext(Unit.Default);
                _replicas = null;
                return;
            }

            _isCurrentReplicaFullyRevealed.Value = false;
            _currentReplica.Value = _replicas[_currentIndex].Text;
            _currentAudioClip.Value = _replicas[_currentIndex].Audio;
        }

        public void Dispose() => _disposables?.Dispose();
    }
}