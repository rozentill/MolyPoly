using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using UnityEngine;

public class MySQLManager {
	
	private Dictionary<string, PersistentDataFile.UserData> users;
	private Thread UserDataRequest;
	private const string connStr = "server=alacritas.cis.utas.edu.au;user=molypoly;database=molypoly;port=3306;password=L3ap1ng!;";
	public string databaseError = "";
	
	public MySQLManager() {
		users = new Dictionary<string, PersistentDataFile.UserData> {};

		//StartUserRequest ();
		CheckNotUploadedLogs ();
		UserDataRequest = new Thread(StartUserRequest);
		UserDataRequest.Start ();
	}

	private List<int> ModuleStringToArray(string modulespec) {
		return modulespec.Split (',').Select (a => a.Trim ()).Select (a => {
						int r;
						return int.TryParse (a, out r) ? r : 0;
				}).ToList ();
	}

	private void StartUserRequest() {

		const string query = "SELECT `StudentID`, `INDEX`, `Modules`, `users`.`GroupID` FROM `molypoly`.`users`,`molypoly`.`GroupModules`" +
			"WHERE `molypoly`.`users`.`GroupID` = `molypoly`.`GroupModules`.`GroupID`";

		//var result = new Dictionary<string, int> ();

		using (var conn = new MySqlConnection(connStr))
		using (var comm = new MySqlCommand(query, conn))
		{
			
			try {
				conn.Open();
						
				using (var reader = comm.ExecuteReader())
				{
					while(reader.Read ()) {
						users[reader.GetString(0)] = new PersistentDataFile.UserData() {
							Username = reader.GetString(0),
							UserID = (int)reader.GetUInt32(1),
							modules = ModuleStringToArray(reader.GetString(2)),
							GroupNumber = (int)reader.GetUInt32(3)
						};
					}
				}
			}
			catch (MySqlException ex)
			{
				//When handling errors, you can your application's response based 
				//on the error number.
				//The two most common error numbers when connecting are as follows:
				//0: Cannot connect to server.
				//1045: Invalid user name and/or password.
				switch (ex.Number)
				{
					case 0:
						databaseError = "Cannot connect to server.  Contact administrator";
						break;
					case 1045:
						databaseError = "Invalid username/password, please try again";
						break;
				}
			}
		}
	}

	public SessionLoginResult TryLogin(string username, ref PersistentDataFile.UserData userdata) {
		if (username == null)
			return SessionLoginResult.BadUsername;

		username = username.Trim ();
		/*First, test the username in case it is a predefined "bypass" username.*/
		if (users.ContainsKey (username)) {
						userdata = users [username];
						userid = (uint)userdata.UserID;
						//OpenLogFile();
						return SessionLoginResult.Success;
				}

		/*Maybe it doesn't contain that username because the data hasn't arrived yet...*/
		if (UserDataRequest.IsAlive)
			return SessionLoginResult.PleaseWait;
		
		/*Maybe the connection to the database failed?*/
		if(databaseError != "") {
			return SessionLoginResult.DatabaseError;
		}
	

		return SessionLoginResult.BadUsername;
	}

	public bool WaitingForUserData() {
		return UserDataRequest.IsAlive;
	}

	private static string NotUploadedPath = Load.persistentDataPath + "/LogNotUploaded";
	private static string UploadedPath = Load.persistentDataPath + "/LogUploaded";
	
	private void CheckNotUploadedLogs() {




		int count = 0;
		UnityEngine.Debug.Log("Checking directory " + NotUploadedPath + " for logs to upload");
		try {
		
		if (!System.IO.Directory.Exists (UploadedPath)) {
			System.IO.Directory.CreateDirectory (UploadedPath);
		} 


		//handle existing logs that aren't uploaded.
		//create "not uploaded yet" directory if it does not exist.
		if (!System.IO.Directory.Exists (NotUploadedPath)) {
			System.IO.Directory.CreateDirectory (NotUploadedPath);
		}
		else {

			//if it already exists, upload all the files that are in it.
			var files = System.IO.Directory.GetFiles(NotUploadedPath,"ActivityLog_*");
			foreach(var file in files) {


				char[] delim = {'_'};
				var bits = file.Split(delim);
				int index;

				//format: ActivityLog_INDEX_UNIQUEID
				if(bits.Length < 3 || !int.TryParse(bits[1], out index)) continue;
					if(WriteLogFileToDatabase(file, index)) {
					//move the file into "success" directory
					count++;
					System.IO.File.Move(file, UploadedPath + "/" + System.IO.Path.GetFileName(file));
					UnityEngine.Debug.Log("Uploading " + file + " success.");
				} else {
					UnityEngine.Debug.Log("Uploading " + file + " failed.");
				}
			}
		
		}

		}
		catch(SystemException ex) {
			UnityEngine.Debug.Log(ex.ToString());
		}

		UnityEngine.Debug.Log(string.Format("Completed uploading logs: {0} files uploaded", count));
		//var datetime_format = new System.Globalization.DateTimeFormatInfo ().SortableDateTimePattern;

		
	}

	private const string DateTimeFormat = "yyyy-MM-dd_hh-mm-ss-tt";

	public string GetUniqueLogFileName(string path, int count = 0) {
		//assume path is a valid directory.
		string filename = string.Format ("/ActivityLog_{0}_{1}.txt", userid, DateTime.Now.ToString(DateTimeFormat));
		if (count != 0) filename += "_" + count.ToString ();
		if (System.IO.File.Exists (filename))
						return GetUniqueLogFileName (path, count + 1);
				else
						return filename;
	}

	private UInt32 userid;
	private StreamWriter logFile;
	private int[] modulelist;

	public bool ResumeLogin(ref PersistentDataFile.UserData userdata) {
		this.userid = (uint) userdata.UserID;
		UserDataRequest.Join (2000);
		return TryLogin (userdata.Username, ref userdata) == SessionLoginResult.Success;
	}

	public void AddLogMessage(string name, object data) {
		var logData = new Dictionary<string, object>() {
			{"timestamp", System.DateTime.Now.ToString (DateTimeFormat)},
			{"event", name},
			{"user", userid},
		};
		if (data != null) logData.Add ("data", data);
		var message = MiniJSON.Json.Serialize (logData);

		if (logFile != null) {
			logFile.WriteLine(message);
			logFile.Flush();
		}
		if (UnityEngine.Debug.isDebugBuild) {
			UnityEngine.Debug.Log (message);
		}
	}


	public void OpenLogFile() {
		//DateTimeFormat = new System.Globalization.DateTimeFormatInfo ().SortableDateTimePattern;
		var filename = GetUniqueLogFileName (UploadedPath);
		try{
			logFile = new StreamWriter (NotUploadedPath + filename, false);
		}
		catch(DirectoryNotFoundException ex) {
			logFile = null;
		}
	}

	public void FinishLog() {
		if (UserDataRequest != null && UserDataRequest.IsAlive)
						UserDataRequest.Abort ();
			
		if (logFile != null) {
					UnityEngine.Debug.Log ("Closing log file ");
					logFile.Close();
					logFile = null;
		}

		CheckNotUploadedLogs ();
	}

	private bool WriteLogFileToDatabase(string filename, int index) {
		const string SQL = @"INSERT INTO `ActivityLog`(`SessionID`, `UserINDEX`, `File`, `FileSize`) VALUES (@SessionID, @Index, @File, @FileSize)";
	
		MySql.Data.MySqlClient.MySqlConnection conn;
		MySql.Data.MySqlClient.MySqlCommand cmd;

		conn = new MySql.Data.MySqlClient.MySqlConnection();
		cmd = new MySql.Data.MySqlClient.MySqlCommand();

		int FileSize;
		byte[] rawData;
		FileStream fs;

		conn.ConnectionString = connStr;

		try
		{
			fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
			FileSize = (int) fs.Length;

			rawData = new byte[FileSize];
			fs.Read(rawData, 0, FileSize);
			fs.Close();

			conn.Open();

			cmd.Connection = conn;
			cmd.CommandText = SQL;
			cmd.Parameters.AddWithValue("@SessionID", System.IO.Path.GetFileName(filename));
			cmd.Parameters.AddWithValue("@Index", index);
			cmd.Parameters.AddWithValue("@FileSize", FileSize);
			cmd.Parameters.AddWithValue("@File", rawData);
			cmd.ExecuteNonQuery();

			UnityEngine.Debug.Log("File inserted into  database successfully.");
			
			conn.Close();
			return true;
		}
		catch (MySql.Data.MySqlClient.MySqlException ex)
		{
			//Debug.WriteLine("File inserted into  database successfully.");
			UnityEngine.Debug.Log("Error " + ex.Number + " has occurred: " + ex.ToString());
			return false;
		}
	}
}
