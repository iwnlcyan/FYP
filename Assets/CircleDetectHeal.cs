using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class CircleDetectHeal : MonoBehaviour {
 
    GameObject go;    //Local object
    public Transform attack;        //detected target
    public float Radius;
    MeshFilter mf;
    MeshRenderer mr;
    Shader shader;

    void Start () {
		
	}
	
	void Update () {

        var idAttack = 0;

        if (Input.GetKeyDown(KeyCode.A))
        {
            ToDrawCircleSolid(transform, transform.localPosition, Radius);
            if (CircleAttack(attack,transform,Radius))
            {
                GameCtrl.instance.UseSkill(idAttack);
                //UICtrl.instance.skill_slotClick(5);
                Debug.Log("In of the Range");
            }
            else
            {
                Debug.Log("Out of the Range");
            }
        }
 
        if (Input.GetKeyUp(KeyCode.A))
        {
            if (go != null)
            {
                Destroy(go);
            }
        }
	}
 

    public bool CircleAttack(Transform attacked, Transform skillPostion, float radius)
    {
        float distance = Vector3.Distance(attacked.position, skillPostion.position);
        if (distance <= radius)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
 
    public GameObject CreateMesh(List<Vector3> vertices)
    {
        int[] triangles;
        Mesh mesh = new Mesh();
        int triangleAmount = vertices.Count - 2;
        triangles = new int[3 * triangleAmount];

        //Calculate the order of vertices for drawing triangles based on the number of trianglesv
        //The order must be clockwise or counterclockwise
        for (int i = 0; i < triangleAmount; i++)
        {
            triangles[3 * i] = 0;
            triangles[3 * i + 1] = i + 1;
            triangles[3 * i + 2] = i + 2;
        }
 
        if (go == null)
        {
            go = new GameObject("circle");
            go.transform.SetParent(transform, false);
            go.transform.position = new Vector3(0, -0.4f, 0);
 
            mf = go.AddComponent<MeshFilter>();
            mr = go.AddComponent<MeshRenderer>();
            shader = Shader.Find("Unlit/Color");
        }
        //Allocate a new array of vertex positions
        mesh.vertices = vertices.ToArray();
        //An array containing all triangles in the mesh
        mesh.triangles = triangles;
        mf.mesh = mesh;
        mr.material.shader = shader;
        mr.material.color = Color.red;
        return go;
 
    }
 
    public void ToDrawCircleSolid(Transform t, Vector3 center, float radius)
    {
        int pointAmount = 100;
        float eachAngle = 360f / pointAmount;
        Vector3 forward = t.forward;
 
        List<Vector3> vertices = new List<Vector3>();
        for (int i = 0; i < pointAmount; i++)
        {
            Vector3 pos = Quaternion.Euler(0f, eachAngle * i, 0f) * forward * radius + center;
            vertices.Add(pos);
        }
        CreateMesh(vertices);
    }
}
