using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [SerializeField] private float maxZoom;
    [SerializeField] private float minZoom;
    [SerializeField] private float zoomSpeed;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed;
    private Camera cam;
    private float _zoomCoeff;


    private Vector3 startInputPos;
    private Vector3 prevMousePos;
    void Start()
    {
        cam = GetComponent<Camera>();
    }
    
    
    void Update()
    {
        var scrollAmount = Input.GetAxis("Mouse ScrollWheel");
        if (scrollAmount != 0)
        {

            _zoomCoeff = Mathf.Clamp(_zoomCoeff * scrollAmount, maxZoom, minZoom);
            ZoomCamera(scrollAmount);
        }
        

        if (Input.GetMouseButton(2))
        {
            Vector2 rot = Vector3.Project(Input.mousePosition - new Vector3(Screen.width / 2f, Screen.height / 2f, 0), Vector3.right) / Screen.width;
            transform.RotateAround(transform.position, Vector3.up, rot.x * rotationSpeed * Time.deltaTime);
        }

        if (Input.GetMouseButtonDown(1))
        {
            startInputPos = Input.mousePosition - new Vector3(Screen.width / 2f , Screen.height / 2f, 0);
        }

        if (Input.GetMouseButton(1))
        {

            var current = Input.mousePosition - new Vector3(Screen.width / 2f , Screen.height / 2f, 0);
            var delta = current.normalized;
            transform.localPosition += new Vector3(delta.x, 0,delta.y) * (moveSpeed * Time.deltaTime);
            prevMousePos = current;
        }
    }

    public void ZoomCamera(float amount)
    {
        transform.localPosition += transform.forward * (Mathf.Sign(amount) * zoomSpeed * Time.deltaTime);
    }
}
