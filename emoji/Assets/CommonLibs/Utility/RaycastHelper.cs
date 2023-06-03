using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Assets.CommonLibs.Utility
{
    public static class RaycastHelper
    {
        /// <summary>
        /// 应用场景中最大的层级数量应该不会超过64吧，如果缓冲满了，报错吧
        /// </summary>
        private static RaycastHit[] raycastResults = new RaycastHit[64];

        /// <summary>
        /// 按距离升序排列
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="count"></param>
        private static void SortByDistanceAscend(RaycastHit[] buffer, int count)
        {
             for (var i = 0; i < count; i++)
            {
                for (var j = i; j < count; j++)
                {
                    if (buffer[i].distance > buffer[j].distance)
                    {
                        var tmp = buffer[i];
                        buffer[i] = buffer[j];
                        buffer[j] = tmp;
                    }
                }
            }
        }

        public static void SortByDistanceAscend(List<RaycastHit> buffer)
        {
             for (var i = 0; i < buffer.Count; i++)
            {
                for (var j = i; j < buffer.Count; j++)
                {
                    if (buffer[i].distance > buffer[j].distance)
                    {
                        var tmp = buffer[i];
                        buffer[i] = buffer[j];
                        buffer[j] = tmp;
                    }
                }
            }
        }

        public static RaycastHit? RaycastNearest(Vector3 origin, Vector3 dir, float distance, int layerMask)
        {
            RaycastHit? result = null;
            RaycastHit hit;
            if(Physics.Raycast(origin, dir, out hit, distance, layerMask, QueryTriggerInteraction.Ignore))
            {
                result = hit;
            }
            return result;
        }

        public static RaycastHit? RaycastNearest(Vector3 origin, Vector3 dir, float distance)
        {
            RaycastHit? result = null;
            RaycastHit hit;
            if(Physics.Raycast(origin, dir, out hit, distance))
            {
                result = hit;
            }
            return result;
        }

        public static RaycastHit? CapsulecastNearest(Vector3 point1, Vector3 point2, float radius, Vector3 dir, float distance, int layerMask)
        {
            RaycastHit? result = null;
            RaycastHit hit;
            if(Physics.CapsuleCast(point1, point2, radius, dir, out hit, distance, layerMask))
            {
                result = hit; 
            }
            return result;
        }

        public static RaycastHit? CapsulecastNearest(Vector3 point1, Vector3 point2, float radius, Vector3 dir, float distance)
        {
            RaycastHit? result = null;
            RaycastHit hit;
            if(Physics.CapsuleCast(point1, point2, radius, dir, out hit, distance))
            {
                result = hit; 
            }
            return result;
        }

        public static (RaycastHit[], int) RaycastAll(Vector3 origin, Vector3 dir, float distance, RaycastHit[] buffer = null)
        {
            var results = null == buffer ? raycastResults : buffer;
            var resultCount = Physics.RaycastNonAlloc(origin, dir, results, distance);
            if (resultCount < 1)
            {
                return (null, 0);
            }
            if(null == buffer && resultCount == raycastResults.Length)
            {
                Debug.LogError("Warning: raycast hit count reachs limit, enlarge the buffer size !");
            }
            return (results, resultCount);
        }

        public static (RaycastHit[], int) RaycastAll(Vector3 origin, Vector3 dir, float distance, int layerMask, RaycastHit[] buffer = null)
        {
            var results = null == buffer ? raycastResults : buffer;
            var resultCount = Physics.RaycastNonAlloc(origin, dir, results, distance, layerMask);
            if (resultCount < 1)
            {
                return (null, 0);
            }
            if(null == buffer && resultCount == raycastResults.Length)
            {
                Debug.LogError("Warning: raycast hit count reachs limit, enlarge the buffer size !");
            }
            return (results, resultCount);
        }

        /// <summary>
        /// 距离排序，由近及远
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="dir"></param>
        /// <param name="distance"></param>
        /// <param name="layerMask"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static (RaycastHit[], int) RaycastAllAsend(Vector3 origin, Vector3 dir, float distance, int layerMask, RaycastHit[] buffer = null)
        {
            var results = null == buffer ? raycastResults : buffer;
            var resultCount = Physics.RaycastNonAlloc(origin, dir, results, distance, layerMask);
            if (resultCount < 1)
            {
                return (null, 0);
            }
            if(null == buffer && resultCount == raycastResults.Length)
            {
                Debug.LogError("Warning: raycast hit count reachs limit, enlarge the buffer size !");
            }
            SortByDistanceAscend(results, resultCount);
            return (results, resultCount);
        }

        /// <summary>
        /// 距离排序，由近及远
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="dir"></param>
        /// <param name="distance"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static (RaycastHit[], int) RaycastAllAsend(Vector3 origin, Vector3 dir, float distance, RaycastHit[] buffer = null)
        {
            var results = null == buffer ? raycastResults : buffer;
            var resultCount = Physics.RaycastNonAlloc(origin, dir, results, distance);
            if (resultCount < 1)
            {
                return (null, 0);
            }
            if(null == buffer && resultCount == raycastResults.Length)
            {
                Debug.LogError("Warning: raycast hit count reachs limit, enlarge the buffer size !");
            }
            SortByDistanceAscend(results, resultCount);
            return (results, resultCount);
        }

    }
}