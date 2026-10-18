using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.GraphicsBuffer;

namespace StableFluids.Marbling
{
	public sealed class MarblingBase : MonoBehaviour
	{
		[SerializeField] private RenderTexture _canvas;
		[SerializeField] private RenderTexture _velocityField;
		[SerializeField] private RenderTexture _colorInjection;
		[SerializeField] private RenderTexture _forceField;
		[SerializeField] private Material _material;

		private MarblingInputHandler _input;
		private MaterialPropertyBlock _mpb;

		private GraphicsBuffer _colorsBuffer;
		private GraphicsBuffer _forceBuffer;

		private readonly List<MarblingCommand> colorList = new();
		private readonly List<MarblingCommand> forceList = new();

		public MarblingInputHandler Input => _input;

		private void Start()
		{
			_mpb = new MaterialPropertyBlock();
			_input = new MarblingInputHandler(_colorInjection);
			_colorsBuffer = new GraphicsBuffer(Target.Structured, UsageFlags.LockBufferForWrite, 1000, Marshal.SizeOf<MarblingCommand>());
			_forceBuffer = new GraphicsBuffer(Target.Structured, UsageFlags.LockBufferForWrite, 1000, Marshal.SizeOf<MarblingCommand>());
			_material.SetFloat("_Aspect", (float)_forceField.width / _forceField.height);

			Graphics.Blit(Texture2D.blackTexture, _colorInjection);
			Graphics.Blit(Texture2D.blackTexture, _forceField);
			Graphics.Blit(Texture2D.blackTexture, _canvas);
			Graphics.Blit(Texture2D.blackTexture, _velocityField);

			RenderPipelineManager.beginContextRendering += OnBeginContextRendering;
		}

		private void OnDestroy()
		{
			RenderPipelineManager.beginContextRendering -= OnBeginContextRendering;

			_colorsBuffer?.Dispose();
			_colorsBuffer = null;

			_forceBuffer?.Dispose();
			_forceBuffer = null;
		}

		private void Update()
		{
			_input.Update();
		}

		private void OnBeginContextRendering(ScriptableRenderContext context, List<Camera> list)
		{
			CommandBuffer cmd = CommandBufferPool.Get("Marbling Pass");
			_mpb.Clear();

			ClearForceField(cmd);
			ClearColorInjection(cmd);

			BlitColorInjection(cmd);
			BlitForceField(cmd);

			colorList.Clear();
			forceList.Clear();
			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}

		private void BlitColorInjection(CommandBuffer cmd)
		{
			// _commandsBuffer.SetData(colorList);
			_mpb.SetBuffer("_CommandsBuffer", _colorsBuffer);
			_mpb.SetInteger("_CommandsCount", colorList.Count);
			cmd.SetBufferData(_colorsBuffer, colorList);
			RTUtil.Blit(cmd, null, _colorInjection, _material, 0, _mpb);
		}

		private void BlitForceField(CommandBuffer cmd)
		{
			// _commandsBuffer.SetData(forceList);
			_mpb.SetBuffer("_CommandsBuffer", _forceBuffer);
			_mpb.SetInteger("_CommandsCount", forceList.Count);
			cmd.SetBufferData(_forceBuffer, forceList);
			RTUtil.Blit(cmd, null, _forceField, _material, 1, _mpb);
		}

		internal void BlitToColor(Vector2 position, Color color, float falloff)
		{
			colorList.Add(new(position, color, falloff));
		}

		internal void BlitToForceField(Vector2 position, Vector2 force, float falloff)
		{
			forceList.Add(new(position, force, falloff));
		}

		internal void ClearForceField(CommandBuffer cmd)
		{
			cmd.Blit(Texture2D.blackTexture, _forceField);
		}

		internal void ClearColorInjection(CommandBuffer cmd)
		{
			cmd.Blit(Texture2D.blackTexture, _colorInjection);
		}
	}
}