using TMPro;
using UnityEngine;

namespace Subsystems
{

    public class FluidRenderer : MonoBehaviour
    {
        public FluidGrid grid;
        public TMP_Text textPrefab;
        public GameObject linePrefab; 
        public GameObject lineVelocityPrefab;
        public GameObject cellPrefab;
        public Gradient divergenceGradient;
        private SpriteRenderer[,] cells;
        private TMP_Text[,] texts;
        private LineRenderer[,] uLines;
        private LineRenderer[,] vLines;
        private float cellSize;
        private float paddingX;
        private float paddingY;


        public void Init(FluidGrid g)
        {
            float camHeight = Camera.main.orthographicSize * 2f;
            
            float camWidth = camHeight * Camera.main.aspect;
            
            // Set 0, 0 to bottom-left corner cam
            Camera.main.transform.position = new Vector3(camWidth / 2f, camHeight / 2f, -10f);
            
            
            cells = new SpriteRenderer[g.width + 2, g.height + 2];
            texts = new TMP_Text[g.width + 2, g.height + 2];
            
            float padding = 1f;
            
            cellSize = (camHeight - 2*padding)/(g.height + 2);
            
            paddingX = 2*padding + (camWidth - (g.width + 2) * cellSize) / 2f; // Center horizontally
            
            paddingY = 2*padding + (camHeight - (g.height + 2) * cellSize) / 2f; // Center vertically
            
            grid = g;
            
            for (int x = 0; x <= g.width + 1; x++)
            {
                for (int y = g.height + 1; y >= 0; y--)
                {
                    GameObject cellObj = Instantiate(cellPrefab, transform);
                    cellObj.transform.position = new Vector3(paddingX + x * cellSize, paddingY + y * cellSize, 0);
                    cellObj.transform.localScale = Vector3.one * cellSize * (1 - g.gapRatio);
                    // cellObj.transform.localScale = Vector3.one*cellSize;
                    cells[x, g.height+1-y] = cellObj.GetComponent<SpriteRenderer>();

                    TMP_Text text = Instantiate(textPrefab, transform);
                    text.text = "0";
                    text.alignment = TextAlignmentOptions.Center;
                    text.transform.position = new Vector3(paddingX + x * cellSize, paddingY + y * cellSize, 0);
                    text.fontSize = cellSize;
                    texts[x, g.height+1-y] = text;
                }
            }
            
            InitializeLines();
        }

        void InitializeLines()
        {
            uLines = new LineRenderer[grid.width + 2, grid.height + 2];
            vLines = new LineRenderer[grid.width + 2, grid.height + 2];
            for (int i = 0; i <= grid.width + 1; i++)
            {
                for (int j = 0; j <= grid.height + 1; j++)
                {
                    var line = Instantiate(linePrefab, transform);
                    uLines[i, j] = line.GetComponent<LineRenderer>();
                }
            }
            for (int i = 0; i <= grid.width + 1; i++)
            {
                for (int j = 0; j <= grid.height + 1; j++)
                {
                    var line = Instantiate(linePrefab, transform);
                    vLines[i, j] = line.GetComponent<LineRenderer>();
                }
            }
        }
        public void Render()
        {
            for (int j = 0; j <= grid.height + 1; j++)
            {
                for (int i = 0; i <= grid.width + 1; i++)
                {
                    Vector3 start = new Vector3(cells[i, j].transform.position.x - cellSize / 2f, cells[i, j].transform.position.y, -0.1f);
                    Vector3 end = start + new Vector3(grid.velocity[grid.Idx(i, j)].x*0.3f, 0, 0);
               
                    uLines[i, j].SetPosition(0, start);
                    uLines[i, j].SetPosition(1, end);
                    
                    start = new Vector3(cells[i, j].transform.position.x, cells[i, j].transform.position.y - cellSize / 2f, -0.1f);
                    end = start + new Vector3(0, grid.velocity[grid.Idx(i, j)].y*0.3f, 0);
                    
                    vLines[i, j].SetPosition(0, start);
                    vLines[i, j].SetPosition(1, end);
                    
                    if (grid.solid[i, j] == 0)
                    {
                        cells[i, j].color = new Color(0.1f, 0.1f, 0.1f, 0.5f); // Solid cell
                        continue;
                    }

                    float dx = grid.velocity[grid.Idx(i+1, j)].x - grid.velocity[grid.Idx(i, j)].x;
                    float dy = grid.velocity[grid.Idx(i, j-1)].y - grid.velocity[grid.Idx(i, j)].y;
                    float d = dx + dy; // Divergence
                    
                    texts[i, j].text = d.ToString("F2");

                    float normalized = Mathf.InverseLerp(-10f, 10f, d);
                    
                    cells[i, j].color = divergenceGradient.Evaluate(normalized);
                }
            }

            if (grid.streamLines)
            {
                RenderVelocityLines();
            }
        }

        void RenderVelocityLines()
        {
            float spacing = cellSize / 4;
            
            for (float x = 0; x <= (grid.width + 2) * cellSize; x += spacing)
            {
                for (float y = 0; y <= (grid.height + 2) * cellSize; y += spacing)
                {
                    Vector2 point = new Vector2(x, y);
                    Vector2 vel = CalculateVelocityAtPoint(point);
                    
                    if (vel == Vector2.negativeInfinity)
                        continue;
                    Debug.DrawLine(new Vector3(paddingX + point.x, paddingY + point.y, 0),
                        new Vector3(paddingX + point.x + vel.x * 0.1f, paddingY + point.y + vel.y * 0.1f, 0), Color.red);
                }
            }
        }

        public Vector2 CalculateVelocityAtPoint(Vector2 point)
        {
            // Adjust point based on padding
            point.x += paddingX;
            point.y += paddingY;
            
            int i = Mathf.Clamp((int)(point.x / cellSize), 0, grid.width + 1);
            int j = grid.height + 1 - Mathf.Clamp((int)(point.y / cellSize), 0, grid.height + 1);
            
            Vector2 firstVel = grid.velocity[grid.Idx(i, j)];
            
            
            
            float secondYVel, secondXVel;
            
            if (j + 1 > grid.height + 1)
                secondYVel = grid.velocity[grid.Idx(i, j)].y;
            else
                secondYVel = grid.velocity[grid.Idx(i, j + 1)].y;
            
            if (i + 1 > grid.width + 1)
                secondXVel = grid.velocity[grid.Idx(i, j)].x;
            else
                secondXVel = grid.velocity[grid.Idx(i + 1, j)].x;

            Vector2 finalVel = Vector2.zero;
            
            finalVel.x = Mathf.Lerp(firstVel.x, secondXVel, (point.x - i * cellSize) / cellSize);
            finalVel.y = Mathf.Lerp(firstVel.y, secondYVel, (point.y - j * cellSize) / cellSize);
            
            return finalVel;
        }
    }
}