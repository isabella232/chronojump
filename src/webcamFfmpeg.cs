/*
 * This file is part of ChronoJump
 *
 * ChronoJump is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or   
 * (at your option) any later version.
 *    
 * ChronoJump is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 *  Copyright (C) 2018   Xavier de Blas <xaviblas@gmail.com> 
 */

using System.Collections.Generic; //List
using System.Diagnostics;
using System;
using System.IO;

public class WebcamFfmpeg : Webcam
{
	public WebcamFfmpeg (string videoDevice)
	{
		this.videoDevice = videoDevice;
		captureExecutable = "ffmpeg";
		Running = false;
	}

	/*
	 * constructor for Play

	public WebcamFfmpeg ()
	{
	}
	 */

	public override Result CapturePrepare (CaptureTypes captureType)
	{
		if(process != null)
			return new Result (false, "");

		return new Result (true, "");
	}

	public override Result Play(string filename)
	{
		//only implemented on mplayer
		return new Result (true, "");
	}
	public override bool Snapshot()
	{
		//only implemented on mplayer
		return true;
	}

	public override Result VideoCaptureStart()
	{
		process = new Process();
		List<string> parameters = createParametersOnlyCapture();
		//List<string> parameters = createParametersCaptureAndDelayedView();
		bool success = ExecuteProcess.RunAtBackground (process, captureExecutable, parameters, true); //redirectInput
		if(! success)
		{
			streamWriter = null;
			process = null;
			return new Result (false, "", programFfmpegNotInstalled);
		}

		streamWriter = process.StandardInput;
		Running = true;

		return new Result (true, "");
	}

	private List<string> createParametersOnlyCapture()
	{
		// ffmpeg -y -f v4l2 -framerate 30 -video_size 640x480 -input_format mjpeg -i /dev/video0 out.mp4
		List<string> parameters = new List<string>();

		int i = 0;
		parameters.Insert (i ++, "-y"); //overwrite
		parameters.Insert (i ++, "-f");
		parameters.Insert (i ++, "v4l2");
		parameters.Insert (i ++, "-framerate");
		parameters.Insert (i ++, "30");
		parameters.Insert (i ++, "-video_size");
		parameters.Insert (i ++, "640x480");
		parameters.Insert (i ++, "-input_format");
		parameters.Insert (i ++, "mjpeg");
		parameters.Insert (i ++, "-i");
		parameters.Insert (i ++, videoDevice);
		parameters.Insert (i ++, Util.GetVideoTempFileName());

		return parameters;
	}

	//Care: press q two times, one for each process on the tee
	//or only one on the ffplay process
	private List<string> createParametersCaptureAndDelayedView()
	{
		//ffmpeg -y -f v4l2 -i /dev/video0 -map 0 -c:v libx264 -f tee "output.mkv|[f=nut]pipe:" | ffplay pipe:
		List<string> parameters = new List<string>();

		int i = 0;
		parameters.Insert (i ++, "-y"); //overwrite
		parameters.Insert (i ++, "-f");
		parameters.Insert (i ++, "v4l2");
		parameters.Insert (i ++, "-i");
		parameters.Insert (i ++, videoDevice);
		parameters.Insert (i ++, "-map");
		parameters.Insert (i ++, "0");
		parameters.Insert (i ++, "-c:v");
		parameters.Insert (i ++, "libx264");
		parameters.Insert (i ++, "-f");
		parameters.Insert (i ++, "tee");
		parameters.Insert (i ++, "'" + Util.GetVideoTempFileName() + "|[f=nut]pipe:'");
		parameters.Insert (i ++, "|");
		parameters.Insert (i ++, "ffplay");
		parameters.Insert (i ++, "pipe:");

		return parameters;
	}

	/*
	 * there are problems calling the process with the "|"
	 * better call a shell script like this:
	 * ffmpeg_capture_and_play.sh
	 *
	 * #!/bin/bash
	 * ffmpeg -y -f v4l2 -i /dev/video0 -map 0 -c:v libx264 -f tee "/tmp/chronojump-last-video.mp4|[f=nut]pipe:" | ffplay pipe:
	 */


	public override Result VideoCaptureEnd()
	{
		//on ffmpeg capture ends on exit: 'q' done at ExitAndFinish()
		return new Result (true, "");
	}


	public override Result ExitAndFinish (int sessionID, Constants.TestTypes testType, int testID)
	{
		ExitCamera();

		//Copy the video to expected place
		if (! Util.CopyTempVideo(sessionID, testType, testID))
			return new Result (false, "", Constants.FileCopyProblem);

		//Delete temp video
		deleteTempFiles();

		return new Result (true, "");
	}

	public override void ExitCamera()
	{
		LogB.Information("Exit camera");
		LogB.Information("streamWriter is null: " + (streamWriter == null).ToString());
		try {
			streamWriter.Write('q');
			streamWriter.Flush();
		} catch {
			//maybe Mplayer window has been closed by user
			process = null;
			Running = false;
			return;
		}

		//System.Threading.Thread.Sleep(500);
		//better check if process still exists to later copy the video
		do {
			LogB.Information("waiting 100 ms to end Ffmpeg");
			System.Threading.Thread.Sleep(100);
		} while(ExecuteProcess.IsRunning2(process, captureExecutable));

		streamWriter = null;
		process = null;
		Running = false;
	}

	/*
	 * protected methods
	 */

	protected override void deleteTempFiles()
	{
		LogB.Information("Deleting temp video");
		if(File.Exists(Util.GetVideoTempFileName()))
			File.Delete(Util.GetVideoTempFileName());
	}

}