using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialChanger : MonoBehaviour
{
    public Material[] materials;

    public int index;

    bool isTargeted;

    Renderer rend;
    // Start is called before the first frame update
    void Start()
    {
        index = 0;
        rend = GetComponent<Renderer>();
        rend.enabled = true;
        rend.sharedMaterial = materials[index];
        isTargeted = false;
    }

    void Update() //TODO find out how to trigger this on boxcast (and untrigger this on boxcast out)
    {
        index = isTargeted ? index : 0;

        rend.sharedMaterial = materials[index];

        isTargeted = false;
    }

    public void SwitchMaterial(int index)
    {
        /*
        if (materials.Length > 1)
        {
            index++;
            if (index > materials.Length)
            {
                index = 0;
            }
        }
        */
        this.index = index;
        isTargeted = true;
    }

}
    
