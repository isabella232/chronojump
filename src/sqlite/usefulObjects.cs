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
 * Copyright (C) 2017-2020   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Mono.Unix;
using System.Collections.Generic; //List<T>

//class from 2.0 code that manages 3 lists an SQL stuff (for a combo)

public class LSqlEnTrans
{
	private string name;
	private List<string> l_sql;
	private int sqlDefault;
	private int sqlCurrent;
	private List<string> l_en;
	private List<string> l_trans;

	public LSqlEnTrans (string name, List<string> l_sql, int sqlDefault, int sqlCurrent, List<string> l_en)
	{
		this.name = name;
		this.l_sql = l_sql;
		this.sqlDefault = sqlDefault;
		this.sqlCurrent = sqlCurrent;
		this.l_en = l_en;

		l_trans = new List<string>();
		foreach(string s in l_en)
			l_trans.Add(Catalog.GetString(s));
	}

	public void SetCurrentFromSQL (string newCurrent)
	{
		for(int i = 0; i < l_sql.Count; i ++)
			if(l_sql[i] == newCurrent)
			{
				sqlCurrent = i;
				return;
			}

		sqlCurrent = 0;
	}

	public void SetCurrentFromComboTranslated (string trString)
	{
		for(int i = 0; i < l_trans.Count; i ++)
			if(l_trans[i] == trString)
			{
				sqlCurrent = i;
				return;
			}

		sqlCurrent = 0;
	}

	private string getSqlDefaultName ()
	{
		return(l_sql[sqlDefault]);
	}
	private string getSqlCurrentName ()
	{
		return(l_sql[sqlCurrent]);
	}

	public string Name
	{
		get { return name; }
	}

	public string SqlDefaultName
	{
		get { return getSqlDefaultName(); }
	}

	public string SqlCurrentName
	{
		get { return getSqlCurrentName(); }
	}

	public int SqlCurrent
	{
		get { return sqlCurrent; }
	}

	public List<string> L_trans
	{
		get { return l_trans; }
	}

	public string TranslatedCurrent
	{
		get { return l_trans[sqlCurrent]; }
	}

}

public class SelectTypes
{
	public int Id;
	public string NameEnglish;
	public string NameTranslated;

	public SelectTypes()
	{
	}

	public SelectTypes(int id, string nameEnglish, string nameTranslated)
	{
		this.Id = id;
		this.NameEnglish = nameEnglish;
		this.NameTranslated = nameTranslated;
	}
}

public class SelectJumpTypes : SelectTypes
{
	public bool StartIn;
	public bool HasWeight;
	public string Description;

	//needed for inheritance
	public SelectJumpTypes()
	{
	}

	public SelectJumpTypes(string nameEnglish)
	{
		this.NameEnglish = nameEnglish;
	}

	public SelectJumpTypes(int id, string nameEnglish, bool startIn, bool hasWeight, string description)
	{
		this.Id = id;
		this.NameEnglish = nameEnglish;
		this.NameTranslated = Catalog.GetString(nameEnglish);
		this.StartIn = startIn;
		this.HasWeight = hasWeight;
		this.Description = description;
	}
}

public class SelectJumpRjTypes : SelectJumpTypes
{
	public bool JumpsLimited;
	public double FixedValue;

	public SelectJumpRjTypes(string nameEnglish)
	{
		this.NameEnglish = nameEnglish;
	}

	public SelectJumpRjTypes(int id, string nameEnglish, bool startIn, bool hasWeight, bool jumpsLimited, double fixedValue, string description)
	{
		this.Id = id;
		this.NameEnglish = nameEnglish;
		this.NameTranslated = Catalog.GetString(nameEnglish);
		this.StartIn = startIn;
		this.HasWeight = hasWeight;
		this.JumpsLimited = jumpsLimited;
		this.FixedValue = fixedValue;
		this.Description = description;
	}
}

public class SelectRunTypes : SelectTypes
{
	public double Distance;
	public string Description;

	//needed for inheritance
	public SelectRunTypes()
	{
	}

	public SelectRunTypes(string nameEnglish)
	{
		this.NameEnglish = nameEnglish;
	}

	public SelectRunTypes(int id, string nameEnglish, double distance, string description)
	{
		this.Id = id;
		this.NameEnglish = nameEnglish;
		this.NameTranslated = Catalog.GetString(nameEnglish);
		this.Distance = distance;
		this.Description = description;
	}
}

public class SelectRunITypes : SelectRunTypes
{
	public bool TracksLimited;
	public int FixedValue;
	public bool Unlimited;
	public string DistancesString;

	public SelectRunITypes(string nameEnglish)
	{
		this.NameEnglish = nameEnglish;
	}

	public SelectRunITypes(int id, string nameEnglish, double distance,
			bool tracksLimited, int fixedValue, bool unlimited,
			string description, string distancesString)
	{
		this.Id = id;
		this.NameEnglish = nameEnglish;
		this.NameTranslated = Catalog.GetString(nameEnglish);
		this.Distance = distance;
		this.TracksLimited = tracksLimited;
		this.FixedValue = fixedValue;
		this.Unlimited = unlimited;
		this.Description = description;
		this.DistancesString = distancesString;
	}
}
