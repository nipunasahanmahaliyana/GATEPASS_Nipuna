//using Aspose.Pdf.Facades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using memo_creator.DbConnection;
using memo_creator.Models;
using memo_creator.Repository;
using iTextSharp.text;
using iTextSharp.text.pdf;
//using iTextSharp.tool.xml;
using System.IO;
using System.Data;
using System.DirectoryServices;

namespace memo_creator.Static
{
    public class Utility
    {
        public DBAccess dbAccess = new DBAccess(); 
        Log eLog = new Log();

        static string ID = "notification";
        static string emailSubject = "Memo Creator";
        static string emailBody = "You have pending tasks. Please log into Memo Creator." + "\n" + "Through VPN/Intranet -  https://erequest.intranet.slt.com.lk/Memo" + "\n" +
            "Through Internet - https://erequest.slt.com.lk/Memo";
        static string emailBodyReqComplete = "Your memo has been approved. Please log in to Memo Creator to download the completed memo." + "\n" + "Through VPN/Intranet - https://erequest.intranet.slt.com.lk/Memo" + "\n" +
            "Through Internet - https://erequest.slt.com.lk/Memo";

        public string ConvertDateTime(string d)
        {
            //our format 8/1/2021 12:00:00 AM
            //convert to 2021-06-16 21:04:04 or 2021-06-16

            try
            {
                DateTime sourceDate = DateTime.Parse(d);
                string formatted = sourceDate.ToString("yyyy-MM-dd hh:mm:ss");
                return formatted;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public List<SystemUser> GetUserData(string username, string password)
        {
            string response = "";
            string[] sArray;
            string uname = "";
            string email = "";
            DataTable res = null;
            ProcessResult result = new ProcessResult();
            eLog.where_at = "Utility:GetUserData function";
            eLog.logged_by = username;

            SystemUser au = null;

            List<SystemUser> dList = new List<SystemUser>();
            dList = GetUserDataMock(username, password);
            if (dList != null && dList.Count > 0)
            {
                return dList;
            }
            au = new SystemUser();
            bool validation = false;
            try
            {

                DirectoryEntry root = new DirectoryEntry("LDAP://intranet.slt.com.lk", username, password);
                DirectorySearcher searcher = new DirectorySearcher(root, "(sAMAccountName=" + username + ")");
                searcher.FindOne().ToString();
                validation = true;
                au.SERVICE_NO = username;

                eLog.description = "AD SERVICE result(email) : " + au.EMAIL;
                eLog.priority = 19;
                eLog.log_type = 23;
                ELog(eLog);
            }
            catch (Exception ex)
            {

                validation = false;

                eLog.description = "AD SERVICE(from LDAP) exception : " + ex.Message;
                eLog.priority = 17;
                eLog.log_type = 21;
                ELog(eLog);
            }
            if (validation)
            {
                try
                {
                    //get designation, section, group of user from api
                    HRDATA_SERVICE.HRData serv = new HRDATA_SERVICE.HRData();
                    res = serv.getEmployee_Details(username, "");

                    string title, fname, surname, designation, section, group, office_phn;

                    if (res != null && res.Rows.Count > 0 && res.Rows[0] != null)
                    {
                        title = res.Rows[0]["EMPLOYEE_TITLE"].ToString();
                        fname = res.Rows[0]["EMPLOYEE_FIRST_NAME"].ToString();
                        surname = res.Rows[0]["EMPLOYEE_SURNAME"].ToString();
                        designation = res.Rows[0]["EMPLOYEE_DESIGNATION"].ToString();
                        section = res.Rows[0]["EMPLOYEE_SECTION"].ToString();
                        group = res.Rows[0]["EMPLOYEE_GROUP_NAME"].ToString();
                        office_phn = res.Rows[0]["EMPLOYEE_MOBILE_PHONE"].ToString();
                        email = res.Rows[0]["EMPLOYEE_OFFICIAL_EMAIL"].ToString();
                        au.NAME = title + " " + fname + " " + surname;
                        au.DESIGNATION = designation;
                        au.SECTION = section;
                        au.GROUP_NAME = group;
                        au.OFFICE_PHONE = FormatMobileNo(office_phn);
                        au.EMAIL = email;
                    }

                    eLog.description = "HRData SERVICE result(email) : " + au.EMAIL;
                    eLog.priority = 19;
                    eLog.log_type = 23;
                    ELog(eLog);

                    //check user already available in system
                    result = dbAccess.SaveUser(au);
                    return GetUserList(au);
                }
                catch (Exception ex)
                {
                    eLog.description = "HRData SERVICE exception : " + ex.Message;
                    eLog.priority = 17;
                    eLog.log_type = 21;
                    ELog(eLog);
                    return null;
                }
            }
            else
            {
                return null;
            }

        }

        public List<SystemUser> GetUserDataDebug(string service_no, string email)
        {
            string[] sArray;

            List<SystemUser> list = new List<SystemUser>();
            if (service_no != "" && email != "")
            {
                SystemUser su = new SystemUser();
                su.SERVICE_NO = service_no;
                su.EMAIL = email;
                list = dbAccess.GetUserList(su);
            }
            return list;
        }

        public List<SystemUser> GetUserDataMock(string service_no, string pwd)
        {
            List<SystemUser> list = new List<SystemUser>();
            if (service_no == "015922" && pwd == "1234")
            {
                SystemUser su = new SystemUser();
                su.SERVICE_NO = "015922";
                su.EMAIL = "hiranyak@slt.com.lk";
                su.NAME = "MS. Hiranya Kuruppu";
                su.PRI_ROLE = 2;
                list.Add(su);
            }
            return list;
        }


        public List<SystemUser> GetUserList(SystemUser su)
        {
            List<SystemUser> list = new List<SystemUser>();
            list = dbAccess.GetUserList(su);
            return list;
        }

        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyMMddHHmmss");
        }

        public ProcessResult SendEmailtoApprover(string memoId, string queueId = "1")
        {
            List<string> responses = new List<string> { };
            ProcessResult process = new ProcessResult();
            ProcessResult requestList = dbAccess.GetWorkProgress(memoId, queueId);
            List<Dictionary<string, string>> dataList = (List<Dictionary<string, string>>)requestList.dataBundle;
            if (dataList.Count == 0)
            {            
                string fileName = dbAccess.GetMemoById(memoId);
                ProcessResult requestor = dbAccess.GetWorkProgress(memoId, "0");
                List<Dictionary<string, string>> list = (List<Dictionary<string, string>>)requestor.dataBundle;
                Dictionary<string, string> dictionaryRequest = (Dictionary<string, string>)list[0];
                ProcessResult memoData = dbAccess.RetrieveMemo(memoId, "original");
                byte[] fileBytes = (byte[])memoData.dataBundle;
                //string outputFileName = "final_" + fileName;
                ProcessResult pdfData = EditPdf(fileName, memoId, fileBytes);
                byte[] outputPdfBytes = (byte[])pdfData.dataBundle;
                SendRequestCompleteMail(dictionaryRequest["ASSIGNEE"]);
                return dbAccess.MemoFileUpload(outputPdfBytes, memoId, "final");
            }
            else
            {
                foreach (Dictionary<string, string> dictionary in dataList)
                {
                    var email = dictionary["ASSIGNEE"];
                    string response = SendMail(email);
                    responses.Add(response);
                    process.dataBundle = responses;
                }
                return process;
            }         
        }

        public String SendMail(string toAddress)
        {
            try
            {
                MAIL_SERVICE.SendMail mail = new MAIL_SERVICE.SendMail();
                string response = mail.Send(ID, toAddress, emailSubject, emailBody, "", "");
                return response;
            }
            catch (Exception)
            {
                return "Failed Sending Mail";
            }
        }

        public String SendRequestCompleteMail(string toAddress)
        {
            try
            {
                MAIL_SERVICE.SendMail mail = new MAIL_SERVICE.SendMail();
                string response = mail.Send(ID, toAddress, emailSubject, emailBodyReqComplete, "", "");
                return response;
            }
            catch (Exception)
            {
                return "Failed Sending Mail";
            }
        }

        public String SendConfirmMail(string toAddress, string memoId, string action)
        {
            string emailBodyConform = "You have performed an action in Memo Creator (Action - " + action + ", Reference No. - " + memoId + "). If this is not you please inform to isurusampath@slt.com.lk";
            try
            {
                MAIL_SERVICE.SendMail mail = new MAIL_SERVICE.SendMail();
                string response = mail.Send(ID, toAddress, emailSubject, emailBodyConform, "", "");
                return response;
            }
            catch (Exception)
            {
                return "Failed Sending Mail";
            }
        }

        public ProcessResult EditPdf(string inputFileName, string memoId, byte[] inputBytes)
        {
            ProcessResult process = new ProcessResult();
            //string inputFile = @"C:\xampp\htdocs\projects\memo\" + inputFileName;
            //string outputFile = @"C:\xampp\htdocs\projects\updated_file\" + outputFileName;

            //Step 1: Create a Docuement-Object
            Document document = new Document();
            MemoryStream ms = new MemoryStream();
            try
            {
               // FileStream fs = new FileStream(outputFileName, FileMode.Create);
                
                //Step 2: we create a writer that listens to the document
                PdfWriter writer = PdfWriter.GetInstance(document, ms);

                writer.PageEvent = new MyHeaderFooterEvent() { memoId = memoId }; ;

                //Step 3: Open the document
                document.Open();

                PdfContentByte cb = writer.DirectContent;

                //The current file path
                //string filename = inputFile;

                // we create a reader for the document
                PdfReader reader = new PdfReader(inputBytes);

                for (int pageNumber = 1; pageNumber < reader.NumberOfPages + 1; pageNumber++)
                {
                    document.SetPageSize(reader.GetPageSizeWithRotation(1));
                    document.NewPage();

                    //Insert to Destination on the first page
                    if (pageNumber == 1)
                    {
                        Chunk fileRef = new Chunk(" ");
                        fileRef.SetLocalDestination(inputFileName);
                        document.Add(fileRef);
                    }

                    PdfImportedPage page = writer.GetImportedPage(reader, pageNumber);
                    int rotation = reader.GetPageRotation(pageNumber);
                    if (rotation == 90 || rotation == 270)
                    {
                        cb.AddTemplate(page, 0, -1f, 1f, 0, 0, reader.GetPageSizeWithRotation(pageNumber).Height);
                    }
                    else
                    {
                        cb.AddTemplate(page, 1f, 0, 0, 1f, 0, 0);
                    }
                }

                // Add a new page to the pdf file
                document.NewPage();

                Paragraph paragraph1 = new Paragraph();
                Paragraph paragraph2 = new Paragraph();

                Font titleFont = new Font(iTextSharp.text.Font.FontFamily.HELVETICA
                                            , 15
                                            , iTextSharp.text.Font.NORMAL
                                            , BaseColor.BLACK
                    );
                Font bodyFont = new Font(iTextSharp.text.Font.FontFamily.HELVETICA
                                           , 12
                                           , iTextSharp.text.Font.NORMAL
                                           , BaseColor.GRAY

                   );
                Chunk annexChunk = new Chunk("Annexures", titleFont);
                Chunk bullet = new Chunk("\u2022", bodyFont);
                List<Attachment> annexes = AnnexList(memoId);
                if (annexes != null)
                {
                    paragraph1.Add(annexChunk);
                    document.Add(paragraph1);
                    document.Add(Chunk.NEWLINE);
                    List list = new List(List.UNORDERED, 20f);
                    list.SetListSymbol("\u2022");
                    foreach (Attachment item in annexes)
                    {
                        list.Add(new ListItem(item.FILE_LOCATION, bodyFont));

                    }
                    document.Add(list);
                    document.Add(Chunk.NEWLINE);
                }
                

                Chunk titleChunk = new Chunk("Approval Summary", titleFont);
                paragraph2.Add(titleChunk);
                document.Add(paragraph2);
                document.Add(Chunk.NEWLINE);
                //Add table data
                PdfPTable table = CreateTable(memoId);
                //table.SetWidths(new float[] { 250, 420 });
                table.LockedWidth = true;
                table.TotalWidth = 500f;
                document.Add(table);
                process.isSuccess = true;
               
            }
            catch (Exception e)
            {
                throw e;
               
            }
            finally
            {
                document.Close();
                process.dataBundle = ms.ToArray();
            }
            return process;
        }

        public PdfPTable CreateTable(string memoId)
        {
            PdfPTable table = new PdfPTable(6);
            DBAccess dbAccess = new DBAccess();
            ProcessResult requestList = dbAccess.GetWorkProgressByMemoId(memoId);
            List<FileData> dataList = (List<FileData>)requestList.dataBundle;
            string[] columns = { "Step", "Assignee", "Action", "Comment", "Signed Time", "Signature"};

            Font headerFont = new Font(iTextSharp.text.Font.FontFamily.HELVETICA
                                            , 12
                                            , iTextSharp.text.Font.NORMAL
                                            , BaseColor.DARK_GRAY

                    );

            Font bodyFont = new Font(iTextSharp.text.Font.FontFamily.HELVETICA
                                            , 12
                                            , iTextSharp.text.Font.NORMAL
                                            , BaseColor.GRAY

                    );
            foreach (string col in columns)
            {
                PdfPCell header = new PdfPCell(new Phrase(col, headerFont));
                header.HorizontalAlignment = Element.ALIGN_CENTER;
                header.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(header);
            }

            foreach (FileData data in dataList)
            {
                
                PdfPCell body1 = new PdfPCell(new Phrase(data.workProgress.STAGE_ID.ToString(), bodyFont));
                body1.HorizontalAlignment = Element.ALIGN_CENTER;
                body1.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(body1);

                PdfPCell body2 = new PdfPCell(new Phrase(data.workProgress.systemUser.NAME, bodyFont));
                body2.HorizontalAlignment = Element.ALIGN_CENTER;
                body2.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(body2);

                PdfPCell body3 = new PdfPCell(new Phrase(data.workProgress.stage.NAME, bodyFont));
                body3.HorizontalAlignment = Element.ALIGN_CENTER;
                body3.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(body3);


                PdfPCell body4 = new PdfPCell(new Phrase(data.workProgress.ASSIGNEE_COMMENT, bodyFont));
                body4.HorizontalAlignment = Element.ALIGN_CENTER;
                body4.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(body4);

                PdfPCell body5 = new PdfPCell(new Phrase(data.workProgress.UPDATED_TIME.ToString(), bodyFont));
                body5.HorizontalAlignment = Element.ALIGN_CENTER;
                body5.VerticalAlignment = Element.ALIGN_MIDDLE;
                table.AddCell(body5);

                if (data.workProgress.STAGE_ID == 0) //stage id = 0 (creator) does not include in the query at the moment
                {

                    Image sigImage = null;
                    PdfPCell body6 = new PdfPCell(sigImage, true);
                    table.AddCell(body6);
                }
                else
                {
                    if (data.fileContent == null)
                    {

                        Image sigImage1 = null;
                        PdfPCell body61 = new PdfPCell(sigImage1, true);
                        table.AddCell(body61);
                    }
                    else
                    {
                        Image sigImage2 = Image.GetInstance(data.fileContent);
                        PdfPCell body62 = new PdfPCell(sigImage2, true);
                        table.AddCell(body62);
                    }
                }


            }
            return table;
        }

        public List<Attachment> AnnexList(string memoId)
        {
            DBAccess db = new DBAccess();
            ProcessResult process = db.GetAnnexureList(memoId);
            List<Attachment> list = (List<Attachment>)process.dataBundle;
            return list;
        }

        public void ELog(Log log)
        {
            Dictionary<string, string> insertDic = new Dictionary<string, string>();
            var created_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); //2008-10-03 22:59:52
            insertDic.Add("description", log.description);
            insertDic.Add("logged_by", log.logged_by);
            insertDic.Add("log_type", "" + log.log_type);
            insertDic.Add("priority", "" + log.priority);
            insertDic.Add("where_at", "Login");
            //insertDic.Add("created_time", created_time);
            dbAccess.ELog(insertDic);
        }

        public ProcessResult AddAdmin(string serviceNo)
        {

            DataTable res = null;
            ProcessResult result = new ProcessResult();
            SystemUser systemUser = new SystemUser();
            DBAccess dBAccess = new DBAccess();
            systemUser.SERVICE_NO = serviceNo;

            try
            {
                //get designation, section, group of user from api
                HRDATA_SERVICE.HRData serv = new HRDATA_SERVICE.HRData();
                res = serv.getEmployee_Details(systemUser.SERVICE_NO, "");

                string title, fname, surname, designation, section, group, email, phone;

                if (res != null && res.Rows.Count > 0 && res.Rows[0] != null)
                {
                    title = res.Rows[0]["EMPLOYEE_TITLE"].ToString();
                    fname = res.Rows[0]["EMPLOYEE_FIRST_NAME"].ToString();
                    surname = res.Rows[0]["EMPLOYEE_SURNAME"].ToString();
                    designation = res.Rows[0]["EMPLOYEE_DESIGNATION"].ToString();
                    section = res.Rows[0]["EMPLOYEE_SECTION"].ToString();
                    group = res.Rows[0]["EMPLOYEE_GROUP_NAME"].ToString();
                    email = res.Rows[0]["EMPLOYEE_OFFICIAL_EMAIL"].ToString();
                    phone = res.Rows[0]["EMPLOYEE_MOBILE_PHONE"].ToString();
                    systemUser.NAME = title + " " + fname + " " + surname;
                    systemUser.DESIGNATION = designation;
                    systemUser.SECTION = section;
                    systemUser.GROUP_NAME = group;
                    systemUser.EMAIL = email;
                    systemUser.OFFICE_PHONE = FormatMobileNo(phone);
                    result = dbAccess.SaveAdmin(systemUser);
                }
                else
                {
                    result.isSuccess = false;
                    result.errorMessage = "Invalid Service No.";
                    result.dataBundle = "Invalid Service No.";
                }
                return result;
            }
            catch (Exception ex)
            {
                result.isSuccess = false;
                result.errorMessage = ex.ToString();
                result.dataBundle = ex.ToString();
                return result;
            }

        }
        public string FormatMobileNo(string mobile)
        {
            var newMobile = "";
            if (mobile.StartsWith("+94"))
            {
                newMobile = "0" + mobile.Substring(3);
                return newMobile;
            }
            else
            {
                return mobile;
            }
        }
    }
}