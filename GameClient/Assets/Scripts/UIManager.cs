// Code developed with help from the teachings of
// Tom Weiland's blog on unity integrated client-server architecture
// https://tomweiland.net/networking-tutorial-server-client-connection/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public GameObject startMenu;
    public InputField userNameField;

        private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.Log("Instance already exists.  Destroying object.");
            Destroy(this);
        }
    }

    public void ConnectToServer()
    {
        {
            startMenu.SetActive(false);
            userNameField.interactable = false;
            Client.instance.ConnectToServer();
        }
    }
}
