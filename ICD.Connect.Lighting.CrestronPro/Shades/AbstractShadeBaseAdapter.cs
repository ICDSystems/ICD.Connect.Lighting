using System;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Misc.CrestronPro.Utils;
using ICD.Connect.Settings;
#if !NETSTANDARD
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using ICD.Connect.Misc.CrestronPro.Extensions;
#endif
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Lighting.Shades;
using ICD.Connect.Lighting.Shades.Controls;

namespace ICD.Connect.Lighting.CrestronPro.Shades
{
#if !NETSTANDARD
	public abstract class AbstractShadeBaseAdapter<TShade, TSettings> : AbstractShadeDevice<TSettings>, IShadeBaseAdapter
		where TShade : ShadeBase
#else
	public abstract class AbstractShadeBaseAdapter<TSettings> : AbstractShadeDevice<TSettings>, IShadeBaseAdapter
#endif
		where TSettings : IShadeBaseAdapterSettings, new()
	{
		/// <summary>
		/// Raised when the last direction changes
		/// </summary>
		public event EventHandler<GenericEventArgs<eShadeDirection>> OnLastDirectionChanged;

		private eShadeDirection m_LastDirection = eShadeDirection.Neither;

		/// <summary>
		/// Last direction the shade moved
		/// </summary>
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
#if !NETSTANDARD
		protected TShade Shade { get; private set; }
#endif

		/// <summary>
		/// Release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnLastDirectionChanged = null;

			base.DisposeFinal(disposing);
		}

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
#if !NETSTANDARD
			return Shade != null && Shade.IsOnline;
#else
			return false;
#endif
		}

#if !NETSTANDARD

		#region Shade Callbacks
		protected void SetShade(TShade shade)
		{
			if (shade == Shade)
				return;

			Unsubscribe(Shade);

			if (Shade != null)
				GenericBaseUtils.TearDown(Shade);

			Shade = shade;

			eDeviceRegistrationUnRegistrationResponse result;
			if (Shade != null && !GenericBaseUtils.SetUp(Shade, this, out result))
				Logger.Log(eSeverity.Error, "Unable to register {0} - {1}", Shade.GetType().Name, result);

			Subscribe(Shade);
			UpdateCachedOnlineStatus();
		}

		protected virtual void Subscribe(TShade shade)
		{
			if (shade == null)
				return;


			shade.OnlineStatusChange += ShadeOnLineStatusChange;
		}

		protected virtual void Unsubscribe(TShade shade)
		{
			if (shade == null)
				return;

			shade.OnlineStatusChange -= ShadeOnLineStatusChange;
		}

		private void ShadeOnLineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
		{
			UpdateCachedOnlineStatus();
		}

		#endregion
#endif

		#region Methods

		public override void Open()
		{
#if !NETSTANDARD
			Shade.Open();
			if (LastDirection == eShadeDirection.Open)
				return;
			LastDirection = eShadeDirection.Open;
#else
			throw new NotSupportedException();
#endif
		}

		public override void Close()
		{
#if !NETSTANDARD
			Shade.Close();
			if (LastDirection == eShadeDirection.Close)
				return;
			LastDirection = eShadeDirection.Close;
#else
			throw new NotSupportedException();
#endif
		}

		public void Stop()
		{
#if !NETSTANDARD
			Shade.Stop();
#else
			throw new NotSupportedException();
#endif
		}

		public bool GetIsInMotion()
		{
#if !NETSTANDARD
			return Shade.IsRaising.GetBoolValueOrDefault() || Shade.IsLowering.GetBoolValueOrDefault();
#else
			throw new NotSupportedException();
#endif
		}

		public float GetPosition()
		{
#if !NETSTANDARD
			return MathUtils.MapRange(0, ushort.MaxValue, 0, 1, (float)Shade.PositionFeedback.GetUShortValueOrDefault());
#else
			throw new NotSupportedException();
#endif
		}

		public void SetPosition(float position)
		{
#if !NETSTANDARD
			position = MathUtils.Clamp(position, 0, 1);
			float floatPosition = MathUtils.MapRange(0, 1, 0, ushort.MaxValue, position);
			Shade.SetPosition((ushort)floatPosition);
#else
			throw new NotSupportedException();
#endif
		}

		#endregion

		#region Settings

		/// <summary>
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(TSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

			addControl(new ShadeStopControl<IShadeBaseAdapter>(this, 0));
			addControl(new ShadeSetPositionControl<IShadeBaseAdapter>(this, 1));
			addControl(new ShadeInMotionFeedbackControl<IShadeBaseAdapter>(this, 2));
			addControl(new ShadePositionFeedbackControl<IShadeBaseAdapter>(this, 3));
			addControl(new ShadeLastDirectionControl<IShadeBaseAdapter>(this, 4));
		}

		#endregion
	}
}