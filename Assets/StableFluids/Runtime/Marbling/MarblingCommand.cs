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
}