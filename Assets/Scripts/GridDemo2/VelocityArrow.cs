using UnityEngine;


namespace GridDemo2
{
    public class VelocityArrow : MonoBehaviour
    {
        public LineRenderer lineRenderer;
        public SpriteRenderer arrowHead;


        public void Draw(Vector2 start, Vector2 end)
        {
            // move whole prefab to start point
            transform.position = start;

            Vector2 dir = end - start;
            float length = dir.magnitude;
            Vector2 norm = dir.normalized;

            // Line (local space)
            lineRenderer.useWorldSpace = false;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, Vector2.zero);
            lineRenderer.SetPosition(1, new Vector2(length, 0));  // draw horizontally

            // Rotate the entire arrow to match direction
            float angle = Mathf.Atan2(norm.y, norm.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            // Arrowhead (local)
            arrowHead.transform.localPosition = new Vector2(length, 0);

            // If arrowhead sprite points up instead of right, rotate it:
            arrowHead.transform.localRotation = Quaternion.Euler(0, 0, -90);
        }
    }
}