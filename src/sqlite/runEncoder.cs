/*
 * This file is part of ChronoJump
 *
 * ChronoJump is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or   
 *    (at your option) any later version.
 *    
 * ChronoJump is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
 *    GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 * Copyright (C) 2019   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
//using System.Data;
using System.Collections;
using System.IO; //DirectoryInfo
using Mono.Data.Sqlite;
using System.Text.RegularExpressions; //Regex

class SqliteRunEncoder : Sqlite
{
	private static string table = Constants.RunEncoderTable;

	public SqliteRunEncoder() {
	}

	~SqliteRunEncoder() {}

	/*
	 * create and initialize tables
	 */

	protected internal static void createTable()
	{
		dbcmd.CommandText =
			"CREATE TABLE " + table + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"personID INT, " +
			"sessionID INT, " +
			"exerciseID INT, " + //right now all will be exercise 0, until we have a clear idea of what exercises could be done and how can affect measurements
			"device TEXT, " +
			"distance INT, " +
			"temperature INT, " +
			"filename TEXT, " +
			"url TEXT, " +		//URL of data files. stored as relative
			"datetime TEXT, " + 	//2019-07-11_15-01-44
			"comments TEXT, " +
			"videoURL TEXT)";	//URL of video of signals. stored as relative
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}

	public static int Insert (bool dbconOpened, string insertString)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "INSERT INTO " + table +
				" (uniqueID, personID, sessionID, exerciseID, device, distance, temperature, filename, url, dateTime, comments, videoURL)" +
				" VALUES " + insertString;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery(); //TODO uncomment this again

		string myString = @"select last_insert_rowid()";
		dbcmd.CommandText = myString;
		int myLast = Convert.ToInt32(dbcmd.ExecuteScalar()); // Need to type-cast since `ExecuteScalar` returns an object.

		closeIfNeeded(dbconOpened);

		return myLast;
	}

	public static void Update (bool dbconOpened, string updateString)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "UPDATE " + table + " SET " + updateString;

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);
	}

	/* right now unused
	public static void DeleteSQLAndFile (bool dbconOpened, int uniqueID)
	{
		RunEncoder fs = (RunEncoder) Select (dbconOpened, uniqueID, -1, -1)[0];
		DeleteSQLAndFile (dbconOpened, fs);
	}
	*/
	public static void DeleteSQLAndFile (bool dbconOpened, RunEncoder fs)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "DELETE FROM " + table + " WHERE uniqueID = " + fs.UniqueID;

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);

		//delete the file
		Util.FileDelete(fs.FullURL);
	}

	public static ArrayList Select (bool dbconOpened, int uniqueID, int personID, int sessionID)
	{
		openIfNeeded(dbconOpened);

		/*
		 * future: when we have RunEncoderExerciseTable
		string selectStr = "SELECT " + table + ".*, " + Constants.RunEncoderExerciseTable + ".Name FROM " + table + ", " + Constants.RunEncoderExerciseTable;
		string whereStr = " WHERE " + table + ".exerciseID = " + Constants.RunEncoderExerciseTable + ".UniqueID ";
		*/
		string selectStr = "SELECT * FROM " + table;
		string connector = " WHERE ";

		string uniqueIDStr = "";
		if(uniqueID != -1)
		{
			uniqueIDStr = connector + table + ".uniqueID = " + uniqueID;
			connector = " AND ";
		}

		string personIDStr = "";
		if(personID != -1)
		{
			personIDStr = connector + table + ".personID = " + personID;
			connector = " AND ";
		}

		string sessionIDStr = "";
		if(sessionID != -1)
		{
			sessionIDStr = connector + table + ".sessionID = " + sessionID;
			connector = " AND ";
		}

		//dbcmd.CommandText = selectStr + whereStr + uniqueIDStr + personIDStr + sessionIDStr + " Order BY " + table + ".uniqueID";
		dbcmd.CommandText = selectStr + uniqueIDStr + personIDStr + sessionIDStr + " Order BY " + table + ".uniqueID";

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList array = new ArrayList(1);
		RunEncoder fs;

		while(reader.Read()) {
			fs = new RunEncoder (
					Convert.ToInt32(reader[0].ToString()),	//uniqueID
					Convert.ToInt32(reader[1].ToString()),	//personID
					Convert.ToInt32(reader[2].ToString()),	//sessionID
					Convert.ToInt32(reader[3].ToString()),	//exerciseID
					(RunEncoder.Devices) Enum.Parse(
						typeof(RunEncoder.Devices), reader[4].ToString()), 	//device
					Convert.ToInt32(reader[5].ToString()),	//distance
					Convert.ToInt32(reader[6].ToString()),	//temperature
					reader[7].ToString(),			//filename
					Util.MakeURLabsolute(fixOSpath(reader[8].ToString())),	//url
					reader[9].ToString(),			//datetime
					reader[10].ToString(),			//comments
					reader[11].ToString()			//videoURL
					/*
					reader[11].ToString(),			//videoURL
					reader[12].ToString()			//exerciseName
					*/
					);
			array.Add(fs);
		}

		reader.Close();
		closeIfNeeded(dbconOpened);

		return array;
	}

	protected internal static void import_from_1_70_to_1_71() //database is opened
	{
		LogB.PrintAllThreads = true; //TODO: remove this
		LogB.Information("at import_from_1_70_to_1_71()");
		//LogB.Information("Sqlite isOpened: " + Sqlite.IsOpened.ToString());

		string raceAnalyzerDir = Util.GetRunEncoderDir();
		DirectoryInfo [] sessions = new DirectoryInfo(raceAnalyzerDir).GetDirectories();
		foreach (DirectoryInfo session in sessions) //session.Name will be the UniqueID
		{
			FileInfo[] files = session.GetFiles();
			foreach (FileInfo file in files)
			{
				//in dir there are .csv and .png, take only the .csv
				if(Util.GetExtension(file.Name) != ".csv")
					continue;

				string fileWithoutExtension = Util.RemoveExtension(Util.GetLastPartOfPath(file.Name));
				RunEncoderLoadTryToAssignPerson relt =
					new RunEncoderLoadTryToAssignPerson(true, fileWithoutExtension, Convert.ToInt32(session.Name));

				Person p = relt.GetPerson();
				if(p.UniqueID == -1)
					continue;

				if(! Util.IsNumber(session.Name, false))
					continue;

				string parsedDate = UtilDate.ToFile(DateTime.MinValue);
				Match match = Regex.Match(file.Name, @"(\d+-\d+-\d+_\d+-\d+-\d+)");
				if(match.Groups.Count == 2)
					parsedDate = match.Value;

				//filename will be this
				string myFilename = p.UniqueID + "_" + p.Name + "_" + parsedDate + ".csv";
				//try to move the file
				try{
					File.Move(file.FullName, Util.GetRunEncoderSessionDir(Convert.ToInt32(session.Name)) + Path.DirectorySeparatorChar + myFilename);
				} catch {
					//if cannot, then use old filename
					myFilename = file.FullName;
				}

				int exerciseID = 0; //initial import with all exercises as 0 (because exercises are not yet defined)
				int distance = 99; //mark to know at import that this have to be changed
				int temperature = 25;
				RunEncoder runEncoder = new RunEncoder(-1, p.UniqueID, Convert.ToInt32(session.Name), exerciseID,
						RunEncoder.Devices.MANUAL, distance, temperature,
						myFilename,
						Util.MakeURLrelative(Util.GetRunEncoderSessionDir(Convert.ToInt32(session.Name))),
						parsedDate, "", "");
				runEncoder.InsertSQL(true);
			}
		}

		LogB.Information("end of import_from_1_70_to_1_71()");
		LogB.PrintAllThreads = false; //TODO: remove this
	}

}
