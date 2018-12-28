using System;
using System.Collections.Generic;
using System.IO;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    /// <summary>
    /// Tracks the names of the top-used CodeCompletionData items and gives them higher
    /// priority in the completion dropdown.
    /// </summary>
    public static class CodeCompletionDataUsageCache
    {
        // File format for stored CodeCompletionDataUsageCache
        // long   magic   = 0x6567617355444343 (identifies file type = 'CCDUsage')
        // short  version = 1                  (file version)
        // int    itemCount
        // {
        //   string itemName
        //   int    itemUses
        //   int    itemShowCount
        // }

        private const long magic = 0x6567617355444343;
        private const short version = 1;

        /// <summary>Minimum number how often an item must be used to be saved to
        /// the file. Items with less uses than this count also get a priority penalty.
        /// (Because the first use would otherwise always be 100% priority)</summary>
        private const int MinUsesForSave = 2;

        private static Dictionary<string, UsageStruct> dict;

        public static string CacheFilename
        {
            get
            {
                if (string.IsNullOrEmpty(PropertyService.ConfigDirectory))
                    return null;
                else
                    return Path.Combine(PropertyService.ConfigDirectory, "CodeCompletionUsageCache.dat");
            }
        }

        private static void LoadCache()
        {
            dict = new Dictionary<string, UsageStruct>();
            string cacheFileName = CacheFilename;
            if (string.IsNullOrEmpty(cacheFileName) || !File.Exists(cacheFileName))
                return;
            using (var fs = new FileStream(cacheFileName, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new BinaryReader(fs))
                {
                    if (reader.ReadInt64() != magic)
                    {
                        return;
                    }
                    if (reader.ReadInt16() != version)
                    {
                        return;
                    }
                    int itemCount = reader.ReadInt32();
                    for (int i = 0; i < itemCount; i++)
                    {
                        string key = reader.ReadString();
                        int uses = reader.ReadInt32();
                        int showCount = reader.ReadInt32();
                        if (showCount > 1000)
                        {
                            // reduce count because the usage in the next time
                            // should have more influence on the past
                            showCount /= 3;
                            uses /= 3;
                        }
                        dict.Add(key, new UsageStruct(uses, showCount));
                    }
                }
            }
        }

        public static void SaveCache()
        {
            string cacheFileName = CacheFilename;
            if (dict == null || string.IsNullOrEmpty(cacheFileName))
            {
                return;
            }
            int count;
            using (var fs = new FileStream(cacheFileName, FileMode.Create, FileAccess.Write))
            {
                using (var writer = new BinaryWriter(fs))
                {
                    count = SaveCache(writer);
                }
            }
        }

        private static int SaveCache(BinaryWriter writer)
        {
            writer.Write(magic);
            writer.Write(version);
            int maxSaveItems = 200;
            if (dict.Count < maxSaveItems)
            {
                writer.Write(dict.Count);
                foreach (var entry in dict)
                {
                    writer.Write(entry.Key);
                    writer.Write(entry.Value.Uses);
                    writer.Write(entry.Value.ShowCount);
                }
                return dict.Count;
            }
            else
            {
                var saveItems = new List<KeyValuePair<string, UsageStruct>>();
                foreach (var entry in dict)
                {
                    if (entry.Value.Uses > MinUsesForSave)
                    {
                        saveItems.Add(entry);
                    }
                }
                if (saveItems.Count > maxSaveItems)
                {
                    saveItems.Sort(new SaveItemsComparer());
                }
                int count = Math.Min(maxSaveItems, saveItems.Count);
                writer.Write(count);
                for (int i = 0; i < count; i++)
                {
                    KeyValuePair<string, UsageStruct> entry = saveItems[i];
                    writer.Write(entry.Key);
                    writer.Write(entry.Value.Uses);
                    writer.Write(entry.Value.ShowCount);
                }
                return count;
            }
        }

        public static void ResetCache()
        {
            dict = new Dictionary<string, UsageStruct>();
            try
            {
                if (File.Exists(CacheFilename))
                {
                    File.Delete(CacheFilename);
                }
            }
            catch (Exception ex)
            {
            }
        }

        public static double GetPriority(string dotnetName, bool incrementShowCount)
        {
            if (dict == null)
            {
                LoadCache();
            }
            UsageStruct usage;
            if (!dict.TryGetValue(dotnetName, out usage))
                return 0;
            double priority = (double) usage.Uses/usage.ShowCount;
            if (usage.Uses < MinUsesForSave)
                priority *= 0.2;
            if (incrementShowCount)
            {
                usage.ShowCount += 1;
                dict[dotnetName] = usage;
            }
            return priority;
        }

        public static void IncrementUsage(string dotnetName)
        {
            if (dict == null)
            {
                LoadCache();
            }
            UsageStruct usage;
            if (!dict.TryGetValue(dotnetName, out usage))
            {
                usage = new UsageStruct(0, 2);
            }
            usage.Uses += 1;
            dict[dotnetName] = usage;
        }

        #region Nested type: SaveItemsComparer

        private class SaveItemsComparer : IComparer<KeyValuePair<string, UsageStruct>>
        {
            #region IComparer<KeyValuePair<string,UsageStruct>> Members

            public int Compare(KeyValuePair<string, UsageStruct> x, KeyValuePair<string, UsageStruct> y)
            {
                double a = ((double) x.Value.Uses/x.Value.ShowCount);
                return -a.CompareTo((double) y.Value.Uses/y.Value.ShowCount);
            }

            #endregion
        }

        #endregion

        #region Nested type: UsageStruct

        private struct UsageStruct
        {
            public int ShowCount;
            public int Uses;

            public UsageStruct(int Uses, int ShowCount)
            {
                this.Uses = Uses;
                this.ShowCount = ShowCount;
            }
        }

        #endregion
    }
}