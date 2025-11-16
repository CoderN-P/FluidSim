using UnityEngine;

namespace GridDemo2
{
    public class FluidDemo2 : MonoBehaviour
    {
        [Header("Grid")]
        public FluidGrid grid;
        
        [Header("Subsystems")] 
        public FluidRenderer2 fluidRenderer2;
        public VelocitySolver velocitySolver;

        void Awake()
        {
            grid = new FluidGrid();
            grid.InitializeVelocities();
            
            fluidRenderer2.Init(grid);
            velocitySolver.Init(grid);
        }

        void Update()
        {
            velocitySolver.Step(); // diffuse, advect velocity
            fluidRenderer2.Render(); // draw current state
        }
    
    }
}