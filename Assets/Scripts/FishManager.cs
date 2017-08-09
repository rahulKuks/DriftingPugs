using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishManager : MonoBehaviour {

    [System.Serializable]
    public class FishSpawn
    {
        public GameObject fishPrefab;
        public int count;
    }

    [SerializeField] private List<FishSpawn> typesOfFishes;
    [Tooltip("The range of movement in the Y axis a fish has from their original position.")]
    [SerializeField] private float fishYAxisRange;

    private List<Flock> fishes;
    private Dictionary<string, Vector3> goalPositions;
	private bool isInitialized = false;

    public List<Flock> Fishes
    { get { return fishes; } }

    // Singleton pattern
    private static FishManager _instance;
    public static FishManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<FishManager>();
            }
            return _instance;
        }
    }

    private void Update()
    {
        // Set a new goal position if fishes are initialized but don't do it too often
		if (isInitialized && (Random.Range(0, 10000) < 50))
        {
            // Set the goal position for each type of fish
            foreach (FishSpawn fs in typesOfFishes)
            {
                goalPositions[fs.fishPrefab.name] = randomFishRange(fs.fishPrefab.name);
            }
        }
    }
    
    /// <summary>
    /// Gets the goal position for a specific type of fish.
    /// </summary>
    /// <param name="fishId">Type of fish.</param>
    /// <returns>A Vector3 of the new goal position.</returns>
    public Vector3 FishGoalPosition(string fishId)
    {
        Vector3 goal = Vector3.zero;
        if (!goalPositions.TryGetValue(fishId, out goal))
            Debug.LogError("Failed to get goal position for fish id: " + fishId);
        return goal;
    }

    /// <summary>
    /// Spawn fishes if they have not already.
    /// </summary>
	public void SpawnFishes()
	{
		if (!isInitialized)
			StartCoroutine(InitializeFishes());
	}

    /// <summary>
    /// Go through the types of fishes to spawn and spawn them via getting the object through
    /// the object pool.
    /// </summary>
    /// <returns></returns>
    private IEnumerator InitializeFishes()
    {
        fishes = new List<Flock>();
        goalPositions = new Dictionary<string, Vector3>();

        int i;
        Flock f;
        foreach (FishSpawn fs in typesOfFishes)
        {
            i = 0;
            while (i++ < fs.count)
            {
				if (fs.fishPrefab == null)
				{
					Debug.LogError("Missing prefab at element: " + i);
					continue;
				}
                // Get the object from object pool
				GameObject fish = ObjectPool.Instance.GetPooledObject(fs.fishPrefab.name);
				if (fish == null)
				{
					Debug.LogError("Failed to find pooled object with id: " + fs.fishPrefab.name);
					continue;
				}
                // Spawns the fish
                fish.transform.SetParent(this.transform);
                f = fish.GetComponent<Flock>();
                f.SetId(fs.fishPrefab.name);
                fishes.Add(f);
                fish.transform.position = randomFishRange(fs.fishPrefab.name);
                fish.SetActive(true);
            }

            goalPositions.Add(fs.fishPrefab.name, randomFishRange(fs.fishPrefab.name));
            goalPositions[fs.fishPrefab.name] = randomFishRange(fs.fishPrefab.name);
        }

		isInitialized = true;
        yield return null;
    }

    /// <summary>
    /// Get a random point in the tank.
    /// </summary>
    /// <returns>A Vector3 in the tank.</returns>
    private Vector3 randomTankRange()
    {
        return new Vector3(Random.Range(-20, 80),
            Random.Range(-25, -200), Random.Range(135, 230));
    }

    /// <summary>
    /// Gets a random point in the tank for a specific type of fish. This random point's position's
    /// y value is restricted depending on the type of the fish.
    /// </summary>
    /// <param name="fishName">Type of fish.</param>
    /// <returns>A Vector3 in the tank.</returns>
    private Vector3 randomFishRange(string fishName)
    {
        foreach (FishSpawn fs in typesOfFishes)
        {
            if (fs.fishPrefab.name == fishName)
            {
                return new Vector3(Random.Range(-20, 80),
                    Random.Range(fs.fishPrefab.transform.position.y - fishYAxisRange,
                        fs.fishPrefab.transform.position.y + fishYAxisRange),
                    Random.Range(135, 230));
            }
        }

        Debug.LogError("Failed to find a fish name: " + fishName);
        return Vector3.zero;
    }
}
