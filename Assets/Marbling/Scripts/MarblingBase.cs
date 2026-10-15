using System;
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
		private bool clear = true;

		public MarblingInputHandler Input => _input;

		private void Start()
		{
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
			if (clear)
			{
				ClearForceField();
				ClearColorInjection();
			}
			_input.Update();
			clear = true;
		}

		public void BlitToColor(Vector2 position, Color color, float pointFalloff)
		{
			_material.SetColor("_Color", color);
			_material.SetVector("_Origin", position);
			_material.SetFloat("_Falloff", pointFalloff);
			Graphics.Blit(null, _colorInjection, _material, 0);
			clear = false;
		}

		public void BlitToForceField(Vector2 position, Vector2 force, float pointFalloff)
		{
			_material.SetVector("_Origin", position);
			_material.SetFloat("_Falloff", pointFalloff);
			_material.SetVector("_Force", force);
			Graphics.Blit(null, _forceField, _material, 1);
			clear = false;
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