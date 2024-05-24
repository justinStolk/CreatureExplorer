using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerInRadius : MonoBehaviour
{
    public GameObject playerIsInRadius;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void onTriggerEnter(Collider other)
    {
        playerIsInRadius = this.transform.GetChild(0).gameObject;
        playerIsInRadius.SetActive(true);
    }
}
