#region Copyright & License
/*
Copyright (c) 2026, Integrated Solutions, Inc.
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
using Cake.Core.Diagnostics;

namespace ISI.Cake.Addin.Extensions
{
	public static class MSBuildPlatformExtensions
	{
		public static ISI.Extensions.VisualStudio.MSBuildPlatform ToMSBuildPlatform(this global::Cake.Common.Tools.MSBuild.MSBuildPlatform msBuildPlatform)
		{
			switch (msBuildPlatform)
			{
				case global::Cake.Common.Tools.MSBuild.MSBuildPlatform.Automatic:
					return ISI.Extensions.VisualStudio.MSBuildPlatform.Automatic;
				case global::Cake.Common.Tools.MSBuild.MSBuildPlatform.x86:
					return ISI.Extensions.VisualStudio.MSBuildPlatform.x86;
				case global::Cake.Common.Tools.MSBuild.MSBuildPlatform.x64:
					return ISI.Extensions.VisualStudio.MSBuildPlatform.x64;
				default:
					throw new ArgumentOutOfRangeException(nameof(msBuildPlatform), msBuildPlatform, null);
			}
		}
	}
}