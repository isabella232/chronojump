
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
 *  Copyright (C) 2004-2020   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Collections.Generic; //List
using Gtk;
using Cairo;


public class JumpsEvolutionGraph : CairoXY
{
	//constructor when there are no points
	public JumpsEvolutionGraph (DrawingArea area, string jumpType)//, string title, string jumpType, string date)
	{
		this.area = area;

		initGraph();

		g.SetFontSize(16);
		printText(area.Allocation.Width /2, area.Allocation.Height /2, 24, textHeight,
				string.Format("Need to execute jumps: {0}.", jumpType), g, true);

		endGraph();
	}

	//regular constructor
	public JumpsEvolutionGraph (
			List<Point> point_l, double[] coefs,
			LeastSquares.ParaboleTypes paraboleType,
			double xAtMMaxY, //x at Model MaxY
			double pointsMaxValue,
			DrawingArea area,
			string title, string jumpType, string date)
	{
		this.point_l = point_l;
		this.coefs = coefs;
		this.paraboleType = paraboleType;
		this.xAtMMaxY = xAtMMaxY;
		this.pointsMaxValue = pointsMaxValue;
		this.area = area;
		this.title = title;
		this.jumpType = jumpType;
		this.date = date;

		axisYLabel = "Height (cm)";
		axisXLabel = "Date";
	}

	public override void Do()
	{
		LogB.Information("at JumpsEvolutionGraph.Do");
		initGraph();

                findPointMaximums();
                findAbsoluteMaximums();
		paintAxisAndGrid(gridTypes.HORIZONTALLINES);
		paintGridDatetime();

		LogB.Information(string.Format("coef length:{0}", coefs.Length));
		if(coefs.Length == 3)
			plotPredictedLine();

		plotRealPoints();

		if(coefs.Length == 3)
		{
			if(paraboleType == LeastSquares.ParaboleTypes.CONVEX)
			{
				plotPredictedMaxPoint();
				writeTextPredictedPoint();
			}
			else
				writeTextConcaveParabole();
		} else {
			//TODO: if two points draw a line, but no need to show error if there are 1 or 2 points
			//writeTextNeed3PointsWithDifferentFall();
		}
		writeTitle();

		endGraph();
	}

	//here X is year, add/subtract third of a year
	protected override void separateMinXMaxXIfNeeded()
	{
		if(minX == maxX || maxX - minX < .1) //<.1 means that maybe we will not see any vertical bar on grid, enlarge it
		{
			minX -= .1;
			maxX += .1;
		}
	}

	//a bit recursive function ;)
	private void paintGridDatetime()
	{
		g.Save();
		g.SetDash(new double[]{1, 2}, 0);

		bool paintMonths = (Convert.ToInt32(Math.Floor(maxX)) - Convert.ToInt32(Math.Floor(minX)) < 3); //paint months if few years

		//-1 to start on previous year to see last months (if fit into graph)
		for(int year = Convert.ToInt32(Math.Floor(minX)) -1; year <= Convert.ToInt32(Math.Floor(maxX)); year ++)
		{
			int xtemp = Convert.ToInt32(calculatePaintX(year, graphWidth, maxX, minX, outerMargins + innerMargins, outerMargins + innerMargins));
			if( ! (xtemp < outerMargins || xtemp > graphWidth - outerMargins) )
			{
				if(paintMonths)
					paintVerticalGridLine(xtemp, string.Format("{0} {1}", year, UtilDate.GetMonthName(0, true)));
				else
					paintVerticalGridLine(xtemp, year.ToString());
			}

			if(! paintMonths)
				continue;

			int monthStep = 3;
			//1 get de distance between 1 month and the next one
			int xtemp1 = Convert.ToInt32(calculatePaintX(year + 1/12.0, graphWidth, maxX, minX, outerMargins + innerMargins, outerMargins + innerMargins));
			int xtemp2 = Convert.ToInt32(calculatePaintX(year + 2/12.0, graphWidth, maxX, minX, outerMargins + innerMargins, outerMargins + innerMargins));
			if(xtemp2 - xtemp1 > 100)
				monthStep = 1;

			for(int month = monthStep; month <= 12-monthStep; month += monthStep)
			{
				LogB.Information(string.Format("year-month: {0}-{1}", year, month));
				xtemp = Convert.ToInt32(calculatePaintX(year + month/12.0, graphWidth, maxX, minX, outerMargins + innerMargins, outerMargins + innerMargins));
				if(xtemp < outerMargins || xtemp > graphWidth - outerMargins)
					continue;

				paintVerticalGridLine(xtemp, string.Format("{0} {1}", year, UtilDate.GetMonthName(month, true)));
			}
		}

		g.Stroke ();
		g.Restore();
	}

	protected override void writeTitle()
	{
		writeTextAtRight(-5, title, true);
		writeTextAtRight(-3, "Jumptype: " + jumpType, false);
		writeTextAtRight(-2, date, false);
	}

}