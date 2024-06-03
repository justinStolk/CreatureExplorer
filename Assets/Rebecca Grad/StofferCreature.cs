using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class StofferCreature : MonoBehaviour
{
    public GameObject Track_walk_gameObject;
    Cinemachine.CinemachinePathBase Track_walk;
    public GameObject DollyCart_Stoffer;
    private Animator anim;

    Cinemachine.CinemachinePathBase Track_current_check;
    float CartPosition_check;
    float CartSpeed_check;

    bool actionIsActive = false;
    public GameObject popUpLocation_1;
    //Number 2 is the one that's followed by a little run
    public GameObject popUpLocation_2;
    public GameObject popUpLocation_3;

    float nextActionNumber;
    //bools for the pop out + run wombo combo
    bool outHoleActivated = false;
    bool runActivated = false;
    bool inActivated = false;
    //bools for the peek out, go back in
    bool upActivated = false;
    bool downActivated = false;
    public int timer = 0;


    // Start is called before the first frame update
    void Start()
    {
        Track_walk = Track_walk_gameObject.GetComponent<CinemachinePathBase>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Track_current_check = DollyCart_Stoffer.GetComponent<CinemachineDollyCart>().m_Path;
        CartPosition_check = DollyCart_Stoffer.GetComponent<CinemachineDollyCart>().m_Position;
        CartSpeed_check = DollyCart_Stoffer.GetComponent<CinemachineDollyCart>().m_Speed;

        //PopUp(popUpLocation_2);

        if (!actionIsActive)
        {
            nextActionNumber = Random.Range(1, 4);
            actionIsActive = true;
        }

        if (nextActionNumber == 1)
        {
            PopUp(popUpLocation_1);
            //Debug.Log("activating popup 1");
        }

        if (nextActionNumber == 2)
        {
            PopUp(popUpLocation_2);
            //Debug.Log("activating popup 2");
        }

        if (nextActionNumber == 3)
        {
            PopUp(popUpLocation_3);
            //Debug.Log("activating popup 3");
        }

    }

    void PopUp(GameObject popUpLocation)
    {
        if (popUpLocation == popUpLocation_2)
        {
            if (!outHoleActivated)
            {
                DollyCart_Stoffer.GetComponent<CinemachineDollyCart>().m_Position = 0;
                this.transform.position = DollyCart_Stoffer.GetComponent<CinemachineDollyCart>().transform.position;
                DollyCart_Stoffer.GetComponent<CinemachineDollyCart>().m_Speed = 0;
                AnimationPlayOnce("Out");

                if(anim.GetCurrentAnimatorStateInfo(0).IsName("Out") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1.0f)
                {
                    outHoleActivated = true;
                }
            }

            if(inActivated == false && outHoleActivated == true)
            {
                DollyCart_Stoffer.GetComponent<CinemachineDollyCart>().m_Speed = 5;
                AnimationLoop("Run");
                runActivated = true;
            }

            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Run") && runActivated == true && CartPosition_check > 24.5)
            {
                DollyCart_Stoffer.GetComponent<CinemachineDollyCart>().m_Speed = 0;
                AnimationLoop("In");
                inActivated = true;
            }

            if(anim.GetCurrentAnimatorStateInfo(0).IsName("In") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1.0f)
            {
                runActivated = false;
                outHoleActivated = false;
                inActivated = false;

                //allow new action to take place
                actionIsActive = false;
            }

        } else
        {
            this.transform.position = new Vector3(popUpLocation.transform.position.x, popUpLocation.transform.position.y, popUpLocation.transform.position.z);
            
            if (!upActivated)
            {
                Debug.Log("activating " + popUpLocation);
                AnimationPlayOnce("Up");

                if(anim.GetCurrentAnimatorStateInfo(0).IsName("Down") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1.0f)
                {
                    while (timer < 100002)
                    {
                        timer++;

                        if (timer > 100000)
                        {
                            upActivated = true;
                            Debug.Log("Timer is over");
                        }
                    }
                }
                
            } else {
                AnimationPlayOnce("Down");

                if(anim.GetCurrentAnimatorStateInfo(0).IsName("Down") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1.0f)
                {
                    while (timer < 1000000002)
                    {
                        timer++;

                        if (timer > 1000000000)
                        {
                            actionIsActive = false;
                            Debug.Log("Timer in between actions is over");
                        }
                    }
                }
            }

        }


    }

    void AnimationLoop(string Animation)
    {
        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 || anim.GetCurrentAnimatorStateInfo(0).normalizedTime == null)
        {
            anim.Play(Animation);
            //Debug.Log("this code is actually reached");
        }
    }

    void AnimationPlayOnce(string Animation)
    {
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName(Animation))
        {
            anim.Play(Animation);
            Debug.Log("Playing animation: " + Animation);
        }
    }
}
