using UnityEngine;

namespace Game.Views.Effects
{
    public class EnvironmentEffectView : MonoBehaviour, IEffectView
    {
        [field: SerializeField, Range(0f, 1f)] public float Probability { get; private set; }

        public void Show() => gameObject.SetActive(true);

        public void Hide() => gameObject.SetActive(false);
    }
}