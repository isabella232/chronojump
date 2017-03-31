/*
 * Copyright (C) 2017  Xavier de Blas <xaviblas@gmail.com>
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
 */

using System;
using System.Collections.Generic; //List<T>

public class Trigger
{
	public enum Modes { ENCODER }

	private int uniqueID;
	private Modes mode;
	private int modeID;
	private int ms;
	private bool inOut;
	private string name;
	private string color;
	private string comments;

	//constructor used on capture
	public Trigger (Modes mode, int ms, bool inOut)
	{
		this.uniqueID = -1; 	//will be assigned on SQL insertion
		this.mode = mode;
		this.modeID = -1; 	//will be assigned on SQL insertion
		this.ms = ms;
		this.inOut = inOut;
		this.name = "";
		this.color = "";
		this.comments = "";
	}

	//constructor used on loading from SQL
	public Trigger (int uniqueID, Modes mode, int modeID, int ms, bool inOut, string name, string color, string comments)
	{
		this.uniqueID = uniqueID;
		this.mode = mode;
		this.modeID = modeID;
		this.ms = ms;
		this.inOut = inOut;
		this.name = name;
		this.color = color;
		this.comments = comments;
	}

	public void Substract(int msToSubstract)
	{
		ms -= msToSubstract;
	}

	public string ToSQL()
	{
	 	return 
			uniqueID.ToString() + "," +
			"\"" + mode.ToString() + "\"" + "," +
			modeID.ToString() + "," +
			ms.ToString() + "," +
			Util.BoolToInt(inOut).ToString() + "," +
			"\"" + name.ToString() + "\"" + "," +
			"\"" + color.ToString() + "\"" + "," +
			"\"" + comments.ToString() + "\""
			;
	}

	public int UniqueID {
		get { return uniqueID; }
	}
}

public class TriggerList
{
	private List<Trigger> l;
	public TriggerList()
	{
		l = new List<Trigger>();
	}

	public void Add(Trigger trigger)
	{
		l.Add(trigger);
	}

	public void Substract(int msToSubstract)
	{
		foreach(Trigger trigger in l)
			trigger.Substract(msToSubstract);
	}

	//just to debug
	public void Print()
	{
		LogB.Information("Printing trigger list");
		foreach(Trigger trigger in l)
			LogB.Information(trigger.ToString());
	}

	public void SQLInsert()
	{
		//save triggers to file (if any)
		if(l == null || l.Count == 0)
			return;

		LogB.Debug("runEncoderCaptureCsharp SQL inserting triggers");
		SqliteTrigger.InsertList(false, l);
	}
	/*
	public void Write()
	{
		//save triggers to file (if any)
		if(l == null || l.Count == 0)
			return;

		LogB.Debug("runEncoderCaptureCsharp saving triggers");
		TextWriter writer = File.CreateText(Util.GetEncoderTriggerDateTimeFileName());

		foreach(Trigger trigger in l)
			writer.WriteLine(trigger.ToString());

		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
	}
	*/
}

