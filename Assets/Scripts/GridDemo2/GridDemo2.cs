using UnityEngine;

namespace GridDemo2
{
    public class FluidDemo2 : MonoBehaviour
    {
        public static FluidDemo2 Instance { get; private set; }
        
        [Header("Grid")]
        public FluidGrid grid;
        
        [Header("Subsystems")] 
        public FluidRenderer2 fluidRenderer2;
        public FluidRenderer3 fluidRenderer3;
        public VelocitySolver velocitySolver;
        

        void Awake()
        {
            Instance = this;
            grid = new FluidGrid();
            grid.InitializeVelocities();
            
            if (fluidRenderer3 != null)
                fluidRenderer3.Init(grid);
            else 
                fluidRenderer2.Init(grid);
            
            velocitySolver.Init(grid);
        }

        void Update()
        {
            if (fluidRenderer3 != null)
                fluidRenderer3.Render(); // draw current state, get input
            else
                fluidRenderer2.Render(); // draw current state, get input
            velocitySolver.Step(); // diffuse, advect velocity
        }
    
    }
}