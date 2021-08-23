using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TenSRR_RTA_AssistTool
{
    class CsvReader
    {
        public static List<List<string>> ReadCsv(string fileName, bool hasHeader, bool removeDoubleQuote)
        {
            List<List<string>> dataList = new List<List<string>>();

            // 読み込みたいCSVファイルを指定して開く
            using (StreamReader r = new StreamReader(fileName, Encoding.GetEncoding("Shift_JIS")))
            {
                if (hasHeader)
                {
                    // 一行読み飛ばす
                    r.ReadLine();
                }

                // 末尾まで繰り返す
                while (!r.EndOfStream)
                {
                    // CSVファイルの一行を読み込む
                    string line = r.ReadLine();

                    // 読み込んだ一行をカンマ毎に分けて配列に格納する
                    string[] values = line.Split(',');

                    // 配列からリストに格納する
                    List<string> lists = new List<string>();
                    lists.AddRange(values);

                    // 項目分繰り返す
                    for (int i = 0; i < lists.Count; ++i)
                    {
                        //先頭のスペースを除去して、ダブルクォーテーションが入っていないか判定する
                        if (lists[i] != string.Empty && lists[i].TrimStart()[0] == '"')
                        {
                            // もう一回ダブルクォーテーションが出てくるまで要素を結合
                            while (lists[i].TrimEnd()[lists[i].TrimEnd().Length - 1] != '"')
                            {
                                lists[i] = lists[i] + "," + lists[i + 1];

                                //結合したら要素を削除する
                                lists.RemoveAt(i + 1);
                            }
                        }
                    }

                    if (removeDoubleQuote)
                    {
                        for (int i = 0; i < lists.Count; ++i)
                        {
                            if (lists[i].StartsWith("\"") && lists[i].EndsWith("\""))
                            {
                                lists[i] = lists[i].Substring(1, lists[i].Length - 2);
                            }
                        }
                    }

                    dataList.Add(lists);

                    // 「改行」をもとに戻す
                    for (int i = 0; i < lists.Count; ++i)
                    {
                        lists[i] = lists[i].Replace("改行", "\n");
                    }

                    // コンソールに出力する
                    //foreach (string list in lists)
                    //{
                    //    System.Console.Write("{0} ", list);
                    //}
                    //System.Console.WriteLine();
                }
            }

            return dataList;
        }
    }
}
