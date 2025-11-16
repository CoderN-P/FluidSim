using UnityEngine;


namespace GridDemo1
{
    public class VelocitySolver : MonoBehaviour
    {
        private FluidGrid grid;

        public void Init(FluidGrid g) => grid = g;

        public void UpdateVelocities()
        {
            for (int i = 0; i < grid.width; i++)
            {
                for (int j = 0; j < grid.height; j++)
                {
                    // Apply gravity
                    grid.velocity[grid.Idx(i, j)].y += -9.81f * Time.deltaTime;
                }
            }
        }

        public void ProjectVelocities()
        {
            for (int iter = 0; iter < grid.solverIterations; iter++)
            {
                for (int i = 1; i <= grid.width; i++)
                {
                    for (int j = 1; j <= grid.height; j++)
                    {
                        int s = grid.solid[i, j + 1] + grid.solid[i, j - 1] + grid.solid[i + 1, j] + grid.solid[i - 1, j];
                        float dx = grid.velocity[grid.Idx(i+1, j)].x - grid.velocity[grid.Idx(i, j)].x;
                        float dy = grid.velocity[grid.Idx(i, j-1)].y - grid.velocity[grid.Idx(i, j)].y;
                        float d = grid.o*(dx + dy); // Divergence
    
                        grid.velocity[grid.Idx(i, j)].x += d * grid.solid[i-1, j] / s;
                        grid.velocity[grid.Idx(i+1, j)].x -= d * grid.solid[i+1, j] / s;
                        grid.velocity[grid.Idx(i, j)].y += d * grid.solid[i, j+1] / s;
                        grid.velocity[grid.Idx(i, j-1)].y -= d * grid.solid[i, j-1] / s;
                    }
                }
            }
        }

        public void Step()
        {
            // UpdateVelocities();
            ProjectVelocities();
        }
    }
}