using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHand : MonoBehaviour
{
	public enum ControlType
	{
		/// <summary>
		/// Control hand like a car
		/// </summary>
		KeyboardCar,

		/// <summary>
		/// Control hand directly
		/// </summary>
		KeyboardDirect
	}

	[Header("Movement")]
	[SerializeField] ControlType controlType = ControlType.KeyboardCar;
	[SerializeField] float maxSpeed = 4.0f;
	[SerializeField] float maxSpeedReverse = -2.0f;
	[SerializeField] float angularSpeed = 90.0f;
	[SerializeField] float acceleration = 1.5f;
	
	[Header("Grabs")]
	[SerializeField] Transform grabAnchor;
	[SerializeField] float grabRadius;
	[SerializeField] float grabDuration = 1.0f;
	[SerializeField] float grabCooldown = 1.0f;

	[Header("Visuals")]
	[SerializeField] Transform[] fingers;
	
	[Header("Audio")]
	[SerializeField] AudioClip[] giggles;

	[Header("Animations")]
	[SerializeField] Animator handAnimator;

	Collider[] colliders = new Collider[16];
	float speed = 0.0f;
	float fingerLerp = 0.0f;
	float grabNextTimeAllowed = -666.0f;
	int grabCount = 0;

	GameObject grabbedObject = null;


	#region Grab State Machine
	enum GrabState
	{
		Idle,
		Grabbing,
		Grabbed,
		Cooldown
	}
	GrabState grabState = GrabState.Idle;
	float timeSinceLastGrabState = 0.0f;
	#endregion

	void Start()
	{
	}

	private void Update()
	{
		#region Hand Grabbing
		bool shouldHandBeClosed = false;
		bool isGrabKeyHeld = Input.GetKey(KeyCode.Space);
		bool isGrabKeyDown = Input.GetKeyDown(KeyCode.Space);

		float prevTimeSinceLastGrabState = this.timeSinceLastGrabState;
		this.timeSinceLastGrabState += Time.deltaTime;
		switch (grabState)
		{
			case GrabState.Idle:
				if (isGrabKeyDown)
				{
					this.grabState = GrabState.Grabbing;
					this.timeSinceLastGrabState = 0.0f;
				}
				break;
			case GrabState.Grabbing:

				const float GRAB_WINDOW = 0.1f;

				shouldHandBeClosed = true;
				if (!isGrabKeyHeld)
				{
					this.grabState = GrabState.Idle;
					this.timeSinceLastGrabState = 0.0f;
				}
				else if (prevTimeSinceLastGrabState < GRAB_WINDOW && GRAB_WINDOW <= this.timeSinceLastGrabState)
				{
					int amount = Physics.OverlapSphereNonAlloc(this.grabAnchor.position, this.grabRadius, colliders);
					for(int i=0; i < amount; ++i)
					{
						var go = this.colliders[i].gameObject;
						bool shouldGrabThisGo = false;
						
						var toddler = go.GetComponent<ToddlerController>();
						if (toddler != null)
						{
							toddler.beingGrabbed = true;
							if (this.giggles.Length > 0)
							{
								var giggle = this.giggles[grabCount % giggles.Length];
								AudioSource.PlayClipAtPoint(giggle, this.transform.position);
							}
							grabCount++;
							shouldGrabThisGo = true;
						}

						if(shouldGrabThisGo)
						{
							this.grabbedObject = go;
							this.grabState = GrabState.Grabbed;
							this.timeSinceLastGrabState = 0.0f;
						}
					}
				}

				break;

			case GrabState.Grabbed:
				if (grabbedObject == null)
                {
					this.grabState = GrabState.Cooldown;
					return;
                }
				
				shouldHandBeClosed = true;
				if (this.timeSinceLastGrabState > this.grabDuration || !isGrabKeyHeld)
				{
					var toddler = this.grabbedObject.GetComponent<ToddlerController>();
					if (toddler != null)
					{
						toddler.beingGrabbed = false;
					}
					this.grabbedObject = null;
					this.grabState = GrabState.Cooldown;
					this.timeSinceLastGrabState = 0.0f;
				}
				break;

			case GrabState.Cooldown:
				if (this.timeSinceLastGrabState > this.grabCooldown)
				{
					this.grabState = GrabState.Idle;
					this.timeSinceLastGrabState = 0.0f;
				}
				break;
		}

		if (shouldHandBeClosed)
		{
			this.fingerLerp = Mathf.MoveTowards(this.fingerLerp, 1.0f, Time.deltaTime * 4.0f);
		}
		else
		{
			fingerLerp = Mathf.MoveTowards(fingerLerp, 0, Time.deltaTime * 2.0f);
		}

		handAnimator.SetBool("IsClosed", shouldHandBeClosed);

		foreach (var finger in this.fingers)
		{
			finger.localEulerAngles = new Vector3(Mathf.Lerp(0.0f, 45.0f, fingerLerp), 0.0f, 0.0f);
		}
		#endregion

		Vector3 displacement = Vector3.zero;

		Vector2 inputAxis = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

		switch (this.controlType)
		{
			case ControlType.KeyboardCar:

				if (this.grabState == GrabState.Idle || this.grabState == GrabState.Cooldown)
				{
					this.transform.Rotate(0, inputAxis.x * this.angularSpeed * Time.deltaTime, 0);
				}

				if (inputAxis.y > 0.01f)
				{
					this.speed = Mathf.MoveTowards(this.speed, this.maxSpeed, Time.deltaTime * this.acceleration);
				}
				else if (inputAxis.y < -0.01f)
				{
					this.speed = Mathf.MoveTowards(this.speed, this.maxSpeedReverse, Time.deltaTime * this.acceleration);
				}
				else
				{
					this.speed = Mathf.MoveTowards(this.speed, 0.0f, Time.deltaTime * this.acceleration);
				}

				displacement = this.transform.forward * (this.speed * Time.deltaTime);

				break;
			case ControlType.KeyboardDirect:

				Vector3 dir = new Vector3(
					inputAxis.x,
					0.0f,
					inputAxis.y
				);

				displacement += dir * (maxSpeed * Time.deltaTime);

				break;
		}
		
		
		this.transform.position += displacement;

		if (this.grabbedObject != null)
		{
			var cc = this.grabbedObject.GetComponent<CharacterController>();
			if (cc != null)
			{
				cc.Move(displacement);
			}
			else
			{
				this.grabbedObject.transform.position += displacement;
			}
		}
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
