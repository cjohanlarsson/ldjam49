using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component will randomly spawn bottles in an XZ radius around its center
/// </summary>
public class BottleManager : MonoBehaviour
{
	public static BottleManager Current { get; private set; }

	public event System.Action<int> OnScoreChanged;
	public int Score { get; private set; }

	[SerializeField] Bottle bottlePrefab;
	[SerializeField] float spawnRadius = 10.0f;
	[SerializeField] float respawnDuration = 1.0f;

	public AudioSource audioSource;

	private void Awake()
	{
		Current = this;
	}

	private void OnDestroy()
	{
		if (Current == this)
			Current = null;
	}

	IEnumerator Start()
	{
		audioSource = this.GetComponent<AudioSource>();

		while (true)
		{
			var offset = Random.insideUnitCircle * this.spawnRadius;
			var bottle = Instantiate(bottlePrefab.gameObject, this.transform.position + new Vector3(offset.x, 0, offset.y), Quaternion.identity);
			audioSource.Play();
			yield return new WaitUntil(() => bottle == null);
			yield return new WaitForSeconds(this.respawnDuration);
		}
	}

	public void AddScore(int points)
	{
		this.Score += points;
		if (OnScoreChanged != null)
			OnScoreChanged(this.Score);
	}

}
