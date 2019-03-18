using System.Collections.Generic;

namespace UltimateTerrains
{
    internal sealed class DetailObjectsIndexer
    {
        private readonly List<DetailsParam.DetailObject> detailsObject;
        private readonly int count;

        public DetailObjectsIndexer(UltimateTerrain terrain)
        {
            if (terrain.Params.DetailsParams == null)
                return;

            detailsObject = new List<DetailsParam.DetailObject>();
            foreach (var param in terrain.Params.DetailsParams) {
                foreach (var obj in param.Objects) {
                    var i = detailsObject.IndexOf(obj);
                    if (i < 0) {
                        obj.Index = detailsObject.Count;
                        detailsObject.Add(obj);
                    } else {
                        obj.Index = i;
                    }
                }
            }

            count = detailsObject.Count;
        }

        public DetailsParam.DetailObject Get(int index)
        {
            return detailsObject[index];
        }

        public int Count {
            get { return count; }
        }
    }
}