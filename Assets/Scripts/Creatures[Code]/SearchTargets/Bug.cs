using UnityEngine;

//[RequireComponent(typeof(ParticleSystem))]
public class Bug : MonoBehaviour
{
    [SerializeField] private Transform bugTransform;

    [SerializeField] float moveSpeed = 0.1f;
    [SerializeField] float moveSphereRadius = 0.2f;

    private Vector3 movepoint;

    //private ParticleSystem particles;
    
    private void Awake()
    {
        movepoint = Random.insideUnitSphere * moveSphereRadius;
    //    particles = GetComponent<ParticleSystem>();
    }

    private void FixedUpdate()
    {
        Vector3 moveDirection = (movepoint- bugTransform.localPosition);
        if (moveDirection.magnitude > moveSpeed*Time.deltaTime)
        {
            bugTransform.localPosition += moveDirection.normalized * moveSpeed * Time.deltaTime;
        }
        else
        {
            movepoint = Random.insideUnitSphere * moveSphereRadius;
        }
    }

    //private void OnEnable()
    //{
    //    particles.Play();
    //}
}
