using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
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

		public event EventHandler<GenericEventArgs<int?>> OnActiveSceneChanged;

		private readonly Dictionary<int, SceneIntegration> m_SceneButtons;
		private readonly Dictionary<int, bool> m_SceneButtonLedState;

		private int? m_ActiveScene;

		public int? ActiveScene
		{
			get { return m_ActiveScene; }
			private set
			{
				if (m_ActiveScene == value)
					return;

				m_ActiveScene = value;

				OnActiveSceneChanged.Raise(this, new GenericEventArgs<int?>(value));
			}
		}

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
		/// <param name="sceneButtons"></param>
		public KeypadDeviceIntegration(int integrationId, string name, ILutronNwkDevice parent, IEnumerable<SceneIntegration> sceneButtons) :
			base(integrationId, name, parent)
		{
			m_SceneButtons = new Dictionary<int, SceneIntegration>();
			m_SceneButtonLedState = new Dictionary<int, bool>();

			SetScenes(sceneButtons);
		}

		private void SetScenes(IEnumerable<SceneIntegration> sceneButtons)
		{
			m_SceneButtons.Clear();

			m_SceneButtons.AddRange(sceneButtons, s => s.IntegrationId);

			QueryActiveScene();
		}

		public void QueryActiveScene()
		{
			foreach (int scene in m_SceneButtons.Keys)
				QueryLed(scene);
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

			SetLedState(button, parameters[0] != "0");
		}

		private void SetLedState(int button, bool state)
		{
			m_SceneButtonLedState[button] = state;

			if (state)
			{
				//If state is true, check to see if this is higher than the active scene - if so, set it as active
				if (!ActiveScene.HasValue || ActiveScene.Value > button)
					ActiveScene = button;
			}
			else
			{
				//If this is the active scene, recaculate the scene based on all button's feedback
				if (ActiveScene.HasValue && ActiveScene.Value == button)
					ActiveScene = CaculateActiveScene();
			}
		}

		private int? CaculateActiveScene()
		{
			KeyValuePair<int, bool> scene;
			if (m_SceneButtonLedState.TryFirst(kvp => kvp.Value, out scene))
				return scene.Key;

			return null;

		}

		#endregion
	}
}
