using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectorySnap : MonoBehaviour
{
    public float snapDistance = 0.1f;
    public float defaultDistance = 0.1f;
    
    private GameObject currentGhost;
    private GameObject ghostObject;
    private GameObject memTrajectory;
    private GameObject twinConnector;

    private LineRenderer memTrajectoryLine;
    private LineRenderer twinConnectorLine;
    private Vector3[] memTrajectoryPoints = new Vector3[30];

    private bool defaultState = false;

    void Update()
    {
        // Find children
        if (currentGhost == null)
        {
            currentGhost = gameObject.transform.Find("currentGhost").gameObject;
            ghostObject = currentGhost.transform.Find("Ray Cube").gameObject;
        }
        if (memTrajectory == null) 
        {
            memTrajectory = gameObject.transform.Find("memTrajectory").gameObject;
            memTrajectoryLine = memTrajectory.GetComponent<LineRenderer>();
            memTrajectoryLine.GetPositions(memTrajectoryPoints);
        }
        if (twinConnector == null) 
        {
            twinConnector = gameObject.transform.Find("twinConnector").gameObject;
            twinConnectorLine = twinConnector.GetComponent<LineRenderer>();
        }

        // Update trajectory
        if (currentGhost != null && memTrajectory != null && twinConnector != null)
        {
            DrawTwinLine(twinConnectorLine, transform, ghostObject.transform);
            if (defaultState)
            {
                memTrajectoryLine.enabled = true;
                if (isNearTrajectory(ghostObject, memTrajectoryPoints))
                {
                    memTrajectoryLine.material.SetColor("_Color", Color.white);
                }
                else
                {
                    memTrajectoryLine.material.SetColor("_Color", Color.grey);
                }
            }
            else
            {
                memTrajectoryLine.enabled = false;
            }
        }
    }

    private bool isNearTrajectory(GameObject ghost, Vector3[] trajectoryPoints)
    {
        if (trajectoryPoints != null)
        {
            foreach (Vector3 point in trajectoryPoints)
            {
                if (Vector3.Distance(ghost.transform.position, point) <= snapDistance)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void CheckNearOriginalPosition(GameObject ghost)
    {
        if (Vector3.Distance(ghost.transform.position, transform.position) <= defaultDistance)
        {
            defaultState = true;
            return;
        }
        defaultState = false;
    }

    private void DrawTwinLine(LineRenderer line, Transform objectTransform, Transform targetTransform)
    {
        Vector3 startPos = objectTransform.position;
        Vector3 endPos = targetTransform.position;

        line.SetPosition(0, startPos);
        line.SetPosition(1, endPos);
    }
}
