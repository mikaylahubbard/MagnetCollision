using Unity.Netcode;
using UnityEngine;

public class MagnetDisplay : NetworkBehaviour
{
    public GameObject magnetPrefab;

    // Player1Magnets = 0, Player2Magnets = 1
    public ulong ownerClientId;

    public int magnetCount = 10;
    public float spacing = 0.6f;

    public override void OnNetworkSpawn()
    {
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
            var magnet = magnetObj.GetComponent<Magnet>();

            if (netObj != null)
            {

                netObj.SpawnWithOwnership(ownerClientId);

                magnet.Initialize(ownerClientId, pos, this);
            }
            else
            {
                Debug.LogError("Magnet prefab is missing NetworkObject!");
            }
        }
    }

    public Vector3 GetNextStackPosition()
    {
        int count = 0;

        foreach (var magnet in FindObjectsOfType<Magnet>())
        {
            if (magnet.GetOwner() == ownerClientId && magnet.IsInStack())
            {
                count++;
            }
        }

        return transform.position + new Vector3(0, count * spacing, 0);
    }
}