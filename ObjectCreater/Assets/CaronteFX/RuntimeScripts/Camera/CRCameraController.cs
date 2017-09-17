using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CRCameraController : MonoBehaviour 
{
	public float zoomSpeed   = 10.0F;
	public float moveSpeed   = 1.9F;
	public float rotateSpeed = 4.0F;
    
	private GameObject orbitVector = null;
  private List<GameObject> selectedObjects = new List<GameObject>();

	void Start () 
  {
		orbitVector = new GameObject();
		orbitVector.name = "Camera Orbit";
    Ray ray = new Ray(transform.position, transform.forward);
    orbitVector.transform.position = ray.GetPoint(10);
    orbitVector.transform.rotation = transform.rotation;

    transform.LookAt(orbitVector.transform.position, Vector3.up);
	}

	void LateUpdate () 
  {
    float x = Input.GetAxis("Mouse X");
    float y = Input.GetAxis("Mouse Y");

    if (Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.LeftAlt))
    {
        float distanceToOrbit = Vector3.Distance(transform.position, orbitVector.transform.position);

        //RMB - ZOOM
        if (Input.GetMouseButton(1))
        {
            float currentZoomSpeed = Mathf.Clamp(zoomSpeed * (distanceToOrbit /  50),  0.1F, 2.0F);
            transform.Translate(Vector3.forward * ( x * currentZoomSpeed));

            //if about to collide with the orbitVecto, repulse the orbitVector slightly to keep it in fornt of us
            if (Vector3.Distance(transform.position, orbitVector.transform.position) < 3)
            {
                orbitVector.transform.Translate(Vector3.forward, transform);
            }
        }
        //LMB - PIVOT
        else if (Input.GetMouseButton(0))
        {
            //Refine the rotateSpped based on distance to orbitVector
            float currentRotateSpeed = Mathf.Clamp(rotateSpeed * (distanceToOrbit / 50), 1.0f, rotateSpeed);

            //Temporarily parent the camera to orbitVector and rotate orbirVector as desired
            transform.parent = orbitVector.transform;
            orbitVector.transform.Rotate(Vector3.right * (y * currentRotateSpeed));
            orbitVector.transform.Rotate(Vector3.up * (x * currentRotateSpeed), Space.World);
            transform.parent = null;
        }
        //MMB - PAN
        else if (Input.GetMouseButton(2))
        {
            Vector3 translateX = Vector3.right * (x * moveSpeed) * -1.0F;
            Vector3 translateY = Vector3.up * (y * moveSpeed) * -1.0F;

            //Move the camera
            transform.Translate(translateX);
            transform.Translate(translateY);


            //Move the orbitVector with the same values, along the camera's axes. In effect causing it to behave as if temporarily parented.
            orbitVector.transform.Translate(translateX, transform);
            orbitVector.transform.Translate(translateY, transform);
        }
    }
    //Fetch input of the F-button (focus) -- this is a fast implementation...
    else if (Input.GetKeyDown("f"))
    {
        Vector3 backtrack = new Vector3(0, 0, -10);
        GameObject selectedObject;
        if (selectedObjects.Count == 1)
        {
            selectedObject = selectedObjects[0];
            transform.position = selectedObject.transform.position;
            orbitVector.transform.position = selectedObject.transform.position;
            transform.Translate(backtrack);
        }
        else if (selectedObjects.Count > 1)
        {
            selectedObject = selectedObjects[0];
            Vector3 average = selectedObject.transform.position;

            for (int i = 0; i < selectedObjects.Count; i++)
            {
                selectedObject = selectedObjects[i];
                average = (average + selectedObject.transform.position) / 2;
            }

            transform.position = average;
            orbitVector.transform.position = average;
            transform.Translate(backtrack);
        }
    }
  }
	//--------------------------------------------------------------------
}


