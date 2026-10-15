using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace StableFluids.Marbling
{
	public sealed class MarblingInputHandler
	{
		public Vector2 Position { get; private set; }
		public Vector2 Velocity { get; private set; }
		public bool LeftPressed { get; private set; }
		public bool RightPressed { get; private set; }
		public bool RightPressedThisFrame { get; private set; }

		private Vector2 _previousInput;
		private float _targetAspectRatio;
		private bool _wasInputActive;

		public MarblingInputHandler(RenderTexture targetTexture)
		{
			_targetAspectRatio = (float)targetTexture.width / targetTexture.height;
		}

		public void Update()
		{
			// Get input from touch or mouse (touch has priority)
			(bool hasInput, Vector2 position, bool left, bool right) = GetActiveInput();

			if (hasInput)
			{
				UpdateInputState(position, left, right);
			}
			else
			{
				UpdateInputState(Vector2.zero, false, false);
			}

		}

		private (bool hasInput, Vector2 position, bool left, bool right) GetActiveInput()
		{
			// Prioritize touch input for WebGL compatibility
			(bool hasInput, Vector2 position, bool left, bool right) touchInput = GetTouchInput();
			if (touchInput.hasInput)
			{
				return touchInput;
			}

			(bool hasInput, Vector2 position, bool left, bool right) mouseInput = GetMouseInput();
			if (mouseInput.hasInput)
			{
				return mouseInput;
			}

			return (false, Vector2.zero, false, false);
		}

		private (bool hasInput, Vector2 position, bool left, bool right) GetMouseInput()
		{
			Mouse mouse = Mouse.current;
			if (mouse == null)
			{
				return (false, Vector2.zero, false, false);
			}


			bool left = mouse.leftButton.isPressed;
			bool right = mouse.rightButton.isPressed;

			if (!left && !right)
			{
				return (false, Vector2.zero, false, false);
			}


			Vector2 position = GetNormalizedInputPosition(mouse.position.ReadValue());
			return (true, position, left, right);
		}

		private (bool hasInput, Vector2 position, bool left, bool right) GetTouchInput()
		{
			Touchscreen touchscreen = Touchscreen.current;
			if (touchscreen == null)
			{
				return (false, Vector2.zero, false, false);
			}

			// Calculate average position of all active touches

			int activeTouches = 0;
			Vector2 sumPosition = Vector2.zero;

			foreach (TouchControl touch in touchscreen.touches)
			{
				if (!touch.isInProgress)
				{
					continue;
				}


				sumPosition += touch.position.ReadValue();
				activeTouches++;
			}

			if (activeTouches == 0)
			{
				return (false, Vector2.zero, false, false);
			}

			// Map touch count to button states: 1 touch = left, 2+ touches = right

			Vector2 position = GetNormalizedInputPosition(sumPosition / activeTouches);
			return (true, position, activeTouches == 1, activeTouches >= 2);
		}

		private void UpdateInputState(Vector2 position, bool leftPressed, bool rightPressed)
		{
			Position = position;
			LeftPressed = leftPressed;

			RightPressedThisFrame = false;
			if (RightPressed != rightPressed && RightPressed == false)
			{
				RightPressedThisFrame = true;
			}

			RightPressed = rightPressed;

			// Reset velocity on first frame of input to avoid large initial jumps
			bool isActive = leftPressed || rightPressed;
			Velocity = isActive && _wasInputActive ? position - _previousInput : Vector2.zero;

			_previousInput = position;
			_wasInputActive = isActive;
		}

		private Vector2 GetNormalizedInputPosition(Vector2 screenPos)
		{
			// Convert screen position to normalized coordinates with aspect ratio correction
			float screenAspect = (float)Screen.width / Screen.height;
			Vector2 offset = screenPos - new Vector2(Screen.width, Screen.height) / 2;
			float width = Screen.height * Mathf.Min(_targetAspectRatio, screenAspect);
			return offset / width;
		}
	}
}