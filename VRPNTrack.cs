﻿//MIT License
//Copyright 2016-Present 
//Ross Tredinnick
//Brady Boettcher
//Living Environments Laboratory
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
//to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, 
//sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
//INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using UnityEngine;
using System.Collections;
using System.Threading;

public class VRPNTrack : MonoBehaviour
{
    [SerializeField]
    private string trackerAddress = "Isense900@C6_V1_HEAD";
    [SerializeField]
    private int channel = 0;
    [SerializeField]
    private bool trackPosition = true;
    [SerializeField]
    private bool trackRotation = true;
    public static Vector3 TrackingSystemOffset = new Vector3(0,0,0);

    public Vector3 trackerRotationOffset;

    public bool debugOutput = false;

    //handles left coordinate system from right based tracking system such as ART.
    public bool convertToLeft = false;

    public int Channel
    {
        get { return channel; }
        set
        {
            channel = value;
        }
    }

    public bool TrackPosition
    {
        get { return trackPosition; }
        set
        {
            trackPosition = value;
            StopCoroutine("Position");
            if (trackPosition && Application.isPlaying)
            {
                StartCoroutine("Position");
            }
        }
    }

    public bool TrackRotation
    {
        get { return trackRotation; }
        set
        {
            trackRotation = value;
            StopCoroutine("Rotation");
            if (trackRotation && Application.isPlaying)
            {
                StartCoroutine("Rotation");
            }
        }
    }

    private void Start()
    {
        if (trackPosition)
        {
            StartCoroutine("Position");
        }

        if (trackRotation)
        {
            StartCoroutine("Rotation");
        }
    }

    private IEnumerator Position()
    {
        while (true)
        {
            Vector3 pos = VRPN.vrpnTrackerPos(trackerAddress, channel);

            if (convertToLeft)
            {
                pos.x = Interlocked.Exchange(ref pos.z, pos.x);
                pos.y *= -1;
                transform.localPosition = pos;
            }

            else
            {
                transform.localPosition = pos;
            }
            //float temp = pos.z;
            //pos.z = pos.x;
            //pos.x = temp;
            // pos.y = -pos.y;

            yield return null;
        }
    }

    private IEnumerator Rotation()
    {
        while (true)
        {
            Quaternion rotation = VRPN.vrpnTrackerQuat(trackerAddress, channel);

            if (convertToLeft)
            {
                rotation.x = Interlocked.Exchange(ref rotation.z, rotation.x);
                rotation.y *= -1;
               
                transform.localRotation =  rotation * Quaternion.Euler(trackerRotationOffset);
            }

            else
            {
				Vector3 fixedRotation = new Vector3();
				fixedRotation.x = trackerRotationOffset.z;
				fixedRotation.y = trackerRotationOffset.y;
				fixedRotation.z = trackerRotationOffset.x;

				transform.localRotation = rotation * Quaternion.Euler(fixedRotation);
            }
            
            yield return null;
        }
    }
}
