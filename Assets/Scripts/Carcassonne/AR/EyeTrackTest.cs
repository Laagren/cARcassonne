using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeTrackTest : MonoBehaviour
{
    MeshRenderer r;
    void Start()
    {
        r = GetComponent<MeshRenderer>();
    }

    public void OnLookAt()
    {
        r.material.color = Color.green;
    }

    public void OffLookAt()
    {
        r.material.color = Color.white;
    }
}
