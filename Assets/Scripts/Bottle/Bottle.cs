using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bottle : MonoBehaviour
{



	private void OnTriggerEnter(Collider other)
	{
		if(other.GetComponentInChildren<ToddlerController>() != null)
		{
			GameObject.Destroy(this.gameObject);
			BottleManager.Current.AddScore(1);
		}
	}
}
