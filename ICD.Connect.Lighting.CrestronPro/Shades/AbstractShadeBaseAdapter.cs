using System;
using ICD.Connect.Misc.CrestronPro.Utils;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using ICD.Connect.Misc.CrestronPro.Extensions;
#endif
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Lighting.Shades;
using ICD.Connect.Lighting.Shades.Controls;

namespace ICD.Connect.Lighting.CrestronPro.Shades
{
#if SIMPLSHARP
	public abstract class AbstractShadeBaseAdapter<TShade, TSettings> : AbstractShadeDevice<TSettings>, IShadeBaseAdapter
		where TShade : ShadeBase
#else
	public abstract class AbstractShadeBaseAdapter<TSettings> : AbstractShadeDevice<TSettings>, IShadeBaseAdapter
#endif
		where TSettings : IShadeBaseAdapterSettings, new()
	{
		public event EventHandler OnDirectionChanged;

#if SIMPLSHARP
		private eShadeDirection m_LastDirection = eShadeDirection.Neither;

		protected TShade Shade { get; private set; }
#endif

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractShadeBaseAdapter()
		{
			Controls.Add(new ShadeStopControl<IShadeBaseAdapter>(this, 0));
			Controls.Add(new ShadeSetPositionControl<IShadeBaseAdapter>(this, 1));
			Controls.Add(new ShadeInMotionFeedbackControl<IShadeBaseAdapter>(this, 2));
			Controls.Add(new ShadePositionFeedbackControl<IShadeBaseAdapter>(this, 3));
			Controls.Add(new ShadeLastDirectionControl<IShadeBaseAdapter>(this, 4));
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnDirectionChanged = null;

			base.DisposeFinal(disposing);
		}

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
#if SIMPLSHARP
			return Shade != null && Shade.IsOnline;
#else
			return false;
#endif
		}

#if SIMPLSHARP

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
				Log(eSeverity.Error, "Unable to register {0} - {1}", Shade.GetType().Name, result);

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

		public override void Open()
		{
#if SIMPLSHARP
			Shade.Open();
			if (m_LastDirection == eShadeDirection.Open)
				return;
			m_LastDirection = eShadeDirection.Open;
			OnDirectionChanged.Raise(this);
#else
			throw new NotSupportedException();
#endif
		}

		public override void Close()
		{
#if SIMPLSHARP
			Shade.Close();
			if (m_LastDirection == eShadeDirection.Close)
				return;
			m_LastDirection = eShadeDirection.Close;
			OnDirectionChanged.Raise(this);
#else
			throw new NotSupportedException();
#endif
		}

		public void Stop()
		{
#if SIMPLSHARP
			Shade.Stop();
#else
			throw new NotSupportedException();
#endif
		}

		public eShadeDirection GetLastDirection()
		{
#if SIMPLSHARP
			switch (Shade.LastDirection)
			{
				case eShadeMovement.NA:
					return eShadeDirection.Neither;
				case eShadeMovement.Opened:
					return eShadeDirection.Open;
				case eShadeMovement.Closed:
					return eShadeDirection.Close;
				default:
					throw new ArgumentOutOfRangeException();
			}
#else
			throw new NotSupportedException();
#endif
		}

		public bool GetIsInMotion()
		{
#if SIMPLSHARP
			return Shade.IsRaising.GetBoolValueOrDefault() || Shade.IsLowering.GetBoolValueOrDefault();
#else
			throw new NotSupportedException();
#endif
		}

		public float GetPosition()
		{
#if SIMPLSHARP
			return MathUtils.MapRange(0, ushort.MaxValue, 0, 1, (float)Shade.PositionFeedback.GetUShortValueOrDefault());
#else
			throw new NotSupportedException();
#endif
		}

		public void SetPosition(float position)
		{
#if SIMPLSHARP
			position = MathUtils.Clamp(position, 0, 1);
			float floatPosition = MathUtils.MapRange(0, 1, 0, ushort.MaxValue, position);
			Shade.SetPosition((ushort)floatPosition);
#else
			throw new NotSupportedException();
#endif
		}
	}
}