using UnityEngine;

namespace GridDemo2
{
   [System.Serializable]
   public class FluidGrid
   {
      public int width = 16;
      public int height = 9;
      public int solverIterations = 20;
      public float overrelaxationFactor = 1.2f;
      public Vector2[,] velocity; // Velocity field (staggered)
      public int[,] solid; // Solid cell map


      public FluidGrid()
      {
         velocity = new Vector2[width+2, height+2]; // +2 for solid boundary cells
         solid = new int[width+2, height+2]; // +2 for solid boundary cells
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
                  velocity[i, j] = Vector2.zero; // Solid cells have zero velocity
                  continue;
               }

               velocity[i, j] = new Vector2(Random.Range(-5f, 5f), Random.Range(-5f, 5f)); // Random initial velocity
               // velocity[i, j] = Vector2.zero; // Start with zero velocity
            }
         }
      }
   }
}