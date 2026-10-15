using UnityEngine;

namespace StableFluids.Marbling
{
	public sealed class MarblingEmitter : MonoBehaviour
	{
		[field: SerializeField] public float PointForce { get; set; } = 300;
		[field: SerializeField] public float PointFalloff { get; set; } = 200;

		[SerializeField] private MarblingBase marblingBase;
		[SerializeField] private Color color = Color.red;
		[SerializeField] private Vector2 direction = Vector2.one;
		[SerializeField] private Vector2 position = Vector2.one / 2;

		private void Update()
		{
			marblingBase.BlitToColor(position, color, PointFalloff);
			marblingBase.BlitToForceField(position, direction * PointForce, PointFalloff);
		}
	}
}