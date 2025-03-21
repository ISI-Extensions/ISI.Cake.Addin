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
using ISI.Extensions.Extensions;

namespace ISI.Cake.Addin.XmlTransform
{
	public class XmlTransformationLogger : Microsoft.Web.XmlTransform.IXmlTransformationLogger
	{
		protected global::Cake.Core.ICakeContext CakeContext { get; }

		public XmlTransformationLogger(
			global::Cake.Core.ICakeContext cakeContext)
		{
			CakeContext = cakeContext;
		}

		public void LogMessage(string message, params object[] messageArgs)
		{
			CakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Information, message, messageArgs);
		}

		public void LogMessage(Microsoft.Web.XmlTransform.MessageType messageType, string message, params object[] messageArgs)
		{
			switch (messageType)
			{
				case Microsoft.Web.XmlTransform.MessageType.Normal:
					CakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Information, message, messageArgs);
					break;
				
				case Microsoft.Web.XmlTransform.MessageType.Verbose:
					CakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Verbose, global::Cake.Core.Diagnostics.LogLevel.Information, message, messageArgs);
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null);
			}
		}

		public void LogWarning(string message, params object[] messageArgs)
		{
			CakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Warning, message, messageArgs);
		}

		public void LogWarning(string file, string message, params object[] messageArgs)
		{
			CakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Warning, "File: \"{0}\"", file);
			CakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Warning, message, messageArgs);
		}

		public void LogWarning(string file, int lineNumber, int linePosition, string message, params object[] messageArgs)
		{
			CakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Warning, "File: \"{0}\", Line: {1}, Position: {2}", file, lineNumber, linePosition);
			CakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Warning, message, messageArgs);
		}

		public void LogError(string message, params object[] messageArgs)
		{
			CakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Error, message, messageArgs);
		}

		public void LogError(string file, string message, params object[] messageArgs)
		{
			CakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Error, "File: \"{0}\"", file);
			CakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Error, message, messageArgs);
		}

		public void LogError(string file, int lineNumber, int linePosition, string message, params object[] messageArgs)
		{
			CakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Error, "File: \"{0}\", Line: {1}, Position: {2}", file, lineNumber, linePosition);
			CakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Error, message, messageArgs);
		}

		public void LogErrorFromException(Exception exception)
		{
			CakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Error, exception.ErrorMessageFormatted());
		}

		public void LogErrorFromException(Exception exception, string file)
		{
			CakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Error, "File: \"{0}\"", file);
			CakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Error, exception.ErrorMessageFormatted());
		}

		public void LogErrorFromException(Exception exception, string file, int lineNumber, int linePosition)
		{
			CakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Error, "File: \"{0}\", Line: {1}, Position: {2}", file, lineNumber, linePosition);
			CakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Error, exception.ErrorMessageFormatted());
		}

		public void StartSection(string message, params object[] messageArgs)
		{
			CakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Warning, message, messageArgs);
		}

		public void StartSection(Microsoft.Web.XmlTransform.MessageType messageType, string message, params object[] messageArgs)
		{
			switch (messageType)
			{
				case Microsoft.Web.XmlTransform.MessageType.Normal:
					CakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Information, message, messageArgs);
					break;
				
				case Microsoft.Web.XmlTransform.MessageType.Verbose:
					CakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Verbose, global::Cake.Core.Diagnostics.LogLevel.Information, message, messageArgs);
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null);
			}
		}

		public void EndSection(string message, params object[] messageArgs)
		{
			CakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Warning, message, messageArgs);
		}

		public void EndSection(Microsoft.Web.XmlTransform.MessageType messageType, string message, params object[] messageArgs)
		{
			switch (messageType)
			{
				case Microsoft.Web.XmlTransform.MessageType.Normal:
					CakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Information, message, messageArgs);
					break;
				
				case Microsoft.Web.XmlTransform.MessageType.Verbose:
					CakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Verbose, global::Cake.Core.Diagnostics.LogLevel.Information, message, messageArgs);
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null);
			}
		}
	}
}
