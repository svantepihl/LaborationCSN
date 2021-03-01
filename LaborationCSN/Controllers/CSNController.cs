using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Xml.Linq;
using LaborationCSN.Models;

namespace LaborationCSN.Controllers
{
    public class CSNController : Controller
    {
        SQLiteConnection sqlite;

        public CSNController()
        {
            string path = HostingEnvironment.MapPath("/db/");
            sqlite = new SQLiteConnection($@"DataSource={path}\csn.sqlite");

        }
        XElement SQLResult(string query, string root, string nodeName)
        {
            sqlite.Open();

            var adapt = new SQLiteDataAdapter(query, sqlite);
            var ds = new DataSet(root);
            adapt.Fill(ds, nodeName);
            XElement xe = XElement.Parse(ds.GetXml());

            sqlite.Close();
            return xe;
        }


        //
        // GET: /Csn/Test
        // 
        // Testmetod som visar på hur ni kan arbeta från SQL till XML till
        // presentations-xml som sedan används i vyn.
        // Lite överkomplicerat för just detta enkla fall men visar på idén.
        public ActionResult Test()
        {
            string query = @"SELECT a.Arendenummer, s.Beskrivning, SUM(((Sluttid-starttid +1) * b.Belopp)) as Summa
                            FROM Arende a, Belopp b, BeviljadTid bt, BeviljadTid_Belopp btb, Stodform s, Beloppstyp blt
                            WHERE a.Arendenummer = bt.Arendenummer AND s.Stodformskod = a.Stodformskod
                            AND btb.BeloppID = b.BeloppID AND btb.BeviljadTidID = bt.BeviljadTidID AND b.Beloppstypkod = blt.Beloppstypkod AND b.BeloppID LIKE '%2009'
							Group by a.Arendenummer
							Order by a.Arendenummer ASC";
            XElement test = SQLResult(query, "BeviljadeTider2009", "BeviljadTid");
            XElement summa = new XElement("Total",
                (from b in test.Descendants("Summa")
                 select (int)b).Sum());
            test.Add(summa);

            // skicka presentations xml:n till vyn /Views/Csn/Test,
            // i vyn kommer vi åt den genom variabeln "Model"
            return View(test);
        }

        //
        // GET: /Csn/Index

        public ActionResult Index()
        {
            
            return View();
        }


        //
        // GET: /Csn/Uppgift1

        public ActionResult Uppgift1()
        {
            string query =
                @"SELECT Arende.Arendenummer, Stodform.Beskrivning, Utbetalning.UtbetDatum, Utbetalning.UtbetStatus, sum(Belopp.Belopp*(UtbetaldTid.Sluttid - UtbetaldTid.StartTid+1)) as Belopp
                            FROM Utbetalning
                            LEFT JOIN Utbetalningsplan on Utbetalningsplan.UtbetPlanID = Utbetalning.UtbetPlanID
                            LEFT JOIN Arende on Arende.Arendenummer = Utbetalningsplan.Arendenummer
                            LEFT JOIN UtbetaldTid on UtbetaldTid.UtbetID = Utbetalning.UtbetID
                            LEFT JOIN UtbetaldTid_Belopp on UtbetaldTid_Belopp.UtbetaldTidID = UtbetaldTid.UtbetTidID
                            LEFT JOIN Belopp on Belopp.BeloppID = UtbetaldTid_Belopp.BeloppID
                            LEFT JOIN Stodform on Stodform.Stodformskod = Arende.Stodformskod
                            GROUP BY Arende.Arendenummer,Utbetalning.UtbetDatum
                            ORDER BY Arende.Arendenummer";
            
            XElement raw = SQLResult(query, "AllArenden", "Utbetalning");

            var groups = raw.Descendants("Utbetalning")
                .GroupBy(x => (string)x.Element("Arendenummer"))
                .ToList();

            XElement result = new XElement("UtbetalningarPerArende");
            
            foreach (var group in groups)
            {
                XElement arendeXElement = new XElement("Arende", new XAttribute("Arendenummer", group.Key));
                group.ToList().ForEach(x=>arendeXElement.Add(x));
                XElement totalSumma = new XElement("TotalSumma",
                    arendeXElement.Descendants("Belopp")
                        .Select(x => (int) x)
                        .Sum());
                arendeXElement.Add(totalSumma);
                
                XElement UtbetaldSumma = new XElement("UtbetaldSumma",
                    arendeXElement.Elements()
                        .Where(el => el.Element("UtbetStatus")?.Value == "Utbetald")
                        .Select(el => (int) el.Element("Belopp"))
                        .Sum());
                arendeXElement.Add(UtbetaldSumma);
                
                XElement KvarvarandeSumma = new XElement("KvarvarandeSumma",
                    arendeXElement.Elements()
                        .Where(el => el.Element("UtbetStatus")?.Value == "Planerad")
                        .Select(el => (int) el.Element("Belopp"))
                        .Sum());
                arendeXElement.Add(KvarvarandeSumma);

                XElement Beskrivning = new XElement("Beskrivning", arendeXElement.Descendants("Beskrivning").First().Value);
                arendeXElement.Add(Beskrivning);
                
                result.Add(arendeXElement);
            }
            
                

            return View(result);
        }


        //
        // GET: /Csn/Uppgift2

        public ActionResult Uppgift2()
        {
            return View();
        }

        //
        // GET: /Csn/Uppgift3

        public ActionResult Uppgift3()
        {
            return View();
        }
    }
}