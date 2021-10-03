using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabbable : MonoBehaviour
{
	[SerializeField] AudioClip[] clipsToPlayOnGrab;
	
	int timesGrabbed = 0;

	List<Rigidbody> rbsToChangeBack = new List<Rigidbody>();

	public void OnGrabbed()
	{
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
		foreach(var rb in this.rbsToChangeBack)
		{
			rb.isKinematic = false;
		}
		this.rbsToChangeBack.Clear();
	}
}
