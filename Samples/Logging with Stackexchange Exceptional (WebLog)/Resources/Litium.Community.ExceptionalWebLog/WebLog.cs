using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Web;
using Litium;
#if NET472
using Litium.Validations;
#endif
using Newtonsoft.Json;
using StackExchange.Exceptional;

namespace Community.WebLog.Exceptional
{
	internal class WebLog :  IWebLog
	{
		private static bool _logStackTrace;

		/// <summary>
		///     Initializes a new instance of the <see cref="T:System.Object" /> class.
		/// </summary>
		public WebLog()
		{
			bool.TryParse(ConfigurationManager.AppSettings["WebLog:StackTrace"], out _logStackTrace);
			StackExchange.Exceptional.Exceptional.Configure(settings => settings.GetCustomData = (exception, context) =>
			{
				var customData = GetCustomData(false);
				if (customData != null)
				{
					foreach (var item in customData)
					{
						context.Add(item.Key, item.Value);
					}
				}
			});
		}

		/// <summary>
		///     Appends the specified message to the
		///     <see cref="HttpContext" />.
		///     The message are appended to the log when
		///     <see cref="IWebLog.Error" /> are called.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="message">The message.</param>
		/// <param name="arguments">The arguments.</param>
		/// <exception cref="System.ArgumentException">Parameter name are required and cant be null or empty.;name</exception>
		/// <exception cref="System.NotImplementedException"></exception>
		public void Append(string key, string message, params object[] arguments)
		{
			if (string.IsNullOrWhiteSpace(key))
			{
				throw new ArgumentException("Parameter key are required and cant be null or empty.", "key");
			}
			if (arguments == null || arguments.Length == 0)
			{
				Add(key, message, 2);
			}
			else
			{
				Add(key, string.Format(message, arguments), 2);
			}
		}

		/// <summary>
		///     Appends the specified object to the <see cref="HttpContext" />.
		///     The object will be json-serialized.
		///     The message are appended to the log when <see cref="IWebLog.Error" /> are called.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="data">The data.</param>
		public void Append(string name, object data)
		{
			if (data == null)
			{
				Append(name, "[null]");
				return;
			}

			var serializationErrors = new StringBuilder();
			var hasSerializationErrors = false;
			var settings = new JsonSerializerSettings
			{
				Error = (sender, args) =>
				{
					serializationErrors.AppendLine(args.ErrorContext.Error.Message);
					serializationErrors.AppendLine(args.ErrorContext.Error.InnerException != null ? args.ErrorContext.Error.InnerException.ToString() : string.Empty);
					serializationErrors.AppendLine();
					args.ErrorContext.Handled = true;
					hasSerializationErrors = true;
				},
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				Formatting = Formatting.Indented,
				TypeNameHandling = TypeNameHandling.Objects
			};

			var body = new StringBuilder();
			Action<Func<string>> safeMethod = x =>
			{
				try
				{
					body.AppendLine(x());
				}
				catch (Exception ex)
				{
					body.AppendLine(ex.Message);
					body.AppendLine(ex.InnerException != null ? ex.InnerException.ToString() : string.Empty);
					body.AppendLine();
				}
			};
			safeMethod(() => JsonConvert.SerializeObject(data, settings));
			if (hasSerializationErrors)
			{
				safeMethod(serializationErrors.ToString);
			}

			Add(name, body.ToString(), 2);
		}

		/// <summary>
		///     Create and error-log with the specific <see cref="Exception" />.
		///     The log will contains all the information that previous are appended in the same <see cref="HttpContext" />.
		/// </summary>
		/// <param name="exception">The exception.</param>
		public void Error(Exception exception)
		{

			var err = exception.Log(HttpContext.Current);
#if NET472
		    if (exception is ValidationException)
		    {
                StringBuilder exMessage = new StringBuilder();
                foreach (var eror in ((ValidationException)exception).ValidationResult.Errors)
                {
                    exMessage.Append(eror.Key + ":");
                    foreach (string value in eror.Value)
                    {
                        exMessage.Append(value + ";");
                    }
                }
                typeof(IWebLog).Log().Error("IWebLog exception '" + exception.Message + "': " + err.GUID + "(" + exMessage +")", exception);
            }
            else
#endif
            {
                typeof(IWebLog).Log().Error("IWebLog exception '" + exception.Message + "': " + err.GUID, exception);
            }
		}

		private void Add(string key, string message, int skipFrames)
		{
			var items = GetCustomData();
			string current;
			if (items.TryGetValue(key, out current))
			{
				current += Environment.NewLine + Environment.NewLine;
			}
			current += message;
			items[key] = current;

			if (_logStackTrace)
			{
				var frames = new StackTrace(fNeedFileInfo: true).GetFrames();
				if (frames != null)
				{
					var displayFilenames = true;
					bool fFirstFrame = true;
					var sb = new StringBuilder(255);
					foreach (var sf in frames.Skip(skipFrames))
					{
						var mb = sf.GetMethod();
						if (mb != null)
						{
							// We want a newline at the end of every line except for the last
							if (fFirstFrame)
							{
								fFirstFrame = false;
							}
							else
							{
								sb.Append(Environment.NewLine);
							}

							sb.AppendFormat(CultureInfo.InvariantCulture, "   {0} ", "at");

							Type t = mb.DeclaringType;
							// if there is a type (non global method) print it
							if (t != null)
							{
								sb.Append(t.FullName.Replace('+', '.'));
								sb.Append(".");
							}
							sb.Append(mb.Name);

							// deal with the generic portion of the method 
							if (mb is MethodInfo && mb.IsGenericMethod)
							{
								Type[] typars = mb.GetGenericArguments();
								sb.Append("[");
								int k = 0;
								bool fFirstTyParam = true;
								while (k < typars.Length)
								{
									if (fFirstTyParam == false)
									{
										sb.Append(",");
									}
									else
									{
										fFirstTyParam = false;
									}

									sb.Append(typars[k].Name);
									k++;
								}
								sb.Append("]");
							}

							// arguments printing
							sb.Append("(");
							ParameterInfo[] pi = mb.GetParameters();
							bool fFirstParam = true;
							foreach (ParameterInfo t1 in pi)
							{
								if (fFirstParam == false)
								{
									sb.Append(", ");
								}
								else
								{
									fFirstParam = false;
								}

								string typeName = t1.ParameterType.Name;
								sb.Append(typeName + " " + t1.Name);
							}
							sb.Append(")");

							// source location printing
							if (displayFilenames && (sf.GetILOffset() != -1))
							{
								// If we don't have a PDB or PDB-reading is disabled for the module,
								// then the file name will be null. 
								String fileName = null;

								// Getting the filename from a StackFrame is a privileged operation - we won't want 
								// to disclose full path names to arbitrarily untrusted code.  Rather than just omit
								// this we could probably trim to just the filename so it's still mostly usefull.
								try
								{
									fileName = sf.GetFileName();
								}
								catch (SecurityException)
								{
									// If the demand for displaying filenames fails, then it won't 
									// succeed later in the loop.  Avoid repeated exceptions by not trying again.
									displayFilenames = false;
								}

								if (fileName != null)
								{
									// tack on " in c:\tmp\MyFile.cs:line 5" 
									sb.Append(' ');
									sb.AppendFormat(CultureInfo.InvariantCulture, " in {0}:line {1}", fileName, sf.GetFileLineNumber());
								}
							}
						}
					}
					sb.Append(Environment.NewLine);

					items[key + "-StackTrace"] = "StackTrace:\n\n" + sb;
				}
			}
		}

		private Dictionary<string, string> GetCustomData(bool clearAfterGet = false)
		{
			var context = HttpContext.Current;
			if (context == null)
			{
				return null;
			}

			lock (context)
			{
				var items = context.Items[typeof(WebLog).FullName] as Dictionary<string, string>;
				if (items == null)
				{
					items = new Dictionary<string, string>();

					if (!clearAfterGet)
					{
						context.Items[typeof(WebLog).FullName] = items;
					}
				}

				if (clearAfterGet)
				{
					context.Items[typeof(WebLog).FullName] = null;
				}

				return items;
			}
		}
	}
}
