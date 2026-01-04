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


namespace ISI.Cake.Addin.Extensions
{
	public static class PlatformTargetExtensions
	{
		public static ISI.Extensions.VisualStudio.BuildPlatformTarget ToBuildPlatformTarget(this global::Cake.Common.Tools.MSBuild.PlatformTarget platformTarget)
		{
			switch (platformTarget)
			{
				case global::Cake.Common.Tools.MSBuild.PlatformTarget.MSIL:
					return ISI.Extensions.VisualStudio.BuildPlatformTarget.MSIL;

				case global::Cake.Common.Tools.MSBuild.PlatformTarget.x86:
					return ISI.Extensions.VisualStudio.BuildPlatformTarget.x86;
				
				case global::Cake.Common.Tools.MSBuild.PlatformTarget.x64:
					return ISI.Extensions.VisualStudio.BuildPlatformTarget.x64;
				
				case global::Cake.Common.Tools.MSBuild.PlatformTarget.ARM:
					return ISI.Extensions.VisualStudio.BuildPlatformTarget.ARM;
				
				case global::Cake.Common.Tools.MSBuild.PlatformTarget.Win32:
					return ISI.Extensions.VisualStudio.BuildPlatformTarget.Win32;
				
				case global::Cake.Common.Tools.MSBuild.PlatformTarget.ARM64:
					return ISI.Extensions.VisualStudio.BuildPlatformTarget.ARM64;
				
				case global::Cake.Common.Tools.MSBuild.PlatformTarget.ARMv6:
					return ISI.Extensions.VisualStudio.BuildPlatformTarget.ARMv6;
				
				case global::Cake.Common.Tools.MSBuild.PlatformTarget.ARMv7:
					return ISI.Extensions.VisualStudio.BuildPlatformTarget.ARMv7;
				
				case global::Cake.Common.Tools.MSBuild.PlatformTarget.ARMv7s:
					return ISI.Extensions.VisualStudio.BuildPlatformTarget.ARMv7s;
				
				default:
					throw new ArgumentOutOfRangeException(nameof(platformTarget), platformTarget, null);
			}
		}
	}
}