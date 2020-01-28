using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateCSVfileFromDB
{
    class Program
    {
        static void Main(string[] args)
        {
            string sqlConnectionString = ConfigurationManager.ConnectionStrings["EmployeeDataContext"].ConnectionString;
            string CSVDelimiter = ConfigurationManager.AppSettings["CSVDelimiter"];
            string CSVFileNameEndsWith = ConfigurationManager.AppSettings["CSVFileNameEndsWith"];
            string CSVFilePath = ConfigurationManager.AppSettings["CSVFilePath"];
            DateTime dt = DateTime.UtcNow;
            string extension1 = dt.ToString("dd/MM/yyyy").Replace("/", "");
            string extension2 = dt.ToString("hh/mm/ss").Replace("/", "");
            string extension3 = CSVFileNameEndsWith ?? "Consultation";
            string inputZipFileName = CSVFilePath + "\\" + extension1 + "_" + extension2 + "_" + extension3 + ".zip";
            string employeeFile = CSVFilePath + "\\" + extension1 + "_" + extension2 + "_" + extension3 + "_KPI.csv";
            string inputEmployeeTypeFileName = CSVFilePath + "\\" + extension1 + "_" + extension2 + "_" + extension3 + "_Type.csv";
            CreateCSVFilesAndZipIt(sqlConnectionString, employeeFile, inputEmployeeTypeFileName,  CSVDelimiter, inputZipFileName);
        }

        private static bool CreateCSVFilesAndZipIt(string sqlConnectionString, string employeeFile, string inputEmployeeTypeFileName, string CSVDelimiter, string inputZipFileName)
        {
            bool success = false;
            Console.WriteLine("Inside CreateCSVFilesAndZipIt Method");

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                try
                {
                    using (var cmd = new SqlCommand("GetEmployeeReport", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataAdapter dataAdapter
                        = new SqlDataAdapter(cmd))
                        {
                            // create the DataSet 
                            DataSet dataSet = new DataSet();
                            // fill the DataSet using our DataAdapter 
                            dataAdapter.Fill(dataSet);
                            DataTable dataTableKPI = dataSet.Tables[0];
                            dataTableKPI.ToCSV(employeeFile, false, CSVDelimiter);
                            DataTable dataTableEmployeeType = dataSet.Tables[1];
                            dataTableEmployeeType.ToCSV(inputEmployeeTypeFileName, false, CSVDelimiter);
                            Console.WriteLine("Employee Type CSV file generated");

                            using (ZipArchive zip = ZipFile.Open(inputZipFileName, ZipArchiveMode.Create))
                            {
                                zip.CreateEntryFromFile(employeeFile, Path.GetFileName(employeeFile));
                                zip.CreateEntryFromFile(inputEmployeeTypeFileName, Path.GetFileName(inputEmployeeTypeFileName));
                            }
                            success = true;
                            Console.WriteLine("Zip file generated");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Zip file not generated:" + ex.Message);
                    success = false;

                    return success;
                }
                return success;
            }
        }
    }
}
