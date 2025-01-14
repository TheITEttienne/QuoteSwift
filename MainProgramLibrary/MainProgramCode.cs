﻿using ProtoBuf;
using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;



namespace QuoteSwift
{
    public static class MainProgramCode
    {
        //Get Last Quote

        public static Quote GetLastQuote(ref Pass passed)
        {
            if (passed != null && passed.PassQuoteList != null)
            {
                int Index = 0;
                DateTime dt = passed.PassQuoteList[0].QuoteCreationDate;
                for (int i = 1; i < passed.PassQuoteList.Count; i++)
                    if (passed.PassQuoteList[i].QuoteCreationDate.Date > dt)
                    {
                        dt = passed.PassQuoteList[i].QuoteCreationDate.Date;
                        Index = i;
                    }
                return passed.PassQuoteList[Index];
            }

            return null;
        }

        /** Message Box Custom Functions */



        //Confirmation Request:

        public static bool RequestConfirmation(string text, string caption)
        {
            DialogResult MessageBoxResult = MessageBox.Show(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            return MessageBoxResult == DialogResult.Yes;
        }

        public static void ShowError(string text, string caption)
        {
            MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void ShowInformation(string text, string caption)
        {
            MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        /*********************************/


        /** Serialization Methods: */

        //Read file into byte Array

        public static byte[] RetreiveData(string FileName)
        {
            string StorePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\" + FileName;
            byte[] Data = null;
            try
            {
                Data = File.ReadAllBytes(StorePath);
                return Data;
            }
            catch (FileNotFoundException)
            {
                if (FileName != "ExportQuote.pbf")
                    if (MainProgramCode.RequestConfirmation(FileName + " could not be found.\nWould you like to continue the execution? (Recommended for first launch)", "REQUEST - Execution Continuation")) return Data;
                MainProgramCode.ShowError(FileName + " Could not be found, please contact the developer to fix this issue.", "ERROR - " + FileName + " Not Found");
                
            }
            catch
            {
                throw;
            }

            return null;
        }


        //Store Byte Array to Directory

        public static bool SaveData(string FileName, byte[] StoreData)
        {
            string StorePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\" + FileName;

            try
            {
                if (File.Exists(StorePath)) File.Delete(StorePath);
                BinaryWriter FileWriter = new BinaryWriter(File.OpenWrite(StorePath));
                FileWriter.Write(StoreData);
                FileWriter.Flush();
                FileWriter.Close();
            }
            catch
            {
                return false;
            }
            return true;
        }

        // Serialize - Protobuf

        public static byte[] ProtoSerialize<T>(T record) where T : class
        {
            if (null == record) return null;

            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    Serializer.Serialize(stream, record);
                    return stream.ToArray();
                }
            }
            catch
            {
                throw;
            }
        }

        //De-serialize - Protobuf

        public static T ProtoDeserialize<T>(byte[] data) where T : class
        {
            if (null == data) return null;

            try
            {
                using (MemoryStream stream = new MemoryStream(data))
                {
                    return Serializer.Deserialize<T>(stream);
                }
            }
            catch
            {
                throw;
            }
        }

        //Serialize Part list:

        public static byte[] SerializePartList(BindingList<Part> PartList)
        {
            byte[] tempByte;
            try
            {
                tempByte = MainProgramCode.ProtoSerialize<BindingList<Part>>(PartList);
            }
            catch
            {
                throw;
            }

            return tempByte;
        }

        //De-serialize Part list:

        public static BindingList<Part> DeserializePartList(byte[] tempByte)
        {
            try
            {
                return MainProgramCode.ProtoDeserialize<BindingList<Part>>(tempByte);
            }
            catch
            {
                return null;
            }
        }

        // Serialize Mandatory Part List Method

        public static void SerializeMandatoryPartList(ref Pass passed)
        {
            //Determine if Mandatory Parts exist:

            if (passed != null && passed.PassMandatoryPartList != null && passed.PassMandatoryPartList.Count > 0)
            {
                byte[] ToStore = MainProgramCode.SerializePartList(passed.PassMandatoryPartList);
                MainProgramCode.SaveData("MandatoryParts.pbf", ToStore);
            }
        }

        // Serialize Non-Mandatory Part List Method

        public static void SerializeNonMandatoryPartList(ref Pass passed)
        {
            //Determine if Non-Mandatory Parts exist:

            if (passed != null && passed.PassNonMandatoryPartList != null && passed.PassNonMandatoryPartList.Count > 0)
            {
                byte[] ToStore = MainProgramCode.SerializePartList(passed.PassNonMandatoryPartList);
                MainProgramCode.SaveData("NonMandatoryParts.pbf", ToStore);
            }
        }

        // Serialize Pump List Method

        public static byte[] SerializePumpList(BindingList<Pump> PumpPartList)
        {
            byte[] tempByte;
            try
            {
                tempByte = MainProgramCode.ProtoSerialize<BindingList<Pump>>(PumpPartList);
            }
            catch
            {
                throw;
            }

            return tempByte;
        }

        // De-serialize Pump List Method

        public static BindingList<Pump> DeserializePumpList(byte[] tempByte)
        {
            try
            {
                return MainProgramCode.ProtoDeserialize<BindingList<Pump>>(tempByte);
            }
            catch
            {
                return null;
            }
        }

        // Serialize and Store Pump List Method

        public static void SerializePumpList(ref Pass passed)
        {
            //Determine if Pump List exist:

            if (passed != null && passed.PassPumpList != null && passed.PassPumpList.Count > 0)
            {
                byte[] ToStore = MainProgramCode.SerializePumpList(passed.PassPumpList);
                MainProgramCode.SaveData("PumpList.pbf", ToStore);
            }
        }


        // Serialize Business List Method

        public static byte[] SerializeBusinessList(BindingList<Business> BusinessList)
        {
            byte[] tempByte;
            try
            {
                tempByte = MainProgramCode.ProtoSerialize<BindingList<Business>>(BusinessList);
            }
            catch
            {
                throw;
            }

            return tempByte;
        }

        // De-serialize Business List Method

        public static BindingList<Business> DeserializeBusinessList(byte[] tempByte)
        {
            try
            {
                return MainProgramCode.ProtoDeserialize<BindingList<Business>>(tempByte);
            }
            catch
            {
                return null;
            }
        }

        // Serialize and Store Business List Method

        public static void SerializeBusinessList(ref Pass passed)
        {
            //Determine if Pump List exist:

            if (passed != null && passed.PassBusinessList != null && passed.PassBusinessList.Count > 0)
            {
                byte[] ToStore = MainProgramCode.SerializeBusinessList(passed.PassBusinessList);
                MainProgramCode.SaveData("BusinessList.pbf", ToStore);
            }
        }

        // Serialize Quote List Method

        public static byte[] SerializeQuoteList(BindingList<Quote> QuoteList)
        {
            byte[] tempByte;
            try
            {
                tempByte = MainProgramCode.ProtoSerialize<BindingList<Quote>>(QuoteList);
            }
            catch
            {
                throw;
            }

            return tempByte;
        }

        // De-serialize Quote List Method

        public static BindingList<Quote> DeserializeQuoteList(byte[] tempByte)
        {
            try
            {
                return MainProgramCode.ProtoDeserialize<BindingList<Quote>>(tempByte);
            }
            catch
            {
                return null;
            }
        }

        // Serialize and Store Quote List Method

        public static void SerializeQuoteList(ref Pass passed)
        {
            //Determine if Pump List exist:

            if (passed != null && passed.PassQuoteList != null && passed.PassQuoteList.Count > 0)
            {
                byte[] ToStore = MainProgramCode.SerializeQuoteList(passed.PassQuoteList);
                MainProgramCode.SaveData("QuoteList.pbf", ToStore);
            }
        }

        // Serialize Quote To Export

        public static void ExportQuote(ref Quote q)
        {
            if (q != null)
            {
                byte[] tempByte;
                try
                {
                    tempByte = MainProgramCode.ProtoSerialize<Quote>(q);
                }
                catch
                {
                    throw;
                }
                MainProgramCode.SaveData("ExportToExcel\\ExportQuote.pbf", tempByte);
            }
        }

        // De-serialize Quote To Export To Excel

        /****************************************************/

        public static Quote DeserializeQuote(byte[] tempByte)
        {
            try
            {
                return ProtoDeserialize<Quote>(tempByte);
            }
            catch
            {
                return null;
            }
        }

        



        //Procedure Handling The Closing Of The Application
        public static void CloseApplication(bool b , ref Pass passed)
        {

            MainProgramCode.SerializeMandatoryPartList(ref passed);

            MainProgramCode.SerializeNonMandatoryPartList(ref passed);

            MainProgramCode.SerializePumpList(ref passed);

            MainProgramCode.SerializeBusinessList(ref passed);

            MainProgramCode.SerializeQuoteList(ref passed);

            if (b)
            {
                Application.Exit();
            }
        }

        /** Parse String Inputs: */

        // Parse Float:

        public static float ParseFloat(string t)
        {
            float.TryParse(t, out float temp);
            return temp;
        }

        // Parse Boole:

        public static bool ParseBoolean(string t)
        {
            bool.TryParse(t, out bool temp);
            return temp;
        }

        // Parse Int:

        public static int ParseInt(string t)
        {
            int.TryParse(t, out int temp);
            return temp;
        }


        /*************************/

    }
}
