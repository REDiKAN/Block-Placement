using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Views.UI
{
    public class StructureInventoryItemView : MonoBehaviour
    {
        [field: SerializeField] public Button Button { get; private set; }
        [field: SerializeField] public TextMeshProUGUI CountText { get; private set; }
    }
}