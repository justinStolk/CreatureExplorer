using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class PlayerEventCaster : EventCaster
{
    public static UnityEvent<Vector3, float> makeSound;

    private Vector3 position = Vector3.zero;

    protected void Awake()
    {
        // Initiate events and add listeners
        makeSound = new UnityEvent<Vector3, float>();
    }
    //protected override void OnDestroy()
    private void OnDestroy()
    {
        //base.OnDestroy();
        makeSound.RemoveAllListeners();
    }

    public static void ListenForSounds(UnityAction<Vector3, float> action)
    {
        // TODO: make sure no double calls are registered?
        makeSound.AddListener(action);
    }

    protected override void UpdateListeners()
    {
        /*
        makeSound.RemoveAllListeners();
        foreach (Creature c in GameObject.FindObjectsOfType<Creature>())
        {
            makeSound.AddListener(c.HearPlayer);
        }
        */
    }

    protected override IEnumerator InvokeFrequentEvents(CancellationToken cancelToken)
    {
        while (!cancelToken.IsCancellationRequested)
        {
            yield return new WaitForSeconds(frequentInvokeTimer);
            /*
            foreach (UnityEvent unityEvent in frequentEvents)
            {
                unityEvent.Invoke();
            }
            */
            makeSound.Invoke(transform.position, PlayerController.Loudness);
        }
    }

    protected override async Task InvokeStandardEvents(CancellationToken cancelToken)
    {
        while (!cancelToken.IsCancellationRequested)
        {
            await Task.Delay((int)(standardInvokeTimer * 1000));

        }
    }

    protected override async Task InvokeSlowEvents(CancellationToken cancelToken)
    {
        while (!cancelToken.IsCancellationRequested)
        {
            await Task.Delay((int)(slowInvokeTimer * 1000));

        }
    }
}
