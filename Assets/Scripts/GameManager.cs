using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Prefabs & objects")]
    public GameObject blockPrefab;
    public GameObject baseblockPrefab;// prefab must have BlockMover (no Rigidbody)
    public Transform towerCenter;          // optional: for Cinemachine follow

    [Header("Gameplay")]
    public float spawnBoundary = 5f;       // X boundary for moving blocks
    public float perfectThreshold = 0.05f; // tolerance for "perfect" placement

    // state
    private GameObject lastPlacedBlock;    // the static block we stack on
    private GameObject movingBlock;        // the current moving block
    private float blockHeight;
    private int score = 0;
    private bool isGameOver = false;
    public int stackHeight = 0;

    void Start()
    {
        if (blockPrefab == null)
        {
            Debug.LogError("GameManager: blockPrefab not assigned!");
            enabled = false;
            return;
        }

        blockHeight = blockPrefab.transform.localScale.y;
        SpawnInitialBlock();   // place base block
        SpawnMovingBlock();    // create first moving block
    }

    void Update()
    {
        if (isGameOver) return;

        if (movingBlock != null && Input.GetMouseButtonDown(0))
        {
            movingBlock.GetComponent<BlockMover>().StopMoving();
            AlignBlock(movingBlock);

            if (!isGameOver)
            {
                lastPlacedBlock = movingBlock;   // the one we just placed
                UpdateTowerCenter();
                SpawnMovingBlock();              // spawn the next block
            }
        }
    }

    void SpawnInitialBlock()
    {


        if (baseblockPrefab == null)
        {
            Debug.LogError("GameManager: baseblockPrefab not assigned in Inspector!");
            return;
        }

        // Create the first base block at the world center
        Vector3 pos = Vector3.zero;
        lastPlacedBlock = Instantiate(baseblockPrefab, pos, Quaternion.identity);
        lastPlacedBlock.name = "Block_Base";

        // Remove Rigidbody if prefab has one
        var rbBase = lastPlacedBlock.GetComponent<Rigidbody>();
        if (rbBase != null) Destroy(rbBase);

        // Disable BlockMover on the base block (it should stay still)
        var mover = lastPlacedBlock.GetComponent<BlockMover>();
        if (mover != null) mover.enabled = false;
    }

    void SpawnMovingBlock()
    {
        float spawnX = 0f;
        float spawnY = lastPlacedBlock.transform.position.y + blockHeight;
        Vector3 spawnPos = new Vector3(spawnX, spawnY, lastPlacedBlock.transform.position.z);

        movingBlock = Instantiate(blockPrefab, spawnPos, Quaternion.identity, null);
        movingBlock.name = "Block_Moving_" + Time.frameCount;

        // ensure we don't accidentally have a Rigidbody on the moving block prefab
        var existingRb = movingBlock.GetComponent<Rigidbody>();
        if (existingRb != null) Destroy(existingRb);

        // initialize BlockMover: start moving to the right, with boundaries
        var mover = movingBlock.GetComponent<BlockMover>();
        if (mover != null) mover.Initialize(Vector3.right, -spawnBoundary, spawnBoundary);
        else Debug.LogWarning("SpawnMovingBlock: BlockMover missing on prefab");
        Debug.Log("Spawned moving block at Y = " + spawnY);

    }

    void AlignBlock(GameObject block)
    {
        Transform prev = lastPlacedBlock.transform;
        Transform curr = block.transform;

        float prevSizeX = prev.localScale.x;
        float currSizeX = curr.localScale.x;

        float deltaX = curr.position.x - prev.position.x; // positive = moved right relative to prev
        float overlap = prevSizeX - Mathf.Abs(deltaX);

        // Missed completely -> game over: allow block to fall
        if (overlap <= 0f)
        {
            Debug.Log("Game Over - Missed completely");
            var rb = curr.gameObject.AddComponent<Rigidbody>();
            rb.mass = 1f;
            isGameOver = true;
            movingBlock = null;
            return;
        }

        // Compute new size and position for the overlapping (kept) part
        float newSizeX = overlap;
        float newPosX = prev.position.x + (deltaX / 2f);

        // Keep old size to compute cut piece
        float oldSizeX = currSizeX;

        // Apply new scale & position to the current (kept) block
        curr.localScale = new Vector3(newSizeX, curr.localScale.y, curr.localScale.z);
        curr.position = new Vector3(newPosX, curr.position.y, curr.position.z);

        // Create the falling piece (the overhang)
        float cutSize = oldSizeX - newSizeX;
        if (cutSize > 0.0001f)
        {
            float dir = deltaX > 0 ? 1f : -1f; // if deltaX>0, overhang is on right side
            float cutCenterX = newPosX + ((newSizeX / 2f) + (cutSize / 2f)) * dir;

            GameObject falling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            falling.name = "FallingPiece";
            falling.transform.localScale = new Vector3(cutSize, curr.localScale.y, curr.localScale.z);
            falling.transform.position = new Vector3(cutCenterX, curr.position.y, curr.position.z);

            // copy material if available
            var prevRenderer = curr.GetComponent<Renderer>();
            var fallRenderer = falling.GetComponent<Renderer>();
            if (prevRenderer != null && fallRenderer != null) fallRenderer.material = prevRenderer.material;

            // add physics so it falls
            var rbFall = falling.AddComponent<Rigidbody>();
            rbFall.mass = 1f;
        }

        // scoring or perfect-check
        if (Mathf.Abs(deltaX) <= perfectThreshold)
        {
            score += 10;
            Debug.Log("Perfect! Score: " + score);
        }
        else
        {
            score += 5;
            Debug.Log("Placed. Score: " + score);
        }

        // movingBlock is now the placed (resized) block
        movingBlock = null;
    }

    void UpdateTowerCenter()
    {
        if (towerCenter == null || lastPlacedBlock == null) return;
        towerCenter.position = new Vector3(0f, lastPlacedBlock.transform.position.y + blockHeight, lastPlacedBlock.transform.position.z);
    }
}
