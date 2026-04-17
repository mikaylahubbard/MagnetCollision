using Unity.Netcode;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Magnet : NetworkBehaviour
{
    public enum MagnetState { InStack, Held, Placed }

    public GameObject outline;

    public static event Action<Magnet> OnMagnetPickedUp;
    public static event Action<Magnet> OnMagnetDropped;

    public float magnetForce = 25f;
    public float magnetRadius = 1.5f;
    public float snapDistance = 0.5f;
    public float magnetismDuration = 5f;

    private NetworkVariable<ulong> ownerId = new NetworkVariable<ulong>();
    private NetworkVariable<MagnetState> state = new NetworkVariable<MagnetState>();
    private NetworkVariable<Vector3> netPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<float> netRotation = new NetworkVariable<float>();
    private NetworkVariable<bool> isHeld = new NetworkVariable<bool>(false);

    private static Dictionary<ulong, MagnetDisplay> displayByOwner = new();

    private MagnetDisplay homeDisplay;
    private Rigidbody2D rb;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            netPosition.Value = transform.position;
            netRotation.Value = transform.eulerAngles.z;
        }
        if (outline != null) outline.SetActive(false);
    }

    // ------------------------
    // SYNC
    // ------------------------
    void Update()
    {
        if (!IsSpawned) return;

        if (!IsServer)
        {
            transform.position = Vector3.Lerp(transform.position, netPosition.Value, 0.35f);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, netRotation.Value), 0.35f);
        }
        else if (rb.bodyType == RigidbodyType2D.Dynamic)
        {
            netPosition.Value = rb.position;
            netRotation.Value = rb.rotation;
        }
    }

    // ------------------------
    // PHYSICS
    // ------------------------
    void FixedUpdate()
    {
        if (!IsServer || !IsPlaced()) return;

        Magnet[] magnets = FindObjectsOfType<Magnet>();
        Vector2 totalForce = Vector2.zero;

        foreach (var other in magnets)
        {
            if (other == this || !other.IsPlaced()) continue;
            totalForce += GetMagneticForceFrom(other);
        }

        rb.AddForce(totalForce, ForceMode2D.Force);
        rb.linearVelocity *= 0.95f;
    }

    // ------------------------
    // INIT
    // ------------------------
    public void Initialize(ulong owner, Vector3 stackPosition, MagnetDisplay display)
    {
        if (!IsServer) return;

        ownerId.Value = owner;
        state.Value = MagnetState.InStack;
        homeDisplay = display;
        displayByOwner[owner] = display;
        netPosition.Value = stackPosition;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    // ------------------------
    // RPCs
    // ------------------------
    [ServerRpc(RequireOwnership = false)]
    public void SetPositionServerRpc(Vector3 pos)
    {
        if (!isHeld.Value) return;
        rb.MovePosition(pos);
        netPosition.Value = pos;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetRotationServerRpc(float rotationZ)
    {
        if (!isHeld.Value) return;
        rb.MoveRotation(rotationZ);
        netRotation.Value = rotationZ;
    }

    // ------------------------
    // STATE
    // ------------------------
    public void PickUp(ulong newOwnerId)
    {
        if (!IsServer) return;

        ownerId.Value = newOwnerId;
        state.Value = MagnetState.Held;
        rb.bodyType = RigidbodyType2D.Kinematic;
        isHeld.Value = true;

        if (outline != null) outline.SetActive(false);
        OnMagnetPickedUp?.Invoke(this);
    }

    public void Drop()
    {
        if (!IsServer) return;

        state.Value = MagnetState.Placed;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.simulated = true;
        isHeld.Value = false;

        OnMagnetDropped?.Invoke(this);

        StartCoroutine(MagnetismThenReturn(ownerId.Value, homeDisplay));
    }

    // ------------------------
    // CORE FLOW
    // ------------------------
    private IEnumerator MagnetismThenReturn(ulong droppedByOwner, MagnetDisplay droppedByDisplay)
    {
        yield return new WaitForSeconds(magnetismDuration);

        Magnet[] magnets = FindObjectsOfType<Magnet>();

        foreach (var m in magnets)
        {
            if (!m.IsPlaced()) continue;
            m.rb.linearVelocity = Vector2.zero;
            m.rb.angularVelocity = 0f;
            m.rb.bodyType = RigidbodyType2D.Kinematic;
        }

        yield return new WaitForSeconds(0.1f);

        magnets = FindObjectsOfType<Magnet>();

        HashSet<Magnet> toReturn = new HashSet<Magnet>();

        foreach (var m in magnets)
        {
            if (!m.IsPlaced()) continue;
            foreach (var other in magnets)
            {
                if (other == m || !other.IsPlaced()) continue;

                if (Vector2.Distance(m.transform.position, other.transform.position) < snapDistance)
                {
                    toReturn.Add(m);
                    toReturn.Add(other);
                }
            }
        }

        foreach (var m in toReturn)
        {
            Vector3 stackPos = droppedByDisplay.GetNextStackPosition();

            m.state.Value = MagnetState.InStack;
            m.rb.bodyType = RigidbodyType2D.Kinematic;
            m.rb.simulated = true;
            m.isHeld.Value = false;
            m.rb.MovePosition(stackPos);
            m.transform.position = stackPos;
            m.netPosition.Value = stackPos;
            m.netRotation.Value = 0f;
            m.ownerId.Value = droppedByOwner;
            m.homeDisplay = droppedByDisplay;
            m.NetworkObject.ChangeOwnership(droppedByOwner);
        }
    }

    // ------------------------
    // MAGNETISM
    // ------------------------
    public bool IsPlaced() => state.Value == MagnetState.Placed;
    public bool IsHeld() => isHeld.Value;
    public bool IsInStack() => state.Value == MagnetState.InStack;
    public ulong GetOwner() => ownerId.Value;

    private Vector2 GetMagneticForceFrom(Magnet other)
    {
        Vector2 dir = (Vector2)other.transform.position - (Vector2)transform.position;
        float dist = dir.magnitude;

        if (dist > magnetRadius) return Vector2.zero;

        dir.Normalize();
        float strength = Mathf.Clamp(magnetForce / (dist * dist + 0.1f), 0f, 50f);

        return dir * strength;
    }

    // ------------------------
    // HOVER
    // ------------------------
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || !IsClient) return;

        Players player = other.GetComponentInParent<Players>();
        NetworkObject netObj = other.GetComponentInParent<NetworkObject>();

        if (player != null && netObj != null && netObj.IsOwner && !isHeld.Value && IsInStack() && GetOwner() == netObj.OwnerClientId)
        {
            if (outline != null) outline.SetActive(true);
            player.SetHoveredMagnet(this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || !IsClient) return;

        Players player = other.GetComponentInParent<Players>();
        NetworkObject netObj = other.GetComponentInParent<NetworkObject>();

        if (player != null && netObj != null && netObj.IsOwner)
        {
            if (outline != null) outline.SetActive(false);
            player.ClearHoveredMagnet(this);
        }
    }
}