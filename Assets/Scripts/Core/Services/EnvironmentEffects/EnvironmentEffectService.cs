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

        public void Initialize()
        {
            foreach (var view in _effectViews)
            {
                if (view is null) continue;

                var isSelected = UnityEngine.Random.value <= view.Probability;

                if (isSelected)
                    view.Show();
                else
                    view.Hide();
            }
        }
    }
}