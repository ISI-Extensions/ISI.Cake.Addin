#region Copyright & License
/*
Copyright (c) 2021, Integrated Solutions, Inc.
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cake.Common.IO;

namespace ISI.Cake.Addin.PackageComponents
{
	public static partial class Aliases
	{
		[global::Cake.Core.Annotations.CakeMethodAlias]
		public static PackageComponentsResponse PackageComponents(this global::Cake.Core.ICakeContext cakeContext, PackageComponentsRequest request)
		{
			var response = new PackageComponentsResponse();

			using (var tempDirectory = new ISI.Extensions.IO.Path.TempDirectory())
			{
				var packageComponentsDirectory = tempDirectory.FullName;
				//var packageComponentsDirectory = ISI.Extensions.IO.Path.GetTempDirectoryName();

				if (!string.IsNullOrWhiteSpace(request.SubDirectory))
				{
					packageComponentsDirectory = System.IO.Path.Combine(packageComponentsDirectory, request.SubDirectory);
				}

				foreach (var packageComponent in request.PackageComponents)
				{
					switch (packageComponent)
					{
						case PackageComponentConsoleApplication packageComponentConsoleApplication:
							BuildPackageComponentConsoleApplication(cakeContext, request.Configuration, request.Platform, packageComponentsDirectory, packageComponentConsoleApplication);
							break;

						case PackageComponentWindowsService packageComponentWindowsService:
							BuildPackageComponentWindowsService(cakeContext, request.Configuration, request.Platform, packageComponentsDirectory, packageComponentWindowsService);
							break;

						case PackageComponentWebSite packageComponentWebSite:
							BuildPackageComponentWebSite(cakeContext, request.Configuration, request.Platform, packageComponentsDirectory, packageComponentWebSite);
							break;

						default:
							throw new ArgumentOutOfRangeException(nameof(packageComponent));
					}
				}

				System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(request.PackageComponentsFullName));

				cakeContext.Zip(cakeContext.Directory(packageComponentsDirectory), cakeContext.File(request.PackageComponentsFullName));
			}

			return response;
		}
	}
}