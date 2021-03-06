using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CameraEx
{

	public static bool ScreenToWorldPointAtPlane(this Camera cam, Vector2 pos, Plane plane, ref Vector3 result)
	{
		return cam.ViewportToWorldPointAtPlane(new Vector2(pos.x / Screen.width, pos.y / Screen.height), plane, ref result);
	}

	public static bool ViewportToWorldPointAtPlane(this Camera cam, Vector2 pos, Plane plane, ref Vector3 result)
	{
		var ray = cam.ViewportPointToRay(new Vector3(pos.x, pos.y, 0f));
		float enter = 0;
		if (plane.Raycast(ray, out enter))
		{
			result = ray.GetPoint(enter);
			return true;
		}
		else
		{
			return false;
		}
	}

}

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
		KeyboardDirect,

		Mouse
	}

	[Header("Movement")]
	[SerializeField] ControlType controlType = ControlType.KeyboardCar;
	[SerializeField] float maxSpeed = 4.0f;
	[SerializeField] float maxSpeedReverse = -2.0f;
	[SerializeField] float angularSpeed = 90.0f;
	[SerializeField] float acceleration = 1.5f;
	[SerializeField] float moveMaxBounds = 4.0f;
	[SerializeField] float offsetOfHandForMousePositionCalc = 0.5f;
	[SerializeField] float maxMouseSpeed = 10.0f;

	[Header("Grabs")]
	[SerializeField] Transform grabAnchor;
	[SerializeField] float grabRadius;
	[SerializeField] float grabDuration = 1.0f;
	[SerializeField] float grabCooldown = 1.0f;
	[SerializeField] float grabOffsetHeight = 0.5f;
	[SerializeField] float grabMaxBounds = 3.5f;

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
		if (!ToddlerManager.Current.IsGameOver)
		{
			#region Hand Grabbing
			bool shouldHandBeClosed = false;
			// bool isGrabKeyHeld = Input.GetKey(KeyCode.Space);
			// bool isGrabKeyDown = Input.GetKeyDown(KeyCode.Space);
			bool isGrabKeyHeld = Input.GetKey(KeyCode.Mouse0);
			bool isGrabKeyDown = Input.GetKeyDown(KeyCode.Mouse0);

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
						for (int i = 0; i < amount; ++i)
						{
							var go = this.colliders[i].gameObject;
							bool shouldGrabThisGo = false;

							var toddler = go.GetComponent<ToddlerController>();
							if (toddler != null)
							{
								toddler.BeingGrabbedBy = this;
								if (this.giggles.Length > 0)
								{
									var giggle = this.giggles[grabCount % giggles.Length];
									AudioSource.PlayClipAtPoint(giggle, this.transform.position);
								}
								grabCount++;
								shouldGrabThisGo = true;
							}
							else
							{
								var grabbable = go.GetComponent<Grabbable>();
								if (grabbable != null)
								{
									grabbable.OnGrabbed();
									go.transform.position += new Vector3(0, grabOffsetHeight, 0);
									go.transform.rotation = Quaternion.LookRotation(Random.onUnitSphere, Vector3.up);
									shouldGrabThisGo = true;
								}
							}

							if (shouldGrabThisGo)
							{
								this.grabbedObject = go;
								this.grabState = GrabState.Grabbed;
								this.timeSinceLastGrabState = 0.0f;
								break;
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
						CancelGrab();
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

			Cursor.visible = false;
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
				case ControlType.Mouse:

					Vector3 mouseTarget = Vector3.zero;
					if ( Camera.main.ScreenToWorldPointAtPlane(Input.mousePosition, new Plane(Vector3.up, this.offsetOfHandForMousePositionCalc), ref mouseTarget))
					{
						mouseTarget.y = 0.0f;
						var currPos = this.transform.position;
						currPos.y = 0.0f;
						displacement = Vector3.ClampMagnitude(mouseTarget - currPos, this.maxMouseSpeed * Time.deltaTime);
					}
					break;
			}


			var pos = this.transform.position + displacement;
			pos.x = Mathf.Clamp(pos.x, -1 * this.moveMaxBounds, this.moveMaxBounds);
			pos.z = Mathf.Clamp(pos.z, -1 * this.moveMaxBounds, this.moveMaxBounds);
			this.transform.position = pos;

			if (this.grabbedObject != null)
			{
				var cc = this.grabbedObject.GetComponent<CharacterController>();
				if (cc != null)
				{
					cc.Move(displacement);
				}
				else
				{
					var finalPosition = this.grabbedObject.transform.position + displacement;
					finalPosition = new Vector3(
						Mathf.Clamp(finalPosition.x, -grabMaxBounds, grabMaxBounds),
						finalPosition.y,
						Mathf.Clamp(finalPosition.z, -grabMaxBounds, grabMaxBounds)
					);
					this.grabbedObject.transform.position = finalPosition;
				}
			}
		}
	}

	/*Collider[] grabColliders = new Collider[32];
	private void TryMoveGrabObject(ShaderVariantCollection displacement)
	{
		var colliderBounds = this.grabbedObject.GetComponent<Collider>().bounds;
		var startPosition = this.grabbedObject.transform.position;
		this.grabbedObject.transform.position += displacement;
		Debug.DrawRay(colliderBounds.center, Vector3.up, Color.red, 1.0f);
		bool canMove = true;
		int num = Physics.OverlapBoxNonAlloc(colliderBounds.center, colliderBounds.extents * 1.1f, grabColliders);
		for (int i = 0; i < num; i++)
		{
			if (this.grabColliders[i].gameObject != this.grabbedObject)
			{
				canMove = false;
			}
		}

		if (!canMove)
		{
			this.grabbedObject.transform.position = startPosition;
		}
	}*/

	public void CancelGrab()
	{
		if(this.grabState == GrabState.Grabbed)
		{
			var toddler = this.grabbedObject.GetComponent<ToddlerController>();
			if (toddler != null)
			{
				toddler.BeingGrabbedBy = null;
			}
			var grabbable = this.grabbedObject.GetComponent<Grabbable>();
			if (grabbable != null)
			{
				grabbable.OnLetGo();
			}
			this.grabbedObject = null;
			this.grabState = GrabState.Cooldown;
			this.timeSinceLastGrabState = 0.0f;
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
