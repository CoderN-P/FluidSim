using Subsystems;
using UnityEngine;

public class FluidDemoManager : MonoBehaviour
{
    [Header("Grid")]
    public FluidGrid grid;

    [Header("Subsystems")]
    public VelocitySolver velocitySolver;
    public FluidRenderer fluidRenderer;

    void Awake()
    {
        grid = new FluidGrid();
        grid.InitializeVelocities();
        // Initialize all subsystems and inject shared data
        velocitySolver.Init(grid);
        fluidRenderer.Init(grid);
    }

    void Update()
    {
        // inputHandler.HandleInput();    // drag, add velocity, density, etc.
        velocitySolver.Step();         // diffuse, advect velocity
        fluidRenderer.Render();        // draw current state
    }
}