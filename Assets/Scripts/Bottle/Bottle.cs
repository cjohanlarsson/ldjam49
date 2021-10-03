using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bottle : MonoBehaviour
{
	[SerializeField] AudioClip clip;
	[SerializeField] AudioSource source;

	private void OnTriggerEnter(Collider other)
	{
		if(other.GetComponentInChildren<ToddlerController>() != null)
		{
			source.Play();
			AudioSource.PlayClipAtPoint(clip, this.transform.position);
			foreach (var r in this.GetComponentsInChildren<Renderer>())
			{
				r.enabled = false;
			}
			GameObject.Destroy(this.gameObject, 3.0f);
			BottleManager.Current.AddScore(1);
		}
	}
}
