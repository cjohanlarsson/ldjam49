using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIStartScreen : MonoBehaviour
{
	public static int NumberOfBabiesSelected = 1;

	public void StartLevel(int numberOfBabies)
	{
		NumberOfBabiesSelected = numberOfBabies;
		SceneManager.LoadScene("Main");
	}
}
