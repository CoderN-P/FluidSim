using UnityEngine;


namespace GridDemo2
{
    public class BrushController : MonoBehaviour
    {
        public SpriteRenderer circle;
        public float radius = 2f;
        public Color activeColor;
        public Color inactiveColor;
        
        private bool dragging = false;
        private Vector2 lastMousePos;

        void Start()
        {
            circle.transform.localScale = new Vector2(radius * 2f, radius * 2f);
        }
        
        void SetBrushActive(bool isActive)
        {
            circle.color = isActive ? activeColor : inactiveColor;
        }
        
        void Update()
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            transform.position = mousePos;
            
            if (Input.GetMouseButtonDown(0))
            {
                dragging = true;
            }
            
            if (Input.GetMouseButtonUp(0))
            {
                dragging = false;
            }
            
            SetBrushActive(dragging);
            
            if (dragging)
            {
                if (FluidDemo2.Instance.fluidRenderer2 != null)
                    FluidDemo2.Instance.fluidRenderer2.ApplyBrush(mousePos, radius, lastMousePos);
                else 
                    FluidDemo2.Instance.fluidRenderer3.ApplyBrush(mousePos, radius, lastMousePos);
            }
            
            lastMousePos = mousePos;
            
        }
    }
}