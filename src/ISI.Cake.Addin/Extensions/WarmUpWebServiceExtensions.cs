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
	public static class WarmUpWebServiceExtensions
	{
		public static readonly HashSet<string> WarmedUpWebServiceHosts = new(StringComparer.InvariantCultureIgnoreCase);

		public static void WarmUpWebService(this IWarmUpWebService request, global::Cake.Core.Diagnostics.ICakeLog log)
		{
			ServiceProvider.Initialize();

			if (request.WarmUpWebService)
			{
				var webServiceUrls = new[]
				{
					(new UriBuilder(request.WebServiceUri) {Query = string.Empty}).Uri.ToString(),
					(new UriBuilder(request.WebServiceUri) {Path = string.Empty, Query = string.Empty}).Uri.ToString(),
				};

				foreach (var webServiceUrl in webServiceUrls)
				{
					var webServerHost = (new UriBuilder(webServiceUrl)).Host;

					if (!WarmedUpWebServiceHosts.Contains(webServerHost))
					{
						log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Information, "Warming up: {0}", webServiceUrl);

						var tryAttemptsLeft = request.WarmUpWebServiceMaxTries;
						while (tryAttemptsLeft > 0)
						{
							try
							{
								var restResponse = ISI.Extensions.WebClient.Rest.ExecuteGet<ISI.Extensions.WebClient.Rest.TextResponse>(webServiceUrl, new(), false);

								tryAttemptsLeft = 0;
							}
							catch (Exception exception)
							{
								log.Error(exception);

								tryAttemptsLeft--;
								if (tryAttemptsLeft < 0)
								{
									throw;
								}

								System.Threading.Thread.Sleep(TimeSpan.FromSeconds(10));
							}
						}

						WarmedUpWebServiceHosts.Add(webServerHost);
					}
				}
			}
		}
	}
}
