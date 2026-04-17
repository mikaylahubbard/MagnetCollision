using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class MagnetDisplay : NetworkBehaviour
{
    public GameObject magnetPrefab;
    public ulong ownerClientId;
    public int magnetCount = 10;
    public float spacing = 0.6f;

    // Track magnets assigned to this display directly
    private List<Magnet> assignedMagnets = new List<Magnet>();

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        SpawnMagnets();
    }

    void SpawnMagnets()
    {
        for (int i = 0; i < magnetCount; i++)
        {
            Vector3 pos = GetPositionForIndex(i);
            GameObject magnetObj = Instantiate(magnetPrefab, pos, Quaternion.identity);
            var netObj = magnetObj.GetComponent<NetworkObject>();
            var magnet = magnetObj.GetComponent<Magnet>();

            if (netObj != null)
            {
                netObj.SpawnWithOwnership(ownerClientId);
                magnet.Initialize(ownerClientId, pos, this);
                assignedMagnets.Add(magnet);
            }
            else
            {
                Debug.LogError("Magnet prefab is missing NetworkObject!");
            }
        }
    }

    // Called when a magnet is being added to this display
    public void RegisterMagnet(Magnet magnet)
    {
        if (!assignedMagnets.Contains(magnet))
            assignedMagnets.Add(magnet);
    }

    public void UnregisterMagnet(Magnet magnet)
    {
        assignedMagnets.Remove(magnet);
    }

    private Vector3 GetPositionForIndex(int index)
    {
        return transform.position + new Vector3(0, index * spacing, 0);
    }

    // Returns the next available stack position based on how many
    // magnets are currently InStack on this display
    public Vector3 GetNextStackPosition()
    {
        int count = 0;
        foreach (var magnet in assignedMagnets)
        {
            if (magnet != null && magnet.IsInStack())
                count++;
        }
        return GetPositionForIndex(count);
    }

    public int GetStackCount()
    {
        int count = 0;
        foreach (var magnet in assignedMagnets)
        {
            if (magnet != null && magnet.IsInStack())
                count++;
        }
        return count;
    }
}