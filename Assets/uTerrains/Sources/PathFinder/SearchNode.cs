namespace UltimateTerrains.Pathfinder
{

	/// <summary>
    /// Class defining nodes used in path finding to mark our routes.</summary>
    public class SearchNode
    {
        public Vector3i position;
        public int cost;
        public int pathCost;
        public SearchNode next;
        public SearchNode nextListElem;

        public SearchNode(Vector3i position, int cost, int pathCost, SearchNode next)
        {
            this.position = position;
            this.cost = cost;
            this.pathCost = pathCost;
            this.next = next;
        }
		
		public override string ToString() {
			return "SearchNode[position=("+position.x+","+position.y+","+position.z+
				"), cost="+cost+", pathCost="+pathCost+"]";
		}
    }
	
}