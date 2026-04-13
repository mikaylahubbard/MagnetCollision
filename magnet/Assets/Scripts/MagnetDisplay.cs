using Unity.Netcode;
using UnityEngine;

public class MagnetDisplay : NetworkBehaviour
{
    public GameObject magnetPrefab;
    public int magnetCount = 10;
    public float spacing = 0.6f;

    public override void OnNetworkSpawn()
    {
        // ONLY SERVER SPAWNS MAGNETS
        if (!IsServer) return;

        SpawnMagnets();
    }

    void SpawnMagnets()
    {
        for (int i = 0; i < magnetCount; i++)
        {
            Vector3 pos = transform.position + new Vector3(0, i * spacing, 0);

            GameObject magnetObj = Instantiate(magnetPrefab, pos, Quaternion.identity);

            var netObj = magnetObj.GetComponent<NetworkObject>();

            if (netObj != null)
            {
                netObj.Spawn();
            }
            else
            {
                Debug.LogError("Magnet prefab is missing NetworkObject!");
            }
        }
    }
}