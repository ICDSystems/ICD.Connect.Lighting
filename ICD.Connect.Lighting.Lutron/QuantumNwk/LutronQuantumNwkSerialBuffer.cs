﻿using System;
using System.Collections.Generic;
using System.Text;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Protocol.SerialBuffers;

namespace ICD.Connect.Lighting.Lutron.QuantumNwk
{
	/// <summary>
	/// The LutronQuantumNwkSerialBuffer splits incoming data on a number of strings.
	/// </summary>
	public sealed class LutronQuantumNwkSerialBuffer : ISerialBuffer
	{
		public event EventHandler<StringEventArgs> OnCompletedSerial;

		private readonly StringBuilder m_RxData;
		private readonly Queue<string> m_Queue;

		private readonly SafeCriticalSection m_QueueSection;
		private readonly SafeCriticalSection m_ParseSection;

		private readonly string[] m_Delimiters =
		{
			LutronUtils.CRLF,
			LutronUtils.QNET
		};

		//These cause the buffer to immideately return, incluing the short circuit string
		private readonly string[] m_ShortCircuits =
		{
			LutronUtils.LOGIN_PROMPT
		};

		/// <summary>
		/// Constructor.
		/// </summary>
		public LutronQuantumNwkSerialBuffer()
		{
			m_RxData = new StringBuilder();
			m_Queue = new Queue<string>();

			m_QueueSection = new SafeCriticalSection();
			m_ParseSection = new SafeCriticalSection();
		}

		/// <summary>
		/// Enqueues the serial data.
		/// </summary>
		/// <param name="data"></param>
		public void Enqueue(string data)
		{
			m_QueueSection.Execute(() => m_Queue.Enqueue(data));
			Parse();
		}

		/// <summary>
		/// Clears all queued data in the buffer.
		/// </summary>
		public void Clear()
		{
			m_ParseSection.Enter();
			m_QueueSection.Enter();

			try
			{
				m_RxData.Clear();
				m_Queue.Clear();
			}
			finally
			{
				m_ParseSection.Leave();
				m_QueueSection.Leave();
			}
		}

		/// <summary>
		/// Searches the enqueued serial data for the delimiter character.
		/// Complete strings are raised via the OnCompletedString event.
		/// </summary>
		private void Parse()
		{
			if (!m_ParseSection.TryEnter())
				return;

			try
			{
				string data = null;

				while (m_QueueSection.Execute(() => m_Queue.Dequeue(out data)))
				{
					while (true)
					{
						string delimiter, shortCircuit;
						int delimiterIndex = data.IndexOf(m_Delimiters, out delimiter);
						int shortCircuitIndex = data.IndexOf(m_ShortCircuits, out shortCircuit);

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

						string output = m_RxData.Pop();
						OnCompletedSerial.Raise(this, new StringEventArgs(output));
					}
				}
			}
			finally
			{
				m_ParseSection.Leave();
			}
		}
	}
}
