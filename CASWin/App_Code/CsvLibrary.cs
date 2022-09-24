using System;
using System.Data;
using System.Configuration;
using System.IO;
using System.Text;

namespace CsvLib
{

    /// <summary>
    /// Summary description for CsvLibrary
    /// </summary>
    public class CsvLibrary
    {
        public CsvLibrary()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public static bool NoCont(string Number)
        {
            bool isnumber = true;

            if (Number.Length < 10)
                return false;

            foreach (char c in Number)
            {
                isnumber = char.IsNumber(c);

                if (!isnumber)
                {
                    return isnumber;
                }
            }

            return isnumber;
        }

        public static string ExportCsv(string Seperator, DataSet ds, bool exportColumnHeadings)
        {
            string header = string.Empty;
            string body = string.Empty;
            string record = string.Empty;// If you want column to be part of the CSV ...
            if (exportColumnHeadings)
            {
                foreach (DataColumn col in ds.Tables[0].Columns)
                {
                    header = header + col.ColumnName + Seperator;
                }
                header = header.Substring(0, header.Length - 1);
            }// Iterate into the rows
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                Object[] arr = row.ItemArray;
                for (int i = 0; i <= arr.Length - 1; i++)
                {
                    if (arr[i].ToString().IndexOf(Seperator) > 0)
                    {
                        record = record + arr[i].ToString() + Seperator;
                    }
                    else
                    {
                        record = record + arr[i].ToString() + Seperator;
                    }
                }
                body = body + record.Substring(0, record.Length - 1) + Environment.NewLine;
                record = "";
            }
            if (exportColumnHeadings)
            {
                return (header + Environment.NewLine + body);
            }
            else
            {
                return body;
            }
        }

        public static DataSet readCsv(string Seperator, Stream MyFileStrema)
        {
            string MystringLine;
            string[] MystringArray;
            char[] charArray = new char[1];
            DataSet MyDataSet = new DataSet();
            DataTable MyDataTable = MyDataSet.Tables.Add("TheData");
            StreamReader MyStreamReader = new StreamReader(MyFileStrema, Encoding.Default);

            charArray = Seperator.ToCharArray();
            try
            {
                MystringLine = MyStreamReader.ReadLine();

                MystringArray = MystringLine.Split(charArray);

                for (int i = 0; i <= MystringArray.GetUpperBound(0); i++)
                {
                    MyDataTable.Columns.Add(MystringArray[i].Trim());
                }

                MystringLine = MyStreamReader.ReadLine();
                while (MystringLine != null)
                {
                    MystringArray = MystringLine.Split(charArray);
                    DataRow dr = MyDataTable.NewRow();
                    for (int i = 0; i <= MystringArray.GetUpperBound(0); i++)
                    {
                        dr[i] = MystringArray[i].Trim();
                    }
                    MyDataTable.Rows.Add(dr);
                    MystringLine = MyStreamReader.ReadLine();
                }
                MyStreamReader.Close();
            }
            catch (Exception ex)
            {
                return null;
            }

            return MyDataSet;
        }


    }
}
