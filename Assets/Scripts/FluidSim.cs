using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidSim : MonoBehaviour
{

    public int height = 20;
    public int width = 20;
    public float o = 1.9f; // Over-relaxation factor
    public int solverIterations = 20; // Gauss-seidel solver iterations
    public Vector2 gravity = new Vector2(0, -9.81f);
    
    int[][] solid; // [width][height] solid cell map
    float[][] pressure; // [width][height] pressure field
    Vector2[][] velocity; // [width][height] velocity field (Staggered) 

    void UpdateVelocityField()
    {
        // Updates velocity to add gravity and other forces;
        
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // Apply gravity
                velocity[i][j] += gravity * Time.deltaTime;
            }
        }
    }

    void InitializeGrids()
    {
        for (int i = 0; i <= width+1; i++)
        {
            for (int j = 0; j <= height+1; j++)
            {
                if (i <= 0 || i > width || j <= 0 || j > height)
                {
                    solid[i][j] = 0; // Boundary cells are solid
                }
                else
                {
                    solid[i][j] = 1; // Interior cells are fluid
                }
            }
        }
    }
    
    
    void ProjectVelocityField()
    {
        // Projects the velocity field to be divergence-free using Gauss-Seidel relaxation

        for (int iter = 0; iter < solverIterations; iter++)
        {
            for (int j = 1; j <= width; j++)
            {
                for (int i = 1; i <= height; i++)
                {
                    int s = solid[j][i + 1] + solid[j][i - 1] + solid[j + 1][i] + solid[j - 1][i];
                    float dx = velocity[j][i + 1].x - velocity[j][i].x;
                    float dy = velocity[j + 1][i].y - velocity[j][i].y;
                    float d = o*(dx + dy); // Divergence

                    velocity[j][i].x += d * solid[j][i - 1] / s;
                    velocity[j][i + 1].x -= d * solid[j][i + 1] / s;
                    velocity[j][i].y += d * solid[j - 1][i] / s;
                    velocity[j + 1][i].y -= d * solid[j + 1][i] / s;
                }
            }
        }
    }
    
    void AdvectVelocityField()
    {
        // Advects the velocity field using semi-Lagrangian advection
        
        

    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
