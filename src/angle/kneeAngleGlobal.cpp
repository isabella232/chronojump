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
 * Initally coded by (v.1.0):
 * Sharad Shankar & Onkar Nath Mishra
 * http://www.logicbrick.com/
 * 
 * Updated by:
 * Xavier de Blas 
 * xaviblas@gmail.com
 *
 *
 */

//config variables
bool showContour = true;
bool debug = false;
int playDelay = 10; //milliseconds between photogrammes wen playing. Used as a waitkey.
//not put values lower than 5 or the enter when executing will be the first pause
//eg: 5 (fast) 1000 (one second each photogramme)
//int playDelayFoundAngle = 150; //as above, but used when angle is found.
int playDelayFoundAngle = 50; //as above, but used when angle is found.
//Useful to see better the detected angle when something is detected
//recommended values: 50 - 200



/* recommended:
	   showAtLinesPoints = true
	   ...DiffPoints = true
	   ...SamePoints = true
	   ...OnlyStartMinEnd = true;
	   */

bool showStickThePoints = true;
bool showStickTheLinesBetweenSamePoints = true;
bool showStickTheLinesBetweenDifferentPoints = true;
bool showStickOnlyStartMinEnd = true;
bool mixStickWithMinAngleWindow = true;

int startAt = 1;


CvScalar WHITE = CV_RGB(255,255,255);
CvScalar BLACK = CV_RGB(0 ,0 , 0);
CvScalar RED = CV_RGB(255, 0, 0);
CvScalar GREEN = CV_RGB(0 ,255, 0);
CvScalar BLUE = CV_RGB(0 ,0 ,255);
CvScalar GREY = CV_RGB(128,128,128);
CvScalar YELLOW = CV_RGB(255,255, 0);
CvScalar MAGENTA = CV_RGB(255, 0,255);
CvScalar CYAN = CV_RGB( 0,255,255); //check

enum { blackAndMarkers = 0, blackOnlyMarkers = 1, skinOnlyMarkers = 2 }; 
enum { SMALL = 1, MID = 2, BIG = 3 }; 
enum { NO = 0, YES = 1, FORWARD = 2, SUPERFORWARD = 3, BACKWARD = 4, QUIT = 5 }; 

double zoomScale = 2; 

CvPoint hipMouse;
CvPoint kneeMouse;
CvPoint toeMouse;

bool forceMouseHip = false;
bool forceMouseKnee = false;
bool forceMouseToe = false;

bool zoomed = false;

bool mouseCanMark = false; 


