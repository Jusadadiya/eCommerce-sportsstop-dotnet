using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using sportsstop.Models;
using sportsstop.Controllers;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace sportsstop.Util
{
    public static class SessionExtensions
    {
        public static void SetObject(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }

    public static class RandomInt
    {
        public static int Get(int min=1000000, int max=9999999)
        {
            Random random = new Random();
            return random.Next(min, max);
        }
    }

    public static class ListExtension
    {
        public static void Swap<T>(this ICollection<T> collection, T oldValue, T newValue)
        {
            // In case the collection is ordered, we'll be able to preserve the order
            var collectionAsList = collection as IList<T>;
            if (collectionAsList != null)
            {
                var oldIndex = collectionAsList.IndexOf(oldValue);
                collectionAsList.RemoveAt(oldIndex);
                collectionAsList.Insert(oldIndex, newValue);
            }
            else
            {
                // No luck, so just remove then add
                collection.Remove(oldValue);
                collection.Add(newValue);
            }

        }
    }
}
