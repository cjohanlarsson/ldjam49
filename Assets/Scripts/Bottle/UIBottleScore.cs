using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBottleScore : MonoBehaviour
{
	[SerializeField] Text score;

	private void Start()
	{
		BottleManager.Current.OnScoreChanged += (s) =>
		{
			score.text = s.ToString();
		};
		this.score.text = BottleManager.Current.Score.ToString();
	}
}
