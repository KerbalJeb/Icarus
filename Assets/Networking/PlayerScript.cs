using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerScript : NetworkBehaviour
{
    //Stuff that happens when server is started
    public override void OnStartServer()
    {
        base.OnStartServer();
        //Add stuff here for when server is started

        Debug.Log("Server started");
    }

    //Stuff that happens when server is stopped
    public override void OnStopServer()
    {
        base.OnStopServer();
        //Add stuff here for when server is stopped

        Debug.Log("Server stopped");
    }

    [SerializeField] private Vector3 movement = new Vector3();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasAuthority) { return; }

        if (!Input.GetKeyDown(KeyCode.Space)) { return; }

        transform.Translate(movement);
    }
}
