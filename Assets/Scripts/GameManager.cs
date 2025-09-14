using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Prefabs & objects")]
    public GameObject blockPrefab;
    public GameObject baseblockPrefab;
    public Transform towerCenter; // optional: for Cinemachine follow

    [Header("Gameplay")]
    public float spawnBoundary = 5f; // X boundary for moving blocks
    public float perfectThreshold = 0.05f; // tolerance for "perfect" placement

    // state
    private GameObject lastPlacedBlock; // the static block we stack on
    private GameObject movingBlock; // the current moving block
    private float blockHeight;
    private int score = 0;
    private bool isGameOver = false;
    public int stackHeight = 0;
    
    // -- NEW -- Added a variable to store the current size of the tower blocks.
    private Vector3 currentBlockSize; 

    void Start()
    {
        if (blockPrefab == null)
        {
            Debug.LogError("GameManager: blockPrefab not assigned!");
            enabled = false;
            return;
        }

        blockHeight = blockPrefab.transform.localScale.y;
        
        // -- NEW -- Get the initial block size from the prefab.
        currentBlockSize = blockPrefab.transform.localScale;

        SpawnInitialBlock(); // place base block
        SpawnMovingBlock(); // create first moving block
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
                lastPlacedBlock = movingBlock; // the one we just placed
                UpdateTowerCenter();
                SpawnMovingBlock(); // spawn the next block
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
        Quaternion rot = Quaternion.Euler(0f, 45f, 0f);

        lastPlacedBlock = Instantiate(baseblockPrefab, pos, rot);
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
        float spawnX = -spawnBoundary; // start from left side so it moves right
        float spawnY = lastPlacedBlock.transform.position.y + blockHeight;
        Vector3 spawnPos = new Vector3(spawnX, spawnY, lastPlacedBlock.transform.position.z);

        // Use the same rotation as the last placed block (e.g., 45° on Y)
        Quaternion spawnRot = lastPlacedBlock.transform.rotation;

        movingBlock = Instantiate(blockPrefab, spawnPos, spawnRot, null);
        movingBlock.name = "Block_Moving_" + Time.frameCount;

        // -- NEW -- Set the new block's scale to the current size of the tower.
        movingBlock.transform.localScale = currentBlockSize;

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
        float prevSizeZ = prev.localScale.z;

        float currSizeX = curr.localScale.x;
        float currSizeZ = curr.localScale.z;

        float deltaX = curr.position.x - prev.position.x;
        float deltaZ = curr.position.z - prev.position.z;

        float overlapX = prevSizeX - Mathf.Abs(deltaX);
        float overlapZ = prevSizeZ - Mathf.Abs(deltaZ);

        // Missed completely -> game over
        if (overlapX <= 0f || overlapZ <= 0f)
        {
            Debug.Log("Game Over - Missed completely");
            var rb = curr.gameObject.AddComponent<Rigidbody>();
            rb.mass = 1f;
            isGameOver = true;
            movingBlock = null;
            return;
        }

        // Compute new size and position
        float newSizeX = Mathf.Max(overlapX, 0.01f); // avoid zero size
        float newSizeZ = Mathf.Max(overlapZ, 0.01f);

        float newPosX = prev.position.x + (deltaX / 2f);
        float newPosZ = prev.position.z + (deltaZ / 2f);

        // Save old sizes for cut pieces
        float oldSizeX = currSizeX;
        float oldSizeZ = currSizeZ;

        // Apply new scale & position
        curr.localScale = new Vector3(newSizeX, curr.localScale.y, newSizeZ);
        curr.position = new Vector3(newPosX, curr.position.y, newPosZ);
        
        // -- NEW -- Update the current tower size for the next block to use.
        currentBlockSize = new Vector3(newSizeX, curr.localScale.y, newSizeZ);

        // Create falling piece for X
        float cutX = oldSizeX - newSizeX;
        if (cutX > 0.0001f)
        {
            float dir = deltaX > 0 ? 1f : -1f;
            float cutCenterX = newPosX + ((newSizeX / 2f) + (cutX / 2f)) * dir;

            GameObject fallingX = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fallingX.name = "FallingPieceX";
            fallingX.transform.localScale = new Vector3(cutX, curr.localScale.y, newSizeZ);
            fallingX.transform.position = new Vector3(cutCenterX, curr.position.y, newPosZ);

            var fallRbX = fallingX.AddComponent<Rigidbody>();
            fallRbX.mass = 1f;
            var renderer = curr.GetComponent<Renderer>();
            if (renderer != null) fallingX.GetComponent<Renderer>().material = renderer.material;
        }

        // Create falling piece for Z
        float cutZ = oldSizeZ - newSizeZ;
        if (cutZ > 0.0001f)
        {
            float dir = deltaZ > 0 ? 1f : -1f;
            float cutCenterZ = newPosZ + ((newSizeZ / 2f) + (cutZ / 2f)) * dir;

            GameObject fallingZ = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fallingZ.name = "FallingPieceZ";
            fallingZ.transform.localScale = new Vector3(newSizeX, curr.localScale.y, cutZ);
            fallingZ.transform.position = new Vector3(newPosX, curr.position.y, cutCenterZ);

            var fallRbZ = fallingZ.AddComponent<Rigidbody>();
            fallRbZ.mass = 1f;
            var renderer = curr.GetComponent<Renderer>();
            if (renderer != null) fallingZ.GetComponent<Renderer>().material = renderer.material;
        }

        // Score
        if (Mathf.Abs(deltaX) <= perfectThreshold && Mathf.Abs(deltaZ) <= perfectThreshold)
        {
            score += 10;
            Debug.Log("Perfect! Score: " + score);
        }
        else
        {
            score += 5;
            Debug.Log("Placed. Score: " + score);
        }
    }

    void UpdateTowerCenter()
    {
        if (towerCenter == null || lastPlacedBlock == null) return;
        towerCenter.position = new Vector3(0f, lastPlacedBlock.transform.position.y + blockHeight, lastPlacedBlock.transform.position.z);
    }
}
