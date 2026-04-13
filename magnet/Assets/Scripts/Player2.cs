
using Unity.Netcode;
using UnityEngine;

public class Player2 : NetworkBehaviour
{
    private float speed = 10f;
    private Rigidbody2D rb;

    private NetworkVariable<float> yPosition = new NetworkVariable<float>(0f);
    private NetworkVariable<float> xPosition = new NetworkVariable<float>(0f);

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (IsOwner)
        {
            Debug.Log("Controlling player: " + OwnerClientId);
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 newPos = transform.position +
                new Vector3(horizontal, vertical, 0) * 5f * Time.deltaTime;

            transform.position = newPos;

            xPosition.Value = newPos.x;
            yPosition.Value = newPos.y;
        }
        else
        {
            transform.position = new Vector3(xPosition.Value, yPosition.Value, 0);
        }
    }

}