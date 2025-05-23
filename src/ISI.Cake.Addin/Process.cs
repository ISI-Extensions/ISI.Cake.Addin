#region Copyright & License
/*
Copyright (c) 2025, Integrated Solutions, Inc.
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

		* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
		* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
		* Neither the name of the Integrated Solutions, Inc. nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion
 
using System;
using System.Collections.Generic;
using System.Text;

namespace ISI.Cake.Addin
{
	public class Process
	{
		public delegate void ProcessResponse_OnChange(bool isAppend, string output);

		public class ProcessResponse
		{
			private string _output = string.Empty;
			public string Output
			{
				get => _output;
				set { _output = value; OnChange?.Invoke(false, value); }
			}

			public int ExitCode { get; set; }
			public bool Errored => (ExitCode != 0);

			public ProcessResponse_OnChange OnChange { get; set; } = null;

			public void Reset()
			{
				Output = string.Empty;
				ExitCode = 0;
			}

			public void AppendLine(string output)
			{
				output += "\r\n";
				_output += output;

				OnChange?.Invoke(true, output);
			}
		}

		public static ProcessResponse WaitForProcessResponse(global::Cake.Core.ICakeContext cakeContext, string processExeFullName, params string[] arguments)
		{
			var response = new ProcessResponse();

			var output = new StringBuilder();

			var processStartInfo = new System.Diagnostics.ProcessStartInfo(processExeFullName)
			{
				Arguments = string.Join(" ", arguments ?? []),
				WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				WorkingDirectory = cakeContext.Environment.WorkingDirectory.FullPath,
			};

			using (var process = System.Diagnostics.Process.Start(processStartInfo))
			{
				process.OutputDataReceived += (sender, dataReceivedEventArgs) =>
				{
					var data = dataReceivedEventArgs.Data;

					if (!string.IsNullOrEmpty(data))
					{
						output.AppendLine(data);

						cakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Information, data);
					}
				};

				process.ErrorDataReceived += (sender, dataReceivedEventArgs) =>
				{
					var data = dataReceivedEventArgs.Data;

					if (!string.IsNullOrEmpty(data))
					{
						output.AppendLine(data);

						cakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Information, data);
					}
				};

				process.BeginOutputReadLine();
				process.WaitForExit();

				response.ExitCode = process.ExitCode;

				process.Close();

				response.Output = output.ToString();

				return response;
			}
		}
	}
}
