using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2Ex
{
	public static Vector2 Rotate(this Vector2 self, float degrees)
	{
		float radians = degrees * Mathf.Deg2Rad;
		float sin = Mathf.Sin(radians);
		float cos = Mathf.Cos(radians);

		float tx = self.x;
		float ty = self.y;

		return new Vector2(cos * tx - sin * ty, sin * tx + cos * ty);
	}
}

public class ToddlerManager : MonoBehaviour
{
	[SerializeField] ToddlerController prefabToSpawn;
	[SerializeField] GameObject loseScreen;

	List<ToddlerController> toddlers = new List<ToddlerController>();

	public bool IsGameOver
	{
		get
		{
			foreach(var t in toddlers)
			{
				if(t.HasThrownTantrum)
				{
					return true;
				}
			}
			return false;
		}
	}

	private void Awake()
	{
		int num = UIStartScreen.NumberOfBabiesSelected;
		for (int i=0; i < num ; ++i)
		{
			var spawnPoint = Vector2.up;
			spawnPoint.Rotate(i * (360.0f / (float)num));
			toddlers.Add( Instantiate(this.prefabToSpawn, new Vector3(spawnPoint.x, 0, spawnPoint.y), Quaternion.identity) );
		}
	}

	private void Update()
	{
		loseScreen.SetActive(IsGameOver);
	}
}
