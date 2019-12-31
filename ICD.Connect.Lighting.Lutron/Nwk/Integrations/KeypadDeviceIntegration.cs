using System;
using ICD.Common.Properties;
using ICD.Connect.Lighting.Lutron.Nwk.Devices.AbstractLutronNwkDevice;

namespace ICD.Connect.Lighting.Lutron.Nwk.Integrations
{
	public sealed class KeypadDeviceIntegration : AbstractIntegrationWithComponent
	{

		private const int ACTION_BUTTON_PRESS = 3;
		private const int ACTION_BUTTON_RELEASE = 4;
		private const int ACTION_BUTTON_HOLD = 5;
		private const int ACTION_BUTTON_DOUBLE_TAP = 6;
		private const int ACTION_BUTTON_HOLD_RELEASE = 32;

		private const int ACTION_LED_STATE = 9;

		private const int BUTTON_TO_LED_OFFSET = 80;

		/// <summary>
		/// The string prefix for communication with the lighting processor, e.g. SHADES.
		/// </summary>
		protected override string Command { get { return LutronUtils.COMMAND_DEVICE; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="integrationId"></param>
		/// <param name="name"></param>
		/// <param name="parent"></param>
		public KeypadDeviceIntegration(int integrationId, string name, ILutronNwkDevice parent) :
			base(integrationId, name, parent)
		{
		}

		/// <summary>
		/// Instantiates a ZoneIntegration from xml.
		/// </summary>
		/// <param name="xml"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		public static KeypadDeviceIntegration FromXml(string xml, ILutronNwkDevice parent)
		{
			int integrationId = GetIntegrationIdFromXml(xml);
			string name = GetNameFromXml(xml);

			return new KeypadDeviceIntegration(integrationId, name, parent);
		}

		#region Public Methods

		[PublicAPI]
		public void ButtonPressRelease(int buttonNumber)
		{
			ButtonPress(buttonNumber);
			ButtonRelease(buttonNumber);
		}

		[PublicAPI]
		public void ButtonPress(int buttonNumber)
		{
			ExecuteComponent(buttonNumber, ACTION_BUTTON_PRESS);
		}

		[PublicAPI]
		public void ButtonRelease(int buttonNumber)
		{
			ExecuteComponent(buttonNumber, ACTION_BUTTON_RELEASE);
		}

		[PublicAPI]
		public void ButtonDoubleTap(int buttonNumber)
		{
			ExecuteComponent(buttonNumber,ACTION_BUTTON_DOUBLE_TAP);
		}

		[PublicAPI]
		public void ButtonHold(int buttonNumber)
		{
			ExecuteComponent(buttonNumber, ACTION_BUTTON_HOLD);
		}

		[PublicAPI]
		public void ButtonHoldRelease(int buttonNumber)
		{
			ExecuteComponent(buttonNumber,ACTION_BUTTON_HOLD_RELEASE);
		}

		[PublicAPI]
		public void QueryLed(int buttonNumber)
		{
			int ledNumber = buttonNumber + BUTTON_TO_LED_OFFSET;
			QueryComponent(ledNumber, ACTION_LED_STATE);
		}

		#endregion

		#region Private/Protected Methods

		/// <summary>
		/// Called when we receive a response from the lighting processor for this integration.
		/// </summary>
		/// <param name="action">The action number for the response.</param>
		/// <param name="component">The compoent number for the response</param>
		/// <param name="parameters">The collection of string parameters.</param>
		protected override void ParentOnResponse(int action, int component, string[] parameters)
		{
			base.ParentOnResponse(action, component, parameters);

			switch (action)
			{
				case ACTION_LED_STATE:
					HandleLedResponse(component, parameters);
					break;
			}
		}

		private void HandleLedResponse(int component, string[] parameters)
		{
			int button = component - BUTTON_TO_LED_OFFSET;
			if (button < 1)
			{
				string message = string.Format("LED action response: component {0} is not valid", component);
				throw new FormatException(message);
			}

			if (parameters.Length < 1)
			{
				string message = string.Format("LED action response component {0}: no LED value", component);
				throw new FormatException(message);
			}

			//todo: Do Stuff Here
		}

		#endregion

	}
}
