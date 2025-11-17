using UnityEngine;

namespace GridDemo2
{
    public class FluidRenderer2 : MonoBehaviour
    {
        public FluidGrid grid;
        public int velocityResolution = 2; // 4 velocity lines per cell width and height -> 16 lines per cell
        public int padding = 0; // Padding around velocity grid
        public float cellSize = 2f;
        public GameObject vectorHandlePrefab;
        public GameObject velocityArrowPrefab;
        
        private float gridOffsetX;
        private VectorHandle[,] uLines;
        private VectorHandle[,] vLines;
        private VelocityArrow[,] velocityLines;
        private float pickRadius = 0.4f;
        private VectorHandle activeHandle;


        public void Init(FluidGrid g)
        {
            grid = g;
            RebuildGrid();
        }
        
        private VectorHandle FindClosestHandle(Vector2 position)
        {
            VectorHandle closest = null;
            float bestDist = pickRadius;


            for (int i = 0; i < grid.width + 1; i++)
            {
                for (int j = 0; j < grid.height + 1; j++)
                {
                    VectorHandle uHandle = uLines[i, j];
         
                    float d = Vector2.Distance(uHandle.GetEndingPos(), position);
                    
                    if (d < bestDist)
                    {
                        bestDist = d;
                        closest = uHandle;
                    }
                    
                    VectorHandle vHandle = vLines[i, j];
                    d = Vector2.Distance(vHandle.GetEndingPos(), position);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        closest = vHandle;
                    }
                }
            }
            return closest;
        }
        
        private void DragHandle(VectorHandle h, Vector2 position)
        {
            Vector2 startPos = h.transform.position;
            
            Vector2 newVelocity = (position - startPos)/h.velocityScale;
            
            if (h.isU)
            {
                h.grid.velocity[h.i, h.j] = new Vector2(newVelocity.x, h.grid.velocity[h.i, h.j].y);
            }
            else
            {
                h.grid.velocity[h.i, h.j] = new Vector2(h.grid.velocity[h.i, h.j].x, newVelocity.y);
            }
        }

        void UpdateInput()
        {
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (Input.GetMouseButtonDown(0))
                activeHandle = FindClosestHandle(mouseWorld);

            if (Input.GetMouseButton(0) && activeHandle != null)
                DragHandle(activeHandle, mouseWorld);

            if (Input.GetMouseButtonUp(0))
                activeHandle = null;
        }
        
        public void ApplyBrush(Vector2 pos, float radius, Vector2 lastPos)
        {
            // Apply velocity brush at position pos with given radius based on delta from lastPos
         
            for (int i = 1; i <= grid.width; i++)
            {
                for (int j = 1; j <= grid.height; j++)
                {
                    Vector2 cellPos = IndexToWorldPos(new Vector2(i, j));
                    float dist = Vector2.Distance(pos, cellPos);
                    if (dist < radius)
                    {
                        Vector2 delta = pos - lastPos;
                        float strength = (radius - dist) / radius; // Linear falloff
                        grid.velocity[i, j] += delta * strength * 5f; // Apply velocity change
                    }
                }
            }
        }
        
        
        Vector2 IndexToWorldPos(Vector2 index)
        {
            index.y = grid.height + 1 - (index.y); // Flip y-axis
            return new Vector2(gridOffsetX + index.x * cellSize, 3*padding + (index.y) * cellSize); // top left corner
        }
        
        void DeleteExistingHandles()
        {
            if (uLines != null)
            {
                foreach (var handle in uLines)
                {
                    if (handle != null)
                        Destroy(handle.gameObject);
                }
            }
            if (vLines != null)
            {
                foreach (var handle in vLines)
                {
                    if (handle != null)
                        Destroy(handle.gameObject);
                }
            }
        }

        void RebuildGrid()
        {
            DeleteExistingHandles();
            
            Camera cam = Camera.main;

            float totalHeight = (grid.height + 2) * cellSize + 2f * padding;
            float totalWidth  = (grid.width+2)  * cellSize + 2f * padding;

            // Set camera size
            cam.orthographicSize = totalHeight * 0.5f;
            
            gridOffsetX = (cam.orthographicSize * cam.aspect - totalWidth * 0.5f); // Center grid horizontally

            // Position camera so bottom-left is (0,0)
            float cameraX = totalWidth  * 0.5f;
            float cameraY = totalHeight * 0.5f;
            cam.transform.position = new Vector3(cameraX, cameraY, -10f);
            
            Debug.DrawLine(new Vector3(0,0,0), new Vector3(1,0,0), Color.red, 10);
            Debug.DrawLine(new Vector3(0,0,0), new Vector3(0,1,0), Color.green, 10);
            
            InitializeVectorHandles();
            InitializeVelocityLines();
        }

        void InitializeVectorHandles()
        {
            uLines = new VectorHandle[grid.width + 2, grid.height + 2];
            vLines = new VectorHandle[grid.width + 2, grid.height + 2];

            for (int i = 0; i < grid.width + 1; i++)
            {
                for (int j = 0; j < grid.height + 1; j++)
                {
                    Vector2 worldPos = IndexToWorldPos(new Vector2(i, j));

                    // U handle
                    Vector2 uPos = worldPos;
                    uPos.y -= cellSize * 0.5f; // Center vertically
                    var uObj = Instantiate(vectorHandlePrefab, uPos, Quaternion.identity, transform);
                    var uHandle = uObj.GetComponent<VectorHandle>();
                    uHandle.Init(i, j, true, grid, 0.5f, 3f);
                    uLines[i, j] = uHandle;

                    // V handle
                    Vector2 vPos = worldPos;
                    vPos.x += cellSize * 0.5f; // Center horizontally
                    vPos.y -= cellSize; // Move to bottom
                    var vObj = Instantiate(vectorHandlePrefab, vPos, Quaternion.identity, transform);
                    var vHandle = vObj.GetComponent<VectorHandle>();
                    vHandle.Init(i, j, false, grid, 0.5f, 3f);
                    vLines[i, j] = vHandle;
                }
            }
        }

        void InitializeVelocityLines()
        {
            velocityLines = new VelocityArrow[velocityResolution * (grid.width + 2),
                velocityResolution * (grid.height + 2)]; // Interpolated velocity


            for (int i = 0; i < grid.width + 2; i++)
            {
                for (int j = 0; j < grid.height + 2; j++)
                {
                    for (int vi = 0; vi < velocityResolution; vi++)
                    {
                        for (int vj = 0; vj < velocityResolution; vj++)
                        {
                            Vector2 worldPos = IndexToWorldPos(new Vector2(i, j));
                            worldPos.x += (vi / (float)velocityResolution) * cellSize;
                            worldPos.y -= (vj / (float)velocityResolution) * cellSize;

                            var velocityObj = Instantiate(velocityArrowPrefab, worldPos, Quaternion.identity, transform);
                            var velocityArrow = velocityObj.GetComponent<VelocityArrow>();
                            velocityLines[i * velocityResolution + vi, j * velocityResolution + vj] = velocityArrow;
                        }
                    }
                }
            }

        }

        void UpdateVelocityLines()
        {
            for (int i = 0; i < grid.width + 2; i++)
            {
                for (int j = 0; j < grid.height + 2; j++)
                {
                    for (int vi = 0; vi < velocityResolution; vi++)
                    {
                        for (int vj = 0; vj < velocityResolution; vj++)
                        {
                            Vector2 worldPos = IndexToWorldPos(new Vector2(i, j));
                            worldPos.x += (vi / (float)velocityResolution) * cellSize;
                            worldPos.y -= (vj / (float)velocityResolution) * cellSize;

                            // Bilinear interpolation of velocity
                            float fx = vi / (float)velocityResolution;
                            float fy = vj / (float)velocityResolution;

                            float u00 = grid.velocity[i, j].x;
                            float u10 = grid.velocity[Mathf.Min(i + 1, grid.width + 1), j].x;
                            float v00 = grid.velocity[i, Mathf.Max(j - 1, 0)].y;
                            float v01 = grid.velocity[i, j].y;

                            Vector2 finalVel;

                            finalVel.x = Mathf.Lerp(u00, u10, fx);
                            finalVel.y = Mathf.Lerp(v00, v01, fy);
                            Vector2 start = worldPos;
                            Vector2 end = start + finalVel * 0.1f;
                            var velocityArrow = velocityLines[i * velocityResolution + vi, j * velocityResolution + vj];
                            velocityArrow.Draw(start, end);
                        }
                    }
                }

            }
        }
        

        public void Render()
        {
            UpdateVelocityLines();
            UpdateInput();
        }
    }
}