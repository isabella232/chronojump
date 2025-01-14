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
 * Copyright (C) 2016-2020   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Gtk;
using Gdk;
using Glade;
using Mono.Unix;

public class DialogThreshold
{
	[Widget] Gtk.Dialog dialog_threshold;

	[Widget] Gtk.Notebook notebook;
	[Widget] Gtk.TextView textview_about;
	[Widget] Gtk.TextView textview_jumps;
	[Widget] Gtk.TextView textview_races;
	[Widget] Gtk.TextView textview_other;
	[Widget] Gtk.RadioButton radio_jumps;
	[Widget] Gtk.RadioButton radio_races;
	[Widget] Gtk.RadioButton radio_other;

	[Widget] Gtk.Label label_threshold_name;
	[Widget] Gtk.Label label_threshold_value;
	[Widget] Gtk.HScale hscale_threshold;

	//just for the colors
	[Widget] Gtk.Label label_about;
	[Widget] Gtk.Label label_radio_jumps;
	[Widget] Gtk.Label label_radio_races;
	[Widget] Gtk.Label label_radio_other;

	private int thresholdCurrent;
	public Button FakeButtonClose;

	public DialogThreshold (Constants.Menuitem_modes m, int thresholdCurrent)
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "dialog_threshold.glade", "dialog_threshold", null);
		gladeXML.Autoconnect(this);
		
		//put an icon to window
		UtilGtk.IconWindow(dialog_threshold);

		//manage window color
		if(! Config.UseSystemColor)
		{
			UtilGtk.DialogColor(dialog_threshold, Config.ColorBackground);
			UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, label_threshold_name);
			UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, label_threshold_value);
			UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, label_about);
			UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, label_radio_jumps);
			UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, label_radio_races);
			UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, label_radio_other);
		}

		FakeButtonClose = new Gtk.Button();

		this.thresholdCurrent = thresholdCurrent;
		hscale_threshold.Value = Convert.ToInt32(thresholdCurrent / 10);
		label_threshold_value.Text = thresholdCurrent.ToString() + " ms";

		writeTexts();

		if(m == Constants.Menuitem_modes.JUMPSSIMPLE || m == Constants.Menuitem_modes.JUMPSREACTIVE)
		{
			label_threshold_name.Text = "<b>" + Catalog.GetString("Threshold for jumps") + "</b>";
			radio_jumps.Active = true;
		}
                else if(m == Constants.Menuitem_modes.RUNSSIMPLE || m == Constants.Menuitem_modes.RUNSINTERVALLIC)
		{
			label_threshold_name.Text = "<b>" + Catalog.GetString("Threshold for runs") + "</b>";
			radio_races.Active = true;
		}
		else 	//other
		{
			label_threshold_name.Text = "<b>" + Catalog.GetString("Threshold for other tests") + "</b>";
			radio_other.Active = true;
		}

		label_threshold_name.UseMarkup = true;
	}

	//hscale does not manage correctly the +10 increments.
	//we solve it with a label
	private void on_hscale_threshold_value_changed(object o, EventArgs arg)
	{
		thresholdCurrent = 10 * Convert.ToInt32(hscale_threshold.Value);
		label_threshold_value.Text = thresholdCurrent.ToString() + " ms";
	}

	private void writeTexts()
	{
		TextBuffer tb_about = new TextBuffer (new TextTagTable());
		tb_about.Text =  Catalog.GetString("Spurious signals are common on electronics.") +
			"\n\n" + Catalog.GetString("Threshold refers to the minimum value measurable and is the common way to clean this spurious signals.") +
			"\n"   + Catalog.GetString("Threshold should be a value lower than expected values.") +

			"\n\n" + Catalog.GetString("On database three different thresholds are stored: jumps, races and other tests.") +
			"\n"  +  Catalog.GetString("If you change these values they will be stored once test is executed.") +

			"\n\n" + Catalog.GetString("Usually threshold values should not be changed but this option is useful for special cases.");
		textview_about.Buffer = tb_about;

		TextBuffer tb_jumps = new TextBuffer (new TextTagTable());
		tb_jumps.Text =  Catalog.GetString("Default value: 50 ms.") +
			"\n\n" + Catalog.GetString("On jumps with contact platforms a value of 50 ms (3 cm jump approximately) is enough to solve electronical problems.") +
			"\n\n" + Catalog.GetString("You may change this value if you have a jumper that loses pressure with the platform while going down previous to a CMJ or ABK jump.") +
			"\n" +   Catalog.GetString("This jumper should change his technique, but if it's difficult, a solution is to increase threshold.");
		textview_jumps.Buffer = tb_jumps;

		TextBuffer tb_races = new TextBuffer (new TextTagTable());
		tb_races.Text =  Catalog.GetString("Default value: 10 ms.") +
			"\n\n" + Catalog.GetString("On races with photocells a value of 10 ms is the default value.") +
			"\n\n" + Catalog.GetString("As Chronojump manages double contacts on photocells, changing threshold value is not very common.");
		textview_races.Buffer = tb_races;

		TextBuffer tb_other = new TextBuffer (new TextTagTable());
		tb_other.Text =  Catalog.GetString("Default value: 50 ms.") +
			"\n\n" + Catalog.GetString("Depending on the test, user could change values.");
		textview_other.Buffer = tb_other;
	}

	private void on_radio_jumps_toggled (object o, EventArgs args)
	{
		if(radio_jumps.Active)
			notebook.CurrentPage = 0;
	}

	private void on_radio_races_toggled (object o, EventArgs args)
	{
		if(radio_races.Active)
			notebook.CurrentPage = 1;
	}

	private void on_radio_other_toggled (object o, EventArgs args)
	{
		if(radio_other.Active)
			notebook.CurrentPage = 2;
	}

	public void on_button_close_clicked (object obj, EventArgs args)
	{
		FakeButtonClose.Click(); //this will call DestroyDialog() later
	}

	private void on_delete_event (object o, DeleteEventArgs args)
	{
		FakeButtonClose.Click(); //this will call DestroyDialog() later

		args.RetVal = true;
	}

	public void DestroyDialog ()
	{
		dialog_threshold.Destroy ();
	}

	public int ThresholdCurrent
	{
		get { return thresholdCurrent;	}
	}
}
