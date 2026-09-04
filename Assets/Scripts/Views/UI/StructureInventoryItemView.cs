using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Views.UI
{
    public class StructureInventoryItemView : MonoBehaviour
    {
        [field: SerializeField] public Button Button { get; private set; }
        [field: SerializeField] public TextMeshProUGUI CountText { get; private set; }
        [field: SerializeField] public TextMeshProUGUI NameText { get; private set; }
        [field: SerializeField] public Image IconImage { get; private set; }

        public void SetIcon(Sprite icon)
        {
            if (IconImage is null) return;

            if (icon is null)
            {
                IconImage.gameObject.SetActive(false);
                return;
            }

            IconImage.gameObject.SetActive(true);
            IconImage.sprite = icon;
        }
    }
}