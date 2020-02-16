using System;
using System.Collections.Generic;
using System.Linq;



namespace DbscanImplementation
{
    /// <summary>
    /// DBSCAN algorithm class
    /// </summary>
    /// <typeparam name="T">Takes dataset item row (features, preferences, vector) type</typeparam>
    public class DbscanAlgorithm<T> where T : MyCustomDatasetItem
    {
        /// <summary>
        /// Performs the DBSCAN clustering algorithm.
        /// </summary>
        /// <param name="allPoints">Dataset</param>
        /// <param name="epsilon">Desired region ball radius</param>
        /// <param name="minPts">Minimum number of points to be in a region</param>
        /// <param name="clusters">returns sets of clusters, renew the parameter</param>
        public void ComputeClusterDbscan(T[] allPoints, double epsilon, int minPts, out HashSet<T[]> clusters)
        {
            
            DbscanPoint<T>[] allPointsDbscan = allPoints.Select(x => new DbscanPoint<T>(x)).ToArray();
            var tree = new KDTree.KDTree<DbscanPoint<T>>(2);
            for (var i = 0; i < allPointsDbscan.Length; ++i)
            {
                MyCustomDatasetItem temp = allPointsDbscan[i].ClusterPoint;
                tree.AddPoint(new double[] { temp.X, temp.Y }, allPointsDbscan[i]);
            }

            
            int clusterId = 0;
            for (int i = 0; i < allPointsDbscan.Length; i++)
            {
                DbscanPoint<T> p = allPointsDbscan[i];
                if (p.IsVisited)
                    continue;
                p.IsVisited = true;

                var neighborPts = RegionQuery(tree, p.ClusterPoint, epsilon);
                if (neighborPts.Length < minPts)
                    p.ClusterId = (int)ClusterIds.Noise;
                else
                {
                    clusterId++;
                    ExpandCluster(tree, p, neighborPts, clusterId, epsilon, minPts);
                }
            }
            clusters = new HashSet<T[]>(
                allPointsDbscan
                    .Where(x => x.ClusterId > 0)
                    .GroupBy(x => x.ClusterId)
                    .Select(x => x.Select(y => y.ClusterPoint).ToArray())
                );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allPoints">Dataset</param>
        /// <param name="point">point to be in a cluster</param>
        /// <param name="neighborPts">other points in same region with point parameter</param>
        /// <param name="clusterId">given clusterId</param>
        /// <param name="epsilon">Desired region ball range</param>
        /// <param name="minPts">Minimum number of points to be in a region</param>
        private void ExpandCluster(KDTree.KDTree<DbscanPoint<T>> tree, DbscanPoint<T> p, DbscanPoint<T>[] neighborPts, int clusterId, double epsilon, int minPts)
        {
            p.ClusterId = clusterId;
//            for (int i = 0; i < neighborPts.Length; i++)
//            {
//                DbscanPoint<T> pn = neighborPts[i];
//                if (!pn.IsVisited)
//                {
//                    pn.IsVisited = true;
//                    DbscanPoint<T>[] neighborPts2 = RegionQuery(tree, pn.ClusterPoint, epsilon);;
//                    if (neighborPts2.Length >= minPts)
//                    {
//                        neighborPts = neighborPts.Union(neighborPts2).ToArray();
//                    }
//                }
//                if (pn.ClusterId == (int)ClusterIds.Unclassified)
//                    pn.ClusterId = clusterId;
//            }
            var queue = new Queue<DbscanPoint<T>>(neighborPts);
            while (queue.Count > 0)
            {
                var point = queue.Dequeue();
                if (point.ClusterId == (int)ClusterIds.Unclassified)
                {
                    point.ClusterId = clusterId;
                }

                if (point.IsVisited)
                {
                    continue;
                }

                point.IsVisited = true;
                var neighbors = RegionQuery(tree, point.ClusterPoint, epsilon);
                if (neighbors.Length >= minPts)
                {
                    foreach (var neighbor in neighbors.Where(neighbor => !neighbor.IsVisited))
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        /// <summary>
        /// Checks and searchs neighbor points for given point
        /// </summary>
        /// <param name="allPoints">Dataset</param>
        /// <param name="point">centered point to be searched neighbors</param>
        /// <param name="epsilon">radius of center point</param>
        /// <param name="neighborPts">result neighbors</param>
        private DbscanPoint<T>[] RegionQuery(KDTree.KDTree<DbscanPoint<T>> tree, T point, double epsilon)
        {
            var neighbors = new List<DbscanPoint<T>>();
            var e = tree.NearestNeighbors(new[] { point.X, point.Y }, 10, epsilon);
            while (e.MoveNext())
            {
                neighbors.Add(e.Current);
            }
            return neighbors.ToArray();
        }
    }
}