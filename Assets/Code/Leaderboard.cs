using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace Code
{
	public class Leaderboard : MonoBehaviour
	{
		[SerializeField] protected TMP_Text _leaderboardText;
		
		public IEnumerator ShowLeaderboard(int score)
		{
			UnityWebRequest request = UnityWebRequest.Get($"https://dev.previewlabs.com/rabiesBabies/leaderboard.php?value={score}");
			
			yield return request.SendWebRequest();
			
			string response = request.downloadHandler.text;
			Debug.Log(response);
			Highscores hs = JsonUtility.FromJson<Highscores>("{ \"highscores\":" + response + "}");
			hs.highscores.Sort();
			hs.highscores.Reverse();
			for (int i = 0; i < hs.highscores.Count; i++)
			{
				if (hs.highscores[i].value == score) {
					hs.highscores[i].mine = true;
					break;
				}
			}
			_leaderboardText.text = string.Join("\n", hs.highscores.Take(6));
		}
	}

	[Serializable]
	class Highscore : IComparable<Highscore>
	{
		public int value;
		public string date;
		public bool mine;

		public int CompareTo(Highscore other)
		{
			return value.CompareTo(other.value);
		}

		public override string ToString()
		{
			string prefix = mine ? "-> " : "";
			string suffix = mine ? " <-" : "";
			var minutes = Mathf.FloorToInt(value / 60);
			var seconds = Mathf.FloorToInt(value % 60);
			return $"{prefix}{minutes}:{seconds:00}{suffix}";
		}
	}
	
	[Serializable]
	class Highscores
	{
		public List<Highscore> highscores;
	}
}