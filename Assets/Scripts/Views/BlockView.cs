using DG.Tweening;
using UnityEngine;

namespace Game.Views
{
    public class BlockView : MonoBehaviour
    {
        private static readonly Vector3 Offset = new(0.5f, 0.5f, 0.5f);

        public void SetPosition(Vector3Int cell) => transform.position = cell + Offset;

        public void SetScale(Vector3 scale) => transform.localScale = scale;

        public Tween AnimateScale(Vector3 targetScale, float duration, Ease ease) =>
            transform.DOScale(targetScale, duration).SetEase(ease).SetAutoKill(true);
    }
}