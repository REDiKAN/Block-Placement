using UniRx;
using UnityEngine;
using Zenject;
using Game.Data;
using Game.Services.Water;

namespace Game.Views
{
    public class WaterShaderView : MonoBehaviour
    {
        [field: SerializeField] private Material WaterMaterial { get; set; }

        [Inject] private IWaterShaderService _waterShaderService;

        private static readonly int WaterDepthId = Shader.PropertyToID("_WaterDepth");
        private static readonly int ShallowWaterColorId = Shader.PropertyToID("_ShallowWaterColor");
        private static readonly int DeepWaterId = Shader.PropertyToID("_DeepWater");
        private static readonly int RefractionSpeedId = Shader.PropertyToID("_RefractionSpeed");
        private static readonly int RefractionScaleId = Shader.PropertyToID("_RefractionScale");
        private static readonly int RefractionStrengthId = Shader.PropertyToID("_RefractionStrength");
        private static readonly int FoamAmountId = Shader.PropertyToID("_FoamAmount");
        private static readonly int FoamCutOffId = Shader.PropertyToID("_FoamCutOff");
        private static readonly int FoamSpeedId = Shader.PropertyToID("_FoamSpeed");
        private static readonly int FoamScaleId = Shader.PropertyToID("_FoamScale");
        private static readonly int FoamColorId = Shader.PropertyToID("_FoamColor");
        private static readonly int Wave1DirId = Shader.PropertyToID("Wave1Dir");
        private static readonly int Wave1AmpId = Shader.PropertyToID("Wave1Amp");
        private static readonly int Wave1LenId = Shader.PropertyToID("Wave1Len");
        private static readonly int Wave1SteepId = Shader.PropertyToID("Wave1Steep");
        private static readonly int Wave2DirId = Shader.PropertyToID("Wave2Dir");
        private static readonly int Wave2AmpId = Shader.PropertyToID("Wave2Amp");
        private static readonly int Wave2LenId = Shader.PropertyToID("Wave2Len");
        private static readonly int Wave2SteepId = Shader.PropertyToID("Wave2Steep");
        private static readonly int WaveSpeedId = Shader.PropertyToID("WaveSpeed");

        private Vector4 _waveVector;

        private void Start()
        {
            if (WaterMaterial is null)
            {
                Debug.LogError("[WaterShaderView] WaterMaterial is not assigned.");
                return;
            }

            _waterShaderService.CurrentParameters
                .Subscribe(ApplyParameters)
                .AddTo(this);
        }

        private void ApplyParameters(WaterShaderParameters parameters)
        {
            if (_waterShaderService.CurrentConfig.Value is null || WaterMaterial is null)
                return;

            WaterMaterial.SetFloat(WaterDepthId, parameters.WaterDepth);
            WaterMaterial.SetColor(ShallowWaterColorId, parameters.ShallowWaterColor);
            WaterMaterial.SetColor(DeepWaterId, parameters.DeepWater);
            WaterMaterial.SetFloat(RefractionSpeedId, parameters.RefractionSpeed);
            WaterMaterial.SetFloat(RefractionScaleId, parameters.RefractionScale);
            WaterMaterial.SetFloat(RefractionStrengthId, parameters.RefractionStrength);
            WaterMaterial.SetFloat(FoamAmountId, parameters.FoamAmount);
            WaterMaterial.SetFloat(FoamCutOffId, parameters.FoamCutOff);
            WaterMaterial.SetFloat(FoamSpeedId, parameters.FoamSpeed);
            WaterMaterial.SetFloat(FoamScaleId, parameters.FoamScale);
            WaterMaterial.SetColor(FoamColorId, parameters.FoamColor);

            _waveVector.Set(parameters.Wave1Dir.x, parameters.Wave1Dir.y, 0f, 0f);
            WaterMaterial.SetVector(Wave1DirId, _waveVector);
            WaterMaterial.SetFloat(Wave1AmpId, parameters.Wave1Amp);
            WaterMaterial.SetFloat(Wave1LenId, parameters.Wave1Len);
            WaterMaterial.SetFloat(Wave1SteepId, parameters.Wave1Steep);

            _waveVector.Set(parameters.Wave2Dir.x, parameters.Wave2Dir.y, 0f, 0f);
            WaterMaterial.SetVector(Wave2DirId, _waveVector);
            WaterMaterial.SetFloat(Wave2AmpId, parameters.Wave2Amp);
            WaterMaterial.SetFloat(Wave2LenId, parameters.Wave2Len);
            WaterMaterial.SetFloat(Wave2SteepId, parameters.Wave2Steep);

            WaterMaterial.SetFloat(WaveSpeedId, parameters.WaveSpeed);
        }
    }
}