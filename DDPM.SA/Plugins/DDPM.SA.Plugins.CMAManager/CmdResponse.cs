using DDPM.RemoteManagement.Common.Interfaces;
using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DDPM.SA.Plugins.CMAManager
{
    public class CmdResponse
    {
        //private InfoResponse.InfoTask.InfoData d;

        //C:\ProgramData\Dell\Dell Display and Peripheral Manager
        private static readonly string path_programdata = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        private static readonly string path_folder = "\\Dell\\Dell Display and Peripheral Manager\\CMAResponse\\";
        private static readonly string FILE_PATH = path_programdata + path_folder;
        //private static readonly string FILE_PATH = @"c:\cmacmd\defer\";
        // private static readonly string FILE_NAME = @"fwjob_data.txt";



        private List<string> listResponse = new List<string>();

        private string data;

        private InfoResponse response;
        private bool isFileLoad;
        /*        private List<InfoTask> tasks;
                private List<InfoData> taskDatas;*/

        private string errorMsg = string.Empty;

        public CmdResponse(string _data)
        {
            data = _data;
            isFileLoad = false;

            listResponse = new List<string>();

            response = new InfoResponse(data, isFileLoad);
            //tasks = initTasks(response.response);

            //Console.WriteLine("CmdResponse response = " + response.ToString());

        }

        public CmdResponse(string guid, bool isLoadFile, FWUpdateInfo _fWUpdateInfo)
        {
            data = loadFile(guid);
            isFileLoad = true;

            listResponse = new List<string>();

            response = new InfoResponse(data, isFileLoad, _fWUpdateInfo);

        }

        public string getData()
        {
            return data;

        }

        public string getErrorMsg()
        {
            return errorMsg;

        }


        public string genResponseFw()
        {
            //string response = string.Empty;

            return response.ToString();

        }

        public string genResponseFwUpdate()
        {
            //string response = string.Empty;

            return response.ToString();

        }

        private string loadFile(string guid)
        {
            string responseString = string.Empty;

            try
            {

                //string info = string.Empty;
                string serialized_string = DDPMFileSecurity.GetSerializedJsonString((FILE_PATH + guid + ".txt"), out errorMsg);

                //Console.WriteLine(serialized_string);

                JArray jarray = JArray.Parse(serialized_string);
                foreach (var item in jarray)
                {

                    if (0 >= item.ToString().Length)
                    {
                        continue;
                    }

                    listResponse.Add(item.ToString());
                    responseString = item.ToString();   // add @ 20250220 stephen
                }
            }
            catch (Exception e)
            {
                // exception
                listResponse = new List<string>();
            }

            return responseString;

        }
        // modified @ 20250326 stephen
        public string writeToFile(string guid, string json)
        {
            if (json == null)
            {
                return "parameter error: json string is null";
            }

            try
            {
                if (!(System.IO.Directory.Exists(FILE_PATH)))
                {
                    System.IO.Directory.CreateDirectory(FILE_PATH);
                }

                // add @ 20250327 stephen
                if (!DDPMFileSecurity.SetFolderPermissions_UserReadAndExecute(FILE_PATH, out errorMsg))
                {
                    return ("[writeToFile] SetFolderPermissions_UserReadAndExecute failed: " + errorMsg);
                }
            }
            catch (Exception e)
            {
                return ("CreateDirectory Exception: " + e.Message);
            }

            /*            // add @ 20250220 stephen : fix string to an object
                        listResponse = new List<string>();
                        listResponse.Add(json);*/

            try
            {
                List<string> listResponse = new List<string> { json };
                string jsonString = JToken.FromObject(listResponse).ToString();
                bool write = DDPMFileSecurity.SetJsonContentFromSerializedString(jsonString, System.IO.Path.Combine(FILE_PATH, guid + ".txt"), out errorMsg);
                return "DDPMFileSecurity.SetJsonContentFromSerializedString = " + write + " ; " + errorMsg;

                /*                bool write = DDPMFileSecurity.SetJsonContentFromSerializedString(JToken.FromObject(listResponse).ToString(), (FILE_PATH + guid + ".txt"), out errorMsg);

                                return ("DDPMFileSecurity.SetJsonContentFromSerializedString = " + write + " ; " + errorMsg);*/
            }
            catch (Exception e)
            {
                return ("Serialization or File Write Exception: " + e.ToString());
            }
        }
        /*
         
        NotifyArgs args = new NotifyArgs();
        args.eventType = Params.EventType.FW.ToString();
        args.notification = "{\"sid\": \"" + "sid" + "\",\"gid\": \"" + data.Guid + "\",\"response\": [" + data.FWUErrorCode + "<" + (int)data.FWUErrorCode + ">" + "(" + data.DeviceName + ", " + data.Model + ")" + "]}";
        OnEventNotify(args);

        args.notification = "{\"sid\": \"" + sid + "\",\"gid\": \"" + gid + "\",\"response\": [" + finalResult + "]}";
        finalResult = finalResult + "{\"tid\": " + taskInfo.tid + ",\"result\": " + Params.Response.STATUS_COMMAND_ERROR_FORMAT_OR_PARAMS + ",\"msg\": \"" + responseMsg + "\",\"data\": [" + cliResult.serialize_Json_response + "]}";
         
         */

        #region Response struct
        public class InfoResponse
        {
            private bool isFileLoad;
            private FWUpdateInfo fWUpdateInfo;

            public InfoResponse(string json, bool _isFileLoad)
            {
                isFileLoad = _isFileLoad;

                try
                {
                    JObject jObject = JObject.Parse(json);

                    gid = (string)jObject["gid"];
                    sid = (string)jObject["sid"];
                    response = jObject["response"].ToArray();


                    //response.SetValue

                }
                catch (Exception e)
                {
                    Console.WriteLine("InfoResponse Exception: " + e.Message);
                }
            }

            public InfoResponse(string json, bool _isFileLoad, FWUpdateInfo _fWUpdateInfo)
            {
                isFileLoad = _isFileLoad;
                fWUpdateInfo = _fWUpdateInfo;

                try
                {
                    JObject jObject = JObject.Parse(json);

                    gid = (string)jObject["gid"];
                    sid = (string)jObject["sid"];
                    response = jObject["response"].ToArray();


                    //response.SetValue

                }
                catch (Exception e)
                {
                    Console.WriteLine("InfoResponse Exception: " + e.Message);
                }
            }

            public override string ToString()
            {
                string resultResponse = string.Empty;

                string finalResult = string.Empty;

                int counter = 0;

                foreach (var s in response)
                {

                    if (counter > 0)
                    {
                        finalResult = finalResult + ",";
                    }

                    InfoTask item = new InfoTask(s.ToString(), isFileLoad);

                    finalResult = finalResult + item.ToString();
                    counter++;
                }

                Console.WriteLine("@@@@@ response = " + response.Length);

                resultResponse = resultResponse + "{\"sid\": \"" + sid + "\",\"gid\": \"" + gid + "\",\"response\": [" + finalResult + "]}";

                //Console.WriteLine(resultResponse);

                return resultResponse;
            }

            public string gid { get; set; }
            public string sid { get; set; }
            public Array response { get; set; }


        }

        public class InfoTask
        {
            private bool isFileLoad;
            FWUpdateInfo fWUpdateInfo;
            public InfoTask(string _data, bool _isFileLoad)
            {
                isFileLoad = _isFileLoad;


                try
                {
                    JObject jObject = JObject.Parse(_data);
                    try
                    {
                        tid = (int)jObject["tid"];
                    }
                    catch
                    {
                        tid = 1;
                    }

                    try
                    {
                        result = (int)jObject["result"];
                    }
                    catch
                    {
                        result = 9999;
                    }

                    try
                    {
                        msg = (string)jObject["msg"];
                    }
                    catch
                    {
                        msg = "Error";
                    }

                    data = jObject["data"].ToArray();

                    foreach (var s in data)
                    {
                        InfoData item = new InfoData(s.ToString(), isFileLoad);

                        try
                        {
                            result = checkResult(item.result) ? Params.Response.STATUS_FW_UPDATE_STARTED : Params.Response.STATUS_FW_UPDATE_ERROR;

                            if (item.message.Contains("No device connected"))
                            {
                                result = Params.Response.STATUS_FW_UPDATE_DEVICE_NOT_CONNECTED;
                            }

                            if (item.message.Contains("No updates available"))
                            {
                                result = Params.Response.STATUS_FW_UPDATE_AT_LATEST;
                            }

                        }
                        catch
                        {
                            result = Params.Response.STATUS_COMMAND_ERROR_FORMAT_OR_PARAMS;
                        }



                    }


                }
                catch (Exception e)
                {
                    tid = 0;
                    result = Params.Response.UNKNOWN_ERROR;
                    msg = "InfoTask Exception: " + e.ToString();
                    data = string.Empty.ToArray();
                }

            }

            public InfoTask(string _data, bool _isFileLoad, FWUpdateInfo _fWUpdateInfo)
            {
                isFileLoad = _isFileLoad;
                fWUpdateInfo = _fWUpdateInfo;

                try
                {
                    JObject jObject = JObject.Parse(_data);

                    tid = (int)jObject["tid"];
                    result = responseCode((int)fWUpdateInfo.FWUErrorCode);
                    msg = fWUpdateInfo.FWUErrorCode.ToString();
                    data = jObject["data"].ToArray();



                }
                catch (Exception e)
                {
                    tid = 0;
                    result = Params.Response.UNKNOWN_ERROR;
                    msg = "InfoTask Exception: " + e.ToString();
                    data = string.Empty.ToArray();
                }

            }

            /*

                        public enum FWUErrorCode
                {
                    NoError = 0,
                    DeviceDisconnected = 1,
                    FirmwareUpdateFailed = 2,
                    FirmwareUpdatNotSupportedForThisDevice = 3,
                    FirmwareUpdateTimeout = 4,
                    PCBatteryTooLow = 5,
                    ConnectMultipleDocks = 6,
                    NetworkDisconnection = 7,
                    UserAborted = 8,
                    UserAbortedFail = 9,
                    FolderIsNotSafe = 10,
                    FileIsNoSafe = 11,
                    CAFail = 12,
                    NamedPipeServerIsNoSafe = 13,
                    FileCheckFail = 14,
                    ConnectMultipleSameModels = 15,
                    DeviceBatteryTooLow = 16,
                    Unknow = 99
                }

             */

            private int responseCode(int code)
            {

                int resultCode = -1;

                switch (code)
                {
                    case (int)FWUErrorCode.NoError:
                        resultCode = Params.Response.STATUS_FW_UPDATE_SUCCESS;
                        break;

                    case (int)FWUErrorCode.DeviceDisconnected:
                        resultCode = Params.Response.STATUS_FW_UPDATE_DEVICE_NOT_CONNECTED;
                        break;

                    case (int)FWUErrorCode.Unknow:
                        resultCode = Params.Response.UNKNOWN_ERROR;
                        break;

                    default:
                        resultCode = Params.Response.STATUS_FW_UPDATE_ERROR;
                        break;

                }


                return resultCode;
            }

            public override string ToString()
            {
                string response = string.Empty;

                string datas = string.Empty;

                int counter = 0;
                foreach (var s in data)
                {
                    if (counter > 0)
                    {
                        datas = datas + ",";
                    }
                    InfoData item = new InfoData(s.ToString(), isFileLoad);
                    datas = datas + item.ToString();
                    counter++;
                }



                response = response + "{";
                response = response + "\"tid\":" + tid + ",";
                response = response + "\"result\":" + result + ",";
                response = response + "\"msg\":\"" + msg + "\",";
                response = response + "\"data\":[" + datas + "]";

                response = response + "}";


                return response;
            }

            private bool checkResult(string inputString)
            {

                string lowerString = inputString.ToLower();

                if (lowerString.Equals("success"))
                {
                    return true;
                }

                if (lowerString.Equals("pass"))
                {
                    return true;
                }

                if (lowerString.Equals("completed"))
                {
                    return true;
                }

                return false;
            }

            public int tid { get; set; } = -1;
            public int result { get; set; } = -1;
            public string msg { get; set; } = string.Empty;
            public Array data { get; set; }
        }


        /*

             {
                      "FWVersion": "[1.0.5.a],[0.0.7.1]",
                      "FWUpdateRESPONSE": [
                        "Ready to start updating Device:Dell Secure Link Receiver to Version:1.0.5.b",
                        "Ready to start updating Device:Dell Pro Premium Mouse to Version:0.0.7.3"
                      ],
                      "Model": "Dell Secure Link Receiver,MS900",
                      "SerialNumber": "N/A",
                      "MarketingName": "N/A",
                      "Index": "N/A",
                      "ServiceTag": ",",
                      "Command": "SET",
                      "TargetFeature": "FIRMWAREUPDATE",
                      "Value": "MOUSE,FORCEWITHNOTICE",
                      "Result": "PASS",
                      "Message": "N/A"
              }

             */
        public class InfoData
        {

            private bool isFileLoad;
            public InfoData(string src, bool _isFileLoad)
            {
                isFileLoad = _isFileLoad;

                try
                {
                    JObject jObject = JObject.Parse(src.ToLower());

                    try
                    {
                        seqnum = (int)jObject["seqnum"];
                    }
                    catch
                    {
                        seqnum = 1;
                    }

                    if (isFileLoad)
                    {
                        seqnum = seqnum + 1;
                    }

                    try
                    {
                        index = (string)jObject["index"];
                    }
                    catch (Exception e)
                    {
                        index = e.ToString();
                    }

                    try
                    {
                        model = (string)jObject["model"];
                    }
                    catch (Exception e)
                    {
                        model = e.ToString();
                    }

                    try
                    {
                        servicetag = (string)jObject["servicetag"];
                    }
                    catch (Exception e)
                    {
                        servicetag = e.ToString();
                    }

                    try
                    {
                        marketingname = (string)jObject["marketingname"];
                    }
                    catch (Exception e)
                    {
                        marketingname = e.ToString();
                    }

                    try
                    {
                        serialnumber = (string)jObject["serialnumber"];
                    }
                    catch (Exception e)
                    {
                        serialnumber = e.ToString();
                    }

                    try
                    {
                        fwversion = (string)jObject["fwversion"];
                    }
                    catch (Exception e)
                    {
                        fwversion = e.ToString();
                    }



                    // result and Error message
                    try
                    {
                        result = (string)jObject["Result"];
                    }
                    catch
                    {
                        result = "N/A";
                    }

                    try
                    {
                        message = (string)jObject["Message"];
                    }
                    catch
                    {
                        message = "N/A";
                    }


                    try
                    {
                        Array fwarray = jObject["FWUpdateRESPONSE"].ToArray();

                        fwupdateresponse = string.Empty;

                        if (fwarray.Length > 0)
                        {
                            int counter = 0;
                            foreach (var fw in fwarray)
                            {
                                if (counter > 0)
                                {
                                    fwupdateresponse = fwupdateresponse + ",\n";
                                }
                                fwupdateresponse = fwupdateresponse + "\"" + fw.ToString() + "\"";  // fix PIMS-352424 @ 20250322 stephen
                                counter++;
                            }
                        }
                    }
                    catch
                    {
                        fwupdateresponse = string.Empty;
                    }

                }
                catch (Exception e)
                {

                    seqnum = 1;
                    index = "N/A";
                    model = "N/A";
                    servicetag = "N/A";
                    marketingname = "N/A";
                    serialnumber = "N/A";
                    fwversion = "N/A";
                    fwupdateresponse = string.Empty;

                    result = "N/A";
                    message = "N/A";

                    throw (new Exception("InfoData Constructor Exception: " + e.ToString()));

                }

            }

            public override string ToString()
            {
                Console.WriteLine("result = " + result);
                Console.WriteLine("message = " + message);

                string response = string.Empty;

                response = response + "{";
                response = response + "\"seqnum\":" + seqnum + ",";
                response = response + "\"index\":\"" + index + "\",";
                response = response + "\"model\":\"" + model + "\",";
                response = response + "\"servicetag\":\"" + servicetag + "\",";
                response = response + "\"marketingname\":\"" + marketingname + "\",";
                response = response + "\"serialnumber\":\"" + serialnumber + "\",";
                response = response + "\"fwversion\":\"" + fwversion + "\",";
                response = response + "\"fwupdateresponse\":[" + fwupdateresponse + "]";
                response = response + "}";


                return response;

            }

            public int seqnum { get; set; } = 1;
            public string index { get; set; } = "N/A";
            public string model { get; set; } = "N/A";
            public string servicetag { get; set; } = "N/A";
            public string marketingname { get; set; } = "N/A";
            public string serialnumber { get; set; } = "N/A";
            public string fwversion { get; set; } = "N/A";
            public string fwupdateresponse { get; set; } = "N/A";

            public string result { get; set; } = "N/A";
            public string message { get; set; } = "N/A";

        }
        #endregion
    }


}

