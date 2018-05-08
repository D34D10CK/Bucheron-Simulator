using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class ProceduralTree : MonoBehaviour {

	Mesh mesh;
    Random rnd;
	Vector3[] vertices;
    Vector2[] uv;
    int nbVertices;
	int[] triangles;
    int nbTriangles;
    float deformation;

    void Awake ()
	{
		mesh = GetComponent<MeshFilter>().mesh;
    }
	// Use this for initialization
	void Start () {
		MakeMeshData ();
        CreateMesh();
        deformation = 0;
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
    int GenTree (int baseOffet, float r, float l, float alpha, float beta, float gamma, Vector3 n, Vector3 pos, int nb_vert, float v)
    {
        float rnd = Random.Range(0.0f, 1.0f);

        if (r < 0.02f)
        {
            return baseOffet + nb_vert;
        }
        else if (rnd < gamma)
        {
            Vector3 n1 = GenNormal(n, alpha, beta);
            Vector3 n2 = GenNormal(n, alpha, beta);

            //To be sure that they don't go both to the same direction
            while (Vector3.Dot(n1, n2) > 0.92) { n2 = GenNormal(n, alpha, beta); }

            float d1 = l * Random.Range(0.8f, 1.2f);
            float d2 = l * Random.Range(0.8f, 1.2f);
            Vector3 pos1 = pos + n1 * d1;
            Vector3 pos2 = pos + n2 * d2;

            l *= 1.1f;
            r *= 0.8f;
            beta *= 0.95f;
            gamma *= 0.9f;

            int offset = 0;
            if (r < 0.2f)
            {
                //First Branch
                nbVertices = GenCircle(vertices, uv, nbVertices, r, n1, pos1, nb_vert, v);
                nbTriangles = ConnectCircles(nbTriangles, baseOffet, baseOffet + nb_vert, nb_vert);
                offset = GenTree(baseOffet + nb_vert, r, l, alpha, beta, gamma, n1, pos1, nb_vert, v + d1 * 5.0f);

                //Second Branch
                nbVertices = GenCircle(vertices, uv, nbVertices, r, n2, pos2, nb_vert, v);
                nbTriangles = ConnectCircles(nbTriangles, baseOffet, offset, nb_vert);
                return GenTree(offset, r, l, alpha, beta, gamma, n2, pos2, nb_vert, v + d2 * 5f);
            }
            else
            {
                //First branch
                Vector3 p = (pos1 - pos) / 10f;
                for (int i = 0; i < 10; i++)
                {
                    nbVertices = GenCircle(vertices, uv, nbVertices, r, n1, pos + i * p, nb_vert, v + d1 * i*0.5f);
                    nbTriangles = ConnectCircles(nbTriangles, baseOffet + nb_vert * i, baseOffet + nb_vert * (i+1), nb_vert);
                }
                offset = GenTree(baseOffet + 10 * nb_vert, r, l, alpha, beta, gamma, n1, pos1, nb_vert, v + d1 * 5f);

                //Second Branch
                p = (pos2 - pos) / 10f;
                nbVertices = GenCircle(vertices, uv, nbVertices, r, n2, pos + p, nb_vert, v);
                nbTriangles = ConnectCircles(nbTriangles, baseOffet, offset, nb_vert);

                for (int i = 1; i < 10; i++)
                {
                    nbVertices = GenCircle(vertices, uv, nbVertices, r, n2, pos + i * p, nb_vert, v + d2 * i * 0.5f);
                    nbTriangles = ConnectCircles(nbTriangles, offset + nb_vert * (i-1), offset + nb_vert * i, nb_vert);
                }
                return GenTree(offset + 9 * nb_vert, r, l, alpha, beta, gamma, n2, pos2, nb_vert, v + d2 * 5f);
            }

        }
        else
        {
            Vector3 n_new = GenNormal(n, 0.9f, beta);
            float d = l * Random.Range(0.7f, 1.3f);
            Vector3 new_pos = pos + n_new * d;

            l *= 0.9f;
            r *= 0.80f;
            beta *= 0.8f;
            gamma *= 1.6f;

            if (r < 0.2f)
            {
                nbVertices = GenCircle(vertices, uv, nbVertices, r, n_new, new_pos, nb_vert, v);
                nbTriangles = ConnectCircles(nbTriangles, baseOffet, baseOffet + nb_vert, nb_vert);
                return GenTree(baseOffet + nb_vert, r, l, alpha, beta, gamma, n_new, new_pos, nb_vert, v + d * 5f);
            }

            Vector3 p = (new_pos - pos) / 10f;
            for (int i = 0; i < 10; i++)
            {
                nbVertices = GenCircle(vertices, uv, nbVertices, r, n_new, pos + i * p, nb_vert, v + d * i * 0.5f);
                nbTriangles = ConnectCircles(nbTriangles, baseOffet + nb_vert * i, baseOffet + nb_vert * (i+1), nb_vert);
            }
            return GenTree(baseOffet + 10 * nb_vert, r, l, alpha, beta, gamma, n_new, new_pos, nb_vert, v + d * 5f);
        }
    }

    void MakeMeshData()
    {
        vertices = new Vector3[6000];
        uv = new Vector2[6000];
        triangles = new int[32000];
        
        nbVertices = 0;
        nbTriangles = 0;

        nbVertices = GenCircle (vertices, uv, nbVertices, 0.35f, new Vector3(0, 1, 0), new Vector3(0, 0, 0), 8, 0.0f);
        GenTree (0, 0.35f, 1.8f, 0.55f, 0.35f, 0.15f, new Vector3(0, 1, 0), new Vector3(0, 0, 0), 8, 4.0f);
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

        vertices = mesh_vertices;
        triangles = mesh_triangles;
        uv = mesh_uv;
    }

    public CuttedTree Prefab;

    public void Deform(Vector3 impactPosition, Vector3 normal, float coeff)
    {
        float max = 0;
        int max_i = 0;

        for (int i = 0; i < nbVertices; i++)
        {
            float dist = (impactPosition - vertices[i]).magnitude;
            if (dist > 1.0)
                continue;
            float d = 1.0f + dist;
            d = coeff / Mathf.Exp(d * d * d * d);
            vertices[i] += normal * d;
            if (d > max)
            {
                max = d;
                max_i = i;
            }
        }

        deformation += max;
        mesh.vertices = vertices;
        
        if (deformation > 1)
        {
            while (max_i % 3 != 0) { max_i = max_i - 1; }

            print("Instanciate Cutted Tree");
            print(max_i);

            Mesh downPart = new Mesh();
            Mesh upPart = new Mesh();

            List<Vector3> downVertices = new List<Vector3>();
            List<Vector2> downUV = new List<Vector2>();
            List<int> downTriangles = new List<int>();

            int i = 0;
            for (i = 0; i < max_i; i++)
            {
                downVertices.Add(vertices[i]);
                downUV.Add(uv[i]);
                vertices[i] = vertices[max_i];
                uv[i] = uv[max_i];
            }

            i = 0;
            while (triangles[i] < max_i)
            {
                downTriangles.Add(triangles[i]);
                i++;
            }
            while (i % 3 != 0)
            {
                downTriangles.RemoveAt(downTriangles.Count - 1);
                i--;
            }

            downPart.SetVertices(downVertices);
            downPart.SetUVs(0, downUV);
            downPart.SetTriangles(downTriangles, 0);

            upPart.vertices = vertices;
            upPart.uv = uv;
            upPart.triangles = triangles;

            CuttedTree tree;
            tree = Instantiate(Prefab, transform.parent) as CuttedTree;
            tree.SetMesh(upPart);
            tree.GetComponent<Rigidbody>().velocity = new Vector3(1, 0, 0);
            tree.GetComponent<Rigidbody>().ResetCenterOfMass();

            CuttedTree treeDown;
            treeDown = Instantiate(Prefab, transform.parent) as CuttedTree;
            treeDown.SetMesh(downPart);
            treeDown.GetComponent<CapsuleCollider>().height = 2;
            treeDown.GetComponent<CapsuleCollider>().transform.position = treeDown.transform.position;
            treeDown.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;

            Destroy(mesh);
            Destroy(GetComponent<Rigidbody>());
            Destroy(GetComponent<CapsuleCollider>());

        }

    }

    void OnCollisionEnter(Collision collision)
    {
        Mesh mesh = this.GetComponent<MeshFilter>().mesh;
        Debug.Log(mesh.vertexCount);

        foreach (ContactPoint contact in collision.contacts)
        {
            Debug.Log(contact.normal);
            Debug.Log(contact.point - transform.position);
        }
    }
}
