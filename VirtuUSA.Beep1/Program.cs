using Beep1.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beep1Common.Utils;
using Beep1.MenuBase;
using Beep1.Utils;
using FishbowlSDK;
using Beep1.Objects;
using System.IO;
using Dapper;

namespace VirtuUSA.Beep1
{
    public class VirtuUSACycle : CustomerController
    {
        private ItemCache ic;

        public VirtuUSACycle(GlobalResources res) : base(res)
        {
            api = res.api;
            //db = res.db;
            ic = res.ItemCache;
        }

        public override bool HasAccess()
        {
            return true;
        }

        public override bool IsCompatible()
        {
            return true;
        }

        public override MenuOptions PublicMenuOption()
        {
            return new MenuOptions("VirtuUSA Cycle", "VUSAC", 99, this);
        }

        public override void Run()
        {
            bool cancel = false;
            while (!cancel)
            {
                C.CL();
                CycleCount(out cancel);
            }

        }
        private void CycleCount(out bool cancel)
        {
            //CycleLineItem item = new CycleLineItem();
            cancel = false;
            bool finish = false;
            String Location = null;
            String partNo = null;
            while (!cancel && !finish)
            {
                bool isReady = Location != null && !String.IsNullOrEmpty(partNo);
                C.CL();
                C.WL("Cycle Count");
                C.WL("Location: " + ((Location != null) ? Location : "NONE"));
                //C.WL("Part: " + (partNo ?? "NONE"));
                //C.WL("F1=Exit" + (isReady ? "F2=Count" : ""));
                C.WL("F1=Exit F3=Next Location");

                if (Location == null)
                {
                    C.WL("Location:");
                    String scan = C.getString(out cancel);
                    if (cancel)
                    {
                        var yn = C.YN("Are you sure? Exiting.");
                        cancel = yn;
                        return;
                    }
                    if (!String.IsNullOrWhiteSpace(scan))
                    {
                        Location = scan;
                        continue;
                    }
                    else
                    {
                        C.A("Location Entry Error");
                        continue;
                    }
                }
                if (String.IsNullOrEmpty(partNo))
                {
                    ConsoleKey? fnKey;
                    C.WL("Part #:");
                    String scan = C.getStringFinishableFunctions(out cancel, out finish, out fnKey);
                    if (cancel)
                    {
                        var yn = C.YN("Are you sure? Exiting.");
                        cancel = yn;
                        continue;
                    }
                    else if (finish)
                    {
                        var yn = C.YN("Are you sure? Finishing.");
                        finish = yn;
                        return;
                    }
                    else if (fnKey.HasValue)
                    {
                        if (fnKey.Value == ConsoleKey.F3)
                        {
                            Location = null;
                        }
                    }
                    //else if (!String.IsNullOrWhiteSpace(scan) && (scan.Length <= 7))
                    else if (!String.IsNullOrWhiteSpace(scan))
                    {
                        var part = ic.Find(scan);
                        if (part != null)
                        {
                            partNo = part.PARTNUMBER;
                        }
                        else
                        {
                            C.A("Part# Not Found");
                            continue;
                        }

                    }
                    else
                    {
                        C.A("Scan Error");
                        continue;
                    }
                }
                if (isReady)
                {
                    PartGetRsType rq = api.getPart(partNo);
                    if (rq.statusCode == "1000")
                    {
                        Save1CountToCSV(Location, partNo, 1);
                        //Location = null;
                        partNo = null;
                    }
                    continue;
                }
            }
        }
        private void Save1CountToCSV(String loc, String part, int count)
        {
            string fileName = AppDomain.CurrentDomain.BaseDirectory + @"\data\" + DateTime.Now.ToString("ddMMyyyy") + "_CycleCount.csv";
            //string fileName = AppDomain.CurrentDomain.BaseDirectory + @"\data\CycleCount_Master.csv";

            if (!File.Exists(fileName))
            {
                String header = "Location,Part No,Qty" + Environment.NewLine;
                File.WriteAllText(fileName, header);
            }
            String sw = loc + "," + part + "," + count + Environment.NewLine;
            File.AppendAllText(fileName, sw);
        }
    }
}
