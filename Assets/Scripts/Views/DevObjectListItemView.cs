using TMPro;
using UnityEngine;
using Game.Services.Registry;

namespace Game.Views
{
    public class DevObjectListItemView : MonoBehaviour
    {
        [field: SerializeField] private TextMeshProUGUI _label;

        public void SetData(PlacedObjectData data) =>
            _label.text = $"[{data.Type}] {data.Identifier} ({data.Position.x}, {data.Position.y}, {data.Position.z})";
    }
}