using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(HangryController))]
public class ToddlerController : MonoBehaviour
{

    public float baseRunSpeed = 2.0f;
    public float baseTurnSpeed = 1.0f;
    [Range(0.0f, 1.0f)]
    [Tooltip("Higher makes toddler spin more often.")]
    public float spinningQuotient = 0.314f;
    [Range(0.0f, 1.0f)]
    [Tooltip("Higher makes toddler jump more often.")]
    public float jumpingQuotient = 0.6f;
    [Range(1.0f, 3.5f)]
    public float baseSpinSpeed = 1.5f;
    public float baseJumpSpeed = 4.0f;

    [SerializeField] private float movementRadius = 5.0f;
    [SerializeField] private float maxMovementDuration = 3.0f;
    [SerializeField] private PhysicsBaby physicsBabyPrefab = null;
    [SerializeField] private Rigidbody hipRootRigidbody = null;
    [SerializeField] private Transform leftLeg = null;
    [SerializeField] private Transform rightLeg = null;
    [SerializeField] private float legRange = 25.0f;
    [SerializeField] private float legSpeed = 4.0f;


    Vector3 targetPosition;
    bool alreadyMoving = false;
    bool isMoving = false;
    bool readyToRun = false;
    float lerpDuration = 0.5f;
    float startYPos;

    HangryController hc;
    CharacterController characterController;

    bool _beingGrabbed = false;
    public bool beingGrabbed
    {
        get { return _beingGrabbed;  }
        set
        {
            _beingGrabbed = value;
            if (_beingGrabbed) { alreadyMoving = false; }
            else
            {
                // reset position if grabbed while jumping
                transform.position = new Vector3(transform.position.x, startYPos, transform.position.z);
            }
            isMoving = false;
            StopAllCoroutines();
        }
    }

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
            return baseRunSpeed + 10 * hangryRatio;
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
        get { return (hc.getHangryLevel() / hc.maxHangry);  }
    }

    enum TAction
    {
        Spin,
        Tantrum,
        RunTowardTarget,
        TurnTowardTarget,
        Jump
    }

    public static ToddlerController Current { get; private set; }

    private void Awake()
    {
        Current = this;

        hc = GetComponent<HangryController>();
        characterController = GetComponent<CharacterController>();
        targetPosition = getRandomPositionNearToddler();
        beingGrabbed = false;

        if (physicsBabyPrefab != null)
        {
            var pb = Instantiate(physicsBabyPrefab);
            pb.hipJoint.connectedBody = this.hipRootRigidbody;
        }
    }

	private void OnDestroy()
	{
        if (Current == this)
            Current = null;
	}

	// Start is called before the first frame update
	void Start()
    {
        startYPos = transform.position.y;
    }

    void Update()
    {
        if(!alreadyMoving && !_beingGrabbed)
        {
            alreadyMoving = true;
            if (readyToRun)
            {
                readyToRun = false;
                if (oooSomethingElse)
                {
                    alreadyMoving = false;
                }
                else
                {
                    StartCoroutine(MoveTowardTarget(targetPosition));
                    //StartCoroutine(MoveToTarget(targetPosition));
                }
                
            }
            else
            {
                int index = UnityEngine.Random.Range(0, Enum.GetValues(typeof(TAction)).Length);
                TAction nextAction = (TAction)index;
                switch (nextAction)
                {
                    case TAction.RunTowardTarget:
                        alreadyMoving = false;
                        break;
                    case TAction.Spin:
                        if (UnityEngine.Random.value < spinningQuotient)
                        {
                            StartCoroutine(Spin(UnityEngine.Random.Range(1.0f, 2.0f)));
                        } else { alreadyMoving = false; }
                        break;
                    case TAction.Tantrum:
                        alreadyMoving = false;
                        break;
                    case TAction.TurnTowardTarget:
                        targetPosition = getRandomPositionNearToddler();
                        StartCoroutine(TurnTowardTarget(targetPosition));
                        break;
                    case TAction.Jump:
                        if (UnityEngine.Random.value < jumpingQuotient)
                        {
                            StartCoroutine(Jump());
                        }
                        else { alreadyMoving = false; }
                        
                        break;
                }
            }
        }

        UpdateLegs();
    }

    void UpdateLegs()
	{

        if (this.leftLeg != null && this.rightLeg != null)
        {
            float lerp = Mathf.Abs(Mathf.Sin(Time.time * Mathf.PI * this.legSpeed));
            if (!isMoving)
            {
                lerp = 0.5f;
            }
            float angle = Mathf.Lerp(-1 * this.legRange, this.legRange, lerp);

            this.leftLeg.localEulerAngles = new Vector3(angle, 0, 0);
            this.rightLeg.localEulerAngles = new Vector3(-1 * angle, 0, 0);
        }
	}


    Vector3 getRandomPositionNearToddler() {
        float randX = UnityEngine.Random.Range(-movementRadius, movementRadius);
        float randZ = UnityEngine.Random.Range(-movementRadius, movementRadius);
        Vector3 target = new Vector3(randX, transform.position.y, randZ);
        return target;
    }

    private IEnumerator MoveTowardTarget(Vector3 targetPos)
    {
        this.isMoving = true;
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
        this.isMoving = false;
        alreadyMoving = false;
    }

    private IEnumerator MoveToTarget(Vector3 targetPos)
    {
        print("Running");
        // pre-cache the initial position
        var startPos = transform.position;

        // using the given average velocity calculate how long the animation
        // shall take in total
        var distance = Vector3.Distance(startPos, targetPos);

        if (Mathf.Approximately(distance, 0))
        {
            Debug.LogWarning("Start and end position are equal!", this);
            // Allow the next routine to start now
            alreadyMoving = false;
            yield break;
        }

        var duration = distance / runSpeed;

        var timeElapsed = 0f;
        while (timeElapsed < duration)
        {
            var factor = timeElapsed / duration;

            factor = 1 - Mathf.Pow(1 - factor, 3);
            factor *= factor; //Mathf.SmoothStep(0, 1, factor);

            transform.position = Vector3.Lerp(startPos, targetPos, factor);

            yield return null;

            timeElapsed += Time.deltaTime;
        }

        transform.position = targetPos;

        alreadyMoving = false;
    }

    private IEnumerator Spin(float duration)
    {
        print("Spinning");
        float endTime = Time.realtimeSinceStartup + duration;

        while (Time.realtimeSinceStartup < endTime)
        {
            transform.Rotate(new Vector3(0, 1.0f * spinSpeed, 0));
            yield return null;
        }

        alreadyMoving = false;
    }

    private IEnumerator TurnTowardTarget(Vector3 targetPos)
    {
        print("Turning");
        
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

        readyToRun = true;
        alreadyMoving = false;
    }

    private IEnumerator Jump()
    {
        print("Jumping");
        Vector3 startPos = transform.position;
        var startTime = Time.time;
        while ((Time.time - startTime) < this.maxMovementDuration)
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

        transform.position = startPos;

        alreadyMoving = false;
    }

    public void throwTantrum()
    {
        StopAllCoroutines();
        alreadyMoving = true;
        // reset position if tantrum while jumping
        transform.position = new Vector3(transform.position.x, startYPos, transform.position.z);
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.AddForce(new Vector3(1, 0, 1), ForceMode.Impulse);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(targetPosition, 0.3f);
    }
}
