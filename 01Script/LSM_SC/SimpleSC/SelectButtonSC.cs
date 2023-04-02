using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectButtonSC : MonoBehaviour
{
	Button myButton;

	private void Start()
	{
		myButton = GetComponent<Button>();
		myButton.onClick.AddListener(GameManager.Instance.mainPlayer.SelectPlayerMinion);
	}
}
