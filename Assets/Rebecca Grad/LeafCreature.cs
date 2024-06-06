using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


[RequireComponent(typeof(Animator))]
public class LeafCreature : MonoBehaviour
{
    public GameObject Track_walk_gameObject;
    Cinemachine.CinemachinePathBase Track_walk;
    public GameObject DollyCart_Leaf;

    Cinemachine.CinemachinePathBase Track_current_check;
    float CartPosition_check;
    float CartSpeed_check;

    // New boolean hell <3
    private Animator anim;
    public UnityAnimationEvent OnAnimationStart;
    public UnityAnimationEvent OnAnimationComplete;
    public float waitTime = 5.0f;
    public float timer = 0.0f;
    public List<GameObject> points = new List<GameObject>();
    public int sequence = -1;
    public List<string> sequences = new List<string>();
    public float sequenceWait = 0.0f;
    public float sequenceWaitTimer = 0.0f;
    public int animationToRun = -1;
    public int lastAnimationRun = -1;
    public int currentAnimationLoop = 0;
    public int animationLoops = 0;

    void Awake()
    {
        anim = GetComponent<Animator>();

        for (int i = 0; i < anim.runtimeAnimatorController.animationClips.Length; i++)
        {
            AnimationClip clip = anim.runtimeAnimatorController.animationClips[i];

            AnimationEvent animationStartEvent = new AnimationEvent();
            animationStartEvent.time = 0;
            animationStartEvent.functionName = "AnimationStartHandler";
            animationStartEvent.stringParameter = clip.name;

            AnimationEvent animationEndEvent = new AnimationEvent();
            animationEndEvent.time = clip.length;
            animationEndEvent.functionName = "AnimationCompleteHandler";
            animationEndEvent.stringParameter = clip.name;

            clip.AddEvent(animationStartEvent);
            clip.AddEvent(animationEndEvent);
        }
    }

    public void AnimationStartHandler(string name)
    {
        // Debug.Log($"{name} animation start.");
        OnAnimationStart?.Invoke(name);
    }
    public void AnimationCompleteHandler(string name)
    {
        sequenceWaitTimer = 0.0f;
        // Debug.Log($"{name} animation complete.");
        OnAnimationComplete?.Invoke(name);

        if (sequences.Count > 0)
        {
            if (currentAnimationLoop >= animationLoops)
            {
                sequence++;
                currentAnimationLoop = 0;
                animationLoops = 0;
            }
            else
            {
                currentAnimationLoop++;
            }
        }

        if (sequence >= sequences.Count)
        {
            lastAnimationRun = animationToRun;
            animationToRun = -1;
            sequence = -1;
            sequences.Clear();
            sequenceWait = 0.0f;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Track_walk = Track_walk_gameObject.GetComponent<CinemachinePathBase>();
    }

    // Update is called once per frame
    void Update()
    {
        Track_current_check = DollyCart_Leaf.GetComponent<CinemachineDollyCart>().m_Path;
        CartPosition_check = DollyCart_Leaf.GetComponent<CinemachineDollyCart>().m_Position;
        CartSpeed_check = DollyCart_Leaf.GetComponent<CinemachineDollyCart>().m_Speed;

        if (sequences.Count == 0 && sequence < 0 && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1.0f)
        {
            timer += Time.deltaTime;

            if (timer > waitTime)
            {
                timer = 0.0f;

                while (animationToRun == -1 || animationToRun == lastAnimationRun)
                {
                    animationToRun = Random.Range(0, points.Count);
                }
            }
        }

        if (animationToRun >= 0 && animationToRun <= points.Count)
        {
            Behaviour(points[animationToRun]);
        }
    }

    public bool IsAnimationReady()
    {
        return anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1.0f && sequenceWaitTimer >= sequenceWait;
    }

    void Behaviour(GameObject startLocation)
    {
        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1.0f && sequenceWaitTimer < sequenceWait)
        {
            sequenceWaitTimer += Time.deltaTime;
        }

        switch (animationToRun)
        {
            case 0:
                if (sequences.Count == 0)
                {
                    sequences.Add("Walk");
                    sequence++;
                }

                if (IsAnimationReady())
                {
                    switch (sequence)
                    {
                        case 0:
                            //DollyCart_Leaf.GetComponent<CinemachineDollyCart>().m_Position = 0;
                            //this.transform.position = DollyCart_Leaf.GetComponent<CinemachineDollyCart>().transform.position;
                            DollyCart_Leaf.GetComponent<CinemachineDollyCart>().m_Speed = 3;

                            AnimationLoop(sequences[0]);

                            break;

                        /*case 1:
                            DollyCart_Stoffer.GetComponent<CinemachineDollyCart>().m_Speed = 5;
                            AnimationLoop(sequences[1]);
                            animationLoops = 5;
                            break;

                        case 2:
                            DollyCart_Stoffer.GetComponent<CinemachineDollyCart>().m_Speed = 0;
                            AnimationPlayOnce(sequences[2]);
                            break;*/
                    }
                }

                return;

            /*case 0:
            case 2:
            case 3:
            case 4:
                this.transform.position = new Vector3(popUpLocation.transform.position.x, popUpLocation.transform.position.y, popUpLocation.transform.position.z);

                if (sequences.Count == 0)
                {
                    sequences.Add("Up");
                    sequences.Add("Down");
                    sequenceWait = 3.0f;
                    sequence++;
                }

                if (IsAnimationReady())
                {
                    AnimationPlayOnce(sequences[sequence]);
                }

                break;*/
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
