using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterMovement : NetworkBehaviour
{
    [SerializeField] private Transform spawnObjectPrefab;
    public float moveSpeed = 5f;  // Speed of movement
    public float jumpForce = 5f;  // Force applied when jumping
    public LayerMask groundLayer; // Layer that represents the ground
    public Transform groundCheck; // Transform to check if grounded
    public GameObject mark;
    private Rigidbody rb; // Reference to the Rigidbody component
    private bool isGrounded; // Flag to check if the character is on the ground
    public Animator anim;

    private NetworkVariable<MyCustomData> randomNumber = new NetworkVariable<MyCustomData>(new MyCustomData {
        _int = 56,
        _bool = true,
    }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    void Start()
    {
        // Get the Rigidbody component attached to the GameObject
        rb = GetComponent<Rigidbody>();
    }
    public override void OnNetworkSpawn()
    {
        randomNumber.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue) =>
        {
            Debug.Log(OwnerClientId + " ; RandomNumber: " + newValue._int+" ; "+newValue._bool+" ; "+newValue.message);

        };
    }
    public struct MyCustomData:INetworkSerializable {
        public int _int;
        public bool _bool;
        public FixedString128Bytes message;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T: IReaderWriter
        {
            
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref message);
        }
    }
    Transform spawnObjTransform;
    void Update()
    {
        if(!IsOwner) return;
        //spawn an object
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            spawnObjTransform= Instantiate(spawnObjectPrefab);
            spawnObjTransform.GetComponent<NetworkObject>().Spawn(true);
            //randomNumber.Value = new MyCustomData
            //{
            //    _int = 10,
            //    _bool = false,
            //    message = "All your base are belong to us!",
            //};
        }
        //despawn an object
        if (Input.GetKeyDown(KeyCode.Q))
        {
            spawnObjTransform.GetComponent<NetworkObject>().Despawn(true);
            Destroy(spawnObjTransform.gameObject);
        }

        playerMove(); 
        
    }
    void playerMove()
    {
        // Get input from the player
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.LeftArrow))
        {
            anim.SetBool("Walk", true);
        }
        else
        {
            anim.SetBool("Walk", false);
        }

        // Calculate movement direction based on input
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        // Apply movement to the Rigidbody
        rb.MovePosition(transform.position + movement * moveSpeed * Time.deltaTime);

        // Check if the character is on the ground
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.1f, groundLayer);

        // Handle jumping
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}
