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
using Gtk;
using Glade;
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using Mono.Unix;

//using System.Threading;


using Gdk; //for the EventMask



public class EventExecuteWindow 
{
	[Widget] Gtk.Window event_execute;
	
	[Widget] Gtk.Label label_person;
	[Widget] Gtk.Label label_event_type;
	[Widget] Gtk.Label label_phases_name;
	[Widget] Gtk.Label label_simulated;
	[Widget] Gtk.Image image_simulated_l;
	[Widget] Gtk.Image image_simulated_r;
	
	[Widget] Gtk.ProgressBar progressbar_event;
	[Widget] Gtk.ProgressBar progressbar_time;
	

	//currently gtk-sharp cannot display a label in a progressBar in activity mode (Pulse() not Fraction)
	//then we show the value in a label:
	[Widget] Gtk.Label label_event_value;
	[Widget] Gtk.Label label_time_value;
	
	[Widget] Gtk.Button button_cancel;
	[Widget] Gtk.Button button_finish;
	[Widget] Gtk.Button button_update;
	[Widget] Gtk.Button button_close;

	
	[Widget] Gtk.Table table_jump_simple;
	[Widget] Gtk.Table table_jump_reactive;
	[Widget] Gtk.Table table_run_simple;
	[Widget] Gtk.Table table_run_interval;
	[Widget] Gtk.Table table_pulse;
	[Widget] Gtk.Table table_reaction_time;
	
	[Widget] Gtk.Table table_jump_simple_values;
	[Widget] Gtk.Table table_jump_reactive_values;
	[Widget] Gtk.Table table_run_simple_values;
	[Widget] Gtk.Table table_run_interval_values;
	[Widget] Gtk.Table table_pulse_values;
	[Widget] Gtk.Table table_reaction_time_values;

	[Widget] Gtk.HBox hbox_jump_simple_titles;
	[Widget] Gtk.HBox hbox_run_simple_titles;
	[Widget] Gtk.HBox hbox_reaction_time_titles;

	//for the color change in the background of the cell label
	[Widget] Gtk.EventBox eventbox_jump_simple_tc;
	[Widget] Gtk.EventBox eventbox_jump_simple_tf;
	[Widget] Gtk.EventBox eventbox_jump_reactive_tc;
	[Widget] Gtk.EventBox eventbox_jump_reactive_tf;
	//[Widget] Gtk.EventBox eventbox_jump_reactive_tf_tc;
	[Widget] Gtk.EventBox eventbox_run_simple_time;
	[Widget] Gtk.EventBox eventbox_run_simple_speed;
	[Widget] Gtk.EventBox eventbox_run_interval_time;
	[Widget] Gtk.EventBox eventbox_run_interval_speed;
	[Widget] Gtk.EventBox eventbox_pulse_time;
	[Widget] Gtk.EventBox eventbox_reaction_time_time;

	
	[Widget] Gtk.Label label_jump_simple_tc_now;
	[Widget] Gtk.Label label_jump_simple_tc_person;
	[Widget] Gtk.Label label_jump_simple_tc_session;
	[Widget] Gtk.Label label_jump_simple_tf_now;
	[Widget] Gtk.Label label_jump_simple_tf_person;
	[Widget] Gtk.Label label_jump_simple_tf_session;

	[Widget] Gtk.Label label_jump_reactive_tc_now;
	[Widget] Gtk.Label label_jump_reactive_tc_avg;
	[Widget] Gtk.Label label_jump_reactive_tf_now;
	[Widget] Gtk.Label label_jump_reactive_tf_avg;
	[Widget] Gtk.Label label_jump_reactive_tf_tc_now;
	[Widget] Gtk.Label label_jump_reactive_tf_tc_avg;

	[Widget] Gtk.Label label_run_simple_time_now;
	[Widget] Gtk.Label label_run_simple_time_person;
	[Widget] Gtk.Label label_run_simple_time_session;
	[Widget] Gtk.Label label_run_simple_speed_now;
	[Widget] Gtk.Label label_run_simple_speed_person;
	[Widget] Gtk.Label label_run_simple_speed_session;

	[Widget] Gtk.Label label_run_interval_time_now;
	[Widget] Gtk.Label label_run_interval_time_avg;
	[Widget] Gtk.Label label_run_interval_speed_now;
	[Widget] Gtk.Label label_run_interval_speed_avg;
	
	[Widget] Gtk.Label label_pulse_now;
	[Widget] Gtk.Label label_pulse_avg;

	[Widget] Gtk.Label label_reaction_time_now;
	[Widget] Gtk.Label label_reaction_time_person;
	[Widget] Gtk.Label label_reaction_time_session;

	[Widget] Gtk.Image image_jump_reactive_tf_good;
	[Widget] Gtk.Image image_jump_reactive_tf_bad;
	[Widget] Gtk.Image image_jump_reactive_tc_good;
	[Widget] Gtk.Image image_jump_reactive_tc_bad;
	[Widget] Gtk.Image image_jump_reactive_tf_tc_good;
	[Widget] Gtk.Image image_jump_reactive_tf_tc_bad;

	[Widget] Gtk.Image image_run_interval_time_good;
	[Widget] Gtk.Image image_run_interval_time_bad;
	
	[Widget] Gtk.DrawingArea drawingarea;
	static Gdk.Pixmap pixmap = null;


	int personID;	
	int sessionID;	
	string tableName;	
	string eventType;	
	private string lastEventWas;
	
	int pDN;
	double limit;
	
	private enum phasesGraph {
		UNSTARTED, DOING, DONE
	}
	private phasesGraph graphProgress;
	
	int radio = 8; 		//radious of the circles
	int arcSystemCorrection = 0; //on Windows circles are paint just one pixel left, fix it
	int rightMargin = 30; 	//at the right we write text (on windows we change later)

	/*
	 * when click on destroy window, delete event is raised
	 * if event has ended, then it should normally close the window
	 * if has not ended, then it should cancel it before.
	 * on 0.7.5.2 and before, we always cancel, 
	 * and this produces and endless loop when event has ended, because there's nothing to cancel
	 */
	bool eventHasEnded;

	
	//for writing text
	Pango.Layout layout;

	static EventGraphConfigureWindow eventGraphConfigureWin;
	
	static EventExecuteWindow EventExecuteWindowBox;
		
	EventExecuteWindow () {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "event_execute", null);
		gladeXML.Autoconnect(this);
		
		//put an icon to window
		UtilGtk.IconWindow(event_execute);

		//this hides it when it's creating (hiding and showing tables stuff)
		//then user doesn't see a moving/changing creation window
		event_execute.Hide ();
		
		if(Util.IsWindows()) {
			rightMargin = 50;
			arcSystemCorrection = 1;
		}

		configureColors();
	}

	static public EventExecuteWindow Show (
			string windowTitle, string phasesName, int personID, string personName, int sessionID, 
			string tableName, string eventType, int pDN, double limit, bool simulated)
	{
		if (EventExecuteWindowBox == null) {
			EventExecuteWindowBox = new EventExecuteWindow (); 
		}

		//create the properties window if doesnt' exists, but do not show
		if(eventGraphConfigureWin == null)
			eventGraphConfigureWin = EventGraphConfigureWindow.Show(false);

		
		EventExecuteWindowBox.hideAllTables();
		EventExecuteWindowBox.hideImages();
	
		EventExecuteWindowBox.initializeVariables (
				windowTitle, phasesName, personID, personName, sessionID, 
				tableName, eventType, pDN, limit, simulated);

		EventExecuteWindowBox.event_execute.Show ();

		return EventExecuteWindowBox;
	}

	void initializeVariables (
			string windowTitle, string phasesName, int personID, string personName, int sessionID,
			string tableName, string eventType, int pDN, double limit, bool simulated) 
	{
		event_execute.Title = windowTitle;
		this.label_phases_name.Text = phasesName; 	//"Jumps" (rjInterval), "Runs" (runInterval), "Ticks" (pulses), 
								//"Phases" (simple jumps, dj, simple runs)
		this.personID = personID;
		this.label_person.Text = personName;
		this.tableName = tableName;
		this.sessionID = sessionID;

		this.eventType = eventType;
		this.label_event_type.Text = this.eventType;
		this.pDN = pDN;
		this.limit = limit;

		if(simulated) {
			label_simulated.Show();
			image_simulated_l.Show();
			image_simulated_r.Show();
		}
		else {
			label_simulated.Hide();
			image_simulated_l.Hide();
			image_simulated_r.Hide();
		}
			

		//finish not sensitive for all events. 
		//Later reactive, interval and pulse will sensitive it when a subevent is done
		button_finish.Sensitive = false;

		if(tableName == "jump")
			showJumpSimpleLabels();
		else if (tableName == "jumpRj")
			showJumpReactiveLabels();
		else if (tableName == "run")
			showRunSimpleLabels();
		else if (tableName == "runInterval")
			showRunIntervalLabels();
		else if (tableName == "pulse")
			showPulseLabels();
		else if (tableName == "reactionTime")
			showReactionTimeLabels(); 

		//for the "update" button
		lastEventWas = tableName;
			
		
		button_cancel.Sensitive = true;
		button_close.Sensitive = false;

		clearDrawingArea();
		clearProgressBars();

	
		eventbox_jump_simple_tc.ModifyBg(Gtk.StateType.Normal, new Gdk.Color( 255, 0, 0));
		eventbox_jump_simple_tf.ModifyBg(Gtk.StateType.Normal, new Gdk.Color( 0, 0, 255));
		eventbox_jump_reactive_tc.ModifyBg(Gtk.StateType.Normal, new Gdk.Color( 255, 0, 0));
		eventbox_jump_reactive_tf.ModifyBg(Gtk.StateType.Normal, new Gdk.Color( 0, 0, 255));
		eventbox_run_simple_time.ModifyBg(Gtk.StateType.Normal, new Gdk.Color( 255, 0, 0));
		eventbox_run_simple_speed.ModifyBg(Gtk.StateType.Normal, new Gdk.Color( 0, 0, 255));
		eventbox_run_interval_time.ModifyBg(Gtk.StateType.Normal, new Gdk.Color( 255, 0, 0));
		eventbox_run_interval_speed.ModifyBg(Gtk.StateType.Normal, new Gdk.Color( 0, 0, 255));
		eventbox_pulse_time.ModifyBg(Gtk.StateType.Normal, new Gdk.Color( 0, 0, 255)); //only one serie in pulse, leave blue
		
		graphProgress = phasesGraph.UNSTARTED; 

		layout = new Pango.Layout (drawingarea.PangoContext);
		layout.FontDescription = Pango.FontDescription.FromString ("Courier 7");

		putNonStandardIcons();
		
		eventHasEnded = false;
	}
	
	private void putNonStandardIcons() {
		Pixbuf pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_bell_green.png");
		image_jump_reactive_tf_good.Pixbuf = pixbuf;
		image_jump_reactive_tc_good.Pixbuf = pixbuf;
		image_jump_reactive_tf_tc_good.Pixbuf = pixbuf;
		image_run_interval_time_good.Pixbuf = pixbuf;
		
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_bell_red.png");
		image_jump_reactive_tf_bad.Pixbuf = pixbuf;
		image_jump_reactive_tc_bad.Pixbuf = pixbuf;
		image_jump_reactive_tf_tc_bad.Pixbuf = pixbuf;
		image_run_interval_time_bad.Pixbuf = pixbuf;
	}

	private void hideImages() {
		image_jump_reactive_tf_good.Hide();
		image_jump_reactive_tf_bad.Hide();
		image_jump_reactive_tc_good.Hide();
		image_jump_reactive_tc_bad.Hide();
		image_jump_reactive_tf_tc_good.Hide();
		image_jump_reactive_tf_tc_bad.Hide();
		image_run_interval_time_good.Hide();
		image_run_interval_time_bad.Hide();
	}

	private void hideAllTables() {
		//hide simple jump info
		hbox_jump_simple_titles.Hide();
		table_jump_simple.Hide();
		table_jump_simple_values.Hide();
		
		//hide reactive info
		table_jump_reactive.Hide();
		table_jump_reactive_values.Hide();
		
		//hide run simple info
		hbox_run_simple_titles.Hide();
		table_run_simple.Hide();
		table_run_simple_values.Hide();
		
		//hide run interval info
		table_run_interval.Hide();
		table_run_interval_values.Hide();
		
		//hide pulse info
		table_pulse.Hide();
		table_pulse_values.Hide();
		
		//hide reaction time info
		hbox_reaction_time_titles.Hide();
		table_reaction_time.Hide();
		table_reaction_time_values.Hide();
	}
	
	private void showJumpSimpleLabels() {
		//show simple jump info
		hbox_jump_simple_titles.Show();
		table_jump_simple.Show();
		table_jump_simple_values.Show();

		//initializeLabels
		label_jump_simple_tc_now.Text = "";
		label_jump_simple_tc_person.Text = "";
		label_jump_simple_tc_session.Text = "";
		label_jump_simple_tf_now.Text = "";
		label_jump_simple_tf_person.Text = "";
		label_jump_simple_tf_session.Text = "";
	}
	
	
	private void showJumpReactiveLabels() {
		//show reactive info
		table_jump_reactive.Show();
		table_jump_reactive_values.Show();

		//initializeLabels
		label_jump_reactive_tc_now.Text = "";
		label_jump_reactive_tc_avg.Text = "";
		label_jump_reactive_tf_now.Text = "";
		label_jump_reactive_tf_avg.Text = "";
		label_jump_reactive_tf_tc_now.Text = "";
		label_jump_reactive_tf_tc_avg.Text = "";
	}
	
	private void showRunSimpleLabels() {
		//show run simple info
		hbox_run_simple_titles.Show();
		table_run_simple.Show();
		table_run_simple_values.Show();
		
		//initializeLabels
		label_run_simple_time_now.Text = "";
		label_run_simple_time_person.Text = "";
		label_run_simple_time_session.Text = "";
		label_run_simple_speed_now.Text = "";
		label_run_simple_speed_person.Text = "";
		label_run_simple_speed_session.Text = "";
	}
		
	private void showRunIntervalLabels() {
		//show run interval info
		table_run_interval.Show();
		table_run_interval_values.Show();
		
		//initializeLabels
		label_run_interval_time_now.Text = "";
		label_run_interval_time_avg.Text = "";
		label_run_interval_speed_now.Text = "";
		label_run_interval_speed_avg.Text = "";
	}
	
	private void showPulseLabels() {
		//show pulse info
		table_pulse.Show();
		table_pulse_values.Show();

		//initializeLabels
		label_pulse_now.Text = "";
		label_pulse_avg.Text = "";
	}
	
	private void showReactionTimeLabels() {
		//show info
		hbox_reaction_time_titles.Show();
		table_reaction_time.Show();
		table_reaction_time_values.Show();

		//initializeLabels
		label_reaction_time_now.Text = "";
		label_reaction_time_person.Text = "";
		label_reaction_time_session.Text = "";
	}

	
	//called for cleaning the graph of a event done before than the current
	private void clearDrawingArea() 
	{
		if(pixmap == null) 
			pixmap = new Gdk.Pixmap (drawingarea.GdkWindow, drawingarea.Allocation.Width, drawingarea.Allocation.Height, -1);
		
		erasePaint(drawingarea);
	}
	
	//reactive, interval, pulse events, put flag needSensitiveButtonFinish to true when started
	//event.cs (Pulse.GTK) calls this method:
	public void ButtonFinishMakeSensitive() {
		button_finish.Sensitive = true;
	}
		
	private void clearProgressBars() 
	{
		progressbar_event.Fraction = 0;
		progressbar_event.Text = "";
		progressbar_time.Fraction = 0;
		progressbar_time.Text = "";
	
		//clear also the close labels
		label_event_value.Text = "";
		label_time_value.Text = "";
	}

	public void on_drawingarea_configure_event(object o, ConfigureEventArgs args)
	{
		Log.Write("A");
		Gdk.EventConfigure ev = args.Event;
		Gdk.Window window = ev.Window;
	

		Log.Write("B1");
		
		Gdk.Rectangle allocation = drawingarea.Allocation;
		
		Log.Write("B2");
	
		if(pixmap == null) {
			pixmap = new Gdk.Pixmap (window, allocation.Width, allocation.Height, -1);
		
			Log.Write("B3");
		
			erasePaint(drawingarea);
			
			graphProgress = phasesGraph.DOING; 
		}
	}
	
	public void on_drawingarea_expose_event(object o, ExposeEventArgs args)
	{
		/* in some mono installations, configure_event is not called, but expose_event yes. 
		 * Do here the initialization
		 */
		
		Log.Write("C");
		
		if(pixmap == null) {
			Log.Write("T1");
			
			Gdk.Rectangle allocation = drawingarea.Allocation;
			pixmap = new Gdk.Pixmap (drawingarea.GdkWindow, allocation.Width, allocation.Height, -1);
			erasePaint(drawingarea);

			Log.Write("T2");
		
			graphProgress = phasesGraph.DOING; 
		}

			
		Log.Write("D");
		
		Gdk.Rectangle area = args.Event.Area;

		Log.Write("E1");

		//sometimes this is called when pait is finished
		//don't let this erase win
		//if(graphProgress != phasesGraph.DONE) {
		if(pixmap != null) {
			Log.Write("E2");

			args.Event.Window.DrawDrawable(drawingarea.Style.WhiteGC, pixmap,
				area.X, area.Y,
				area.X, area.Y,
				area.Width, area.Height);
		}
		
		Log.Write("E3");
	}


	private void erasePaint(Gtk.DrawingArea drawingarea) {
		pixmap.DrawRectangle (drawingarea.Style.WhiteGC, true, 0, 0,
				drawingarea.Allocation.Width, drawingarea.Allocation.Height);
		
		// -- refresh
		drawingarea.QueueDraw();
	}
	

	// simple and DJ jump	
	public void PrepareJumpSimpleGraph(double tv, double tc) 
	{
		//check graph properties window is not null (propably user has closed it with the DeleteEvent
		//then create it, but not show it
		if(eventGraphConfigureWin == null)
			eventGraphConfigureWin = EventGraphConfigureWindow.Show(false);

		
		Log.Write("k1");
		
		//obtain data
		double tvPersonAVG = SqliteSession.SelectAllEventsOfAType(sessionID, personID, tableName, eventType, "TV");
		double tvSessionAVG = SqliteSession.SelectAllEventsOfAType(sessionID, -1, tableName, eventType, "TV");

		double tcPersonAVG = 0; 
		double tcSessionAVG = 0; 
		if(tc > 0) {
			tcPersonAVG = SqliteSession.SelectAllEventsOfAType(sessionID, personID, tableName, eventType, "TC");
			tcSessionAVG = SqliteSession.SelectAllEventsOfAType(sessionID, -1, tableName, eventType, "TC");
		}
		
		double maxValue = 0;
		double minValue = 0;
		int topMargin = 10; 
		int bottomMargin = 10; 

		//if max value of graph is automatic
		if(eventGraphConfigureWin.Max == -1) {
			maxValue = Util.GetMax(
					tv.ToString() + "=" + tvPersonAVG.ToString() + "=" + tvSessionAVG.ToString() + "=" +
					tc.ToString() + "=" + tcPersonAVG.ToString() + "=" + tcSessionAVG.ToString());
		} else {
			maxValue = eventGraphConfigureWin.Max;
			topMargin = 0;
		}
		
		//if min value of graph is automatic
		if(eventGraphConfigureWin.Min == -1) {
			string myString = tv.ToString() + "=" + tvPersonAVG.ToString() + "=" + tvSessionAVG.ToString();
			if(tc > 0)
				myString = myString + "=" + tc.ToString() + "=" + tcPersonAVG.ToString() + "=" + tcSessionAVG.ToString();
			minValue = Util.GetMin(myString);
		} else {
			minValue = eventGraphConfigureWin.Min;
			bottomMargin = 0;
		}
		
		//paint graph
		paintJumpSimple (drawingarea, tv, tvPersonAVG, tvSessionAVG, tc, tcPersonAVG, tcSessionAVG, maxValue, minValue, topMargin, bottomMargin);

		//printLabels
		printLabelsJumpSimple (tv, tvPersonAVG, tvSessionAVG, tc, tcPersonAVG, tcSessionAVG);
		
		Log.Write("k2");
	
		// -- refresh
		drawingarea.QueueDraw();
	
		Log.Write("k3");
		
	}
	
	// Reactive jump 
	public void PrepareJumpReactiveGraph(double lastTv, double lastTc, string tvString, string tcString, 
			bool volumeOn, RepetitiveConditionsWindow repetitiveConditionsWin) {
		//check graph properties window is not null (propably user has closed it with the DeleteEvent
		//then create it, but not show it
		if(eventGraphConfigureWin == null)
			eventGraphConfigureWin = EventGraphConfigureWindow.Show(false);

		Log.Write("l1");

		//search MAX
		double maxValue = 0;
		int topMargin = 10;
		//if max value of graph is automatic
		if(eventGraphConfigureWin.Max == -1) 
			maxValue = Util.GetMax(tvString + "=" + tcString);
		else {
			maxValue = eventGraphConfigureWin.Max;
			topMargin = 0;
		}
	
		//search min
		double minValue = 1000;
		int bottomMargin = 10; 
		//if min value of graph is automatic
		if(eventGraphConfigureWin.Min == -1) 
			minValue = Util.GetMin(tvString + "=" + tcString);
		else {
			minValue = eventGraphConfigureWin.Min;
			bottomMargin = 10; 
		}		

		int jumps = Util.GetNumberOfJumps(tvString, true); 

		//paint graph
		paintJumpReactive (drawingarea, lastTv, lastTc, tvString, tcString, Util.GetAverage(tvString), Util.GetAverage(tcString), 
				maxValue, minValue, jumps, topMargin, bottomMargin, 
				bestOrWorstTvTcIndex(true, tvString, tcString), bestOrWorstTvTcIndex(false, tvString, tcString), 
				volumeOn, repetitiveConditionsWin);
		
		Log.Write("l2");
	
		// -- refresh
		drawingarea.QueueDraw();
		
		Log.Write("l3");
	}
	
	//identify which subjump is the best or the worst in tv/tc index	
	private int bestOrWorstTvTcIndex(bool isBest, string tvString, string tcString) 
	{
		string [] myTVStringFull = tvString.Split(new char[] {'='});
		string [] myTCStringFull = tcString.Split(new char[] {'='});
		double myTVDouble = 0;
		double myTCDouble = 0;
		double maxTvTc = 0;
		double minTvTc = 100000;
		int count = 0;
		int posSelected = 0;

		foreach (string myTV in myTVStringFull) {
			myTVDouble = Convert.ToDouble(myTV);
			myTCDouble = Convert.ToDouble(myTCStringFull[count]);
			if(myTCDouble > 0) {
				if(isBest) {
					if(myTVDouble / myTCDouble > maxTvTc) {
						maxTvTc = myTVDouble / myTCDouble;
						posSelected = count;
					}
				}
				else {
					if(myTVDouble / myTCDouble < minTvTc) {
						minTvTc = myTVDouble / myTCDouble;
						posSelected = count;
					}
				}
			}

			count ++;
		}
		return posSelected; 
	}
			

	// run simple
	public void PrepareRunSimpleGraph(double time, double speed) 
	{
		//check graph properties window is not null (propably user has closed it with the DeleteEvent
		//then create it, but not show it
		if(eventGraphConfigureWin == null)
			eventGraphConfigureWin = EventGraphConfigureWindow.Show(false);

		
		Log.Write("k1");
			
		bool paintTime = false; //paint speed
		if(eventGraphConfigureWin.RunsTimeActive) 
			paintTime = true;
		
		//obtain data
		double timePersonAVG = SqliteSession.SelectAllEventsOfAType(sessionID, personID, tableName, eventType, "time");
		double timeSessionAVG = SqliteSession.SelectAllEventsOfAType(sessionID, -1, tableName, eventType, "time");

		//double distancePersonAVG = SqliteSession.SelectAllEventsOfAType(sessionID, personID, tableName, eventType, "distance");
		//double distanceSessionAVG = SqliteSession.SelectAllEventsOfAType(sessionID, -1, tableName, eventType, "distance");
		//better to know speed like:
		//SELECT AVG(distance/time) from run; than 
		//SELECT AVG(distance) / SELECT AVG(time) 
		//first is ok, because is the speed AVG
		//2nd is not good because it tries to do an AVG of all distances and times
		double speedPersonAVG = SqliteSession.SelectAllEventsOfAType(sessionID, personID, tableName, eventType, "distance/time");
		double speedSessionAVG = SqliteSession.SelectAllEventsOfAType(sessionID, -1, tableName, eventType, "distance/time");

		double maxValue = 0;
		double minValue = 0;
		int topMargin = 10; 
		int bottomMargin = 10; 

		//if max value of graph is automatic
		if(eventGraphConfigureWin.Max == -1) 
			if(paintTime)
				maxValue = Util.GetMax(time.ToString() + "=" + timePersonAVG.ToString() + "=" + timeSessionAVG.ToString());
			else						//paint speed
				maxValue = Util.GetMax(speed.ToString() + "=" + speedPersonAVG.ToString() + "=" + speedSessionAVG.ToString());
		else {
			maxValue = eventGraphConfigureWin.Max;
			topMargin = 0;
		}
			
		//if min value of graph is automatic
		if(eventGraphConfigureWin.Min == -1) 
			if(paintTime)
				minValue = Util.GetMin(time.ToString() + "=" + timePersonAVG.ToString() + "=" + timeSessionAVG.ToString());
			else
				minValue = Util.GetMin(speed.ToString() + "=" + speedPersonAVG.ToString() + "=" + speedSessionAVG.ToString());
		else {
			minValue = eventGraphConfigureWin.Min;
			bottomMargin = 0;
		}
			
		
		//paint graph
		if(paintTime)
			paintRunSimple (drawingarea, pen_rojo, time, timePersonAVG, timeSessionAVG, maxValue, minValue, topMargin, bottomMargin);
		else						//paint speed
			paintRunSimple (drawingarea, pen_azul, speed, speedPersonAVG, speedSessionAVG, maxValue, minValue, topMargin, bottomMargin);
		
		//printLabels
		printLabelsRunSimple (time, timePersonAVG, timeSessionAVG, speed, speedPersonAVG, speedSessionAVG);
		
		Log.Write("k2");
		
		// -- refresh
		drawingarea.QueueDraw();
		
		Log.Write("k3");
	}
	
	// run interval
	public void PrepareRunIntervalGraph(double distance, double lastTime, string timesString,
			bool volumeOn, RepetitiveConditionsWindow repetitiveConditionsWin) {
		//check graph properties window is not null (propably user has closed it with the DeleteEvent
		//then create it, but not show it
		if(eventGraphConfigureWin == null)
			eventGraphConfigureWin = EventGraphConfigureWindow.Show(false);

		Log.Write("l1");
			
		bool paintTime = false; //paint speed
		if(eventGraphConfigureWin.RunsTimeActive) 
			paintTime = true;

		//search MAX 
		double maxValue = 0;
		int topMargin = 10;
		//if max value of graph is automatic
		if(eventGraphConfigureWin.Max == -1) {
			if(paintTime)
				maxValue = Util.GetMax(timesString);
			else
				maxValue = distance / Util.GetMin(timesString); //getMin because is on the "denominador"
		} else {
			maxValue = eventGraphConfigureWin.Max;
			topMargin = 0;
		}
			
		//search min
		double minValue = 1000;
		int bottomMargin = 10; 
		//if min value of graph is automatic
		if(eventGraphConfigureWin.Min == -1) 
			if(paintTime)
				minValue = Util.GetMin(timesString);
			else
				minValue = distance / Util.GetMax(timesString); //getMax because is in the "denominador"
		else {
			minValue = eventGraphConfigureWin.Min;
			bottomMargin = 10; 
		}		

		int tracks = Util.GetNumberOfJumps(timesString, true); 

		//paint graph
		paintRunInterval (drawingarea, paintTime, distance, lastTime, timesString, Util.GetAverage(timesString), maxValue, minValue, tracks, topMargin, bottomMargin,
				Util.GetPosMax(timesString), Util.GetPosMin(timesString),
				volumeOn, repetitiveConditionsWin);
		
		Log.Write("l2");
		
		// -- refresh
		drawingarea.QueueDraw();
		
		Log.Write("l3");
	}


	// pulse 
	public void PreparePulseGraph(double lastTime, string timesString) { 
		//check graph properties window is not null (propably user has closed it with the DeleteEvent
		//then create it, but not show it
		if(eventGraphConfigureWin == null)
			eventGraphConfigureWin = EventGraphConfigureWindow.Show(false);

		Log.Write("l1");

		//search MAX 
		double maxValue = 0;
		int topMargin = 10;
		//if max value of graph is automatic
		if(eventGraphConfigureWin.Max == -1) 
			maxValue = Util.GetMax(timesString);
		else {
			maxValue = eventGraphConfigureWin.Max;
			topMargin = 0;
		}
			
		//search MIN 
		double minValue = 1000;
		int bottomMargin = 10;
		//if min value of graph is automatic
		if(eventGraphConfigureWin.Min == -1) 
			minValue = Util.GetMin(timesString);
		else {
			minValue = eventGraphConfigureWin.Min;
			bottomMargin = 0;
		}
			
		int pulses = Util.GetNumberOfJumps(timesString, true); 

		//paint graph
		paintPulse (drawingarea, lastTime, timesString, Util.GetAverage(timesString), pulses, maxValue, minValue, topMargin, bottomMargin);
		
		Log.Write("l2");
		
		// -- refresh
		drawingarea.QueueDraw();
		
		Log.Write("l3");
	}
	
	public void PrepareReactionTimeGraph(double time) 
	{
		//check graph properties window is not null (propably user has closed it with the DeleteEvent
		//then create it, but not show it
		if(eventGraphConfigureWin == null)
			eventGraphConfigureWin = EventGraphConfigureWindow.Show(false);

		
		Log.Write("k1");
		
		//obtain data
		double timePersonAVG = SqliteSession.SelectAllEventsOfAType(sessionID, personID, tableName, eventType, "time");
		double timeSessionAVG = SqliteSession.SelectAllEventsOfAType(sessionID, -1, tableName, eventType, "time");

		double maxValue = 0;
		double minValue = 0;
		int topMargin = 10; 
		int bottomMargin = 10; 

		//if max value of graph is automatic
		if(eventGraphConfigureWin.Max == -1) {
			maxValue = Util.GetMax(
					time.ToString() + "=" + timePersonAVG.ToString() + "=" + timeSessionAVG.ToString());
		} else {
			maxValue = eventGraphConfigureWin.Max;
			topMargin = 0;
		}
		
		//if min value of graph is automatic
		if(eventGraphConfigureWin.Min == -1) {
			minValue = Util.GetMin(
					time.ToString() + "=" + timePersonAVG.ToString() + "=" + timeSessionAVG.ToString());
		} else {
			minValue = eventGraphConfigureWin.Min;
			bottomMargin = 0;
		}
		
		//paint graph (use simple jump method)
		paintJumpSimple (drawingarea, time, timePersonAVG, timeSessionAVG, 0, 0, 0, maxValue, minValue, topMargin, bottomMargin);

		printLabelsReactionTime (time, timePersonAVG, timeSessionAVG);
		
		Log.Write("k2");
	
		// -- refresh
		drawingarea.QueueDraw();
	
		Log.Write("k3");
	}
	

	private void printLabelsJumpSimple (double tvNow, double tvPerson, double tvSession, double tcNow, double tcPerson, double tcSession) {
		if(tcNow > 0) {
			label_jump_simple_tc_now.Text = Util.TrimDecimals(tcNow.ToString(), pDN);
			label_jump_simple_tc_person.Text = Util.TrimDecimals(tcPerson.ToString(), pDN);
			label_jump_simple_tc_session.Text = Util.TrimDecimals(tcSession.ToString(), pDN);
		} else {
			label_jump_simple_tc_now.Text = "";
			label_jump_simple_tc_person.Text = "";
			label_jump_simple_tc_session.Text = "";
		}
		label_jump_simple_tf_now.Text = Util.TrimDecimals(tvNow.ToString(), pDN);
		label_jump_simple_tf_person.Text = Util.TrimDecimals(tvPerson.ToString(), pDN);
		label_jump_simple_tf_session.Text = Util.TrimDecimals(tvSession.ToString(), pDN);
	}
	
	private void printLabelsRunSimple (double timeNow, double timePerson, double timeSession, double speedNow, double speedPerson, double speedSession) {
		label_run_simple_time_now.Text = Util.TrimDecimals(timeNow.ToString(), pDN);
		label_run_simple_time_person.Text = Util.TrimDecimals(timePerson.ToString(), pDN);
		label_run_simple_time_session.Text = Util.TrimDecimals(timeSession.ToString(), pDN);
		
		label_run_simple_speed_now.Text = Util.TrimDecimals(speedNow.ToString(), pDN);
		label_run_simple_speed_person.Text = Util.TrimDecimals(speedPerson.ToString(), pDN);
		label_run_simple_speed_session.Text = Util.TrimDecimals(speedSession.ToString(), pDN);
	}
	
	private void printLabelsReactionTime (double timeNow, double timePerson, double timeSession) {
		label_reaction_time_now.Text = Util.TrimDecimals(timeNow.ToString(), pDN);
		label_reaction_time_person.Text = Util.TrimDecimals(timePerson.ToString(), pDN);
		label_reaction_time_session.Text = Util.TrimDecimals(timeSession.ToString(), pDN);
	}
	

	private void paintJumpSimple (Gtk.DrawingArea drawingarea, 
			double tvNow, double tvPerson, double tvSession, 
			double tcNow, double tcPerson, double tcSession,
			double maxValue, double minValue, int topMargin, int bottomMargin)
	{
		
		int ancho=drawingarea.Allocation.Width;
		int alto=drawingarea.Allocation.Height;
		
		
		Log.Write(" paint1 ");
		
		erasePaint(drawingarea);
		
		Log.Write(" paint2 ");
	
		writeMarginsText(maxValue, minValue, alto);
		
		Log.Write(" paint7 ");
	
		//check now here that we will have not division by zero problems
		if(maxValue - minValue > 0) {
			//red for TC
			if(tcNow > 0) {
				pixmap.DrawLine(pen_rojo, ancho*1/6, alto, ancho*1/6, calculatePaintHeight(tcNow, alto, maxValue, minValue, topMargin, bottomMargin));
				pixmap.DrawLine(pen_rojo, ancho*3/6, alto, ancho*3/6, calculatePaintHeight(tcPerson, alto, maxValue, minValue, topMargin, bottomMargin));
				pixmap.DrawLine(pen_rojo, ancho*5/6, alto, ancho*5/6, calculatePaintHeight(tcSession, alto, maxValue, minValue, topMargin, bottomMargin));
			}
		
			//blue for TF
			pixmap.DrawLine(pen_azul, ancho*1/6 +10, alto, ancho*1/6 +10, calculatePaintHeight(tvNow, alto, maxValue, minValue, topMargin, bottomMargin));
			pixmap.DrawLine(pen_azul, ancho*3/6 +10, alto, ancho*3/6 +10, calculatePaintHeight(tvPerson, alto, maxValue, minValue, topMargin, bottomMargin));
			pixmap.DrawLine(pen_azul, ancho*5/6 +10, alto, ancho*5/6 +10, calculatePaintHeight(tvSession, alto, maxValue, minValue, topMargin, bottomMargin));
			
			//circles
			if(eventGraphConfigureWin.PaintCircle) {
				if(tcNow > 0) {
					pixmap.DrawArc(pen_rojo, true, ancho*1/6 - radio/2 + arcSystemCorrection, calculatePaintHeight(tcNow, alto, maxValue, minValue, topMargin, bottomMargin) -radio/2, radio , radio, 0, 360*64);
					pixmap.DrawArc(pen_rojo, true, ancho*3/6 - radio/2 + arcSystemCorrection, calculatePaintHeight(tcPerson, alto, maxValue, minValue, topMargin, bottomMargin) -radio/2, radio , radio, 0, 360*64);
					pixmap.DrawArc(pen_rojo, true, ancho*5/6 - radio/2 + arcSystemCorrection, calculatePaintHeight(tcSession, alto, maxValue, minValue, topMargin, bottomMargin) -radio/2, radio , radio, 0, 360*64);
				}
				pixmap.DrawArc(pen_azul, true, ancho*1/6 +10 - radio/2 + arcSystemCorrection, calculatePaintHeight(tvNow, alto, maxValue, minValue, topMargin, bottomMargin) -radio/2, radio , radio, 0, 360*64);
				pixmap.DrawArc(pen_azul, true, ancho*3/6 +10 - radio/2 + arcSystemCorrection, calculatePaintHeight(tvPerson, alto, maxValue, minValue, topMargin, bottomMargin) -radio/2, radio , radio, 0, 360*64);
				pixmap.DrawArc(pen_azul, true, ancho*5/6 +10 - radio/2 + arcSystemCorrection, calculatePaintHeight(tvSession, alto, maxValue, minValue, topMargin, bottomMargin) -radio/2, radio , radio, 0, 360*64);
			}	
			
	
			//paint reference guide black and green if needed
			drawGuideOrAVG(pen_negro_discont, eventGraphConfigureWin.BlackGuide, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
			drawGuideOrAVG(pen_green_discont, eventGraphConfigureWin.GreenGuide, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
		}
		
	
		
		Log.Write(" paint2 ");

		graphProgress = phasesGraph.DONE; 
	}

	private void paintRunSimple (Gtk.DrawingArea drawingarea, Gdk.GC myPen, double now, double person, double session,
			double maxValue, double minValue, int topMargin, int bottomMargin)
	{
		int ancho=drawingarea.Allocation.Width;
		int alto=drawingarea.Allocation.Height;
		
		
		Log.Write(" paint1 ");
		
		erasePaint(drawingarea);
		
		writeMarginsText(maxValue, minValue, alto);
		
		Log.Write(" paint7 ");
	
		//check now here that we will have not division by zero problems
		if(maxValue - minValue > 0) {
			pixmap.DrawLine(myPen, ancho*1/6 , alto, ancho*1/6 , calculatePaintHeight(now, alto, maxValue, minValue, topMargin, bottomMargin));
			pixmap.DrawLine(myPen, ancho*3/6 , alto, ancho*3/6 , calculatePaintHeight(person, alto, maxValue, minValue, topMargin, bottomMargin));
			pixmap.DrawLine(myPen, ancho*5/6 , alto, ancho*5/6 , calculatePaintHeight(session, alto, maxValue, minValue, topMargin, bottomMargin));


			if(eventGraphConfigureWin.PaintCircle) {
				pixmap.DrawArc(myPen, true, ancho*1/6 - radio/2 + arcSystemCorrection, calculatePaintHeight(now, alto, maxValue, minValue, topMargin, bottomMargin) -radio/2, radio , radio, 0, 360*64);
				pixmap.DrawArc(myPen, true, ancho*3/6 - radio/2 + arcSystemCorrection, calculatePaintHeight(person, alto, maxValue, minValue, topMargin, bottomMargin) -radio/2, radio , radio, 0, 360*64);
				pixmap.DrawArc(myPen, true, ancho*5/6 - radio/2 + arcSystemCorrection, calculatePaintHeight(session, alto, maxValue, minValue, topMargin, bottomMargin) -radio/2, radio , radio, 0, 360*64);
			}	
			
	
			//paint reference guide black and green if needed
			drawGuideOrAVG(pen_negro_discont, eventGraphConfigureWin.BlackGuide, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
			drawGuideOrAVG(pen_green_discont, eventGraphConfigureWin.GreenGuide, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
		}
		
	
		
		Log.Write(" paint2 ");

		graphProgress = phasesGraph.DONE; 
	}

	
	private void paintJumpReactive (Gtk.DrawingArea drawingarea, double lastTv, double lastTc, string tvString, string tcString, 
			double avgTV, double avgTC, double maxValue, double minValue, int jumps, 
			int topMargin, int bottomMargin, int posMax, int posMin, bool volumeOn,
			RepetitiveConditionsWindow repetitiveConditionsWin)
	{
		//int topMargin = 10; 
		int ancho=drawingarea.Allocation.Width;
		int alto=drawingarea.Allocation.Height;
		
		
		Log.Write(" paint1 reactive 1");
		
		erasePaint(drawingarea);
		
		writeMarginsText(maxValue, minValue, alto);
		
		//check now here that we will have not division by zero problems
		if(maxValue - minValue > 0) {

			if(jumps > 1) {
				//blue tf average discountinuos line	
				drawGuideOrAVG(pen_azul_discont, avgTV, alto, ancho, topMargin, bottomMargin, maxValue, minValue);

				//red tc average discountinuos line	
				drawGuideOrAVG(pen_rojo_discont, avgTC, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
			}

			//paint reference guide black and green if needed
			drawGuideOrAVG(pen_negro_discont, eventGraphConfigureWin.BlackGuide, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
			drawGuideOrAVG(pen_green_discont, eventGraphConfigureWin.GreenGuide, alto, ancho, topMargin, bottomMargin, maxValue, minValue);

			
			//blue tf evolution	
			string [] myTVStringFull = tvString.Split(new char[] {'='});
			int count = 0;
			double oldValue = 0;
			double myTVDouble = 0;

			foreach (string myTV in myTVStringFull) {
				myTVDouble = Convert.ToDouble(myTV);
				if(myTVDouble < 0)
					myTVDouble = 0;

				if (count > 0) 
					pixmap.DrawLine(pen_azul, //blue for TF
							Convert.ToInt32((ancho-rightMargin)*(count-.5)/jumps), calculatePaintHeight(oldValue, alto, maxValue, minValue, topMargin, bottomMargin),
							Convert.ToInt32((ancho-rightMargin)*(count+.5)/jumps), calculatePaintHeight(myTVDouble, alto, maxValue, minValue, topMargin, bottomMargin));

				//paint Y lines
				if(eventGraphConfigureWin.VerticalGrid) 
					pixmap.DrawLine(pen_beige_discont, Convert.ToInt32((ancho - rightMargin) *(count+.5)/jumps), topMargin, Convert.ToInt32((ancho - rightMargin) *(count+.5)/jumps), alto-topMargin);

				oldValue = myTVDouble;
				count ++;
			}
			
			drawCircleAndWriteValue(pen_azul, myTVDouble, --count, jumps, ancho, alto, maxValue, minValue, topMargin, bottomMargin);

			//read tc evolution	
			string [] myTCStringFull = tcString.Split(new char[] {'='});
			count = 0;
			oldValue = 0;
			double myTCDouble = 0;

			foreach (string myTC in myTCStringFull) {
				myTCDouble = Convert.ToDouble(myTC);

				//if we are at second value (or more), and first was not a "-1"
				//-1 means here that first jump has not TC (started inside)
				if (count > 0 && oldValue != -1)  
					pixmap.DrawLine(pen_rojo, //red for TC
							Convert.ToInt32((ancho-rightMargin)*(count-.5)/jumps), calculatePaintHeight(oldValue, alto, maxValue, minValue, topMargin, bottomMargin),
							Convert.ToInt32((ancho-rightMargin)*(count+.5)/jumps), calculatePaintHeight(myTCDouble, alto, maxValue, minValue, topMargin, bottomMargin));

				oldValue = myTCDouble;
				count ++;
			}
			
			drawCircleAndWriteValue(pen_rojo, myTCDouble, --count, jumps, ancho, alto, maxValue, minValue, topMargin, bottomMargin);
		

			//draw best tv/tc
			pixmap.DrawLine(pen_brown_bold,
					Convert.ToInt32((ancho-rightMargin)*(posMax+.5)/jumps), calculatePaintHeight(Convert.ToDouble(myTVStringFull[posMax]), alto, maxValue, minValue, topMargin, bottomMargin),
					Convert.ToInt32((ancho-rightMargin)*(posMax+.5)/jumps), calculatePaintHeight(Convert.ToDouble(myTCStringFull[posMax]), alto, maxValue, minValue, topMargin, bottomMargin));
			//draw worst tv/tc
			pixmap.DrawLine(pen_violet_bold,
					Convert.ToInt32((ancho-rightMargin)*(posMin+.5)/jumps), calculatePaintHeight(Convert.ToDouble(myTVStringFull[posMin]), alto, maxValue, minValue, topMargin, bottomMargin),
					Convert.ToInt32((ancho-rightMargin)*(posMin+.5)/jumps), calculatePaintHeight(Convert.ToDouble(myTCStringFull[posMin]), alto, maxValue, minValue, topMargin, bottomMargin));

			//bells & images
			image_jump_reactive_tf_good.Hide();
			image_jump_reactive_tf_bad.Hide();
			image_jump_reactive_tc_good.Hide();
			image_jump_reactive_tc_bad.Hide();
			image_jump_reactive_tf_tc_good.Hide();
			image_jump_reactive_tf_tc_bad.Hide();
			bool showTfGood = false;
			bool showTfBad = false;
			bool showTcGood = false;
			bool showTcBad = false;
			bool showTfTcGood = false;
			bool showTfTcBad = false;

			//sounds of best & worst
			if(count > 0) {
				if(repetitiveConditionsWin.TfTcBest && posMax == count)
					showTfTcGood = true;
				else if(repetitiveConditionsWin.TfTcWorst && posMin == count)
					showTfTcBad = true;
			}
				
			if(repetitiveConditionsWin.TfGreater && lastTv > repetitiveConditionsWin.TfGreaterValue) 
				showTfGood = true;
			if(repetitiveConditionsWin.TfLower && lastTv < repetitiveConditionsWin.TfLowerValue) 
				showTfBad = true;

			if(repetitiveConditionsWin.TcGreater && lastTc > repetitiveConditionsWin.TcGreaterValue) 
				showTcBad = true;
			if(repetitiveConditionsWin.TcLower && lastTc < repetitiveConditionsWin.TcLowerValue) 
				showTcGood = true;

			if(lastTc > 0 && repetitiveConditionsWin.TfTcGreater && lastTv/lastTc > repetitiveConditionsWin.TfTcGreaterValue) 
				showTfTcGood = true;
			if(lastTc > 0 && repetitiveConditionsWin.TfTcLower && lastTv/lastTc < repetitiveConditionsWin.TfTcLowerValue) 
				showTfTcGood = true;


			if(showTfGood || showTcGood || showTfTcGood)
				Util.PlaySound(Constants.SoundTypes.GOOD, volumeOn);
			if(showTfBad || showTcBad || showTfTcBad)
				Util.PlaySound(Constants.SoundTypes.BAD, volumeOn);

			if(showTfGood)
				image_jump_reactive_tf_good.Show();
			if(showTfBad)
				image_jump_reactive_tf_bad.Show();
			if(showTcGood)
				image_jump_reactive_tc_good.Show();
			if(showTcBad)
				image_jump_reactive_tc_bad.Show();
			if(showTfTcGood)
				image_jump_reactive_tf_tc_good.Show();
			if(showTfTcBad)
				image_jump_reactive_tf_tc_bad.Show();
		}

		Log.Write(" paint reactive 2 ");

		label_jump_reactive_tc_now.Text = Util.TrimDecimals(lastTc.ToString(), pDN);
		label_jump_reactive_tc_avg.Text = Util.TrimDecimals(avgTC.ToString(), pDN);
		label_jump_reactive_tf_now.Text = Util.TrimDecimals(lastTv.ToString(), pDN);
		label_jump_reactive_tf_avg.Text = Util.TrimDecimals(avgTV.ToString(), pDN);
		if(lastTc > 0)
			label_jump_reactive_tf_tc_now.Text = Util.TrimDecimals((lastTv/lastTc).ToString(), pDN);
		else
			label_jump_reactive_tf_tc_now.Text = "0";
		if(avgTC > 0)
			label_jump_reactive_tf_tc_avg.Text = Util.TrimDecimals((avgTV/avgTC).ToString(), pDN);
		else
			label_jump_reactive_tf_tc_avg.Text = "0";
		
		graphProgress = phasesGraph.DONE; 
	}

	private void paintRunInterval (Gtk.DrawingArea drawingarea, bool paintTime, double distance, double lastTime, 
			string timesString, double avgTime, double maxValue, double minValue, int tracks, int topMargin, int bottomMargin, 
			int hightValuePosition, int lowValuePosition,
			bool volumeOn, RepetitiveConditionsWindow repetitiveConditionsWin)
	{
		//int topMargin = 10; 
		int ancho=drawingarea.Allocation.Width;
		int alto=drawingarea.Allocation.Height;
		
		
		Log.Write(" paint1 run interval 1");
		
		erasePaint(drawingarea);
		
		writeMarginsText(maxValue, minValue, alto);
		
		//check now here that we will have not division by zero problems
		if(maxValue - minValue > 0) {

			if(tracks > 1) {
				if(paintTime) 
					//red time average discountinuos line	
					drawGuideOrAVG(pen_rojo_discont, avgTime, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
				else 
					//blue speed average discountinuos line	
					drawGuideOrAVG(pen_azul_discont, distance/avgTime, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
			}

			//paint reference guide black and green if needed
			drawGuideOrAVG(pen_negro_discont, eventGraphConfigureWin.BlackGuide, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
			drawGuideOrAVG(pen_green_discont, eventGraphConfigureWin.GreenGuide, alto, ancho, topMargin, bottomMargin, maxValue, minValue);

			
			string [] myTimesStringFull = timesString.Split(new char[] {'='});
			int count = 0;
			double oldValue = 0;
			double myTimeDouble = 0;

			Gdk.GC myPen = pen_rojo; //default value
			double myValue = 0;

			foreach (string myTime in myTimesStringFull) {
				myTimeDouble = Convert.ToDouble(myTime);
				if(myTimeDouble < 0)
					myTimeDouble = 0;

				if(paintTime) {
					//red time evolution
					myPen = pen_rojo;
					myValue = myTimeDouble;
				} else {
					//blue speed evolution	
					myPen = pen_azul;
					myValue = distance / myTimeDouble;
				}

				if (count > 0) {
					pixmap.DrawLine(myPen,
							Convert.ToInt32((ancho - rightMargin) *(count-.5)/tracks), calculatePaintHeight(oldValue, alto, maxValue, minValue, topMargin, bottomMargin),
							Convert.ToInt32((ancho - rightMargin) *(count+.5)/tracks), calculatePaintHeight(myValue, alto, maxValue, minValue, topMargin, bottomMargin));
				}
				
				//paint Y lines
				if(eventGraphConfigureWin.VerticalGrid) 
					pixmap.DrawLine(pen_beige_discont, Convert.ToInt32((ancho - rightMargin) *(count+.5)/tracks), topMargin, Convert.ToInt32((ancho - rightMargin) *(count+.5)/tracks), alto-topMargin);

				oldValue = myValue;
				count ++;
			}
			
			drawCircleAndWriteValue(myPen, myValue, --count, tracks, ancho, alto, maxValue, minValue, topMargin, bottomMargin);
		

			//bells & images
			image_run_interval_time_good.Hide();
			image_run_interval_time_bad.Hide();
			bool showTimeGood = false;
			bool showTimeBad = false;
	
			//sounds of best & worst
			if(count > 0) {
				if(repetitiveConditionsWin.RunTimeBest && lowValuePosition == count) 
					showTimeGood = true;
				else if(repetitiveConditionsWin.RunTimeWorst && hightValuePosition == count) 
					showTimeBad = true;
			}

			if(repetitiveConditionsWin.RunTimeLower && lastTime < repetitiveConditionsWin.RunTimeLowerValue) 
				showTimeGood = true;
			if(repetitiveConditionsWin.RunTimeGreater && lastTime > repetitiveConditionsWin.RunTimeGreaterValue) 
				showTimeBad = true;

			if(showTimeGood) {
				Util.PlaySound(Constants.SoundTypes.GOOD, volumeOn);
				image_run_interval_time_good.Show();
			}
			if(showTimeBad) {
				Util.PlaySound(Constants.SoundTypes.BAD, volumeOn);
				image_run_interval_time_bad.Show();
			}
		}
		
		Log.Write(" paint interval 2 ");

		label_run_interval_time_now.Text = Util.TrimDecimals(lastTime.ToString(), pDN);
		label_run_interval_time_avg.Text = Util.TrimDecimals(avgTime.ToString(), pDN);
		label_run_interval_speed_now.Text = Util.TrimDecimals((distance / lastTime).ToString(), pDN);
		label_run_interval_speed_avg.Text = Util.TrimDecimals((distance / avgTime).ToString(), pDN);
		
		graphProgress = phasesGraph.DONE; 
	}

	private void paintPulse (Gtk.DrawingArea drawingarea, double lastTime, string timesString, double avgTime, int pulses, 
			double maxValue, double minValue, int topMargin, int bottomMargin)
	{
		int ancho=drawingarea.Allocation.Width;
		int alto=drawingarea.Allocation.Height;
		
		
		Log.Write(" paint1 pulse 1");
		
		erasePaint(drawingarea);
		
		writeMarginsText(maxValue, minValue, alto);
		
		//check now here that we will have not division by zero problems
		if(maxValue - minValue > 0) {

			//blue time average discountinuos line	
			drawGuideOrAVG(pen_azul_discont, avgTime, alto, ancho, topMargin, bottomMargin, maxValue, minValue);

			//paint reference guide black and green if needed
			drawGuideOrAVG(pen_negro_discont, eventGraphConfigureWin.BlackGuide, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
			drawGuideOrAVG(pen_green_discont, eventGraphConfigureWin.GreenGuide, alto, ancho, topMargin, bottomMargin, maxValue, minValue);

			//blue time evolution	
			string [] myTimesStringFull = timesString.Split(new char[] {'='});
			int count = 0;
			double oldValue = 0;
			double myTimeDouble = 0;

			foreach (string myTime in myTimesStringFull) {
				myTimeDouble = Convert.ToDouble(myTime);
				if(myTimeDouble < 0)
					myTimeDouble = 0;

				if (count > 0) {
					pixmap.DrawLine(pen_azul, //blue for time
							Convert.ToInt32((ancho-rightMargin)*(count-.5)/pulses), calculatePaintHeight(oldValue, alto, maxValue, minValue, topMargin, bottomMargin),
							Convert.ToInt32((ancho-rightMargin)*(count+.5)/pulses), calculatePaintHeight(myTimeDouble, alto, maxValue, minValue, topMargin, bottomMargin));
				}

				//paint Y lines
				if(eventGraphConfigureWin.VerticalGrid) 
					pixmap.DrawLine(pen_beige_discont, Convert.ToInt32((ancho - rightMargin) *(count+.5)/pulses), topMargin, Convert.ToInt32((ancho - rightMargin) *(count+.5)/pulses), alto-topMargin);
				
				
				oldValue = myTimeDouble;
				count ++;
			}
		
			drawCircleAndWriteValue(pen_azul, myTimeDouble, --count, pulses, ancho, alto, maxValue, minValue, topMargin, bottomMargin);

		}
		
		Log.Write(" paint pulse 2 ");

		label_pulse_now.Text = Util.TrimDecimals(lastTime.ToString(), pDN);
		label_pulse_avg.Text = Util.TrimDecimals(avgTime.ToString(), pDN);
		
		graphProgress = phasesGraph.DONE; 
	}


	private void drawCircleAndWriteValue (Gdk.GC myPen, double myValue, int count, int total, int ancho, int alto, 
			double maxValue, double minValue, int topMargin, int bottomMargin) {

		//write text
		layout.SetMarkup((Math.Round(myValue,3)).ToString());
		pixmap.DrawLayout (myPen, ancho -rightMargin, (int)calculatePaintHeight(myValue, alto, maxValue, minValue, topMargin, bottomMargin) -7, layout); //-7 for aligning (is baseline) (font is Courier 7)

		if(eventGraphConfigureWin.PaintCircle) {
			//put circle in last value
			pixmap.DrawArc(myPen, true, Convert.ToInt32((ancho - rightMargin) *(count+.5)/total) - radio/2 + arcSystemCorrection, calculatePaintHeight(myValue, alto, maxValue, minValue, topMargin, bottomMargin) - radio/2, radio, radio, 0, 360*64);
		}
	}

		
	private void drawGuideOrAVG(Gdk.GC myPen, double guideHeight, int alto, int ancho, int topMargin, int bottomMargin, double maxValue, double minValue) 
	{
		if(guideHeight == -1)
			return; //return if checkbox guide is not checked
		else {
			pixmap.DrawLine(myPen, 
					0, calculatePaintHeight(guideHeight, alto, maxValue, minValue, topMargin, bottomMargin),
					ancho - rightMargin, calculatePaintHeight(guideHeight, alto, maxValue, minValue, topMargin, bottomMargin));
			//write textual data
			layout.SetMarkup((Math.Round(guideHeight,3)).ToString());
			pixmap.DrawLayout (pen_gris, ancho -rightMargin, (int)calculatePaintHeight(guideHeight, alto, maxValue, minValue, topMargin, bottomMargin) -7, layout); //-7 for aligning with Courier 7 font baseline
		}
	}

	//this calculates the Y of every point in the graph
	//the first "alto -" is because the graph comes from down to up, and we have to reverse
	private int calculatePaintHeight(double currentValue, int alto, double maxValue, double minValue, int topMargin, int bottomMargin) {
		return Convert.ToInt32(alto - bottomMargin - ((currentValue - minValue) * (alto - topMargin - bottomMargin) / (maxValue - minValue)));
	}

	private void writeMarginsText(double maxValue, double minValue, int alto) {
		
		Log.WriteLine(string.Format("(ini) PIXMAP: {0}, LAYOUT: {1}", pixmap, layout));
		Log.Write(" margin0 ");
		//write margins textual data
		layout.SetMarkup((Math.Round(maxValue, 3)).ToString());
		pixmap.DrawLayout (pen_gris, 0, 0, layout);
		//pixmap.DrawLayout (pen_gris, 0, 3, layout); //y to 3 (not 0) probably this solves rando Pango problems where this is not written and interface gets "clumsy"
		Log.Write(" margin1 ");
		layout.SetMarkup((Math.Round(minValue, 3)).ToString());
		pixmap.DrawLayout (pen_gris, 0, alto -10, layout); //don't search Y using alto - bottomMargin, because bottomMargin can be 0, 
									//and text goes down from the baseline, and will not be seen
		Log.Write(" margin2 ");
		
		/*
		//see if refresh helps in the hanging observed
		// -- refresh
		drawingarea.QueueDraw();
		*/
		Log.Write(" margin3 ");
		Log.WriteLine(string.Format("(end) PIXMAP: {0}, LAYOUT: {1}", pixmap, layout));
	}
		
			
	private void hideButtons() {
		button_cancel.Sensitive = false;
		button_close.Sensitive = true;
		button_finish.Sensitive = false;
	}


	public void EventEnded() {
		hideButtons();
		eventHasEnded = true;
	}
	
	
	
	void on_button_properties_clicked (object o, EventArgs args) {
		//now show the eventGraphConfigureWin
		eventGraphConfigureWin = EventGraphConfigureWindow.Show(true);
	}

	void on_button_update_clicked (object o, EventArgs args) 
	{
		//event will be raised, and managed in chronojump.cs
		//see ButtonUpdate at end of class
	}
	
		
	void on_finish_clicked (object o, EventArgs args)
	{
		//event will be raised, and managed in chronojump.cs
		//see ButtonFinish at end of class
	}
	
	void on_button_help_clicked (object o, EventArgs args)
	{
/*		
		new DialogMessage(Constants.MessageTypes.HELP, Catalog.GetString("This window shows the execution of a test. In the graph, you may see:\n\nSIMPLE TESTS:\n-\"Now\": shows the data of the current test.\n-\"Person AVG\": shows the average of the current person executing this type of test on this session.\n-\"Session AVG\": shows the Average of all persons executing this type of test on this session.\n\nREPETITIVE TESTS:\n-\"Now\": shows the data of the current test.\n-\"AVG\": shows the average of the current test.\n\n(For more statistics data, you may use the statistics window).\n\nYou may change the graph options using buttons on the left.\n\nAt the bottom you may see the evolution of the test, and you may finish it (depending on the type of test), or even cancel it."));
*/
	}
	
	public void ProgressBarEventOrTimePreExecution (bool isEvent, bool percentageMode, double events) 
	{
		if (isEvent) 
			progressbarEventOrTimeExecution (progressbar_event, percentageMode, label_event_value, events);
		else
			progressbarEventOrTimeExecution (progressbar_time, percentageMode, label_time_value, events);
	}

	private void progressbarEventOrTimeExecution (Gtk.ProgressBar progressbar, bool percentageMode, Gtk.Label label_value, double events)
	{
		if(limit == -1) {	//unlimited event (until 'finish' is clicked)
			progressbar.Pulse();
			//label_value.Text = events.ToString();
			if(events != -1)
				label_value.Text = Math.Round(events,3).ToString();
		} else {
			if(percentageMode) {
				double myFraction = events / limit;

				if(myFraction > 1)
					myFraction = 1;
				else if(myFraction < 0)
					myFraction = 0;

				//Log.Write("{0}-{1}", limit, myFraction);
				progressbar.Fraction = myFraction;
				//progressbar.Text = Util.TrimDecimals(events.ToString(), 1) + " / " + limit.ToString();
				if(events == -1) //we don't want to display nothing
					//progressbar.Text = "";
					label_value.Text = "";
				else 
					//progressbar.Text = Math.Round(events,3).ToString() + " / " + limit.ToString();
					label_value.Text = Math.Round(events,3).ToString();
			} else {
				//activity mode
				progressbar.Pulse();

				//pass -1 in events in activity mode if don't want to use this label
				if(events != -1)
					//label_value.Text = Util.TrimDecimals(events.ToString(), 1);
					label_value.Text = Math.Round(events,3).ToString();
			}
		}
	}

	//projecte cubevirtual de juan gonzalez
	
	Gdk.GC pen_rojo; //tc, also time
	Gdk.GC pen_azul; //tf, also speed and pulse
	Gdk.GC pen_rojo_discont; //avg tc in reactive
	Gdk.GC pen_azul_discont; //avg tf in reactive
	Gdk.GC pen_negro_discont; //guide
	Gdk.GC pen_green_discont; //guide
	Gdk.GC pen_gris; //textual data
	Gdk.GC pen_beige_discont; //Y cols
	Gdk.GC pen_brown_bold; //best tv/tc in rj
	Gdk.GC pen_violet_bold; //worst tv/tc in rj
	//Gdk.GC pen_blanco;
	

	void configureColors()
	{
		Gdk.Color rojo = new Gdk.Color(0xff,0,0);
		Gdk.Color azul  = new Gdk.Color(0,0,0xff);
		Gdk.Color negro = new Gdk.Color(0,0,0); 
		Gdk.Color green = new Gdk.Color(0,0xff,0);
		Gdk.Color gris = new Gdk.Color(0x66,0x66,0x66);
		Gdk.Color beige = new Gdk.Color(0x99,0x99,0x99);
		Gdk.Color brown = new Gdk.Color(0xd6,0x88,0x33);
		Gdk.Color violet = new Gdk.Color(0xc4,0x20,0xf3);
		//Gdk.Color blanco = new Gdk.Color(0xff,0xff,0xff);

		Gdk.Colormap colormap = Gdk.Colormap.System;
		colormap.AllocColor (ref rojo, true, true);
		colormap.AllocColor (ref azul,true,true);
		colormap.AllocColor (ref negro,true,true);
		colormap.AllocColor (ref green,true,true);
		colormap.AllocColor (ref gris,true,true);
		colormap.AllocColor (ref beige,true,true);
		colormap.AllocColor (ref brown,true,true);
		colormap.AllocColor (ref violet,true,true);
		//colormap.AllocColor (ref blanco,true,true);

		//-- Configurar los contextos graficos (pinceles)
		pen_rojo = new Gdk.GC(drawingarea.GdkWindow);
		pen_azul = new Gdk.GC(drawingarea.GdkWindow);
		pen_rojo_discont = new Gdk.GC(drawingarea.GdkWindow);
		pen_azul_discont = new Gdk.GC(drawingarea.GdkWindow);
		//pen_negro = new Gdk.GC(drawingarea.GdkWindow);
		//pen_blanco= new Gdk.GC(drawingarea.GdkWindow);
		pen_negro_discont = new Gdk.GC(drawingarea.GdkWindow);
		pen_green_discont = new Gdk.GC(drawingarea.GdkWindow);
		pen_gris = new Gdk.GC(drawingarea.GdkWindow);
		pen_beige_discont = new Gdk.GC(drawingarea.GdkWindow);
		pen_brown_bold = new Gdk.GC(drawingarea.GdkWindow);
		pen_violet_bold = new Gdk.GC(drawingarea.GdkWindow);

		
		pen_rojo.Foreground = rojo;
		pen_azul.Foreground = azul;
		
		pen_rojo_discont.Foreground = rojo;
		pen_azul_discont.Foreground = azul;
		pen_rojo_discont.SetLineAttributes(1, Gdk.LineStyle.OnOffDash, Gdk.CapStyle.Butt, Gdk.JoinStyle.Round);
		pen_azul_discont.SetLineAttributes(1, Gdk.LineStyle.OnOffDash, Gdk.CapStyle.Butt, Gdk.JoinStyle.Round);
		
		pen_negro_discont.Foreground = negro;
		pen_negro_discont.SetLineAttributes(1, Gdk.LineStyle.OnOffDash, Gdk.CapStyle.Butt, Gdk.JoinStyle.Round);
		pen_green_discont.Foreground = green;
		pen_green_discont.SetLineAttributes(1, Gdk.LineStyle.OnOffDash, Gdk.CapStyle.Butt, Gdk.JoinStyle.Round);
		
		pen_gris.Foreground = gris;

		pen_beige_discont.Foreground = beige;
		pen_beige_discont.SetLineAttributes(1, Gdk.LineStyle.OnOffDash, Gdk.CapStyle.Butt, Gdk.JoinStyle.Round);
		
		pen_brown_bold.Foreground = brown;
		pen_brown_bold.SetLineAttributes(2, Gdk.LineStyle.Solid, Gdk.CapStyle.Butt, Gdk.JoinStyle.Round);
		pen_violet_bold.Foreground = violet;
		pen_violet_bold.SetLineAttributes(2, Gdk.LineStyle.Solid, Gdk.CapStyle.Butt, Gdk.JoinStyle.Round);
	}
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		//event will be raised, and managed in chronojump.cs
		hideButtons();
	}
		
	void on_button_close_clicked (object o, EventArgs args)
	{
		EventExecuteWindowBox.event_execute.Hide();
		EventExecuteWindowBox = null;
	}
	
	void on_delete_event (object o, DeleteEventArgs args)
	{
		//if there's an event doing, simulate a cancel
		//see eventHasEnded comments at beginning of this file
		if(!eventHasEnded)
			button_cancel.Click();
		
		EventExecuteWindowBox.event_execute.Hide();
		EventExecuteWindowBox = null;
	}
	
	//when event finishes, we should put in the label_time, the correct totalTime, that comes from chronopic
	//label_time shows a updating value from a software chrono: onTimer, this is not exact and is now
	//replaced with the chronopic timer
	public double LabelTimeValue 
	{
		set { 
			label_time_value.Text = Math.Round(value,3).ToString();
		
			//also put progressBar text to "" because probably doesn't mach labe_time
			progressbar_time.Fraction = 1; 
			progressbar_time.Text = ""; 
		}
	}
	//same as LabelTimeValue	
	public double LabelEventValue 
	{
		set { label_event_value.Text = value.ToString(); }
	}
		
	
	public Button ButtonCancel 
	{
		get { return button_cancel; }
	}
	
	public Button ButtonFinish 
	{
		get { return button_finish; }
	}

	public Button ButtonUpdate 
	{
		get { return button_update; }
	}
	
}
