// Author: MyName
// Copyright:   Copyright 2021 Keysight Technologies
//              You have a royalty-free right to use, modify, reproduce and distribute
//              the sample application files (and/or any modified version) in any way
//              you find useful, provided that you agree that Keysight Technologies has no
//              warranty, obligations or liability for any sample application files.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using OpenTap;
using Keysight.OpenRanStudio;
using OpenRANStudio.RestClient;
using xRAN_Configuration.Persistent;
using xRAN_Configuration;
using static Keysight.OpenRanStudio.OrsConfiguration;
//using static Keysight.OpenRanStudio;

namespace ORAN_Studio.ORANStudio
{
    [Display("GeneratePCAP", Groups: new[] { "ORAN_Studio", "ORAN" }, Description: "Class for ORAN to generate PCAP file")]
    public class Generate_PCAP : TestStep
    {
        #region Settings
        private OpenTap.TraceSource ORAN_LOGS = OpenTap.Log.CreateSource("ORAN Studio Logs");

        //[FilePath(FilePathAttribute.BehaviorChoice.Open, "scp")]
        [Display("SCP File Name", Group: "ORAN Setting", Description: "Select SCP File name", Order: 1.1)]
        public string ScpFileName { get; set; }
        #endregion

        public Generate_PCAP()
        {
            ScpFileName = "6.5TAE";
        }

        public override void Run()
        {
            string _FullFilePath = @"C:\ORANscp\"+ ScpFileName +".scp";
            ORAN_LOGS.Info("Creating instance of ORAN Studio. Please wait...");
            Api myApi = new Api();
            ORAN_LOGS.Info("Loading SCP file");
            var myProject = myApi.ImportWaveformProject(_FullFilePath);
            ORAN_LOGS.Info("SCP file Loaded.");
            ORAN_LOGS.Info("getting Carrier Info from Project: " + myProject.GetType().GetProperty("NumberOfCarriers").GetValue(myProject));

            var myOrsConfig = new OrsConfiguration(myProject);
            ORAN_LOGS.Info("Setting Configuration");

            //Flow Table Entry
            //myOrsConfig.Flow_TableSize(DataDirectionT.DL, 1);
            myOrsConfig.Flow_TableSize(Keysight.OpenRanStudio.OrsConfiguration.DataDirection.DL, 1);
            myOrsConfig.Flow_TableEntry(Keysight.OpenRanStudio.OrsConfiguration.DataDirection.DL, 1, 1, 4, 4, 4, 4, Keysight.OpenRanStudio.OrsConfiguration.UPlaneCmpType.STATIC, Keysight.OpenRanStudio.OrsConfiguration.UPlaneCmpMethod.NONE, 16);
            myOrsConfig.FlowIdxMap_AddCarrierMapEntry(0, 1);
            myOrsConfig.Numerology_RecoverIqFlowBandwidth(OrsConfiguration.Bandwidth.FR1_100M);
            myOrsConfig.Numerology_RecoverIqFlowMu(OrsConfiguration.NumerologyType.Mu1);


            //Timing
            myOrsConfig.Timing_TCpDl(625000); // Downlink timing - DL C-plane Time Advance
            myOrsConfig.Timing_TUpDl(500000); // DL U-plane time advance
            myOrsConfig.Timing_TCpDl(125000); // C-plane time
            // Uplink timing
            myOrsConfig.Timing_TCpUl(250000); // UL C-plane time advance
            myOrsConfig.Timing_TUpUl(0); // UL U-pane
            
            //maximum transmissiom units and vlan ID
            myOrsConfig.Networking_Mtu(1400);// MTU
            myOrsConfig.Options_VlanId(100); // VLAN ID

            //generate pcap
            ORAN_LOGS.Info("Generating PCAP");
            myApi.ExportStimulus(myOrsConfig);
            ORAN_LOGS.Info("Pcap Generated");
        }
    }
}
