using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabbable : MonoBehaviour
{
	[SerializeField] AudioClip[] clipsToPlayOnGrab;
	
	int timesGrabbed = 0;

	List<Rigidbody> rbsToChangeBack = new List<Rigidbody>();
	
	public bool IsBeingGrabbed { get; private set; }

	public void OnGrabbed()
	{
		this.IsBeingGrabbed = true;
		if (this.clipsToPlayOnGrab.Length > 0)
		{
			var clip = this.clipsToPlayOnGrab[this.timesGrabbed % this.clipsToPlayOnGrab.Length];
			AudioSource.PlayClipAtPoint(clip, this.transform.position);
		}
		this.timesGrabbed++;

		foreach(var rb in this.GetComponentsInChildren<Rigidbody>())
		{
			if(!rb.isKinematic)
			{
				this.rbsToChangeBack.Add(rb);
				rb.isKinematic = true;
			}
		}
	}

	public void OnLetGo()
	{
		this.IsBeingGrabbed = false;
		foreach (var rb in this.rbsToChangeBack)
		{
			rb.isKinematic = false;
		}
		this.rbsToChangeBack.Clear();
	}

	/*void OnCollisionEnter(Collision collision)
	{
		if (this.IsBeingGrabbed)
		{
			Debug.Log("IS BEING GRABBED: " + collision.gameObject.name);
		}
	}*/
}
