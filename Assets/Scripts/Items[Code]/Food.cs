using System.Collections;
using UnityEngine;

public class Food : MonoBehaviour
{
    [field: SerializeField] private AudioClip fallingSound;
    private SoundPlayer soundPlayer;

    private void Awake()
    {
        if (GetComponent<Rigidbody>().useGravity != true)
        {
            GetComponent<Collider>().isTrigger = true;
        }

        soundPlayer = GetComponent<SoundPlayer>();
        if (soundPlayer == null)
            soundPlayer = GetComponentInParent<SoundPlayer>();
    }

    public void ActivatePhysics()
    {
        GetComponent<Rigidbody>().useGravity = true;
        GetComponent<Collider>().isTrigger = false;
        transform.parent = null;
        StartCoroutine(Decay(10));
    }

    private void OnCollisionEnter(Collision collision)
    {
        // TODO: test whether letting the berries play a sound on every collision leads to unexpected sounds
        if (collision.gameObject.layer == 3 && soundPlayer != null)
        {
            soundPlayer.PlaySound(fallingSound, true);
        }
    }

    private IEnumerator Decay(float timer)
    {
        yield return new WaitForSeconds(timer);

        DestroyImmediate(gameObject);
    }
}
