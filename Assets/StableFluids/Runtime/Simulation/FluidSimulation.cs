using System;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace StableFluids
{
	public sealed class FluidSimulation : IDisposable
	{
		public float Viscosity { get; set; } = 1e-6f;

		private readonly Vector2Int _resolution;
		private RenderTexture _v1, _v2, _v3, _p1, _p2, _divW;
		private MaterialPropertyBlock _mpb;
		private Material _mat;

		public FluidSimulation(RenderTexture velocityField, Material kernelsMaterial)
		{
			_mpb = new MaterialPropertyBlock();
			_v1 = velocityField;
			_resolution = new Vector2Int(_v1.width, _v1.height);

			_v2 = RTUtil.AllocateRGHalf(_resolution);
			_v3 = RTUtil.AllocateRGHalf(_resolution);
			_p1 = RTUtil.AllocateRHalf(_resolution);
			_p2 = RTUtil.AllocateRHalf(_resolution);
			_divW = RTUtil.AllocateRHalf(_resolution);

			_mat = kernelsMaterial;
			_mat.SetInt(ShaderIDs.TexWidth, _resolution.x);
			_mat.SetInt(ShaderIDs.TexHeight, _resolution.y);
		}

		public void Dispose()
		{
			Object.Destroy(_v2);
			Object.Destroy(_v3);
			Object.Destroy(_p1);
			Object.Destroy(_p2);
			Object.Destroy(_divW);
			_mpb.Clear();
		}

		public void ClearVelocityField()
		{
			Graphics.Blit(Texture2D.blackTexture, _v1);
		}

		public void ApplyForceField(CommandBuffer cmd, RenderTexture forceField)
		{
			_mpb.SetTexture(ShaderIDs.ForceField, forceField);
			RTUtil.Blit(cmd, _v2, _v3, _mat, 0, _mpb);
			(_v2, _v3) = (_v3, _v2);
		}

		public void PreStep(CommandBuffer cmd)
		{
			float dt = Time.deltaTime;

			// Advection U -> W (_v1 -> v2)
			RTUtil.Blit(cmd, _v1, _v2, _mat, 1, _mpb);

			// Diffusion via Jacobi on vector field (pass 4)
			float dx = 1.0f / _resolution.x;
			float dif_alpha = dx * dx / (Mathf.Max(Viscosity, 1e-12f) * Mathf.Max(dt, 1e-12f));
			float beta = 4 + dif_alpha;

			cmd.CopyTexture(_v2, _v1); // B2_in = _v1

			// Iterate: X2_in/out over v2 <-> v3
			for (int i = 0; i < 20; i++)
			{
				_mpb.SetTexture(ShaderIDs.X, _v2);
				_mpb.SetTexture(ShaderIDs.B, _v1);
				_mpb.SetFloat(ShaderIDs.Alpha, dif_alpha);
				_mpb.SetFloat(ShaderIDs.Beta, beta);
				RTUtil.Blit(cmd, _v2, _v3, _mat, 4, _mpb);
				(_v2, _v3) = (_v3, _v2);
			}
		}

		public void PostStep(CommandBuffer cmd)
		{
			// PSetup: compute divergence of W_in (v2) to divW and clear p1 (pass 2)
			_mpb.SetTexture(ShaderIDs.MainTex, _v2);
			RTUtil.Blit(cmd, _v2, _divW, _mat, 2, _mpb);
			cmd.Blit(Texture2D.blackTexture, _p1);

			// Jacobi on scalar field for pressure
			float dx = 1.0f / _resolution.x;
			_mpb.SetFloat(ShaderIDs.Alpha, -dx * dx);
			_mpb.SetFloat(ShaderIDs.Beta, 4);
			for (int i = 0; i < 20; i++)
			{
				_mpb.SetTexture(ShaderIDs.X, _p1);
				_mpb.SetTexture(ShaderIDs.B, _divW);
				RTUtil.Blit(cmd, _p1, _p2, _mat, 3, _mpb);
				(_p1, _p2) = (_p2, _p1);
			}

			// PFinish (pass 5): subtract pressure gradient to get divergence-free field U_out (_v1)
			_mpb.SetTexture(ShaderIDs.W, _v2);
			_mpb.SetTexture(ShaderIDs.P, _p1);
			RTUtil.Blit(cmd, _v2, _v1, _mat, 5, _mpb);
		}
	}
}