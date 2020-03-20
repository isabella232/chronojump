/*
 * This file is part of ChronoJump
 *
 * Chronojump is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or   
 *    (at your option) any later version.
 *    
 * Chronojump is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
 *    GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 * Copyright (C) 2018   Xavier de Blas <xaviblas@gmail.com> 
 */

//this file has methods of ChronoJumpWindow related to manage persons

using System;
using Gtk;
using Gdk;
using Glade;

public partial class ChronoJumpWindow
{

	private void showPersonsOnTop (bool onTop)
	{
		viewport_persons.Visible = currentSession != null && ! onTop;
		hbox_top_person.Visible = onTop;
		hbox_top_person_encoder.Visible = onTop;
	}

	private void showPersonPhoto (bool showPhoto)
	{
		hbox_persons_bottom_photo.Visible = showPhoto;
		hbox_persons_bottom_no_photo.Visible = ! showPhoto;
	}

	private void label_person_change()
	{
		label_top_person_name.Text = "<b>" + currentPerson.Name + "</b>";
		label_top_person_name.UseMarkup = true;

		label_top_encoder_person_name.Text = "<b>" + currentPerson.Name + "</b>";
		label_top_encoder_person_name.UseMarkup = true;

		string filenameMini = Util.UserPhotoURL(true, currentPerson.UniqueID);
		if(filenameMini != "")
		{
			Pixbuf pixbuf = new Pixbuf (filenameMini);
			image_current_person.Pixbuf = pixbuf;
			button_image_current_person_zoom.Sensitive = true;
			button_image_current_person_zoom_h.Sensitive = true;
		} else {
			//image_current_person.Pixbuf = null;
			Pixbuf pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_no_photo.png");
			image_current_person.Pixbuf = pixbuf;
			button_image_current_person_zoom.Sensitive = false;
			button_image_current_person_zoom_h.Sensitive = false;
		}
	}

}