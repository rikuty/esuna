using System.Collections.Generic;
using UnityEngine;

namespace UltimateTerrains
{
    internal static class TreesObjectPools
    {
        private static UltimateTerrain terrain;
        private static GameObject[] gameObjects;
        private static Queue<TreeComponent>[] pool;

        public static void Init(UltimateTerrain uTerrain)
        {
            terrain = uTerrain;
            if (terrain.Params.TreesParams == null)
                return;

            var list = new List<GameObject>();
            foreach (var param in terrain.Params.TreesParams) {
                foreach (var obj in param.Objects) {
                    var i = list.IndexOf(obj.Object);
                    if (i < 0) {
                        obj.Index = list.Count;
                        list.Add(obj.Object);
                    } else {
                        obj.Index = i;
                    }
                }
            }

            gameObjects = new GameObject[list.Count];
            pool = new Queue<TreeComponent>[list.Count];
            for (var i = 0; i < list.Count; ++i) {
                gameObjects[i] = list[i];
                pool[i] = new Queue<TreeComponent>();
            }
        }

        public static void Reset()
        {
            terrain = null;
            gameObjects = null;
            pool = null;
        }

        public static TreeComponent Get(int index)
        {
            TreeComponent tree;
            var queue = pool[index];
            if (queue.Count == 0) {
                var obj = Object.Instantiate(gameObjects[index]);
                obj.hideFlags = Param.HideFlags;
                tree = obj.GetComponent<TreeComponent>();
                if (tree == null) {
                    tree = obj.AddComponent<TreeComponent>();
                    tree.Awake();
                }

                tree.Index = index;
            } else {
                tree = queue.Dequeue();
            }

            tree.IsFree = false;
            return tree;
        }

        public static void Free(TreeComponent obj)
        {
            if (!obj.IsFree) {
                obj.IsFree = true;
                obj.Disable();
                pool[obj.Index].Enqueue(obj);
            }
        }
    }
}