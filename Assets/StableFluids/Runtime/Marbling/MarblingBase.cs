using System;
using System.Collections.Generic;
using UnityEngine;

namespace StableFluids.Marbling
{
	public struct MarblingCommand
	{
		public Color color;
		public Vector2 position;
		public Vector2 force;
		public float falloff;

		public MarblingCommand(Vector2 position, Color color, float falloff)
		{
			this.position = position;
			this.color = color;
			this.falloff = falloff;
			this.force = Vector2.zero;
		}

		public MarblingCommand(Vector2 position, Vector2 force, float falloff)
		{
			this.position = position;
			this.force = force;
			this.falloff = falloff;
			this.color = Color.clear;
		}
	}

	public sealed class MarblingBase : MonoBehaviour
	{
		[SerializeField] private RenderTexture _colorInjection;
		[SerializeField] private RenderTexture _forceField;
		[SerializeField] private Material _material;

		private MarblingInputHandler _input;
		private readonly List<MarblingCommand> colorList = new();
		private readonly List<MarblingCommand> forceList = new();

		public MarblingInputHandler Input => _input;

		private void Start()
		{
			_input = new MarblingInputHandler(_colorInjection);
			_material.SetFloat("_Aspect", (float)_forceField.width / _forceField.height);

			Graphics.Blit(Texture2D.blackTexture, _colorInjection);
			Graphics.Blit(Texture2D.blackTexture, _forceField);
		}

		private void Update()
		{
			colorList.Clear();
			forceList.Clear();
			_input.Update();
		}

		private void LateUpdate()
		{
			foreach (MarblingCommand command in colorList)
			{
				BlitToColorInternal(command);
			}

			foreach (MarblingCommand command in forceList)
			{
				BlitToForceFieldInternal(command);
			}
		}

		internal void BlitToColor(Vector2 position, Color color, float falloff)
		{
			BlitToColor(new(position, color, falloff));
		}

		internal void BlitToForceField(Vector2 position, Vector2 force, float falloff)
		{
			BlitToForceField(new(position, force, falloff));
		}

		public void BlitToColor(MarblingCommand command)
		{
			colorList.Add(command);
		}

		public void BlitToForceField(MarblingCommand command)
		{
			forceList.Add(command);
		}

		private void BlitToColorInternal(MarblingCommand command)
		{
			_material.SetColor("_Color", command.color);
			_material.SetVector("_Origin", command.position);
			_material.SetFloat("_Falloff", command.falloff);
			Graphics.Blit(null, _colorInjection, _material, 0);
		}

		private void BlitToForceFieldInternal(MarblingCommand command)
		{
			_material.SetVector("_Origin", command.position);
			_material.SetFloat("_Falloff", command.falloff);
			_material.SetVector("_Force", command.force);
			Graphics.Blit(null, _forceField, _material, 1);
		}

		internal void ClearForceField()
		{
			Graphics.Blit(Texture2D.blackTexture, _forceField);
		}

		internal void ClearColorInjection()
		{
			Graphics.Blit(Texture2D.blackTexture, _colorInjection);
		}
	}
}