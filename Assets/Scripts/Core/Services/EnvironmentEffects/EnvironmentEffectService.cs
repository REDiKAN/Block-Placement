using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Game.Views.Effects;

namespace Game.Services.EnvironmentEffects
{
    public class EnvironmentEffectService : IEnvironmentEffectService, IInitializable
    {
        private readonly IEnumerable<IEffectView> _effectViews;

        public EnvironmentEffectService(IEnumerable<IEffectView> effectViews)
        {
            _effectViews = effectViews;
        }

        public void Initialize() => Regenerate();

        public void Regenerate()
        {
            foreach (var view in _effectViews)
            {
                if (view is null) continue;
                var isSelected = Random.value <= view.Probability;
                if (isSelected && !view.IsVisible)
                    view.Show();
                else if (!isSelected && view.IsVisible)
                    view.Hide();
            }
        }

        public bool IsEffectVisible<T>() where T : class, IEffectView
        {
            foreach (var view in _effectViews)
            {
                if (view is T tView && tView.IsVisible)
                    return true;
            }
            return false;
        }
    }
}