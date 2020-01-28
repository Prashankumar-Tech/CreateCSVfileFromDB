using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateCSVfileFromDB
{
    public static class CSVFileGenerator
    {
        private static readonly object FileLocker = new object();
        public static void ToCSV(this DataTable dtDataTable, string strFilePath, bool header, string CSVDelimiter)
        {
            lock (FileLocker)
            {
                System.IO.FileInfo file = new System.IO.FileInfo(strFilePath);
                file.Directory.Create();
                using (StreamWriter sw = new StreamWriter(strFilePath, false, new UTF8Encoding(false)))
                {
                    //headers  
                    if (header)
                    {
                        for (int i = 0; i < dtDataTable.Columns.Count; i++)
                        {
                            sw.Write(dtDataTable.Columns[i]);
                            if (i < dtDataTable.Columns.Count - 1)
                            {
                                sw.Write(CSVDelimiter);
                            }
                        }
                        sw.Write(sw.NewLine);
                    }
                    foreach (DataRow dr in dtDataTable.Rows)
                    {
                        for (int i = 0; i < dtDataTable.Columns.Count; i++)
                        {
                            if (!Convert.IsDBNull(dr[i]))
                            {
                                string value = dr[i].ToString().Trim();
                                if (value.Contains(','))
                                {
                                    value = String.Format("\"{0}\"", value);
                                    sw.Write(value);
                                }
                                else
                                {
                                    sw.Write(dr[i].ToString().Trim());
                                }
                            }
                            if (i < dtDataTable.Columns.Count - 1)
                            {
                                sw.Write(CSVDelimiter);
                            }
                        }
                        sw.Write(sw.NewLine);
                    }
                    sw.Close();
                }
            }
        }
    }

}
