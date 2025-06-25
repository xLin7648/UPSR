using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyThis : MonoBehaviour
{
    public float length;

    public float time;

    // Update is called once per frame
    void Update()
    {
        if (time >= length) 
        {
            Destroy(this.gameObject);
        }
        time += Time.deltaTime;
    }
}
