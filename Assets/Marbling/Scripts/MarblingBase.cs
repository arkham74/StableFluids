using System;
using System.Collections.Generic;
using UnityEngine;

namespace StableFluids.Marbling
{
	public sealed class MarblingBase : MonoBehaviour
	{
		[SerializeField] private RenderTexture _colorInjection = null;
		[SerializeField] private RenderTexture _forceField = null;
		[SerializeField] private Shader _shader = null;

		private MarblingInputHandler _input;
		private Material _material;

		public MarblingInputHandler Input => _input;

		private readonly List<(Vector2, Color, float)> colorList = new();
		private readonly List<(Vector2, Vector2, float)> forceList = new();

		private void Start()
		{
			colorList.Clear();
			forceList.Clear();

			_input = new MarblingInputHandler(_colorInjection);
			_material = new Material(_shader);
			_material.SetFloat("_Aspect", (float)_forceField.width / _forceField.height);

			Graphics.Blit(Texture2D.blackTexture, _colorInjection);
			Graphics.Blit(Texture2D.blackTexture, _forceField);
		}

		private void OnDestroy()
		{
			Destroy(_material);
		}

		private void Update()
		{
			colorList.Clear();
			forceList.Clear();
			_input.Update();
		}

		private void LateUpdate()
		{
			bool clearColor = true;

			foreach ((Vector2 pos, Color col, float fal) in colorList)
			{
				BlitToColorInternal(pos, col, fal);
				clearColor = false;
			}

			if (clearColor)
			{
				ClearColorInjection();
			}

			bool clearForce = true;

			foreach ((Vector2 pos, Vector2 @for, float fal) in forceList)
			{
				BlitToForceFieldInternal(pos, @for, fal);
				clearForce = false;
			}

			if (clearForce)
			{
				ClearForceField();
			}
		}

		public void BlitToColor(Vector2 position, Color color, float pointFalloff)
		{
			colorList.Add((position, color, pointFalloff));
		}

		public void BlitToForceField(Vector2 position, Vector2 force, float pointFalloff)
		{
			forceList.Add((position, force, pointFalloff));
		}

		private void BlitToColorInternal(Vector2 position, Color color, float pointFalloff)
		{
			_material.SetColor("_Color", color);
			_material.SetVector("_Origin", position);
			_material.SetFloat("_Falloff", pointFalloff);
			Graphics.Blit(null, _colorInjection, _material, 0);
		}

		private void BlitToForceFieldInternal(Vector2 position, Vector2 force, float pointFalloff)
		{
			_material.SetVector("_Origin", position);
			_material.SetFloat("_Falloff", pointFalloff);
			_material.SetVector("_Force", force);
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