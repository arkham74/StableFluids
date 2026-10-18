using Unity.Mathematics;
using UnityEngine;

namespace StableFluids.Marbling
{
	public sealed class MarblingEmitter : MonoBehaviour
	{
		[field: SerializeField] public float PointForce { get; set; } = 300;
		[field: SerializeField] public float PointFalloff { get; set; } = 200;

		[SerializeField] private float targetAspectRatio = 16 / 9f;
		[SerializeField] private MarblingBase marblingBase;
		[SerializeField] private Color color = Color.red;
		[SerializeField] private bool emitColor = true;
		[SerializeField] private bool emitForce = true;

		private void Update()
		{
			RectTransform rectTransform = transform as RectTransform;
			Vector2 normalizedPosition = RTUtil.ScreenToNormalizedPosition((Vector2)rectTransform.position, targetAspectRatio);
			if (emitColor) marblingBase.BlitToColor(normalizedPosition, color, PointFalloff);
			if (emitForce) marblingBase.BlitToForceField(normalizedPosition, (Vector2)rectTransform.right * PointForce, PointFalloff);
		}
	}
}