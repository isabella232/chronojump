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
 * Xavier de Blas: 
 * http://www.xdeblas.com, http://www.deporteyciencia.com (parleblas)
 */

using System;
using System.Data;
using System.Text; //StringBuilder
using System.Threading;
using Mono.Unix;
using Gtk;
using Gdk;
using Glade;

public class Server
{
	public static string Ping(bool doInsertion, string progName, string progVersion) {
		try {
			ChronojumpServer myServer = new ChronojumpServer();
			Log.WriteLine(myServer.ConnectDatabase());
		
			int evalSID = Convert.ToInt32(SqlitePreferences.Select("evaluatorServerID"));

			ServerPing myPing = new ServerPing(evalSID, progName + " " + progVersion, Util.GetOS(), 
					Constants.IPUnknown, Util.DateParse(DateTime.Now.ToString())); //evaluator, ip, date
			//if !doIsertion nothing will be uploaded,
			//is ok for uploadPerson to know if server is online
			myPing.UniqueID = myServer.UploadPing(myPing, doInsertion);
			
			Log.WriteLine(myServer.DisConnectDatabase());

			return myPing.ToString();
		} catch {
			return Constants.ServerOffline;
		}
	}
	
	/* server session update */

	static Thread thread;

	public static SessionUploadWindow sessionUploadWin;
	[Widget] public static Gtk.Window app1;
	
	public static Session currentSession;
	public static string progName;
	public static string progVersion;

	public static bool serverSessionError;
	public static bool needUpdateServerSession;
	public static bool updatingServerSession;
	public static SessionUploadPersonData sessionUploadPersonData;
			
	public static void InitializeSessionVariables() {
		serverSessionError = false;
		needUpdateServerSession = false;
		updatingServerSession = false;
		sessionUploadPersonData = new SessionUploadPersonData();
	}
			
	public static void ThreadStart() {
		thread = new Thread(new ThreadStart(on_server_upload_session_started));
		GLib.Idle.Add (new GLib.IdleHandler (pulseGTKServer));
		thread.Start(); 
	}
	
	private static bool pulseGTKServer ()
	{
		if(! thread.IsAlive) {
			sessionUploadWin.UploadFinished();
			Log.Write("dying");
			return false;
		}

		if (serverSessionError) {
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Error uploading session to server"));
			return false;
		}

		//need to do this, if not it crashes because chronopicWin gets died by thread ending
		sessionUploadWin = SessionUploadWindow.Show(app1);
		//sessionUploadWin = SessionUploadWindow.Show();

		if(needUpdateServerSession && !updatingServerSession) {
			//prevent that FillData is called again with same data
			updatingServerSession = true;

			//fill data
			sessionUploadWin.FillData(sessionUploadPersonData);

			//not need to update until there'm more data coming from the other thread
			updatingServerSession = false;
			needUpdateServerSession = false;
		}
		
		Thread.Sleep (50);
		Log.Write(thread.ThreadState.ToString());
		return true;
	}
	
	private static void on_server_upload_session_started () 
	{
		int evalSID = Convert.ToInt32(SqlitePreferences.Select("evaluatorServerID"));

		try {	
			ChronojumpServer myServer = new ChronojumpServer();
			Log.WriteLine(myServer.ConnectDatabase());
		
			//create ServerSession based on Session currentSession
			ServerSession serverSession = new ServerSession(currentSession, evalSID, progName + " " + progVersion, 
					Util.GetOS(), Util.DateParse(DateTime.Now.ToString()), Constants.ServerSessionStates.UPLOADINGSESSION); 

			//if uploading session for first time
			if(currentSession.ServerUniqueID == Constants.ServerUndefinedID) 
			{
				//upload ServerSession
				int idAtServer = myServer.UploadSession(serverSession);

				//update session currentSession (serverUniqueID) on client database
				currentSession.ServerUniqueID = idAtServer;
				SqliteSession.UpdateServerUniqueID(currentSession.UniqueID, currentSession.ServerUniqueID);
			}

			myServer.UpdateSession(currentSession.ServerUniqueID, Constants.ServerSessionStates.UPLOADINGDATA); 

			//upload persons (updating also person.serverUniqueID locally)
			string [] myPersons = SqlitePersonSession.SelectCurrentSession(serverSession.UniqueID, true, false); //onlyIDAndName, not reversed
			Constants.UploadCodes uCode;
			foreach(string personStr in myPersons) {
				Person person = SqlitePersonSession.PersonSelect(Util.FetchID(personStr), serverSession.UniqueID); 
				//check person if exists
				if(person.ServerUniqueID != Constants.ServerUndefinedID) 
					uCode = Constants.UploadCodes.EXISTS;
				else {
					uCode = Constants.UploadCodes.OK;

					//if sport is user defined, upload it
					//and when upload the person, do it with new sportID
					Sport sport = SqliteSport.Select(person.SportID);
					if(sport.UserDefined) 
						person.SportID = myServer.UploadSport(sport);

					person = serverUploadPerson(myServer, person, serverSession.UniqueID);
				}

				//a person can be in the database for one session, 
				//but maybe now we add jumps from another session and we should add an entry at personsessionweight
				serverUploadPersonSessionIfNeeded(myServer, person.ServerUniqueID, currentSession.ServerUniqueID, person.Weight);

				//other thread updates the gui:
				sessionUploadPersonData.person = person;
				sessionUploadPersonData.personCode = uCode;

				//upload jumps
				int countU = 0;					
				int countE = 0;					
				int countS = 0;					

				string [] jumps = SqliteJump.SelectJumps(currentSession.UniqueID, person.UniqueID, "");
				foreach(string myJump in jumps) {
					string [] js = myJump.Split(new char[] {':'});
					//select jump
					Jump test = SqliteJump.SelectJumpData(Convert.ToInt32(js[1])); //uniqueID
					//fix it to server person, session keys
					test.PersonID = person.ServerUniqueID;
					test.SessionID = currentSession.ServerUniqueID;

					//if test is not simulated and has not been uploaded,
					//see if it's type is not predefined and is not in the database
					//then upload it first
					if(test.Simulated == 0) {
						//upload jumpType if is user defined and doesn't exists in server database
						//JumpType type = new JumpType(test.Type);
						JumpType type = SqliteJumpType.SelectAndReturnJumpType(test.Type);
						if( ! type.IsPredefined) {
							//Console.WriteLine("USER DEFINED TEST: " + test.Type);
							//
							//this uploads the new type, as it's user created, it will be like this
							//eg: for user defined jumpType: "supra" of evaluatorServerID: 9
							//at server will be "supra-9"
							//then two problems get solved:
							//1.- every evaluator that uploads a type will have a different name 
							//than other evaluator uploading a type that is named the same but could be different 
							//(one can think that "supra" is another thing
							//2- when the same evaluator upload some supra's, only a new type is created
					
							test.Type = myServer.UploadJumpType(type, evalSID);
					
							//test.Type in the server will have the correct name "supra-9" 
						}
					}

					//upload... (if not because of simulated or uploaded before, report also the user)
					uCode = serverUploadTest(myServer, Constants.TestTypes.JUMP, Constants.JumpTable, test);

					if(uCode == Constants.UploadCodes.OK)
						countU ++;
					else if(uCode == Constants.UploadCodes.EXISTS)
						countE ++;
					else //SIMULATED
						countS ++;
				}

				//other thread updates the gui:
				sessionUploadPersonData.jumpsU = countU;
				sessionUploadPersonData.jumpsE = countE;
				sessionUploadPersonData.jumpsS = countS;

				//upload jumpsRj
				countU = 0;					
				countE = 0;					
				countS = 0;					

				string [] jumpsRj = SqliteJumpRj.SelectJumps(currentSession.UniqueID, person.UniqueID, "");
				foreach(string myJump in jumpsRj) {
					string [] js = myJump.Split(new char[] {':'});
					//select jump
					JumpRj test = SqliteJumpRj.SelectJumpData(Constants.JumpRjTable, Convert.ToInt32(js[1])); //uniqueID
					//fix it to server person, session keys
					test.PersonID = person.ServerUniqueID;
					test.SessionID = currentSession.ServerUniqueID;
					//upload...
					uCode = serverUploadTest(myServer, Constants.TestTypes.JUMP_RJ, Constants.JumpRjTable, test);

					if(uCode == Constants.UploadCodes.OK)
						countU ++;
					else if(uCode == Constants.UploadCodes.EXISTS)
						countE ++;
					else //SIMULATED
						countS ++;
				}

				//other thread updates the gui:
				sessionUploadPersonData.jumpsRjU = countU;
				sessionUploadPersonData.jumpsRjE = countE;
				sessionUploadPersonData.jumpsRjS = countS;

				//upload runs
				countU = 0;					
				countE = 0;					
				countS = 0;					

				string [] runs = SqliteRun.SelectAllRuns(currentSession.UniqueID, person.UniqueID);
				foreach(string myRun in runs) {
					string [] js = myRun.Split(new char[] {':'});
					//select run
					Run test = SqliteRun.SelectRunData(Convert.ToInt32(js[1])); //uniqueID
					//fix it to server person, session keys
					test.PersonID = person.ServerUniqueID;
					test.SessionID = currentSession.ServerUniqueID;
					//upload...
					uCode = serverUploadTest(myServer, Constants.TestTypes.RUN, Constants.RunTable, test);

					if(uCode == Constants.UploadCodes.OK)
						countU ++;
					else if(uCode == Constants.UploadCodes.EXISTS)
						countE ++;
					else //SIMULATED
						countS ++;
				}

				//other thread updates the gui:
				sessionUploadPersonData.runsU = countU;
				sessionUploadPersonData.runsE = countE;
				sessionUploadPersonData.runsS = countS;

				//upload runs intervallic
				countU = 0;					
				countE = 0;					
				countS = 0;					

				string [] runsI = SqliteRunInterval.SelectAllRuns(currentSession.UniqueID, person.UniqueID);
				foreach(string myRun in runsI) {
					string [] js = myRun.Split(new char[] {':'});
					//select run
					RunInterval test = SqliteRunInterval.SelectRunData(Constants.RunIntervalTable, Convert.ToInt32(js[1])); //uniqueID
					//fix it to server person, session keys
					test.PersonID = person.ServerUniqueID;
					test.SessionID = currentSession.ServerUniqueID;
					//upload...
					uCode = serverUploadTest(myServer, Constants.TestTypes.RUN_I, Constants.RunIntervalTable, test);

					if(uCode == Constants.UploadCodes.OK)
						countU ++;
					else if(uCode == Constants.UploadCodes.EXISTS)
						countE ++;
					else //SIMULATED
						countS ++;
				}

				//other thread updates the gui:
				sessionUploadPersonData.runsIU = countU;
				sessionUploadPersonData.runsIE = countE;
				sessionUploadPersonData.runsIS = countS;

				//upload reaction times
				countU = 0;					
				countE = 0;					
				countS = 0;					

				string [] rts = SqliteReactionTime.SelectAllReactionTimes(currentSession.UniqueID, person.UniqueID);
				foreach(string myRt in rts) {
					string [] js = myRt.Split(new char[] {':'});
					//select rt
					ReactionTime test = SqliteReactionTime.SelectReactionTimeData(Convert.ToInt32(js[1])); //uniqueID
					//fix it to server person, session keys
					test.PersonID = person.ServerUniqueID;
					test.SessionID = currentSession.ServerUniqueID;
					//upload...
					uCode = serverUploadTest(myServer, Constants.TestTypes.RT, Constants.ReactionTimeTable, test);

					if(uCode == Constants.UploadCodes.OK)
						countU ++;
					else if(uCode == Constants.UploadCodes.EXISTS)
						countE ++;
					else //SIMULATED
						countS ++;
				}

				//other thread updates the gui:
				sessionUploadPersonData.rtsU = countU;
				sessionUploadPersonData.rtsE = countE;
				sessionUploadPersonData.rtsS = countS;

				//upload pulses
				countU = 0;					
				countE = 0;					
				countS = 0;					

				string [] pulses = SqlitePulse.SelectAllPulses(currentSession.UniqueID, person.UniqueID);
				foreach(string myPulse in pulses) {
					string [] js = myPulse.Split(new char[] {':'});
					//select pulse
					Pulse test = SqlitePulse.SelectPulseData(Convert.ToInt32(js[1])); //uniqueID
					//fix it to server person, session keys
					test.PersonID = person.ServerUniqueID;
					test.SessionID = currentSession.ServerUniqueID;
					//upload...
					uCode = serverUploadTest(myServer, Constants.TestTypes.PULSE, Constants.PulseTable, test);

					if(uCode == Constants.UploadCodes.OK)
						countU ++;
					else if(uCode == Constants.UploadCodes.EXISTS)
						countE ++;
					else //SIMULATED
						countS ++;
				}

				//other thread updates the gui:
				sessionUploadPersonData.pulsesU = countU;
				sessionUploadPersonData.pulsesE = countE;
				sessionUploadPersonData.pulsesS = countS;

				needUpdateServerSession = true;
				while(needUpdateServerSession) {
					//wait until data is printed on the other thread
				}

			}

			myServer.UpdateSession(currentSession.ServerUniqueID, Constants.ServerSessionStates.DONE); 

			Log.WriteLine(myServer.DisConnectDatabase());
		} catch {
			//other thread updates the gui:
			serverSessionError = true;
		}
	}
	
	//upload a person
	private static Person serverUploadPerson(ChronojumpServer myServer, Person person, int serverSessionID) 
	{
		int idAtServer = myServer.UploadPerson(person, serverSessionID);

		//update person (serverUniqueID) on client database
		person.ServerUniqueID = idAtServer;
		SqlitePerson.Update(person);

		return person;
	}

	private static void serverUploadPersonSessionIfNeeded(ChronojumpServer myServer, int personServerID, int sessionServerID, int weight)
	{
		myServer.UploadPersonSessionIfNeeded(personServerID, sessionServerID, weight);
	}

	//upload a test
	private static Constants.UploadCodes serverUploadTest(ChronojumpServer myServer, Constants.TestTypes type, string tableName, Event myTest) 
	{
		Constants.UploadCodes uCode;

		if(myTest.Simulated == Constants.Simulated) {
			//Test is simulated, don't upload
			uCode = Constants.UploadCodes.SIMULATED;
		} else if(myTest.Simulated > 0) {
			//Test is already uploaded, don't upload
			uCode = Constants.UploadCodes.EXISTS;
		} else {
			int idAtServer = -1;
			idAtServer = myServer.UploadTest((Event) myTest, type, tableName);
			
			//update test (simulated) on client database
			myTest.Simulated = idAtServer;
			SqliteEvent.UpdateSimulated(false, tableName, myTest.UniqueID, idAtServer);
			
			uCode = Constants.UploadCodes.OK;
		}
		return uCode;
	}

	public static void ServerUploadEvaluator () {
		try {
			ChronojumpServer myServer = new ChronojumpServer();
			Log.WriteLine(myServer.ConnectDatabase());
			
			//get Data, TODO: do it in a gui/window
			ServerEvaluator myEval = new ServerEvaluator("myName", "myEmail", "myDateBorn", Constants.CountryUndefinedID, false);
			//upload
			myEval.UniqueID = myServer.UploadEvaluator(myEval);
			//update evaluatorServerID locally
			SqlitePreferences.Update("evaluatorServerID", myEval.UniqueID.ToString(), false);

			new DialogMessage(Constants.MessageTypes.INFO, "Uploaded with ID: " + myEval.UniqueID);
			
			Log.WriteLine(myServer.DisConnectDatabase());
		} catch {
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.ServerOffline);
		}
	}
	
}
