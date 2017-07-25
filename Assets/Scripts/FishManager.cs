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

    private List<Flock> fishes;
    private Vector3 goalPos = Vector3.zero;

    public List<Flock> Fishes
    { get { return fishes; } }
    public Vector3 GoalPos
    { get { return goalPos; } }

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

    void Start () {
        StartCoroutine(InitializeFishes());
        goalPos = RandomTankRange();
	}

    private void Update()
    {
        if (Random.Range(0, 10000) < 50)
        {
            goalPos = RandomTankRange();
        }
    }

    private IEnumerator InitializeFishes()
    {
        fishes = new List<Flock>();

        int i;
        Flock f;
        foreach (FishSpawn fs in typesOfFishes)
        {
            i = 0;
            while (i++ < fs.count)
            {
                GameObject fish = ObjectPool.Instance.GetPooledObject(fs.fishPrefab.name);
                if (fish == null)
                {
                    Debug.Log("Failed to find pooled object with id: " + fs.fishPrefab.name);
                }
                fish.transform.SetParent(this.transform);
                f = fish.GetComponent<Flock>();
                fishes.Add(f);
                fish.transform.position = new Vector3(Random.Range(-20, 80),
                    Random.Range(fs.fishPrefab.transform.position.y - 10, fs.fishPrefab.transform.position.y + 10),
                    Random.Range(135, 230));
                fish.SetActive(true);
            }
        }

        yield return null;
    }

    private Vector3 RandomTankRange()
    {
        return new Vector3(Random.Range(-20, 80),
            Random.Range(-25, -200),
            Random.Range(135, 230));
    }
}
