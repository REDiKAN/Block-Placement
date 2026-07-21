using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Game.Core;
using Game.Services.Input;
using Game.Services.Progression;
using UnityEngine.SceneManagement;

namespace Game.Views.UI
{
    public class PauseMenuView : MonoBehaviour
    {
        [field: SerializeField] private CanvasGroup CanvasGroup { get; set; }
        [field: SerializeField] private TextMeshProUGUI LevelLabel { get; set; }
        [field: SerializeField] private Button ReturnToMenuButton { get; set; }
        [field: SerializeField] private Button RestartButton { get; set; }
        [field: SerializeField] private Vector3 PausedCameraPosition { get; set; }
        [field: SerializeField] private float PausedCameraSize { get; set; } = 5f;

        [Inject] private Camera _gameCamera;
        [Inject] private IInputContextService _contextService;
        [Inject] private ILevelProgressionService _progressionService;

        private const float CameraMoveDuration = 0.6f;
        private const Ease CameraMoveEase = Ease.OutCubic;
        private const float FadeDuration = 0.3f;

        private Vector3 _originalCameraPosition;
        private float _originalCameraSize;
        private bool _isOrthographic;
        private bool _isPaused;
        private bool _isAnimating;

        private Sequence _cameraSequence;
        private Sequence _uiSequence;

        private void Start()
        {
            if (_gameCamera is not null)
            {
                _originalCameraPosition = _gameCamera.transform.position;
                _isOrthographic = _gameCamera.orthographic;
                _originalCameraSize = _isOrthographic ? _gameCamera.orthographicSize : _gameCamera.fieldOfView;
            }

            if (CanvasGroup is not null)
            {
                CanvasGroup.alpha = 0f;
                CanvasGroup.blocksRaycasts = false;
            }

            if (LevelLabel is not null)
                LevelLabel.text = $"Level {LevelContext.SelectedLevelId + 1}";

            if (ReturnToMenuButton is not null)
                ReturnToMenuButton.onClick.AddListener(ReturnToMenu);

            if (RestartButton is not null)
                RestartButton.onClick.AddListener(HandleRestart);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && !_isAnimating)
            {
                if (_isPaused)
                    Resume();
                else
                    Pause();
            }
        }

        private void Pause()
        {
            _isAnimating = true;
            _isPaused = true;

            if (_gameCamera is not null)
            {
                _originalCameraPosition = _gameCamera.transform.position;
                _originalCameraSize = _isOrthographic ? _gameCamera.orthographicSize : _gameCamera.fieldOfView;
            }

            _contextService.SetContext(InputContext.Paused);

            _cameraSequence?.Kill();
            _cameraSequence = DOTween.Sequence();

            if (_gameCamera is not null)
            {
                _cameraSequence.Join(_gameCamera.transform.DOMove(PausedCameraPosition, CameraMoveDuration).SetEase(CameraMoveEase));

                var sizeTween = _isOrthographic
                    ? _gameCamera.DOOrthoSize(PausedCameraSize, CameraMoveDuration)
                    : _gameCamera.DOFieldOfView(PausedCameraSize, CameraMoveDuration);

                _cameraSequence.Join(sizeTween.SetEase(CameraMoveEase));
            }

            _cameraSequence.OnComplete(() => _isAnimating = false);

            if (CanvasGroup is not null)
            {
                _uiSequence?.Kill();
                _uiSequence = DOTween.Sequence()
                    .Append(CanvasGroup.DOFade(1f, FadeDuration))
                    .OnComplete(() => CanvasGroup.blocksRaycasts = true);
            }
        }

        private void Resume()
        {
            _isAnimating = true;
            _isPaused = false;

            _contextService.SetContext(InputContext.None);

            if (CanvasGroup is not null)
            {
                CanvasGroup.blocksRaycasts = false;
                _uiSequence?.Kill();
                _uiSequence = DOTween.Sequence()
                    .Append(CanvasGroup.DOFade(0f, FadeDuration));
            }

            _cameraSequence?.Kill();
            _cameraSequence = DOTween.Sequence();

            if (_gameCamera is not null)
            {
                _cameraSequence.Join(_gameCamera.transform.DOMove(_originalCameraPosition, CameraMoveDuration).SetEase(CameraMoveEase));

                var sizeTween = _isOrthographic
                    ? _gameCamera.DOOrthoSize(_originalCameraSize, CameraMoveDuration)
                    : _gameCamera.DOFieldOfView(_originalCameraSize, CameraMoveDuration);

                _cameraSequence.Join(sizeTween.SetEase(CameraMoveEase));
            }

            _cameraSequence.OnComplete(() => _isAnimating = false);
        }

        private void HandleRestart()
        {
            Time.timeScale = 1f;
            _progressionService.RequestRestart();
        }

        private void ReturnToMenu()
        {
            Time.timeScale = 1f;
            LevelContext.SelectedLevelId = 0;
            SceneManager.LoadScene("MenuScene");
        }

        private void OnDestroy()
        {
            _cameraSequence?.Kill();
            _uiSequence?.Kill();
        }
    }
}