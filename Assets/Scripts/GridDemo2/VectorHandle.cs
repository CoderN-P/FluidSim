using UnityEngine;

namespace GridDemo2
{
    public class VectorHandle : MonoBehaviour
    {
        public FluidGrid grid;
        public int i, j;
        public bool isU;
        public float velocityScale = 0.5f; // Scale factor for visualizing velocity
        public float maxArrowLength = 2f;

        private bool isSelected;
        private bool isDragging;

        public SpriteRenderer circle;
        public LineRenderer arrowLine;
        public SpriteRenderer arrowHead;

        private Camera cam;

        public void Init(int i, int j, bool isU, FluidGrid grid, float velocityScale, float maxArrowLength)
        {
            this.i = i;
            this.j = j;
            this.isU = isU;
            this.grid = grid;
            this.velocityScale = velocityScale;
            this.maxArrowLength = maxArrowLength;
        }

        void Start()
        {
            cam = Camera.main;
        }
        
        void Update()
        {
            UpdateArrow();
        }

        public Vector2 GetEndingPos()
        {
            return this.transform.position + arrowLine.GetPosition(1);
        }

        void UpdateArrow()
        {
            Vector2 velocity;
            
            if (isU)
                velocity = new Vector2(grid.velocity[i, j].x, 0);
            else
                velocity = new Vector2(0, grid.velocity[i, j].y);
            
            if (velocity.magnitude < 0.01f)
            {
                arrowLine.enabled = false;
                arrowHead.enabled = false;
                circle.enabled = true;
                return;
            }
            
            arrowLine.enabled = true;
            arrowHead.enabled = true;
            
            arrowLine.useWorldSpace = false;
            arrowLine.positionCount = 2;
            arrowLine.SetPosition(0, Vector3.zero);
            
            Vector2 scaledVelocity = velocity * velocityScale;
            
            if (scaledVelocity.magnitude > maxArrowLength)
                scaledVelocity = scaledVelocity.normalized * maxArrowLength;
            
            arrowLine.SetPosition(1, scaledVelocity);

            arrowHead.transform.localPosition = scaledVelocity;

            if (isU)
            {
                // Points right if positive x, left if negative x
                if (scaledVelocity.x >= 0)
                    arrowHead.transform.localRotation = Quaternion.Euler(0, 0, -90);
                else
                    arrowHead.transform.localRotation = Quaternion.Euler(0, 0, 90);
            }
            else
            {
                // Points up if positive y, down if negative y
                if (scaledVelocity.y >= 0)
                    arrowHead.transform.localRotation = Quaternion.Euler(0, 0, 0);
                else
                    arrowHead.transform.localRotation = Quaternion.Euler(0, 0, 180);
            }
        }
    }
}