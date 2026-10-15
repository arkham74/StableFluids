using UnityEngine;

namespace StableFluids.Marbling
{
	public sealed class MarblingController : MonoBehaviour
	{
		[field: SerializeField] public float PointForce { get; set; } = 300;
		[field: SerializeField] public float PointFalloff { get; set; } = 200;

		[SerializeField] private MarblingBase marblingBase;
		[SerializeField] private Color color;

		private void LateUpdate()
		{
			UpdateColorInjection();
			UpdateForceField();
		}

		private void UpdateColorInjection()
		{
			if (marblingBase.Input.RightPressedThisFrame)
			{
				color = Color.HSVToRGB(Time.time % 1, 1, 1);
			}
			else if (marblingBase.Input.RightPressed)
			{
				marblingBase.BlitToColor(marblingBase.Input.Position, color, PointFalloff);
			}
			// else
			// {
			// 	marblingBase.ClearColorInjection();
			// }
		}

		private void UpdateForceField()
		{
			if (marblingBase.Input.RightPressed || marblingBase.Input.LeftPressed)
			{
				marblingBase.BlitToForceField(marblingBase.Input.Position, marblingBase.Input.Velocity * PointForce, PointFalloff);
			}
			// else
			// {
			// 	marblingBase.ClearForceField();
			// }
		}
	}
}