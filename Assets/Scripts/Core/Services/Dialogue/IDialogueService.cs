using System;
using UniRx;
using UnityEngine;

namespace Game.Services.Dialogue
{
    public interface IDialogueService
    {
        IReadOnlyReactiveProperty<string> CurrentReplica { get; }
        IReadOnlyReactiveProperty<AudioClip> CurrentAudioClip { get; }
        IReadOnlyReactiveProperty<bool> IsCurrentReplicaFullyRevealed { get; }
        IObservable<Unit> OnDialogueCompleted { get; }
        void StartDialogue(Game.Data.DialogueReplica[] replicas);
        void NotifyRevealCompleted();
        void AdvanceOrSkip();
    }
}