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
        /// Gelen dosyları istatistiksel olarak değerlendirir.Json tipinde sonuç döndürür.
        /// </summary>
        /// <param name="url">Birden fazla csv dosya yolu </param>
        /// <returns>Json türünde değer verir.</returns>
        [HttpGet]
        public JObject evaluation([FromUri] List<String> url )
        {
            DataTable sonuc = new DataTable();
            for(int i=0;i<url.Count;i++)
            {
                DataTable dt = new DataTable();

                dt = CsvToDataTable(url[i]);
                if( url.Count>1)
                    {
                    sonuc.Merge(dt);
                    }
                else
                {
                    sonuc = dt;
                }
            }
                      

          
            string expression; //koşul
            string sortOrder; // sıralama

            //Hangi politikaci 2013 senesinde en fazla kelimeyi kullanmistir?
            #region Soru 1 
            string cevap1 = "";
            try
            {
                expression = "(tarih >= #1/1/2013# AND tarih <=#12/31/2013#)";
                sortOrder = "KelimeSayisi desc";

                DataRow[] result = sonuc.Select(expression, sortOrder);
                cevap1 = result[0][0].ToString();
              
            }
            catch
            {
                cevap1 = "null";
            }
            #endregion

            //Hangi politikaci "Güvenlik" hakkinda en fazla kelimeyi kullanmistir?
            #region Soru 2
            string cevap2 = "";
            try
            {
                expression = "(konu='Güvenlik')";
                sortOrder = "KelimeSayisi desc";

                DataRow[] result = sonuc.Select(expression, sortOrder);
                cevap2 = result[0][0].ToString();

            }
            catch
            {
                cevap2 = "null";
            }
            #endregion

            //Hangi politikaci en az kelimeyi kullanmistir?
            #region Soru 3
            string cevap3 = "";
            try
            {
                expression = "(KelimeSayisi=MIN(KelimeSayisi))";
                sortOrder = "";

                DataRow[] result = sonuc.Select(expression, sortOrder);
                cevap3 = result[0][0].ToString();

            }
            catch
            {
                cevap3 = "null";
            }
            #endregion

            JObject json = new JObject();
            json.Add("mostSpeeches", cevap1);
            json.Add("mostSecurity", cevap2);
            json.Add("leastWordy", cevap3);
            return json;
        }

    }
}
