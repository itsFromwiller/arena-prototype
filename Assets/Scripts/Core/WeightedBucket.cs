using System.Collections.Generic;
using UnityEngine;

namespace Arena.Assets.Scripts.Core
{
    public class WeightedBucket<T>
    {
        private class BucketItem
        {
            public T Item;
            public int Weight;
        }

        private List<BucketItem> Items = new List<BucketItem>();
        private int TotalWeight = 0;

        public void AddItem(T item, int weight)
        {
            if (weight <= 0) return; // Ignore zero or negative weights
            Items.Add(new BucketItem { Item = item, Weight = weight });
            TotalWeight += weight;
        }

        public T GetRandomItem()
        {
            if (Items.Count == 0) return default;
            int randomRoll = Random.Range(0, TotalWeight);
            int currentStep = 0;

            foreach (var bucketItem in Items)
            {
                currentStep += bucketItem.Weight;
                if (randomRoll <= currentStep)
                {
                    return bucketItem.Item;
                }
            }
            return Items[Items.Count - 1].Item; // Fallback
        }
    }
}