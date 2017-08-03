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
		if (isInitialized && (Random.Range(0, 10000) < 50))
        {
            foreach (FishSpawn fs in typesOfFishes)
            {
                goalPositions[fs.fishPrefab.name] = randomFishRange(fs.fishPrefab.name);
            }
        }
    }

    public Vector3 FishGoalPosition(string fishId)
    {
        Vector3 goal = Vector3.zero;
        if (!goalPositions.TryGetValue(fishId, out goal))
            Debug.LogError("Failed to get goal position for fish id: " + fishId);
        return goal;
    }

	public void SpawnFishes()
	{
		if (!isInitialized)
			StartCoroutine(InitializeFishes());
	}

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
				GameObject fish = ObjectPool.Instance.GetPooledObject(fs.fishPrefab.name);
				if (fish == null)
				{
					Debug.LogError("Failed to find pooled object with id: " + fs.fishPrefab.name);
					continue;
				}
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

    private Vector3 randomTankRange()
    {
        return new Vector3(Random.Range(-20, 80),
            Random.Range(-25, -200), Random.Range(135, 230));
    }

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
