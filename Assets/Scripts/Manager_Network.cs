using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode.Transports.UTP;
using System.Collections;

/// <summary>
/// 
/// Author: Michael Knighen
/// 
/// Date Started: 8_21_2024
/// 
/// Last Updated: NULL
/// 
///  <<<DON'T TOUCH MY CODE>>>
///  
/// --------------------------------------------------------------------------------------------------------
/// 
/// Initialization 8_21_2024:
/// 
/// Description:
/// 
/// The Manager Network script is was allows the user to declare themselves a server host or a client to connect to a host.
/// 
/// This was originally designed for my personal game and was adapted for this project.
/// 
/// Package:
/// 
/// Unity Netcode for GameObjects
/// 
/// Note:
/// 
/// Singletons carry over from scene to scene and allow for persistant data.
/// 
/// A port number is a way to identify specific processes or services on a network.
/// 
/// Both client and Host must have the same Port Number to connect.
/// 
/// You need both the Port Number and the IP Address to connect.
/// 
/// This is a work in progress.
/// 
/// --------------------------------------------------------------------------------------------------------
/// </summary>

public class Manager_Network : MonoBehaviour
{
    // Attributes:=--------------------------------------------------------------------------------------------------------------

    // Buttons:
    public Button Host;
    public Button Client;

    // Networking:
    public TMP_InputField IPAddressInput;
    private string hostIP;
    private string ipAddress;
    private readonly ushort portNumber = 7777;


    // Life_Cycle Methods:-----------------------------------------------------------------------------------------------------

    private void Start()
    {
        // Assign button listeners:
        Host.onClick.AddListener(StartHost);
        Client.onClick.AddListener(TryStartClient);

    }

    // Network Methods:---------------------------------------------------------------------------------------------------

    private void StartHost()
    {
        // Start the game as a Host (Both Server and Client):

        // Relay IP Address:
        ipAddress = GetLocalIPAddress();
        Debug.Log($"Local IP Address: {ipAddress} and Port Numner: " + portNumber);

        //Establish IP Address and Port Number
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ipAddress, portNumber);

        // Host game:
        NetworkManager.Singleton.StartHost();
        HideClientHostUI();
    }

    private void TryStartClient()
    {
        // Update hostIP with the current input field value
        hostIP = IPAddressInput.text;

        // Start a coroutine to attempt client connection
        StartCoroutine(StartClientCoroutine());
    }

    private IEnumerator StartClientCoroutine()
    {
        // Get the IP address input
        hostIP = IPAddressInput.text;

        // Set connection data
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(hostIP, portNumber);

        // Start the client
        NetworkManager.Singleton.StartClient();

        // Timeout duration (in seconds)
        float timeoutDuration = 10f;
        float timer = 0f;

        // Wait until the client connects or fails, or timeout occurs
        while (timer < timeoutDuration)
        {
            if (NetworkManager.Singleton.IsClient && NetworkManager.Singleton.IsConnectedClient)
            {
                Debug.Log("Client connected successfully.");
                HideClientHostUI();
                yield break; // Exit the coroutine
            }

            timer += Time.deltaTime; // Increment the timer
            yield return null; // Wait for the next frame
        }

        // Timeout reached or failed to connect
        Debug.LogError("Failed to connect to the server. Please try again.");
        // Optionally, provide feedback to the user
    }


    public static string GetLocalIPAddress()
    {
        string localIP = "Unable to determine IP address";

        try
        {
            foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                // Check if the IP address is IPv4 and not a loopback address
                if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                {
                    localIP = ip.ToString();
                    break; // Use the first valid IP address
                }
            }
        }

        // Error Catch:
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception while getting local IP address: {ex.Message}");
        }

        return localIP;
    }

    // UI Methods:---------------------------------------------------------------------------------------------------------

    private void HideClientHostUI()
    {
        // Hide after use:
        Host.gameObject.SetActive(false);
        Client.gameObject.SetActive(false);
        IPAddressInput.gameObject.SetActive(false);
    }
}
