using Unity.Netcode;
using UnityEngine;
using System;

public class Magnet : NetworkBehaviour
{
    [Header("References")]
    public GameObject outline;

    public static event Action<Magnet> OnMagnetPickedUp;
    public static event Action<Magnet> OnMagnetDropped;

    private NetworkVariable<Vector3> netPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<bool> isHeld = new NetworkVariable<bool>(false);

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            netPosition.Value = transform.position;
        }

        if (outline != null)
            outline.SetActive(false);
    }

    void Update()
    {
        if (!IsSpawned) return;

        transform.position = netPosition.Value;
    }

    [ServerRpc]
    public void SetPositionServerRpc(Vector3 pos)
    {
        netPosition.Value = pos;
    }

    public void PickUp(ulong newOwnerId)
    {
        if (!IsServer) return;

        isHeld.Value = true;

        if (outline != null)
            outline.SetActive(false);

        OnMagnetPickedUp?.Invoke(this);
    }

    public void Drop()
    {
        if (!IsServer) return;

        isHeld.Value = false;

        OnMagnetDropped?.Invoke(this);
    }

    public bool IsHeld()
    {
        return isHeld.Value;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var netObj = other.GetComponent<NetworkObject>();

        if (other.CompareTag("Player") && netObj != null && netObj.IsOwner)
        {
            if (!isHeld.Value && outline != null)
                outline.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var netObj = other.GetComponent<NetworkObject>();

        if (other.CompareTag("Player") && netObj != null && netObj.IsOwner)
        {
            if (!isHeld.Value && outline != null)
                outline.SetActive(false);
        }
    }
}