using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Code
{
	public class SecurityScreens : MonoBehaviour
	{
		[SerializeField] private List<RawImage> _screens;
		[SerializeField] private Texture2D _staticTexture;
		
		private List<BabyBehavior> _shownBabies = new List<BabyBehavior>();
		
		private void Start()
		{
			StartCoroutine(UpdateScreensRoutine());
		}

		public void Reset()
		{
			_shownBabies.Clear();
		}

		private IEnumerator UpdateScreensRoutine()
		{
			while (isActiveAndEnabled)
			{
				yield return new WaitForSeconds(2);

				BabyBehavior[] babies = FindObjectsByType<BabyBehavior>(FindObjectsSortMode.None);
				if (babies.Count() > 5)
				{
					// switch a screen
					// find one not shown
					List<BabyBehavior> notShownBabies = babies.ToList().Except(_shownBabies).ToList();
					BabyBehavior toShowBaby = notShownBabies[Random.Range(0, notShownBabies.Count - 1)];
					int toHideBabyIndex = Random.Range(0, _shownBabies.Count - 1);
					BabyBehavior toHideBaby = _shownBabies[toHideBabyIndex];
					
					_shownBabies[toHideBabyIndex] = toShowBaby;
					_screens[toHideBabyIndex].texture = _staticTexture;
					yield return new WaitForSeconds(0.25f);
					_screens[toHideBabyIndex].texture = toShowBaby.RenderTexture;
				}
			}
			
			yield break;
		}

		public void AddBaby(BabyBehavior babyBehavior)
		{
			// init screen
			if (_shownBabies.Count < 5)
			{
				RawImage rawImage = _screens[_shownBabies.Count];
				rawImage.texture = babyBehavior.RenderTexture;
				
				_shownBabies.Add(babyBehavior);
			}
		}

		public void RemoveBaby(BabyBehavior babyBehavior)
		{
			int index = _shownBabies.IndexOf(babyBehavior);
			_screens[index].texture = _staticTexture;
			_shownBabies.Remove(babyBehavior);
		}
	}
}