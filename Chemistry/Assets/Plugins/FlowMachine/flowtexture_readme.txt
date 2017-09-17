FLOWTEXTURES
--------------------------------

Important components to get this system working are:
 -Tagged FlowProbes. Add the FlowProbe script to visualize the FlowProbe in the Editor. Remember to TAG the FlowProbe as a 'FlowProbe'
 -The FlowTexture surface uses a special shader.
 -Render the data using the 'Flow Mapper' tool located under menu 'GameObject/FlowMachine/FlowMapper'

Other notes and wish list:
Use as many FlowProbes as necessary to achieve the desired effect. The number of FlowProbes used has no performance hit during runtime.
Since the flow data is stored in the mesh vertex colors the mesh must has a good number of vertices as shown in the demo scene.

Good luck
/Sam Hagelund
