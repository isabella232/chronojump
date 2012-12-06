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
 * Copyright (C) 2004-2011   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Glade;
using System.Text; //StringBuilder
using System.Collections; //ArrayList

using System.Threading;
using Mono.Unix;


//--------------------------------------------------------
//---------------- EDIT JUMP WIDGET ----------------------
//--------------------------------------------------------

public class EditJumpWindow : EditEventWindow
{
	[Widget] private Gtk.Box jumps_single_leg;
	[Widget] private Gtk.RadioButton jumps_radiobutton_single_leg_mode_vertical;
	[Widget] private Gtk.RadioButton jumps_radiobutton_single_leg_mode_horizontal;
	[Widget] private Gtk.RadioButton jumps_radiobutton_single_leg_mode_lateral;
	[Widget] private Gtk.RadioButton jumps_radiobutton_single_leg_right;
	[Widget] private Gtk.RadioButton jumps_radiobutton_single_leg_left;
	[Widget] private Gtk.RadioButton jumps_radiobutton_single_leg_dominance_this_limb;
	[Widget] private Gtk.RadioButton jumps_radiobutton_single_leg_dominance_opposite;
	[Widget] private Gtk.RadioButton jumps_radiobutton_single_leg_dominance_unknown;
	[Widget] private Gtk.RadioButton jumps_radiobutton_single_leg_fall_this_limb;
	[Widget] private Gtk.RadioButton jumps_radiobutton_single_leg_fall_opposite;
	[Widget] private Gtk.RadioButton jumps_radiobutton_single_leg_fall_both;
	[Widget] private Gtk.SpinButton jumps_spinbutton_single_leg_distance;

	static EditJumpWindow EditJumpWindowBox;
	protected double personWeight;
	protected int sessionID; //for know weight specific to this session

	//for inheritance
	protected EditJumpWindow () {
	}

	public EditJumpWindow (Gtk.Window parent) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "edit_event", null);
		gladeXML.Autoconnect(this);
		this.parent =  parent;

		//put an icon to window
		UtilGtk.IconWindow(edit_event);
	
		eventBigTypeString = Catalog.GetString("jump");
	}

	static new public EditJumpWindow Show (Gtk.Window parent, Event myEvent, bool weightPercentPreferred, int pDN)
	{
		if (EditJumpWindowBox == null) {
			EditJumpWindowBox = new EditJumpWindow (parent);
		}	

		EditJumpWindowBox.weightPercentPreferred = weightPercentPreferred;
		EditJumpWindowBox.personWeight = SqlitePersonSession.SelectAttribute(
				false,
				Convert.ToInt32(myEvent.PersonID),
				Convert.ToInt32(myEvent.SessionID),
				Constants.Weight); 

		EditJumpWindowBox.pDN = pDN;
		
		EditJumpWindowBox.sessionID = myEvent.SessionID;

		EditJumpWindowBox.initializeValues();

		EditJumpWindowBox.fillDialog (myEvent);
		
		if(myEvent.Type == "slCMJ")
			EditJumpWindowBox.fillSingleLeg (myEvent.Description);
		
		EditJumpWindowBox.edit_event.Show ();

		return EditJumpWindowBox;
	}
	
	protected override void initializeValues () {
		typeOfTest = Constants.TestTypes.JUMP;
		showType = true;
		showRunStart = false;
		showTv = true;
		showTc= true;
		showFall = true;
		showDistance = false;
		showTime = false;
		showSpeed = false;
		showWeight = true;
		showLimited = false;
		showAngle = true;
		showMistakes = false;
		
		if(weightPercentPreferred)
			label_weight_units.Text = "%";
		else
			label_weight_units.Text = "Kg";

		Log.WriteLine(string.Format("-------------{0}", personWeight));
	}

	protected override string [] findTypes(Event myEvent) {
		Jump myJump = (Jump) myEvent;
		string [] myTypes;
		if (myJump.TypeHasFall) {
			myTypes = SqliteJumpType.SelectJumpTypes("", "TC", true); //don't show allJumpsName row, TC jumps, only select name
		} else {
			myTypes = SqliteJumpType.SelectJumpTypes("", "nonTC", true); //don't show allJumpsName row, nonTC jumps, only select name
		}
		return myTypes;
	}

	protected override void fillTv(Event myEvent) {
		Jump myJump = (Jump) myEvent;
		entryTv = myJump.Tv.ToString();

		//show all the decimals for not triming there in edit window using
		//(and having different values in formulae like GetHeightInCm ...)
		//entry_tv_value.Text = Util.TrimDecimals(entryTv, pDN);
		entry_tv_value.Text = entryTv;
	
		//hide tv if it's only a takeoff	
		if(myEvent.Type == Constants.TakeOffName || myEvent.Type == Constants.TakeOffWeightName) 
			entry_tv_value.Sensitive = false;
	}

	protected override void fillTc (Event myEvent) {
		//on normal jumps fills Tc and Fall
		Jump myJump = (Jump) myEvent;

		if (myJump.TypeHasFall) {
			entryTc = myJump.Tc.ToString();
			
			//show all the decimals for not triming there in edit window using
			//(and having different values in formulae like GetHeightInCm ...)
			//entry_tc_value.Text = Util.TrimDecimals(entryTc, pDN);
			entry_tc_value.Text = entryTc;
			
			entryFall = myJump.Fall.ToString();
			entry_fall_value.Text = entryFall;
			entry_tc_value.Sensitive = true;
			entry_fall_value.Sensitive = true;
		} else {
			entry_tc_value.Sensitive = false;
			entry_fall_value.Sensitive = false;
		}
	}

	protected override void fillWeight(Event myEvent) {
		Jump myJump = (Jump) myEvent;
		if(myJump.TypeHasWeight) {
			if(weightPercentPreferred)
				entryWeight = myJump.Weight.ToString();
			else
				entryWeight = Util.WeightFromPercentToKg(myJump.Weight, personWeight).ToString();

			entry_weight_value.Text = entryWeight;
			entry_weight_value.Sensitive = true;
		} else {
			entry_weight_value.Sensitive = false;
		}
	}
	
	protected override void fillAngle(Event myEvent) {
		Jump myJump = (Jump) myEvent;
		
		//default values are -1.0 or -1 (old int)
		if(myJump.Angle < 0) { 
			entryAngle = "-1,0";
			entry_angle_value.Text = "-";
		} else {
			entryAngle = myJump.Angle.ToString();
			entry_angle_value.Text = entryAngle;
		}
	}
	
	//this disallows loops on radio actions	
	private bool toggleRaisesSignal = true;

	private void fillSingleLeg(string description) {
		jumps_single_leg.Show();
		entry_description.Sensitive = false;

		string [] d = description.Split(new char[] {' '});

		toggleRaisesSignal = false;
		
		switch(d[0]) {
			case "Vertical": 
					jumps_radiobutton_single_leg_mode_vertical.Active = true; 
					jumps_spinbutton_single_leg_distance.Sensitive = false;
					jumps_spinbutton_single_leg_distance.Value = 0;
					break;
			case "Horizontal": 
					jumps_radiobutton_single_leg_mode_horizontal.Active = true; 
					jumps_spinbutton_single_leg_distance.Sensitive = true;
					jumps_spinbutton_single_leg_distance.Value = Convert.ToInt32(d[4]);
					break;
			case "Lateral": 
					jumps_radiobutton_single_leg_mode_lateral.Active = true; 
					jumps_spinbutton_single_leg_distance.Sensitive = true;
					jumps_spinbutton_single_leg_distance.Value = Convert.ToInt32(d[4]);
					break;
		}
		switch(d[1]) {
			case "Right": jumps_radiobutton_single_leg_right.Active = true; break;
			case "Left": jumps_radiobutton_single_leg_left.Active = true; break;
		}
		switch(d[2]) {
			case "This": jumps_radiobutton_single_leg_dominance_this_limb.Active = true; break;
			case "Opposite": jumps_radiobutton_single_leg_dominance_opposite.Active = true; break;
			case "Unknown": jumps_radiobutton_single_leg_dominance_unknown.Active = true; break;
		}
		switch(d[3]) {
			case "This": jumps_radiobutton_single_leg_fall_this_limb.Active = true; break;
			case "Opposite": jumps_radiobutton_single_leg_fall_opposite.Active = true; break;
			case "Both": jumps_radiobutton_single_leg_fall_both.Active = true; break;
		}
			
		toggleRaisesSignal = true;
	}
	
	private void on_radio_single_leg_1_toggled(object o, EventArgs args) {
		if(toggleRaisesSignal) {
			string [] d = entry_description.Text.Split(new char[] {' '});
			if(jumps_radiobutton_single_leg_mode_vertical.Active) {
				d[0] = "Vertical";	
				d[4] = "0";	
			}
			else if(jumps_radiobutton_single_leg_mode_horizontal.Active)
				d[0] = "Horizontal";
			else
				d[0] = "Lateral";
			
			entry_description.Text = d[0] + " " + d[1] + " " + d[2] + " " + d[3] + " " + d[4];
			fillSingleLeg(entry_description.Text);
		}
	}

	private void on_radio_single_leg_2_toggled(object o, EventArgs args) {
		if(toggleRaisesSignal) {
			string [] d = entry_description.Text.Split(new char[] {' '});
			if(jumps_radiobutton_single_leg_right.Active)
				d[1] = "Right";	
			else
				d[1] = "Left";

			entry_description.Text = d[0] + " " + d[1] + " " + d[2] + " " + d[3] + " " + d[4];
			fillSingleLeg(entry_description.Text);
		}
	}

	private void on_radio_single_leg_3_toggled(object o, EventArgs args) {
		if(toggleRaisesSignal) {
			string [] d = entry_description.Text.Split(new char[] {' '});
			if(jumps_radiobutton_single_leg_dominance_this_limb.Active)
				d[2] = "This";	
			else if(jumps_radiobutton_single_leg_dominance_opposite.Active)
				d[2] = "Opposite";
			else
				d[2] = "Unknown";

			entry_description.Text = d[0] + " " + d[1] + " " + d[2] + " " + d[3] + " " + d[4];
			fillSingleLeg(entry_description.Text);
		}
	}

	private void on_radio_single_leg_4_toggled(object o, EventArgs args) {
		if(toggleRaisesSignal) {
			string [] d = entry_description.Text.Split(new char[] {' '});
			if(jumps_radiobutton_single_leg_fall_this_limb.Active)
				d[3] = "This";	
			else if(jumps_radiobutton_single_leg_fall_opposite.Active)
				d[3] = "Opposite";
			else
				d[3] = "Both";

			entry_description.Text = d[0] + " " + d[1] + " " + d[2] + " " + d[3] + " " + d[4];
			fillSingleLeg(entry_description.Text);
		}
	}

	private void on_spin_single_leg_changed(object o, EventArgs args) {
		if(toggleRaisesSignal) {
			string [] d = entry_description.Text.Split(new char[] {' '});

			d[4] = jumps_spinbutton_single_leg_distance.Value.ToString();

			entry_description.Text = d[0] + " " + d[1] + " " + d[2] + " " + d[3] + " " + d[4];
			fillSingleLeg(entry_description.Text);
		}
	}


	
	protected override void createSignal() {
		//only for jumps & runs
		combo_eventType.Changed += new EventHandler (on_combo_eventType_changed);
	}

	string weightOldStore = "0";
	private void on_combo_eventType_changed (object o, EventArgs args) {
		//if the distance of the new runType is fixed, put this distance
		//if not conserve the old
		JumpType myJumpType = new JumpType (UtilGtk.ComboGetActive(combo_eventType));

		if(myJumpType.Name == Constants.TakeOffName || myJumpType.Name == Constants.TakeOffWeightName) {
			entry_tv_value.Text = "0";
			entry_tv_value.Sensitive = false;
		} else 
			entry_tv_value.Sensitive = true;


		if(myJumpType.HasWeight) {
			if(weightOldStore != "0")
				entry_weight_value.Text = weightOldStore;

			entry_weight_value.Sensitive = true;
		} else {
			//store weight in a variable if needed
			if(entry_weight_value.Text != "0")
				weightOldStore = entry_weight_value.Text;

			entry_weight_value.Text = "0";
			entry_weight_value.Sensitive = false;
		}
		
		jumps_single_leg.Visible = myJumpType.Name == "slCMJ";
	}


	protected override void on_button_cancel_clicked (object o, EventArgs args)
	{
		EditJumpWindowBox.edit_event.Hide();
		EditJumpWindowBox = null;
	}
	
	protected override void on_delete_event (object o, DeleteEventArgs args)
	{
		EditJumpWindowBox.edit_event.Hide();
		EditJumpWindowBox = null;
	}
	
	protected override void hideWindow() {
		EditJumpWindowBox.edit_event.Hide();
		EditJumpWindowBox = null;
	}
	
	protected override void updateEvent(int eventID, int personID, string description) {
		//only for jump
		double jumpPercentWeightForNewPerson = updateWeight(personID, sessionID);
		
		SqliteJump.Update(eventID, UtilGtk.ComboGetActive(combo_eventType), entryTv, entryTc, entryFall, personID, jumpPercentWeightForNewPerson, description, Convert.ToDouble(entryAngle));
	}

	
	protected virtual double updateWeight(int personID, int mySessionID) {
		//only for jumps, jumpsRj
		//update the weight percent of jump if needed
		double jumpPercentWeightForNewPerson = 0;
		if(entryWeight != "0") {
			double oldPersonWeight = personWeight;

			double jumpWeightInKg = 0;
			if(weightPercentPreferred)
				jumpWeightInKg = Util.WeightFromPercentToKg(Convert.ToDouble(entryWeight), oldPersonWeight);
			else
				jumpWeightInKg = Convert.ToDouble(entryWeight);
			
			double newPersonWeight = SqlitePersonSession.SelectAttribute(false, personID, mySessionID, Constants.Weight); 
			//jumpPercentWeightForNewPerson = jumpWeightInKg * 100 / newPersonWeight; 
			jumpPercentWeightForNewPerson = Util.WeightFromKgToPercent(jumpWeightInKg, newPersonWeight); 
			Log.WriteLine(string.Format("oldPW: {0}, jWinKg {1}, newPW{2}, jWin%NewP{3}",
					oldPersonWeight, jumpWeightInKg, newPersonWeight, jumpPercentWeightForNewPerson));
		}

		return jumpPercentWeightForNewPerson;
	}
	

}

//--------------------------------------------------------
//---------------- EDIT JUMP RJ WIDGET -------------------
//--------------------------------------------------------

public class EditJumpRjWindow : EditJumpWindow
{
	static EditJumpRjWindow EditJumpRjWindowBox;

	EditJumpRjWindow (Gtk.Window parent) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "edit_event", null);
		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(edit_event);
	
		eventBigTypeString = Catalog.GetString("reactive jump");
	}

	static new public EditJumpRjWindow Show (Gtk.Window parent, Event myEvent, bool weightPercentPreferred, int pDN)
	{
		if (EditJumpRjWindowBox == null) {
			EditJumpRjWindowBox = new EditJumpRjWindow (parent);
		}

		EditJumpRjWindowBox.weightPercentPreferred = weightPercentPreferred;
		EditJumpRjWindowBox.personWeight = SqlitePersonSession.SelectAttribute(
				false, myEvent.PersonID, myEvent.SessionID, Constants.Weight); 

		EditJumpRjWindowBox.pDN = pDN;
		
		EditJumpRjWindowBox.sessionID = myEvent.SessionID;

		EditJumpRjWindowBox.initializeValues();

		EditJumpRjWindowBox.fillDialog (myEvent);
		
		EditJumpRjWindowBox.edit_event.Show ();

		return EditJumpRjWindowBox;
	}
	
	protected override void initializeValues () {
		typeOfTest = Constants.TestTypes.JUMP_RJ;
		showType = true;
		showRunStart = false;
		showTv = false;
		showTc = false;
		showFall = true;
		showDistance = false;
		showTime = false;
		showSpeed = false;
		showWeight = true;
		showLimited = true;
		showMistakes = false;
		
		if(weightPercentPreferred)
			label_weight_units.Text = "%";
		else
			label_weight_units.Text = "Kg";
	}

	protected override string [] findTypes(Event myEvent) {
		//type cannot change on jumpRj
		combo_eventType.Sensitive=false;

		string [] myTypes;
		myTypes = SqliteJumpType.SelectJumpRjTypes("", true); //don't show allJumpsName row, only select name
		return myTypes;
	}

	protected override void fillWeight(Event myEvent) {
		JumpRj myJump = (JumpRj) myEvent;
		if(myJump.TypeHasWeight) {
			if(weightPercentPreferred)
				entryWeight = myJump.Weight.ToString();
			else
				entryWeight = Util.WeightFromPercentToKg(myJump.Weight, personWeight).ToString();

			entry_weight_value.Text = entryWeight;
			entry_weight_value.Sensitive = true;
		} else {
			entry_weight_value.Sensitive = false;
		}
	}

	protected override void fillFall(Event myEvent) {
		JumpRj myJump = (JumpRj) myEvent;
		entryFall = myJump.Fall.ToString();
		entry_fall_value.Text = entryFall;
	}

	protected override void fillLimited(Event myEvent) {
		JumpRj myJumpRj = (JumpRj) myEvent;
		label_limited_value.Text = Util.GetLimitedRounded(myJumpRj.Limited, pDN);
	}


	protected override void on_button_cancel_clicked (object o, EventArgs args)
	{
		EditJumpRjWindowBox.edit_event.Hide();
		EditJumpRjWindowBox = null;
	}
	
	protected override void on_delete_event (object o, DeleteEventArgs args)
	{
		EditJumpRjWindowBox.edit_event.Hide();
		EditJumpRjWindowBox = null;
	}
	
	protected override void hideWindow() {
		EditJumpRjWindowBox.edit_event.Hide();
		EditJumpRjWindowBox = null;
	}
	
	protected override void updateEvent(int eventID, int personID, string description) {
		//only for jumps
		double jumpPercentWeightForNewPerson = updateWeight(personID, sessionID);
		
		SqliteJumpRj.Update(eventID, personID, entryFall, jumpPercentWeightForNewPerson, description);
	}
}


//--------------------------------------------------------
//---------------- Repair jumpRJ WIDGET ------------------
//--------------------------------------------------------

public class RepairJumpRjWindow 
{
	[Widget] Gtk.Window repair_sub_event;
	[Widget] Gtk.Label label_header;
	[Widget] Gtk.Label label_totaltime_value;
	[Widget] Gtk.TreeView treeview_subevents;
	private TreeStore store;
	[Widget] Gtk.Button button_accept;
	[Widget] Gtk.Button button_add_before;
	[Widget] Gtk.Button button_add_after;
	[Widget] Gtk.Button button_delete;
	[Widget] Gtk.TextView textview1;

	static RepairJumpRjWindow RepairJumpRjWindowBox;
	Gtk.Window parent;
	//int pDN;

	JumpType jumpType;
	JumpRj jumpRj; //used on button_accept
	

	RepairJumpRjWindow (Gtk.Window parent, JumpRj myJump, int pDN) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "repair_sub_event", null);
		gladeXML.Autoconnect(this);
	
		//put an icon to window
		UtilGtk.IconWindow(repair_sub_event);
	
		this.parent = parent;
		this.jumpRj = myJump;

		//this.pDN = pDN;
	
		repair_sub_event.Title = Catalog.GetString("Repair reactive jump");
		
		System.Globalization.NumberFormatInfo localeInfo = new System.Globalization.NumberFormatInfo();
		localeInfo = System.Globalization.NumberFormatInfo.CurrentInfo;
		label_header.Text = string.Format(Catalog.GetString("Use this window to repair this test.\nDouble clic any cell to edit it (decimal separator: '{0}')"), localeInfo.NumberDecimalSeparator);
	
		
		jumpType = SqliteJumpType.SelectAndReturnJumpRjType(myJump.Type, false);
		
		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.Text = createTextForTextView(jumpType);
		textview1.Buffer = tb;
		
		createTreeView(treeview_subevents);
		//count, tc, tv
		store = new TreeStore(typeof (string), typeof (string), typeof(string));
		treeview_subevents.Model = store;
		fillTreeView (treeview_subevents, store, myJump, pDN);
	
		button_add_before.Sensitive = false;
		button_add_after.Sensitive = false;
		button_delete.Sensitive = false;
		
		label_totaltime_value.Text = getTotalTime().ToString() + " " + Catalog.GetString("seconds");
		
		treeview_subevents.Selection.Changed += onSelectionEntry;
	}
	
	static public RepairJumpRjWindow Show (Gtk.Window parent, JumpRj myJump, int pDN)
	{
		//Log.WriteLine(myJump);
		if (RepairJumpRjWindowBox == null) {
			RepairJumpRjWindowBox = new RepairJumpRjWindow (parent, myJump, pDN);
		}
		
		RepairJumpRjWindowBox.repair_sub_event.Show ();

		return RepairJumpRjWindowBox;
	}
	
	private string createTextForTextView (JumpType myJumpType) {
		string jumpTypeString = string.Format(Catalog.GetString(
					"JumpType: {0}."), myJumpType.Name);

		//if it's a jump type that starts in, then don't allow first TC be different than -1
		string startString = "";
		if(myJumpType.StartIn) {
			startString = string.Format(Catalog.GetString("\nThis jump type starts inside, the first time should be a flight time."));
		}

		string fixedString = "";
		if(myJumpType.FixedValue > 0) {
			if(myJumpType.JumpsLimited) {
				//if it's a jump type jumpsLimited with a fixed value, then don't allow the creation of more jumps, and respect the -1 at last TF if found
				fixedString = "\n" + string.Format(
						Catalog.GetPluralString(
							"This jump type is fixed to one jump.",
							"This jump type is fixed to {0} jumps.",
							Convert.ToInt32(myJumpType.FixedValue)),
						myJumpType.FixedValue) +
					Catalog.GetString("You cannot add more.");
			} else {
				//if it's a jump type timeLimited with a fixed value, then complain when the total time is higher
				fixedString = "\n" + string.Format(
						Catalog.GetPluralString(
							"This jump type is fixed to one second.",
							"This jump type is fixed to {0} seconds.",
							Convert.ToInt32(myJumpType.FixedValue)),
						myJumpType.FixedValue) +
					Catalog.GetString("You cannot add more.");
			}
		}
		return jumpTypeString + startString + fixedString;
	}

	
	private void createTreeView (Gtk.TreeView myTreeView) {
		myTreeView.HeadersVisible=true;
		int count = 0;

		myTreeView.AppendColumn ( Catalog.GetString ("Count"), new CellRendererText(), "text", count++);

		Gtk.TreeViewColumn tcColumn = new Gtk.TreeViewColumn ();
		tcColumn.Title = Catalog.GetString("TC");
		Gtk.CellRendererText tcCell = new Gtk.CellRendererText ();
		tcCell.Editable = true;
		tcCell.Edited += tcCellEdited;
		tcColumn.PackStart (tcCell, true);
		tcColumn.AddAttribute(tcCell, "text", count ++);
		myTreeView.AppendColumn ( tcColumn );
		
		Gtk.TreeViewColumn tvColumn = new Gtk.TreeViewColumn ();
		tvColumn.Title = Catalog.GetString("TF");
		Gtk.CellRendererText tvCell = new Gtk.CellRendererText ();
		tvCell.Editable = true;
		tvCell.Edited += tvCellEdited;
		tvColumn.PackStart (tvCell, true);
		tvColumn.AddAttribute(tvCell, "text", count ++);
		myTreeView.AppendColumn ( tvColumn );
	}
	
	private void tcCellEdited (object o, Gtk.EditedArgs args)
	{
		Gtk.TreeIter iter;
		store.GetIter (out iter, new Gtk.TreePath (args.Path));
		if(Util.IsNumber(args.NewText, true) && (string) treeview_subevents.Model.GetValue(iter,1) != "-1") {
			//if it's limited by fixed value of seconds
			//and new seconds are bigger than allowed, return
			if(jumpType.FixedValue > 0 && ! jumpType.JumpsLimited &&
					getTotalTime() //current total time in treeview
					- Convert.ToDouble((string) treeview_subevents.Model.GetValue(iter,1)) //-old cell
					+ Convert.ToDouble(args.NewText) //+new cell
					> jumpType.FixedValue) {	//bigger than allowed
				return;
			} else {
				store.SetValue(iter, 1, args.NewText);

				//update the totaltime label
				label_totaltime_value.Text = getTotalTime().ToString() + " " + Catalog.GetString("seconds");
			}
		}
		
		//if is not number or if it was -1, the old data will remain
	}

	private void tvCellEdited (object o, Gtk.EditedArgs args)
	{
		Gtk.TreeIter iter;
		store.GetIter (out iter, new Gtk.TreePath (args.Path));
		if(Util.IsNumber(args.NewText, true) && (string) treeview_subevents.Model.GetValue(iter,2) != "-1") {
			//if it's limited by fixed value of seconds
			//and new seconds are bigger than allowed, return
			if(jumpType.FixedValue > 0 && ! jumpType.JumpsLimited &&
					getTotalTime() //current total time in treeview
					- Convert.ToDouble((string) treeview_subevents.Model.GetValue(iter,2)) //-old cell
					+ Convert.ToDouble(args.NewText) //+new cell
					> jumpType.FixedValue) {	//bigger than allowed
				return;
			} else {
				store.SetValue(iter, 2, args.NewText);
				
				//update the totaltime label
				label_totaltime_value.Text = getTotalTime().ToString() + " " + Catalog.GetString("seconds");
			}
		}
		//if is not number or if it was -1, the old data will remain
	}

	private double getTotalTime() {
		TreeIter myIter;
		double totalTime = 0;
		bool iterOk = store.GetIterFirst (out myIter);
		string stringTc = "";
		string stringTv = "";
		if(iterOk) {
			do {
				//be cautious because when there's no value (like first tc in a jump that starts in,
				//it's stored as "-1", but it's shown as "-"
				stringTc = (string) treeview_subevents.Model.GetValue(myIter, 1);
				if(stringTc != "-" && stringTc != "-1") 
					totalTime += Convert.ToDouble(stringTc);
				
				stringTv = (string) treeview_subevents.Model.GetValue(myIter, 2);
				if(stringTv != "-" && stringTv != "-1") 
					totalTime += Convert.ToDouble(stringTv);
				
			} while (store.IterNext (ref myIter));
		}
		return totalTime;
	}
	
	private void fillTreeView (Gtk.TreeView tv, TreeStore store, JumpRj myJump, int pDN)
	{
		if(myJump.TcString.Length > 0 && myJump.TvString.Length > 0) {
			string [] tcArray = myJump.TcString.Split(new char[] {'='});
			string [] tvArray = myJump.TvString.Split(new char[] {'='});

			int count = 0;
			foreach (string myTc in tcArray) {
				string myTv;
				if(tvArray.Length >= count)
					myTv = Util.TrimDecimals(tvArray[count], pDN);
				else
					myTv = "";

				store.AppendValues ( (count+1).ToString(), Util.TrimDecimals(myTc, pDN), myTv );

				count ++;
			}
		}
	}

	void onSelectionEntry (object o, EventArgs args) {
		TreeModel model;
		TreeIter iter;
		
		if (((TreeSelection)o).GetSelected(out model, out iter)) {
			button_add_before.Sensitive = true;
			button_add_after.Sensitive = true;
			button_delete.Sensitive = true;

			//don't allow to add a row before the first if first row has a -1 in 'TC'
			//also don't allow deleting
			if((string) model.GetValue (iter, 1) == "-1") {
				button_add_before.Sensitive = false;
				button_delete.Sensitive = false;
			}

			//don't allow to add a row after the last if it has a -1
			//also don't allow deleting
			//the only -1 in flight time can be in the last row
			if((string) model.GetValue (iter, 2) == "-1") {
				button_add_after.Sensitive = false;
				button_delete.Sensitive = false;
			}
			
			//don't allow to add a row before or after 
			//if the jump type is fixed to n jumps and we reached n
			if(jumpType.FixedValue > 0 && jumpType.JumpsLimited) {
				int lastRow = 0;
				do {
					lastRow = Convert.ToInt32 ((string) model.GetValue (iter, 0));
				} while (store.IterNext (ref iter));

				//don't allow if max rows reached
				if(lastRow == jumpType.FixedValue ||
						( lastRow == jumpType.FixedValue +1 && jumpType.StartIn) ) {
					button_add_before.Sensitive = false;
					button_add_after.Sensitive = false;
				}
			}
		}
	}

	void on_button_add_before_clicked (object o, EventArgs args) {
		TreeModel model; 
		TreeIter iter; 
		if (treeview_subevents.Selection.GetSelected (out model, out iter)) {
			int position = Convert.ToInt32( (string) model.GetValue (iter, 0) ) -1; //count starts at '0'
			iter = store.InsertNode(position);
			store.SetValue(iter, 1, "0");
			store.SetValue(iter, 2, "0");
			putRowNumbers(store);
		}
	}
	
	void on_button_add_after_clicked (object o, EventArgs args) {
		TreeModel model; 
		TreeIter iter; 
		if (treeview_subevents.Selection.GetSelected (out model, out iter)) {
			int position = Convert.ToInt32( (string) model.GetValue (iter, 0) ); //count starts at '0'
			iter = store.InsertNode(position);
			store.SetValue(iter, 1, "0");
			store.SetValue(iter, 2, "0");
			putRowNumbers(store);
		}
	}
	
	private void putRowNumbers(TreeStore myStore) {
		TreeIter myIter;
		bool iterOk = myStore.GetIterFirst (out myIter);
		if(iterOk) {
			int count = 1;
			do {
				store.SetValue(myIter, 0, (count++).ToString());
			} while (myStore.IterNext (ref myIter));
		}
	}
		
	void on_button_delete_clicked (object o, EventArgs args) {
		TreeModel model; 
		TreeIter iter; 
		if (treeview_subevents.Selection.GetSelected (out model, out iter)) {
			store.Remove(ref iter);
			putRowNumbers(store);
		
			label_totaltime_value.Text = getTotalTime().ToString() + " " + Catalog.GetString("seconds");

			button_add_before.Sensitive = false;
			button_add_after.Sensitive = false;
			button_delete.Sensitive = false;
		}
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		//foreach all lines... extrac tcString and tvString
		TreeIter myIter;
		string tcString = "";
		string tvString = "";
		
		bool iterOk = store.GetIterFirst (out myIter);
		if(iterOk) {
			string equal= ""; //first iteration should not appear '='
			do {
				tcString = tcString + equal + (string) treeview_subevents.Model.GetValue (myIter, 1);
				tvString = tvString + equal + (string) treeview_subevents.Model.GetValue (myIter, 2);
				equal = "=";
			} while (store.IterNext (ref myIter));
		}
		
		jumpRj.TvString = tvString;	
		jumpRj.TcString = tcString;	
		jumpRj.Jumps = Util.GetNumberOfJumps(tvString, false);
		jumpRj.Time = Util.GetTotalTime(tcString, tvString);

		//calculate other variables needed for jumpRj creation
		
		if(jumpType.FixedValue > 0) {
			//if this jumpType has a fixed value of jumps or time, limitstring has not changed
			if(jumpType.JumpsLimited) {
				jumpRj.Limited = jumpType.FixedValue.ToString() + "J";
			} else {
				jumpRj.Limited = jumpType.FixedValue.ToString() + "T";
			}
		} else {
			//else limitstring should be calculated
			if(jumpType.JumpsLimited) {
				jumpRj.Limited = jumpRj.Jumps.ToString() + "J";
			} else {
				jumpRj.Limited = Util.GetTotalTime(tcString, tvString) + "T";
			}
		}

		//save it deleting the old first for having the same uniqueID
		Sqlite.Delete(Constants.JumpRjTable, jumpRj.UniqueID);
		jumpRj.InsertAtDB(false, Constants.JumpRjTable); 
		/*
		SqliteJump.InsertRj("jumpRj", jumpRj.UniqueID.ToString(), jumpRj.PersonID, jumpRj.SessionID, 
				jumpRj.Type, Util.GetMax(tvString), Util.GetMax(tcString), 
				jumpRj.Fall, jumpRj.Weight, jumpRj.Description,
				Util.GetAverage(tvString), Util.GetAverage(tcString),
				tvString, tcString,
				jumps, Util.GetTotalTime(tcString, tvString), limitString
				);
				*/

		//close the window
		RepairJumpRjWindowBox.repair_sub_event.Hide();
		RepairJumpRjWindowBox = null;
	}

	void on_button_cancel_clicked (object o, EventArgs args)
	{
		RepairJumpRjWindowBox.repair_sub_event.Hide();
		RepairJumpRjWindowBox = null;
	}
	
	void on_delete_event (object o, DeleteEventArgs args)
	{
		RepairJumpRjWindowBox.repair_sub_event.Hide();
		RepairJumpRjWindowBox = null;
	}
	
	public Button Button_accept 
	{
		set { button_accept = value;	}
		get { return button_accept;	}
	}

}

//--------------------------------------------------------
//---------------- jump extra WIDGET --------------------
//---------------- in 0.9.3 included in main gui ---------
//--------------------------------------------------------

partial class ChronoJumpWindow
{
	//options jumps
	[Widget] Gtk.SpinButton extra_window_jumps_spinbutton_weight;
	[Widget] Gtk.SpinButton extra_window_jumps_spinbutton_fall;
	[Widget] Gtk.RadioButton extra_window_jumps_radiobutton_kg;
	[Widget] Gtk.RadioButton extra_window_jumps_radiobutton_weight;
	[Widget] Gtk.Label extra_window_jumps_label_weight;
	[Widget] Gtk.Label extra_window_jumps_label_fall;
	[Widget] Gtk.Label extra_window_jumps_label_cm;
	[Widget] Gtk.Label extra_window_jumps_label_dj_arms;
	[Widget] Gtk.CheckButton extra_window_jumps_check_dj_arms;
	[Widget] Gtk.Label extra_window_label_jumps_no_options;
	
	[Widget] Gtk.Box vbox_extra_window_jumps_single_leg;
	[Widget] Gtk.RadioButton extra_window_jumps_radiobutton_single_leg_mode_vertical;
	[Widget] Gtk.RadioButton extra_window_jumps_radiobutton_single_leg_mode_horizontal;
	[Widget] Gtk.RadioButton extra_window_jumps_radiobutton_single_leg_mode_lateral;
	[Widget] Gtk.RadioButton extra_window_jumps_radiobutton_single_leg_right;
	[Widget] Gtk.RadioButton extra_window_jumps_radiobutton_single_leg_left;
	[Widget] Gtk.RadioButton extra_window_jumps_radiobutton_single_leg_dominance_this_limb;
	[Widget] Gtk.RadioButton extra_window_jumps_radiobutton_single_leg_dominance_opposite;
	[Widget] Gtk.RadioButton extra_window_jumps_radiobutton_single_leg_dominance_unknown;
	[Widget] Gtk.RadioButton extra_window_jumps_radiobutton_single_leg_fall_this_limb;
	[Widget] Gtk.RadioButton extra_window_jumps_radiobutton_single_leg_fall_opposite;
	[Widget] Gtk.RadioButton extra_window_jumps_radiobutton_single_leg_fall_both;
	
	//options jumps_rj
	[Widget] Gtk.Label extra_window_jumps_rj_label_limit;
	[Widget] Gtk.SpinButton extra_window_jumps_rj_spinbutton_limit;
	[Widget] Gtk.Label extra_window_jumps_rj_label_limit_units;
	[Widget] Gtk.SpinButton extra_window_jumps_rj_spinbutton_weight;
	[Widget] Gtk.SpinButton extra_window_jumps_rj_spinbutton_fall;
	[Widget] Gtk.RadioButton extra_window_jumps_rj_radiobutton_kg;
	[Widget] Gtk.RadioButton extra_window_jumps_rj_radiobutton_weight;
	[Widget] Gtk.Label extra_window_jumps_rj_label_weight;
	[Widget] Gtk.Label extra_window_jumps_rj_label_fall;
	[Widget] Gtk.Label extra_window_jumps_rj_label_cm;
	[Widget] Gtk.Label extra_window_label_jumps_rj_no_options;

	//labels notebook_execute	
	[Widget] Gtk.Label label_extra_window_radio_jump_free;
	[Widget] Gtk.Label label_extra_window_radio_jump_sj;
	[Widget] Gtk.Label label_extra_window_radio_jump_sjl;
	[Widget] Gtk.Label label_extra_window_radio_jump_cmj;
	[Widget] Gtk.Label label_extra_window_radio_jump_cmjl;
	[Widget] Gtk.Label label_extra_window_radio_jump_slcmj;
	[Widget] Gtk.Label label_extra_window_radio_jump_abk;
	[Widget] Gtk.Label label_extra_window_radio_jump_dj;
	[Widget] Gtk.Label label_extra_window_radio_jump_rocket;
	[Widget] Gtk.Label label_extra_window_radio_jump_takeoff;
	[Widget] Gtk.Label label_extra_window_radio_jump_more;

	//radio notebook_execute	
	[Widget] Gtk.RadioButton extra_window_radio_jump_free;
	[Widget] Gtk.RadioButton extra_window_radio_jump_sj;
	[Widget] Gtk.RadioButton extra_window_radio_jump_sjl;
	[Widget] Gtk.RadioButton extra_window_radio_jump_cmj;
	[Widget] Gtk.RadioButton extra_window_radio_jump_cmjl;
	[Widget] Gtk.RadioButton extra_window_radio_jump_slcmj;
	[Widget] Gtk.RadioButton extra_window_radio_jump_abk;
	[Widget] Gtk.RadioButton extra_window_radio_jump_dj;
	[Widget] Gtk.RadioButton extra_window_radio_jump_rocket;
	[Widget] Gtk.RadioButton extra_window_radio_jump_takeoff;
	[Widget] Gtk.RadioButton extra_window_radio_jump_more;
	
	[Widget] Gtk.Label label_extra_window_radio_jump_rj_j;
	[Widget] Gtk.Label label_extra_window_radio_jump_rj_t;
	[Widget] Gtk.Label label_extra_window_radio_jump_rj_unlimited;
	[Widget] Gtk.Label label_extra_window_radio_jump_rj_hexagon;
	[Widget] Gtk.Label label_extra_window_radio_jump_rj_more;

	[Widget] Gtk.RadioButton extra_window_radio_jump_rj_j;
	[Widget] Gtk.RadioButton extra_window_radio_jump_rj_t;
	[Widget] Gtk.RadioButton extra_window_radio_jump_rj_unlimited;
	[Widget] Gtk.RadioButton extra_window_radio_jump_rj_hexagon;
	[Widget] Gtk.RadioButton extra_window_radio_jump_rj_more;

	//selected test labels	
	[Widget] Gtk.Label extra_window_jumps_label_selected;
	[Widget] Gtk.Label extra_window_jumps_rj_label_selected;
	
	//for RunAnalysis
	//but will be used and recorded with "fall"
	//static double distance;

	//jumps
	string extra_window_jumps_option = "Kg";
	double extra_window_jumps_weight = 20;
	double extra_window_jumps_fall = 20;
	bool extra_window_jumps_arms = false;
	
	//jumps_rj
	double extra_window_jumps_rj_limited = 10;
	bool extra_window_jumps_rj_jumpsLimited;
	string extra_window_jumps_rj_option = "Kg";
	double extra_window_jumps_rj_weight = 20;
	double extra_window_jumps_rj_fall = 20;
	
	private JumpType previousJumpType; //used on More to turnback if cancel or delete event is pressed
	private JumpType previousJumpRjType; //used on More to turnback if cancel or delete event is pressed
	
	private void on_extra_window_jumps_test_changed(object o, EventArgs args)
	{
		if(extra_window_radio_jump_free.Active) currentJumpType = new JumpType("Free");
		else if(extra_window_radio_jump_sj.Active) currentJumpType = new JumpType("SJ");
		else if(extra_window_radio_jump_sjl.Active) currentJumpType = new JumpType("SJl");
		else if(extra_window_radio_jump_cmj.Active) currentJumpType = new JumpType("CMJ");
		else if(extra_window_radio_jump_cmjl.Active) currentJumpType = new JumpType("CMJl");
		else if(extra_window_radio_jump_slcmj.Active) currentJumpType = new JumpType("slCMJ");
		else if(extra_window_radio_jump_abk.Active) currentJumpType = new JumpType("ABK");
		else if(extra_window_radio_jump_dj.Active) currentJumpType = new JumpType("DJ");
		else if(extra_window_radio_jump_rocket.Active) currentJumpType = new JumpType("Rocket");
		else if(extra_window_radio_jump_takeoff.Active) currentJumpType = new JumpType(Constants.TakeOffName);

		extra_window_jumps_initialize(currentJumpType);
	}
	
	private void on_extra_window_jumps_more(object o, EventArgs args)
	{
		previousJumpType = currentJumpType;

		if(extra_window_radio_jump_more.Active) {
			jumpsMoreWin = JumpsMoreWindow.Show(app1, true);
			jumpsMoreWin.Button_accept.Clicked += new EventHandler(on_more_jumps_accepted);
			jumpsMoreWin.Button_cancel.Clicked += new EventHandler(on_more_jumps_cancelled);
			jumpsMoreWin.Button_selected.Clicked += new EventHandler(on_more_jumps_draw_image_test);
		}
	}
	
	private void on_extra_window_jumps_rj_test_changed(object o, EventArgs args)
	{
		if(extra_window_radio_jump_rj_j.Active) currentJumpRjType = new JumpType("RJ(j)");
		else if(extra_window_radio_jump_rj_t.Active) currentJumpRjType = new JumpType("RJ(t)");
		else if(extra_window_radio_jump_rj_unlimited.Active) currentJumpRjType = new JumpType("RJ(unlimited)");
		else if(extra_window_radio_jump_rj_hexagon.Active) currentJumpRjType = new JumpType("RJ(hexagon)");

		extra_window_jumps_rj_initialize(currentJumpRjType);
	}

	private void on_extra_window_jumps_rj_more(object o, EventArgs args) 
	{
		previousJumpRjType = currentJumpRjType;

		if(extra_window_radio_jump_rj_more.Active) {
			jumpsRjMoreWin = JumpsRjMoreWindow.Show(app1, true);
			jumpsRjMoreWin.Button_accept.Clicked += new EventHandler(on_more_jumps_rj_accepted);
			jumpsRjMoreWin.Button_cancel.Clicked += new EventHandler(on_more_jumps_rj_cancelled);
			jumpsRjMoreWin.Button_selected.Clicked += new EventHandler(on_more_jumps_rj_draw_image_test);
		}
	}


	private void extra_window_jumps_initialize(JumpType myJumpType) 
	{
		extra_window_jumps_label_selected.Text = "<b>" + Catalog.GetString(myJumpType.Name) + "</b>";
		extra_window_jumps_label_selected.UseMarkup = true; 
		currentEventType = myJumpType;
		changeTestImage(EventType.Types.JUMP.ToString(), myJumpType.Name, myJumpType.ImageFileName);
		bool hasOptions = false;
	
		if(myJumpType.HasWeight) {
			hasOptions = true;
			extra_window_showWeightData(myJumpType, true);	
		} else 
			extra_window_showWeightData(myJumpType, false);	

		if(myJumpType.StartIn || myJumpType.Name == Constants.TakeOffName || 
				myJumpType.Name == Constants.TakeOffWeightName) 
			extra_window_showFallData(myJumpType, false);	
		else {
			hasOptions = true;
			extra_window_showFallData(myJumpType, true);
		}
		
		if(myJumpType.Name == "DJa" || myJumpType.Name == "DJna") { 
			//on DJa and DJna (coming from More jumps) need to show technique data but not change
			if(myJumpType.Name == "DJa")
				extra_window_jumps_check_dj_arms.Active = true;
			else //myJumpType.Name == "DJna"
				extra_window_jumps_check_dj_arms.Active = false;

			hasOptions = true;
			extra_window_showTechniqueArmsData(true, false); //visible, sensitive
		} else if(myJumpType.Name == "DJ") { 
			//user has pressed DJ button
			hasOptions = true;
			extra_window_jumps_check_dj_arms.Active = extra_window_jumps_arms;

			on_extra_window_jumps_check_dj_arms_clicked(new object(), new EventArgs());
			extra_window_showTechniqueArmsData(true, true); //visible, sensitive
		} else 
			extra_window_showTechniqueArmsData(false, false); //visible, sensitive
		
		extra_window_jumps_spinbutton_weight.Value = extra_window_jumps_weight;
		extra_window_jumps_spinbutton_fall.Value = extra_window_jumps_fall;
		if (extra_window_jumps_option == "Kg") {
			extra_window_jumps_radiobutton_kg.Active = true;
		} else {
			extra_window_jumps_radiobutton_weight.Active = true;
		}

		extra_window_showSingleLegStuff(myJumpType.Name == "slCMJ");
		if(myJumpType.Name == "slCMJ")
			hasOptions = true;

		extra_window_jumps_showNoOptions(myJumpType, hasOptions);
	}
	
	private void extra_window_jumps_rj_initialize(JumpType myJumpType) 
	{
		extra_window_jumps_rj_label_selected.Text = "<b>" + Catalog.GetString(myJumpType.Name) + "</b>";
		extra_window_jumps_rj_label_selected.UseMarkup = true; 
		currentEventType = myJumpType;
		changeTestImage(EventType.Types.JUMP.ToString(), myJumpType.Name, myJumpType.ImageFileName);
		bool hasOptions = false;
	
		if(myJumpType.FixedValue >= 0) {
			hasOptions = true;
			string jumpsName = Catalog.GetString("jumps");
			string secondsName = Catalog.GetString("seconds");
			if(myJumpType.JumpsLimited) {
				extra_window_jumps_rj_jumpsLimited = true;
				extra_window_jumps_rj_label_limit_units.Text = jumpsName;
			} else {
				extra_window_jumps_rj_jumpsLimited = false;
				extra_window_jumps_rj_label_limit_units.Text = secondsName;
			}
			if(myJumpType.FixedValue > 0) {
				extra_window_jumps_rj_spinbutton_limit.Sensitive = false;
				extra_window_jumps_rj_spinbutton_limit.Value = myJumpType.FixedValue;
			} else {
				extra_window_jumps_rj_spinbutton_limit.Sensitive = true;
				extra_window_jumps_rj_spinbutton_limit.Value = extra_window_jumps_rj_limited;
			}
			extra_window_showLimitData (true);
		} else  //unlimited
			extra_window_showLimitData (false);

		if(myJumpType.HasWeight) {
			hasOptions = true;
			extra_window_showWeightData(myJumpType, true);	
		} else 
			extra_window_showWeightData(myJumpType, false);	

		if(myJumpType.StartIn || myJumpType.Name == Constants.TakeOffName || 
				myJumpType.Name == Constants.TakeOffWeightName)
			extra_window_showFallData(myJumpType, false);	
		else {
			extra_window_showFallData(myJumpType, true);	
			hasOptions = true;
		}
		
		extra_window_jumps_rj_spinbutton_weight.Value = extra_window_jumps_rj_weight;
		extra_window_jumps_rj_spinbutton_fall.Value = extra_window_jumps_rj_fall;
		if (extra_window_jumps_rj_option == "Kg") {
			extra_window_jumps_rj_radiobutton_kg.Active = true;
		} else {
			extra_window_jumps_rj_radiobutton_weight.Active = true;
		}

		extra_window_jumps_showNoOptions(myJumpType, hasOptions);
	}

	private void on_extra_window_jumps_check_dj_arms_clicked(object o, EventArgs args)
	{
		JumpType j = new JumpType();
		if(extra_window_jumps_check_dj_arms.Active) 
			j = new JumpType("DJa");
		else
			j = new JumpType("DJna");

		changeTestImage(EventType.Types.JUMP.ToString(), j.Name, j.ImageFileName);
	}


	private void on_more_jumps_draw_image_test (object o, EventArgs args) {
		currentEventType = new JumpType(jumpsMoreWin.SelectedEventName);
		changeTestImage(currentEventType.Type.ToString(), currentEventType.Name, currentEventType.ImageFileName);
	}
	
	private void on_more_jumps_rj_draw_image_test (object o, EventArgs args) {
		currentEventType = new JumpType(jumpsRjMoreWin.SelectedEventName);
		changeTestImage(currentEventType.Type.ToString(), currentEventType.Name, currentEventType.ImageFileName);
	}
	
	//used from the dialogue "jumps more"
	private void on_more_jumps_accepted (object o, EventArgs args) 
	{
		jumpsMoreWin.Button_accept.Clicked -= new EventHandler(on_more_jumps_accepted);
		
		currentJumpType = new JumpType(
				//jumpsMoreWin.SelectedJumpType,
				jumpsMoreWin.SelectedEventName, //type of jump
								//SelectedEventType would be: jump, or run, ...
				jumpsMoreWin.SelectedStartIn,
				jumpsMoreWin.SelectedExtraWeight,
				false,		//isRepetitive
				false,		//jumpsLimited (false, because is not repetitive)
				0,		//limitValue
				false,		//unlimited
				jumpsMoreWin.SelectedDescription,
				SqliteEvent.GraphLinkSelectFileName("jump", jumpsMoreWin.SelectedEventName)
				);
	
		extra_window_jumps_toggle_desired_button_on_toolbar(currentJumpType);
		
		//destroy the win for not having updating problems if a new jump type is created
		//jumpsMoreWin = null; //don't work
		jumpsMoreWin.Destroy(); //works ;)
	}
	
	//used from the dialogue "jumps rj more"
	private void on_more_jumps_rj_accepted (object o, EventArgs args) 
	{
		jumpsRjMoreWin.Button_accept.Clicked -= new EventHandler(on_more_jumps_rj_accepted);

		currentJumpRjType = new JumpType(
				//jumpsRjMoreWin.SelectedJumpType,
				jumpsRjMoreWin.SelectedEventName,
				jumpsRjMoreWin.SelectedStartIn,
				jumpsRjMoreWin.SelectedExtraWeight,
				true,		//isRepetitive
				jumpsRjMoreWin.SelectedLimited,
				jumpsRjMoreWin.SelectedLimitedValue,
				jumpsRjMoreWin.SelectedUnlimited,
				jumpsRjMoreWin.SelectedDescription,
				SqliteEvent.GraphLinkSelectFileName("jumpRj", jumpsRjMoreWin.SelectedEventName)
				);

		
		extra_window_jumps_rj_toggle_desired_button_on_toolbar(currentJumpRjType);
	
		//destroy the win for not having updating problems if a new jump type is created
		jumpsRjMoreWin.Destroy();
	}

	//if it's cancelled (or deleted event) select desired toolbar button
	private void on_more_jumps_cancelled (object o, EventArgs args) 
	{
		currentJumpType = previousJumpType;
		extra_window_jumps_toggle_desired_button_on_toolbar(currentJumpType);
	}
	
	private void on_more_jumps_rj_cancelled (object o, EventArgs args) 
	{
		currentJumpRjType = previousJumpRjType;
		extra_window_jumps_rj_toggle_desired_button_on_toolbar(currentJumpRjType);
	}
	
	private void extra_window_jumps_toggle_desired_button_on_toolbar(JumpType type) {
		if(type.Name == "Free") extra_window_radio_jump_free.Active = true;
		else if(type.Name == "SJ") extra_window_radio_jump_sj.Active = true;
		else if(type.Name == "SJl") extra_window_radio_jump_sjl.Active = true;
		else if(type.Name == "CMJ") extra_window_radio_jump_cmj.Active = true;
		else if(type.Name == "CMJl") extra_window_radio_jump_cmjl.Active = true;
		else if(type.Name == "slCMJ") extra_window_radio_jump_slcmj.Active = true;
		else if(type.Name == "ABK") extra_window_radio_jump_abk.Active = true;
//		else if(type.Name == "DJ") extra_window_radio_jump_dj.Active = true;
		else if(type.Name == "Rocket") extra_window_radio_jump_rocket.Active = true;
		else if(type.Name == Constants.TakeOffName) extra_window_radio_jump_takeoff.Active = true;
		else {
			//don't do this:
			//extra_window_radio_jump_more.Active = true;
			//because it will be a loop
			//only do:
			extra_window_jumps_initialize(type);
		}
	}

	private void extra_window_jumps_rj_toggle_desired_button_on_toolbar(JumpType type) {
		if(type.Name == "RJ(j)") extra_window_radio_jump_rj_j.Active = true;
		else if(type.Name == "RJ(t)") extra_window_radio_jump_rj_t.Active = true;
		else if(type.Name == "RJ(unlimited)") extra_window_radio_jump_rj_unlimited.Active = true;
		else if(type.Name == "RJ(hexagon)") extra_window_radio_jump_rj_hexagon.Active = true;
		else {
			//don't do this:
			//extra_window_radio_jump_more.Active = true;
			//because it will be a loop
			//only do:
			extra_window_jumps_rj_initialize(type);
		}
	}

	private void extra_window_showWeightData (JumpType myJumpType, bool show) {
		if(myJumpType.IsRepetitive) {
			extra_window_jumps_rj_label_weight.Visible = show;
			extra_window_jumps_rj_spinbutton_weight.Visible = show;
			extra_window_jumps_rj_radiobutton_kg.Visible = show;
			extra_window_jumps_rj_radiobutton_weight.Visible = show;
		} else {
			extra_window_jumps_label_weight.Visible = show;
			extra_window_jumps_spinbutton_weight.Visible = show;
			extra_window_jumps_radiobutton_kg.Visible = show;
			extra_window_jumps_radiobutton_weight.Visible = show;
		}
	}
	
	private void extra_window_showTechniqueArmsData (bool show, bool sensitive) {
		extra_window_jumps_label_dj_arms.Visible = show;
		extra_window_jumps_check_dj_arms.Visible = show;
		
		extra_window_jumps_label_dj_arms.Sensitive = sensitive;
		extra_window_jumps_check_dj_arms.Sensitive = sensitive;
	}
	
	private void extra_window_showFallData (JumpType myJumpType, bool show) {
		if(myJumpType.IsRepetitive) {
			extra_window_jumps_rj_label_fall.Visible = show;
			extra_window_jumps_rj_spinbutton_fall.Visible = show;
			extra_window_jumps_rj_label_cm.Visible = show;
		} else {
			extra_window_jumps_label_fall.Visible = show;
			extra_window_jumps_spinbutton_fall.Visible = show;
			extra_window_jumps_label_cm.Visible = show;
		}
	}
	
	private void extra_window_showLimitData (bool show) {
		extra_window_jumps_rj_label_limit.Visible = show;
		extra_window_jumps_rj_spinbutton_limit.Visible = show;
		extra_window_jumps_rj_label_limit_units.Visible = show;
	}
	
	private void extra_window_showSingleLegStuff(bool show) {
		vbox_extra_window_jumps_single_leg.Visible = show;
	}
			
	private void extra_window_jumps_showNoOptions(JumpType myJumpType, bool hasOptions) {
		if(myJumpType.IsRepetitive) 
			extra_window_label_jumps_rj_no_options.Visible = ! hasOptions;
		else 
			extra_window_label_jumps_no_options.Visible = ! hasOptions;
	}



	private void on_extra_window_jumps_radiobutton_kg_toggled (object o, EventArgs args)
	{
		extra_window_jumps_option = "Kg";
	}
	
	private void on_extra_window_jumps_radiobutton_weight_toggled (object o, EventArgs args)
	{
		extra_window_jumps_option = "%";
	}
	
	private void on_extra_window_jumps_rj_radiobutton_kg_toggled (object o, EventArgs args)
	{
		extra_window_jumps_rj_option = "Kg";
	}
	
	private void on_extra_window_jumps_rj_radiobutton_weight_toggled (object o, EventArgs args)
	{
		extra_window_jumps_rj_option = "%";
	}
	
	
	private string limitString()
	{
		if(extra_window_jumps_rj_jumpsLimited) 
			return extra_window_jumps_rj_limited.ToString() + "J";
		else 
			return extra_window_jumps_rj_limited.ToString() + "T";
	}
	
	//do not translate this
	private string slCMJString()
	{
		string str = "";
		if(extra_window_jumps_radiobutton_single_leg_mode_vertical.Active) str = "Vertical";
		else if(extra_window_jumps_radiobutton_single_leg_mode_horizontal.Active) str = "Horizontal";
		else str = "Lateral";
		
		if(extra_window_jumps_radiobutton_single_leg_right.Active) str += " Right";
		else str += " Left";
		
		if(extra_window_jumps_radiobutton_single_leg_dominance_this_limb.Active) str += " This";
		else if(extra_window_jumps_radiobutton_single_leg_dominance_opposite.Active) str += " Opposite";
		else str += " Unknown";

		if(extra_window_jumps_radiobutton_single_leg_fall_this_limb.Active) str += " This";
		else if(extra_window_jumps_radiobutton_single_leg_fall_opposite.Active) str += " Opposite";
		else str += " Both";

		return str;
	}

}


//--------------------------------------------------------
//---------------- jumps_more widget ---------------------
//--------------------------------------------------------

public class JumpsMoreWindow : EventMoreWindow
{
	[Widget] Gtk.Window jumps_runs_more;
	static JumpsMoreWindow JumpsMoreWindowBox;
	private bool selectedStartIn;
	private bool selectedExtraWeight;

	public JumpsMoreWindow (Gtk.Window parent, bool testOrDelete) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "jumps_runs_more", null);
		gladeXML.Autoconnect(this);
		this.parent = parent;
		this.testOrDelete = testOrDelete;
		
		if(!testOrDelete)
			jumps_runs_more.Title = Catalog.GetString("Delete test type defined by user");
		
		//put an icon to window
		UtilGtk.IconWindow(jumps_runs_more);
		
		selectedEventType = EventType.Types.JUMP.ToString();
		
		//name, startIn, weight, description
		store = new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string));

		initializeThings();
	}
	
	static public JumpsMoreWindow Show (Gtk.Window parent, bool testOrDelete)
	{
		if (JumpsMoreWindowBox == null) {
			JumpsMoreWindowBox = new JumpsMoreWindow (parent, testOrDelete);
		}
		JumpsMoreWindowBox.jumps_runs_more.Show ();
		
		return JumpsMoreWindowBox;
	}
	
	protected override void createTreeView (Gtk.TreeView tv) {
		tv.HeadersVisible=true;
		int count = 0;
		
		tv.AppendColumn ( Catalog.GetString ("Name"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Start inside"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Extra weight"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Description"), new CellRendererText(), "text", count++);
	}
	
	protected override void fillTreeView (Gtk.TreeView tv, TreeStore store) 
	{
		//select data without inserting an "all jumps", without filter, and not obtain only name of jump
		string [] myJumpTypes = SqliteJumpType.SelectJumpTypes("", "", false);
		foreach (string myType in myJumpTypes) {
			string [] myStringFull = myType.Split(new char[] {':'});
			if(myStringFull[2] == "1") {
				myStringFull[2] = Catalog.GetString("Yes");
			} else {
				myStringFull[2] = Catalog.GetString("No");
			}
			if(myStringFull[3] == "1") {
				myStringFull[3] = Catalog.GetString("Yes");
			} else {
				myStringFull[3] = Catalog.GetString("No");
			}

			JumpType tempType = new JumpType (myStringFull[1]);
			string description  = getDescriptionLocalised(tempType, myStringFull[4]);

			//if we are going to execute: show all types
			//if we are going to delete: show user defined types
			if(testOrDelete || ! tempType.IsPredefined)
				store.AppendValues (
						//myStringFull[0], //don't display de uniqueID
						myStringFull[1],	//name 
						myStringFull[2], 	//startIn
						myStringFull[3], 	//weight
						description
						);
		}	
	}

	protected override void onSelectionEntry (object o, EventArgs args) {
		TreeModel model;
		TreeIter iter;
		selectedEventName = "-1";
		selectedStartIn = false;
		selectedExtraWeight = false;
		selectedDescription = "";

		if (((TreeSelection)o).GetSelected(out model, out iter)) {
			selectedEventName = (string) model.GetValue (iter, 0);
			if( (string) model.GetValue (iter, 1) == Catalog.GetString("Yes") ) {
				selectedStartIn = true;
			}
			if( (string) model.GetValue (iter, 2) == Catalog.GetString("Yes") ) {
				selectedExtraWeight = true;
			}
			selectedDescription = (string) model.GetValue (iter, 3);

			if(testOrDelete) {
				button_accept.Sensitive = true;
				//update graph image test on main window
				button_selected.Click();
			} else
				button_delete_type.Sensitive = true;
		}
	}
	
	protected override void on_row_double_clicked (object o, Gtk.RowActivatedArgs args)
	{
		//return if we are to delete a test
		if(!testOrDelete)
			return;

		TreeView tv = (TreeView) o;
		TreeModel model;
		TreeIter iter;

		if (tv.Selection.GetSelected (out model, out iter)) {
			//put selection in selected
			selectedEventName = (string) model.GetValue (iter, 0);
			if( (string) model.GetValue (iter, 1) == Catalog.GetString("Yes") ) {
				selectedStartIn = true;
			}
			if( (string) model.GetValue (iter, 2) == Catalog.GetString("Yes") ) {
				selectedExtraWeight = true;
			}
			selectedDescription = (string) model.GetValue (iter, 3);

			//activate on_button_accept_clicked()
			button_accept.Activate();
		}
	}
	
	protected override void deleteTestLine() {
		SqliteJumpType.Delete(Constants.JumpTypeTable, selectedEventName, false);
	}

	protected override string [] findTestTypesInSessions() {
		return SqliteJump.SelectJumps(-1, -1, "", selectedEventName); 
	}
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		JumpsMoreWindowBox.jumps_runs_more.Hide();
		JumpsMoreWindowBox = null;
	}
	
	void on_jumps_runs_more_delete_event (object o, DeleteEventArgs args)
	{
		//raise signal
		button_cancel.Click();

		//JumpsMoreWindowBox.jumps_runs_more.Hide();
		//JumpsMoreWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		JumpsMoreWindowBox.jumps_runs_more.Hide();
	}
	
	//when a jump is done using jumpsMoreWindow, the accept doesn't destroy this instance, because 
	//later we need data from it.
	//This is used for destroying, then if a new jump type is added, it will be shown at first time clicking "more" button
	public void Destroy() {		
		JumpsMoreWindowBox = null;
	}
	
	public bool SelectedStartIn 
	{
		get {
			return selectedStartIn;
		}
	}
	
	public bool SelectedExtraWeight 
	{
		get {
			return selectedExtraWeight;
		}
	}
}

//--------------------------------------------------------
//---------------- jumps_rj_more widget ------------------
//--------------------------------------------------------

public class JumpsRjMoreWindow : EventMoreWindow 
{
	[Widget] Gtk.Window jumps_runs_more;
	static JumpsRjMoreWindow JumpsRjMoreWindowBox;
	
	private bool selectedStartIn;
	private bool selectedExtraWeight;
	private bool selectedLimited;
	private double selectedLimitedValue;
	private bool selectedUnlimited;
	
	public JumpsRjMoreWindow (Gtk.Window parent, bool testOrDelete) {
		//the glade window is the same as jumps_more
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "jumps_runs_more", null);
		gladeXML.Autoconnect(this);
		this.parent = parent;
		this.testOrDelete = testOrDelete;
		
		if(!testOrDelete)
			jumps_runs_more.Title = Catalog.GetString("Delete test type defined by user");
		
		//put an icon to window
		UtilGtk.IconWindow(jumps_runs_more);

		//if jumps_runs_more is opened to showing Rj jumpTypes make it wider
		jumps_runs_more.Resize(600,300);
		
		selectedEventType = EventType.Types.JUMP.ToString();

		//name, limited by, limited value, startIn, weight, description
		store = new TreeStore(typeof (string), typeof (string), typeof(string),
				typeof (string), typeof (string), typeof (string));
		
		initializeThings();
	}
	
	static public JumpsRjMoreWindow Show (Gtk.Window parent, bool testOrDelete)
	{
		if (JumpsRjMoreWindowBox == null) {
			JumpsRjMoreWindowBox = new JumpsRjMoreWindow (parent, testOrDelete);
		}
		JumpsRjMoreWindowBox.jumps_runs_more.Show ();
		
		return JumpsRjMoreWindowBox;
	}
	
	protected override void createTreeView (Gtk.TreeView tv) {
		tv.HeadersVisible=true;
		int count = 0;

		tv.AppendColumn ( Catalog.GetString ("Name"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Limited by"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Limited value"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Start inside"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Extra weight"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Description"), new CellRendererText(), "text", count++);
	}
	
	protected override void fillTreeView (Gtk.TreeView tv, TreeStore store) 
	{
		//select data without inserting an "all jumps", and not obtain only name of jump
		string [] myJumpTypes = SqliteJumpType.SelectJumpRjTypes("", false);
		foreach (string myType in myJumpTypes) {
			string [] myStringFull = myType.Split(new char[] {':'});
			if(myStringFull[2] == "1") {
				myStringFull[2] = Catalog.GetString("Yes");
			} else {
				myStringFull[2] = Catalog.GetString("No");
			}
			if(myStringFull[3] == "1") {
				myStringFull[3] = Catalog.GetString("Yes");
			} else {
				myStringFull[3] = Catalog.GetString("No");
			}
			//limited
			string myLimiter = "";
			string myLimiterValue = "";
			
			//check if it's unlimited
			if(myStringFull[5] == "-1") { //unlimited mark
				myLimiter= Catalog.GetString("Unlimited");
				myLimiterValue = "-";
			} else {
				myLimiter = Catalog.GetString("Jumps");
				if(myStringFull[4] == "0") {
					myLimiter = Catalog.GetString("Seconds");
				}
				myLimiterValue = "?";
				if(Convert.ToDouble(myStringFull[5]) > 0) {
					myLimiterValue = myStringFull[5];
				}
			}

			JumpType tempType = new JumpType (myStringFull[1]);
			string description  = getDescriptionLocalised(tempType, myStringFull[6]);

			//if we are going to execute: show all types
			//if we are going to delete: show user defined types
			if(testOrDelete || ! tempType.IsPredefined)
				store.AppendValues (
						//myStringFull[0], //don't display de uniqueID
						myStringFull[1],	//name 
						myLimiter,		//jumps or seconds		
						myLimiterValue,		//? or exact value
						myStringFull[2], 	//startIn
						myStringFull[3], 	//weight
						description
						);
		}	
	}

	protected override void onSelectionEntry (object o, EventArgs args)
	{
		TreeModel model;
		TreeIter iter;
		selectedEventName = "-1";
		selectedStartIn = false;
		selectedExtraWeight = false;
		selectedLimited = false;
		selectedLimitedValue = 0;
		selectedUnlimited = false; //true if it's an unlimited reactive jump
		selectedDescription = "";

		if (((TreeSelection)o).GetSelected(out model, out iter)) {
			selectedEventName = (string) model.GetValue (iter, 0);
			
			if( (string) model.GetValue (iter, 1) == Catalog.GetString("Unlimited") ) {
				selectedUnlimited = true;
				selectedLimited = true; //unlimited jumps will be limited by clicking on "finish"
							//and this will be always done by the user when
							//some jumps are done (not time like in runs)
			} 
			
			if( (string) model.GetValue (iter, 1) == Catalog.GetString("Jumps") ) {
				selectedLimited = true;
			}
			
			if( (string) model.GetValue (iter, 2) == "?")
				selectedLimitedValue = 0;
			else if( (string) model.GetValue (iter, 2) == "-") 
				selectedLimitedValue = -1.0;
			else 
				selectedLimitedValue = Convert.ToDouble( (string) model.GetValue (iter, 2) );

			if( (string) model.GetValue (iter, 3) == Catalog.GetString("Yes") ) {
				selectedStartIn = true;
			}
			if( (string) model.GetValue (iter, 4) == Catalog.GetString("Yes") ) {
				selectedExtraWeight = true;
			}
			selectedDescription = (string) model.GetValue (iter, 5);

			if(testOrDelete) {
				button_accept.Sensitive = true;
				//update graph image test on main window
				button_selected.Click();
			} else
				button_delete_type.Sensitive = true;
		}
	}
	
	protected override void on_row_double_clicked (object o, Gtk.RowActivatedArgs args)
	{
		//return if we are to delete a test
		if(!testOrDelete)
			return;

		TreeView tv = (TreeView) o;
		TreeModel model;
		TreeIter iter;

		if (tv.Selection.GetSelected (out model, out iter)) {
			selectedEventName = (string) model.GetValue (iter, 0);
			
			if( (string) model.GetValue (iter, 1) == Catalog.GetString("Unlimited") ) {
				selectedUnlimited = true;
				selectedLimited = true; //unlimited jumps will be limited by clicking on "finish"
							//and this will be always done by the user when
							//some jumps are done (not time like in runs)
			} 
			
			if( (string) model.GetValue (iter, 1) == Catalog.GetString("Jumps") ) {
				selectedLimited = true;
			}
			
			if( (string) model.GetValue (iter, 2) == "?")
				selectedLimitedValue = 0;
			else if( (string) model.GetValue (iter, 2) == "-") 
				selectedLimitedValue = -1.0;
			else 
				selectedLimitedValue = Convert.ToDouble( (string) model.GetValue (iter, 2) );

			if( (string) model.GetValue (iter, 3) == Catalog.GetString("Yes") ) {
				selectedStartIn = true;
			}
			if( (string) model.GetValue (iter, 4) == Catalog.GetString("Yes") ) {
				selectedExtraWeight = true;
			}
			selectedDescription = (string) model.GetValue (iter, 5);

			//activate on_button_accept_clicked()
			button_accept.Activate();
		}
	}
	
	protected override void deleteTestLine() {
		SqliteJumpType.Delete(Constants.JumpRjTypeTable, selectedEventName, false);
	}
	
	protected override string [] findTestTypesInSessions() {
		return SqliteJumpRj.SelectJumps(-1, -1, "", selectedEventName); 
	}
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		JumpsRjMoreWindowBox.jumps_runs_more.Hide();
		JumpsRjMoreWindowBox = null;
	}
	
	void on_jumps_runs_more_delete_event (object o, DeleteEventArgs args)
	{
		//raise signal
		button_cancel.Click();

		//JumpsRjMoreWindowBox.jumps_runs_more.Hide();
		//JumpsRjMoreWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		JumpsRjMoreWindowBox.jumps_runs_more.Hide();
	}

	//when a jump Rj is done using jumpsRjMoreWindow, the accept doesn't destroy this instance, because 
	//later we need data from it.
	//This is used for destroying, then if a new jump rj type is added, it will be shown at first time clicking "more" button
	public void Destroy() {		
		JumpsRjMoreWindowBox = null;
	}

	public bool SelectedLimited 
	{
		get { return selectedLimited; }
	}
	
	public double SelectedLimitedValue 
	{
		get { return selectedLimitedValue; }
	}
	
	public bool SelectedStartIn 
	{
		get { return selectedStartIn; }
	}
	
	public bool SelectedExtraWeight 
	{
		get { return selectedExtraWeight; }
	}
	
	public bool SelectedUnlimited 
	{
		get { return selectedUnlimited; }
	}
}
