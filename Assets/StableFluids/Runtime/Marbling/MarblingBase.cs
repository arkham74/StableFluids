using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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
		private readonly List<MarblingCommand> colorList = new();
		private readonly List<MarblingCommand> forceList = new();

		public MarblingInputHandler Input => _input;

		private void Start()
		{
			_mpb = new MaterialPropertyBlock();
			_input = new MarblingInputHandler(_colorInjection);
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

			foreach (MarblingCommand command in colorList)
			{
				BlitToColorInternal(cmd, command);
			}

			foreach (MarblingCommand command in forceList)
			{
				BlitToForceFieldInternal(cmd, command);
			}

			colorList.Clear();
			forceList.Clear();
			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}

		internal void BlitToColor(Vector2 position, Color color, float falloff)
		{
			colorList.Add(new(position, color, falloff));
		}

		internal void BlitToForceField(Vector2 position, Vector2 force, float falloff)
		{
			forceList.Add(new(position, force, falloff));
		}

		private void BlitToColorInternal(CommandBuffer cmd, MarblingCommand command)
		{
			_mpb.SetColor("_Color", command.color);
			_mpb.SetVector("_Origin", command.position);
			_mpb.SetFloat("_Falloff", command.falloff);

			RTUtil.Blit(cmd, null, _colorInjection, _material, 0, _mpb);
		}

		private void BlitToForceFieldInternal(CommandBuffer cmd, MarblingCommand command)
		{
			_mpb.SetVector("_Origin", command.position);
			_mpb.SetFloat("_Falloff", command.falloff);
			_mpb.SetVector("_Force", command.force);

			RTUtil.Blit(cmd, null, _forceField, _material, 1, _mpb);
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