using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHand : MonoBehaviour
{
	[SerializeField] float maxSpeed = 4.0f;
	[SerializeField] float maxSpeedReverse = -2.0f;
	[SerializeField] float angularSpeed = 90.0f;
	[SerializeField] float acceleration = 1.5f;
	[SerializeField] Transform grabAnchor;
	[SerializeField] float grabRadius;

	[SerializeField] Transform[] fingers;
	[SerializeField] Collider[] colliders = new Collider[16];
	float speed = 0.0f;
	float fingerLerp = 0.0f;

	GameObject grabbedObject = null;

	private void Update()
	{
		
		

		bool isGrabbing = false;
		if (Input.GetKey(KeyCode.Space))
		{
			isGrabbing = true;
			fingerLerp = Mathf.MoveTowards(fingerLerp, 1.0f, Time.deltaTime * 4.0f);
			if(grabbedObject == null)
			{
				int amount = Physics.OverlapSphereNonAlloc(this.grabAnchor.position, this.grabRadius, colliders);
				if(amount > 0)
				{
					grabbedObject = this.colliders[0].gameObject;
				}
			}
		}
		else
		{
			this.grabbedObject = null;
			fingerLerp = Mathf.MoveTowards(fingerLerp, 0, Time.deltaTime * 2.0f);
		}

		if (!isGrabbing)
		{
			if (Input.GetKey(KeyCode.LeftArrow))
			{
				this.transform.Rotate(0, -1 * this.angularSpeed * Time.deltaTime, 0);
			}
			else if (Input.GetKey(KeyCode.RightArrow))
			{
				this.transform.Rotate(0, this.angularSpeed * Time.deltaTime, 0);
			}
		}

		if (Input.GetKey(KeyCode.UpArrow) )
		{
			this.speed = Mathf.MoveTowards(this.speed, this.maxSpeed, Time.deltaTime * this.acceleration);
		}
		else if(Input.GetKey(KeyCode.DownArrow))
		{
			this.speed = Mathf.MoveTowards(this.speed, this.maxSpeedReverse, Time.deltaTime * this.acceleration);
		}
		else
		{
			this.speed = Mathf.MoveTowards(this.speed, 0.0f, Time.deltaTime * this.acceleration);
		}


		foreach (var finger in this.fingers)
		{
			finger.localEulerAngles = new Vector3(Mathf.Lerp(0.0f, 45.0f, fingerLerp), 0.0f, 0.0f);
		}
		Vector3 displacement = this.transform.forward * (this.speed * Time.deltaTime);
		this.transform.position += displacement;
		if (this.grabbedObject != null)
			this.grabbedObject.transform.position += displacement;
	}

	private void OnDrawGizmos()
	{
		if (this.grabAnchor != null)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(this.grabAnchor.position, this.grabRadius);
		}
	}
}
