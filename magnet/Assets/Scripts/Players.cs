using Unity.Netcode;
using UnityEngine;

public class Players : NetworkBehaviour
{
    public float speed = 5f;

    private NetworkVariable<Vector3> netPosition = new NetworkVariable<Vector3>(
        writePerm: NetworkVariableWritePermission.Owner);

    private Magnet carriedMagnet = null;

    public SpriteRenderer spriteRenderer;

    public override void OnNetworkSpawn()
    {
        if (OwnerClientId == 0)
        {
            transform.position = new Vector3(-6, 0, 0);
            spriteRenderer.color = Color.red;
        }
        else
        {
            transform.position = new Vector3(6, 0, 0);
            spriteRenderer.color = Color.blue;
        }

        netPosition.Value = transform.position;
    }

    void Update()
    {
        if (IsOwner)
        {
            HandleMovement();
            HandlePickupDrop();
        }
        else
        {
            transform.position = netPosition.Value;
        }
    }

    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 newPos = transform.position + new Vector3(h, v, 0) * speed * Time.deltaTime;

        transform.position = newPos;
        netPosition.Value = newPos;
    }

    void HandlePickupDrop()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (carriedMagnet == null)
                TryPickUpMagnet();
            else
                DropMagnet();
        }

        if (carriedMagnet != null)
        {
            Vector3 offset = new Vector3(0, 0.5f, 0);
            carriedMagnet.SetPositionServerRpc(transform.position + offset);
        }
    }

    void TryPickUpMagnet()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.7f);

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            Magnet magnet = hit.GetComponent<Magnet>();

            if (magnet != null && !magnet.IsHeld())
            {
                RequestPickupServerRpc(magnet.NetworkObjectId);
                return;
            }
        }
    }

    void DropMagnet()
    {
        if (carriedMagnet == null) return;

        RequestDropServerRpc(carriedMagnet.NetworkObjectId);
        carriedMagnet = null;
    }

    [ServerRpc]
    void RequestPickupServerRpc(ulong magnetId, ServerRpcParams rpcParams = default)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects.ContainsKey(magnetId)) return;

        var netObj = NetworkManager.SpawnManager.SpawnedObjects[magnetId];
        var magnet = netObj.GetComponent<Magnet>();

        if (magnet == null || magnet.IsHeld()) return;

        ulong senderId = rpcParams.Receive.SenderClientId;

        netObj.ChangeOwnership(senderId);
        magnet.PickUp(senderId);

        SetCarriedMagnetClientRpc(magnetId, senderId);
    }

    [ServerRpc]
    void RequestDropServerRpc(ulong magnetId)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects.ContainsKey(magnetId)) return;

        var netObj = NetworkManager.SpawnManager.SpawnedObjects[magnetId];
        var magnet = netObj.GetComponent<Magnet>();

        if (magnet == null) return;

        magnet.Drop();
    }

    [ClientRpc]
    void SetCarriedMagnetClientRpc(ulong magnetId, ulong ownerId)
    {
        if (OwnerClientId != ownerId) return;

        if (!NetworkManager.SpawnManager.SpawnedObjects.ContainsKey(magnetId)) return;

        var netObj = NetworkManager.SpawnManager.SpawnedObjects[magnetId];
        carriedMagnet = netObj.GetComponent<Magnet>();
    }
}