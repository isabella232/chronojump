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
 * Copyright (C) 2019   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.IO;
using System.Text.RegularExpressions; //Regex

public class UtilDate
{
	//comes from sql like YYYY-MM-DD (with always all digits)
        //return datetime
        public static DateTime FromSql (string date)
        {
                Console.WriteLine("UtilDate.FromSql date: " + date);
                /*
                   on report we do a session select with uniqueID = -1
                   it returns nothing, date has nothing
                   */
                if(date == null || date == "")
                        return DateTime.Now; //TODO: ensure this now is year-month-day

                /*
                   maybe date format is before 0.72 (d/m/Y)
                   this is still here and not in a standalone conversion
                   because if someone converts from older database
                   can have problems wih constructors with different date formats
                   */

                DateTime dt; //Datetime (year, month, day) constructor
                if(date.IndexOf('/') == -1) {
                        //not contains '/'
                        //new sqlite3 compatible date format sice db 0.72 YYYY-MM-DD
                        string [] dateFull = date.Split(new char[] {'-'});
                        dt = new DateTime(
                                        Convert.ToInt32(dateFull[0]),
                                        Convert.ToInt32(dateFull[1]),
                                        Convert.ToInt32(dateFull[2]));
                } else {
                        //contains '/'
                        //old D/M/Y format 
                        string [] dateFull = date.Split(new char[] {'/'});
                        dt = new DateTime(
                                        Convert.ToInt32(dateFull[2]),
                                        Convert.ToInt32(dateFull[1]),
                                        Convert.ToInt32(dateFull[0]));
                }
                return dt;
        }

	//this function is exclusive from processMultiDatabases code
	public static string FindMoment(string location, string filename)
	{
		//1 parse date of filename
		Console.WriteLine("FindMoment for filename: " + filename);
		string searchedDatetime = getFilenameDatetime(filename);
		if(searchedDatetime == "")
			return "(moment)";

		//2 search date on all folders
		int foundCount = 0;
		string moment = "NOTFOUND:" + searchedDatetime;
		DirectoryInfo [] dirArray = new DirectoryInfo(location).GetDirectories();
                foreach (DirectoryInfo dir in dirArray)
		{
	                foreach (FileInfo file in dir.GetFiles())
			{
				//Console.WriteLine("filename: {0}, date: {1}", file.Name, getFilenameDatetime(file.Name));
				if(getFilenameDatetime(file.Name) == searchedDatetime)
				{
					Console.WriteLine("FOUND at folder: {0}", dir.Name);
					moment = dir.Name;
					foundCount ++;
				}
			}
		}

		if(foundCount >= 2)
		{
			Console.WriteLine(string.Format("FOUND {0} times!!!", foundCount));
			return "DUPLICATED";
		}
		return moment;
	}

	//this function is exclusive from processMultiDatabases code
	private static string getFilenameDatetime(string filename)
	{
		Match match = Regex.Match(filename, @"(\d+-\d+-\d+_\d+-\d+-\d+)");
		if(match.Groups.Count == 2)
			return match.Value;

		return "";
	}
}
