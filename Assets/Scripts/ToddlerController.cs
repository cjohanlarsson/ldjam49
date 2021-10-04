using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(HangryController))]
public class ToddlerController : MonoBehaviour
{

	[Header("Movement")]
	public float baseRunSpeed = 2.0f;
    public float hangryMaxRunSpeed = 4.0f;
    public float baseTurnSpeed = 1.0f;
    [Tooltip("Weighted time spent idling.")] public float idleQuotient = 0.1f;
    [Tooltip("Weighted time spent running.")] public float runQuotient = 0.2f;
    [Tooltip("Weighted time spent spinning.")] public float spinningQuotient = 0.314f;
    [Tooltip("Weighted time spent jumping.")] public float jumpingQuotient = 0.6f;
    [Range(1.0f, 3.5f)]
    public float baseSpinSpeed = 1.5f;
    public float baseJumpSpeed = 4.0f;
    public float idleLength = 1.0f;

    [SerializeField] private float movementRadius = 5.0f;
    [SerializeField] private float maxMovementDuration = 3.0f;
    [SerializeField] private float maxJumpDuration = 1.5f;
    [SerializeField] private PhysicsBaby physicsBabyPrefab = null;
    [SerializeField] private Rigidbody hipRootRigidbody = null;
    [SerializeField] private Transform leftLeg = null;
    [SerializeField] private Transform rightLeg = null;
    [SerializeField] private float legRange = 25.0f;
    [SerializeField] private float legSpeed = 4.0f;
    [SerializeField] private GameObject toddlerUIPrefab;

	[Header("Tripping")]
	[SerializeField] private bool tripEnabled = true;
	[SerializeField] private float tripLength = 2.0f;
	[SerializeField] private float tripGetupLength = 0.5f;


	[Header("Audio")]
    [SerializeField] AudioClip[] steps;
    [SerializeField] AudioClip[] tantrumSFX;
    [SerializeField] AudioClip[] jumpSFX;
    private AudioClip step;
    private AudioSource audioSource;

    Vector3 targetPosition;
    bool isMoving = false;
    float lerpDuration = 0.5f;
    float startYPos;

    HangryController hc;
    CharacterController characterController;
    CapsuleCollider rigidbodyCollider;
    GameObject toddlerUI;

    bool beingGrabbed { get { return this._beingGrabbedBy != null; } }
    PlayerHand _beingGrabbedBy = null;
    public PlayerHand BeingGrabbedBy
    {
        get { return _beingGrabbedBy; }
        set
        {
            _beingGrabbedBy = value;
            if(_beingGrabbedBy != null)
			{
                CancelCurrentAction();
			}
        }
    }

    public bool HasThrownTantrum { get; private set; }

    bool oooSomethingElse
    {
        get
        {
            return UnityEngine.Random.value < 0.2f;
        }
    }


    float spinSpeed
    {
        get
        {
            return baseSpinSpeed + 4 * hangryRatio;
        }
    }

    float turnSpeed
    {
        get
        {
            return baseTurnSpeed + 3 * hangryRatio;
        }
    }

    float runSpeed
    {
        get
        {
            return Mathf.Lerp(baseRunSpeed , this.hangryMaxRunSpeed, hangryRatio);
        }
    }

    float jumpSpeed
    {
        get
        {
            return baseJumpSpeed + 5 * hangryRatio;
        }
    }
    float hangryRatio
    {
        get { return (hc.getHangryLevel() / hc.maxHangry); }
    }

    enum TAction
    {
        None,
        Spin,
        Moving,
        Idle,
        Jump,
        Trip
    }

    TAction currentAction = TAction.None;

    Rigidbody rigidbodyRoot;

    private void Awake()
    {
        audioSource = this.GetComponent<AudioSource>();

        hc = GetComponent<HangryController>();
        characterController = GetComponent<CharacterController>();
        targetPosition = getRandomPositionNearToddler();
        
        rigidbodyCollider = GetComponent<CapsuleCollider>();
        rigidbodyRoot = GetComponent<Rigidbody>();
    }

    private void OnDestroy()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        startYPos = transform.position.y;
        if (physicsBabyPrefab != null)
        {
            var pb = Instantiate(physicsBabyPrefab);
            pb.hipJoint.connectedBody = this.hipRootRigidbody;
        }

        this.toddlerUI = Instantiate(toddlerUIPrefab, this.transform.position, Quaternion.identity);
        this.toddlerUI.GetComponentInChildren<HangryMeter>().toddlerToWatch = this.GetComponent<HangryController>();
    }

    TAction ChooseNextAction()
	{
        float rand = UnityEngine.Random.Range(0, this.runQuotient + this.spinningQuotient + this.jumpingQuotient + this.idleQuotient);
		if (rand <= this.runQuotient)
		{
			return TAction.Moving;
			
		}
		else if (rand <= this.runQuotient + this.spinningQuotient)
		{
			return TAction.Spin;
		}
		else if (rand <= this.runQuotient + this.spinningQuotient + this.jumpingQuotient)
		{
			return TAction.Jump;
		}
		else
		{
			return TAction.Idle;
		}
	}

    float actionCooldownTill = -666.0f;
    void Update()
    {
        if (this.currentAction == TAction.None && !this.beingGrabbed && !HasThrownTantrum && Time.time >= this.actionCooldownTill)
        {
            var nextAction = ChooseNextAction();
            Debug.LogFormat("Toddler {0} chooses {1}", this.name, nextAction.ToString());
            actionCooldownTill = Time.time + 1.0f;

            if (nextAction == TAction.Moving)
            {
                // do run
                targetPosition = getRandomPositionNearToddler();
                StartCoroutine(MovingAction_CoroTurnTowardTarget(targetPosition));
            }
            else if (nextAction == TAction.Spin)
            {
                StartCoroutine(SpinAction_Coro(UnityEngine.Random.Range(1.0f, 2.0f)));
            }
            else if (nextAction == TAction.Jump)
            {
                StartCoroutine(JumpAction_Coro());
            }
            else
			{
				StartCoroutine(IdleAction_Coro());
			}
		}

        UpdateLegs();
    }

	private void LateUpdate()
	{
        toddlerUI.transform.position = this.transform.position;

    }

	/*void OnControllerColliderHit(ControllerColliderHit hit)
	{
		Rigidbody body = hit.collider.attachedRigidbody;
		if (body != null && !body.isKinematic)
             body.velocity += hit.controller.velocity;
	}*/

    [SerializeField] float hitForceMultiplyer = 100.0f;

    void OnControllerColliderHit(ControllerColliderHit collision)
	{
        if (this.tripEnabled)
        {
            var grabbable = collision.gameObject.GetComponent<Grabbable>();
            if (grabbable != null)
            {
                if (grabbable.IsBeingGrabbed || this.beingGrabbed)
                {
                    if (this.currentAction != TAction.Trip)
                    {
                        if (this.BeingGrabbedBy != null)
                        {
                            this.BeingGrabbedBy.CancelGrab();
                        }
                        Debug.LogFormat("Toddler {0} was tripped by {1} ", this.gameObject.name, collision.gameObject.name);
                        CancelCurrentAction();
                        StartCoroutine(TripAction_Coro());
                        return;
                    }
                }
            }
        }

		//checks if there is rigidbody
		if (collision.rigidbody == null || collision.rigidbody.isKinematic) { return; }
		Vector3 pushDir = collision.controller.velocity;
		//Adds force to the object
		collision.rigidbody.AddForce(pushDir * collision.controller.velocity.magnitude * hitForceMultiplyer * Time.deltaTime, ForceMode.Impulse);
	}

	int stepsMade = 0;

    float prevLerp = -1.0f;

    void UpdateLegs()
    {

        if (this.leftLeg != null && this.rightLeg != null)
        {
            float lerp = Mathf.Abs(Mathf.Sin(Time.time * Mathf.PI * this.legSpeed));
            if (!this.HasThrownTantrum)
            {
                if (!isMoving)
                {
                    lerp = 0.5f;
                }
                else
                {
                    if ((this.prevLerp < 0.5f && lerp >= 0.5f) || (this.prevLerp > 0.5f && lerp <= 0.5f))
                    {
                        PlayStepSound();
                    }
                    this.prevLerp = lerp;
                }
            }
            float angle = Mathf.Lerp(-1 * this.legRange, this.legRange, lerp);

            this.leftLeg.localEulerAngles = new Vector3(angle, 0, 0);
            this.rightLeg.localEulerAngles = new Vector3(-1 * angle, 0, 0);
        }
    }

    void PlayStepSound()
    {
        if (this.steps.Length > 0 && !HasThrownTantrum)
            AudioSource.PlayClipAtPoint(this.steps[this.stepsMade++ % this.steps.Length], this.transform.position);
    }


    Vector3 getRandomPositionNearToddler() {
        float randX = UnityEngine.Random.Range(-movementRadius, movementRadius);
        float randZ = UnityEngine.Random.Range(-movementRadius, movementRadius);
        Vector3 target = new Vector3(randX, transform.position.y, randZ);
        return target;
    }

	#region Spin Action
	private IEnumerator SpinAction_Coro(float duration)
    {
        this.currentAction = TAction.Spin;
        // stop "up!" sfx
        audioSource.Stop();
        float endTime = Time.realtimeSinceStartup + duration;

        while (Time.realtimeSinceStartup < endTime)
        {
            transform.Rotate(new Vector3(0, 1.0f * spinSpeed, 0));
            yield return null;
        }

        SpinAction_End();
    }

    void SpinAction_End()
	{
        this.currentAction = TAction.None;
	}
	#endregion

	#region Moving Action
	private IEnumerator MovingAction_CoroTurnTowardTarget(Vector3 targetPos)
    {
        this.currentAction = TAction.Moving;
        // stop "up!" sfx
        audioSource.Stop();
        Quaternion startRotation = transform.rotation;
        Vector3 direction = (targetPos - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        var duration = 1 / turnSpeed;
        float timeElapsed = 0;
        while (timeElapsed < duration) {
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, timeElapsed / lerpDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;

        yield return MovingAction_CoroMoveTowards(targetPos);
    }

	private IEnumerator MovingAction_CoroMoveTowards(Vector3 targetPos)
	{
		this.isMoving = true;
		// stop "up!" sfx
		audioSource.Stop();
		var startTime = Time.time;
		while ((Time.time - startTime) < this.maxMovementDuration)
		{
			Vector3 direction = (targetPos - transform.position);
			direction.y = 0.0f;
			if (direction.sqrMagnitude < 0.01f)
				break;

			direction.Normalize();
			characterController.SimpleMove(direction * this.runSpeed);

			yield return null;
		}

		MovingAction_End();
	}

	private void MovingAction_End()
	{
		this.isMoving = false;
		this.currentAction = TAction.None;
	}
	#endregion

	Vector3 jumpStartPosition;
    private IEnumerator JumpAction_Coro()
    {
        this.currentAction = TAction.Jump;
        this.characterController.enabled = false;
        this.rigidbodyCollider.enabled = true;
        // play "up!" sfx
        audioSource.Play();
        Vector3 startPos = transform.position;
        startPos.y = startYPos;
        jumpStartPosition = startPos;
        var startTime = Time.time;
        while ((Time.time - startTime) < this.maxJumpDuration)
        {
            float factor = Mathf.Abs(Mathf.Sin((Time.time - startTime) * jumpSpeed));
            Vector3 newPos = new Vector3(transform.position.x, startPos.y + factor, transform.position.z);
            transform.position = newPos;
            yield return null;
        }

        // smooth out landing
        while (transform.position.y - startPos.y > 0.1f)
        {
            float factor = Mathf.Abs(Mathf.Sin((Time.time - startTime) * jumpSpeed));
            Vector3 newPos = new Vector3(transform.position.x, startPos.y + factor, transform.position.z);
            transform.position = newPos;
            yield return null;
        }

        
        JumpAction_End();
    }

    void JumpAction_End()
	{
        this.characterController.enabled = true;
        this.rigidbodyCollider.enabled = false;
        this.transform.position = jumpStartPosition;
        this.currentAction = TAction.None;
    }

    private IEnumerator IdleAction_Coro()
	{
        this.currentAction = TAction.Idle;
        // stop "up!" sfx
        audioSource.Stop();
        yield return new WaitForSeconds(this.idleLength);
        IdleAction_End();
	}

    void IdleAction_End()
	{
        this.currentAction = TAction.None;
    }

    Vector3 positionReturn;
    Quaternion rotationReturn;

    private IEnumerator TripAction_Coro()
	{
        this.currentAction = TAction.Trip;
        var rb = this.rigidbodyRoot;

        // setup trip
		positionReturn = rb.transform.position;
        rotationReturn = rb.transform.rotation;
        yield return new WaitForSeconds(0.1f);

        rb.velocity = Vector3.zero;
        rb.isKinematic = false;
        rb.AddForce(new Vector3(2, -1, 2), ForceMode.Impulse);
        this.characterController.enabled = false;
        this.rigidbodyCollider.enabled = true;

        // wait for trip to finish
        yield return new WaitForSeconds(this.tripLength);

        // end trip
        rb.isKinematic = true;
		this.rigidbodyCollider.enabled = false;

		var positionFallen = rb.transform.position;
        var rotationFallen = rb.transform.rotation;
        var lerp = 0.0f;
        while(lerp < 1.0f)
		{
            lerp += (Time.deltaTime / this.tripGetupLength);
            rb.transform.position = Vector3.Lerp(positionFallen, positionReturn, Mathf.Clamp01(lerp));
            rb.transform.rotation = Quaternion.Slerp(rotationFallen, rotationReturn, Mathf.Clamp01(lerp));
            yield return null;
		}

        TripAction_End();
    }

    void TripAction_End()
	{
        this.currentAction = TAction.None;
		this.characterController.enabled = true;
        this.rigidbodyCollider.enabled = false;
        this.rigidbodyRoot.isKinematic = true;
        this.rigidbodyRoot.transform.position = positionReturn;
        this.rigidbodyRoot.transform.rotation = rotationReturn;
	}

	public void throwTantrum(int sfxIndex = 0)
    {
        sfxIndex = sfxIndex % tantrumSFX.Length;

        if (!HasThrownTantrum)
        {
            
            HasThrownTantrum = true;
            // stop "up!" sfx
            audioSource.Stop();
            CancelCurrentAction();

            // reset position if tantrum while jumping
            transform.position = new Vector3(transform.position.x, startYPos, transform.position.z);
            this.rigidbodyCollider.enabled = true;
            this.characterController.enabled = false;
            Rigidbody rb = this.rigidbodyRoot;
            //Rigidbody rbChild = GetComponentInChildren<Rigidbody>();
            rb.isKinematic = false;
            //rbChild.isKinematic = false;
            rb.AddForce(new Vector3(2, -1, 2), ForceMode.Impulse);
            print("playing index " + sfxIndex);
            AudioSource.PlayClipAtPoint(tantrumSFX[sfxIndex], this.transform.position);
        }

    }

    public void delayedTantrum(int sfxIndex = 0)
    {
        CancelCurrentAction();
        
        StartCoroutine(dTantrum(sfxIndex));
    }

    private IEnumerator dTantrum(int sfxIndex) {
        yield return new WaitForSeconds(UnityEngine.Random.value * 2.0f);
        throwTantrum(sfxIndex);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(targetPosition, 0.3f);
    }


    private void OnCollisionEnter(Collision collision)
    {
        print("collieded with" + collision.gameObject.name);
        if ((collision.gameObject.tag == "Crib" || collision.gameObject.tag == "Wall") && !HasThrownTantrum && this.currentAction == TAction.Moving)
        {
            CancelCurrentAction();
        }
    }

    private void CancelCurrentAction()
	{
        if (this.currentAction == TAction.None)
            return;
        
        switch(this.currentAction)
		{
            case TAction.Idle:
                IdleAction_End();
                break;
            case TAction.Jump:
                JumpAction_End();
                break;
            case TAction.Moving:
                MovingAction_End();
                break;
            case TAction.Spin:
                SpinAction_End();
                break;
            case TAction.Trip:
                TripAction_End();
                break;
            default:
                throw new System.Exception("Couldn't find: " + this.currentAction.ToString());
		}
        this.currentAction = TAction.None;
        StopAllCoroutines();
	}
}
