using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Promo.EverythingIsNew.WebApp.Models
{
    public class ViewHelpers
    {
        public static List<List<T>> SplitByColumns<T>(List<T> list, int totalPartitions)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            if (totalPartitions < 1)
                throw new ArgumentOutOfRangeException("totalPartitions");

            List<T>[] partitions = new List<T>[totalPartitions];

            List<List<T>> result = new List<List<T>>();
            int batchSize = (list.Count + totalPartitions - 1) / totalPartitions;
            List<T> newInnerList = new List<T>();

            for (int n = 0; n < totalPartitions; n++)
            {
                newInnerList = new List<T>();
                for (int i = n * batchSize; i < Math.Min(batchSize * (n+1), list.Count); i++)
                {
                    newInnerList.Add(list[i]);
                }
                result.Add(newInnerList);
            }
            return result;
        }
    }
}