using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeCollider : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.GetComponent<ProceduralTree>() != null)
        {
            Vector3 colPosition = transform.InverseTransformPoint(other.contacts[0].point);
            other.gameObject.GetComponent<ProceduralTree>().Deform(colPosition, -other.contacts[0].normal, 1.0f);
        }
    }

    private void OnTriggerStay(Collider other)
    {
    }

    void OnTriggerEnter(Collider other)
    {
    }
}
