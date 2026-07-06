using UnityEngine;

namespace Game.Views
{
    public class FloorCellView : MonoBehaviour
    {
        [field: SerializeField] public int Index { get; set; }

        public void SetVisible(bool isVisible) => gameObject.SetActive(isVisible);
    }
}