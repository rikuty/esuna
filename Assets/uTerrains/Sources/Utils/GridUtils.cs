using System;

namespace UltimateTerrains
{
    public static class GridUtils
    {
        public delegate bool VisitCell(int cx, int cy, int cz);

        public static bool TraverseCellsIn3DGrid(Vector3i s, Vector3i e, VisitCell visitCellDelegate)
        {
            //'steep' xy Line, make longest delta x plane
            int swaper;
            var swap_xy = Math.Abs(e.y - s.y) > Math.Abs(e.x - s.x);
            if (swap_xy) {
                swaper = s.x;
                s.x = s.y;
                s.y = swaper;
                swaper = e.x;
                e.x = e.y;
                e.y = swaper;
            }

            //do same for xz
            var swap_xz = Math.Abs(e.z - s.z) > Math.Abs(e.x - s.x);
            if (swap_xz) {
                swaper = s.x;
                s.x = s.z;
                s.z = swaper;
                swaper = e.x;
                e.x = e.z;
                e.z = swaper;
            }

            //delta is Length in each plane
            var delta_x = Math.Abs(e.x - s.x);
            var delta_y = Math.Abs(e.y - s.y);
            var delta_z = Math.Abs(e.z - s.z);

            //drift controls when to step in 'shallow' planes
            //starting value keeps Line centred
            var drift_xy = delta_x * 0.5f;
            var drift_xz = drift_xy;

            //direction of line
            var step_x = 1;
            if (s.x > e.x)
                step_x = -1;
            var step_y = 1;
            if (s.y > e.y)
                step_y = -1;
            var step_z = 1;
            if (s.z > e.z)
                step_z = -1;

            //starting point
            var y = s.y;
            var z = s.z;
            var drifted = false;

            //step through longest delta (which we have swapped to x)
            for (var x = s.x; x != e.x + step_x; x += step_x) {
                //passes through this point
                if (SwapAndVisitCell(x, y, z, swap_xz, swap_xy, visitCellDelegate)) {
                    return true;
                }

                //update progress in other planes
                drift_xy -= delta_y;
                drift_xz -= delta_z;

                //step in y plane
                var oldy = y;
                if (drift_xy < 0) {
                    y += step_y;
                    drift_xy += delta_x;
                    if (SwapAndVisitCell(x, y, z, swap_xz, swap_xy, visitCellDelegate)) {
                        return true;
                    }

                    drifted = true;
                }

                //same in z
                if (drift_xz < 0) {
                    z += step_z;
                    drift_xz += delta_x;
                    if (SwapAndVisitCell(x, oldy, z, swap_xz, swap_xy, visitCellDelegate)) {
                        return true;
                    }

                    if (drifted && SwapAndVisitCell(x, y, z, swap_xz, swap_xy, visitCellDelegate)) {
                        return true;
                    }
                }

                drifted = false;
            }

            return false;
        }

        private static bool SwapAndVisitCell(int x, int y, int z, bool swap_xz, bool swap_xy, VisitCell visitCellDelegate)
        {
            //unswap (in reverse)
            if (swap_xz) {
                var swaper = x;
                x = z;
                z = swaper;
            }

            if (swap_xy) {
                var swaper = x;
                x = y;
                y = swaper;
            }

            return visitCellDelegate(x, y, z);
        }
    }
}