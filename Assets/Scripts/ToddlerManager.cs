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
	public static ToddlerManager Current { get; private set; }

	[SerializeField] ToddlerController prefabToSpawn;
	[SerializeField] GameObject loseScreen;
	[SerializeField] float spawnRadius = 2.0f;

	private bool hasSetOffOtherBabiesToTantrum = false;

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

	public float Hangriness
    {
        get
        {
			float maxHangry = float.MinValue;
			foreach (var t in toddlers)
            {
				maxHangry = Mathf.Max(t.GetComponent<HangryController>().Hangriness, maxHangry);
            }
			return maxHangry;
        }
    }

	private void Awake()
	{
		Current = this;

		int num = UIStartScreen.NumberOfBabiesSelected;
		for (int i=0; i < num ; ++i)
		{
			var spawnPoint = Vector2.up;
			spawnPoint = spawnPoint.Rotate(i * (360.0f / (float)num));
			toddlers.Add( Instantiate(this.prefabToSpawn, spawnRadius * new Vector3(spawnPoint.x, 0, spawnPoint.y), Quaternion.identity) );
		}
	}

	private void Update()
	{
		Cursor.visible = IsGameOver;
		loseScreen.SetActive(IsGameOver);
		if(IsGameOver)
		{
			if (!hasSetOffOtherBabiesToTantrum)
			{
				hasSetOffOtherBabiesToTantrum = true;
				foreach (var tod in toddlers)
				{
					if (!tod.HasThrownTantrum)
					{
						tod.delayedTantrum();
					}
				}
			}
		}
	}

    private void OnDestroy()
    {
		if (Current = this)
			Current = null;
    }
}
