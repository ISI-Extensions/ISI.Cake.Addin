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

namespace ISI.Cake.Addin
{
	public static partial class Aliases
	{
		[global::Cake.Core.Annotations.CakeMethodAlias]
		public static void NupkgPush(this global::Cake.Core.ICakeContext cakeContext, global::Cake.Core.IO.FilePath nupkgFilePath, NupkgPushToolSettings nupkgPushToolSettings)
		{
			NupkgPush(cakeContext, new[] { nupkgFilePath }, nupkgPushToolSettings);
		}

		[global::Cake.Core.Annotations.CakeMethodAlias]
		public static void NupkgPush(this global::Cake.Core.ICakeContext cakeContext, IEnumerable<global::Cake.Core.IO.FilePath> nupkgFilePaths, NupkgPushToolSettings nupkgPushToolSettings)
		{
			NupkgPush(cakeContext, nupkgFilePaths.Select(filePath => filePath.FullPath), nupkgPushToolSettings);
		}

		[global::Cake.Core.Annotations.CakeMethodAlias]
		public static void NupkgPush(this global::Cake.Core.ICakeContext cakeContext, string nupkgFullName, NupkgPushToolSettings nupkgPushToolSettings)
		{
			NupkgPush(cakeContext, new[] { nupkgFullName }, nupkgPushToolSettings);
		}

		[global::Cake.Core.Annotations.CakeMethodAlias]
		public static void NupkgPush(this global::Cake.Core.ICakeContext cakeContext, IEnumerable<string> nupkgFullNames, NupkgPushToolSettings nupkgPushToolSettings)
		{
			void copyToNugetCacheDirectory(string fullName)
			{
				if (!string.IsNullOrWhiteSpace(nupkgPushToolSettings.NugetCacheDirectory?.FullPath))
				{
					System.IO.File.Copy(fullName, System.IO.Path.Combine(nupkgPushToolSettings.NugetCacheDirectory?.FullPath, System.IO.Path.GetFileName(fullName)));
				}

			}

			if (nupkgPushToolSettings.UseNugetPush)
			{
				foreach (var nupkgFullName in nupkgFullNames)
				{
					ISI.Cake.Addin.Process.WaitForProcessResponse(cakeContext, "nuget", string.Format("push \"{0}\"", nupkgFullName), nupkgPushToolSettings.ApiKey, string.Format("/Source \"{0}\"", nupkgPushToolSettings.RepositoryUri));

					copyToNugetCacheDirectory(nupkgFullName);
				}
			}
			else
			{
				foreach (var nupkgFullName in nupkgFullNames)
				{
					var fileSegments = new Queue<byte[]>();

					using (var ms = new System.IO.MemoryStream())
					{
						using (System.IO.Stream iStream = System.IO.File.OpenRead(nupkgFullName))
						{
							const int chunkSize = 2048;
							var buffer = new byte[chunkSize];
							var readBlocks = 0;

							readBlocks = iStream.Read(buffer, 0, chunkSize);

							while (readBlocks > 0)
							{
								ms.Write(buffer, 0, readBlocks);
								readBlocks = (readBlocks >= chunkSize ? iStream.Read(buffer, 0, chunkSize) : 0);

								ms.Flush();

								if ((ms.Position > 0) && ((readBlocks == 0) || (ms.Position + chunkSize > nupkgPushToolSettings.MaxFileSegmentSize)))
								{
									ms.Position = 0;
									fileSegments.Enqueue(ms.ToArray());
									ms.SetLength(0);
								}
							}
						}
					}


					while (fileSegments.Any())
					{
						var data = fileSegments.Dequeue();

						var tryAttemptsLeft = nupkgPushToolSettings.MaxTries;
						while (tryAttemptsLeft > 0)
						{
							try
							{
								System.Net.ServicePointManager.Expect100Continue = true;
								//System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Ssl3;
								System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls;

								var webRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(string.Format("{0}api/v2/package-chunk", nupkgPushToolSettings.RepositoryUri.ToString()));
								cakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Information, string.Format("  webRequest '{0}'.", string.Format("{0}api/v2/package-chunk", nupkgPushToolSettings.RepositoryUri.ToString())));

								webRequest.Method = System.Net.WebRequestMethods.Http.Post;
								webRequest.ContentType = "application/x-www-form-urlencoded";

								var content = new StringBuilder();

								content.AppendFormat("{0}={1}&", "fileName", System.Web.HttpUtility.UrlEncode(System.IO.Path.GetFileName(nupkgFullName)));
								content.AppendFormat("{0}={1}&", "finalSegment", System.Web.HttpUtility.UrlEncode((fileSegments.Any() ? "false" : "true")));
								cakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Information, string.Format("  Content '{0}'.", content.ToString()));

								content.AppendFormat("{0}={1}&", "apiKey", System.Web.HttpUtility.UrlEncode(nupkgPushToolSettings.ApiKey));
								content.AppendFormat("{0}={1}&", "file", System.Web.HttpUtility.UrlEncode(Convert.ToBase64String(data)));

								var encodedRequest = (new ASCIIEncoding()).GetBytes(content.ToString());

								using (var requestStream = webRequest.GetRequestStream())
								{
									requestStream.Write(encodedRequest, 0, encodedRequest.Length);

									using (var response = webRequest.GetResponse())
									{
										if (((System.Net.HttpWebResponse)response).StatusCode != System.Net.HttpStatusCode.OK)
										{
											throw new Exception(string.Format("{0}: {1}", ((System.Net.HttpWebResponse)response).StatusCode, ((System.Net.HttpWebResponse)response).StatusDescription));
										}
									}

									requestStream.Close();
								}

								tryAttemptsLeft = 0;
							}
							catch (Exception exception)
							{
								cakeContext.Log.Write(global::Cake.Core.Diagnostics.Verbosity.Normal, global::Cake.Core.Diagnostics.LogLevel.Information, exception.Message);
								tryAttemptsLeft--;
								if (tryAttemptsLeft < 0)
								{
									throw;
								}

								System.Threading.Thread.Sleep(5000);
							}
						}

						copyToNugetCacheDirectory(nupkgFullName);
					}
				}
			}
		}
	}
}