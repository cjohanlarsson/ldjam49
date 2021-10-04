using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bottle : MonoBehaviour
{
	[Header("Audio")]
	[SerializeField] AudioClip bottleSpawn;
	[SerializeField] AudioClip bottleDrink;

	void Start()
    {
		AudioSource.PlayClipAtPoint(bottleSpawn, this.transform.position);
    }

	private void OnTriggerEnter(Collider other)
	{
		if(other.GetComponentInChildren<ToddlerController>() != null && !ToddlerManager.Current.IsGameOver)
		{
			AudioSource.PlayClipAtPoint(bottleDrink, this.transform.position);
			GameObject.Destroy(this.gameObject);
			BottleManager.Current.AddScore(1);
			other.GetComponent<HangryController>().resetHangry();
		}
	}
}
