using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Game.Data;
using Game.Services.Audio;
using Game.Services.Input;

namespace Game.Views.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class AudioSpectrumView : Graphic
    {
        [Inject] private readonly IAudioSpectrumService _spectrumService;
        [Inject] private readonly IInputContextService _inputContextService;
        [Inject] private readonly AudioSpectrumConfig _config;

        private float[] _currentData;
        private bool _isVisible;

        private void Start()
        {
            _spectrumService.OnSpectrumUpdated
                .Subscribe(OnDataReceived)
                .AddTo(this);

            _inputContextService.CurrentContext
                .Subscribe(OnContextChanged)
                .AddTo(this);
        }

        private void OnContextChanged(InputContext context)
        {
            bool shouldBeVisible = context == InputContext.Paused;
            if (_isVisible != shouldBeVisible)
            {
                _isVisible = shouldBeVisible;
                SetVerticesDirty();
            }
        }

        private void OnDataReceived(float[] data)
        {
            _currentData = data;
            if (_isVisible)
                SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (!_isVisible || _currentData is null || _currentData.Length == 0)
                return;

            RectTransform rect = rectTransform;
            Vector2 size = rect.rect.size;
            int barCount = _config.BarCount;

            float totalSpacing = _config.Spacing * (barCount - 1);
            float barWidth = (size.x - totalSpacing) / barCount;

            if (barWidth <= 0f) return;

            UIVertex vert = UIVertex.simpleVert;
            vert.color = _config.BarColor;

            float startX = -size.x / 2f;

            for (int i = 0; i < barCount; i++)
            {
                float rawValue = i < _currentData.Length ? _currentData[i] : 0f;
                float normalized = Mathf.Clamp01(rawValue / _config.MaxInputValue);
                float barHeight = Mathf.Lerp(_config.MinHeight, _config.MaxHeight, normalized);

                float x = startX + i * (barWidth + _config.Spacing);
                float y = -size.y / 2f;

                Vector2 bottomLeft = new(x, y);
                Vector2 bottomRight = new(x + barWidth, y);
                Vector2 topLeft = new(x, y + barHeight);
                Vector2 topRight = new(x + barWidth, y + barHeight);

                vert.position = new Vector3(bottomLeft.x, bottomLeft.y, 0f);
                vh.AddVert(vert);

                vert.position = new Vector3(bottomRight.x, bottomRight.y, 0f);
                vh.AddVert(vert);

                vert.position = new Vector3(topRight.x, topRight.y, 0f);
                vh.AddVert(vert);

                vert.position = new Vector3(topLeft.x, topLeft.y, 0f);
                vh.AddVert(vert);

                int startIndex = i * 4;
                vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
                vh.AddTriangle(startIndex, startIndex + 2, startIndex + 3);
            }
        }
    }
}