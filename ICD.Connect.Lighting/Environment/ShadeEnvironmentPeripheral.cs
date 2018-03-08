using System;
using System.Collections.Generic;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;
using ICD.Connect.Lighting.Shades;
using ICD.Connect.Lighting.Shades.Controls;

namespace ICD.Connect.Lighting.Environment
{
	public class ShadeEnvironmentPeripheral : IShadeEnvironmentPeripheral
	{
		private IShadeWithStop m_Shade;
		private eShadeDirection m_LastDirection = eShadeDirection.Neither;

		public LightingProcessorControl.ePeripheralType PeripheralType { get; private set; }
		public int Id { get; private set; }
		public int Room { get; private set; }
		public string Name { get; private set; }
		public eShadeDirection LastDirection { get { return m_LastDirection; } }

		public ShadeEnvironmentPeripheral(int id, int roomId, string name)
		{
			Id = id;
			Room = roomId;
			Name = name;
		}

		public LightingProcessorControl ToLightingProcessorControl()
		{
			return m_Shade is IShadeGroup
				       ? LightingProcessorControl.ShadeGroup(Id, Room, Name)
				       : LightingProcessorControl.Shade(Id, Room, Name);
		}

		public void Dispose()
		{
			SetShade(null);
		}

		public void SetShade(IShadeWithStop shade)
		{
			Unsubscribe(shade);

			m_Shade = shade;

			PeripheralType = m_Shade is IShadeGroup
				              ? LightingProcessorControl.ePeripheralType.ShadeGroup
				              : LightingProcessorControl.ePeripheralType.Shade;

			Subscribe(shade);
		}

		private static bool HasLastDirectionFeedback(IDeviceBase shade)
		{
			return shade is IShadeWithLastDirectionFeedback;
		}

		private void Subscribe(IShadeWithStop shade)
		{
			if (!HasLastDirectionFeedback(shade))
				return;

			IShadeWithLastDirectionFeedback feedbackShade = (IShadeWithLastDirectionFeedback)shade;
			feedbackShade.OnDirectionChanged += FeedbackShadeOnDirectionChanged;
		}

		private void Unsubscribe(IShadeWithStop shade)
		{
			if (!HasLastDirectionFeedback(shade))
				return;

			IShadeWithLastDirectionFeedback feedbackShade = (IShadeWithLastDirectionFeedback)shade;
			feedbackShade.OnDirectionChanged -= FeedbackShadeOnDirectionChanged;
		}

		private void FeedbackShadeOnDirectionChanged(object sender, EventArgs eventArgs)
		{
			IShadeWithLastDirectionFeedback feedbackShade = (IShadeWithLastDirectionFeedback)sender;
			m_LastDirection = feedbackShade.GetLastDirection();
		}

		#region IShadeEnvironmentPeripheral
		/// <summary>
		/// Starts raising the shade.
		/// </summary>
		public void StartRaising()
		{
			m_Shade.Open();
			if (!HasLastDirectionFeedback(m_Shade))
			{
				m_LastDirection = eShadeDirection.Open;
			}
		}

		/// <summary>
		/// Starts lowering the shade.
		/// </summary>
		public void StartLowering()
		{
			m_Shade.Close();
			if (!HasLastDirectionFeedback(m_Shade))
			{
				m_LastDirection = eShadeDirection.Close;
			}
		}

		/// <summary>
		/// Stops moving the shade.
		/// </summary>
		public void StopMoving()
		{
			m_Shade.Stop();
		}
		#endregion

		#region Console
		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		public string ConsoleName { get { return GetType().Name; } }

		/// <summary>
		/// Gets the help information for the node.
		/// </summary>
		public string ConsoleHelp { get { return string.Empty; } }

		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}