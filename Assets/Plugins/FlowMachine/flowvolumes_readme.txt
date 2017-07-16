FLOWVOLUMES
--------------------------------

Important components to get this system working are:
- FlowVolumes,
- FlowEmitter

Other notes and wish list:
Note that the water mesh is using a special shader. The ground mesh is separate and not connected to the system. For the demo scene I used a copy of the ground mesh as the water mesh.
Denser vertices on the water mesh make for better looking water flowing but also slows down the system. Its up to the developer to find the correct balance of vertex count vs speed.

The FlowObstacles can be scaled and moved around but NOT rotated.
I tried implementing rotation using matrix rotations but the result were not pleasing enough so I reverted to this.
I plan to look into rotation of FlowObstacles in the future when time allows.
Multi threading is currently not supported but should be adapted! Simple attempts at threadpooling was unsuccessful and manual threading needs to be tested. -_-;

Good luck
/Sam Hagelund
