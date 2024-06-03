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

    bool actionIsActive;
    public GameObject popUpLocation_1;
    //Number 2 is the one that's followed by a little run
    public GameObject popUpLocation_2;
    public GameObject popUpLocation_3;

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
    }

    void PopUp(GameObject popUpLocation)
    {
        if (popUpLocation == popUpLocation_2)
        {
            actionIsActive = true;
            AnimationPlayOnce("Out Hole");
            DollyCart_Stoffer.GetComponent<CinemachineDollyCart>().m_Speed = 3;
            AnimationLoop("Run");
        }
    }

    void AnimationLoop(string Animation)
    {
        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1 || anim.GetCurrentAnimatorStateInfo(0).normalizedTime == null)
        {
            anim.Play(Animation);
        }
    }

    void AnimationPlayOnce(string Animation)
    {
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName(Animation) && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
        {
            anim.Play(Animation);
            Debug.Log("Playing animation: " + Animation);
        }
    }
}
