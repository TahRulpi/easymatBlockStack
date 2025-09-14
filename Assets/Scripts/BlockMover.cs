using UnityEngine;

public class BlockMover : MonoBehaviour
{
    public float moveSpeed = 3f;
    private bool isMoving = true;
    private Vector3 direction;

    // movement boundaries (local/global space)
    public float xLimit = 3f;
    public float zLimit = 3f;

    private Vector3 startPos;

    void Start()
    {
        // Remember where this block spawned
        //startPos = transform.position;
        startPos = Vector3.zero;

        // Randomly decide movement axis (X or Z)
        //direction = Random.value > 0.5f ? Vector3.right : Vector3.forward;
    }

    void Update()
    {
        if (!isMoving) return;

        transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);

        // Clamp in X direction
        if (direction.x != 0)
        {
            if (transform.position.x >= startPos.x + xLimit)
                direction = Vector3.left;
            else if (transform.position.x <= startPos.x - xLimit)
                direction = Vector3.right;
        }

        // Clamp in Z direction
        if (direction.z != 0)
        {
            if (transform.position.z >= startPos.z + zLimit)
                direction = Vector3.back;
            else if (transform.position.z <= startPos.z - zLimit)
                direction = Vector3.forward;
        }
    }

    public void StopMoving()
    {
        isMoving = false;
    }

    public void Initialize(Vector3 startDirection, float left, float right)
    {
        direction = startDirection.normalized;
        startPos = transform.position; // reset center when spawned
        isMoving = true;
    }
}
