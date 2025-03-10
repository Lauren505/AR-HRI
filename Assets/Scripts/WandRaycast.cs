using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WandRaycast : MonoBehaviour
{
    public LayerMask layerMask;
    public OVRInput.RawButton shootingButton;
    public LineRenderer rayPrefab;
    public GameObject rayImpactPrefab;
    public Transform shootingPoint;
    public float maxLineDistance = 5;
    public float lineShowTimer = 0.3f;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(OVRInput.GetDown(shootingButton))
        {
            Shoot();
        }
    }

    public void Shoot()
    {
        Ray ray = new Ray(shootingPoint.position, shootingPoint.forward);
        bool hasHit = Physics.Raycast(ray, out RaycastHit hit, maxLineDistance, layerMask);

        Vector3 endPoint = Vector3.zero;

        if(hasHit)
        {
            endPoint = hit.point;

            Quaternion rayImpactRotation = Quaternion.LookRotation(-hit.normal);
            GameObject rayImpact = Instantiate(rayImpactPrefab, hit.point, rayImpactRotation);
            Destroy(rayImpact, 1);
        }
        else
        {
            endPoint = shootingPoint.position + shootingPoint.forward * maxLineDistance;
        }
        LineRenderer line = Instantiate(rayPrefab);
        line.positionCount = 2;
        line.SetPosition(0, shootingPoint.position);

        line.SetPosition(1, endPoint);
        Destroy(line.gameObject, lineShowTimer);
    }
}
