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

using System.Threading;
using Mono.Unix;


//--------------------------------------------------------
//---------------- EDIT EVENT WIDGET ---------------------
//--------------------------------------------------------

public class EditEventWindow 
{
	[Widget] protected Gtk.Window edit_event;
	[Widget] protected Gtk.Button button_accept;
	[Widget] protected Gtk.Label label_header;
	[Widget] protected Gtk.Label label_type_title;
	[Widget] protected Gtk.Label label_type_value;
	[Widget] protected Gtk.Label label_event_id_value;
	[Widget] protected Gtk.Label label_tv_title;
	[Widget] protected Gtk.Entry entry_tv_value;
	[Widget] protected Gtk.Label label_tc_title;
	[Widget] protected Gtk.Entry entry_tc_value;
	[Widget] protected Gtk.Label label_fall_title;
	[Widget] protected Gtk.Entry entry_fall_value;
	[Widget] protected Gtk.Label label_distance_title;
	[Widget] protected Gtk.Entry entry_distance_value;
	[Widget] protected Gtk.Label label_time_title;
	[Widget] protected Gtk.Entry entry_time_value;
	[Widget] protected Gtk.Label label_speed_title;
	[Widget] protected Gtk.Label label_speed_value;
	[Widget] protected Gtk.Label label_weight_title;
	[Widget] protected Gtk.Entry entry_weight_value;
	[Widget] protected Gtk.Label label_limited_title;
	[Widget] protected Gtk.Label label_limited_value;
	
	[Widget] protected Gtk.Box hbox_combo_eventType;
	[Widget] protected Gtk.ComboBox combo_eventType;
	[Widget] protected Gtk.Box hbox_combo_person;
	[Widget] protected Gtk.ComboBox combo_persons;
	
	[Widget] protected Gtk.TextView textview_description;

	static EditEventWindow EditEventWindowBox;
	protected Gtk.Window parent;
	protected int pDN;
	protected bool metersSecondsPreferred;
	protected string type;
	protected string entryTv; //contains a entry that is a Number. If changed the entry as is not a number, recuperate this
	protected string entryTc = "0";
	protected string entryFall = "0"; 
	protected string entryDistance = "0";
	protected string entryTime = "0";
	protected string entrySpeed = "0";
	protected string entryWeight = "0"; //used to record the % for old person if we change it

	protected bool showTv;
	protected bool showTc;
	protected bool showFall;
	protected bool showDistance;
	protected bool showTime;
	protected bool showSpeed;
	protected bool showWeight;
	protected bool showLimited;

	protected string eventBigTypeString = "a test";

	protected int oldPersonID; //used to record the % for old person if we change it

	//for inheritance
	protected EditEventWindow () {
	}

	EditEventWindow (Gtk.Window parent) {
		//Glade.XML gladeXML;
		//gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "edit_event", null);
		//gladeXML.Autoconnect(this);
		this.parent = parent;
	}

	static public EditEventWindow Show (Gtk.Window parent, Event myEvent, int pDN)
		//run win have also metersSecondsPreferred
	{
		if (EditEventWindowBox == null) {
			EditEventWindowBox = new EditEventWindow (parent);
		}
		
		EditEventWindowBox.pDN = pDN;
		
		EditEventWindowBox.initializeValues();

		EditEventWindowBox.fillDialog (myEvent);

		EditEventWindowBox.edit_event.Show ();

		return EditEventWindowBox;
}
	
	protected virtual void initializeValues () {
		showTv = true;
		showTc = true;
		showFall = true;
		showDistance = true;
		showTime = true;
		showSpeed = true;
		showWeight = true;
		showLimited = true;
	}

	protected void fillDialog (Event myEvent)
	{
		fillWindowTitleAndLabelHeader();	
		label_event_id_value.Text = myEvent.UniqueID.ToString();

		if(showTv)
			fillTv(myEvent);
		else { 
			label_tv_title.Hide();
			entry_tv_value.Hide();
		}

		if(showTc)
			fillTc(myEvent);
		else { 
			label_tc_title.Hide();
			entry_tc_value.Hide();
		}

		if(showFall)
			fillFall(myEvent);
		else { 
			label_fall_title.Hide();
			entry_fall_value.Hide();
		}

		if(showDistance)
			fillDistance(myEvent);
		else { 
			label_distance_title.Hide();
			entry_distance_value.Hide();
		}

		if(showTime)
			fillTime(myEvent);
		else { 
			label_time_title.Hide();
			entry_time_value.Hide();
		}

		if(showSpeed)
			fillSpeed(myEvent);
		else { 
			label_speed_title.Hide();
			label_speed_value.Hide();
		}

		if(showWeight)
			fillWeight(myEvent);
		else { 
			label_weight_title.Hide();
			entry_weight_value.Hide();
		}

		if(showLimited)
			fillLimited(myEvent);
		else { 
			label_limited_title.Hide();
			label_limited_value.Hide();
		}

		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.Text = myEvent.Description;
		textview_description.Buffer = tb;

		createComboEventType(myEvent);

		string [] persons = SqlitePersonSession.SelectCurrentSession(myEvent.SessionID, true, false); //onlyIDAndName, not reversed
		combo_persons = ComboBox.NewText();
		UtilGtk.ComboUpdate(combo_persons, persons);
		combo_persons.Active = UtilGtk.ComboMakeActive(persons, myEvent.PersonID + ":" + myEvent.PersonName);
		
		oldPersonID = myEvent.PersonID;
			
		hbox_combo_person.PackStart(combo_persons, true, true, 0);
		hbox_combo_person.ShowAll();
	}
	
	protected void fillWindowTitleAndLabelHeader() {
		edit_event.Title = string.Format(Catalog.GetString("Edit {0}"), eventBigTypeString);

		System.Globalization.NumberFormatInfo localeInfo = new System.Globalization.NumberFormatInfo();
		localeInfo = System.Globalization.NumberFormatInfo.CurrentInfo;
		label_header.Text = string.Format(Catalog.GetString("Use this window to edit a {0}.\n(decimal separator: '{1}')"), eventBigTypeString, localeInfo.NumberDecimalSeparator);
	}

		
	protected void createComboEventType(Event myEvent) 
	{
		combo_eventType = ComboBox.NewText ();
		string [] myTypes = findTypes(myEvent);
		UtilGtk.ComboUpdate(combo_eventType, myTypes);
		combo_eventType.Active = UtilGtk.ComboMakeActive(myTypes, myEvent.Type);
		hbox_combo_eventType.PackStart(combo_eventType, true, true, 0);
		hbox_combo_eventType.ShowAll();

		createSignal();
	}

	protected virtual string [] findTypes(Event myEvent) {
		string [] myTypes = new String[0];
		return myTypes;
	}

	protected virtual void createSignal() {
		//only for runs
	}

	protected void fillTv(Event myEvent) {
		Jump myJump = (Jump) myEvent;
		entryTv = myJump.Tv.ToString();
		entry_tv_value.Text = Util.TrimDecimals(entryTv, pDN);
	}

	protected virtual void fillTc (Event myEvent) {
	}

	protected virtual void fillFall(Event myEvent) {
	}

	protected virtual void fillDistance(Event myEvent) {
		/*
		Run myRun = (Run) myEvent;
		entryDistance = myRun.Distance.ToString();
		entry_distance_value.Text = Util.TrimDecimals(entryDistance, pDN);
		*/
	}

	protected virtual void fillTime(Event myEvent) {
		/*
		Run myRun = (Run) myEvent;
		entryTime = myRun.Time.ToString();
		entry_time_value.Text = Util.TrimDecimals(entryTime, pDN);
		*/
	}
	
	protected virtual void fillSpeed(Event myEvent) {
		/*
		Run myRun = (Run) myEvent;
		label_speed_value.Text = Util.TrimDecimals(myRun.Speed.ToString(), pDN);
		*/
	}

	protected virtual void fillWeight(Event myEvent) {
		/*
		Jump myJump = (Jump) myEvent;
		if(myJump.TypeHasWeight) {
			entryWeight = myJump.Weight.ToString();
			entry_weight_value.Text = entryWeight;
			entry_weight_value.Sensitive = true;
		} else {
			entry_weight_value.Sensitive = false;
		}
		*/
	}

	protected virtual void fillLimited(Event myEvent) {
		/*
		JumpRj myJumpRj = (JumpRj) myEvent;
		label_limited_value.Text = Util.GetLimitedRounded(myJumpRj.Limited, pDN);
		*/
	}

		
	private void on_entry_tv_value_changed (object o, EventArgs args) {
		if(Util.IsNumber(entry_tv_value.Text.ToString())){
			entryTv = entry_tv_value.Text.ToString();
		} else {
			entry_tv_value.Text = "";
			entry_tv_value.Text = entryTv;
		}
	}
		
	private void on_entry_tc_value_changed (object o, EventArgs args) {
		if(Util.IsNumber(entry_tc_value.Text.ToString())){
			entryTc = entry_tc_value.Text.ToString();
		} else {
			entry_tc_value.Text = "";
			entry_tc_value.Text = entryTc;
		}
	}
		
	private void on_entry_fall_value_changed (object o, EventArgs args) {
		if(Util.IsNumber(entry_fall_value.Text.ToString())){
			entryFall = entry_fall_value.Text.ToString();
		} else {
			entry_fall_value.Text = "";
			entry_fall_value.Text = entryFall;
		}
	}
		
	private void on_entry_time_changed (object o, EventArgs args) {
		if(Util.IsNumber(entry_time_value.Text.ToString())){
			entryTime = entry_time_value.Text.ToString();
			label_speed_value.Text = Util.TrimDecimals(
					Util.GetSpeed (entryDistance, entryTime, metersSecondsPreferred) , pDN);
		} else {
			entry_time_value.Text = "";
			entry_time_value.Text = entryTime;
		}
	}
	
	private void on_entry_distance_changed (object o, EventArgs args) {
		if(Util.IsNumber(entry_distance_value.Text.ToString())){
			entryDistance = entry_distance_value.Text.ToString();
			label_speed_value.Text = Util.TrimDecimals(
					Util.GetSpeed (entryDistance, entryTime, metersSecondsPreferred) , pDN);
		} else {
			entry_distance_value.Text = "";
			entry_distance_value.Text = entryDistance;
		}
	}
		
	private void on_entry_weight_value_changed (object o, EventArgs args) {
		if(Util.IsNumber(entry_weight_value.Text.ToString())){
			entryWeight = entry_weight_value.Text.ToString();
		} else {
			entry_weight_value.Text = "";
			entry_weight_value.Text = entryWeight;
		}
	}
		
	protected virtual void on_button_cancel_clicked (object o, EventArgs args)
	{
		EditEventWindowBox.edit_event.Hide();
		EditEventWindowBox = null;
	}
	
	protected virtual void on_delete_event (object o, DeleteEventArgs args)
	{
		EditEventWindowBox.edit_event.Hide();
		EditEventWindowBox = null;
	}

	protected virtual void hideWindow() {
		EditEventWindowBox.edit_event.Hide();
		EditEventWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		int eventID = Convert.ToInt32 ( label_event_id_value.Text );
		string myPerson = UtilGtk.ComboGetActive(combo_persons);
		string [] myPersonFull = myPerson.Split(new char[] {':'});
		
		string myDesc = textview_description.Buffer.Text;
		

		updateEvent(eventID, Convert.ToInt32(myPersonFull[0]), myDesc);

		hideWindow();
	}

	protected virtual void updateEvent(int eventID, int personID, string description) {
	}

	public Button Button_accept 
	{
		set { button_accept = value;	}
		get { return button_accept;	}
	}

}


//--------------------------------------------------------
//---------------- event_more widget ---------------------
//--------------------------------------------------------

public class EventMoreWindow 
{
	protected TreeStore store;
	[Widget] protected Gtk.TreeView treeview_more;
	[Widget] protected Gtk.Button button_accept;
	protected Gtk.Window parent;

	protected string selectedEventType;
	protected string selectedEventName;
	protected string selectedDescription;
	public Gtk.Button button_selected;

	public EventMoreWindow () {
	}

	public EventMoreWindow (Gtk.Window parent) {
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "jumps_runs_more", null);
		gladeXML.Autoconnect(this);
		*/

		//name, startIn, weight, description
		store = new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string));

		initializeThings();
	}

	protected void initializeThings() 
	{
		button_selected = new Gtk.Button();
		
		createTreeView(treeview_more);

		treeview_more.Model = store;
		fillTreeView(treeview_more,store);

		button_accept.Sensitive = false;
		 
		treeview_more.Selection.Changed += OnSelectionEntry;
	}


	//if eventType is predefined, it will have a translation on src/evenType or derivated class
	//this is useful if user changed language
	protected string getDescriptionLocalised(EventType myType, string descriptionFromDb) {
	if(myType.IsPredefined)
		return myType.Description;
	else
		return descriptionFromDb;
	}


	protected void OnSelectionEntry (object o, EventArgs args)
	{
		TreeModel model;
		TreeIter iter;

		if (((TreeSelection)o).GetSelected(out model, out iter))
		{
			selectedEventName = (string) model.GetValue (iter, 0);
			button_selected.Click();
		}
	}
	
	protected virtual void createTreeView (Gtk.TreeView tv) {
	}
	
	protected virtual void fillTreeView (Gtk.TreeView tv, TreeStore store) 
	{
	}

	//puts a value in private member selected
	protected virtual void on_treeview_changed (object o, EventArgs args)
	{
	}
	
	protected virtual void on_row_double_clicked (object o, Gtk.RowActivatedArgs args)
	{
	}
	
	//fired when something is selected for drawing on imageTest
	public Button Button_selected
	{
		get { return button_selected; }
	}

	public Button Button_accept 
	{
		set {
			button_accept = value;	
		}
		get {
			return button_accept;
		}
	}
	
	public string SelectedEventName
	{
		set {
			selectedEventName = value;	
		}
		get {
			return selectedEventName;
		}
	}
	
	public string SelectedDescription {
		get { return selectedDescription; }
	}
}
