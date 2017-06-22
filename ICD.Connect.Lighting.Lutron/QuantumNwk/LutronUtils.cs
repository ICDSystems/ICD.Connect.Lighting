using System;
using System.Linq;
using ICD.Common.Utils;

namespace ICD.Connect.Lighting.Lutron.QuantumNwk
{
	public static class LutronUtils
	{
		public const char MODE_EXECUTE = '#';
		public const char MODE_QUERY = '?';
		public const char MODE_RESPONSE = '~';

		private static readonly char[] s_Modes = {MODE_EXECUTE, MODE_QUERY, MODE_RESPONSE};

		public const string QNET = "QNET>";

		public const string COMMAND_AREA = "AREA";
		public const string COMMAND_DEVICE = "DEVICE";
		public const string COMMAND_OUTPUT = "OUTPUT";
		public const string COMMAND_GROUP = "GROUP";
		public const string COMMAND_SHADEGROUP = "SHADEGRP";
		public const string COMMAND_PING = "PING";

		public const string COMMAND_MONITORING = "MONITORING";
		public const string COMMAND_ERROR = "ERROR";
		public const string COMMAND_HELP = "HELP";
		public const string COMMAND_SYSTEM = "SYSTEM";

		public const char DELIMITER = ',';
		public const char TIME_DELIMITER = ':';

		public const string CRLF = "\x0D\x0A";

		public const string NULL_VALUE = "xx";

		public const int ERROR_PARAMETER_COUNT = 1;
		public const int ERROR_OBJECT_DOES_NOT_EXIST = 2;
		public const int ERROR_INVALID_ACTION_NUMBER = 3;
		public const int ERROR_PARAMETER_OUT_OF_RANGE = 4;
		public const int ERROR_PARAMETER_MALFORMED = 5;
		public const int ERROR_UNSUPPORTED_COMMAND = 6;

		// Lutron rounds to lowest quarter, so this should be adequate for float comparisons.
		public const float FLOAT_TOLERANCE = 0.1f;

		/// <summary>
		/// Builds a data string for an integration command.
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="command"></param>
		/// <param name="integrationId"></param>
		/// <param name="actionNumber"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public static string BuildData(char mode, string command, int integrationId, int actionNumber,
		                               params object[] parameters)
		{
			string[] paramsArray = parameters.Select<object, string>(ParameterToString)
			                                 .ToArray();

			string paramsString = string.Join(DELIMITER.ToString(), paramsArray);

			return string.Format("{0}{1}{2}{3}{2}{4}{2}{5}{6}", mode, command, DELIMITER, integrationId, actionNumber,
			                     paramsString, CRLF);
		}

		/// <summary>
		/// Converts a parameter to a string representation.
		/// </summary>
		/// <param name="parameter"></param>
		/// <returns></returns>
		public static string ParameterToString(object parameter)
		{
			return parameter == null ? NULL_VALUE : parameter.ToString();
		}

		/// <summary>
		/// Converts a TimeSpan to a Lutron time parameter (HH:MM:SS)
		/// </summary>
		/// <param name="span"></param>
		/// <returns></returns>
		public static string TimeSpanToParameter(TimeSpan span)
		{
			return string.Format("{0:00}{1}{2:00}{1}{3:00}", span.Hours, TIME_DELIMITER, span.Minutes, span.Seconds);
		}

		/// <summary>
		/// Converts a parameter string to a TimeSpan.
		/// </summary>
		/// <param name="parameter"></param>
		/// <returns></returns>
		public static TimeSpan ParameterToTimeSpan(string parameter)
		{
			string[] split = parameter.Split(TIME_DELIMITER).Reverse().ToArray();

			string secondsString = split.FirstOrDefault();
			string minutesString = split.Skip(1).FirstOrDefault();
			string hoursString = split.Skip(2).FirstOrDefault();

			int seconds;
			int minutes;
			int hours;

			StringUtils.TryParse(secondsString, out seconds);
			StringUtils.TryParse(minutesString, out minutes);
			StringUtils.TryParse(hoursString, out hours);

			return new TimeSpan(hours, minutes, seconds);
		}

		/// <summary>
		/// Gets a string representation of the percentage.
		/// </summary>
		/// <param name="percentage">Percentage between 0.0f and 1.0f</param>
		/// <returns>String in the format 0.00 to 100.00</returns>
		public static string FloatToPercentageParameter(float percentage)
		{
			return string.Format("{0:0.00}", percentage * 100.0f);
		}

		/// <summary>
		/// Converts the percentage string representation to a float.
		/// </summary>
		/// <param name="parameter"></param>
		/// <returns></returns>
		public static float PercentageParameterToFloat(string parameter)
		{
			float percentage = float.Parse(parameter);
			return percentage / 100.0f;
		}

		/// <summary>
		/// Returns the mode from the data string.
		/// </summary>
		/// <param name="data">Serial data to/from the lighting processor.</param>
		/// <returns>The first character of the data.</returns>
		public static char GetMode(string data)
		{
			return data[0];
		}

		/// <summary>
		/// Returns the command from the data string.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static string GetCommand(string data)
		{
			data = TrimMode(data);
			string[] split = SplitData(data);
			return split[0];
		}

		/// <summary>
		/// Returns the integration id from the data string.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static int GetIntegrationId(string data)
		{
			string[] split = SplitData(data);
			string integrationIdString = split.Length > 1 ? split[1] : string.Empty;

			int integrationId;
			if (StringUtils.TryParse(integrationIdString, out integrationId))
				return integrationId;

			string message = string.Format("Data \"{0}\" has no Integration ID", data);
			throw new FormatException(message);
		}

		/// <summary>
		/// Returns the integration action number from the data string.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static int GetIntegrationActionNumber(string data)
		{
			string[] split = SplitData(data);
			string actionNumberString = split.Length > 2 ? split[2] : string.Empty;

			int actionNumber;
			if (StringUtils.TryParse(actionNumberString, out actionNumber))
				return actionNumber;

			string message = string.Format("Data \"{0}\" has no Action Number", data);
			throw new FormatException(message);
		}

		/// <summary>
		/// Splits a data string on the delimiter.
		/// e.g. #AREA,2,1,70,4,2\x0D\x0A
		/// becomes { "#AREA", "2", "1", "70", "4", "2\x0D\x0A" }
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static string[] SplitData(string data)
		{
			return data.Split(DELIMITER);
		}

		/// <summary>
		/// Returns the integration action parameters for the data string.
		/// e.g. #AREA,2,1,70,4,2\x0D\x0A
		/// becomes { "70", "4", "2" }
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static string[] GetIntegrationActionParameters(string data)
		{
			data = TrimCrlf(data);
			// Skip command, integration and action
			return SplitData(data).Skip(3).ToArray();
		}

		/// <summary>
		/// Returns the data string without the mode.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static string TrimMode(string data)
		{
			if (s_Modes.Any(m => data.StartsWith(m.ToString())))
				data = data.Substring(1);
			return data;
		}

		/// <summary>
		/// Returns the data string without the trailing CRLF.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static string TrimCrlf(string data)
		{
			if (data.EndsWith(CRLF))
				data = data.Substring(0, data.Length - CRLF.Length);
			return data;
		}

		/// <summary>
		/// Creates a key for the given command and integration id.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="integrationId"></param>
		/// <returns></returns>
		public static string GetKey(string command, int integrationId)
		{
			return string.Format("{0}{1}{2}", command, DELIMITER, integrationId);
		}

		/// <summary>
		/// Gets the key for the given data string.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static string GetKeyFromData(string data)
		{
			string command = GetCommand(data);
			int integrationId = GetIntegrationId(data);
			return GetKey(command, integrationId);
		}
	}
}
