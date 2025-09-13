using UnityEngine;

public class Camfollow : MonoBehaviour
{
    public Transform target; // Reference to GameManager (or tower center)
    public int offsetY = 3;

    private GameManager gameManager;

    void Start()
    {
        gameManager = target.GetComponent<GameManager>();
    }

    void Update()
    {
        if (gameManager != null)
        {
            transform.position = new Vector3(0, gameManager.stackHeight + offsetY, -10);
        }
    }
}
