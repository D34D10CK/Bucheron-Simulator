using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct meshdata
{
    public Vector3[] vertices;
    public int[] triangles;
}

[RequireComponent(typeof(MeshFilter))]
public class proceduralTree : MonoBehaviour {

	Mesh mesh;
    Random rnd;
	Vector3[] vertices;
    int nbVertices;
	int[] triangles;
    int nbTriangles;

	void Awake ()
	{
		mesh = GetComponent<MeshFilter>().mesh;
	}
	// Use this for initialization
	void Start () {
		MakeMeshData ();
        CreateMesh();
    }

    Vector3[] GenCircle (float r, float h, Vector3 n, Vector3 pos, int nbVertices)
    {
        Vector3[] vertices;

        n.Normalize();
        Vector3 front = new Vector3(0, 0, 1);
        Vector3 right = Vector3.Cross(front, n).normalized;
        front = Vector3.Cross(right, n);

        Matrix4x4 mat = new Matrix4x4(new Vector4(right.x, right.y, right.z, 0),
                                      new Vector4(n.x, n.y, n.z, 0.0f),
                                      new Vector4(front.x, front.y, front.z, 0),
                                      new Vector4(0, 0, 0, 1));
        mat = mat.transpose;
        mat = Matrix4x4.Translate(pos) * mat;

        vertices = new Vector3[nbVertices];

        float dv = 2 * Mathf.PI / nbVertices;

        for (int i = 0; i < nbVertices; i++)
        {
            Vector4 v = new Vector4(r * Mathf.Cos(dv * i), 0.0f, r * Mathf.Sin(dv * i), 1);
            v = mat * v;
            vertices[i] = new Vector3(v.x, v.y, v.z);
        }

        return vertices;
    }
    /**
     * start1Offset : index to the first vertex of the first circle
     * start2Offset : index to the first vertex of the second circle
     */
    int[] ConnectCircles (int offset1, int offset2, int length)
    {
        int[] triangles = new int[length * 3 * 2];
        int i, j;
        for (i = 0, j = 0; i < 6 * (length - 1); i += 6, j++)
        {
            triangles[i] = offset2 + j;
            triangles[i + 1] = offset1 + j + 1;
            triangles[i + 2] = offset1 + j;

            triangles[i + 3] = offset2 + j;
            triangles[i + 4] = offset2 + j + 1;
            triangles[i + 5] = offset1 + j + 1;
        }

        triangles[i] = offset2 + length - 1;
        triangles[i + 1] = offset1;
        triangles[i + 2] = offset1 + length - 1;

        triangles[i + 3] = offset2 + length - 1;
        triangles[i + 4] = offset2;
        triangles[i + 5] = offset1;

        return triangles;
    }

    meshdata GenCylinder (float r, float h, Vector3 n, Vector3 pos, int nbVertices)
    {
        Vector3[] vertices;
        int[] triangles;

        n.Normalize();
        Vector3 front = new Vector3(0, 0, 1);
        Vector3 right = Vector3.Cross(front, n).normalized;
        front = Vector3.Cross(right, n);

        Matrix4x4 mat = new Matrix4x4(new Vector4(right.x, right.y, right.z, 0), 
                                      new Vector4(n.x, n.y, n.z, 0.0f),
                                      new Vector4(front.x, front.y, front.z, 0),
                                      new Vector4(0, 0, 0, 1));

        mat = mat.transpose;
        mat = Matrix4x4.Translate (pos) * mat;


        vertices = new Vector3[nbVertices];
        triangles = new int[nbVertices * 3];

        float dv = 4 * Mathf.PI / nbVertices;
        int i, j;

        for (i = 0; i < nbVertices/2; i++)
        {
            Vector4 v = new Vector4 (r * Mathf.Cos(dv * i), 0.0f, r * Mathf.Sin(dv * i), 1);
            v = mat * v;
            vertices[i] = new Vector3(v.x, v.y, v.z);
        }
        for (i = nbVertices / 2; i < nbVertices; i++)
        {
            Vector4 v = new Vector4 (r * Mathf.Cos(dv * i), h, r * Mathf.Sin(dv * i), 1);
            v = mat * v;
            vertices[i] = new Vector3(v.x, v.y, v.z);
        }

        for (i = 0, j = 0; i < 3 * (nbVertices - 2); i += 6, j++)
        {
            triangles[i+2] = j;
            triangles[i + 1] = j + 1;
            triangles[i] = nbVertices / 2 + j;

            triangles[i + 5] = j + 1;
            triangles[i + 4] = nbVertices / 2 + j + 1;
            triangles[i + 3] = nbVertices / 2 + j;
        }
        triangles[i + 2] = nbVertices / 2 - 1;
        triangles[i + 1] = 0;
        triangles[i] = nbVertices - 1;

        triangles[i + 5] = 0;
        triangles[i + 4] = nbVertices / 2;
        triangles[i + 3] = nbVertices - 1;

        meshdata m;
        m.vertices = vertices;
        m.triangles = triangles;
        return m;
    }

    int GenTree (int baseOffet, float r, Vector3 n, Vector3 pos, int nbVertices)
    {
        float theta = Random.Range(0.0f, 0.78f);

        if (r < 0.1)
        {
            return baseOffet;
        }
        else if (theta > 0.3f)
        {
            Vector3 new_n = n;
            Vector3 new_pos = pos;

            Vector3[] circle = GenCircle(r, r*5, new_n, new_pos, nbVertices);
            circle.CopyTo(vertices, nbVertices);
            nbVertices += nbVertices;

            int[] index = ConnectCircles(baseOffet, baseOffet + nbVertices, nbVertices);
            index.CopyTo(triangles, nbTriangles);
            nbTriangles += index.Length;

            int offset = GenTree(baseOffet + 8, r * 0.8f, new_n, new_pos, nbVertices);

            circle = GenCircle(r, r*5, new_n, new_pos, nbVertices);
            circle.CopyTo(vertices, nbVertices);
            nbVertices += nbVertices;

            index = ConnectCircles(baseOffet, offset, nbVertices);
            index.CopyTo(triangles, nbTriangles);
            nbTriangles += index.Length;

            return GenTree(offset, r * 0.8f, new_n, new_pos, nbVertices);
        }
        else
        {
            Vector3 new_n = n;
            Vector3 new_pos = pos;

            Vector3[] circle = GenCircle(r, r*5, new_n, new_pos, nbVertices);
            circle = GenCircle(r, r * 5, new_n, new_pos, nbVertices);
            circle.CopyTo(vertices, nbVertices);
            nbVertices += nbVertices;

            int[] index = ConnectCircles(baseOffet, baseOffet + nbVertices, nbVertices);
            index.CopyTo(triangles, nbTriangles);
            nbTriangles += index.Length;

            return GenTree(baseOffet, r * 0.8f, new_n, new_pos, nbVertices);
        }
    }

    void MakeMeshData() {
        vertices = new Vector3[1000];
        triangles = new int[1000];

        Vector3[] circle1 = GenCircle(1, 4, new Vector3(0, 1, 0), new Vector3(0, 0, 0), 8);
        Vector3[] circle2 = GenCircle(1, 4, new Vector3(0, 1, 0), new Vector3(0, 2, 0), 8);
        Vector3[] circle3 = GenCircle(0.5f, 4, new Vector3(1, 1, 0), new Vector3(4, 4, 0), 8);
        Vector3[] circle4 = GenCircle(0.5f, 4, new Vector3(-1, 1, 0), new Vector3(-4, 4, 0), 8);

        circle1.CopyTo(vertices, 0);
        circle2.CopyTo(vertices, circle1.Length);
        circle3.CopyTo(vertices, circle1.Length + circle2.Length);
        circle4.CopyTo(vertices, circle1.Length + circle2.Length + circle3.Length);

        int[] t1 = ConnectCircles(0, 8, 8);
        int[] t2 = ConnectCircles(8, 16, 8);
        int[] t3 = ConnectCircles(8, 24, 8);
        
        t1.CopyTo(triangles, 0);
        t2.CopyTo(triangles, t1.Length);
        t3.CopyTo(triangles, t1.Length + t2.Length);

        nbVertices = circle1.Length + circle2.Length + circle3.Length + circle4.Length;
        nbTriangles = t1.Length + t2.Length + t3.Length;
    }

	void CreateMesh() { 
		mesh.Clear ();
        Vector3[] mesh_vertices = new Vector3[nbVertices];
		int[] mesh_triangles = new int[nbTriangles];

        for (int i = 0; i < nbTriangles; i++)
            mesh_triangles[i] = triangles[i];

        for (int i = 0; i < nbVertices; i++)
            mesh_vertices[i] = vertices[i];

        mesh.vertices = mesh_vertices;
        mesh.triangles = mesh_triangles;
    }
		
}
