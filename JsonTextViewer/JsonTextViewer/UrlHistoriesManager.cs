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

        public event EventHandler UrlHistoriesUpdated;

        public static readonly UrlHistoriesManager Instance = new UrlHistoriesManager();

        private UrlHistoriesManager()
        {
            LoadOrCreateFile();
        }

        private const string historyFilePath = "history.txt";

        private const int maxRecordCount = 50;

        public List<string> UrlHistories { get; private set; }


        private void LoadOrCreateFile()
        {
            if (!File.Exists(historyFilePath))
                File.Create(historyFilePath).Dispose();

            var history = File.ReadLines(historyFilePath)
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Distinct()
                .Take(maxRecordCount)
                .ToList();

            UrlHistories = history;
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

        public void RefreshUrl(string url)
        {
            if (UrlHistories.Any(item => item == url))
            {
                UrlHistories.Remove(url);
            }
            else
            {
                if (UrlHistories.Count >= maxRecordCount)
                {
                    UrlHistories.RemoveAt(UrlHistories.Count - 1);
                }
            }
            UrlHistories.Insert(0, url);
            var handler = UrlHistoriesUpdated;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}
