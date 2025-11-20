using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace Poltergeist;

public class RPCTransform : NetworkBehaviour
{
    private float posLastUpdateTime = 0;
    private float rotLastUpdateTime = 0;
    private float curUpdateDelay = 0;
    private float updateDist = 0.1f;
    private Vector3 lastPos = Vector3.zero;
    private Vector3 lastRot = Vector3.zero;

    //Min delay between transform checks.
    //Higher numbers will lead to less stable behavior, but less network load
    private float updateDelay = 0;

    /**
     * If we own this, have it update others on the transform
     */
    private void Update()
    {
        //Would be bad if non-owners were trying to update transforms
        if (!IsOwner)
            return;

        //Only want to do any checks if it's time
        curUpdateDelay -= Time.deltaTime;
        if(curUpdateDelay <= 0)
        {
            curUpdateDelay = updateDelay;

            //If we need to update position, do so
            if(Vector3.Distance(lastPos, transform.position) >= updateDist)
            {
                UpdatePosServerRPC(transform.position.x, transform.position.y, transform.position.z, Time.time);
                lastPos = transform.position;
            }

            //If we need to update rotation, do so
            if(Vector3.Distance(lastRot, transform.eulerAngles) >= updateDist)
            {
                UpdateRotServerRPC(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z, Time.time);
                lastRot = transform.eulerAngles;
            }
        }
    }

    /**
     * Lets the client tell the server to update their position
     */
    [ServerRpc]
    private void UpdatePosServerRPC(float x, float y, float z, float time)
    {
        UpdatePosClientRPC(x, y, z, time);
    }

    /**
     * Lets the server tell clients to update the position
     */
    [ClientRpc]
    private void UpdatePosClientRPC(float x, float y, float z, float time)
    {
        UpdatePosLocally(x, y, z, time);
    }

    /**
     * Updates the position locally
     */
    private void UpdatePosLocally(float x, float y, float z, float time)
    {
        //If we're the owner or have a more recent update, we don't need to update it
        if (IsOwner || time < posLastUpdateTime)
            return;

        //Otherwise, set the position
        //(figure out interpolation later down the line?)
        transform.position = new Vector3(x, y, z);
        posLastUpdateTime = time;
    }

    /**
     * Lets the client tell the server to update their rotation
     */
    [ServerRpc]
    private void UpdateRotServerRPC(float x, float y, float z, float time)
    {
        UpdateRotClientRPC(x, y, z, time);
    }

    /**
     * Lets the server tell clients to update the rotation
     */
    [ClientRpc]
    private void UpdateRotClientRPC(float x, float y, float z, float time)
    {
        UpdateRotLocally(x, y, z, time);
    }

    /**
     * Updates the rotation locally
     */
    private void UpdateRotLocally(float x, float y, float z, float time)
    {
        //If we're the owner or have a more recent update, we don't need to update it
        if (IsOwner || time < rotLastUpdateTime)
            return;

        //Otherwise, set the rotation
        //(figure out interpolation later down the line?)
        transform.eulerAngles = new Vector3(x, y, z);
        rotLastUpdateTime = time;
    }
}
