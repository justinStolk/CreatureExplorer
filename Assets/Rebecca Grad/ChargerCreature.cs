using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ChargerCreature : MonoBehaviour
{
    public GameObject Track_walk_gameObject;
    public GameObject Track_ramTree_gameObject;
    public GameObject BerrySprite;
    new Vector3 BerrySpawnPoint;
    Cinemachine.CinemachinePathBase Track_walk;
    Cinemachine.CinemachinePathBase Track_ramTree;
    public GameObject DollyCart_Charger;
    private Animator anim;

    Cinemachine.CinemachinePathBase Track_current_check;
    bool walkPathCompleted;
    bool ramPathCompleted;
    bool ramPathChanged = false;
    float CartPosition_check;
    float CartSpeed_check;

    bool Charger_mainLoop = true;

    // Start is called before the first frame update
    void Start()
    {
        Track_walk = Track_walk_gameObject.GetComponent<CinemachinePathBase>();
        Track_ramTree = Track_ramTree_gameObject.GetComponent<CinemachinePathBase>();
        anim = GetComponent<Animator>();
        BerrySpawnPoint = new Vector3(BerrySprite.position.x, BerrySprite.position.y, BerrySprite.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        Track_current_check = DollyCart_Charger.GetComponent<CinemachineDollyCart>().m_Path;
        CartPosition_check = DollyCart_Charger.GetComponent<CinemachineDollyCart>().m_Position;
        CartSpeed_check = DollyCart_Charger.GetComponent<CinemachineDollyCart>().m_Speed;

        

        //play animations accordingly
        if (Track_current_check == Track_walk)
        {
            if(CartSpeed_check > 0)
            {
                AnimationLoop("Walk");
            } else
            {
                AnimationLoop("Neutral");
            }
        }

        //standard behavior
        if(CartPosition_check > 213)
        {
            walkPathCompleted = true;
        }

        if(walkPathCompleted == true)
        {
            RamTree();
        }

        if(Charger_mainLoop == true)
        {
            //play animations accordingly
            if (Track_current_check == Track_walk)
            {
                if (CartSpeed_check > 0)
                {
                    AnimationLoop("Walk");
                }
                else
                {
                    AnimationLoop("Neutral");
                }
            }

            //standard behavior
            if (CartPosition_check > 213)
            {
                walkPathCompleted = true;
            }

            if (walkPathCompleted == true)
            {
                RamTree();
            }
        }
        
    }

    // change this to OnTriggerEnter
    private void OnColliderEnter(Collision other)
    {
        if (other.gameObject.tag == "berry")
        {
            Debug.Log("collider is triggered by berry");
        } else
        {
            Debug.Log("collider is being triggered by something else");
        }
    }

    void AnimationLoop(string Animation)
    {
        if(anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1 || anim.GetCurrentAnimatorStateInfo(0).normalizedTime == null)
        {
            anim.Play(Animation);
        }
    }

    void AnimationPlayOnce(string Animation)
    {
        if(!anim.GetCurrentAnimatorStateInfo(0).IsName(Animation) && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
        {
            anim.Play(Animation);
            //Debug.Log("Playing animation: " + Animation);
        }
    }

    void RamTree()
    {
        //Debug.Log("Ram Tree function is called");

        if(ramPathChanged == false)
        {
            DollyCart_Charger.GetComponent<CinemachineDollyCart>().m_Path = Track_ramTree;
            DollyCart_Charger.GetComponent<CinemachineDollyCart>().m_Position = 0;
            
            ramPathChanged = true;
        }

        if (DollyCart_Charger.GetComponent<CinemachineDollyCart>().m_Position < 9)
        {
            DollyCart_Charger.GetComponent<CinemachineDollyCart>().m_Speed = 8;
            AnimationPlayOnce("Ram");
        }

        if (DollyCart_Charger.GetComponent<CinemachineDollyCart>().m_Position >= 9 && DollyCart_Charger.GetComponent<CinemachineDollyCart>().m_Position < 10)
        {
            if(anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1.0f)
            {
                BerrySprite.GetComponent<Rigidbody>().useGravity = true;
                DollyCart_Charger.GetComponent<CinemachineDollyCart>().m_Speed = 0;
                AnimationPlayOnce("Eat");
                // add berry that falls from the tree at this point, then a small delay before the animation happens
                
            }

            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Eat") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1.0f)
            {
                BerrySprite.GetComponent<Rigidbody>().useGravity = false;
                BerrySprite.transform.position(BerrySpawnPoint);
                DollyCart_Charger.GetComponent<CinemachineDollyCart>().m_Speed = 3;
                AnimationLoop("Walk");
            }
        }

        if (DollyCart_Charger.GetComponent<CinemachineDollyCart>().m_Position > 18)
        {
            DollyCart_Charger.GetComponent<CinemachineDollyCart>().m_Path = Track_walk;
            DollyCart_Charger.GetComponent<CinemachineDollyCart>().m_Position = 0;
            walkPathCompleted = false;
            //ramPathCompleted = false;
            ramPathChanged = false;
        }
        
    }
}
