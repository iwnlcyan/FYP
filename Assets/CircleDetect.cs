using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
/// <summary>
/// 圆形检测，并绘制出运行的攻击范围
/// </summary>
public class CircleDetect : MonoBehaviour {
 
    GameObject go;    //生成矩形的对象
    public Transform attack;        //被攻击方
    public float Radius;
    MeshFilter mf;
    MeshRenderer mr;
    Shader shader;
 
	void Start () {
		
	}
	
	void Update () {
        if (Input.GetKeyDown(KeyCode.A))
        {
            ToDrawCircleSolid(transform, transform.localPosition, Radius);
            if (CircleAttack(attack,transform,Radius))
            {
                Debug.Log("In the Range");
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
 
    /// <summary>
    /// 圆形检测
    /// </summary>
    /// <param name="attacked">被攻击者</param>
    /// <param name="skillPostion">技能的位置</param>
    /// <param name="radius">半径</param>
    /// <returns></returns>
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
 
    //生成网格
    public GameObject CreateMesh(List<Vector3> vertices)
    {
        int[] triangles;
        Mesh mesh = new Mesh();
        int triangleAmount = vertices.Count - 2;
        triangles = new int[3 * triangleAmount];
 
        //根据三角形的个数，来计算绘制三角形的顶点顺序
        //顺序必须为顺时针或者逆时针
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
        //分配一个新的顶点位置数组
        mesh.vertices = vertices.ToArray();
        //包含网格中所有三角形的数组
        mesh.triangles = triangles;
        mf.mesh = mesh;
        mr.material.shader = shader;
        mr.material.color = Color.red;
        return go;
 
    }
 
    /// <summary>
    /// 绘制实心圆形
    /// </summary>
    /// <param name="t">圆形参考物</param>
    /// <param name="center">圆心</param>
    /// <param name="radius">半径</param>
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
