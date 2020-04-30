using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingEdge : MonoBehaviour
{
	public int endOne, endTwo;
	public Vector3 midpoint;

	public void Initialize(int index, int endOne, Vector3 midpoint, int endTwo)
	{
		this.endOne = endOne;
		this.midpoint = midpoint;
		this.endTwo = endTwo;
		transform.localPosition = midpoint;
		transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
		name = index.ToString()+" Edge "+transform.localPosition.ToString();
		GetComponent<Renderer>().material.color = Color.magenta;
		GetComponent<Renderer>().enabled = false;
	}
}
