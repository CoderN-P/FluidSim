using UnityEngine;


[System.Serializable]
public class FluidGrid
{
    public int width = 10;
    public int height = 6;
    public Vector2 gravity = new Vector2(0, -9.81f);
    public int solverIterations = 20;
    public float gapRatio = 0.02f; // Ratio of gap between cells
    public float o = 1.2f; // Over-relaxation factor
    public bool streamLines = false;
    
    [Header ("Grid Data")]
    [SerializeField] public Vector2[] velocity; // Velocity field (staggered)
    public int[,] solid; // Solid cell map

    public int Idx(int i, int j) => i + j * (width + 2);
    
    public FluidGrid()
    {
        velocity = new Vector2[(width+2)*(width+2)]; // +2 for solid boundary cells
        solid = new int[width + 2, height + 2]; // +2 for solid boundary cells
        
        InitializeSolid();
    }

    void InitializeSolid()
    {
        for (int i = 0; i <= width + 1; i++)
        {
            for (int j = 0; j <= height + 1; j++)
            {
                if (i == 0 || i == width + 1 || j == 0 || j == height + 1)
                {
                    solid[i, j] = 0; // Boundary cells are solid
                }
                else
                {
                    solid[i, j] = 1; // Interior cells are fluid
                }
            }
        }
    }
    
    public void InitializeVelocities()
    {
        for (int i = 0; i <= width + 1; i++)
        {
            for (int j = 0; j <= height + 1; j++)
            {
                if (solid[i, j] == 0)
                {
                    velocity[Idx(i, j)] = Vector2.zero; // Solid cells have zero velocity
                    continue;
                }

                velocity[Idx(i, j)] = new Vector2(Random.Range(-5f, 5f), Random.Range(-5f, 5f)); // Random initial velocity
            }
        }
    }
    
    
}