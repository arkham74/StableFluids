using UnityEngine;

namespace StableFluids.Marbling
{
	public sealed class MarblingFluidSimulator : MonoBehaviour
	{
		[field: SerializeField] public float Viscosity { get; set; } = 1e-6f;
		[SerializeField] private RenderTexture _canvas = null;
		[SerializeField] private RenderTexture _velocityField = null;
		[SerializeField] private RenderTexture _forceField = null;
		[SerializeField, HideInInspector] private Shader _kernelShader = null;
		private FluidSimulation _simulation;

		private void Start()
		{
			_simulation = new FluidSimulation(_velocityField, _kernelShader);
			_simulation.ClearVelocityField();
			Graphics.Blit(Texture2D.blackTexture, _canvas);
		}

		private void OnDestroy()
		{
			_simulation.Dispose();
			_simulation = null;
		}

		private void Update()
		{
			_simulation.Viscosity = Viscosity;
			_simulation.PreStep();
			_simulation.ApplyForceField(_forceField);
			_simulation.PostStep();
		}
	}
}