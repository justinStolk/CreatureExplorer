using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snorlax : Charger
{
    [Header("Snorlax")]

    [SerializeField] private Action sleepAction;
    [SerializeField] private Action moveAction, fleeAction;

    private bool luredAway = false;
    private Animator animator;

    // Start is called before the first frame update
    protected override void Start()
    {
        animator = GetComponentInChildren<Animator>();
        
        GetComponent<UnityEngine.AI.NavMeshAgent>().updateUpAxis = false;
        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), col);
        }

        surroundCheck = new CheckSurroundings(CheckForFood);
        surroundCheck += CheckForPredators;
        surroundCheck += CheckForFleeing;
        StartAction();

        if (surroundCheck != null)
            InvokeRepeating("LookAtSurroundings", 0, data.CheckSurroundingsTimer);

        GetComponentInChildren<Animator>().SetTrigger("FallAsleep");
    }

    private void Update()
    {
        
        if (CurrentAction == sleepAction)
        {
            animator.ResetTrigger("Walk");
        }
        /*
        if ((!(animator.GetCurrentAnimatorStateInfo(0).IsName("Sleeping") || animator.GetCurrentAnimatorStateInfo(0).IsName("Fall Asleep") || animator.GetCurrentAnimatorStateInfo(0).IsName("Wake")) || animator.GetNextAnimatorStateInfo(0).IsName("Walking")) && CurrentAction == sleepAction)
        {
            GetComponentInChildren<Animator>().SetTrigger("Sleep");
        }
        */
    }

    /*
    // Update is called once per frame
    protected override void FixedUpdate()
    {        
        if (luredAway)
        {
            Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit);
            if (transform.up != hit.normal)
            {
                Vector3 tempForward = Vector3.Cross(hit.normal, transform.right);
                if ((transform.forward - tempForward).magnitude > 1)
                {
                    tempForward *= -1;
                }
                transform.up = hit.normal;
                transform.forward = tempForward;
            }

            if (CurrentAction == moveAction && (CurrentAction.finished || CurrentAction.failed))
            {
                DestroyImmediate(currentTarget.gameObject);
                surroundCheck += CheckForFood;

                Interrupt(sleepAction);
                luredAway = false;
            }
        }
    }
    */

    protected override void ReactToThreat(Vector3 threatPosition, float threatLoudness)
    {
        base.ReactToThreat(threatPosition, threatLoudness);

        Interrupt(fleeAction);
        GetComponent<Collider>().isTrigger = true;
    }

    protected override void ReactToAttack(Vector3 attackPos)
    {
        base.ReactToAttack(attackPos);
        GetComponent<Collider>().isTrigger = true;
    }
    
    
    protected override void ReactToPlayer(Vector3 playerPos, float playerLoudness)
    {
    }

    protected override void ReactToPlayerLeaving(Vector3 playerPos)
    {
    }

    /// <summary>
    /// Checks for food in neighbourhood and ups the hunger value with the amount of food nearby
    /// </summary>
    public override void CheckForFood()
    {
        Food f = null;
        if (LookForObjects<Food>.TryGetClosestObject(f, transform.position, data.HearingSensitivity, out f) && CurrentAction != fleeAction)
        {
            currentTarget = f.gameObject;
            surroundCheck -= CheckForFood;

            Interrupt(moveAction, "", true);
            luredAway = true;
            GetComponent<Collider>().isTrigger = true; 
            animator.SetTrigger("Wake");
        }
    }
}
