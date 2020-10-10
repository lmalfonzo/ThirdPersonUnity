using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialChanger : MonoBehaviour
{

    public Material[] materials;

    public int size;

    Renderer rend;
    // Start is called before the first frame update
    void Start()
    {
        size = 0;
        rend = GetComponent<Renderer>();
        rend.enabled = true;
        rend.sharedMaterial = materials[size];
    }

    void OnValidate() //TODO find out how to trigger this on boxcast (and untrigger this on boxcast out)
    {
        rend.sharedMaterial = materials[size];
    }

}
    
