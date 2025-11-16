using UnityEngine;

namespace GridDemo2
{
    public class VelocitySolver : MonoBehaviour
    {
        public FluidGrid grid;
        public void Init(FluidGrid g) => grid = g;
        
        public void Step()
        {
            ProjectVelocities();
            AdvectVelocityField();
        }

        public void ProjectVelocities()
        {
            for (int iter = 0; iter < grid.solverIterations; iter++)
            {
                for (int i = 1; i < grid.width + 1; i++)
                {
                    for (int j = 1; j < grid.height + 1; j++)
                    {
                        int s = grid.solid[i, j + 1] + grid.solid[i, j - 1] + grid.solid[i + 1, j] + grid.solid[i - 1, j];
                        float dx = grid.velocity[i + 1, j].x - grid.velocity[i, j].x;
                        float dy = grid.velocity[i, j - 1].y - grid.velocity[i, j].y;
                        float d = grid.overrelaxationFactor * (dx + dy); // Divergence

                        grid.velocity[i, j].x += d * grid.solid[i - 1, j] / s;
                        grid.velocity[i + 1, j].x -= d * grid.solid[i + 1, j] / s;
                        grid.velocity[i, j].y += d * grid.solid[i, j + 1] / s;
                        grid.velocity[i, j - 1].y -= d * grid.solid[i, j - 1] / s;
                    }
                }
            }
        }
        
        public Vector2 GetVelocityAtPosition(float x, float y)
        {
            // Bilinear interpolation of velocity field at position (x, y)
            int i = Mathf.Clamp(Mathf.FloorToInt(x), 1, grid.width);
            int j = Mathf.Clamp(Mathf.FloorToInt(y), 1, grid.height);
            
            float tx = x - i;
            float ty = y - j;
            
            float u00 = grid.velocity[i, j].x;
            float u10 = grid.velocity[Mathf.Min(i + 1, grid.width + 1), j].x;
            float v00 = grid.velocity[i, Mathf.Max(j - 1, 0)].y;
            float v01 = grid.velocity[i, j].y;
            
            Vector2 finalVel;
            finalVel.x = Mathf.Lerp(u00, u10, tx);
            finalVel.y = Mathf.Lerp(v00, v01, ty);
            
            return finalVel;
        }

        public Vector2 Get2DVelocityAtU(int i, int j)
        {
            float u = grid.velocity[i, j].x;
            float w00 = 0.5f;
            float w10 = 0.5f;
            float w01 = 0.5f;
            float w11 = 0.5f;
            
            float v = w00*w10*grid.velocity[i, j].y + w01*w10*grid.velocity[i+1, j].y + w01*w11*grid.velocity[i, j-1].y + w00*w11*grid.velocity[i+1, j-1].y;
            
            return new Vector2(u, v);
        }
        
        public Vector2 Get2DVelocityAtV(int i, int j)
        {
            float v = grid.velocity[i, j].y;
            float w00 = 0.5f;
            float w10 = 0.5f;
            float w01 = 0.5f;
            float w11 = 0.5f;
            
            float u = w00*w10*grid.velocity[i, j].x + w01*w10*grid.velocity[i+1, j].x + w01*w11*grid.velocity[i, j+1].x + w00*w11*grid.velocity[i+1, j+1].x;
            
            return new Vector2(u, v);
        }

        public void AdvectVelocityField()
        {
            for (int i = 1; i < grid.width + 1; i++)
            {
                for (int j = 1; j < grid.height + 1; j++)
                {
                    Vector2 vel = Get2DVelocityAtU(i, j);
                    float prevX = i - vel.x * Time.deltaTime;
                    float prevY = j - vel.y * Time.deltaTime;
                    Vector2 newVel = GetVelocityAtPosition(prevX, prevY);
                    grid.velocity[i, j].x = newVel.x;
                    
                    vel = Get2DVelocityAtV(i, j);
                    prevX = i - vel.x * Time.deltaTime;
                    prevY = j - vel.y * Time.deltaTime;
                    newVel = GetVelocityAtPosition(prevX, prevY);
                    grid.velocity[i, j].y = newVel.y;
                }
            }
        }
    }
}