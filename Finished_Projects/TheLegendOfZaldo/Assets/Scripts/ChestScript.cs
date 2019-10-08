﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fungus;

public class ChestScript : MonoBehaviour
{
    public GameObject playerObject;
    private bool opened = false;
    public Flowchart flowchart;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (opened == false)
            {
                playerObject.GetComponent<PlayerScript>().setSwordActive();
                flowchart.ExecuteBlock("SwordChest");
                opened = true;
            }
            else
            {
                flowchart.ExecuteBlock("opened");
            }
        }
    }
}
