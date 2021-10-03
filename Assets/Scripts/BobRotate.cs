using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BobRotate : MonoBehaviour
{
	public enum LerpType
	{
		None,
		Sinusoidal,
		Linear
	}
	public LerpType bobType;
	public Vector3 bobAmount = Vector3.up;
	public float bobSpeed = 1;
	public Vector3 rotationDegreesPerSecond = Vector3.zero;

	[SerializeField] private Vector3 rotationAdditionalRange;

	[SerializeField] private int chanceToFlipXRotation;
	[SerializeField] private int chanceToFlipYRotation;
	[SerializeField] private int chanceToFlipZRotation;

	public LerpType scaleType;
	public Vector3 scaleAmount;
	public float scaleSpeed;

	private Vector3 startPosition;
	private Vector3 startScale;
	private float startTime;

	void Awake()
	{
		startPosition = this.transform.localPosition;
		startScale = this.transform.localScale;
		this.rotationDegreesPerSecond += new Vector3(
			Random.Range(-1 * rotationAdditionalRange.x, rotationAdditionalRange.x),
			Random.Range(-1 * rotationAdditionalRange.y, rotationAdditionalRange.y),
			Random.Range(-1 * rotationAdditionalRange.z, rotationAdditionalRange.z)
		);
		this.rotationDegreesPerSecond.x *= Random.Range(0, 100) < chanceToFlipXRotation ? -1 : 1;
		this.rotationDegreesPerSecond.y *= Random.Range(0, 100) < chanceToFlipYRotation ? -1 : 1;
		this.rotationDegreesPerSecond.z *= Random.Range(0, 100) < chanceToFlipZRotation ? -1 : 1;
	}

	void Start()
	{
		startTime = Time.time;
	}

	static float LerpFunc(LerpType typ, float t, float speed)
	{
		switch (typ)
		{
			case LerpType.None:
				return 0f;
			case LerpType.Linear:
				return Mathf.PingPong((speed * t) + 0.5f, 1f) * 2f - 1f;
			case LerpType.Sinusoidal:
				return Mathf.Sin(speed * t);
		}

		throw new System.Exception("BobFunc, No type found: " + typ.ToString());
	}

	public void Update()
	{
		if (scaleType != LerpType.None)
			transform.localScale = startScale + (LerpFunc(scaleType, Time.time - startTime, scaleSpeed) * scaleAmount);

		transform.localPosition = startPosition + (LerpFunc(bobType, Time.time - startTime, bobSpeed) * bobAmount);
		transform.Rotate(rotationDegreesPerSecond * Time.deltaTime);
	}
}
