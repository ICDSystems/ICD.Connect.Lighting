using System.Collections.Generic;
using System.Text;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Protocol.SerialBuffers;

namespace ICD.Connect.Lighting.Lutron.Nwk
{
	/// <summary>
	/// The LutronQuantumNwkSerialBuffer splits incoming data on a number of strings.
	/// </summary>
	public sealed class LutronQuantumNwkSerialBuffer : AbstractSerialBuffer
	{
		private static readonly string[] s_Delimiters =
		{
			LutronUtils.CRLF,
			LutronUtils.QNET,
			LutronUtils.QSE
		};

		// These cause the buffer to immideately return, incluing the short circuit string
		private static readonly string[] s_ShortCircuits =
		{
			LutronUtils.LOGIN_PROMPT
		};

		private readonly StringBuilder m_RxData;

		/// <summary>
		/// Constructor.
		/// </summary>
		public LutronQuantumNwkSerialBuffer()
		{
			m_RxData = new StringBuilder();
		}

		/// <summary>
		/// Override to clear any current state.
		/// </summary>
		protected override void ClearFinal()
		{
			m_RxData.Clear();
		}

		/// <summary>
		/// Override to process the given item for chunking.
		/// </summary>
		/// <param name="data"></param>
		protected override IEnumerable<string> Process(string data)
		{
			while (true)
			{
				string delimiter, shortCircuit;
				int delimiterIndex = data.IndexOf(s_Delimiters, out delimiter);
				int shortCircuitIndex = data.IndexOf(s_ShortCircuits, out shortCircuit);

				if (delimiterIndex < 0 && shortCircuitIndex < 0)
				{
					m_RxData.Append(data);
					break;
				}

				if (delimiterIndex >= 0)
				{
					m_RxData.Append(data.Substring(0, delimiterIndex));
					data = data.Substring(delimiterIndex + delimiter.Length);
				}
				else
				{
					// Short Circuit includes the short circuit string, delimiter does not
					m_RxData.Append(data.Substring(0, shortCircuitIndex + shortCircuit.Length));
					data = data.Substring(shortCircuitIndex + shortCircuit.Length);
				}

				yield return m_RxData.Pop();
			}
		}
	}
}
