using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawingToolExample : MonoBehaviour
{
    public DrawingTool drawingTool;
    private Material myMaterial;
    private bool prevDown = false;
    private bool activated = false;
    private Vector2 prevPoint;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(InitializeDrawToolCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        if (activated && Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Collider collider = GetComponent<Collider>();
            collider.Raycast(ray, out hit, float.MaxValue);

            if (hit.collider != null)
            {
                Vector2 objectPos = transform.InverseTransformPoint(hit.point);
                Vector2 thisPoint = new Vector2(objectPos.x + 0.5f, objectPos.y + 0.5f);
                if (!prevDown)
                {
                    prevPoint = thisPoint;
                }
                drawingTool.DrawLine(prevPoint, thisPoint);
                prevPoint = thisPoint;
            }
        }
        prevDown = Input.GetMouseButton(0);
    }

    IEnumerator InitializeDrawToolCoroutine()
    {
        yield return null;
        if (drawingTool == null)
        {
            yield break;
        }
        myMaterial = GetComponent<MeshRenderer>().material;
        myMaterial.SetTexture("_MainTex", drawingTool.currentDrawing);

        yield return null;

        activated = true;
        Debug.Log("Done");
    }
}
