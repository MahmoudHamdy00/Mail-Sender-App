using System;
using System.Windows.Forms;
using System.Net;
using System.Net.Mail;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Threading;

namespace Mail_Sender_App
{
    public partial class Form1 : Form
    {
        private string filePath;
        MailMessage msg;
        SmtpClient smtp;

        Thread thread;
        public Form1()
        {
            InitializeComponent();
           
            filePath = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;// full path untill bin folder
            filePath = Directory.GetParent(filePath).FullName;// full path untill broject folder
            filePath = Path.Combine(filePath, "sourceDataSheet.xlsx");

            dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

            dataGridView1.Visible = true;
            dataGridView1.DataSource = ReadExcel(filePath, "xlsx");

            thread = new Thread(sendByIndividual);

        }
        private void send_btn_Click(object sender, EventArgs e)
        {
            try
            {
                send_btn.Enabled = false;
                /*MailMessage*/
                msg = new MailMessage();
                msg.From = new MailAddress(senderEmail.Text);

                msg.Subject = Subject.Text;
                // msg.Attachments
                /*SmtpClient*/
                smtp = new SmtpClient
                {
                    UseDefaultCredentials = false,
                    Host = "smtp.gmail.com",
                    EnableSsl = true,
                    Port = 587,
                    //  DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(senderEmail.Text, password.Text),
                    // Timeout = 20000

                };
                if (tabControl1.SelectedIndex == 0)
                {
                    sendByGroup(msg, smtp);
                }
                else
                {
                    //thread.Start();
                    //while (thread.IsAlive) ;
                      sendByIndividual(/*msg, smtp*/);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                send_btn.Enabled = true;

            }

        }
        private void sendByGroup(MailMessage msg, SmtpClient smtp)
        {
            if (Emails.Text.Length == 0)
            {
                MessageBox.Show("Please Type the recipients");
                return;
            }
            string[] emails = Emails.Text.Split(',');
            msg.Body = "Dear " + name.Text + Environment.NewLine;
            msg.Body += Body.Text;
            msg.To.Clear();
            foreach (string s in emails)
            {
                //TO-DO: optimize it to ignore invalid mails
                msg.To.Add(s);
            }
            smtp.Send(msg);
            MessageBox.Show("Mission Acomplished, Sent Email to " + msg.To.Count, "Result", MessageBoxButtons.OK);

        }
        private void sendByIndividual(/*MailMessage msg, SmtpClient smtp*/)
        {
            if (dataGridView1.RowCount == 0)
            {
                MessageBox.Show("Please open the data sheet that has the recipients");
                return;
            }
            int cnt = 0;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells[0].Value == null || row.Cells[1].Value == null || row.Cells[0].Value.ToString() == "") continue;
                string email = row.Cells[0].Value.ToString();
                string name = row.Cells[1].Value.ToString();
                string isSentBefore = row.Cells[2].Value.ToString();
                if (isSentBefore == "Sent") continue;
                // MessageBox.Show(email + "," + name);
                msg.Body = "Dear " + name + "\n";
                msg.Body += Body.Text;
                cnt++;
                msg.To.Clear();
                msg.To.Add(email);

                smtp.Send(msg);
                row.Cells[2].Value = "Sent";
              //  saveButton.PerformClick();
            }
            MessageBox.Show("Mission Acomplished, Sent Email to " + cnt, "Result", MessageBoxButtons.OK);
            saveButton.PerformClick();
            if (thread.IsAlive)
                thread.Abort();

        }

        private void openSourceSheetbtn_Click(object sender, EventArgs e)
        {
            filePath = string.Empty;
            string fileExt = string.Empty;
            OpenFileDialog file = new OpenFileDialog(); //open dialog to choose file  
            if (file.ShowDialog() == System.Windows.Forms.DialogResult.OK) //if there is a file choosen by the user  
            {
                filePath = file.FileName; //get the path of the file  
                fileExt = Path.GetExtension(filePath); //get the file extension  
                if (fileExt.CompareTo(".xls") == 0 || fileExt.CompareTo(".xlsx") == 0)
                {
                    try
                    {
                        DataTable dtExcel = new DataTable();
                        dtExcel = ReadExcel(filePath, fileExt); //read excel file  
                        dataGridView1.Visible = true;
                        dataGridView1.DataSource = dtExcel;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message.ToString());
                    }
                }
                else
                {
                    MessageBox.Show("Please choose .xls or .xlsx file only.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error); //custom messageBox to show error  
                }
            }
        }

        private DataTable ReadExcel(string fileName, string fileExt)
        {
            // install https://www.microsoft.com/en-us/download/confirmation.aspx?id=13255

            string conn = string.Empty;
            DataTable dtexcel = new DataTable();
            if (fileExt.CompareTo(".xls") == 0)
                conn = @"provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileName + ";Extended Properties='Excel 8.0;HRD=Yes;IMEX=1';"; //for below excel 2007  
            else
                conn = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileName + ";Extended Properties='Excel 12.0;HDR=YES';"; //for above excel 2007  
            using (OleDbConnection con = new OleDbConnection(conn))
            {
                try
                {
                    OleDbDataAdapter oleAdpt = new OleDbDataAdapter("select * from [Sheet1$]", con); //here we read data from sheet1  
                    oleAdpt.Fill(dtexcel); //fill excel data into dataTable  
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
            return dtexcel;
        }

        private void dataGridView1_DataSourceChanged(object sender, EventArgs e)
        {
            RemoveEmptyColumns(ref dataGridView1);
        }
        public void RemoveEmptyColumns(ref DataGridView grdView)
        {
            int numOfcols = grdView.Columns.Count;
            foreach (DataGridViewColumn clm in grdView.Columns)
            {
                //clm.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                if (clm.HeaderText.Length <= 3)//nead to think in a beter condition
                {
                    numOfcols--;
                    grdView.Columns[clm.Index].Visible = false;
                }
            }
            foreach (DataGridViewColumn clm in grdView.Columns)
            {
                clm.Width = (grdView.Width - 70) / numOfcols;
            }
            AdjustColumnSize(ref grdView, numOfcols);
        }
        public void AdjustColumnSize(ref DataGridView grdView, int numOfCols)
        {
            foreach (DataGridViewColumn clm in grdView.Columns)
            {
                clm.Width = (grdView.Width - 70) / numOfCols;
            }
            grdView.AutoResizeRows();
        }
        private void dataGridView1_SizeChanged(object sender, EventArgs e)
        {
            RemoveEmptyColumns(ref dataGridView1); //need to implemnt diffrent function than remove :no optimize performance
        }

        private void saveButton_Click(object sender, EventArgs e)
        {

            // creating Excel Application  
            Microsoft.Office.Interop.Excel._Application app = new Microsoft.Office.Interop.Excel.Application();
            //File.Open(filePath, FileMode.OpenOrCreate);
            // creating new WorkBook within Excel application  
            Microsoft.Office.Interop.Excel._Workbook workbook = app.Workbooks.Open(filePath);
            //Add(Type.Missing);
            // creating new Excelsheet in workbook  
            Microsoft.Office.Interop.Excel._Worksheet worksheet = null;
            // see the excel sheet behind the program  
            //app.Visible = true;
            // get the reference of first sheet. By default its name is Sheet1.  
            // store its reference to worksheet  
            worksheet = workbook.Sheets["Sheet1"];
            worksheet = workbook.ActiveSheet;
            // changing the name of active sheet  
            // storing header part in Excel  
            for (int i = 1; i < dataGridView1.Columns.Count + 1; i++)
            {
                worksheet.Cells[1, i] = dataGridView1.Columns[i - 1].HeaderText;
            }
            // storing Each row and column value to excel sheet  
            for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
            {
                for (int j = 0; j < dataGridView1.Columns.Count; j++)
                {
                    worksheet.Cells[i + 2, j + 1] = dataGridView1.Rows[i].Cells[j].Value.ToString();
                }
            }
            // save the application  
            workbook.Save();
            // workbook.SaveAs(@"C:\Users\MahmoudHamdy-LAPTOP\OneDrive\Desktop\output111.xlsx", Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
            // Exit from the application  
            //workbook.Save();

            app.Quit();

        }

        private void stopSendingBtn_Click(object sender, EventArgs e)
        {
            try
            {
                thread.Interrupt();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
