using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingVertex : MonoBehaviour
{
	public bool filled = false;
	public Dictionary<int, GameObject> edges;

	public void Initialize(int index, Vector3 pos)
	{
		edges = new Dictionary<int, GameObject>();
		transform.localPosition = pos;
		transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
		name = index.ToString()+" Vertex "+transform.localPosition.ToString();
		GetComponent<Renderer>().material.color = Color.black;
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit) && hit.collider.gameObject.name == name)
			{
				ToggleState();
				transform.parent.GetComponent<MarchingPolyhedron>().CalculateMesh();
			}
		}
	}

	public void ToggleState()
	{
		filled = !filled;
		GetComponent<Renderer>().material.color = filled ? Color.white : Color.black;
		foreach (KeyValuePair<int, GameObject> entry in edges)
			entry.Value.GetComponent<Renderer>().enabled = !entry.Value.GetComponent<Renderer>().enabled;
		//StartCoroutine( transform.parent.GetComponent<MarchingPolyhedron>().RecreateMesh() ) ;
	}
}
