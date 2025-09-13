using UnityEngine;

public class BlockMover : MonoBehaviour
{
    public float moveSpeed = 3f;
    private bool isMoving = true;
    private Vector3 direction;

    void Start()
    {
        // Randomly decide movement direction (x or z)
        direction = Random.value > 0.5f ? Vector3.right : Vector3.forward;
    }

    void Update()
    {
        if (isMoving)
            transform.Translate(direction * moveSpeed * Time.deltaTime);
    }

    public void StopMoving()
    {
        isMoving = false;
    }

    public void Initialize(Vector3 startDirection, float left, float right)
    {
        direction = startDirection.normalized;
        
        isMoving = true;
    }
}
