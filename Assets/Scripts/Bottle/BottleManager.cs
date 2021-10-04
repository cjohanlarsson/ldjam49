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

	// public AudioSource audioSource;
	int previousBottleIndex = 0;

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
		// audioSource = this.GetComponent<AudioSource>();

		while (true)
		{
			Vector3 spawnPoint = Vector3.zero;

			if (this.transform.childCount == 0)
			{
				var offset = Random.insideUnitCircle * this.spawnRadius;
				spawnPoint = this.transform.position + new Vector3(offset.x, 0, offset.y);
			}
			else
			{
				this.previousBottleIndex = (this.previousBottleIndex + Random.Range(1, this.transform.childCount)) % this.transform.childCount;
				spawnPoint = this.transform.GetChild( this.previousBottleIndex ).position;
			}

			var bottle = Instantiate(bottlePrefab.gameObject, spawnPoint, Quaternion.identity);
			// audioSource.Play();
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

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.cyan;
		foreach (Transform t in this.transform)
		{
			Gizmos.DrawWireCube(t.position , new Vector3(0.1f, 0.1f, 0.1f));
			Gizmos.DrawCube(t.position + (Vector3.up * 0.5f), new Vector3(0.2f, 0.5f, 0.2f));
		}
	}

}
