using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtrl : MonoBehaviour
{
    public static CameraCtrl instance;

    [HideInInspector]
    public Vector3 center;
    public Vector3 limtXY;
    private Coroutine c_shake;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {

        center = this.transform.position;


    }

    // Update is called once per frame
    void Update()
    {
     
    }

    private void LateUpdate()
    {
        if (Input.GetMouseButton(0))
        {

            var x = Input.GetAxis("Mouse X") * 15 * Time.deltaTime;
            var z = Input.GetAxis("Mouse Y") * 15 * Time.deltaTime;

            var eulerAngles = Camera.main.transform.localEulerAngles;

            eulerAngles.x = 0;

            var ro = Quaternion.Euler(eulerAngles);
            Camera.main.transform.Translate(ro * new Vector3(-x, 0, -z), Space.World);

        }


        var maxX = center.x + limtXY.x;
        var minX = center.x - limtXY.x;

        var minZ = center.z - limtXY.z;
        var maxZ = center.z + limtXY.z;
        var pos = this.transform.position;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        this.transform.position = pos;
    }
    //[ContextMenu("Shake")]

    public void Shake(float duration, float power = 1)
    {
        if (c_shake != null) StopCoroutine(c_shake);

         c_shake = StartCoroutine(c_Shake(duration, power));
    }

    /// <summary>
    /// 晃动镜头 晃动力量衰减
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    IEnumerator c_Shake(float duration,float power=1)
    {
        var startTime = Time.time;

        var y = this.transform.position.y;
        var org_pos = this.transform.position;
        var effectPos = this.transform.position;
        var floatValue = 0; 

        while (true)
        {

            var t = (Time.time - startTime) / duration;
            //晃动力量衰减
            effectPos.y = y + Mathf.Sin(floatValue)*(1-t)* power;
            floatValue += 1;
            this.transform.position = effectPos;

            if (t >= 1)
            {
                this.transform.position = org_pos;
                break;
            }
            yield return new WaitForEndOfFrame();
        }
    }


}
