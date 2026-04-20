using Unity.Netcode;
using UnityEngine;
using SQLite;
using System.Collections.Generic;
public class Players : NetworkBehaviour
{
    public float speed = 5f;

    private NetworkVariable<Vector3> netPosition = new NetworkVariable<Vector3>(
        writePerm: NetworkVariableWritePermission.Owner);

    private Magnet carriedMagnet = null;
    private Magnet hoveredMagnet = null;

    public SpriteRenderer spriteRenderer;

    private float currentRotation = 0f;

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
        // Block input if it's not this player's turn or turn is resolving
        if (GameManager.Instance != null && !GameManager.Instance.IsMyTurn(OwnerClientId))
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (carriedMagnet == null)
                TryPickUpMagnet();
            else
                DropMagnet();
        }

        if (carriedMagnet != null)
        {
            carriedMagnet.SetPositionServerRpc(transform.position + new Vector3(0, 0.5f, 0));
            HandleMagnetRotation();
        }
    }

    void HandleMagnetRotation()
    {
        float rotationSpeed = 150f;
        float input = 0f;

        if (Input.GetKey(KeyCode.Q)) input += 1f;
        if (Input.GetKey(KeyCode.E)) input -= 1f;

        if (input != 0f)
        {
            currentRotation += input * rotationSpeed * Time.deltaTime;
            carriedMagnet.SetRotationServerRpc(currentRotation);
        }
    }

    void TryPickUpMagnet()
    {
        if (hoveredMagnet == null) return;

        if (hoveredMagnet.GetOwner() == OwnerClientId && hoveredMagnet.IsInStack())
        {
            currentRotation = 0f;
            RequestPickupServerRpc(hoveredMagnet.NetworkObjectId);
        }
    }

    void DropMagnet()
    {
        if (carriedMagnet == null) return;

        RequestDropServerRpc(carriedMagnet.NetworkObjectId);
        carriedMagnet = null;
    }

    public void SetHoveredMagnet(Magnet magnet) => hoveredMagnet = magnet;

    public void ClearHoveredMagnet(Magnet magnet)
    {
        if (hoveredMagnet == magnet)
            hoveredMagnet = null;
    }

    [ServerRpc]
    void RequestPickupServerRpc(ulong magnetId, ServerRpcParams rpcParams = default)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects.ContainsKey(magnetId)) return;

        var magnet = NetworkManager.SpawnManager.SpawnedObjects[magnetId].GetComponent<Magnet>();
        if (magnet == null || magnet.IsHeld()) return;

        ulong senderId = rpcParams.Receive.SenderClientId;
        if (magnet.GetOwner() != senderId || !magnet.IsInStack()) return;

        magnet.PickUp(senderId);
        SetCarriedMagnetClientRpc(magnetId, senderId);
    }

    [ServerRpc]
    void RequestDropServerRpc(ulong magnetId)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects.ContainsKey(magnetId)) return;

        var magnet = NetworkManager.SpawnManager.SpawnedObjects[magnetId].GetComponent<Magnet>();
        if (magnet == null) return;

        magnet.Drop();
    }

    [ClientRpc]
    void SetCarriedMagnetClientRpc(ulong magnetId, ulong ownerId)
    {
        if (OwnerClientId != ownerId) return;
        if (!NetworkManager.SpawnManager.SpawnedObjects.ContainsKey(magnetId)) return;

        carriedMagnet = NetworkManager.SpawnManager.SpawnedObjects[magnetId].GetComponent<Magnet>();
    }
}