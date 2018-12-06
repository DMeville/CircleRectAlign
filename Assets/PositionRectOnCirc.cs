using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
public class PositionRectOnCirc : MonoBehaviour {

    //important stuff.
    //we don't move rect directly when aligning to the circle. Instead we move the proxy.  
    //We do this with a proxy because when we move the real position the calculation freaks out if we're doing it in update. If you're just doing it once it's probably fine to remove it
    public Image circ;
    public Image rect; //the real position of the image, we 
    public Image rectImage; //the "moved" position of the image.
    public float radius = 1;


    //bunch of helper images to see what's going on.
    public Image intersectionMarker;
    public Image intersectionMarker2;
    public Image intersectionMarkerOnRectEdge;
    public Image intersectionMarkerOnCircEdge;
    public Image line1;
    public Image line2;

    public float lineWidth = 2f;
    public List<Vector2> intersections = new List<Vector2>();
    public List<Image> intersectionMarkers = new List<Image>();

    Vector2 direction = new Vector2();
    public void Update() {
        
        //get all four corners of the real rect image in worldspace
        Vector3[] c = new Vector3[4];
        rect.rectTransform.GetWorldCorners(c);

        // markers to see what wer're doing,
        //line1.rectTransform.position = c[0];
        //line2.rectTransform.position = c[1];
        //intersectionMarker2.rectTransform.position = rect.transform.position; //center

        //Get the position of the circle edge towards the rect center
        Vector3 dir = rect.rectTransform.position - circ.rectTransform.position;
        Vector2 circEdgePos = new Vector2(circ.rectTransform.position.x + (dir.x / dir.magnitude * radius), circ.rectTransform.position.y + (dir.y / dir.magnitude * radius)); //what if we scale the circle by rect scale? NOPE didn't work

        //marker
        //intersectionMarkerOnCircEdge.rectTransform.position = circEdgePos;

        //check the intersections to find the one edge we intersect with
        intersections = new List<Vector2>();
        CheckIntersection(c, 0, 1);
        CheckIntersection(c, 1, 2);
        CheckIntersection(c, 2, 3);
        CheckIntersection(c, 3, 0);

        //markers
        //for(int i = 0; i < intersections.Count; i++) {
        //    intersectionMarkers[i].rectTransform.position = intersections[i];
        //}

        //actually move the image to the correct position. Note the image is a PROXY to the real object, because we don't want to move the real objects position otherwise the calculations freak out
        rectImage.rectTransform.position = circEdgePos - intersections[0] + (Vector2)rect.rectTransform.position;
    }

    public void CheckIntersection(Vector3[] c, int a, int b) { //should only get one intersection becasue we're doing line-segment vs line-segment, along rect edges against the segment made by connecting the circle and rect centers
        Vector2 intersection = Vector2.zero;
        if(LineSegmentsIntersection(circ.rectTransform.transform.position, rect.rectTransform.position, c[a], c[b], out intersection)) {

            float lx = Mathf.InverseLerp(c[a].x, c[b].x, intersection.x);
            float ly = Mathf.InverseLerp(c[a].y, c[b].y, intersection.y);

            //float sx = Mathf.SmoothStep(c[a].x, c[b].x, Mathf.SmoothStep(0, 1, lx)); //this is the issue when using scales.
            //float sy = Mathf.SmoothStep(c[a].y, c[b].y, Mathf.SmoothStep(0, 1, ly));
            float sx = Mathf.Lerp(c[a].x, c[b].x, CustomLerpCurve(lx, rect.rectTransform.localScale.y)); //this is the issue when using scales.
            float sy = Mathf.Lerp(c[a].y, c[b].y, CustomLerpCurve(ly, rect.rectTransform.localScale.x));

            intersections.Add(new Vector2(sx, sy));
        } else {
            Debug.Log(string.Format("{0} - {1} intersection failed", a, b));
        }
    }

    public float CustomLerpCurve(float t, float rectScale) {

        return Mathf.SmoothStep(0, 1f, t);
        //x*x +y * Y = 1;
    }

    //mathimagical
    public bool LineSegmentsIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 intersection) {
        intersection = Vector2.zero;

        var d = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);

        if(d == 0.0f) {
            return false;
        }

        var u = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;
        var v = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / d;

        if(u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f) {
            return false;
        }

        intersection.x = p1.x + u * (p2.x - p1.x);
        intersection.y = p1.y + u * (p2.y - p1.y);

        return true;
    }
}
