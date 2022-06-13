using System;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;

class MultiplayerConnect:MonoBehaviour
{
    public static void ConnectBtn()
    {
        TextMeshProUGUI connectionStatus = GameObject.Find("ConnectionStatus").GetComponent<TextMeshProUGUI>();
        TMP_InputField IPAddressInputText = GameObject.Find("IPAddressInput").GetComponent<TMP_InputField> ();
        if (!Regex.IsMatch(IPAddressInputText.text, "(?:[0-9]{1,3}\\.){3}[0-9]{1,3}"))
        {
            connectionStatus.text = "Entered IP address is incorrect.";
            return;
        }
        connectionStatus.text = "Connection...";
        Client client = FindObjectsOfType<Client>()[0];
        string resp = IPAddressInputText.text;
        client.Connecta(resp);
    }
}

