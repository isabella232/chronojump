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
using System.Data;
using Gtk;
using System.Collections; //ArrayList


public class StatRjEvolution : Stat
{
	protected int maxJumps;
	protected string [] columnsString;

	//if this is not present i have problems like (No overload for method `xxx' takes `0' arguments) with some inherited classes
	public StatRjEvolution () 
	{
		this.showSex = false;
		this.statsJumpsType = 0;
		this.limit = 0;
	}

	public StatRjEvolution (StatTypeStruct myStatTypeStruct, int numContinuous, Gtk.TreeView treeview) 
	{
		completeConstruction (myStatTypeStruct, treeview);

		this.numContinuous = numContinuous;
		
		string sessionString = obtainSessionSqlString(sessions);

		//we need to know the reactive with more jumps for prepare columns
		maxJumps = SqliteStat.ObtainMaxNumberOfJumps(sessionString);
		
		this.dataColumns = maxJumps*2 + 2;	//for simplesession (index, (tv , tc)*jumps, fall)

		//only simplesession
		store = getStore(dataColumns +1); //jumper, datacolumns 
		string [] columnsString = new String[dataColumns +1];
		columnsString[0] = Catalog.GetString("Jumper");
		columnsString[1] = Catalog.GetString("Index");
		int i;
		for(i=0; i < maxJumps; i++) {
			columnsString[i*2 +2] = Catalog.GetString("TC") + (i+1).ToString(); //cols: 2, 4, 6, ...
			columnsString[i*2 +2 +1] = Catalog.GetString("TV") + (i+1).ToString(); //cols: 3, 5, 7, ...
		}
		columnsString[i*2 +2] = Catalog.GetString("Fall");
		
		if(toReport) {
			reportString = prepareHeadersReport(columnsString);
		} else {
			treeview.Model = store;
			prepareHeaders(columnsString);
		}
	}

		
	protected int findBestContinuous(string [] statValues, int numContinuous)
	{
		//if numContinuous is 3, we check the best consecutive three tc,tv values
		int bestPos=-1;	//position where the three best pair values start
				//will return -1 if less tha three jumps
		double bestCount=-10000;	//best value found of 3 pairs

		//read all values in pairs tc,tv
		//start in pos 2 because first is name, second is index
		//end in Length-numContinuous*2 because we should not count only the last tc,tv pair or the last two, only the last three
		//end in -1 because last value is fall
		for ( int i=2; i < statValues.Length -numContinuous*2 -1 ; i=i+2 ) 
		{
			double myCount = 0;
			bool jumpFinished = false;
			//read the n consecutive values 
			for (int j=i; j < i + numContinuous*2 ; j=j+2 )
			{
				if( statValues[j] == "-" || statValues[j+1] == "-" ) {
					jumpFinished = true;
					break;
				}
				double tc = Convert.ToDouble(statValues[i]);
				double tv = Convert.ToDouble(statValues[i+1]);
				myCount += (tv * 100) / tc;
			}
			
			//Console.WriteLine("i{0}, myCount{1}, bestCount{2}", i, myCount, bestCount);
			//if found a max, record it
			if(myCount > bestCount && !jumpFinished) {
				bestCount = myCount;
				bestPos = i;
				//Console.WriteLine("best i{0}", i);
			}
		}
		return bestPos;
	}

	protected string [] markBestContinuous(string [] statValues, int numContinuous, int bestPos) {
		if(toReport) {
			for ( int i=0; i < statValues.Length ; i=i+2 ) {

				if(i >= bestPos && i < bestPos+numContinuous*2) {
					//Console.WriteLine("i{0}, bp{1}, svi{2}, svi+1{3}", i, bestPos, statValues[i], statValues[i+1]);
					statValues[i] = "<font color=\"red\">" + statValues[i] + "</font>";
					statValues[i+1] = "<font color=\"red\">" + statValues[i+1] + "</font>";
				}
			}
		} else {
			// this marks the first and the last with '[' and ']'
			statValues[bestPos] = "[" + statValues[bestPos];
			statValues[bestPos + (numContinuous*2) -1] = statValues[bestPos + (numContinuous*2) -1] + "]";
		}
		
		return statValues;
	}
	
	protected override void printData (string [] statValues) 
	{
		if(numContinuous != -1) {
			int bestPos = findBestContinuous(statValues, numContinuous);
			if(bestPos != -1) {
				statValues = markBestContinuous(statValues, numContinuous, bestPos);
			}
		}
		
		if(toReport) {
			reportString += "<TR>";
			for (int i=0; i < statValues.Length ; i++) {
				reportString += "<TD>" + statValues[i] + "</TD>";
			}
			reportString += "</TR>\n";
		} else {
			iter = store.AppendValues (statValues); 
		}
	}

	
	
	public override void PrepareData() 
	{
		string sessionString = obtainSessionSqlString(sessions);
		//only simplesession
		bool multisession = false;

		//we send maxJumps for make all the results of same length (fill it with '-'s)
		//
		// cannot be avg in this statistic
		
		string operation = ""; //no need of "MAX", there's an order by (index) desc
		//and clenaDontWanted will do his work
		processDataSimpleSession ( cleanDontWanted (
					SqliteStat.RjEvolution(sessionString, multisession, 
						operation, jumpType, showSex, maxJumps), 
					statsJumpsType, limit),
				false, dataColumns); //don't print AVG and SD at end of row (has no sense)
	}
		
	public override string ToString () 
	{
		string selectedValuesString = "";
		if(statsJumpsType == 0) { //all jumps
			selectedValuesString = allValuesString; 
		} else if(statsJumpsType == 1) { //limit
			selectedValuesString = string.Format(Catalog.GetString("First {0} values"), limit); 
		} else if(statsJumpsType == 2) { //best of each jumper
			selectedValuesString = string.Format(Catalog.GetString("Max {0} values of each jumper"), limit);
		} 
		/* this option is not possible in this statistic
		 * 
		else if(statsJumpsType == 3) { //avg of each jumper
			selectedValuesString = avgValuesString; 
		}  
		*/

		string mySessionString = "";
		if(sessions.Count > 1) {
			mySessionString =  Catalog.GetString (" various sessions "); 
		} else {
			string [] strFull = sessions[0].ToString().Split(new char[] {':'});
			mySessionString =  Catalog.GetString (" session ") + 
				strFull[0] + "(" + strFull[2] + ")";
		}
		
		string bestResalted = "";
		if(numContinuous != -1) {
			bestResalted = string.Format(Catalog.GetString(" (best {0} consecutive jumps marked using [tv/tc *100])"), numContinuous);
		}

		return string.Format(Catalog.GetString("{0} in Rj Evolution applied to {1} on {2}{3}"), selectedValuesString, jumpType, mySessionString, bestResalted);
	}
}


