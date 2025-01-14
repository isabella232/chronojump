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
 * Copyright (C) 2004-2021   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.IO;
using Mono.Data.Sqlite;
using System.Collections; //ArrayList


class SqlitePreferences : Sqlite
{
	public const string UnitsStr = "units";
	public const string MenuType = "menuType";
	public const string ColorBackground = "colorBackground";
	public const string ColorBackgroundOsColor = "colorBackgroundOsColor";
	public const string LogoAnimatedShow = "logoAnimatedShow";
	public const string FontsOnGraphs = "fontsOnGraphs";
	public const string RestTimeMinutes = "restTimeMinutes";
	public const string RestTimeSeconds = "restTimeSeconds";

	//person (appearance)
	public const string PersonSelectWinImages = "personSelectWinImages";

	//news stuff
	public const string NewsLanguageEs = "newsLanguageEs";
	public const string ServerNewsDatetime = "serverNewsDatetime"; // NOT stored on SQL. string of last news in server, obtained on pingAtNewsAtStart
	public const string ClientNewsDatetime = "clientNewsDatetime"; // stored on SQL. string of last news local, when user clicks on getNews(), if this is diff than server... news are downloaded and this is updated

	//contacts
	public const string JumpsFVProfileOnlyBestInWeight = "jumpsFVProfileOnlyBestInWeight";
	public const string JumpsFVProfileShowFullGraph = "jumpsFVProfileShowFullGraph";
	public const string JumpsEvolutionOnlyBestInSession = "jumpsEvolutionOnlyBestInSession";

	//encoder
	public const string EncoderCaptureInfinite = "encoderCaptureInfinite";
	public const string EncoderExerciseIDGravitatory = "encoderExerciseIDGravitatory";
	public const string EncoderExerciseIDInertial = "encoderExerciseIDInertial";
	public const string EncoderContractionGravitatory = "encoderContractionGravitatory";
	public const string EncoderContractionInertial = "encoderContractionInertial";
	public const string EncoderLateralityGravitatory = "encoderLateralityGravitatory";
	public const string EncoderLateralityInertial = "encoderLateralityInertial";
	public const string EncoderMassGravitatory = "encoderMassGravitatory";
	public const string EncoderWeightsInertial = "encoderWeightsInertial";

	public const string EncoderAutoSaveCurveBestNValue = "encoderAutoSaveCurveBestNValue";
	public const string EncoderWorkKcal = "encoderWorkKcal";
	public const string EncoderInertialGraphsX = "encoderInertialGraphsX";

	public const string EncoderRhythmActiveStr = "encoderRhythmActive";
	public const string EncoderRhythmRepsOrPhasesStr = "encoderRhythmRepsOrPhases";
	public const string EncoderRhythmRepSecondsStr = "encoderRhythmRepSeconds";
	public const string EncoderRhythmEccSecondsStr = "encoderRhythmEccSeconds";
	public const string EncoderRhythmConSecondsStr = "encoderRhythmConSeconds";
	public const string EncoderRhythmRestRepsSecondsStr = "encoderRhythmRestRepsSeconds";
	public const string EncoderRhythmRestAfterEccStr = "encoderRhythmRestAfterEcc";
	public const string EncoderRhythmRepsClusterStr = "encoderRhythmRepsCluster";
	public const string EncoderRhythmRestClustersSecondsStr = "encoderRhythmRestClustersSeconds";
	public const string EncoderCaptureMainVariableThisSetOrHistorical = "encoderCaptureMainVariableThisSetOrHistorical";
	public const string EncoderCaptureMainVariableGreaterActive = "encoderCaptureMainVariableGreaterActive";
	public const string EncoderCaptureMainVariableGreaterValue = "encoderCaptureMainVariableGreaterValue";
	public const string EncoderCaptureMainVariableLowerActive = "encoderCaptureMainVariableLowerActive";
	public const string EncoderCaptureMainVariableLowerValue = "encoderCaptureMainVariableLowerValue";
	public const string EncoderCaptureFeedbackEccon = "encoderCaptureFeedbackEccon";
	public const string EncoderCaptureInertialEccOverloadMode = "encoderCaptureInertialEccOverloadMode";
	public const string EncoderCaptureShowLoss = "encoderCaptureShowLoss";

	//forceSensor
	public const string ForceSensorCaptureWidthSeconds = "forceSensorCaptureWidthSeconds";
	public const string ForceSensorCaptureScroll = "forceSensorCaptureScroll";
	public const string ForceSensorElasticEccMinDispl = "forceSensorElasticEccMinDispl";
	public const string ForceSensorElasticConMinDispl = "forceSensorElasticConMinDispl";
	public const string ForceSensorNotElasticEccMinForce = "forceSensorNotElasticEccMinForce";
	public const string ForceSensorNotElasticConMinForce = "forceSensorNotElasticConMinForce";
	public const string ForceSensorGraphsLineWidth = "forceSensorGraphsLineWidth";
	public const string ForceSensorVariabilityMethod = "forceSensorVariabilityMethod";

	public const string ForceSensorTareDateTimeStr = "forceSensorTareDateTime";
	public const string ForceSensorTareStr = "forceSensorTare";
	public const string ForceSensorCalibrationDateTimeStr = "forceSensorCalibrationDateTime";
	public const string ForceSensorCalibrationWeightStr = "forceSensorCalibrationWeight";
	public const string ForceSensorCalibrationFactorStr = "forceSensorCalibrationFactor";
	public const string ForceSensorCaptureFeedbackActive = "forceSensorCaptureFeedbackActive";
	public const string ForceSensorCaptureFeedbackAt = "forceSensorCaptureFeedbackAt";
	public const string ForceSensorCaptureFeedbackRange = "forceSensorCaptureFeedbackRange";
	public const string ForceSensorStartEndOptimized = "forceSensorStartEndOptimized";
	public const string ForceSensorMIFDurationMode = "forceSensorMIFDurationMode";
	public const string ForceSensorMIFDurationSeconds = "forceSensorMIFDurationSeconds";
	public const string ForceSensorMIFDurationPercent = "forceSensorMIFDurationPercent";
	public const string ForceSensorAnalyzeABSliderIncrement = "forceSensorAnalyzeABSliderIncrement";
	public const string ForceSensorAnalyzeMaxAVGInWindow = "forceSensorAnalyzeMaxAVGInWindow";

	//runEncoder
	public const string RunEncoderMinAccel = "runEncoderMinAccel";
	public const string RunEncoderPPS = "runEncoderPPS";

	//advanced
	public const string ImporterPythonVersion = "importerPythonVersion";

	//session
	public const string LoadLastSessionAtStart = "loadLastSessionAtStart";
	public const string LastSessionID = "lastSessionID";
	public const string LoadLastModeAtStart = "loadLastModeAtStart";
	public const string LastMode = "lastMode";

	//export
	public const string ExportGraphWidth = "exportGraphWidth";
	public const string ExportGraphHeight = "exportGraphHeight";

	protected internal static new void createTable()
	{
		dbcmd.CommandText = 
			"CREATE TABLE " + Constants.PreferencesTable + " ( " +
			"name TEXT, " +
			"value TEXT) ";
		dbcmd.ExecuteNonQuery();
	}
	
	protected internal static void initializeTable(string databaseVersion, bool creatingBlankDatabase)
	{
		using(SqliteTransaction tr = dbcon.BeginTransaction())
		{
			using (SqliteCommand dbcmdTr = dbcon.CreateCommand())
			{
				dbcmdTr.Transaction = tr;

				Insert ("databaseVersion", databaseVersion, dbcmdTr); 

				if(UtilAll.IsWindows() || creatingBlankDatabase)
					Insert ("chronopicPort", Constants.ChronopicDefaultPortWindows, dbcmdTr);
				else
					Insert ("chronopicPort", Constants.ChronopicDefaultPortLinux, dbcmdTr);

				//appearance
				Insert ("maximized", Preferences.MaximizedTypes.NO.ToString(), dbcmdTr);
				Insert ("personWinHide", "False", dbcmdTr);
				Insert ("personPhoto", "True", dbcmdTr);
				Insert (PersonSelectWinImages, "True", dbcmdTr);
				Insert (MenuType, Preferences.MenuTypes.ALL.ToString(), dbcmdTr);
				Insert (LogoAnimatedShow, "True", dbcmdTr);
				Insert (ColorBackground, "#0e1e46", dbcmdTr);
				Insert (ColorBackgroundOsColor, "False", dbcmdTr);
				Insert (FontsOnGraphs, Preferences.FontTypes.Helvetica.ToString(), dbcmdTr);
				Insert (RestTimeMinutes, "2", dbcmdTr);
				Insert (RestTimeSeconds, "0", dbcmdTr);
				Insert (UnitsStr, Preferences.UnitsEnum.METRIC.ToString(), dbcmdTr);
				Insert (EncoderCaptureInfinite, "False", dbcmdTr);
				Insert ("encoderCaptureShowOnlyBars", "True", dbcmdTr);
				Insert ("encoderCaptureShowNRepetitions", "-1", dbcmdTr);
				Insert ("digitsNumber", "3", dbcmdTr);
				Insert ("showPower", "True", dbcmdTr);
				Insert ("showStiffness", "True", dbcmdTr);
				Insert ("showInitialSpeed", "True", dbcmdTr);
				Insert ("showAngle", "False", dbcmdTr); //for treeviewjumps
				Insert ("showQIndex", "False", dbcmdTr); //for treeviewJumps
				Insert ("showDjIndex", "False", dbcmdTr); //for treeviewJumps
				Insert ("simulated", "True", dbcmdTr);
				Insert ("weightStatsPercent", "False", dbcmdTr);
				Insert ("askDeletion", "True", dbcmdTr);
				Insert ("heightPreferred", "False", dbcmdTr);
				Insert ("metersSecondsPreferred", "True", dbcmdTr);
				Insert ("language", "", dbcmdTr); 
				Insert ("crashLogLanguage", "", dbcmdTr);
				Insert ("allowFinishRjAfterTime", "True", dbcmdTr); 
				Insert ("volumeOn", "True", dbcmdTr);

				UtilAll.OperatingSystems os = UtilAll.GetOSEnum();
				if(os == UtilAll.OperatingSystems.WINDOWS)
					Insert (Preferences.GstreamerStr, Preferences.GstreamerTypes.SYSTEMSOUNDS.ToString());
				else if(os == UtilAll.OperatingSystems.MACOSX)
					Insert (Preferences.GstreamerStr, Preferences.GstreamerTypes.FFPLAY.ToString(), dbcmdTr);
				else
					Insert (Preferences.GstreamerStr, Preferences.GstreamerTypes.GST_1_0.ToString(), dbcmdTr);

				Insert ("videoOn", "True", dbcmdTr); 
				Insert ("evaluatorServerID", "-1", dbcmdTr);
				Insert ("versionAvailable", "", dbcmdTr);
				Insert ("runSpeedStartArrival", "True", dbcmdTr);

				Insert ("runDoubleContactsMode", 
						Constants.DoubleContact.LAST.ToString(), dbcmdTr); 
				Insert ("runDoubleContactsMS", "300", dbcmdTr);
				Insert ("runIDoubleContactsMode", 
						Constants.DoubleContact.AVERAGE.ToString(), dbcmdTr); 
				Insert ("runIDoubleContactsMS", "300", dbcmdTr);

				Insert ("thresholdJumps", "50", dbcmdTr);
				Insert ("thresholdRuns", "10", dbcmdTr);
				Insert ("thresholdOther", "50", dbcmdTr);

				Random rnd = new Random();
				string machineID = rnd.Next().ToString();
				Insert ("machineID", machineID, dbcmdTr);

				Insert ("multimediaStorage", Constants.MultimediaStorage.BYSESSION.ToString(), dbcmdTr);

				//jumps
				SqlitePreferences.Insert (JumpsFVProfileOnlyBestInWeight, "True", dbcmdTr);
				SqlitePreferences.Insert (JumpsFVProfileShowFullGraph, "True", dbcmdTr);
				SqlitePreferences.Insert (JumpsEvolutionOnlyBestInSession, "False", dbcmdTr);

				//encoder
				Insert ("encoderCaptureTime", "60", dbcmdTr);
				Insert ("encoderCaptureInactivityEndTime", "3", dbcmdTr);
				Insert ("encoderCaptureMainVariable", Constants.EncoderVariablesCapture.MeanPower.ToString(), dbcmdTr);
				Insert ("encoderCaptureSecondaryVariable", Constants.EncoderVariablesCapture.RangeAbsolute.ToString(), dbcmdTr);
				Insert ("encoderCaptureSecondaryVariableShow", "True", dbcmdTr);
				Insert (EncoderCaptureFeedbackEccon, Preferences.EncoderPhasesEnum.BOTH.ToString(), dbcmdTr);
				Insert (EncoderCaptureInertialEccOverloadMode, Preferences.encoderCaptureEccOverloadModes.SHOW_LINE.ToString(), dbcmdTr);
				Insert (EncoderCaptureMainVariableThisSetOrHistorical, "True", dbcmdTr);
				Insert (EncoderCaptureMainVariableGreaterActive, "False", dbcmdTr);
				Insert (EncoderCaptureMainVariableGreaterValue, "90", dbcmdTr);
				Insert (EncoderCaptureMainVariableLowerActive, "False", dbcmdTr);
				Insert (EncoderCaptureMainVariableLowerValue, "70", dbcmdTr);
				Insert (EncoderCaptureShowLoss, "True", dbcmdTr);
				Insert ("encoderCaptureMinHeightGravitatory", "20", dbcmdTr);
				Insert ("encoderCaptureMinHeightInertial", "5", dbcmdTr);
				Insert ("encoderCaptureInertialDiscardFirstN", "3", dbcmdTr);
				Insert ("encoderCaptureCheckFullyExtended", "True", dbcmdTr);
				Insert ("encoderCaptureCheckFullyExtendedValue", "4", dbcmdTr);
				Insert ("encoderCaptureBarplotFontSize", "14", dbcmdTr);
				Insert ("encoderShowStartAndDuration", "False", dbcmdTr);
				Insert ("encoderCaptureCutByTriggers", Preferences.TriggerTypes.NO_TRIGGERS.ToString(), dbcmdTr);
				Insert ("encoderPropulsive", "True", dbcmdTr);
				Insert (EncoderWorkKcal, "True", dbcmdTr);
				Insert (EncoderInertialGraphsX, Preferences.EncoderInertialGraphsXTypes.EQUIVALENT_MASS.ToString(), dbcmdTr);
				Insert ("encoderSmoothEccCon", "0.6", dbcmdTr);
				Insert ("encoderSmoothCon", "0.7", dbcmdTr);
				Insert ("encoder1RMMethod", Constants.Encoder1RMMethod.WEIGHTED2.ToString(), dbcmdTr);

				ArrayList encoderExercises =
					SqliteEncoder.SelectEncoderExercises(true, -1, true);

				if(encoderExercises.Count > 0) {
					EncoderExercise ex = (EncoderExercise) encoderExercises[0];
					Insert (EncoderExerciseIDGravitatory, ex.uniqueID.ToString(), dbcmdTr);
					Insert (EncoderExerciseIDInertial, ex.uniqueID.ToString(), dbcmdTr);
				}
				else {
					Insert (EncoderExerciseIDGravitatory, "1", dbcmdTr);
					Insert (EncoderExerciseIDInertial, "1", dbcmdTr);
				}

				Insert (EncoderContractionGravitatory, Constants.Concentric, dbcmdTr);
				Insert (EncoderContractionInertial, Constants.EccentricConcentric, dbcmdTr);
				Insert (EncoderLateralityGravitatory, "RL", dbcmdTr);
				Insert (EncoderLateralityInertial, "RL", dbcmdTr);
				Insert (EncoderMassGravitatory, "10", dbcmdTr);
				Insert (EncoderWeightsInertial, "0", dbcmdTr);

				//encoderRhythm
				EncoderRhythm er = new EncoderRhythm();
				Insert (EncoderRhythmActiveStr, er.Active.ToString(), dbcmdTr);
				Insert (EncoderRhythmRepsOrPhasesStr, er.RepsOrPhases.ToString(), dbcmdTr);
				Insert (EncoderRhythmRepSecondsStr, Util.ConvertToPoint(er.RepSeconds), dbcmdTr);
				Insert (EncoderRhythmEccSecondsStr, Util.ConvertToPoint(er.EccSeconds), dbcmdTr);
				Insert (EncoderRhythmConSecondsStr, Util.ConvertToPoint(er.ConSeconds), dbcmdTr);
				Insert (EncoderRhythmRestRepsSecondsStr, Util.ConvertToPoint(er.RestRepsSeconds), dbcmdTr);
				Insert (EncoderRhythmRestAfterEccStr, er.RestAfterEcc.ToString(), dbcmdTr);
				Insert (EncoderRhythmRepsClusterStr, Util.ConvertToPoint(er.RepsCluster), dbcmdTr);
				Insert (EncoderRhythmRestClustersSecondsStr, Util.ConvertToPoint(er.RestClustersSeconds), dbcmdTr);

				//forceSensor
				Insert (ForceSensorCaptureWidthSeconds, "10", dbcmdTr);
				Insert (ForceSensorCaptureScroll, "True", dbcmdTr); //scroll. not zoom out
				Insert (ForceSensorElasticEccMinDispl, ".1", dbcmdTr);
				Insert (ForceSensorElasticConMinDispl, ".1", dbcmdTr);
				Insert (ForceSensorNotElasticEccMinForce, "100", dbcmdTr);
				Insert (ForceSensorNotElasticConMinForce, "100", dbcmdTr);
				Insert (ForceSensorGraphsLineWidth, "2", dbcmdTr);
				Insert (ForceSensorVariabilityMethod, Preferences.VariabilityMethodEnum.CVRMSSD.ToString(), dbcmdTr);
				Insert (ForceSensorCaptureFeedbackActive, "False", dbcmdTr);
				Insert (ForceSensorCaptureFeedbackAt, "100", dbcmdTr);
				Insert (ForceSensorCaptureFeedbackRange, "40", dbcmdTr);
				Insert (ForceSensorTareDateTimeStr, "", dbcmdTr);
				Insert (ForceSensorTareStr, "-1", dbcmdTr); //result value from sensor. Decimal is point!!
				Insert (ForceSensorCalibrationDateTimeStr, "", dbcmdTr);
				Insert (ForceSensorCalibrationWeightStr, "-1", dbcmdTr);
				Insert (ForceSensorCalibrationFactorStr, "-1", dbcmdTr); //result value from sensor. Decimal is point!!
				Insert (ForceSensorStartEndOptimized, "True", dbcmdTr);
				Insert (ForceSensorMIFDurationMode, Preferences.ForceSensorMIFDurationModes.SECONDS.ToString(), dbcmdTr);
				Insert (ForceSensorMIFDurationSeconds, "2", dbcmdTr);
				Insert (ForceSensorMIFDurationPercent, "5", dbcmdTr);
				Insert (ForceSensorAnalyzeABSliderIncrement, "1", dbcmdTr);
				Insert (ForceSensorAnalyzeMaxAVGInWindow, "1", dbcmdTr);

				//runEncoder
				Insert (RunEncoderMinAccel, "10.0", dbcmdTr);
				Insert (RunEncoderPPS, "10", dbcmdTr);

				Insert (Preferences.runEncoderAnalyzeAccel.Name,
					Preferences.runEncoderAnalyzeAccel.SqlDefaultName, dbcmdTr);
				Insert (Preferences.runEncoderAnalyzeForce.Name,
					Preferences.runEncoderAnalyzeForce.SqlDefaultName, dbcmdTr);
				Insert (Preferences.runEncoderAnalyzePower.Name,
					Preferences.runEncoderAnalyzePower.SqlDefaultName, dbcmdTr);

				//multimedia
				Insert ("videoDevice", "", dbcmdTr); //first
				Insert ("videoDevicePixelFormat", "", dbcmdTr);
				Insert ("videoDeviceResolution", "", dbcmdTr);
				Insert ("videoDeviceFramerate", "", dbcmdTr);
				Insert ("videoStopAfter", "2", dbcmdTr);

				//other
				Insert ("inertialmomentum", "0.01", dbcmdTr);
				Insert ("CSVExportDecimalSeparator", Util.GetDecimalSeparatorFromLocale(), dbcmdTr);
				Insert ("RGraphsTranslate", "True", dbcmdTr);
				Insert ("useHeightsOnJumpIndexes", "True", dbcmdTr);
				Insert ("encoderAutoSaveCurve", Constants.EncoderAutoSaveCurve.BEST.ToString(), dbcmdTr); 
				Insert (EncoderAutoSaveCurveBestNValue, "3", dbcmdTr);
				Insert ("email", "", dbcmdTr);
				Insert ("muteLogs", "False", dbcmdTr);
				Insert (ImporterPythonVersion, Preferences.pythonVersionEnum.Python3.ToString(), dbcmdTr);

				//news
				Insert (NewsLanguageEs, "False", dbcmdTr);
				Insert (ClientNewsDatetime, "", dbcmdTr);

				//session
				Insert (LoadLastSessionAtStart, "True", dbcmdTr);
				Insert (LastSessionID, "-1", dbcmdTr);
				Insert (LoadLastModeAtStart, "True", dbcmdTr);
				Insert (LastMode, Constants.Menuitem_modes.UNDEFINED.ToString(), dbcmdTr);

				//export
				Insert (ExportGraphWidth, "900", dbcmdTr);
				Insert (ExportGraphHeight, "600", dbcmdTr);

				//removed on 1.37
				//Insert ("encoderConfiguration", new EncoderConfiguration().ToStringOutput(EncoderConfiguration.Outputs.SQL), dbcmdTr);

			}
			tr.Commit();
		}
	}

	//called from some Chronojump methods
	//adds dbcmd to be used on next Insert method
	public static void Insert(string myName, string myValue)
	{
		Insert(myName, myValue, dbcmd);
	}
	//Called from initialize
	public static void Insert(string myName, string myValue, SqliteCommand mycmd)
	{
		//Sqlite.Open();
		mycmd.CommandText = "INSERT INTO " + Constants.PreferencesTable + 
			" (name, value) VALUES (\"" + 
			myName + "\", \"" + myValue + "\")" ;
		LogB.SQL(mycmd.CommandText.ToString());
		mycmd.ExecuteNonQuery();
		//Sqlite.Close();
	}

	public static void Update(string myName, bool myValue, bool dbconOpened)
	{
		Update(myName, myValue.ToString(), dbconOpened);
	}
	public static void Update(string myName, string myValue, bool dbconOpened)
	{
		if(! dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "UPDATE " + Constants.PreferencesTable +
			" SET value = \"" + myValue + 
			"\" WHERE name == \"" + myName + "\"" ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		if(! dbconOpened)
			Sqlite.Close();
	}

	//Called from most of all old Chronojump methods
	public static string Select (string myName) 
	{
		return Select(myName, false);
	}
	//Called from new methods were dbcon is opened
	public static string Select (string myName, bool dbconOpened) 
	{
		if(! dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "SELECT value FROM " + Constants.PreferencesTable + 
			" WHERE name == \"" + myName + "\"" ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		//SqliteDataReader reader;
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		string myReturn = "0";
	
		if(reader.Read()) {
			myReturn = reader[0].ToString();
		}
		reader.Close();
		
		if(! dbconOpened)
			Sqlite.Close();

		return myReturn;
	}

	//Some are sent to preferences window, others not
	//check: preferences.cs at the top
	public static Preferences SelectAll () 
	{
		Sqlite.Open();
		dbcmd.CommandText = "SELECT * FROM " + Constants.PreferencesTable; 
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		Preferences preferences = new Preferences();

		while(reader.Read()) {
			//LogB.Debug("Reading preferences");
			//LogB.Information(reader[0].ToString() + ":" + reader[1].ToString());

			if(reader[0].ToString() == "maximized")
				preferences.maximized = (Preferences.MaximizedTypes)
					Enum.Parse(typeof(Preferences.MaximizedTypes), reader[1].ToString());
			else if(reader[0].ToString() == "personWinHide")
				preferences.personWinHide = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "personPhoto")
				preferences.personPhoto = reader[1].ToString() == "True";
			else if(reader[0].ToString() == PersonSelectWinImages)
				preferences.personSelectWinImages = reader[1].ToString() == "True";
			else if(reader[0].ToString() == MenuType)
				preferences.menuType = (Preferences.MenuTypes)
					Enum.Parse(typeof(Preferences.MenuTypes), reader[1].ToString());
			else if(reader[0].ToString() == ColorBackground)
			{
				preferences.colorBackgroundString = reader[1].ToString();
				preferences.colorBackgroundIsDark = UtilGtk.ColorIsDark(reader[1].ToString());
			}
			else if(reader[0].ToString() == ColorBackgroundOsColor)
				preferences.colorBackgroundOsColor = reader[1].ToString() == "True";
			else if(reader[0].ToString() == LogoAnimatedShow)
				preferences.logoAnimatedShow = reader[1].ToString() == "True";
			else if(reader[0].ToString() == FontsOnGraphs)
				preferences.fontType = (Preferences.FontTypes)
					Enum.Parse(typeof(Preferences.FontTypes), reader[1].ToString());
			else if(reader[0].ToString() == RestTimeMinutes)
			{
				if(Util.IsNumber(reader[1].ToString(), false))
					preferences.restTimeMinutes = Convert.ToInt32(reader[1].ToString());
				else
					preferences.restTimeMinutes = 0;
			}
			else if(reader[0].ToString() == RestTimeSeconds)
			{
				if(Util.IsNumber(reader[1].ToString(), false))
					preferences.restTimeSeconds = Convert.ToInt32(reader[1].ToString());
				else
					preferences.restTimeSeconds = 0;
			}
			else if(reader[0].ToString() == NewsLanguageEs )
				preferences.newsLanguageEs = reader[1].ToString() == "True"; //bool
			else if(reader[0].ToString() == ClientNewsDatetime )
				preferences.clientNewsDatetime = reader[1].ToString();
			else if(reader[0].ToString() == UnitsStr)
				preferences.units = (Preferences.UnitsEnum)
					Enum.Parse(typeof(Preferences.UnitsEnum), reader[1].ToString());
			else if(reader[0].ToString() == EncoderCaptureInfinite)
				preferences.encoderCaptureInfinite = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "encoderCaptureShowOnlyBars")
				preferences.encoderCaptureShowOnlyBars = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "encoderCaptureShowNRepetitions")
				preferences.encoderCaptureShowNRepetitions = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == "showPower")
				preferences.showPower = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "showStiffness")
				preferences.showStiffness = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "showInitialSpeed")
				preferences.showInitialSpeed = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "showAngle")
				preferences.showAngle = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "showQIndex")
				preferences.showQIndex = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "showDjIndex")
				preferences.showDjIndex = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "weightStatsPercent")
				preferences.weightStatsPercent = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "heightPreferred")
				preferences.heightPreferred = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "metersSecondsPreferred")
				preferences.metersSecondsPreferred = reader[1].ToString() == "True";
			//jumps
			else if(reader[0].ToString() == JumpsFVProfileOnlyBestInWeight)
				preferences.jumpsFVProfileOnlyBestInWeight = reader[1].ToString() == "True";
			else if(reader[0].ToString() == JumpsFVProfileShowFullGraph)
				preferences.jumpsFVProfileShowFullGraph = reader[1].ToString() == "True";
			else if(reader[0].ToString() == JumpsEvolutionOnlyBestInSession)
				preferences.jumpsEvolutionOnlyBestInSession = reader[1].ToString() == "True";
			//encoder capture
			else if(reader[0].ToString() == "encoderCaptureTime")
				preferences.encoderCaptureTime = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == "encoderCaptureInactivityEndTime")
				preferences.encoderCaptureInactivityEndTime = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == "encoderCaptureMainVariable")
				preferences.encoderCaptureMainVariable = (Constants.EncoderVariablesCapture) 
					Enum.Parse(typeof(Constants.EncoderVariablesCapture), reader[1].ToString()); 
			else if(reader[0].ToString() == "encoderCaptureSecondaryVariable")
				preferences.encoderCaptureSecondaryVariable = (Constants.EncoderVariablesCapture)
					Enum.Parse(typeof(Constants.EncoderVariablesCapture), reader[1].ToString());
			else if(reader[0].ToString() == "encoderCaptureSecondaryVariableShow")
				preferences.encoderCaptureSecondaryVariableShow = reader[1].ToString() == "True";
			else if(reader[0].ToString() == EncoderCaptureFeedbackEccon)
				preferences.encoderCaptureFeedbackEccon = (Preferences.EncoderPhasesEnum)
					Enum.Parse(typeof(Preferences.EncoderPhasesEnum), reader[1].ToString());
			else if(reader[0].ToString() == EncoderCaptureInertialEccOverloadMode)
				preferences.encoderCaptureInertialEccOverloadMode = (Preferences.encoderCaptureEccOverloadModes)
					Enum.Parse(typeof(Preferences.encoderCaptureEccOverloadModes), reader[1].ToString());
			else if(reader[0].ToString() == EncoderCaptureMainVariableThisSetOrHistorical)
				preferences.encoderCaptureMainVariableThisSetOrHistorical = reader[1].ToString() == "True";
			else if(reader[0].ToString() == EncoderCaptureMainVariableGreaterActive)
				preferences.encoderCaptureMainVariableGreaterActive = reader[1].ToString() == "True";
			else if(reader[0].ToString() == EncoderCaptureMainVariableGreaterValue)
				preferences.encoderCaptureMainVariableGreaterValue = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == EncoderCaptureMainVariableLowerActive)
				preferences.encoderCaptureMainVariableLowerActive = reader[1].ToString() == "True";
			else if(reader[0].ToString() == EncoderCaptureMainVariableLowerValue)
				preferences.encoderCaptureMainVariableLowerValue = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == EncoderCaptureShowLoss)
				preferences.encoderCaptureShowLoss = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "encoderCaptureMinHeightGravitatory")
				preferences.encoderCaptureMinHeightGravitatory = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == "encoderCaptureMinHeightInertial")
				preferences.encoderCaptureMinHeightInertial = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == "encoderCaptureInertialDiscardFirstN")
				preferences.encoderCaptureInertialDiscardFirstN = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == "encoderCaptureCheckFullyExtended")
				preferences.encoderCaptureCheckFullyExtended = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "encoderCaptureCheckFullyExtendedValue")
				preferences.encoderCaptureCheckFullyExtendedValue = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == "encoderAutoSaveCurve")
				preferences.encoderAutoSaveCurve = (Constants.EncoderAutoSaveCurve) 
					Enum.Parse(typeof(Constants.EncoderAutoSaveCurve), reader[1].ToString()); 
			else if(reader[0].ToString() == EncoderAutoSaveCurveBestNValue)
				preferences.encoderAutoSaveCurveBestNValue = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == "encoderCaptureBarplotFontSize")
				preferences.encoderCaptureBarplotFontSize = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == "encoderShowStartAndDuration")
				preferences.encoderShowStartAndDuration = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "encoderCaptureCutByTriggers")
				preferences.encoderCaptureCutByTriggers = (Preferences.TriggerTypes)
					Enum.Parse(typeof(Preferences.TriggerTypes), reader[1].ToString());
			//encoder other
			else if(reader[0].ToString() == "encoderPropulsive")
				preferences.encoderPropulsive = reader[1].ToString() == "True";
			else if(reader[0].ToString() == EncoderWorkKcal)
				preferences.encoderWorkKcal = reader[1].ToString() == "True";
			else if(reader[0].ToString() == EncoderInertialGraphsX)
				preferences.encoderInertialGraphsX = (Preferences.EncoderInertialGraphsXTypes)
					Enum.Parse(typeof(Preferences.EncoderInertialGraphsXTypes), reader[1].ToString());
			else if(reader[0].ToString() == "encoderSmoothCon")
				preferences.encoderSmoothCon = Convert.ToDouble(
						Util.ChangeDecimalSeparator(reader[1].ToString()));
			else if(reader[0].ToString() == "encoder1RMMethod")
				preferences.encoder1RMMethod = (Constants.Encoder1RMMethod) 
					Enum.Parse(typeof(Constants.Encoder1RMMethod), reader[1].ToString()); 

			//encoder rhythm
			else if(reader[0].ToString() == EncoderRhythmActiveStr)
				preferences.encoderRhythmActive = reader[1].ToString() == "True"; //bool
			else if(reader[0].ToString() == EncoderRhythmRepsOrPhasesStr)
				preferences.encoderRhythmRepsOrPhases = reader[1].ToString() == "True"; //bool
			else if(reader[0].ToString() == EncoderRhythmRepSecondsStr)
				preferences.encoderRhythmRepSeconds = Convert.ToDouble(
						Util.ChangeDecimalSeparator(reader[1].ToString()));
			else if(reader[0].ToString() == EncoderRhythmEccSecondsStr)
				preferences.encoderRhythmEccSeconds = Convert.ToDouble(
						Util.ChangeDecimalSeparator(reader[1].ToString()));
			else if(reader[0].ToString() == EncoderRhythmConSecondsStr)
				preferences.encoderRhythmConSeconds = Convert.ToDouble(
						Util.ChangeDecimalSeparator(reader[1].ToString()));
			else if(reader[0].ToString() == EncoderRhythmRestRepsSecondsStr)
				preferences.encoderRhythmRestRepsSeconds = Convert.ToDouble(
						Util.ChangeDecimalSeparator(reader[1].ToString()));
			else if(reader[0].ToString() == EncoderRhythmRestAfterEccStr)
				preferences.encoderRhythmRestAfterEcc = reader[1].ToString() == "True"; //bool
			else if(reader[0].ToString() == EncoderRhythmRepsClusterStr)
				preferences.encoderRhythmRepsCluster = Convert.ToInt32(reader[1].ToString()); //int
			else if(reader[0].ToString() == EncoderRhythmRestClustersSecondsStr)
				preferences.encoderRhythmRestClustersSeconds = Convert.ToDouble(
						Util.ChangeDecimalSeparator(reader[1].ToString()));

			//video... other
			else if(reader[0].ToString() == "videoDevice")
				preferences.videoDevice = reader[1].ToString();
			else if(reader[0].ToString() == "videoDevicePixelFormat")
				preferences.videoDevicePixelFormat = reader[1].ToString();
			else if(reader[0].ToString() == "videoDeviceResolution")
				preferences.videoDeviceResolution = reader[1].ToString();
			else if(reader[0].ToString() == "videoDeviceFramerate")
				preferences.videoDeviceFramerate = reader[1].ToString(); //if it is decimal will be always a '.' as needed by ffmpeg
			else if(reader[0].ToString() == "videoStopAfter")
				preferences.videoStopAfter = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == "CSVExportDecimalSeparator")
				preferences.CSVExportDecimalSeparator = reader[1].ToString();
			else if(reader[0].ToString() == "language")
				preferences.language = reader[1].ToString();
			else if(reader[0].ToString() == "crashLogLanguage")
				preferences.crashLogLanguage = reader[1].ToString();
			else if(reader[0].ToString() == "RGraphsTranslate")
				preferences.RGraphsTranslate = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "useHeightsOnJumpIndexes")
				preferences.useHeightsOnJumpIndexes = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "allowFinishRjAfterTime")
				preferences.allowFinishRjAfterTime = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "volumeOn")
				preferences.volumeOn = reader[1].ToString() == "True";
			else if(reader[0].ToString() == Preferences.GstreamerStr)
			{
				preferences.gstreamer = (Preferences.GstreamerTypes)
					Enum.Parse(typeof(Preferences.GstreamerTypes), reader[1].ToString());

				//on 2.0 gstreamer is disabled on mac
				if(UtilAll.GetOSEnum() == UtilAll.OperatingSystems.MACOSX && (
						preferences.gstreamer == Preferences.GstreamerTypes.GST_0_1 ||
						preferences.gstreamer == Preferences.GstreamerTypes.GST_1_0 ) )
					preferences.gstreamer = Preferences.GstreamerTypes.FFPLAY;
			}
			else if(reader[0].ToString() == "videoOn")
				preferences.videoOn = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "evaluatorServerID")
				preferences.evaluatorServerID = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == "versionAvailable")
				preferences.versionAvailable = reader[1].ToString();
			else if(reader[0].ToString() == "runSpeedStartArrival")
				preferences.runSpeedStartArrival = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "runDoubleContactsMode")
				preferences.runDoubleContactsMode = (Constants.DoubleContact) 
					Enum.Parse(typeof(Constants.DoubleContact), reader[1].ToString()); 
			else if(reader[0].ToString() == "runDoubleContactsMS")
				preferences.runDoubleContactsMS = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == "runIDoubleContactsMode")
				preferences.runIDoubleContactsMode = (Constants.DoubleContact) 
					Enum.Parse(typeof(Constants.DoubleContact), reader[1].ToString()); 
			else if(reader[0].ToString() == "runIDoubleContactsMS")
				preferences.runIDoubleContactsMS = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == "thresholdJumps")
				preferences.thresholdJumps = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == "thresholdRuns")
				preferences.thresholdRuns = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == "thresholdOther")
				preferences.thresholdOther = Convert.ToInt32(reader[1].ToString());

			//force sensor capture
			else if(reader[0].ToString() == ForceSensorCaptureWidthSeconds)
				preferences.forceSensorCaptureWidthSeconds = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == ForceSensorCaptureScroll)
				preferences.forceSensorCaptureScroll = reader[1].ToString() == "True";
			else if(reader[0].ToString() == ForceSensorElasticEccMinDispl)
				preferences.forceSensorElasticEccMinDispl = Convert.ToDouble(
						Util.ChangeDecimalSeparator(reader[1].ToString()));
			else if(reader[0].ToString() == ForceSensorElasticConMinDispl)
				preferences.forceSensorElasticConMinDispl = Convert.ToDouble(
						Util.ChangeDecimalSeparator(reader[1].ToString()));
			else if(reader[0].ToString() == ForceSensorNotElasticEccMinForce)
				preferences.forceSensorNotElasticEccMinForce = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == ForceSensorNotElasticConMinForce)
				preferences.forceSensorNotElasticConMinForce = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == ForceSensorGraphsLineWidth)
				preferences.forceSensorGraphsLineWidth = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == ForceSensorVariabilityMethod)
				preferences.forceSensorVariabilityMethod = (Preferences.VariabilityMethodEnum)
					Enum.Parse(typeof(Preferences.VariabilityMethodEnum), reader[1].ToString());
			else if(reader[0].ToString() == ForceSensorCaptureFeedbackActive)
				preferences.forceSensorCaptureFeedbackActive = reader[1].ToString() == "True";
			else if(reader[0].ToString() == ForceSensorCaptureFeedbackAt)
				preferences.forceSensorCaptureFeedbackAt = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == ForceSensorCaptureFeedbackRange)
				preferences.forceSensorCaptureFeedbackRange = Convert.ToInt32(reader[1].ToString());

			//force sensor tare
			else if(reader[0].ToString() == ForceSensorTareDateTimeStr)
				preferences.forceSensorTareDateTime = reader[1].ToString();
			else if(reader[0].ToString() == ForceSensorTareStr)
				preferences.forceSensorTare = Convert.ToDouble(
						Util.ChangeDecimalSeparator(reader[1].ToString()));

			//force sensor calibrate
			else if(reader[0].ToString() == ForceSensorCalibrationDateTimeStr)
				preferences.forceSensorCalibrationDateTime = reader[1].ToString();
			else if(reader[0].ToString() == ForceSensorCalibrationWeightStr)
				preferences.forceSensorCalibrationWeight = Convert.ToDouble(
						Util.ChangeDecimalSeparator(reader[1].ToString()));
			else if(reader[0].ToString() == ForceSensorCalibrationFactorStr)
				preferences.forceSensorCalibrationFactor = Convert.ToDouble(
						Util.ChangeDecimalSeparator(reader[1].ToString()));
			//force sensor MIF
			else if(reader[0].ToString() == ForceSensorStartEndOptimized)
				preferences.forceSensorStartEndOptimized = reader[1].ToString() == "True";
			else if(reader[0].ToString() == ForceSensorMIFDurationMode)
				preferences.forceSensorMIFDurationMode = (Preferences.ForceSensorMIFDurationModes)
					Enum.Parse(typeof(Preferences.ForceSensorMIFDurationModes), reader[1].ToString());
			else if(reader[0].ToString() == ForceSensorMIFDurationSeconds)
				preferences.forceSensorMIFDurationSeconds = Convert.ToDouble(
						Util.ChangeDecimalSeparator(reader[1].ToString()));
			else if(reader[0].ToString() == ForceSensorMIFDurationPercent)
				preferences.forceSensorMIFDurationPercent = Convert.ToInt32(
						reader[1].ToString());

			else if(reader[0].ToString() == ForceSensorAnalyzeABSliderIncrement)
				preferences.forceSensorAnalyzeABSliderIncrement = Convert.ToDouble(
						Util.ChangeDecimalSeparator(reader[1].ToString()));

			else if(reader[0].ToString() == ForceSensorAnalyzeMaxAVGInWindow)
				preferences.forceSensorAnalyzeMaxAVGInWindow = Convert.ToDouble(
						Util.ChangeDecimalSeparator(reader[1].ToString()));

			//runEncoder
			else if(reader[0].ToString() == RunEncoderMinAccel)
				preferences.runEncoderMinAccel = Convert.ToDouble(
						Util.ChangeDecimalSeparator(reader[1].ToString()));
			else if(reader[0].ToString() == RunEncoderPPS)
				preferences.runEncoderPPS = Convert.ToInt32(reader[1].ToString());

			else if(reader[0].ToString() == Preferences.runEncoderAnalyzeAccel.Name)
				Preferences.runEncoderAnalyzeAccel.SetCurrentFromSQL(reader[1].ToString());
			else if(reader[0].ToString() == Preferences.runEncoderAnalyzeForce.Name)
				Preferences.runEncoderAnalyzeForce.SetCurrentFromSQL(reader[1].ToString());
			else if(reader[0].ToString() == Preferences.runEncoderAnalyzePower.Name)
				Preferences.runEncoderAnalyzePower.SetCurrentFromSQL(reader[1].ToString());

			//advanced tab
			else if(reader[0].ToString() == "digitsNumber")
				preferences.digitsNumber = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == "askDeletion")
				preferences.askDeletion = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "muteLogs")
				preferences.muteLogs = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "machineID")
				preferences.machineID = reader[1].ToString();
			else if(reader[0].ToString() == "multimediaStorage")
				preferences.multimediaStorage = (Constants.MultimediaStorage) 
					Enum.Parse(typeof(Constants.MultimediaStorage), reader[1].ToString()); 
			else if(reader[0].ToString() == ImporterPythonVersion)
				preferences.importerPythonVersion = (Preferences.pythonVersionEnum)
					Enum.Parse(typeof(Preferences.pythonVersionEnum), reader[1].ToString());
			else if(reader[0].ToString() == "databaseVersion")
				preferences.databaseVersion = reader[1].ToString();

			//session
			else if(reader[0].ToString() == LoadLastSessionAtStart)
				preferences.loadLastSessionAtStart = reader[1].ToString() == "True";
			else if(reader[0].ToString() == LastSessionID)
			{
				if(Util.IsNumber(reader[1].ToString(), false))
					preferences.lastSessionID = Convert.ToInt32(reader[1].ToString());
				else
					preferences.lastSessionID = -1;
			}
			else if(reader[0].ToString() == LoadLastModeAtStart)
				preferences.loadLastModeAtStart = reader[1].ToString() == "True";
			else if(reader[0].ToString() == LastMode)
				preferences.lastMode = (Constants.Menuitem_modes)
					Enum.Parse(typeof(Constants.Menuitem_modes), reader[1].ToString());

			//export
			else if(reader[0].ToString() == ExportGraphWidth)
				preferences.exportGraphWidth = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == ExportGraphHeight)
				preferences.exportGraphHeight = Convert.ToInt32(reader[1].ToString());
		}

		reader.Close();
		Sqlite.Close();

		return preferences;
	}
}

