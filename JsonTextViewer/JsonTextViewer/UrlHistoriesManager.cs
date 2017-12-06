/* 
 * FileName:    UrlHistoryManager.cs
 * Author:      functionghw<functionghw@hotmail.com>
 * CreateTime:  2017/2/21 16:32:53
 * Version:     v1.0
 * Description:
 * */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonTextViewer
{
    public sealed class UrlHistoriesManager
    {

        static UrlHistoriesManager()
        {
            string localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appDataDir = Path.Combine(localApp, "JsonTextViewer");
            Directory.CreateDirectory(appDataDir);
            historyFilePath = Path.Combine(appDataDir, "urlhistories.txt");

            Instance = new UrlHistoriesManager();
        }

        public event EventHandler UrlHistoriesUpdated;

        public static readonly UrlHistoriesManager Instance;

        private UrlHistoriesManager()
        {
            LoadOrCreateFile();
        }

        private static readonly string historyFilePath;

        private const int maxRecordCount = 50;

        public List<UrlHistory> UrlHistories { get; private set; }


        private void LoadOrCreateFile()
        {
            if (!File.Exists(historyFilePath))
                File.Create(historyFilePath).Dispose();

            var history = File.ReadLines(historyFilePath)
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(CreateFromString)
                .Where(item => item != null)
                .Distinct()
                .Take(maxRecordCount)
                .ToList();

            UrlHistories = history;
        }

        private UrlHistory CreateFromString(string line)
        {
            var rowData = line.Split(new[] {"||"}, 2, StringSplitOptions.RemoveEmptyEntries);
            if (rowData.Length != 2)
                return null;

            return new UrlHistory(rowData[0], rowData[1]);
        }

        public void SaveToFile()
        {
            var list = UrlHistories.ToArray();

            using (var fs = File.Open(historyFilePath, FileMode.Create, FileAccess.Write))
            {
                fs.SetLength(0);
                using (var sw = new StreamWriter(fs))
                {
                    foreach (var item in list)
                    {
                        sw.WriteLine(item);
                    }
                    sw.Flush();
                }
            }
        }

        public void RefreshUrl(string method, string url)
        {
            var existItem = UrlHistories.FirstOrDefault(item => item.Method == method && item.Url == url);
            if (existItem != null)
            {
                UrlHistories.Remove(existItem);
            }
            else
            {
                if (UrlHistories.Count >= maxRecordCount)
                {
                    UrlHistories.RemoveAt(UrlHistories.Count - 1);
                }
            }
            UrlHistories.Insert(0, new UrlHistory(method, url));
            var handler = UrlHistoriesUpdated;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }

    public class UrlHistory : IEquatable<UrlHistory>
    {
        public UrlHistory(string method, string url)
        {
            Method = method;
            Url = url;
        }

        public string Method { get; }

        public string Url { get; }

        public override string ToString()
        {
            return $"{Method}||{Url}";
        }

        public override bool Equals(object obj)
        {
            if (obj is UrlHistory other)
            {
                return Equals(other);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public bool Equals(UrlHistory other)
        {
            if (other == null)
                return false;

            return Method == other.Method && Url == other.Url;
        }
    }
}
