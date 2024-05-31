using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ramTree : MonoBehaviour
{
    public GameObject Track_walk_gameObject;
    public GameObject Track_ramTree_gameObject;
    Cinemachine.CinemachinePathBase Track_walk;
    Cinemachine.CinemachinePathBase Track_ramTree;
    public GameObject DollyCart_Charger;
    private Animator anim;

    Cinemachine.CinemachinePathBase Track_current;
    bool walkPathCompleted;
    bool ramPathCompleted;
    float CartPosition;
    float CartSpeed;

    // Start is called before the first frame update
    void Start()
    {
        Track_walk = Track_walk_gameObject.GetComponent<CinemachinePathBase>();
        Track_ramTree = Track_ramTree_gameObject.GetComponent<CinemachinePathBase>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Track_current = DollyCart_Charger.GetComponent<CinemachineDollyCart>().m_Path;
        CartPosition = DollyCart_Charger.GetComponent<CinemachineDollyCart>().m_Position;
        CartSpeed = DollyCart_Charger.GetComponent<CinemachineDollyCart>().m_Speed;

        //play animations accordingly
        if (Track_current == Track_walk)
        {
            if(CartSpeed > 0)
            {
                AnimationLoop("Walk");
            } else
            {
                AnimationLoop("Neutral");
            }
        }

        //standard behavior
        if(CartPosition > 257)
        {
            walkPathCompleted = true;
        }

        if(walkPathCompleted == true)
        {
            Track_current = Track_ramTree;
            
            if(CartPosition < 10)
            {
                CartSpeed = 20;
            } else
            {
                CartSpeed = 3;
            }

            if (CartPosition > 21)
            {
                Track_current = Track_walk;
                walkPathCompleted = false;

            }
        }
    }

    void AnimationLoop(string Animation)
    {
        if(anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1 || anim.GetCurrentAnimatorStateInfo(0).normalizedTime == null)
        {
            anim.Play(Animation);
        }
    }
}
