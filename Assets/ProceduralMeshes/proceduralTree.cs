using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class proceduralTree : MonoBehaviour {

	Mesh mesh;
    Material material;

    Random rnd;
	Vector3[] vertices;
    Vector2[] uv;
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

    int GenCircle (Vector3[] vertices, Vector2[] uv, int idx, float r, Vector3 n, Vector3 pos, int nbVert, float up)
    {
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

        float dv = 2 * Mathf.PI / nbVert;

        for (int i = 0; i < nbVert; i++)
        {
            Vector4 v = new Vector4(r * Mathf.Cos(dv * i), 0.0f, r * Mathf.Sin(dv * i), 1);
            v = mat * v;
            vertices[idx + i] = new Vector3(v.x, v.y, v.z);
            uv[idx + i] = new Vector2(4.0f * i / nbVert, up);
        }
        return idx + nbVert;
    }
    /**
     * start1Offset : index to the first vertex of the first circle
     * start2Offset : index to the first vertex of the second circle
     */
    int ConnectCircles (int idx, int offset1, int offset2, int length)
    {
        //int[] triangles = new int[length * 3 * 2];
        int i, j;
        for (i = 0, j = 0; i < 6 * (length - 1); i += 6, j++)
        {
            triangles[idx + i] = offset2 + j;
            triangles[idx + i + 1] = offset1 + j + 1;
            triangles[idx + i + 2] = offset1 + j;

            triangles[idx + i + 3] = offset2 + j;
            triangles[idx + i + 4] = offset2 + j + 1;
            triangles[idx + i + 5] = offset1 + j + 1;
        }

        triangles[idx + i] = offset2 + length - 1;
        triangles[idx + i + 1] = offset1;
        triangles[idx + i + 2] = offset1 + length - 1;

        triangles[idx + i + 3] = offset2 + length - 1;
        triangles[idx + i + 4] = offset2;
        triangles[idx + i + 5] = offset1;

        return idx + length * 6;
    }

    Vector3 PerpVector (Vector3 dir)
    {
        Vector3 v = new Vector3(Random.Range(-1.0f, 1.0f),
                                Random.Range(-1.0f, 1.0f),
                                Random.Range(-1.0f, 1.0f));
 
        return Vector3.Normalize (Vector3.Cross(v, dir));
    }

    Vector3 GenNormal (Vector3 dir, float alpha, float beta)
    {
        Vector3 up = new Vector3(0, 1, 0);
        Vector3 t = PerpVector (dir);
        t = alpha * dir + (1.0f - alpha) * t;
        return beta * up + (1.0f - beta) * t;
    }

    /*
     * r is radius
     * l is length 
     * alpha influences tendencies to goes alongside branch dir
     * beta influences tendencies to goes upward
     * v = v coordinate of uv
    */
    int GenTree (int baseOffet, float r, float l, float alpha, float beta, Vector3 n, Vector3 pos, int vert, float v)
    {
        float rnd = Random.Range(0.0f, 1.0f);

        if (r < 0.2f)
        {
            return baseOffet + vert;
        }
        else if (rnd < 0.25f)
        {
            Vector3 n1 = GenNormal(n, alpha, beta);
            Vector3 n2 = GenNormal(n, alpha, beta);

            //To be sure that they don't go both to the same direction
            while (Vector3.Dot(n1, n2) > 0.92)
            {
                n2 = GenNormal(n, alpha, beta);
            }

            float d1 = l * Random.Range(0.8f, 1.2f);
            float d2 = l * Random.Range(0.8f, 1.2f);
            Vector3 pos1 = pos + n1 * d1;
            Vector3 pos2 = pos + n2 * d2;

            r *= 0.8f;
            alpha *= 1.0f;
            beta *= 0.95f;
  

            nbVertices = GenCircle(vertices, uv, nbVertices, r, n1, pos1, vert, v);
            nbTriangles = ConnectCircles(nbTriangles, baseOffet, baseOffet + vert, vert);

            int offset = GenTree(baseOffet + vert, r, l, alpha, beta, n1, pos1, vert, v + d1 * 0.5f);

            nbVertices = GenCircle(vertices, uv, nbVertices, r, n2, pos2, vert, v);
            nbTriangles = ConnectCircles(nbTriangles, baseOffet, offset, vert);

            return GenTree(offset, r, l, alpha, beta, n2, pos2, vert, v + d2 * 0.5f);
        }
        else
        {
            Vector3 n_new = GenNormal(n, alpha, beta);
            float d = l * Random.Range(0.8f, 1.2f);
            Vector3 new_pos = pos + n_new * d;

            r *= 0.9f;
            alpha *= 1.0f;
            beta *= 0.8f;
 
            nbVertices = GenCircle(vertices, uv, nbVertices, r, n_new, new_pos, vert, v);
            nbTriangles = ConnectCircles(nbTriangles, baseOffet, baseOffet + vert, vert);

            return GenTree(baseOffet + vert, r, l, alpha, beta, n_new, new_pos, vert, v + d * 0.5f);
        }
    }

    void MakeMeshData()
    {
        vertices = new Vector3[6000];
        uv = new Vector2[6000];
        triangles = new int[21000];
        
        nbVertices = 0;
        nbTriangles = 0;

        nbVertices = GenCircle(vertices, uv, nbVertices, 3.0f, new Vector3(0, 1, 0), new Vector3(0, 0, 0), 8, 0.0f);
        GenTree(0, 2.0f, 8f, 0.8f, 0.45f, new Vector3(0, 1, 0), new Vector3(0, 0, 0), 8, 4.0f);
    }

    void CreateMesh()
    { 
		mesh.Clear ();
        Vector2[] mesh_uv = new Vector2[nbVertices];
        Vector3[] mesh_vertices = new Vector3[nbVertices];
		int[] mesh_triangles = new int[nbTriangles];

        for (int i = 0; i < nbTriangles; i++)
            mesh_triangles[i] = triangles[i];

        for (int i = 0; i < nbVertices; i++)
        {
            mesh_vertices[i] = vertices[i];
            mesh_uv[i] = uv[i];
        }

        mesh.vertices = mesh_vertices;
        mesh.triangles = mesh_triangles;
        mesh.uv = mesh_uv;
    }
		
}
