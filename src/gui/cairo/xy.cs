
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

public abstract class CairoXY
{
	//used on construction
	protected List<Point> point_l;
	protected double[] coefs;
	protected LeastSquares.ParaboleTypes paraboleType;
	protected double xAtMMaxY;
	protected double pointsMaxValue;
	protected DrawingArea area;
	protected string title;
	protected string jumpType;
	protected string date;

	protected Cairo.Context g;
	protected const int textHeight = 12;
	protected string axisRightLabel = "";

	double minX = 1000000;
	double maxX = 0;
	double minY = 1000000;
	double maxY = 0;
	double yAtMMaxY;
	double absoluteMaxX;
	double absoluteMaxY;
	int graphWidth;
	int graphHeight;
	Cairo.Color red;
	Cairo.Color blue;

	//for all 4 sides
	const int outerMargins = 30; //blank space outside the axis
	const int innerMargins = 30; //space between the axis and the real coordinates
	const int totalMargins = outerMargins + innerMargins;

	public abstract void Do();

	protected void initGraph()
	{
		//1 create context
		g = Gdk.CairoHelper.Create (area.GdkWindow);
		
		//2 clear DrawingArea (white)
		g.SetSourceRGB(1,1,1);
		g.Paint();

		graphWidth = Convert.ToInt32(area.Allocation.Width *.8);
		graphHeight = area.Allocation.Height;

		g.SetSourceRGB(0,0,0);
		g.LineWidth = 2;

		//4 prepare font
		g.SelectFontFace("Helvetica", Cairo.FontSlant.Normal, Cairo.FontWeight.Normal);
		g.SetFontSize(textHeight);

		red = colorFromRGB(200,0,0);
		blue = colorFromRGB(178, 223, 238); //lightblue
	}

	protected void findPointMaximums()
	{
		foreach(Point p in point_l)
		{
			if(p.X < minX)
				minX = p.X;
			if(p.X > maxX)
				maxX = p.X;
			if(p.Y < minY)
				minY = p.Y;
			if(p.Y > maxY)
				maxY = p.Y;
		}

		//if there is only one point, or by any reason mins == maxs, have mins and maxs separated
		if(minX == maxX)
		{
			minX -= .5 * minX;
			maxX += .5 * maxX;
		}
		if(minY == maxY)
		{
			minY -= .5 * minY;
			maxY += .5 * maxY;
		}

		absoluteMaxX = maxX;
		absoluteMaxY = maxY;
	}

	//includes point  and model
	protected void findAbsoluteMaximums()
	{
		if(coefs.Length == 3 && paraboleType == LeastSquares.ParaboleTypes.CONVEX)
		{
			//x
			absoluteMaxX = xAtMMaxY;
			if(maxX > absoluteMaxX)
				absoluteMaxX = maxX;

			//y
			yAtMMaxY = coefs[0] + coefs[1]*xAtMMaxY + coefs[2]*Math.Pow(xAtMMaxY,2);
			absoluteMaxY = yAtMMaxY;
			if(maxY > absoluteMaxY)
				absoluteMaxY = maxY;
		}
	}

	protected void paintAxisAndGrid()
	{
		//1 paint axis
		g.MoveTo(outerMargins, outerMargins);
		g.LineTo(outerMargins, graphHeight - outerMargins);
		g.LineTo(graphWidth - outerMargins, graphHeight - outerMargins);
		g.Stroke ();
		printText(2, Convert.ToInt32(outerMargins/2), 0, textHeight, "Height (cm)", g, false);
		printText(graphWidth - Convert.ToInt32(outerMargins/2), graphHeight - outerMargins, 0, textHeight, axisRightLabel, g, false);

		//2 paint grid: horizontal, vertical
		paintGrid (minY, absoluteMaxY, 5, true);
		paintGrid (minX, absoluteMaxX, 5, false);
	}

	protected void plotPredictedLine()
	{
		bool firstValue = false;
		double minMax50Percent = (minX + absoluteMaxX)/2;
		double xgraphOld = 0;
		bool wasOutOfMargins = false; //avoids to not draw a line between the end point of a line on a margin and the start point again of that line

		for(double x = minX - minMax50Percent; x < absoluteMaxX + minMax50Percent; x += (absoluteMaxX-minX)/200)
		{
			double xgraph = calculatePaintX(
					( x ),
					graphWidth, absoluteMaxX, minX, totalMargins, totalMargins);

			//do not plot two times the same x point
			if(xgraph == xgraphOld)
				continue;
			xgraphOld = xgraph;

			double ygraph = calculatePaintY(
					( coefs[0] + coefs[1]*x + coefs[2]*Math.Pow(x,2) ),
					graphHeight, absoluteMaxY, minY, totalMargins, totalMargins);

			//do not plot line outer the axis
			if(
					xgraph < outerMargins || xgraph > graphWidth - outerMargins ||
					ygraph < outerMargins || ygraph > graphHeight - outerMargins )
			{
				wasOutOfMargins = true;
				continue;
			} else {
				if(wasOutOfMargins)
					g.MoveTo(xgraph, ygraph);

				wasOutOfMargins = false;
			}

			if(! firstValue)
				g.LineTo(xgraph, ygraph);

			g.MoveTo(xgraph, ygraph);
			firstValue = false;
		}
		g.Stroke ();
	}

	protected void plotRealPoints()
	{
		foreach(Point p in point_l)
		{
			//LogB.Information(string.Format("point: {0}", p));
			double xgraph = calculatePaintX(
					( p.X ),
					graphWidth, absoluteMaxX, minX, totalMargins, totalMargins);
			double ygraph = calculatePaintY(
					( p.Y ),
					graphHeight, absoluteMaxY, minY, totalMargins, totalMargins);
			LogB.Information(string.Format("{0}, {1}", xgraph, ygraph));
			g.MoveTo(xgraph+6, ygraph);
			g.Arc(xgraph, ygraph, 6.0, 0.0, 2.0 * Math.PI); //full circle
			g.Color = blue;
			g.FillPreserve();
			g.SetSourceRGB(0, 0, 0);
			g.Stroke ();

			/*
			//print X, Y of each point
			printText(xgraph, graphHeight - Convert.ToInt32(bottomMargin/2), 0, textHeight, Util.TrimDecimals(p.X, 2), g, true);
			printText(Convert.ToInt32(leftMargin/2), ygraph, 0, textHeight, Util.TrimDecimals(p.Y, 2), g, true);
			*/
		}
	}

	protected void plotPredictedMaxPoint()
	{
		double xgraph = calculatePaintX(xAtMMaxY, graphWidth, absoluteMaxX, minX, totalMargins, totalMargins);
		double ygraph = calculatePaintY(yAtMMaxY, graphHeight, absoluteMaxY, minY, totalMargins, totalMargins);

		//print X, Y of maxY
		//at axis
		g.Save();
		g.SetDash(new double[]{14, 6}, 0);
		g.MoveTo(xgraph, graphHeight - outerMargins);
		g.LineTo(xgraph, ygraph);
		g.LineTo(outerMargins, ygraph);
		g.Stroke ();
		g.Restore();


		g.MoveTo(xgraph+8, ygraph);
		g.Arc(xgraph, ygraph, 8.0, 0.0, 2.0 * Math.PI); //full circle
		g.Color = red;
		g.FillPreserve();
		g.SetSourceRGB(0, 0, 0);
		g.Stroke ();
	}

	protected abstract void writeTitle();

	protected void writeTextPredictedPoint()
	{
		writeTextAtRight(0, "Fall: " + Util.TrimDecimals(xAtMMaxY, 2) + " cm", false);
		writeTextAtRight(1, "Jump height: " + Util.TrimDecimals(yAtMMaxY, 2) + " cm", false);
	}

	protected void writeTextConcaveParabole()
	{
		writeTextAtRight(0, "Error:", false);
		writeTextAtRight(1, "Parabole is concave", false);
	}

	protected void writeTextNeed3PointsWithDifferentFall()
	{
		writeTextAtRight(0, "Error:", false);
		writeTextAtRight(1, "Need at least 3 points", false);
		writeTextAtRight(2, "with different falling heights", false);
	}

	protected void writeTextAtRight(int line, string text, bool bold)
	{
		if(bold)
			g.SelectFontFace("Helvetica", Cairo.FontSlant.Normal, Cairo.FontWeight.Bold);

		printText(graphWidth + Convert.ToInt32(outerMargins/2), Convert.ToInt32(graphHeight/2) + textHeight*2*line, 0, textHeight, text, g, false);
		
		if(bold)
			g.SelectFontFace("Helvetica", Cairo.FontSlant.Normal, Cairo.FontWeight.Normal);
	}

	protected void endGraph()
	{
		g.GetTarget().Dispose ();
		g.Dispose ();
	}

	//TODO: fix if min == max (crashes)
	protected void paintGrid (double min, double max, int seps, bool horiz)
	{
		LogB.Information(string.Format("paintGrid: {0}, {1}, {2}, {3}", min, max, seps, horiz));

		//TODO: improve this
		if(min == max)
			return;

		//show 5 steps positive, 5 negative (if possible)
		int temp = Convert.ToInt32(Util.DivideSafe(max - min, seps));
		int step = temp;

		//to have values multiples than 10, 100 ...
		if(step <= 10)
			step = temp;
		else if(step <= 100)
			step = temp - (temp % 10);
		else if(step <= 1000)
			step = temp - (temp % 100);
		else if(step <= 10000)
			step = temp - (temp % 1000);
		else //if(step <= 100000)
			step = temp - (temp % 10000);

		//fix crash when no force
		if(step == 0)
			step = 1;

		g.Save();
		g.SetDash(new double[]{1, 2}, 0);
		// i <= max*1.5 to allow to have grid just above the maxpoint if it's below innermargins
		// see: if(ytemp < outerMargins) continue;
		for(double i = min; i <= max *1.5 ; i += step)
		{
			//LogB.Information("i: " + i.ToString());
			if(horiz)
			{
				int ytemp = Convert.ToInt32(calculatePaintY(i, graphHeight, max, min, outerMargins + innerMargins, outerMargins + innerMargins));
				if(ytemp < outerMargins)
					continue;
				g.MoveTo(outerMargins, ytemp);
				g.LineTo(graphWidth - outerMargins, ytemp);
				printText(Convert.ToInt32(outerMargins/2), ytemp, 0, textHeight, Convert.ToInt32(i).ToString(), g, true);
			} else {
				int xtemp = Convert.ToInt32(calculatePaintX(i, graphWidth, max, min, outerMargins + innerMargins, outerMargins + innerMargins));
				if(xtemp > graphWidth - outerMargins)
					continue;
				g.MoveTo(xtemp, graphHeight - outerMargins);
				g.LineTo(xtemp, outerMargins);
				printText(xtemp, graphHeight - Convert.ToInt32(outerMargins/2), 0, textHeight, Convert.ToInt32(i).ToString(), g, true);
			}
		}
		g.Stroke ();
		g.Restore();
	}

	protected double calculatePaintX(double currentValue, int ancho, double maxValue, double minValue, int rightMargin, int leftMargin)
	{
                return leftMargin + (currentValue - minValue) * (ancho - rightMargin - leftMargin) / (maxValue - minValue);
        }

	protected double calculatePaintY(double currentValue, int alto, double maxValue, double minValue, int topMargin, int bottomMargin)
	{
                return alto - bottomMargin - ((currentValue - minValue) * (alto - topMargin - bottomMargin) / (maxValue - minValue));
        }

	protected Cairo.Color colorFromRGB(int red, int green, int blue)
	{
		return new Cairo.Color(red/256.0, green/256.0, blue/256.0);
	}

	protected void printText (int x, int y, int height, int textHeight, string text, Cairo.Context g, bool centered)
	{
		int moveToLeft = 0;
		if(centered)
		{
			Cairo.TextExtents te;
			te = g.TextExtents(text);
			moveToLeft = Convert.ToInt32(te.Width/2);
		}
		g.MoveTo( x - moveToLeft, ((y+y+height)/2) + textHeight/2 );
		g.ShowText(text);
	}

	/*
	//unused code
	private void plotBars()
	{
                //calculate separation between series and bar width
                int distanceBetweenCols = Convert.ToInt32((graphWidth - rightMargin)*(1+.5)/point_l.Count) -
                        Convert.ToInt32((graphWidth - rightMargin)*(0+.5)/point_l.Count);

                int tctfSep = Convert.ToInt32(.3*distanceBetweenCols);
                int barWidth = Convert.ToInt32(.3*distanceBetweenCols);
                int barDesplLeft = Convert.ToInt32(.5*barWidth);

		int i = 10;
		int count = 0;
		//note p.X is jump fall and p.Y jump height
		//TODO: maybe this will be for a legend, because the graph wants X,Y points
		foreach(Point p in point_l)
		{
			int x = Convert.ToInt32((graphWidth - rightMargin)*(count+.5)/point_l.Count)-barDesplLeft;
			int y = calculatePaintY(Convert.ToDouble(p.X), graphHeight, pointsMaxValue, 0, topMargin, bottomMargin + bottomAxis);

			LogB.Information(string.Format("red: {0}, {1}, {2}, {3}", Convert.ToDouble(p.X), graphHeight, pointsMaxValue, y));
			drawRoundedRectangle (x, y, barWidth, graphHeight - y, 4, g, red);

			x = Convert.ToInt32((graphWidth - rightMargin)*(count+.5)/point_l.Count)-barDesplLeft+tctfSep;
			y = calculatePaintY(Convert.ToDouble(p.Y), graphHeight, pointsMaxValue, 0, topMargin, bottomMargin + bottomAxis);

			LogB.Information(string.Format("blue: {0}, {1}, {2}, {3}", Convert.ToDouble(p.Y), graphHeight, pointsMaxValue, y));
			drawRoundedRectangle (x, y, barWidth, graphHeight -y, 4, g, blue);

			count ++;
		}
	}
	*/

}