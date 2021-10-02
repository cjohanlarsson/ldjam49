using System;
using System.Collections;
using UnityEngine;

public class ToddlerController : MonoBehaviour
{
    public float runSpeed = 2.0f;
    public float turnSpeed = 100.0f;
    Vector3 targetPosition;
    bool alreadyMoving = false;
    bool readyToRun = false;
    float lerpDuration = 0.5f;

    enum TAction
    {
        spin,
        tantrum,
        runTowardTarget,
        turnTowardTarget
    }

    private void Awake()
    {
        targetPosition = getRandomPositionNearToddler();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        if(!alreadyMoving)
        {
            alreadyMoving = true;
            if (readyToRun)
            {
                readyToRun = false;
                StartCoroutine(MoveToTarget(targetPosition));
            }
            else
            {
                int index = UnityEngine.Random.Range(0, Enum.GetValues(typeof(TAction)).Length);
                TAction nextAction = (TAction)index;
                switch (nextAction)
                {
                    case TAction.runTowardTarget:
                        alreadyMoving = false;
                        //StartCoroutine(MoveToTarget(targetPosition));
                        break;
                    case TAction.spin:
                        StartCoroutine(Spin(UnityEngine.Random.Range(1.0f, 2.0f)));
                        break;
                    case TAction.tantrum:
                        alreadyMoving = false;
                        break;
                    case TAction.turnTowardTarget:
                        targetPosition = getRandomPositionNearToddler();
                        StartCoroutine(TurnTowardTarget(targetPosition));
                        break;
                }
            }
        }
    }


    Vector3 getRandomPositionNearToddler() {
        float randX = transform.position.x + UnityEngine.Random.Range(-3.0f, 3.0f);
        float randZ = transform.position.y + UnityEngine.Random.Range(-3.0f, 3.0f);
        Vector3 target = new Vector3(randX, transform.position.y, randZ);
        return target;
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

        var timePassed = 0f;
        while (timePassed < duration)
        {
            var factor = timePassed / duration;
            factor = Mathf.SmoothStep(0, 1, factor);

            transform.position = Vector3.Lerp(startPos, targetPos, factor);

            yield return null;

            timePassed += Time.deltaTime;
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
            transform.Rotate(new Vector3(0, 1.0f, 0));
            yield return null;
        }

        alreadyMoving = false;
    }

    private IEnumerator TurnTowardTarget(Vector3 targetPos)
    {
        print("Turning");
        float timeElapsed = 0;
        Quaternion startRotation = transform.rotation;
        Vector3 direction = (targetPos - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        while (timeElapsed < lerpDuration) {
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, timeElapsed / lerpDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;

        readyToRun = true;
        alreadyMoving = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(targetPosition, 0.3f);
    }
}
