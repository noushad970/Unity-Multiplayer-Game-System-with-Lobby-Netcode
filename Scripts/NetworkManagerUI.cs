using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button serverbtn;
    [SerializeField] private Button clientbtn;
    [SerializeField] private Button hostbtn;

    private void Awake()
    {
        serverbtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
        });
        clientbtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
        });
        hostbtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
        });
    }
}
