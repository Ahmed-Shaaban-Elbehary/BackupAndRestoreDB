using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Configuration;
using System.Collections.Generic;

namespace BackupAndRestoreDB
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            txt_ServerName.Text = Environment.MachineName.Trim();
            List<string> Dbs = GetDatabasesName();
            // Assuming you don't want to continually add new items use Clear()
            cmb_DatabaseName.Items.Clear();
            //Use AddRange to add the list.  ToArray() is used to convert List<> to string[]
            cmb_DatabaseName.Items.AddRange(Dbs.ToArray());
            cmb_DatabaseName.SelectedIndex = 0;
        }
        private void BackUp_Click(object sender, EventArgs e)
        {
            string _UserID = txt_UserId.Text.Trim();
            string _Password = txt_Password.Text.Trim();
            if (cmb_DatabaseName.SelectedIndex == 0)
            {
                MessageBox.Show(SystemMessages.errorDatabaseNameIsEmpty, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (txt_ServerName.Text == "" || txt_ServerName == null)
            {
                MessageBox.Show(SystemMessages.errorServerNameIsEmpty, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            string _InitialCatalog = cmb_DatabaseName.Text.Trim();
            string _DataSource = txt_ServerName.Text.Trim();

            SqlConnectionStringBuilder myBuilder = new SqlConnectionStringBuilder
            {
                UserID = _UserID,
                Password = _Password,
                InitialCatalog = _InitialCatalog,
                DataSource = _DataSource
            };

            string _DestinationPath = ConfigurationManager.AppSettings["DestinationPath"];

            BackupDatabase(myBuilder, _DestinationPath);
        }
        public void BackupDatabase(SqlConnectionStringBuilder csb, string destinationPath)
        {
            ServerConnection connection = null;
            Backup bkpDatabase = new Backup();
            SaveFileDialog sfd = new SaveFileDialog();
            if (csb.UserID != "" && csb.Password != "")
            {
                try
                {
                    connection = new ServerConnection(csb.DataSource, csb.UserID, csb.Password);
                    Server sqlServer = new Server(connection);
                    bkpDatabase.Action = BackupActionType.Database;
                    bkpDatabase.Database = csb.InitialCatalog;
                    sfd.Filter = "BackUp File|*" + csb.InitialCatalog + ".BAK";
                    sfd.FileName = "BackUp_" + csb.InitialCatalog + "_" + (DateTime.Now.ToString("ddMMyyyyHHmmss"));
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        BackupDeviceItem bkpDevice = new BackupDeviceItem(sfd.FileName, DeviceType.File);
                        bkpDatabase.Devices.Add(bkpDevice);
                        bkpDatabase.SqlBackup(sqlServer);

                        MessageBox.Show(SystemMessages.SuccessBackUpDatabase, "سيرفر", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    connection.Disconnect();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }
            else
            {

                try
                {
                    connection = new ServerConnection(csb.DataSource);
                    Server sqlServer = new Server(connection);
                    bkpDatabase.Action = BackupActionType.Database;
                    bkpDatabase.Database = csb.InitialCatalog;
                    sfd.Filter = "BackUp File|*" + csb.InitialCatalog + ".BAK";
                    sfd.FileName = "BackUp_" + csb.InitialCatalog + "_" + (DateTime.Now.ToString("ddMMyyyyHHmmss"));
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        BackupDeviceItem bkpDevice = new BackupDeviceItem(sfd.FileName, DeviceType.File);
                        bkpDatabase.Devices.Add(bkpDevice);
                        bkpDatabase.SqlBackup(sqlServer);
                        MessageBox.Show(SystemMessages.SuccessBackUpDatabase, "سيرفر", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    connection.Disconnect();
                }
                catch (Exception)
                {
                    MessageBox.Show(SystemMessages.errorFaildOperations, "سيرفر", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        public List<string> GetDatabasesName()
        {
            List<string> DbNameList = new List<string>();
            string connectionString = "Data Source=.; Integrated Security=True;";
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT name from sys.databases", con))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            DbNameList.Add(dr[0].ToString());
                        }
                    }
                }
                return DbNameList;
            }
        }
    }
}