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
 * Copyright (C) 2004-2009   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;

using System.Threading;
using System.IO.Ports;
using Mono.Unix;

public class PulseExecute : EventExecute
{
	double fixedPulse;
	int totalPulsesNum;

	string timesString;
	int tracks;
	double contactTime;

	//better as private and don't inherit, don't know why
	//protected Chronopic cp;
	private Chronopic cp;

	//used by the updateTimeProgressBar for display its time information
	//explained also at updateTimeProgressBar() 
	protected enum pulsePhases {
		WAIT_FIRST_EVENT, //we are outside at beginning
			DOING, 
			DONE
	}
	protected pulsePhases pulsePhase;
		
	
	//used on treeviewPulse
	public PulseExecute() {
	}

	//execution
	public PulseExecute(EventExecuteWindow eventExecuteWin, int personID, string personName, int sessionID, string type, double fixedPulse, int totalPulsesNum,  
			Chronopic cp, Gtk.Statusbar appbar, Gtk.Window app, int pDN, bool volumeOn)
	{
		this.eventExecuteWin = eventExecuteWin;
		this.personID = personID;
		this.personName = personName;
		this.sessionID = sessionID;
		this.type = type;
		this.fixedPulse = fixedPulse;
		this.totalPulsesNum = totalPulsesNum;
		
	
		this.cp = cp;
		this.appbar = appbar;
		this.app = app;

		this.pDN = pDN;
		this.volumeOn = volumeOn;
	
		fakeButtonFinished = new Gtk.Button();

		simulated = false;
		
		needUpdateEventProgressBar = false;
		needUpdateGraph = false;
		
		//initialize eventDone as a Pulse	
		eventDone = new Pulse();
	}
	
	public override void SimulateInitValues(Random randSent)
	{
		Log.WriteLine("From execute/pulse.cs");

		rand = randSent; //we send the random, because if we create here, the values will be the same for each nbew instance
		simulated = true;
		simulatedTimeAccumulatedBefore = 0;
		simulatedTimeLast = 0;
		simulatedContactTimeMin = 0; //seconds
		simulatedContactTimeMax = .2; //seconds ('0' gives problems)
		simulatedFlightTimeMin = 0.8; //seconds
		simulatedFlightTimeMax = 1.3; //seconds

		//values of simulation will be the contactTime at the first time 
		//(next flight, contact, next flight...)
		//tc+tv will be registered
		simulatedCurrentTimeIntervalsAreContact = true;
	}
	
	public override void Manage()
	{
		bool success = false;
		
		
		if (simulated) 
			platformState = Chronopic.Plataforma.OFF;
		else
			platformState = chronopicInitialValue(cp);
		

		//you should start OFF (outside) the platform 
		//we record always de TC+TF (or time between we pulse platform and we pulse again)
		//we don't care about the time between the get in and the get out the platform
		if (platformState==Chronopic.Plataforma.ON) {
			string myMessage = Catalog.GetString("You are IN, please leave the platform, prepare for start, and press the 'accept' button!!");

			ConfirmWindow confirmWin;		
			confirmWin = ConfirmWindow.Show(myMessage, "", "");
			Util.PlaySound(Constants.SoundTypes.BAD, volumeOn);

			//we call again this function
			confirmWin.Button_accept.Clicked += new EventHandler(callAgainManage);
		} else {
			appbar.Push( 1, Catalog.GetString("You are OUT, start when prepared!!") );
			Util.PlaySound(Constants.SoundTypes.CAN_START, volumeOn);

			loggedState = States.OFF;

			success = true;
		}

		if(success) {
			//initialize variables
			timesString = "";
			tracks = 0;

			//prepare jump for being cancelled if desired
			cancel = false;
			totallyCancelled = false;

			//prepare jump for being finished earlier if desired
			finish = false;
			totallyFinished= false;

			//mark we haven't started
			pulsePhase = pulsePhases.WAIT_FIRST_EVENT;
			
			//in simulated mode, make the event start just when we arrive to waitEvent at the first time
			//mark now that we have landed:
			if (simulated)
				platformState = Chronopic.Plataforma.ON;
			
			//start thread
			thread = new Thread(new ThreadStart(waitEvent));
			GLib.Idle.Add (new GLib.IdleHandler (PulseGTK));
			thread.Start(); 
		}
	}
	
	protected override void waitEvent ()
	{
			double timestamp = 0;
			bool success = false;
			string equal = "";

			bool ok;

			do {
				if(simulated) 
					ok = true;
				else 
					ok = cp.Read_event(out timestamp, out platformState);


				if (ok && !cancel && !finish) {
					if (platformState == Chronopic.Plataforma.ON && loggedState == States.OFF) {
						//has arrived

						//if we arrive to the platform for the first time, don't record anything
						if(pulsePhase == pulsePhases.WAIT_FIRST_EVENT) {
							pulsePhase = pulsePhases.DOING;
							//pulse starts
							initializeTimer();
						} else {
							//is not the first pulse
							if(totalPulsesNum == -1) {
								//if is "unlimited", 
								//then play with the progress bar until finish button is pressed
								if(simulated)
									timestamp = simulatedTimeLast * 1000; //conversion to milliseconds
								if(timesString.Length > 0) { equal = "="; }
								timesString = timesString + equal + (contactTime/1000.0 + timestamp/1000.0).ToString();
								tracks ++;	

								//update event progressbar
								//eventExecuteWin.ProgressBarEventOrTimePreExecution(
								updateProgressBar= new UpdateProgressBar (
										true, //isEvent
										false, //activityMode
										tracks
										);  

								needUpdateEventProgressBar = true;

								//update graph
								//eventExecuteWin.PreparePulseGraph(timestamp/1000.0, timesString);
								prepareEventGraphPulse = new PrepareEventGraphPulse(timestamp/1000.0, timesString);
								needUpdateGraphType = eventType.PULSE;
								needUpdateGraph = true;

								//put button_finish as sensitive when first jump is done (there's something recordable)
								if(tracks == 1)
									needSensitiveButtonFinish = true;
							}
							else {
								//is not the first pulse, and it's limited by tracks (ticks)
								tracks ++;	

								if(simulated)
									timestamp = simulatedTimeLast * 1000; //conversion to milliseconds
								if(timesString.Length > 0) { equal = "="; }
								timesString = timesString + equal + (contactTime/1000.0 + timestamp/1000.0).ToString();

								if(tracks >= totalPulsesNum) 
								{
									//finished
									write();
									success = true;
									pulsePhase = pulsePhases.DONE;
								}

								//update event progressbar
								//eventExecuteWin.ProgressBarEventOrTimePreExecution(
								updateProgressBar= new UpdateProgressBar (
										true, //isEvent
										true, //PercentageMode
										tracks
										);  
								needUpdateEventProgressBar = true;

								//update graph
								//eventExecuteWin.PreparePulseGraph(timestamp/1000.0, timesString);
								prepareEventGraphPulse = new PrepareEventGraphPulse(timestamp/1000.0, timesString);
								needUpdateGraphType = eventType.PULSE;
								needUpdateGraph = true;

								//put button_finish as sensitive when first jump is done (there's something recordable)
								if(tracks == 1)
									needSensitiveButtonFinish = true;
							}
						}

						//change the automata state
						loggedState = States.ON;
					}
					else if (platformState == Chronopic.Plataforma.OFF && loggedState == States.ON) {
						//it's out, was inside (= has abandoned platform)
						
						if(simulated)
							timestamp = simulatedTimeLast * 1000; //conversion to milliseconds
						contactTime = timestamp;

						//change the automata state
						loggedState = States.OFF;
					}
				}
			} while ( ! success && ! cancel && ! finish );

			if (finish) {
				write();
				pulsePhase = pulsePhases.DONE;
				totallyFinished= true;
			}
			if(cancel || finish) {
				//event will be raised, and managed in chronojump.cs
				fakeButtonFinished.Click();
			
				totallyCancelled = true;
			}
	}

	//now pulses are not thought for being able to finish by time
	protected override bool shouldFinishByTime() {
			return false;
	}
	
	protected override void updateProgressBarForFinish() {
		eventExecuteWin.ProgressBarEventOrTimePreExecution(
				false, //isEvent false: time
				true, //percentageMode: it has finished, show bar at 100%
				totalPulsesNum
				);  
	}

	protected override void updateTimeProgressBar() {
		double myTimeValue = 0;
		if(pulsePhase == pulsePhases.WAIT_FIRST_EVENT) 
			myTimeValue = -1;
		else
			myTimeValue = timerCount;

		//if event has end, chronojump will overwrite label_time_value
			
		//limited by tracks, but has no finished
		eventExecuteWin.ProgressBarEventOrTimePreExecution(
				false, //isEvent false: time
				false, //activiyMode
				myTimeValue
				); 
	}


	protected override void write()
	{
		int totalPulsesNum = 0;

		totalPulsesNum = Util.GetNumberOfJumps(timesString, false);

		uniqueID = SqlitePulse.Insert(false, Constants.PulseTable, "NULL", personID, sessionID, type, 
				fixedPulse, totalPulsesNum, timesString, 
				"", Util.BoolToNegativeInt(simulated) //description
				);

		//define the created object
		eventDone = new Pulse(uniqueID, personID, sessionID, type, fixedPulse, totalPulsesNum, timesString, "", Util.BoolToNegativeInt(simulated)); 
		
		string myStringPush =   Catalog.GetString("Last pulse") + ": " + personName + " " + type ;
		if(simulated)
			appbar.Push(1, Constants.SimulatedMessage);
		else
			appbar.Push( 1, myStringPush );
				
	
		//event will be raised, and managed in chronojump.cs
		fakeButtonFinished.Click();
		
		//eventExecuteWin.PreparePulseGraph(Util.GetLast(timesString), timesString);
		prepareEventGraphPulse = new PrepareEventGraphPulse(Util.GetLast(timesString), timesString);
		needUpdateGraphType = eventType.PULSE;
		needUpdateGraph = true;
		//eventExecuteWin.EventEnded();
		needEndEvent = true; //used for hiding some buttons on eventWindow, and also for updateTimeProgressBar here
	}

		
	~PulseExecute() {}
}

