using System;
using UniRx;

namespace Game.Services.Dialogue
{
    public interface IDialogueService
    {
        IReadOnlyReactiveProperty<string> CurrentReplica { get; }
        IReadOnlyReactiveProperty<bool> IsCurrentReplicaFullyRevealed { get; }
        IObservable<Unit> OnDialogueCompleted { get; }
        void StartDialogue(string[] replicas);
        void NotifyRevealCompleted();
        void AdvanceOrSkip();
    }
}