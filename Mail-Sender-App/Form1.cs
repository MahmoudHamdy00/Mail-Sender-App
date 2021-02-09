using System;
using System.Windows.Forms;
using System.Net;
using System.Net.Mail;
namespace Mail_Sender_App
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                button1.Enabled = false;
                string[] emails = Emails.Text.Split(',');
                MailMessage msg = new MailMessage();
                msg.From = new MailAddress(senderEmail.Text);

                msg.Subject = Subject.Text;
                msg.Body = Body.Text;
                // msg.Attachments
                SmtpClient smtp = new SmtpClient
                {
                    UseDefaultCredentials = false,
                    Host = "smtp.gmail.com",
                    EnableSsl = true,
                    Port = 587,
                    Credentials = new NetworkCredential(senderEmail.Text, password.Text)
                };
                foreach (string s in emails)
                {
                    //TO-DO: optimize it to ignore invalid mails
                    msg.To.Clear();
                    msg.To.Add(s);
                    smtp.Send(msg);
                }
                MessageBox.Show("Mission Acomplished", "Result", MessageBoxButtons.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                button1.Enabled = true;
            }
        }
    }
}
