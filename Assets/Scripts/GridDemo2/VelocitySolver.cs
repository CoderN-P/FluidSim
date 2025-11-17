using UnityEngine;

namespace GridDemo2
{
    public class VelocitySolver : MonoBehaviour
    {
        public FluidGrid grid;
        public bool solve = true;
        public bool advect = true;
        public void Init(FluidGrid g) => grid = g;
        
        public void Step()
        {
            ApplyVorticityConfinement();
            if (advect)
            {
                AdvectVelocityField();
            }
            
            if (solve)
            {
                ProjectVelocities();
            }
            
        }
        
        float ComputeVorticity(int i, int j)
        {
            float dv_dx = (grid.velocity[i+1, j].y - grid.velocity[i-1, j].y) * 0.5f;
            float du_dy = (grid.velocity[i, j+1].x - grid.velocity[i, j-1].x) * 0.5f;
            return dv_dx - du_dy; 
        }
        
        Vector2 ComputeVorticityGradient(int i, int j)
        {
            float wL = Mathf.Abs(grid.vort[i-1,j]);
            float wR = Mathf.Abs(grid.vort[i+1,j]);
            float wB = Mathf.Abs(grid.vort[i,j-1]);
            float wT = Mathf.Abs(grid.vort[i,j+1]);

            float dx = (wR - wL) * 0.5f;
            float dy = (wT - wB) * 0.5f;

            Vector2 N = new Vector2(dx, dy);
            return N.normalized;
        }
        
        void UpdateVorticityGrid()
        {
            for (int i = 1; i < grid.width + 1; i++)
            {
                for (int j = 1; j < grid.height + 1; j++)
                {
                    grid.vort[i, j] = ComputeVorticity(i, j);
                }
            }
        }
        
        void UpdateVorticityGradientGrid()
        {
            for (int i = 1; i < grid.width + 1; i++)
            {
                for (int j = 1; j < grid.height + 1; j++)
                {
                    grid.vortGradient[i, j] = ComputeVorticityGradient(i, j);
                }
            }
        }
        
        void ApplyVorticityConfinement()
        {
            UpdateVorticityGrid();
            UpdateVorticityGradientGrid();
            
            for (int i = 1; i < grid.width + 1; i++)
            {
                for (int j = 1; j < grid.height + 1; j++)
                {
                    Vector2 N = grid.vortGradient[i, j];
                    float w = grid.vort[i, j];
                    
                    Vector2 fVC = grid.vorticityConfinementStrength * new Vector2(N.y, -N.x) * w; // Perpendicular to N
                    
                    grid.velocity[i, j] += fVC * Time.deltaTime;
                }
            }
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
                        float d = grid.overrelaxationFactor * GetDivergenceAtCell(i, j); // Divergence

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
            float u = SampleU(new Vector2(x, y));
            float v = SampleV(new Vector2(x, y));
            return new Vector2(u, v);
        }

        float SampleV(Vector2 p)
        {
            float fx = p.x - 0.5f;
            float fy = p.y - 1f;

            int i = Mathf.Clamp(Mathf.FloorToInt(fx), 0, grid.width - 1);
            int j = Mathf.Clamp(Mathf.FloorToInt(fy), 0, grid.height - 1);

            float tx = fx - i;
            float ty = fy - j;

            // Surrounding V-samples:
            float v00 = grid.velocity[i,     j].y;     // bottom-left
            float v10 = grid.velocity[i + 1, j].y;     // bottom-right
            float v01 = grid.velocity[i,     j + 1].y; // top-left
            float v11 = grid.velocity[i + 1, j + 1].y; // top-right

            // Bilinear interpolation
            float vx0 = Mathf.Lerp(v00, v10, tx);
            float vx1 = Mathf.Lerp(v01, v11, tx);

            return Mathf.Lerp(vx0, vx1, ty);
        }
        
        float SampleU(Vector2 p)
        {
            // Convert world position to U-grid space
            float fx = p.x;
            float fy = p.y - 0.5f;

            int i = Mathf.Clamp(Mathf.FloorToInt(fx), 0, grid.width - 1);
            int j = Mathf.Clamp(Mathf.FloorToInt(fy), 0, grid.height - 1);

            float tx = fx - i;
            float ty = fy - j;

            // Four surrounding U samples (vertical faces)
            float u00 = grid.velocity[i,     j].x;
            float u10 = grid.velocity[i + 1, j].x;
            float u01 = grid.velocity[i,     j + 1].x;
            float u11 = grid.velocity[i + 1, j + 1].x;

            // Bilinear interpolation
            float ux0 = Mathf.Lerp(u00, u10, tx);
            float ux1 = Mathf.Lerp(u01, u11, tx);

            return Mathf.Lerp(ux0, ux1, ty);
        }

        public Vector2 Get2DVelocityAtU(int i, int j)
        {
            float u = grid.velocity[i, j].x;
            
            float v = 0.25f * (
                grid.velocity[i, j].y +
                grid.velocity[i, j - 1].y +
                grid.velocity[i - 1, j].y +
                grid.velocity[i - 1, j - 1].y
            );
            
            return new Vector2(u, v);
        }
        
        public Vector2 Get2DVelocityAtV(int i, int j)
        {
            float v = grid.velocity[i, j].y;
            float u = 0.25f * (
                grid.velocity[i, j].x +
                grid.velocity[i + 1, j].x +
                grid.velocity[i, j + 1].x +
                grid.velocity[i + 1, j + 1].x
            );
            
            return new Vector2(u, v);
        }

        public float GetDivergenceAtCell(int i, int j)
        {
            if (i < 1 || i > grid.width || j < 1 || j > grid.height)
                return 0f;
            
            float dx = grid.velocity[i + 1, j].x - grid.velocity[i, j].x;
            float dy = grid.velocity[i, j - 1].y - grid.velocity[i, j].y;
            return dx + dy;
        }

        public void AdvectVelocityField()
        {
            for (int i = 1; i < grid.width + 1; i++)
            {
                for (int j = 1; j < grid.height + 1; j++)
                {
                    Vector2 vel = Get2DVelocityAtU(i, j);
                    Vector2 prevPos = new Vector2(i, j+0.5f) - vel * Time.deltaTime;
                    Vector2 newVel = GetVelocityAtPosition(prevPos.x, prevPos.y);
                    grid.tempVelocity[i, j].x = newVel.x;
                }
            }
            
            for (int i = 1; i < grid.width + 1; i++)
            {
                for (int j = 1; j < grid.height + 1; j++)
                {
                    Vector2 vel = Get2DVelocityAtV(i, j);
                    Vector2 prevPos = new Vector2(i+0.5f, j+1) - vel * Time.deltaTime;
                    Vector2 newVel = GetVelocityAtPosition(prevPos.x, prevPos.y);
                    grid.tempVelocity[i, j].y = newVel.y;
                }
            }
            
            for (int i = 1; i < grid.width + 1; i++)
            {
                for (int j = 1; j < grid.height + 1; j++)
                {
                    grid.velocity[i, j] = grid.tempVelocity[i, j];
                }
            }
        }
    }
}