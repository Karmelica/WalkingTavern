using Cooking.Minigames;
using Managers;
using UnityEngine;
using World;

namespace Cooking
{
	public class Pot : DishMinigame
	{
		private Vector2 _lastMousePos;
		private int _lastSlice = -1;
		private int _mixStreak;
		private Vector2 _screenMiddle;
		private int _thisSlice = -1;

		protected override void Awake()
		{
			base.Awake();
			_screenMiddle = new Vector2(Screen.width / 2f, Screen.height / 2f);

			_lastMousePos = MousePos;
		}

		private void OnTriggerEnter(Collider other)
		{
			AddCollidedFood(other);
		}

		private void OnTriggerExit(Collider other)
		{
			RemoveCollidedFood(other);
		}

		private void AddCollidedFood(Collider other)
		{
			if (!other.TryGetComponent(out ProcessedFoodItem foodItem)) return;
			foodItem.OnObjectDisable += RemoveCollidedFood;
			TryAddIngredient(foodItem);
		}

		private void RemoveCollidedFood(Collider other)
		{
			if (!other.TryGetComponent(out ProcessedFoodItem foodItem)) return;
			foodItem.OnObjectDisable -= RemoveCollidedFood;
			TryRemoveIngredient(foodItem);
		}

		protected override void DoMinigame()
		{
			base.DoMinigame();
			if (!DidHit) {
				AudioManager.Instance.StopStirring();
				return;
			}

			PlayStirSound();
			var newSlice = GetSlice(MousePos);

			if (newSlice == _thisSlice || newSlice == -1) return;

			_lastSlice = _thisSlice;
			_thisSlice = newSlice;

			if (_lastSlice == -1) return;

			if (IsClockwiseTransition(_lastSlice, _thisSlice)) {
				_mixStreak++;
			} else {
				_mixStreak = 0;
			}

			if (_mixStreak == 4) {
				Score++;
				_mixStreak = 0;
			}
		}

		private void PlayStirSound()
		{
			var mousePos = MousePos;
			if (mousePos != _lastMousePos) {
				AudioManager.Instance.StartStirring();
			} else {
				AudioManager.Instance.StopStirring();
			}

			_lastMousePos = mousePos;
		}

		private int GetSlice(Vector3 mousePos)
		{
			if (mousePos.y > _screenMiddle.y) {
				if (mousePos.x < _screenMiddle.x) return 0;
				if (mousePos.x > _screenMiddle.x) return 1;
			} else {
				if (mousePos.x < _screenMiddle.x) return 2;
				if (mousePos.x > _screenMiddle.x) return 3;
			}

			return -1;
		}

		private static bool IsClockwiseTransition(int from, int to)
		{
			return (from == 0 && to == 1) ||
			       (from == 1 && to == 3) ||
			       (from == 3 && to == 2) ||
			       (from == 2 && to == 0);
		}

		public override string GetInteractText()
		{
			return "Mixing Pot";
		}
	}
}