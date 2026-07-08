using UnityEngine;
using Zenject;

namespace Game.Views
{
    public class DevModeView : MonoBehaviour
    {
        [Inject]
        private void Construct([Inject(Id = "IsDeveloperMode")] bool isDeveloperMode) =>
            gameObject.SetActive(isDeveloperMode);
    }
}