using UnityEngine;

namespace GridDemo2
{
    public class FluidRenderer3 : MonoBehaviour
    {
        public FluidGrid grid;
        public float padding;
        public float cellSize = 1f;
        public GameObject velocityArrowPrefab;
        public GameObject squareCellPrefab;
        public Gradient divergenceGradient;
        public Gradient speedGradient;
        public float maxSpeedForColor = 5f;
        public float gapRatio = 0.1f;
        public VisualizationMode visMode = VisualizationMode.Divergence;
        public bool showVelocityVectors = true;
        
        
        private float gridOffsetX;
        private VelocityArrow[,] velocityLines;
        private SpriteRenderer[,] cellSquares;
        private float pickRadius = 0.4f;
        
        public void Init(FluidGrid g)
        {
            grid = g;
            RebuildGrid();
        }

        public void Render()
        {
            UpdateVelocityLines(); // Update velocity arrows to match grid velocities
            UpdateCellSquares(); // Change color based on divergence
        }
        
        void RebuildGrid()
        {
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

            InitializeCellSquares();
            InitializeVelocityLines();
        }

        void InitializeCellSquares()
        {
            cellSquares = new SpriteRenderer[grid.width + 2, grid.height + 2];
            for (int i = 0; i < grid.width + 2; i++)
            {
                for (int j = 0; j < grid.height + 2; j++)
                {
                    Vector2 worldPos = IndexToWorldPos(new Vector2(i, j));
                    GameObject cellObj = Instantiate(squareCellPrefab, transform);
                    cellObj.transform.position = new Vector3(worldPos.x+0.5f, worldPos.y-0.5f, 0); // Top corner at worldPos
                    cellObj.transform.localScale = Vector3.one * cellSize * (1 - gapRatio);
                    cellSquares[i, j] = cellObj.GetComponent<SpriteRenderer>();
                }
            }
        }

        void InitializeVelocityLines()
        {
            // 1 velocity arrow per cell (centered)
            velocityLines = new VelocityArrow[grid.width + 2, grid.height + 2];
            
            for (int i = 0; i < grid.width + 2; i++)
            {
                for (int j = 0; j < grid.height + 2; j++)
                {
                    Vector2 worldPos = IndexToWorldPos(new Vector2(i, j));
                    worldPos += new Vector2(0.5f, -0.5f); // Center of cell
                    
                    var arrowObj = Instantiate(velocityArrowPrefab, worldPos, Quaternion.identity, transform);
                    var arrow = arrowObj.GetComponent<VelocityArrow>();
                    arrow.SetScale(2);
                    velocityLines[i, j] = arrow;
                }
            }
        }
        
        void UpdateCellSquares()
        {
            // Update cell colors based on divergence
            for (int i = 0; i <= grid.width + 1; i++)
            {
                for (int j = 0; j <= grid.height + 1; j++)
                {
                    if (grid.solid[i, j] == 0)
                    {
                        cellSquares[i, j].color = new Color(0.1f, 0.1f, 0.1f); // Solid cell color
                        continue;
                    }

                    if (visMode == VisualizationMode.Divergence)
                    {
                        float div = FluidDemo2.Instance.velocitySolver.GetDivergenceAtCell(i, j);
                        float t = Mathf.InverseLerp(-1f, 1f, div); // Map divergence to [0,1]
                        cellSquares[i, j].color = divergenceGradient.Evaluate(t);
                    } else if (visMode == VisualizationMode.Speed)
                    {
                        float speed = grid.velocity[i, j].magnitude;
                        float t = Mathf.InverseLerp(0f, maxSpeedForColor, speed); // Map speed to [0,1]
                        cellSquares[i, j].color = speedGradient.Evaluate(t);
                    }
                }
            }
        }
        
        void UpdateVelocityLines()
        {
            // Update velocity arrows to match grid velocities
            for (int i = 0; i < grid.width + 2; i++)
            {
                for (int j = 0; j < grid.height + 2; j++)
                {
                    if (grid.solid[i, j] == 0)
                    {
                        velocityLines[i, j].SetVisible(false);
                        continue;
                    }
                    Vector2 start = IndexToWorldPos(new Vector2(i, j));
                    start += new Vector2(0.5f, -0.5f); // Center of cell
                    
                    Vector2 vel = grid.velocity[i, j];
                    vel.y *= -1; // Flip y for world space
                    Vector2 end = start + vel * 0.1f;
                    velocityLines[i, j].Draw(start, end);
                    
                    velocityLines[i, j].SetVisible(showVelocityVectors);
                }
            }
        }
        
        Vector2 IndexToWorldPos(Vector2 index)
        {
            index.y = grid.height + 2 - (index.y); // Flip y-axis
            return new Vector2(gridOffsetX + index.x * cellSize, padding + (index.y) * cellSize); // top left corner
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
                        delta.y *= -1; // Flip y for grid space
                        float strength = (radius - dist) / radius; // Linear falloff
                        grid.velocity[i, j] += delta * strength * 5f; // Apply velocity change
                    }
                }
            }
        }
    }
}