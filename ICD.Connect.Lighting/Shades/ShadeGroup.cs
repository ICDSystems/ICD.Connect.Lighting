using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Devices.EventArguments;
using ICD.Connect.Lighting.Shades.Controls;
using ICD.Connect.Settings;

namespace ICD.Connect.Lighting.Shades
{
	public sealed class ShadeGroup : AbstractShadeDevice<ShadeGroupSettings>,
		IShadeGroup,
		IShadeWithInMotionFeedback,
		IShadeWithLastDirectionFeedback,
		IShadeWithSettablePosition,
		IShadeWithStop
	{
		#region Private Members
		private readonly List<IShadeDevice> m_Shades = new List<IShadeDevice>();
		private eShadeDirection m_LastDirection = eShadeDirection.Neither;
		#endregion

		public event EventHandler<GenericEventArgs<eShadeDirection>> OnLastDirectionChanged;

		public IEnumerable<IShadeDevice> Shades { get { return m_Shades; } }

		public eShadeDirection LastDirection
		{
			get { return m_LastDirection; }
			private set
			{
				if (m_LastDirection == value)
					return;

				m_LastDirection = value;
				
				OnLastDirectionChanged.Raise(this, m_LastDirection);
			}
		}

		#region IShadeDevice
		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return Shades.All(shade => shade.IsOnline);
		}

		private void ShadeOnIsOnlineStateChanged(object sender, DeviceBaseOnlineStateApiEventArgs deviceBaseOnlineStateApiEventArgs)
		{
			UpdateCachedOnlineStatus();
		}

		public override void Open()
		{
			foreach (IShadeDevice shade in Shades)
			{
				shade.Open();
			}
		}

		public override void Close()
		{
			foreach (IShadeDevice shade in Shades)
			{
				shade.Close();
			}
		}
		#endregion

		#region IShadeWithStop
		public void Stop()
		{
			foreach (IShadeStopControl control in Shades.Select(shade => shade.Controls.GetControl<IShadeStopControl>())
														.Where(control => control != null))
			{
				control.Stop();
			}
		}
		#endregion

		#region IShadeWithSettablePosition
		public void SetPosition(float position)
		{
			foreach (IShadeSetPositionControl control in Shades.Select(shade => shade.Controls.GetControl<IShadeSetPositionControl>())
														.Where(control => control != null))
			{
				control.SetPosition(position);
			}
		}
		#endregion

		#region IShadeWithLastDirectionFeedback
		public eShadeDirection GetLastDirection()
		{
			return LastDirection;
		}

		private void ShadeLastLastDirectionChanged(object sender, EventArgs eventArgs)
		{
			var lastDirectionControl = sender as IShadeLastDirectionControl;
			if (lastDirectionControl != null)
			{
				LastDirection = lastDirectionControl.LastDirection;
			}
		}
		#endregion

		#region IShadeWithInMotionFeedback
		public bool GetIsInMotion()
		{
			return Shades.Select(shade => shade.Controls.GetControl<IShadeInMotionFeedbackControl>())
			             .Where(control => control != null)
			             .Any(control => control.GetInMotion());
		}
		#endregion

		#region Private Helpers

		private void AddShade(IShadeDevice shade)
		{
			m_Shades.Add(shade);

			shade.OnIsOnlineStateChanged += ShadeOnIsOnlineStateChanged;

			var lastDirectionControl = shade.Controls.GetControl<IShadeLastDirectionControl>();
			if (lastDirectionControl != null)
			{
				lastDirectionControl.OnLastDirectionChanged += ShadeLastLastDirectionChanged;
			}
		}

		private void RemoveShade(IShadeDevice shade)
		{
			m_Shades.Remove(shade);

			shade.OnIsOnlineStateChanged -= ShadeOnIsOnlineStateChanged;

			var lastDirectionControl = shade.Controls.GetControl<IShadeLastDirectionControl>();
			if (lastDirectionControl != null)
			{
				lastDirectionControl.OnLastDirectionChanged -= ShadeLastLastDirectionChanged;
			}
		}

		#endregion

		#region Settings

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(ShadeGroupSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			foreach (int id in settings.ShadeIds.ExceptNulls())
				AddShade(factory.GetOriginatorById(id) as IShadeDevice);
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			var shadesToRemove = Shades.ToArray();
			foreach (var shade in shadesToRemove)
			{
				RemoveShade(shade);
			}
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(ShadeGroupSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.ShadeIds = m_Shades.Select(shade => (int?)shade.Id);
		}

		/// <summary>
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(ShadeGroupSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

			addControl(new ShadeStopControl<ShadeGroup>(this, 0));
			addControl(new ShadeSetPositionControl<ShadeGroup>(this, 1));
			addControl(new ShadeInMotionFeedbackControl<ShadeGroup>(this, 2));
			addControl(new ShadeLastDirectionControl<ShadeGroup>(this, 4));
		}

		#endregion
	}
}