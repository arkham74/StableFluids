using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace StableFluids.Marbling
{
	public sealed class MarblingFluidSimulator : MonoBehaviour
	{
		[field: SerializeField] public float Viscosity { get; set; } = 1e-6f;

		[SerializeField] private RenderTexture _velocityField;
		[SerializeField] private RenderTexture _forceField;
		[SerializeField] private Material _kernelMaterial;

		private FluidSimulation _simulation;

		private void Start()
		{
			_simulation = new FluidSimulation(_velocityField, _kernelMaterial);
			_simulation.ClearVelocityField();
			RenderPipelineManager.beginContextRendering += OnBeginContextRendering;
		}

		private void OnDestroy()
		{
			RenderPipelineManager.beginContextRendering -= OnBeginContextRendering;
			_simulation.Dispose();
			_simulation = null;
		}

		private void OnBeginContextRendering(ScriptableRenderContext context, List<Camera> list)
		{
			CommandBuffer cmd = CommandBufferPool.Get("Simulation Pass");

			_simulation.Viscosity = Viscosity;
			_simulation.PreStep(cmd);
			_simulation.ApplyForceField(cmd, _forceField);
			_simulation.PostStep(cmd);

			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}
	}
}