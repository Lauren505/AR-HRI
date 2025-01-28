using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorPlacement : MonoBehaviour
{
    public GameObject anchorPrefab;
    public OVRInput.RawButton placeAnchorButton;

    void Update()
    {
        if (OVRInput.GetDown(placeAnchorButton, OVRInput.Controller.RTouch))
        {
            CreateSpatialAnchor();
        }
    }

    public void CreateSpatialAnchor()
    {
        GameObject prefab = Instantiate(anchorPrefab, OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch), OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch));
        prefab.AddComponent<OVRSpatialAnchor>();
    }
}
