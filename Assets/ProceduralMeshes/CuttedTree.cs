using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttedTree : MonoBehaviour {

    Mesh mesh;

    public void SetMesh(Mesh m)
    {
        mesh.vertices = m.vertices;
        mesh.uv = m.uv;
        mesh.triangles = m.triangles;
    }

    void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    // Use this for initialization
    void Start()
    {

    }

	// Update is called once per frame
	void Update () {
		
	}
}
