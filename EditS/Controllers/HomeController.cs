using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LumenWorks.Framework.IO.Csv;
using System.Data;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Text;

namespace EditS.Controllers
{
    public class HomeController : ApiController
    {
        /// <summary>
        /// Gelen csv dosyalarını datatable'a dönüştüren fonksiyon. Parametre olarak Dosya Yolu alır.
        /// </summary>
        /// <param name="filePath">DosyaYolu</param>
        /// <returns>DataTables</returns>
        public DataTable CsvToDataTable(string filePath)
        {
           
            DataTable dt = new DataTable();
            using (StreamReader sr = new StreamReader(filePath, Encoding.GetEncoding("iso-8859-9"))) 
            {
                string[] headers = sr.ReadLine().Split(';');
                foreach (string header in headers)
                {
                    dt.Columns.Add(header);
                }
                while (!sr.EndOfStream) // Dosya bitene kadar oku
                {
                    string[] rows = sr.ReadLine().Split(';');
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < headers.Length; i++)
                    {
                        dr[i] = rows[i];
                    }
                    dt.Rows.Add(dr);//satırları oluştur
                }
            }
            DataTable dt2 = dt.Clone();
            dt2.Columns[3].DataType=typeof(Int32); // DataTable'ı klonlayıp 3. kolonun tipini int yap
            foreach(DataRow rows in dt.Rows) // yenidatatable'a verileri ekle
            {
                dt2.ImportRow(rows);
            } 
            return dt2;
        }


        /// <summary>
        /// Gelen dosyaları istatistiksel olarak değerlendirir.Json tipinde sonuç döndürür.
        /// </summary>
        /// <param name="url">Birden fazla csv dosya yolu </param>
        /// <returns>Json türünde değer verir.</returns>
        [HttpGet]
        public JObject evaluation([FromUri] List<String> url )
        {
            DataTable totalTable = new DataTable();
            for(int i=0;i<url.Count;i++)
            {
                DataTable dt = new DataTable();

                dt = CsvToDataTable(url[i]);
                if( url.Count>1)
                    {
                    totalTable.Merge(dt);
                    }
                else
                {
                    totalTable = dt;
                }
            }
                      

          
            string expression; //koşul
            string sortOrder; // sıralama

            //Hangi politikaci 2013 senesinde en fazla kelimeyi kullanmistir?
            #region Question 1 
            string answer1 = "";
            try
            {
                expression = "(tarih >= #1/1/2013# AND tarih <=#12/31/2013#)";
                sortOrder = "KelimeSayisi desc";

                DataRow[] result = totalTable.Select(expression, sortOrder);
                answer1 = result[0][0].ToString();
              
            }
            catch
            {
                answer1 = "null";
            }
            #endregion

            //Hangi politikaci "Güvenlik" hakkinda en fazla kelimeyi kullanmistir?
            #region Question 2
            string answer2 = "";
            try
            {
                expression = "(konu='Güvenlik')";
                sortOrder = "KelimeSayisi desc";

                DataRow[] result = totalTable.Select(expression, sortOrder);
                answer2 = result[0][0].ToString();

            }
            catch
            {
                answer2 = "null";
            }
            #endregion

            //Hangi politikaci en az kelimeyi kullanmistir?
            #region Question 3
            string answer3 = "";
            try
            {
                expression = "(KelimeSayisi=MIN(KelimeSayisi))";
                sortOrder = "";

                DataRow[] result = totalTable.Select(expression, sortOrder);
                answer3 = result[0][0].ToString();

            }
            catch
            {
                answer3 = "null";
            }
            #endregion

            JObject json = new JObject();
            json.Add("mostSpeeches", answer1);
            json.Add("mostSecurity", answer2);
            json.Add("leastWordy", answer3);
            return json;
        }

    }
}
